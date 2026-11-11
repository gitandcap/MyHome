namespace DCB.Gateway.Authentication;

/// <summary>
/// Dati dell'evento <see cref="IDeconzAuthenticator.ApiKeyObtained"/>.
/// </summary>
/// <remarks>La chiave non è inclusa di proposito: chi ascolta l'evento potrebbe registrarlo nei log.</remarks>
public sealed class ApiKeyObtainedEventArgs : EventArgs
{
    /// <summary>Crea i dati dell'evento.</summary>
    /// <param name="obtainedAt">Data e ora di rilascio della chiave.</param>
    public ApiKeyObtainedEventArgs(DateTimeOffset obtainedAt)
    {
        ObtainedAt = obtainedAt;
    }

    /// <summary>Data e ora di rilascio della chiave.</summary>
    public DateTimeOffset ObtainedAt { get; }
}
