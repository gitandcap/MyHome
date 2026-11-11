using System.Text.Json;
using DCB.Gateway.Clients.FullState;
using DCB.Gateway.Core;
using DCB.Gateway.Dto.FullState;
using DCB.Gateway.Tests.Fakes;
using DCB.Gateway.Tests.TestData;

namespace DCB.Gateway.Tests.Clients;

public class FullStateClientTests
{
    [Fact]
    public async Task GetFullStateAsync_CallsApiRootWithKey()
    {
        var context = new GatewayTestContext(_ => FakeHttpMessageHandler.Json(DeconzSampleFiles.Read(DeconzSampleFiles.FullState)));
        context.ApiKeyHolder.Set(DeconzSamples.ApiKey);
        var client = new FullStateClient(context.RestClient);

        DeconzFullStateDto state = await client.GetFullStateAsync(CancellationToken.None);

        Assert.Equal($"http://deconz.test:8080/api/{DeconzSamples.ApiKey}", context.Handler.Requests[0].Uri.ToString());
        Assert.Equal(11, state.Lights.Count);
        Assert.Equal(6, state.Sensors.Count);
        Assert.Single(state.Groups);
        // Dato reale (deCONZ 2.33.2): WebSocket normale sulla porta REST, cifrato sulla 443 (PA-19).
        Assert.Equal(80, state.Config?.WebSocketPort);
        Assert.Equal(443, state.Config?.WebSocketSecurePort);
    }

    [Fact]
    public void RealFullState_WindowCoveringIsReadAsLightWithLift()
    {
        DeconzFullStateDto state = ReadRealFullState();

        var cover = state.Lights["1"];
        Assert.Equal("Window covering device", cover.Type);
        Assert.Equal("TapparellaSalotto", cover.Name);
        Assert.Equal("f4:ce:36:23:f3:df:c4:63-0a", cover.UniqueId);
        Assert.NotNull(cover.State?.Lift);
        Assert.NotNull(cover.State?.Open);
        Assert.True(cover.State?.Reachable);
    }

    [Fact]
    public void RealFullState_SensorStateKeepsVariableFieldsAndRawDates()
    {
        DeconzFullStateDto state = ReadRealFullState();

        var remote = state.Sensors["6"];
        Assert.Equal("ZHASwitch", remote.Type);
        Assert.Equal(1, remote.Endpoint);
        Assert.True(remote.State!.ContainsKey("buttonevent"));
        // deCONZ usa "none" come data: deve restare leggibile come stringa grezza.
        Assert.Equal("none", remote.State["lastupdated"].GetString());
        Assert.Equal(20, remote.Config!["battery"].GetInt32());
    }

    [Fact]
    public void RealFullState_GroupIsRead()
    {
        DeconzFullStateDto state = ReadRealFullState();

        var group = state.Groups["1"];
        Assert.Equal("LightGroup", group.Type);
        Assert.Equal(["15"], group.Lights);
        Assert.False(group.State?.AnyOn);
    }

    [Fact]
    public void RealFullState_UnmappedFieldsSurviveReserialization()
    {
        // DBZ è una copia grezza: rileggere e riscrivere un dispositivo non deve perdere campi (es. nwkaddress, etag).
        DeconzFullStateDto state = ReadRealFullState();

        string json = JsonSerializer.Serialize(state.Lights["1"], DeconzJson.Options);

        Assert.Contains("\"nwkaddress\":\"0xC126\"", json);
        Assert.Contains("\"etag\"", json);
        Assert.Contains("\"lift\"", json);
    }

    [Fact]
    public void RealFullState_SerializedConfigNeverContainsWhitelist()
    {
        const string fullStateWithWhitelist = """
            { "config": { "websocketport": 443, "whitelist": { "SEGRETA": { "name": "x" } } }, "lights": {}, "sensors": {}, "groups": {} }
            """;

        DeconzFullStateDto state = JsonSerializer.Deserialize<DeconzFullStateDto>(fullStateWithWhitelist, DeconzJson.Options)!;
        string json = JsonSerializer.Serialize(state, DeconzJson.Options);

        Assert.DoesNotContain("SEGRETA", json);
    }

    private static DeconzFullStateDto ReadRealFullState() =>
        JsonSerializer.Deserialize<DeconzFullStateDto>(DeconzSampleFiles.Read(DeconzSampleFiles.FullState), DeconzJson.Options)!;
}
