using DCB.Gateway.Core;
using DCB.Gateway.Dto.Lights;
using DCB.Gateway.Dto.Shared;

namespace DCB.Gateway.Clients.Lights;

/// <summary>
/// Client delle luci, prese, relè e tapparelle (<c>DCB-GTW-LIG</c>).
/// </summary>
/// <remarks>
/// Le letture servono a prove e debug: in MyHome lo stato arriva da <c>DBZ</c>, tenuto aggiornato da <c>DCB-SYN</c>
/// (ANA-06 §4.6). I comandi sono invece l'uso principale.
/// </remarks>
public interface ILightsClient
{
    /// <summary>Elenco delle luci (<c>GET /api/{key}/lights</c>), per id deCONZ.</summary>
    /// <param name="cancellationToken">Annullamento dell'operazione.</param>
    Task<IReadOnlyDictionary<string, DeconzLightDto>> GetLightsAsync(CancellationToken cancellationToken);

    /// <summary>Dettaglio di una luce (<c>GET /api/{key}/lights/{id}</c>).</summary>
    /// <param name="lightId">Id deCONZ della luce.</param>
    /// <param name="cancellationToken">Annullamento dell'operazione.</param>
    Task<DeconzLightDto> GetLightAsync(string lightId, CancellationToken cancellationToken);

    /// <summary>Comando sullo stato (<c>PUT /api/{key}/lights/{id}/state</c>): accensione, luminosità, tapparelle…</summary>
    /// <param name="lightId">Id deCONZ della luce.</param>
    /// <param name="state">Campi da cambiare; quelli <c>null</c> non vengono inviati.</param>
    /// <param name="cancellationToken">Annullamento dell'operazione.</param>
    Task<DeconzCommandResult> SetStateAsync(string lightId, DeconzLightStateRequestDto state, CancellationToken cancellationToken);
}
