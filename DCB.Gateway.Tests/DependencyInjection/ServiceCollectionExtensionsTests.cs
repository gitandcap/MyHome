using DCB.Gateway.Abstractions;
using DCB.Gateway.Authentication;
using DCB.Gateway.Clients.Config;
using DCB.Gateway.Clients.FullState;
using DCB.Gateway.Configuration;
using DCB.Gateway.DependencyInjection;
using DCB.Gateway.Tests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DCB.Gateway.Tests.DependencyInjection;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDeconzGateway_RegistersPublicServices()
    {
        using ServiceProvider provider = BuildProvider(_ => { });

        Assert.NotNull(provider.GetRequiredService<IDeconzAuthenticator>());
        Assert.NotNull(provider.GetRequiredService<IConfigClient>());
        Assert.NotNull(provider.GetRequiredService<IFullStateClient>());
        Assert.Same(
            provider.GetRequiredService<IDeconzAuthenticator>(),
            provider.GetRequiredService<IDeconzAuthenticator>());
    }

    [Fact]
    public void AddDeconzGateway_RejectsInvalidOptions()
    {
        using ServiceProvider provider = BuildProvider(options => options.HttpPort = 0);

        var exception = Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IOptions<DeconzGatewayOptions>>().Value);

        Assert.Contains("HttpPort", exception.Message);
    }

    private static ServiceProvider BuildProvider(Action<DeconzGatewayOptions> configure)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IApiKeyStore>(new InMemoryApiKeyStore());
        services.AddDeconzGateway(configure);
        return services.BuildServiceProvider();
    }
}
