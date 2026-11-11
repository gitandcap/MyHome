using DCB.Gateway.Core;

namespace DCB.Gateway.Clients.Scenes;

/// <summary>
/// Client delle scene (<c>DCB-GTW-SCN</c>).
/// </summary>
/// <remarks>
/// Nella prima versione solo il richiamo; creazione e salvataggio delle scene si fanno da Phoscon (PA-16).
/// L'elenco delle scene di un gruppo è già in <see cref="Dto.Groups.DeconzGroupDto.Scenes"/>.
/// </remarks>
public interface IScenesClient
{
    /// <summary>Richiama una scena (<c>PUT /api/{key}/groups/{groupId}/scenes/{sceneId}/recall</c>).</summary>
    /// <param name="groupId">Id deCONZ del gruppo a cui appartiene la scena.</param>
    /// <param name="sceneId">Id della scena nel gruppo.</param>
    /// <param name="cancellationToken">Annullamento dell'operazione.</param>
    Task<DeconzCommandResult> RecallSceneAsync(string groupId, string sceneId, CancellationToken cancellationToken);
}
