using DCB.Gateway.Authentication;
using DCB.Gateway.Clients.Config;
using DCB.Gateway.Clients.FullState;
using DCB.Gateway.Configuration;
using DCB.Gateway.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DCB.Gateway.DependencyInjection;

/// <summary>
/// Registrazione del Gateway Connector nel contenitore dei servizi.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra i servizi di <c>DCB.Gateway</c>.
    /// </summary>
    /// <remarks>
    /// Chi usa la libreria deve registrare anche un'implementazione di <see cref="Abstractions.IApiKeyStore"/>
    /// (in MyHome: <c>DCB-STO</c> sul file <c>apikey.json</c>).
    /// I servizi sono singleton: l'autenticatore conserva chi è iscritto ai suoi eventi, e il client HTTP è creato
    /// a ogni richiesta da <see cref="IHttpClientFactory"/>, quindi non resta "catturato" nei singleton.
    /// </remarks>
    /// <param name="services">Contenitore dei servizi.</param>
    /// <param name="configureOptions">Impostazione dei parametri (in MyHome: lettura della sezione <c>Deconz</c>).</param>
    public static IServiceCollection AddDeconzGateway(
        this IServiceCollection services, Action<DeconzGatewayOptions> configureOptions)
    {
        services.AddOptions<DeconzGatewayOptions>()
            .Configure(configureOptions)
            .Validate(options => !string.IsNullOrWhiteSpace(options.Host), "Deconz:Host è obbligatorio.")
            .Validate(options => IsValidPort(options.HttpPort), "Deconz:HttpPort deve essere tra 1 e 65535.")
            .Validate(options => options.WsPort is null || IsValidPort(options.WsPort.Value), "Deconz:WsPort deve essere vuota o tra 1 e 65535.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.DeviceType), "Deconz:DeviceType è obbligatorio.")
            .Validate(options => options.RequestTimeoutSeconds > 0, "Deconz:RequestTimeoutSeconds deve essere maggiore di 0.")
            .Validate(options => options.UnlockRetrySeconds > 0, "Deconz:UnlockRetrySeconds deve essere maggiore di 0.");

        services.AddHttpClient(DeconzRestClient.HttpClientName);
        services.AddLogging();
        services.TryAddSingleton(TimeProvider.System);

        services.TryAddSingleton<DeconzApiKeyHolder>();
        services.TryAddSingleton<DeconzRestClient>();
        services.TryAddSingleton<IConfigClient, ConfigClient>();
        services.TryAddSingleton<IFullStateClient, FullStateClient>();
        services.TryAddSingleton<IDeconzAuthenticator, DeconzAuthenticator>();

        return services;
    }

    private static bool IsValidPort(int port) => port is >= 1 and <= 65535;
}
