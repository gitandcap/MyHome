using System.Net;
using DCB.Gateway.Core;
using DCB.Gateway.Dto.Config;
using DCB.Gateway.Tests.Fakes;
using DCB.Gateway.Tests.TestData;

namespace DCB.Gateway.Tests.Core;

public class DeconzRestClientTests
{
    [Fact]
    public async Task GetAsync_UsesHostPortAndApiKeyInUrl()
    {
        var context = new GatewayTestContext(_ => FakeHttpMessageHandler.Json(DeconzSamples.FullConfig));
        context.ApiKeyHolder.Set(DeconzSamples.ApiKey);

        await context.RestClient.GetAsync<DeconzConfigDto>("config", CancellationToken.None);

        RecordedRequest request = Assert.Single(context.Handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal($"http://deconz.test:8080/api/{DeconzSamples.ApiKey}/config", request.Uri.ToString());
    }

    [Fact]
    public async Task GetWithoutKeyAsync_DoesNotPutApiKeyInUrl()
    {
        var context = new GatewayTestContext(_ => FakeHttpMessageHandler.Json(DeconzSamples.PublicConfig));
        context.ApiKeyHolder.Set(DeconzSamples.ApiKey);

        await context.RestClient.GetWithoutKeyAsync<DeconzConfigDto>("config", CancellationToken.None);

        Assert.Equal("http://deconz.test:8080/api/config", context.Handler.Requests[0].Uri.ToString());
    }

    [Fact]
    public async Task GetAsync_WithoutApiKey_ThrowsBeforeCallingDeconz()
    {
        var context = new GatewayTestContext(_ => FakeHttpMessageHandler.Json(DeconzSamples.FullConfig));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.RestClient.GetAsync<DeconzConfigDto>("config", CancellationToken.None));
        Assert.Empty(context.Handler.Requests);
    }

    [Fact]
    public async Task ErrorArrayWithHttp200_ThrowsDeconzApiException()
    {
        // deCONZ può segnalare un errore anche con codice HTTP 2xx.
        var context = new GatewayTestContext(_ => FakeHttpMessageHandler.Json(DeconzSamples.UnauthorizedUserError));
        context.ApiKeyHolder.Set(DeconzSamples.ApiKey);

        var exception = await Assert.ThrowsAsync<DeconzApiException>(
            () => context.RestClient.GetAsync<DeconzConfigDto>("config", CancellationToken.None));

        Assert.True(exception.HasErrorType(DeconzErrorTypes.UnauthorizedUser));
        Assert.Equal(HttpStatusCode.OK, exception.StatusCode);
    }

    [Fact]
    public async Task HttpErrorWithoutJsonBody_ThrowsDeconzApiExceptionWithStatusCode()
    {
        var context = new GatewayTestContext(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("<html>errore</html>"),
        });

        var exception = await Assert.ThrowsAsync<DeconzApiException>(
            () => context.RestClient.GetWithoutKeyAsync<DeconzConfigDto>("config", CancellationToken.None));

        Assert.Empty(exception.Errors);
        Assert.Equal(HttpStatusCode.InternalServerError, exception.StatusCode);
    }

    [Fact]
    public async Task ConnectionRefused_ThrowsDeconzUnreachableException()
    {
        var context = new GatewayTestContext(_ => throw new HttpRequestException("Connessione rifiutata"));

        await Assert.ThrowsAsync<DeconzUnreachableException>(
            () => context.RestClient.GetWithoutKeyAsync<DeconzConfigDto>("config", CancellationToken.None));
    }

    [Fact]
    public async Task NoResponseWithinTimeout_ThrowsDeconzUnreachableException()
    {
        var handler = new FakeHttpMessageHandler(async (_, cancellationToken) =>
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
            throw new InvalidOperationException("Non raggiungibile: la richiesta deve essere annullata.");
        });
        var context = new GatewayTestContext(handler);

        Task<DeconzConfigDto> operation = context.RestClient.GetWithoutKeyAsync<DeconzConfigDto>("config", CancellationToken.None);

        await Assert.ThrowsAsync<DeconzUnreachableException>(
            () => context.RunAdvancingTimeAsync(operation, TimeSpan.FromSeconds(context.GatewayOptions.RequestTimeoutSeconds)));
    }

    [Fact]
    public async Task CallerCancellation_IsNotReportedAsUnreachable()
    {
        var handler = new FakeHttpMessageHandler(async (_, cancellationToken) =>
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
            throw new InvalidOperationException("Non raggiungibile: la richiesta deve essere annullata.");
        });
        var context = new GatewayTestContext(handler);
        using var cancellation = new CancellationTokenSource();

        Task<DeconzConfigDto> operation = context.RestClient.GetWithoutKeyAsync<DeconzConfigDto>("config", cancellation.Token);
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operation);
    }

    [Fact]
    public async Task PostWithoutKeyAsync_SendsOnlyNonNullFieldsAndReturnsSuccesses()
    {
        var context = new GatewayTestContext(_ => FakeHttpMessageHandler.Json(DeconzSamples.NewApiKeySuccess));

        var successes = await context.RestClient.PostWithoutKeyAsync(
            string.Empty, new DeconzAuthRequestDto { DeviceType = "MyHome#DCB" }, CancellationToken.None);

        RecordedRequest request = Assert.Single(context.Handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("http://deconz.test:8080/api", request.Uri.ToString());
        Assert.Equal("""{"devicetype":"MyHome#DCB"}""", request.Body);
        Assert.Single(successes);
    }
}
