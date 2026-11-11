namespace DCB.Gateway.WebSocket;

/// <summary>
/// Stato della connessione al WebSocket di deCONZ, notificato da <see cref="IDeconzEventStream.ConnectionStateChanged"/>.
/// </summary>
public enum DeconzConnectionState
{
    /// <summary>Non collegato (stato iniziale, oppure connessione appena persa).</summary>
    Disconnected,

    /// <summary>Connessione persa o non riuscita: sono in corso i tentativi di riconnessione.</summary>
    Reconnecting,

    /// <summary>Collegato: gli eventi arrivano.</summary>
    Connected,
}
