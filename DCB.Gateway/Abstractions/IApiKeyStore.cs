namespace DCB.Gateway.Abstractions;

/// <summary>
/// Conserva l'ultima API key deCONZ utilizzata, così all'avvio non se ne crea una nuova (ANA-06 §4.3).
/// </summary>
/// <remarks>
/// Punto di contatto sostituibile di <c>DCB-GTW-AUT</c>: in MyHome è implementato da <c>DCB-STO</c> sul file
/// <c>apikey.json</c> (<c>KEY</c>, ANA-07 §3.3); nei test da una versione in memoria.
/// </remarks>
public interface IApiKeyStore
{
    /// <summary>Legge l'ultima API key salvata.</summary>
    /// <param name="cancellationToken">Annullamento dell'operazione.</param>
    /// <returns>La chiave, oppure <c>null</c> se non ne è mai stata salvata una.</returns>
    Task<string?> LoadAsync(CancellationToken cancellationToken);

    /// <summary>Salva una nuova API key al posto della precedente.</summary>
    /// <param name="apiKey">Chiave rilasciata da deCONZ.</param>
    /// <param name="obtainedAt">Data e ora di rilascio (informativa).</param>
    /// <param name="cancellationToken">Annullamento dell'operazione.</param>
    Task SaveAsync(string apiKey, DateTimeOffset obtainedAt, CancellationToken cancellationToken);
}
