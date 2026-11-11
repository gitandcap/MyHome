namespace DCB.Gateway.Core;

/// <summary>
/// deCONZ non risponde: connessione rifiutata, indirizzo errato o tempo di risposta scaduto.
/// </summary>
/// <remarks>
/// Il Gateway fa un solo tentativo e segnala il problema con questa eccezione: la ripetizione spetta a chi
/// lo usa (in MyHome il bootstrap <c>DCB-BOT</c>, che apre la condizione "deCONZ non raggiungibile" e riprova
/// con attese crescenti; ANA-06 §4.2, PA-04).
/// Caso tipico: deCONZ in locale parte solo dopo il login, il DCB (servizio) prima.
/// </remarks>
public sealed class DeconzUnreachableException : Exception
{
    /// <summary>Crea l'eccezione.</summary>
    /// <param name="message">Descrizione del problema.</param>
    /// <param name="innerException">Eccezione originale di rete o di timeout.</param>
    public DeconzUnreachableException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
