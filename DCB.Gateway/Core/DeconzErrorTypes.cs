namespace DCB.Gateway.Core;

/// <summary>
/// Codici degli errori deCONZ usati dal DCB (campo <c>type</c> di <see cref="Dto.Shared.DeconzErrorDto"/>).
/// </summary>
public static class DeconzErrorTypes
{
    /// <summary>API key non valida: rimossa dalla whitelist, gateway cambiato o resettato.</summary>
    public const int UnauthorizedUser = 1;

    /// <summary>La risorsa richiesta non esiste.</summary>
    public const int ResourceNotAvailable = 3;

    /// <summary>Valore non valido per un parametro (es. fuori intervallo).</summary>
    public const int InvalidValue = 7;

    /// <summary>Gateway non sbloccato: per ottenere una nuova API key serve "Authenticate app" da Phoscon.</summary>
    public const int LinkButtonNotPressed = 101;
}
