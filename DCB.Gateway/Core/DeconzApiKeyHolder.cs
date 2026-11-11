namespace DCB.Gateway.Core;

/// <summary>
/// API key in uso durante l'esecuzione, condivisa da tutti i client REST.
/// </summary>
/// <remarks>
/// È impostata da <c>DCB-GTW-AUT</c> dopo la verifica o il rilascio di una nuova chiave; la copia persistente
/// sta invece in <see cref="Abstractions.IApiKeyStore"/>.
/// </remarks>
internal sealed class DeconzApiKeyHolder
{
    private volatile string? _apiKey;

    /// <summary>Chiave corrente, oppure <c>null</c> se l'autenticazione non è ancora avvenuta.</summary>
    public string? ApiKey => _apiKey;

    /// <summary>Imposta la chiave da usare nelle chiamate successive.</summary>
    /// <param name="apiKey">Chiave verificata o appena rilasciata da deCONZ.</param>
    public void Set(string apiKey) => _apiKey = apiKey;
}
