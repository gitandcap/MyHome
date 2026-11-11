using DCB.Gateway.Core;

namespace DCB.Gateway.Clients.Scenes;

/// <summary>
/// Implementazione di <see cref="IScenesClient"/> sopra <see cref="DeconzRestClient"/>.
/// </summary>
internal sealed class ScenesClient : IScenesClient
{
    private const string GroupsResource = "groups";
    private const string ScenesResource = "scenes";
    private const string RecallResource = "recall";

    private readonly DeconzRestClient _restClient;

    /// <summary>Crea il client.</summary>
    public ScenesClient(DeconzRestClient restClient)
    {
        _restClient = restClient;
    }

    /// <inheritdoc />
    public Task<DeconzCommandResult> RecallSceneAsync(string groupId, string sceneId, CancellationToken cancellationToken)
    {
        string resource = DeconzResourcePath.Combine(GroupsResource, groupId, ScenesResource, sceneId, RecallResource);

        // Il richiamo di una scena non ha corpo: tutti i valori sono già memorizzati nella scena.
        return _restClient.PutAsync(resource, body: null, cancellationToken);
    }
}
