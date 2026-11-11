using DCB.Gateway.Core;
using DCB.Gateway.Dto.FullState;

namespace DCB.Gateway.Clients.FullState;

/// <summary>
/// Implementazione di <see cref="IFullStateClient"/> sopra <see cref="DeconzRestClient"/>.
/// </summary>
internal sealed class FullStateClient : IFullStateClient
{
    /// <summary>Lo stato completo è la radice <c>/api/{key}</c>, senza risorsa.</summary>
    private const string RootResource = "";

    private readonly DeconzRestClient _restClient;

    /// <summary>Crea il client.</summary>
    public FullStateClient(DeconzRestClient restClient)
    {
        _restClient = restClient;
    }

    /// <inheritdoc />
    public Task<DeconzFullStateDto> GetFullStateAsync(CancellationToken cancellationToken) =>
        _restClient.GetAsync<DeconzFullStateDto>(RootResource, cancellationToken);
}
