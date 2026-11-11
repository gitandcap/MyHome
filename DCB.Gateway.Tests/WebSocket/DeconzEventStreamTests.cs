using DCB.Gateway.Dto.WebSocket;
using DCB.Gateway.Tests.Fakes;
using DCB.Gateway.Tests.TestData;
using DCB.Gateway.WebSocket;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace DCB.Gateway.Tests.WebSocket;

public class DeconzEventStreamTests
{
    /// <summary>Evento reale ricevuto l'11/11/2026 (attributo lastseen di una tapparella).</summary>
    private const string LastSeenEvent =
        """{"attr":{"lastseen":"2026-11-11T19:49Z"},"e":"changed","id":"5","r":"lights","t":"event","uniqueid":"f4:ce:36:21:18:17:83:a1-0a"}""";

    private const string ButtonEvent =
        """{"t":"event","e":"changed","r":"sensors","id":"6","uniqueid":"cc:86:ec:ff:fe:8c:d8:ba-01-1000","state":{"buttonevent":1002,"lastupdated":"2026-11-11T19:50:01.123"}}""";

    private const string SensorAddedEvent =
        """{"t":"event","e":"added","r":"sensors","id":"7","sensor":{"name":"Switch 7","type":"ZHASwitch","uniqueid":"00:11:22:33:44:55:66:77-01-1000","state":{"buttonevent":null}}}""";

    [Fact]
    public async Task WithoutConfiguredPort_UsesWebsocketPortFromDeconzConfig()
    {
        var connection = FakeWebSocketConnection.DeliversThenStaysOpen(LastSeenEvent);
        var (context, stream, _) = CreateStream(new FakeWebSocketConnectionFactory(connection));

        await ReadEventsAsync(context, stream, count: 1);

        // websocketport letto da /config (80, PA-19), connessione non cifrata.
        Assert.Equal("ws://deconz.test/", connection.ConnectedUri!.ToString());
        Assert.Contains(context.Handler.Requests, request => request.Uri.AbsolutePath.EndsWith("/config"));
    }

    [Fact]
    public async Task ConfiguredPort_TakesPrecedenceOverDeconzConfig()
    {
        var connection = FakeWebSocketConnection.DeliversThenStaysOpen(LastSeenEvent);
        var (context, stream, _) = CreateStream(new FakeWebSocketConnectionFactory(connection));
        context.GatewayOptions.WsPort = 8443;

        await ReadEventsAsync(context, stream, count: 1);

        Assert.Equal("ws://deconz.test:8443/", connection.ConnectedUri!.ToString());
        Assert.Empty(context.Handler.Requests);
    }

    [Fact]
    public async Task Events_AreParsedFromDeconzFormat()
    {
        var factory = new FakeWebSocketConnectionFactory(
            FakeWebSocketConnection.DeliversThenStaysOpen(LastSeenEvent, ButtonEvent, SensorAddedEvent));
        var (context, stream, _) = CreateStream(factory);

        List<DeconzWsEventDto> events = await ReadEventsAsync(context, stream, count: 3);

        Assert.Equal("lastseen", Assert.Single(events[0].Attributes!).Key);
        Assert.Equal("lights", events[0].ResourceType);

        Assert.Equal("changed", events[1].EventType);
        Assert.Equal(1002, events[1].State!["buttonevent"].GetInt32());

        Assert.Equal("added", events[2].EventType);
        Assert.Equal("ZHASwitch", events[2].Sensor?.Type);
    }

    [Fact]
    public async Task InvalidAndNonEventMessages_AreSkipped()
    {
        var factory = new FakeWebSocketConnectionFactory(
            FakeWebSocketConnection.DeliversThenStaysOpen("non è json", """{"t":"altro"}""", ButtonEvent));
        var (context, stream, _) = CreateStream(factory);

        List<DeconzWsEventDto> events = await ReadEventsAsync(context, stream, count: 1);

        Assert.Equal("6", Assert.Single(events).Id);
    }

