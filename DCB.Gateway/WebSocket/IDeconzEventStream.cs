using DCB.Gateway.Dto.WebSocket;

namespace DCB.Gateway.WebSocket;

/// <summary>
/// Flusso degli eventi in tempo reale di deCONZ (<c>DCB-GTW-WSS</c>, ANA-06 §4.4).
/// </summary>
/// <remarks>
/// Si collega con <c>ws://</c> alla porta <c>Deconz:WsPort</c> o, se vuota, a <c>websocketport</c> letta da
/// <c>/config</c> (PA-19); per questo va avviato dopo l'autenticazione (passo <c>events</c> del bootstrap).
/// Dopo una disconnessione si riconnette da solo con attese crescenti. Non rilegge lo stato: lo segnala con
/// <see cref="ConnectionStateChanged"/> e la rilettura spetta a <c>DCB-SYN</c>.
/// </remarks>
public interface IDeconzEventStream
{
    /// <summary>Cambio dello stato della connessione (collegato, scollegato, in riconnessione).</summary>
    event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;

    /// <summary>Stato attuale della connessione.</summary>
    DeconzConnectionState State { get; }

    /// <summary>
    /// Legge gli eventi finché non viene annullato, riconnettendosi quando serve. Va letto da un solo consumatore alla volta.
    /// </summary>
    /// <param name="cancellationToken">Annullamento: chiude la connessione e termina la lettura.</param>
    IAsyncEnumerable<DeconzWsEventDto> ReadEventsAsync(CancellationToken cancellationToken);
}
