using DCB.Gateway.Core;
using DCB.Gateway.Dto.Sensors;

namespace DCB.Gateway.Clients.Sensors;

/// <summary>
/// Client dei sensori (<c>DCB-GTW-SEN</c>).
/// </summary>
/// <remarks>
/// Le letture servono a prove e debug (lo stato arriva da <c>DBZ</c>); il comando principale è la modifica della
/// configurazione, es. la temperatura desiderata di una valvola TRV.
/// </remarks>
public interface ISensorsClient
{
    /// <summary>Elenco dei sensori (<c>GET /api/{key}/sensors</c>), per id deCONZ.</summary>
    /// <param name="cancellationToken">Annullamento dell'operazione.</param>
    Task<IReadOnlyDictionary<string, DeconzSensorDto>> GetSensorsAsync(CancellationToken cancellationToken);

    /// <summary>Dettaglio di un sensore (<c>GET /api/{key}/sensors/{id}</c>).</summary>
    /// <param name="sensorId">Id deCONZ del sensore.</param>
    /// <param name="cancellationToken">Annullamento dell'operazione.</param>
    Task<DeconzSensorDto> GetSensorAsync(string sensorId, CancellationToken cancellationToken);

    /// <summary>Modifica della configurazione (<c>PUT /api/{key}/sensors/{id}/config</c>).</summary>
    /// <param name="sensorId">Id deCONZ del sensore.</param>
    /// <param name="config">Campi da cambiare.</param>
    /// <param name="cancellationToken">Annullamento dell'operazione.</param>
    Task<DeconzCommandResult> SetConfigAsync(string sensorId, DeconzSensorConfigRequestDto config, CancellationToken cancellationToken);
}
