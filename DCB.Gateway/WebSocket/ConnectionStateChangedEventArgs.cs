namespace DCB.Gateway.WebSocket;

/// <summary>
/// Dati dell'evento <see cref="IDeconzEventStream.ConnectionStateChanged"/>.
/// </summary>
public sealed class ConnectionStateChangedEventArgs : EventArgs
{
    /// <summary>Crea i dati dell'evento.</summary>
    /// <param name="state">Nuovo stato della connessione.</param>
    /// <param name="isReconnection">Vero se lo stato è <see cref="DeconzConnectionState.Connected"/> dopo una connessione già avvenuta.</param>
    public ConnectionStateChangedEventArgs(DeconzConnectionState state, bool isReconnection)
    {
        State = state;
        IsReconnection = isReconnection;
    }

    /// <summary>Nuovo stato della connessione.</summary>
    public DeconzConnectionState State { get; }

    /// <summary>
    /// Vero quando la connessione è tornata dopo essere stata persa: gli eventi del periodo di disconnessione
    /// sono andati persi e lo stato va riletto (in MyHome lo fa <c>DCB-SYN</c>, ANA-06 §4.7).
    /// Falso alla prima connessione, già coperta dal passo <c>sync</c> del bootstrap.
    /// </summary>
    public bool IsReconnection { get; }
}
