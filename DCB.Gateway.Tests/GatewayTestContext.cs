using DCB.Gateway.Authentication;
using DCB.Gateway.Clients.Config;
using DCB.Gateway.Configuration;
using DCB.Gateway.Core;
using DCB.Gateway.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace DCB.Gateway.Tests;

/// <summary>
/// Costruisce i servizi del Gateway collegati a rete, tempo e archivio delle chiavi finti.
/// </summary>
internal sealed class GatewayTestContext
{
    public GatewayTestContext(Func<HttpRequestMessage, HttpResponseMessage> respond, string? storedApiKey = null)
        : this(new FakeHttpMessageHandler(respond), storedApiKey)
    {
    }

    public GatewayTestContext(FakeHttpMessageHandler handler, string? storedApiKey = null)
    {
        Handler = handler;
        ApiKeyStore = new InMemoryApiKeyStore(storedApiKey);
        RestClient = new DeconzRestClient(
            new FakeHttpClientFactory(Handler),
            ApiKeyHolder,
            Options.Create(GatewayOptions),
            Time,
            NullLogger<DeconzRestClient>.Instance);
        ConfigClient = new ConfigClient(RestClient);
        Authenticator = new DeconzAuthenticator(
            ApiKeyStore,
            ApiKeyHolder,
            ConfigClient,
            RestClient,
            Options.Create(GatewayOptions),
            Time,
            NullLogger<DeconzAuthenticator>.Instance);
    }

    public DeconzGatewayOptions GatewayOptions { get; } = new() { Host = "deconz.test", HttpPort = 8080 };

    public FakeTimeProvider Time { get; } = new();

    public FakeHttpMessageHandler Handler { get; }

    public InMemoryApiKeyStore ApiKeyStore { get; }

    public DeconzApiKeyHolder ApiKeyHolder { get; } = new();

    public DeconzRestClient RestClient { get; }

    public ConfigClient ConfigClient { get; }

    public DeconzAuthenticator Authenticator { get; }

    /// <summary>
    /// Fa avanzare il tempo simulato a passi finché l'operazione non termina (attese di sblocco, timeout).
    /// </summary>
    public async Task<T> RunAdvancingTimeAsync<T>(Task<T> operation, TimeSpan step, int maxSteps = 50)
    {
        for (int i = 0; i < maxSteps && !operation.IsCompleted; i++)
        {
            Time.Advance(step);
            await Task.Delay(5);
        }

        return await operation;
    }
}
