using System.Net;
using DCB.Gateway.Core;
using DCB.Gateway.Dto.Config;
using DCB.Gateway.Tests.Fakes;
using DCB.Gateway.Tests.TestData;

namespace DCB.Gateway.Tests.Authentication;

public class DeconzAuthenticatorTests
{
    private static readonly TimeSpan UnlockRetryStep = TimeSpan.FromSeconds(5);

    [Fact]
    public async Task ValidStoredKey_IsReusedWithoutWaitingForUnlock()
    {
        var context = new GatewayTestContext(_ => FakeHttpMessageHandler.Json(DeconzSamples.FullConfig), DeconzSamples.ApiKey);
        var events = new AuthenticatorEventRecorder(context);

        DeconzConfigDto config = await context.Authenticator.EnsureAuthenticatedAsync(CancellationToken.None);

        Assert.Equal(80, config.WebSocketPort);
        Assert.Equal(DeconzSamples.ApiKey, context.ApiKeyHolder.ApiKey);
        Assert.Single(context.Handler.Requests);
        Assert.Equal(0, events.WaitingForUnlockCount);
        Assert.Equal(0, events.ApiKeyObtainedCount);
        Assert.Equal(0, context.ApiKeyStore.SaveCount);
    }

    [Fact]
    public async Task RejectedStoredKey_WaitsForUnlockThenSavesNewKey()
    {
        var gateway = new UnlockingGatewaySimulator(attemptsBeforeUnlock: 2);
        var context = new GatewayTestContext(gateway.Respond, DeconzSamples.ApiKey);
        var events = new AuthenticatorEventRecorder(context);

        DeconzConfigDto config = await context.RunAdvancingTimeAsync(
            context.Authenticator.EnsureAuthenticatedAsync(CancellationToken.None), UnlockRetryStep);

        Assert.Equal(80, config.WebSocketPort);
        Assert.Equal(3, gateway.PostAttempts);
        Assert.Equal(DeconzSamples.NewApiKey, context.ApiKeyStore.ApiKey);
        Assert.Equal(DeconzSamples.NewApiKey, context.ApiKeyHolder.ApiKey);
        Assert.Equal(1, events.WaitingForUnlockCount);
        Assert.Equal(1, events.ApiKeyObtainedCount);
    }

    [Fact]
    public async Task NoStoredKey_GoesStraightToUnlockWait()
    {
        var gateway = new UnlockingGatewaySimulator(attemptsBeforeUnlock: 0);
        var context = new GatewayTestContext(gateway.Respond, storedApiKey: null);
        var events = new AuthenticatorEventRecorder(context);

        await context.RunAdvancingTimeAsync(
            context.Authenticator.EnsureAuthenticatedAsync(CancellationToken.None), UnlockRetryStep);

        Assert.Equal(HttpMethod.Post, context.Handler.Requests[0].Method);
        Assert.Equal("""{"devicetype":"MyHome#DCB"}""", context.Handler.Requests[0].Body);
        Assert.Equal(1, events.WaitingForUnlockCount);
        Assert.Equal(DeconzSamples.NewApiKey, context.ApiKeyStore.ApiKey);
    }

    [Fact]
    public async Task UnlockWait_StopsWhenCancelled()
    {
        var gateway = new UnlockingGatewaySimulator(attemptsBeforeUnlock: int.MaxValue);
        var context = new GatewayTestContext(gateway.Respond, storedApiKey: null);
        using var cancellation = new CancellationTokenSource();

        Task<DeconzConfigDto> operation = context.Authenticator.EnsureAuthenticatedAsync(cancellation.Token);
        context.Time.Advance(UnlockRetryStep);
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operation);
        Assert.Null(context.ApiKeyStore.ApiKey);
    }

    [Fact]
    public async Task UnreachableDeconz_IsReportedToCaller()
    {
        var context = new GatewayTestContext(_ => throw new HttpRequestException("Connessione rifiutata"), DeconzSamples.ApiKey);

        await Assert.ThrowsAsync<DeconzUnreachableException>(
            () => context.Authenticator.EnsureAuthenticatedAsync(CancellationToken.None));
    }

    /// <summary>
    /// Simula deCONZ: rifiuta la chiave salvata e risponde 101 a <c>POST /api</c> finché il gateway non viene "sbloccato".
    /// </summary>
    private sealed class UnlockingGatewaySimulator
    {
        private readonly int _attemptsBeforeUnlock;

        public UnlockingGatewaySimulator(int attemptsBeforeUnlock)
        {
            _attemptsBeforeUnlock = attemptsBeforeUnlock;
        }

        public int PostAttempts { get; private set; }

        public HttpResponseMessage Respond(HttpRequestMessage request)
        {
            string path = request.RequestUri!.AbsolutePath;

            if (request.Method == HttpMethod.Post && path == "/api")
            {
                PostAttempts++;
                return PostAttempts > _attemptsBeforeUnlock
                    ? FakeHttpMessageHandler.Json(DeconzSamples.NewApiKeySuccess)
                    : FakeHttpMessageHandler.Json(DeconzSamples.LinkButtonNotPressedError, HttpStatusCode.Forbidden);
            }

            if (path == $"/api/{DeconzSamples.NewApiKey}/config")
            {
                return FakeHttpMessageHandler.Json(DeconzSamples.FullConfig);
            }

            return FakeHttpMessageHandler.Json(DeconzSamples.UnauthorizedUserError, HttpStatusCode.Forbidden);
        }
    }

    private sealed class AuthenticatorEventRecorder
    {
        public AuthenticatorEventRecorder(GatewayTestContext context)
        {
            context.Authenticator.WaitingForUnlock += (_, _) => WaitingForUnlockCount++;
            context.Authenticator.ApiKeyObtained += (_, _) => ApiKeyObtainedCount++;
        }

        public int WaitingForUnlockCount { get; private set; }

        public int ApiKeyObtainedCount { get; private set; }
    }
}
