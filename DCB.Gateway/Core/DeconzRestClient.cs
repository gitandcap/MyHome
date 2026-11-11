using System.Net.Http.Json;
using System.Text.Json;
using DCB.Gateway.Configuration;
using DCB.Gateway.Dto.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DCB.Gateway.Core;

/// <summary>
/// Base comune verso le REST API di deCONZ (<c>DCB-GTW</c>): costruisce gli URL, invia le richieste,
/// riconosce gli errori e deserializza le risposte.
/// </summary>
/// <remarks>
/// È l'<b>unica</b> classe che usa <see cref="HttpClient"/>: i client per area indicano solo risorsa, metodo e corpo,
/// così la gestione degli errori deCONZ sta in un solo posto (COD-02 §3).
/// L'API key fa parte dell'URL (<c>/api/{key}/…</c>) e dà pieno controllo del gateway: nei log viene sempre mascherata.
/// </remarks>
internal sealed class DeconzRestClient
{
    /// <summary>Nome del client HTTP registrato in <see cref="IHttpClientFactory"/>.</summary>
    public const string HttpClientName = "Deconz";

    private const string MaskedApiKey = "***";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly DeconzApiKeyHolder _apiKeyHolder;
    private readonly IOptions<DeconzGatewayOptions> _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<DeconzRestClient> _logger;

    /// <summary>Crea il client.</summary>
    public DeconzRestClient(
        IHttpClientFactory httpClientFactory,
        DeconzApiKeyHolder apiKeyHolder,
        IOptions<DeconzGatewayOptions> options,
        TimeProvider timeProvider,
        ILogger<DeconzRestClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _apiKeyHolder = apiKeyHolder;
        _options = options;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <summary>Legge una risorsa protetta: <c>GET /api/{key}/{resource}</c>.</summary>
    /// <typeparam name="T">DTO in cui deserializzare la risposta.</typeparam>
    /// <param name="resource">Percorso dopo la chiave (es. <c>config</c>).</param>
    /// <param name="cancellationToken">Annullamento dell'operazione.</param>
    public async Task<T> GetAsync<T>(string resource, CancellationToken cancellationToken)
    {
        JsonElement root = await SendAsync(HttpMethod.Get, RequireApiKey(), resource, body: null, cancellationToken);
        return Deserialize<T>(root);
    }

    /// <summary>Invia un comando a una risorsa protetta: <c>PUT /api/{key}/{resource}</c>.</summary>
    /// <param name="resource">Percorso dopo la chiave (es. <c>lights/3/state</c>).</param>
    /// <param name="body">Corpo del comando; <c>null</c> per i comandi senza corpo (es. richiamo di una scena).</param>
    /// <param name="cancellationToken">Annullamento dell'operazione.</param>
    /// <returns>Campi applicati e campi rifiutati; se deCONZ li rifiuta tutti viene sollevata <see cref="DeconzApiException"/>.</returns>
    public async Task<DeconzCommandResult> PutAsync(string resource, object? body, CancellationToken cancellationToken)
    {
        JsonElement root = await SendAsync(HttpMethod.Put, RequireApiKey(), resource, body, cancellationToken);
        return DeconzCommandResult.FromResponseItems(DeconzErrorReader.ReadResponseItems(root));
    }

    /// <summary>Legge una risorsa pubblica, senza chiave: <c>GET /api/{resource}</c>.</summary>
    /// <typeparam name="T">DTO in cui deserializzare la risposta.</typeparam>
    /// <param name="resource">Percorso dopo <c>/api</c> (es. <c>config</c>).</param>
    /// <param name="cancellationToken">Annullamento dell'operazione.</param>
    public async Task<T> GetWithoutKeyAsync<T>(string resource, CancellationToken cancellationToken)
    {
        JsonElement root = await SendAsync(HttpMethod.Get, apiKey: null, resource, body: null, cancellationToken);
        return Deserialize<T>(root);
    }

    /// <summary>Invia una richiesta senza chiave e restituisce gli esiti positivi: <c>POST /api/{resource}</c>.</summary>
    /// <param name="resource">Percorso dopo <c>/api</c>; vuoto per <c>POST /api</c>.</param>
    /// <param name="body">Corpo della richiesta.</param>
    /// <param name="cancellationToken">Annullamento dell'operazione.</param>
    /// <returns>Il contenuto di ogni elemento <c>success</c> della risposta.</returns>
    public async Task<IReadOnlyList<JsonElement>> PostWithoutKeyAsync(string resource, object body, CancellationToken cancellationToken)
    {
        JsonElement root = await SendAsync(HttpMethod.Post, apiKey: null, resource, body, cancellationToken);

        return DeconzErrorReader.ReadResponseItems(root)
            .Where(item => item.Success is not null)
            .Select(item => item.Success!.Value)
            .ToList();
    }

    private string RequireApiKey() =>
        _apiKeyHolder.ApiKey
            ?? throw new InvalidOperationException("API key non disponibile: eseguire prima l'autenticazione (DCB-GTW-AUT).");

    private async Task<JsonElement> SendAsync(
        HttpMethod method, string? apiKey, string resource, object? body, CancellationToken cancellationToken)
    {
        DeconzGatewayOptions options = _options.Value;
        Uri uri = BuildUri(options, apiKey, resource);
        string pathForLog = BuildPath(apiKey is null ? null : MaskedApiKey, resource);

        using var request = new HttpRequestMessage(method, uri);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, body.GetType(), options: DeconzJson.Options);
        }

