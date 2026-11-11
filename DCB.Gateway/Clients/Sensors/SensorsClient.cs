using DCB.Gateway.Core;
using DCB.Gateway.Dto.Sensors;

namespace DCB.Gateway.Clients.Sensors;

/// <summary>
/// Implementazione di <see cref="ISensorsClient"/> sopra <see cref="DeconzRestClient"/>.
/// </summary>
internal sealed class SensorsClient : ISensorsClient
{
    private const string SensorsResource = "sensors";
    private const string ConfigResource = "config";

    private readonly DeconzRestClient _restClient;

    /// <summary>Crea il client.</summary>
    public SensorsClient(DeconzRestClient restClient)
    {
        _restClient = restClient;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, DeconzSensorDto>> GetSensorsAsync(CancellationToken cancellationToken) =>
        await _restClient.GetAsync<Dictionary<string, DeconzSensorDto>>(SensorsResource, cancellationToken);

    /// <inheritdoc />
    public Task<DeconzSensorDto> GetSensorAsync(string sensorId, CancellationToken cancellationToken) =>
        _restClient.GetAsync<DeconzSensorDto>(DeconzResourcePath.Combine(SensorsResource, sensorId), cancellationToken);

    /// <inheritdoc />
    public Task<DeconzCommandResult> SetConfigAsync(
        string sensorId, DeconzSensorConfigRequestDto config, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(config);
        if (config.Fields.Count == 0)
        {
            throw new ArgumentException("Nessun campo da modificare nella configurazione del sensore.", nameof(config));
        }

        return _restClient.PutAsync(DeconzResourcePath.Combine(SensorsResource, sensorId, ConfigResource), config, cancellationToken);
    }
}
