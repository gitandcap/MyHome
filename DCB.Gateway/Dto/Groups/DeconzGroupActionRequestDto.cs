using System.Text.Json.Serialization;
using DCB.Gateway.Dto.Shared;

namespace DCB.Gateway.Dto.Groups;

/// <summary>
/// Comando a tutte le luci di un gruppo: corpo di <c>PUT /groups/{id}/action</c>.
/// </summary>
/// <remarks>
/// Stessi campi del comando su una singola luce, inviati con un solo messaggio Zigbee al gruppo; in più <see cref="Toggle"/>.
/// </remarks>
public sealed class DeconzGroupActionRequestDto : DeconzLightStateRequestDto
{
    /// <summary>Inverte lo stato acceso/spento del gruppo (si invia solo <c>true</c>).</summary>
    [JsonPropertyName("toggle")]
    public bool? Toggle { get; set; }
}
