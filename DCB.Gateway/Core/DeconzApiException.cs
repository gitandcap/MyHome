using System.Net;
using DCB.Gateway.Dto.Shared;

namespace DCB.Gateway.Core;

/// <summary>
/// deCONZ ha risposto, ma segnalando uno o più errori (o un codice HTTP di errore).
/// </summary>
/// <remarks>
/// Si distingue da <see cref="DeconzUnreachableException"/>: qui deCONZ è raggiungibile e ha rifiutato la richiesta.
/// </remarks>
public sealed class DeconzApiException : Exception
{
    /// <summary>Crea l'eccezione a partire dagli errori restituiti da deCONZ.</summary>
    /// <param name="errors">Errori letti dalla risposta; può essere vuoto se c'è solo il codice HTTP.</param>
    /// <param name="statusCode">Codice HTTP della risposta.</param>
    public DeconzApiException(IReadOnlyList<DeconzErrorDto> errors, HttpStatusCode statusCode)
        : base(BuildMessage(errors, statusCode))
    {
        Errors = errors;
        StatusCode = statusCode;
    }

    /// <summary>Errori restituiti da deCONZ.</summary>
    public IReadOnlyList<DeconzErrorDto> Errors { get; }

    /// <summary>Codice HTTP della risposta.</summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>Indica se tra gli errori c'è quello del tipo indicato (vedi <see cref="DeconzErrorTypes"/>).</summary>
    /// <param name="errorType">Codice dell'errore deCONZ.</param>
    public bool HasErrorType(int errorType) => Errors.Any(error => error.Type == errorType);

    private static string BuildMessage(IReadOnlyList<DeconzErrorDto> errors, HttpStatusCode statusCode)
    {
        if (errors.Count == 0)
        {
            return $"deCONZ ha risposto con HTTP {(int)statusCode} senza dettagli sull'errore.";
        }

        return $"deCONZ ha segnalato: {string.Join(" | ", errors)}";
    }
}
