namespace DCB.Gateway.WebSocket;

/// <summary>
/// Connessione WebSocket ridotta a ciò che serve a <see cref="DeconzEventStream"/>; sostituibile nei test.
/// </summary>
internal interface IWebSocketConnection : IDisposable
{
    /// <summary>Apre la connessione.</summary>
    /// <param name="uri">Indirizzo <c>ws://host:porta</c>.</param>
    /// <param name="cancellationToken">Annullamento dell'operazione.</param>
    Task ConnectAsync(Uri uri, CancellationToken cancellationToken);

    /// <summary>Attende il prossimo messaggio di testo completo.</summary>
    /// <param name="cancellationToken">Annullamento dell'operazione.</param>
    /// <returns>Il messaggio, oppure <c>null</c> se la connessione è stata chiusa.</returns>
    Task<string?> ReceiveMessageAsync(CancellationToken cancellationToken);
}

/// <summary>Crea una nuova connessione per ogni tentativo (un <c>ClientWebSocket</c> non si può riaprire).</summary>
internal interface IWebSocketConnectionFactory
{
    /// <summary>Crea una connessione non ancora aperta.</summary>
    IWebSocketConnection Create();
}