        _logger.LogDebug("deCONZ {Method} {Path}", method, pathForLog);

        using HttpResponseMessage response = await SendWithTimeoutAsync(request, options, pathForLog, cancellationToken);
        string content = await response.Content.ReadAsStringAsync(cancellationToken);
        JsonElement root = ParseJson(content);

        ThrowIfDeconzError(root, response, method, pathForLog);
        return root;
    }

    /// <summary>
    /// Invia la richiesta con un tempo massimo di risposta; i problemi di rete diventano <see cref="DeconzUnreachableException"/>.
    /// </summary>
    /// <remarks>
    /// Il tempo massimo usa <see cref="TimeProvider"/>, così nei test si simula senza attese reali.
    /// L'annullamento chiesto dal chiamante non viene trasformato: resta un <see cref="OperationCanceledException"/>.
    /// </remarks>
    private async Task<HttpResponseMessage> SendWithTimeoutAsync(
        HttpRequestMessage request, DeconzGatewayOptions options, string pathForLog, CancellationToken cancellationToken)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(options.RequestTimeoutSeconds), _timeProvider);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);

        HttpClient httpClient = _httpClientFactory.CreateClient(HttpClientName);
        try
        {
            return await httpClient.SendAsync(request, linked.Token);
        }
        catch (HttpRequestException ex)
        {
            throw new DeconzUnreachableException(
                $"deCONZ non raggiungibile su {options.Host}:{options.HttpPort} ({request.Method} {pathForLog}): {ex.Message}", ex);
        }
        catch (OperationCanceledException ex) when (timeout.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            throw new DeconzUnreachableException(
                $"deCONZ non ha risposto entro {options.RequestTimeoutSeconds} s ({request.Method} {pathForLog}).", ex);
        }
    }

    private void ThrowIfDeconzError(JsonElement root, HttpResponseMessage response, HttpMethod method, string pathForLog)
    {
        IReadOnlyList<DeconzApiResponseDto> items = DeconzErrorReader.ReadResponseItems(root);
        List<DeconzErrorDto> errors = items.Where(item => item.Error is not null).Select(item => item.Error!).ToList();
        bool hasSuccess = items.Any(item => item.Success is not null);

        // Una risposta con soli errori è un fallimento anche se il codice HTTP è 2xx.
        // Le risposte miste (successi + errori) dei comandi PUT non sono un fallimento: le riporta DeconzCommandResult.
        if (errors.Count > 0 && !hasSuccess)
        {
            _logger.LogWarning("deCONZ {Method} {Path}: {Errors}", method, pathForLog, string.Join(" | ", errors));
            throw new DeconzApiException(errors, response.StatusCode);
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("deCONZ {Method} {Path}: HTTP {StatusCode}", method, pathForLog, (int)response.StatusCode);
            throw new DeconzApiException(errors, response.StatusCode);
        }
    }

    private static Uri BuildUri(DeconzGatewayOptions options, string? apiKey, string resource) =>
        new UriBuilder(Uri.UriSchemeHttp, options.Host, options.HttpPort, BuildPath(apiKey, resource)).Uri;

    /// <summary>Compone <c>/api[/{key}][/{resource}]</c>.</summary>
    private static string BuildPath(string? apiKey, string resource)
    {
        var segments = new List<string> { "api" };
        if (apiKey is not null)
        {
            segments.Add(apiKey);
        }

        if (!string.IsNullOrEmpty(resource))
        {
            segments.Add(resource.Trim('/'));
        }

        return "/" + string.Join('/', segments);
    }

    /// <summary>
    /// Legge il corpo come JSON; un corpo vuoto o non JSON (es. pagina HTML d'errore) diventa un elemento indefinito,
    /// così il codice HTTP resta l'informazione principale sull'esito.
    /// </summary>
    private static JsonElement ParseJson(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return default;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(content);
            return document.RootElement.Clone();
        }
        catch (JsonException)
        {
            return default;
        }
    }

    private static T Deserialize<T>(JsonElement root) =>
        root.ValueKind == JsonValueKind.Undefined
            ? throw new InvalidDataException($"deCONZ ha restituito una risposta vuota o non JSON (attesa: {typeof(T).Name}).")
            : root.Deserialize<T>(DeconzJson.Options)
                ?? throw new InvalidDataException($"Risposta deCONZ non interpretabile come {typeof(T).Name}.");
}
