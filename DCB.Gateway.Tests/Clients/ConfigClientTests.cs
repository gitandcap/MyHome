using DCB.Gateway.Core;
using DCB.Gateway.Tests.Fakes;
using DCB.Gateway.Tests.TestData;

namespace DCB.Gateway.Tests.Clients;

public class ConfigClientTests
{
    [Fact]
    public async Task GetConfigAsync_ReadsFieldsNeededByDcb()
    {
        var context = new GatewayTestContext(_ => FakeHttpMessageHandler.Json(DeconzSamples.FullConfig));
        context.ApiKeyHolder.Set(DeconzSamples.ApiKey);

        var config = await context.ConfigClient.GetConfigAsync(CancellationToken.None);

        Assert.Equal("00212EFFFF051A5C", config.BridgeId);
        Assert.Equal("1.16.0", config.ApiVersion);
        Assert.Equal("2.33.2", config.SoftwareVersion);
        Assert.Equal(80, config.WebSocketPort);
        Assert.Equal(15, config.ZigbeeChannel);
    }

    [Fact]
    public async Task GetConfigAsync_ReducedResponse_IsTreatedAsUnauthorized()
    {
        // Configurazione pubblica restituita con HTTP 200 a una chiave non riconosciuta (PA-18).
        var context = new GatewayTestContext(_ => FakeHttpMessageHandler.Json(DeconzSamples.PublicConfig));
        context.ApiKeyHolder.Set(DeconzSamples.ApiKey);

        var exception = await Assert.ThrowsAsync<DeconzApiException>(
            () => context.ConfigClient.GetConfigAsync(CancellationToken.None));

        Assert.True(exception.HasErrorType(DeconzErrorTypes.UnauthorizedUser));
    }

    [Fact]
    public async Task GetPublicConfigAsync_AcceptsMissingFields()
    {
        var context = new GatewayTestContext(_ => FakeHttpMessageHandler.Json(DeconzSamples.PublicConfig));

        var config = await context.ConfigClient.GetPublicConfigAsync(CancellationToken.None);

        Assert.Equal("00212EFFFF051A5C", config.BridgeId);
        Assert.Null(config.WebSocketPort);
        Assert.Equal("http://deconz.test:8080/api/config", context.Handler.Requests[0].Uri.ToString());
    }
}
