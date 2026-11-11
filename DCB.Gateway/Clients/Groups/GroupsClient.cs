using DCB.Gateway.Core;
using DCB.Gateway.Dto.Groups;

namespace DCB.Gateway.Clients.Groups;

/// <summary>
/// Implementazione di <see cref="IGroupsClient"/> sopra <see cref="DeconzRestClient"/>.
/// </summary>
internal sealed class GroupsClient : IGroupsClient
{
    private const string GroupsResource = "groups";
    private const string ActionResource = "action";

    private readonly DeconzRestClient _restClient;

    /// <summary>Crea il client.</summary>
    public GroupsClient(DeconzRestClient restClient)
    {
        _restClient = restClient;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, DeconzGroupDto>> GetGroupsAsync(CancellationToken cancellationToken) =>
        await _restClient.GetAsync<Dictionary<string, DeconzGroupDto>>(GroupsResource, cancellationToken);

    /// <inheritdoc />
    public Task<DeconzGroupDto> GetGroupAsync(string groupId, CancellationToken cancellationToken) =>
        _restClient.GetAsync<DeconzGroupDto>(DeconzResourcePath.Combine(GroupsResource, groupId), cancellationToken);

    /// <inheritdoc />
    public Task<DeconzCommandResult> SetActionAsync(
        string groupId, DeconzGroupActionRequestDto action, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(action);
        return _restClient.PutAsync(DeconzResourcePath.Combine(GroupsResource, groupId, ActionResource), action, cancellationToken);
    }
}
