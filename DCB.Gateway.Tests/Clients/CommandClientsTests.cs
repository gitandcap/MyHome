using System.Net;
using System.Text.Json.Nodes;
using DCB.Gateway.Clients.Groups;
using DCB.Gateway.Clients.Lights;
using DCB.Gateway.Clients.Scenes;
using DCB.Gateway.Clients.Sensors;
using DCB.Gateway.Core;
using DCB.Gateway.Dto.Groups;
using DCB.Gateway.Dto.Sensors;
using DCB.Gateway.Dto.Shared;
using DCB.Gateway.Tests.Fakes;
using DCB.Gateway.Tests.TestData;

namespace DCB.Gateway.Tests.Clients;

/// <summary>
/// Comandi di <c>DCB-GTW-LIG</c>, <c>-SEN</c>, <c>-GRP</c>, <c>-SCN</c>: URL, metodo, corpo ed esito.
/// </summary>
public class CommandClientsTests
{
    private const string ApplyAllSuccess = """[{ "success": { "/lights/1/state/lift": 50 } }]""";

    [Fact]
    public async Task LightSetState_SendsOnlyTheFieldsToChange()
    {
        var context = CreateContext(_ => FakeHttpMessageHandler.Json(ApplyAllSuccess));
        var client = new LightsClient(context.RestClient);

        DeconzCommandResult result = await client.SetStateAsync("1", new DeconzLightStateRequestDto { Lift = 50 }, CancellationToken.None);

        RecordedRequest request = Assert.Single(context.Handler.Requests);
        Assert.Equal(HttpMethod.Put, request.Method);
        Assert.Equal($"http://deconz.test:8080/api/{DeconzSamples.ApiKey}/lights/1/state", request.Uri.ToString());
        Assert.Equal("""{"lift":50}""", request.Body);
        Assert.True(result.IsFullySuccessful);
        Assert.Equal(50, result.Applied["/lights/1/state/lift"].GetInt32());
    }

    [Fact]
    public async Task LightSetState_StopsWindowCovering()
    {
        var context = CreateContext(_ => FakeHttpMessageHandler.Json("""[{ "success": { "/lights/1/state/stop": true } }]"""));
        var client = new LightsClient(context.RestClient);

        await client.SetStateAsync("1", new DeconzLightStateRequestDto { Stop = true }, CancellationToken.None);

        Assert.Equal("""{"stop":true}""", context.Handler.Requests[0].Body);
    }

    [Fact]
    public async Task PartiallyAppliedCommand_ReportsAppliedFieldsAndErrors()
    {
        var context = CreateContext(_ => FakeHttpMessageHandler.Json(DeconzSamples.MixedResult));
        var client = new LightsClient(context.RestClient);

        DeconzCommandResult result = await client.SetStateAsync(
            "3", new DeconzLightStateRequestDto { On = true, Brightness = 300 }, CancellationToken.None);

        Assert.False(result.IsFullySuccessful);
        Assert.True(result.Applied["/lights/3/state/on"].GetBoolean());
        DeconzErrorDto error = Assert.Single(result.Errors);
        Assert.Equal(DeconzErrorTypes.InvalidValue, error.Type);
    }

    [Fact]
    public async Task CommandRejectedEntirely_ThrowsDeconzApiException()
    {
        const string notFound = """[{ "error": { "type": 3, "address": "/lights/99", "description": "resource, /lights/99, not available" } }]""";
        var context = CreateContext(_ => FakeHttpMessageHandler.Json(notFound, HttpStatusCode.NotFound));
        var client = new LightsClient(context.RestClient);

        var exception = await Assert.ThrowsAsync<DeconzApiException>(
            () => client.SetStateAsync("99", new DeconzLightStateRequestDto { On = true }, CancellationToken.None));

        Assert.True(exception.HasErrorType(DeconzErrorTypes.ResourceNotAvailable));
    }

