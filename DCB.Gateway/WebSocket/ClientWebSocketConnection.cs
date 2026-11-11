using System.Net.WebSockets;
using System.Text;
using DCB.Gateway.Configuration;
using Microsoft.Extensions.Options;

namespace DCB.Gateway.WebSocket;

/// <summary>
/// <see cref="IWebSocketConnection"/> sopra <see cref="ClientWebSocket"/> di .NET.
/// </summary>
/// <remarks>
/// Ping periodici con tempo massimo di risposta (<c>KeepAliveInterval</c> + <c>KeepAliveTimeout</c>): se deCONZ si blocca
/// senza chiudere la connessione, la ricezione fallisce e parte la riconnessione. Verificato l'11/11/2026 che
/// deCONZ 2.33.2 risponde ai ping.
/// </remarks>
internal sealed class ClientWebSocketConnection : IWebSocketConnection
{
    private const int ReceiveBufferSize = 8192;

    private readonly ClientWebSocket _webSocket = new();

    /// <summary>Crea la connessione (non ancora aperta).</summary>
    /// <param name="keepAlive">Intervallo dei ping e tempo massimo di risposta.</param>
    public ClientWebSocketConnection(TimeSpan keepAlive)
    {
        _webSocket.Options.KeepAliveInterval = keepAlive;
        _webSocket.Options.KeepAliveTimeout = keepAlive;
    }

    /// <inheritdoc />
    public Task ConnectAsync(Uri uri, CancellationToken cancellationToken) =>
        _webSocket.ConnectAsync(uri, cancellationToken);

    /// <inheritdoc />
    public async Task<string?> ReceiveMessageAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[ReceiveBufferSize];
        using var message = new MemoryStream();

        // Un messaggio può arrivare in più frammenti: si accumulano fino a EndOfMessage.
        while (true)
        {
            WebSocketReceiveResult result = await _webSocket.ReceiveAsync(buffer, cancellationToken);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                return null;
            }

            message.Write(buffer, 0, result.Count);
            if (result.EndOfMessage)
            {
                return Encoding.UTF8.GetString(message.GetBuffer(), 0, (int)message.Length);
            }
        }
    }

    /// <inheritdoc />
    public void Dispose() => _webSocket.Dispose();
}

/// <summary>Crea connessioni <see cref="ClientWebSocketConnection"/> con i parametri correnti.</summary>
internal sealed class ClientWebSocketConnectionFactory : IWebSocketConnectionFactory
{
    private readonly IOptions<DeconzGatewayOptions> _options;

    /// <summary>Crea la factory.</summary>
    public ClientWebSocketConnectionFactory(IOptions<DeconzGatewayOptions> options)
    {
        _options = options;
    }

    /// <inheritdoc />
    public IWebSocketConnection Create() =>
        new ClientWebSocketConnection(TimeSpan.FromSeconds(_options.Value.WsKeepAliveSeconds));
}