    [Fact]
    public async Task LostConnection_ReconnectsAndNotifiesReconnection()
    {
        var first = FakeWebSocketConnection.DeliversThenCloses(LastSeenEvent);
        var second = FakeWebSocketConnection.DeliversThenStaysOpen(ButtonEvent);
        var (context, stream, states) = CreateStream(new FakeWebSocketConnectionFactory(first, second));

        List<DeconzWsEventDto> events = await ReadEventsAsync(context, stream, count: 2);

        Assert.Equal(2, events.Count);
        Assert.True(first.IsDisposed);
        Assert.Equal(
            [
                (DeconzConnectionState.Connected, false),
                (DeconzConnectionState.Disconnected, false),
                (DeconzConnectionState.Reconnecting, false),
                (DeconzConnectionState.Connected, true),
            ],
            states);
    }

    [Fact]
    public async Task RefusedConnections_AreRetriedWithGrowingDelays()
    {
        var factory = new FakeWebSocketConnectionFactory(
            FakeWebSocketConnection.Refused(),
            FakeWebSocketConnection.Refused(),
            FakeWebSocketConnection.Refused(),
            FakeWebSocketConnection.DeliversThenStaysOpen(ButtonEvent));
        var (context, stream, states) = CreateStream(factory);
        context.GatewayOptions.WsPort = 80;
        DateTimeOffset start = context.Time.GetUtcNow();

        await ReadEventsAsync(context, stream, count: 1);

        // Attese 2 s + 4 s + 8 s prima del quarto tentativo.
        Assert.True(context.Time.GetUtcNow() - start >= TimeSpan.FromSeconds(14));
        Assert.Equal(4, factory.Created.Count);
        Assert.Equal(
            [(DeconzConnectionState.Reconnecting, false), (DeconzConnectionState.Connected, false)],
            states);
    }

    [Fact]
    public async Task Cancellation_EndsReading()
    {
        var factory = new FakeWebSocketConnectionFactory(FakeWebSocketConnection.DeliversThenStaysOpen());
        var (_, stream, _) = CreateStream(factory);
        using var cancellation = new CancellationTokenSource();

        Task reading = Task.Run(async () =>
        {
            await foreach (DeconzWsEventDto _ in stream.ReadEventsAsync(cancellation.Token))
            {
            }
        });
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => reading);
    }

    private static (GatewayTestContext Context, DeconzEventStream Stream, List<(DeconzConnectionState, bool)> States) CreateStream(
        FakeWebSocketConnectionFactory factory)
    {
        var context = new GatewayTestContext(_ => Fakes.FakeHttpMessageHandler.Json(DeconzSamples.FullConfig));
        context.ApiKeyHolder.Set(DeconzSamples.ApiKey);

        var stream = new DeconzEventStream(
            factory,
            context.ConfigClient,
            Options.Create(context.GatewayOptions),
            context.Time,
            NullLogger<DeconzEventStream>.Instance);

        var states = new List<(DeconzConnectionState, bool)>();
        stream.ConnectionStateChanged += (_, args) => states.Add((args.State, args.IsReconnection));
        return (context, stream, states);
    }

    /// <summary>Legge il numero di eventi indicato, facendo avanzare il tempo simulato per le attese di riconnessione.</summary>
    private static async Task<List<DeconzWsEventDto>> ReadEventsAsync(GatewayTestContext context, DeconzEventStream stream, int count)
    {
        using var safety = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var events = new List<DeconzWsEventDto>();

        Task reading = Task.Run(async () =>
        {
            await foreach (DeconzWsEventDto deconzEvent in stream.ReadEventsAsync(safety.Token))
            {
                events.Add(deconzEvent);
                if (events.Count == count)
                {
                    break;
                }
            }
        });

        while (!reading.IsCompleted)
        {
            context.Time.Advance(TimeSpan.FromSeconds(1));
            await Task.Delay(5);
        }

        await reading;
        return events;
    }
}
