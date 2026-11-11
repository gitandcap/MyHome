using System.Text.Json;
using DCB.Gateway.Abstractions;
using DCB.Gateway.Clients.Config;
using DCB.Gateway.Configuration;
using DCB.Gateway.Core;
using DCB.Gateway.Dto.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DCB.Gateway.Authentication;

/// <summary>
/// Implementazione di <see cref="IDeconzAuthenticator"/> (ANA-06 §4.3).
/// </summary>
/// <remarks>
/// Flusso: chiave salvata → verifica con <c>GET /api/{key}/config</c>; se manca o deCONZ risponde con l'errore 1
/// (<i>unauthorized user</i>) → attesa dello sblocco, ripetendo <c>POST /api</c> finché deCONZ non rilascia una chiave.
/// Una chiave deCONZ non scade per tempo: diventa non valida solo se rimossa dalla whitelist o se il gateway cambia.
/// L'attesa usa <see cref="TimeProvider"/>, così nei test si simula senza attese reali.
/// </remarks>
internal sealed class DeconzAuthenticator : IDeconzAuthenticator
{
    private readonly IApiKeyStore _apiKeyStore;
    private readonly DeconzApiKeyHolder _apiKeyHolder;
    private readonly IConfigClient _configClient;
    private readonly DeconzRestClient _restClient;
    private readonly IOptions<DeconzGatewayOptions> _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<DeconzAuthenticator> _logger;

    /// <summary>Crea il servizio di autenticazione.</summary>
    public DeconzAuthenticator(
        IApiKeyStore apiKeyStore,
        DeconzApiKeyHolder apiKeyHolder,
        IConfigClient configClient,
        DeconzRestClient restClient,
        IOptions<DeconzGatewayOptions> options,
        TimeProvider timeProvider,
        ILogger<DeconzAuthenticator> logger)
    {
        _apiKeyStore = apiKeyStore;
        _apiKeyHolder = apiKeyHolder;
        _configClient = configClient;
        _restClient = restClient;
        _options = options;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public event EventHandler? WaitingForUnlock;

    /// <inheritdoc />
    public event EventHandler<ApiKeyObtainedEventArgs>? ApiKeyObtained;

    /// <inheritdoc />
    public async Task<DeconzConfigDto> EnsureAuthenticatedAsync(CancellationToken cancellationToken)
    {
        DeconzConfigDto? config = await TryStoredApiKeyAsync(cancellationToken);
        if (config is not null)
        {
            return config;
        }

        await WaitForNewApiKeyAsync(cancellationToken);
        return await _configClient.GetConfigAsync(cancellationToken);
    }

    /// <summary>Verifica l'ultima chiave salvata.</summary>
    /// <returns>La configurazione se la chiave è valida; <c>null</c> se manca o è stata rifiutata.</returns>
    private async Task<DeconzConfigDto?> TryStoredApiKeyAsync(CancellationToken cancellationToken)
    {
        string? storedApiKey = await _apiKeyStore.LoadAsync(cancellationToken);
        if (string.IsNullOrEmpty(storedApiKey))
        {
            _logger.LogInformation("Nessuna API key deCONZ salvata.");
            return null;
        }

        _apiKeyHolder.Set(storedApiKey);
        try
        {
            DeconzConfigDto config = await _configClient.GetConfigAsync(cancellationToken);
            _logger.LogInformation("API key deCONZ salvata verificata.");
            return config;
        }
        catch (DeconzApiException ex) when (ex.HasErrorType(DeconzErrorTypes.UnauthorizedUser))
        {
            _logger.LogWarning("L'API key deCONZ salvata non è più valida.");
            return null;
        }
    }

    /// <summary>
    /// Attende lo sblocco del gateway da Phoscon ripetendo <c>POST /api</c>; salva e attiva la chiave ottenuta.
    /// </summary>
    private async Task WaitForNewApiKeyAsync(CancellationToken cancellationToken)
    {
        DeconzGatewayOptions options = _options.Value;
        var request = new DeconzAuthRequestDto { DeviceType = options.DeviceType };
        TimeSpan retryInterval = TimeSpan.FromSeconds(options.UnlockRetrySeconds);

        _logger.LogWarning("In attesa dello sblocco del gateway: usare \"Authenticate app\" in Phoscon.");
        WaitingForUnlock?.Invoke(this, EventArgs.Empty);

        while (true)
        {
            string? newApiKey = await TryRequestApiKeyAsync(request, cancellationToken);
            if (newApiKey is not null)
            {
                await ActivateNewApiKeyAsync(newApiKey, cancellationToken);
                return;
            }

            await Task.Delay(retryInterval, _timeProvider, cancellationToken);
        }
    }

    /// <summary>Un tentativo di <c>POST /api</c>.</summary>
    /// <returns>La nuova chiave, oppure <c>null</c> se il gateway non è ancora sbloccato (errore 101).</returns>
    private async Task<string?> TryRequestApiKeyAsync(DeconzAuthRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<JsonElement> successes = await _restClient.PostWithoutKeyAsync(string.Empty, request, cancellationToken);
            string? apiKey = successes
                .Select(success => success.Deserialize<DeconzAuthResponseDto>(DeconzJson.Options)?.UserName)
                .FirstOrDefault(userName => !string.IsNullOrEmpty(userName));

            return apiKey ?? throw new InvalidDataException("deCONZ ha accettato la richiesta ma non ha restituito la chiave.");
        }
        catch (DeconzApiException ex) when (ex.HasErrorType(DeconzErrorTypes.LinkButtonNotPressed))
        {
            return null;
        }
    }

    private async Task ActivateNewApiKeyAsync(string apiKey, CancellationToken cancellationToken)
    {
        DateTimeOffset obtainedAt = _timeProvider.GetLocalNow();
        await _apiKeyStore.SaveAsync(apiKey, obtainedAt, cancellationToken);
        _apiKeyHolder.Set(apiKey);

        _logger.LogInformation("Nuova API key deCONZ ottenuta e salvata.");
        ApiKeyObtained?.Invoke(this, new ApiKeyObtainedEventArgs(obtainedAt));
    }
}
