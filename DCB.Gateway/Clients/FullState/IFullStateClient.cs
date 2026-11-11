using DCB.Gateway.Dto.FullState;

namespace DCB.Gateway.Clients.FullState;

/// <summary>
/// Client dello stato completo del gateway (<c>DCB-GTW-FUL</c>).
/// </summary>
/// <remarks>
/// Usato da <c>DCB-SYN</c> per la lettura completa (passo <c>sync</c> e dopo ogni riconnessione del WebSocket,
/// ANA-06 §4.7): una sola chiamata, quindi una fotografia dello stesso istante.
/// </remarks>
public interface IFullStateClient
{
    /// <summary>Legge lo stato completo con l'API key corrente (<c>GET /api/{key}</c>).</summary>
    /// <param name="cancellationToken">Annullamento dell'operazione.</param>
    Task<DeconzFullStateDto> GetFullStateAsync(CancellationToken cancellationToken);
}
