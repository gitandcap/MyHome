using DCB.Gateway.Core;
using DCB.Gateway.Dto.Lights;
using DCB.Gateway.Dto.Shared;

namespace DCB.Gateway.Clients.Lights;

/// <summary>
/// Implementazione di <see cref="ILightsClient"/> sopra <see cref="DeconzRestClient"/>.
/// </summary>
internal sealed class LightsClient : ILightsClient
{
    private const string LightsResource = "lights";
    private const string StateResource = "state";

    private readonly DeconzRestClient _restClient;

    /// <summary>Crea il client.</summary>
    public LightsClient(DeconzRestClient restClient)
    {
        _restClient = restClient;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, DeconzLightDto>> GetLightsAsync(CancellationToken cancellationToken) =>
        await _restClient.GetAsync<Dictionary<string, DeconzLightDto>>(LightsResource, cancellationToken);

    /// <inheritdoc />
    public Task<DeconzLightDto> GetLightAsync(string lightId, CancellationToken cancellationToken) =>
        _restClient.GetAsync<DeconzLightDto>(DeconzResourcePath.Combine(LightsResource, lightId), cancellationToken);

    /// <inheritdoc />
    public Task<DeconzCommandResult> SetStateAsync(
        string lightId, DeconzLightStateRequestDto state, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(state);
        return _restClient.PutAsync(DeconzResourcePath.Combine(LightsResource, lightId, StateResource), state, cancellationToken);
    }
}
