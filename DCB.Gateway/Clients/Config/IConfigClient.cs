using DCB.Gateway.Dto.Config;

namespace DCB.Gateway.Clients.Config;

/// <summary>
/// Client della configurazione del gateway (<c>DCB-GTW-CFG</c>).
/// </summary>
public interface IConfigClient
{
    /// <summary>
    /// Legge la configurazione pubblica, senza API key (<c>GET /api/config</c>).
    /// Serve a verificare che deCONZ risponda (passo <c>gateway</c> del bootstrap, ANA-06 §4.2).
    /// </summary>
    /// <param name="cancellationToken">Annullamento dell'operazione.</param>
    Task<DeconzConfigDto> GetPublicConfigAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Legge la configurazione completa con l'API key corrente (<c>GET /api/{key}/config</c>).
    /// </summary>
    /// <param name="cancellationToken">Annullamento dell'operazione.</param>
    /// <exception cref="Core.DeconzApiException">Chiave non valida (errore <see cref="Core.DeconzErrorTypes.UnauthorizedUser"/>).</exception>
    Task<DeconzConfigDto> GetConfigAsync(CancellationToken cancellationToken);
}
