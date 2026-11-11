using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text.Json;
using DCB.Gateway.Clients.Config;
using DCB.Gateway.Configuration;
using DCB.Gateway.Core;
using DCB.Gateway.Dto.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DCB.Gateway.WebSocket;

/// <summary>
/// Implementazione di <see cref="IDeconzEventStream"/>: connessione, lettura e riconnessione con attese crescenti.
/// </summary>
/// <remarks>
/// Ciclo: risoluzione della porta → connessione → lettura dei messaggi finché la connessione regge →
/// attesa (raddoppiata a ogni tentativo fallito, fino al massimo) → nuova connessione.
/// La porta è riletta a ogni connessione, così un cambio in deCONZ viene seguito senza riavviare il DCB.
/// Le attese usano <see cref="TimeProvider"/>, così nei test si simulano senza attese reali.
/// </remarks>
internal sealed class DeconzEventStream : IDeconzEventStream
{
    private const string EventMessageType = "event";

    private readonly IWebSocketConnectionFactory _connectionFactory;
    private readonly IConfigClient _configClient;
    private readonly IOptions<DeconzGatewayOptions> _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<DeconzEventStream> _logger;

    private bool _hasConnectedBefore;

    /// <summary>Crea il flusso degli eventi.</summary>
    public DeconzEventStream(
        IWebSocketConnectionFactory connectionFactory,
        IConfigClient configClient,
        IOptions<DeconzGatewayOptions> options,
        TimeProvider timeProvider,
        ILogger<DeconzEventStream> logger)
    {
        _connectionFactory = connectionFactory;
        _configClient = configClient;
        _options = options;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;

    /// <inheritdoc />
    public DeconzConnectionState State { get; private set; } = DeconzConnectionState.Disconnected;

    /// <inheritdoc />
    public async IAsyncEnumerable<DeconzWsEventDto> ReadEventsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        int failedAttempts = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using IWebSocketConnection? connection = await TryConnectAsync(cancellationToken);
            if (connection is null)
            {
                failedAttempts++;
                ChangeState(DeconzConnectionState.Reconnecting);
                await WaitBeforeReconnectAsync(failedAttempts, cancellationToken);
                continue;
            }

            failedAttempts = 0;
            ChangeState(DeconzConnectionState.Connected);
            _hasConnectedBefore = true;

            while (await TryReceiveAsync(connection, cancellationToken) is { } message)
            {
                if (TryParseEvent(message) is { } deconzEvent)
                {
                    yield return deconzEvent;
                }
            }

            ChangeState(DeconzConnectionState.Disconnected);
            ChangeState(DeconzConnectionState.Reconnecting);
            await WaitBeforeReconnectAsync(attempt: 1, cancellationToken);
        }
    }

    /// <summary>Apre una nuova connessione.</summary>
    /// <returns>La connessione aperta, oppure <c>null</c> se deCONZ non è raggiungibile.</returns>
    private async Task<IWebSocketConnection?> TryConnectAsync(CancellationToken cancellationToken)
    {
        IWebSocketConnection connection = _connectionFactory.Create();
        try
        {
            Uri uri = await ResolveUriAsync(cancellationToken);
            await ConnectWithTimeoutAsync(connection, uri, cancellationToken);
            _logger.LogInformation("WebSocket deCONZ collegato a {Uri}.", uri);
            return connection;
        }
        catch (Exception ex) when (IsConnectionFailure(ex, cancellationToken))
        {
            _logger.LogWarning("Connessione al WebSocket deCONZ non riuscita: {Message}", ex.Message);
            connection.Dispose();
            return null;
        }
    }

    /// <summary>
    /// <c>ws://host:porta</c>: la porta è <c>Deconz:WsPort</c> se impostata, altrimenti <c>websocketport</c> di <c>/config</c>.
    /// </summary>
    private async Task<Uri> ResolveUriAsync(CancellationToken cancellationToken)
    {
        DeconzGatewayOptions options = _options.Value;
        int port = options.WsPort
            ?? (await _configClient.GetConfigAsync(cancellationToken)).WebSocketPort
            ?? throw new InvalidDataException("deCONZ non ha indicato la porta del WebSocket (websocketport).");

        return new UriBuilder("ws", options.Host, port).Uri;
    }

    private async Task ConnectWithTimeoutAsync(IWebSocketConnection connection, Uri uri, CancellationToken cancellationToken)
    {
        TimeSpan timeout = TimeSpan.FromSeconds(_options.Value.RequestTimeoutSeconds);
        using var timeoutSource = new CancellationTokenSource(timeout, _timeProvider);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token);

        await connection.ConnectAsync(uri, linked.Token);
    }

    /// <summary>Attende il prossimo messaggio.</summary>
    /// <returns>Il messaggio, oppure <c>null</c> se la connessione è stata chiusa o si è interrotta.</returns>
    private async Task<string?> TryReceiveAsync(IWebSocketConnection connection, CancellationToken cancellationToken)
    {
        try
        {
            return await connection.ReceiveMessageAsync(cancellationToken);
        }
        catch (Exception ex) when (IsConnectionFailure(ex, cancellationToken))
        {
            _logger.LogWarning("WebSocket deCONZ interrotto: {Message}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Errori di rete, di protocollo, di timeout o di deCONZ: portano alla riconnessione.
    /// L'annullamento chiesto dal chiamante invece non viene intercettato e termina la lettura.
    /// </summary>
    private static bool IsConnectionFailure(Exception ex, CancellationToken cancellationToken) =>
        !cancellationToken.IsCancellationRequested
        && ex is WebSocketException or HttpRequestException or OperationCanceledException or IOException
            or DeconzApiException or DeconzUnreachableException or InvalidDataException;

    private DeconzWsEventDto? TryParseEvent(string message)
    {
        try
        {
            DeconzWsEventDto? deconzEvent = JsonSerializer.Deserialize<DeconzWsEventDto>(message, DeconzJson.Options);
            return deconzEvent?.MessageType == EventMessageType ? deconzEvent : null;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning("Messaggio WebSocket deCONZ non interpretabile, ignorato: {Message}", ex.Message);
            return null;
        }
    }

    /// <summary>Attesa crescente: iniziale × 2^(tentativo − 1), al massimo il valore configurato.</summary>
    private Task WaitBeforeReconnectAsync(int attempt, CancellationToken cancellationToken)
    {
        DeconzGatewayOptions options = _options.Value;
        double seconds = Math.Min(
            options.WsReconnectInitialDelaySeconds * Math.Pow(2, attempt - 1),
            options.WsReconnectMaxDelaySeconds);

        return Task.Delay(TimeSpan.FromSeconds(seconds), _timeProvider, cancellationToken);
    }

    /// <summary>Aggiorna lo stato e lo notifica solo se cambia.</summary>
    private void ChangeState(DeconzConnectionState newState)
    {
        if (State == newState)
        {
            return;
        }

        State = newState;
        bool isReconnection = newState == DeconzConnectionState.Connected && _hasConnectedBefore;
        ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(newState, isReconnection));
    }
}