    [Fact]
    public async Task ResourceIds_AreEscapedInUrl()
    {
        var context = CreateContext(_ => FakeHttpMessageHandler.Json(ApplyAllSuccess));
        var client = new LightsClient(context.RestClient);

        await client.SetStateAsync("1/../../config", new DeconzLightStateRequestDto { On = true }, CancellationToken.None);

        Assert.Equal(
            $"/api/{DeconzSamples.ApiKey}/lights/1%2F..%2F..%2Fconfig/state",
            context.Handler.Requests[0].Uri.AbsolutePath);
    }

    [Fact]
    public async Task GetLights_ReadsRealLightsList()
    {
        string realLights = JsonNode.Parse(DeconzSampleFiles.Read(DeconzSampleFiles.FullState))!["lights"]!.ToJsonString();
        var context = CreateContext(_ => FakeHttpMessageHandler.Json(realLights));
        var client = new LightsClient(context.RestClient);

        var lights = await client.GetLightsAsync(CancellationToken.None);

        Assert.Equal(11, lights.Count);
        Assert.Equal($"/api/{DeconzSamples.ApiKey}/lights", context.Handler.Requests[0].Uri.AbsolutePath);
    }

    [Fact]
    public async Task GroupSetAction_IncludesInheritedFieldsAndToggle()
    {
        var context = CreateContext(_ => FakeHttpMessageHandler.Json("""[{ "success": { "/groups/1/action/toggle": true } }]"""));
        var client = new GroupsClient(context.RestClient);

        await client.SetActionAsync("1", new DeconzGroupActionRequestDto { TransitionTime = 10, Toggle = true }, CancellationToken.None);

        Assert.Equal($"/api/{DeconzSamples.ApiKey}/groups/1/action", context.Handler.Requests[0].Uri.AbsolutePath);
        // L'ordine dei campi nel JSON non conta: si confronta il contenuto.
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("""{"transitiontime":10,"toggle":true}"""),
            JsonNode.Parse(context.Handler.Requests[0].Body!)));
    }

    [Fact]
    public async Task SensorSetConfig_SendsFieldsAtTopLevel()
    {
        var context = CreateContext(_ => FakeHttpMessageHandler.Json("""[{ "success": { "/sensors/5/config/heatsetpoint": 2100 } }]"""));
        var client = new SensorsClient(context.RestClient);

        await client.SetConfigAsync("5", new DeconzSensorConfigRequestDto().Set("heatsetpoint", 2100), CancellationToken.None);

        Assert.Equal($"/api/{DeconzSamples.ApiKey}/sensors/5/config", context.Handler.Requests[0].Uri.AbsolutePath);
        Assert.Equal("""{"heatsetpoint":2100}""", context.Handler.Requests[0].Body);
    }

    [Fact]
    public async Task SensorSetConfig_WithoutFields_IsRejectedBeforeCallingDeconz()
    {
        var context = CreateContext(_ => FakeHttpMessageHandler.Json(ApplyAllSuccess));
        var client = new SensorsClient(context.RestClient);

        await Assert.ThrowsAsync<ArgumentException>(
            () => client.SetConfigAsync("5", new DeconzSensorConfigRequestDto(), CancellationToken.None));
        Assert.Empty(context.Handler.Requests);
    }

    [Fact]
    public async Task SceneRecall_SendsPutWithoutBody()
    {
        var context = CreateContext(_ => FakeHttpMessageHandler.Json("""[{ "success": { "id": "2" } }]"""));
        var client = new ScenesClient(context.RestClient);

        DeconzCommandResult result = await client.RecallSceneAsync("1", "2", CancellationToken.None);

        RecordedRequest request = Assert.Single(context.Handler.Requests);
        Assert.Equal(HttpMethod.Put, request.Method);
        Assert.Equal($"/api/{DeconzSamples.ApiKey}/groups/1/scenes/2/recall", request.Uri.AbsolutePath);
        Assert.Null(request.Body);
        Assert.True(result.IsFullySuccessful);
    }

    private static GatewayTestContext CreateContext(Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        var context = new GatewayTestContext(respond);
        context.ApiKeyHolder.Set(DeconzSamples.ApiKey);
        return context;
    }
}
