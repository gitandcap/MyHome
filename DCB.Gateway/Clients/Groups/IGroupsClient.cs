using DCB.Gateway.Core;
using DCB.Gateway.Dto.Groups;

namespace DCB.Gateway.Clients.Groups;

/// <summary>
/// Client dei gruppi (<c>DCB-GTW-GRP</c>): comandi a più dispositivi con un solo messaggio Zigbee.
/// </summary>
public interface IGroupsClient
{
    /// <summary>Elenco dei gruppi (<c>GET /api/{key}/groups</c>), per id deCONZ.</summary>
    /// <param name="cancellationToken">Annullamento dell'operazione.</param>
    Task<IReadOnlyDictionary<string, DeconzGroupDto>> GetGroupsAsync(CancellationToken cancellationToken);

    /// <summary>Dettaglio di un gruppo (<c>GET /api/{key}/groups/{id}</c>).</summary>
    /// <param name="groupId">Id deCONZ del gruppo.</param>
    /// <param name="cancellationToken">Annullamento dell'operazione.</param>
    Task<DeconzGroupDto> GetGroupAsync(string groupId, CancellationToken cancellationToken);

    /// <summary>Comando a tutte le luci del gruppo (<c>PUT /api/{key}/groups/{id}/action</c>).</summary>
    /// <param name="groupId">Id deCONZ del gruppo.</param>
    /// <param name="action">Campi da cambiare; quelli <c>null</c> non vengono inviati.</param>
    /// <param name="cancellationToken">Annullamento dell'operazione.</param>
    Task<DeconzCommandResult> SetActionAsync(string groupId, DeconzGroupActionRequestDto action, CancellationToken cancellationToken);
}
