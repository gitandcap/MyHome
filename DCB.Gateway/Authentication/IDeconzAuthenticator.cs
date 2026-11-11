using DCB.Gateway.Dto.Config;

namespace DCB.Gateway.Authentication;

/// <summary>
/// Autenticazione verso deCONZ (<c>DCB-GTW-AUT</c>, ANA-06 §4.3): riusa l'ultima API key e, se non è valida,
/// attende lo sblocco del gateway per ottenerne una nuova.
/// </summary>
/// <remarks>
/// Le notifiche sono eventi .NET (COD-02 §3): in MyHome <c>DCB.Server</c> li trasforma nella condizione
/// "attesa sblocco gateway" dello stato attuale (ANA-06 §6).
/// </remarks>
public interface IDeconzAuthenticator
{
    /// <summary>
    /// La chiave manca o non è valida: serve "Authenticate app" da Phoscon. Sollevato una volta all'inizio dell'attesa.
    /// </summary>
    event EventHandler? WaitingForUnlock;

    /// <summary>deCONZ ha rilasciato una nuova API key, già salvata. Chiude l'attesa dello sblocco.</summary>
    event EventHandler<ApiKeyObtainedEventArgs>? ApiKeyObtained;

    /// <summary>
    /// Garantisce una API key valida e la rende disponibile ai client REST.
    /// Se serve, attende lo sblocco del gateway finché non viene annullato.
    /// </summary>
    /// <param name="cancellationToken">Annullamento dell'attesa.</param>
    /// <returns>La configurazione completa letta con la chiave valida (contiene <c>websocketport</c> e versione).</returns>
    /// <exception cref="Core.DeconzUnreachableException">deCONZ non risponde.</exception>
    Task<DeconzConfigDto> EnsureAuthenticatedAsync(CancellationToken cancellationToken);
}
