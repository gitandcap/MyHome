using System.Net.WebSockets;
using DCB.Gateway.WebSocket;

namespace DCB.Gateway.Tests.Fakes;

/// <summary>
/// WebSocket finto: può rifiutare la connessione, consegnare una serie di messaggi e poi chiudersi
/// oppure restare aperto (in attesa) finché la lettura non viene annullata.
/// </summary>
internal sealed class FakeWebSocketConnection : IWebSocketConnection
{
    private readonly Queue<string> _messages;
    private readonly bool _failToConnect;
    private readonly bool _closeAfterMessages;

    private FakeWebSocketConnection(IEnumerable<string> messages, bool failToConnect, bool closeAfterMessages)
    {
        _messages = new Queue<string>(messages);
        _failToConnect = failToConnect;
        _closeAfterMessages = closeAfterMessages;
    }

    public Uri? ConnectedUri { get; private set; }

    public bool IsDisposed { get; private set; }

    /// <summary>Connessione rifiutata (deCONZ non raggiungibile).</summary>
    public static FakeWebSocketConnection Refused() => new([], failToConnect: true, closeAfterMessages: true);

    /// <summary>Consegna i messaggi e poi chiude la connessione.</summary>
    public static FakeWebSocketConnection DeliversThenCloses(params string[] messages) =>
        new(messages, failToConnect: false, closeAfterMessages: true);

    /// <summary>Consegna i messaggi e poi resta aperta.</summary>
    public static FakeWebSocketConnection DeliversThenStaysOpen(params string[] messages) =>
        new(messages, failToConnect: false, closeAfterMessages: false);

    public Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
    {
        if (_failToConnect)
        {
            throw new WebSocketException("Connessione rifiutata (finta).");
        }

        ConnectedUri = uri;
        return Task.CompletedTask;
    }

    public async Task<string?> ReceiveMessageAsync(CancellationToken cancellationToken)
    {
        if (_messages.TryDequeue(out string? message))
        {
            return message;
        }

        if (_closeAfterMessages)
        {
            return null;
        }

        await Task.Delay(Timeout.Infinite, cancellationToken);
        return null;
    }

    public void Dispose() => IsDisposed = true;
}

/// <summary>Restituisce le connessioni finte nell'ordine; finite quelle, una connessione che resta aperta senza messaggi.</summary>
internal sealed class FakeWebSocketConnectionFactory : IWebSocketConnectionFactory
{
    private readonly Queue<FakeWebSocketConnection> _connections;

    public FakeWebSocketConnectionFactory(params FakeWebSocketConnection[] connections)
    {
        _connections = new Queue<FakeWebSocketConnection>(connections);
    }

    public List<FakeWebSocketConnection> Created { get; } = [];

    public IWebSocketConnection Create()
    {
        FakeWebSocketConnection connection = _connections.TryDequeue(out var next)
            ? next
            : FakeWebSocketConnection.DeliversThenStaysOpen();
        Created.Add(connection);
        return connection;
    }
}
