using System.Net;
using DCB.Gateway.Core;
using DCB.Gateway.Dto.Config;
using DCB.Gateway.Dto.Shared;

namespace DCB.Gateway.Clients.Config;

/// <summary>
/// Implementazione di <see cref="IConfigClient"/> sopra <see cref="DeconzRestClient"/>.
/// </summary>
internal sealed class ConfigClient : IConfigClient
{
    private const string ConfigResource = "config";

    private readonly DeconzRestClient _restClient;

    /// <summary>Crea il client.</summary>
    public ConfigClient(DeconzRestClient restClient)
    {
        _restClient = restClient;
    }

    /// <inheritdoc />
    public Task<DeconzConfigDto> GetPublicConfigAsync(CancellationToken cancellationToken) =>
        _restClient.GetWithoutKeyAsync<DeconzConfigDto>(ConfigResource, cancellationToken);

    /// <inheritdoc />
    public async Task<DeconzConfigDto> GetConfigAsync(CancellationToken cancellationToken)
    {
        DeconzConfigDto config = await _restClient.GetAsync<DeconzConfigDto>(ConfigResource, cancellationToken);

        // Cautela: alcuni gateway rispondono a una chiave non valida con la sola configurazione pubblica (HTTP 200)
        // invece che con l'errore 1. La porta del WebSocket compare solo nella risposta autorizzata ed è proprio
        // il dato che serve dopo l'autenticazione: se manca, la chiave è trattata come non valida (PA-18).
        if (config.WebSocketPort is null)
        {
            throw CreateUnauthorizedException();
        }

        return config;
    }

    private static DeconzApiException CreateUnauthorizedException()
    {
        var error = new DeconzErrorDto
        {
            Type = DeconzErrorTypes.UnauthorizedUser,
            Address = "/" + ConfigResource,
            Description = "configurazione ridotta: chiave non riconosciuta (rilevato dal DCB)",
        };

        return new DeconzApiException([error], HttpStatusCode.OK);
    }
}
