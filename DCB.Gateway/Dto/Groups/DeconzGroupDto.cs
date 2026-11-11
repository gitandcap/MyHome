using System.Text.Json;
using System.Text.Json.Serialization;
using DCB.Gateway.Dto.Shared;

namespace DCB.Gateway.Dto.Groups;

/// <summary>
/// Risorsa <c>groups</c> di deCONZ: insieme di luci comandabili con un solo messaggio Zigbee.
/// </summary>
public sealed class DeconzGroupDto
{
    /// <summary>Identificativo deCONZ del gruppo.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>Nome del gruppo.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Tipo del gruppo (es. <c>LightGroup</c>).</summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>Id deCONZ delle luci del gruppo.</summary>
    [JsonPropertyName("lights")]
    public List<string>? Lights { get; set; }

    /// <summary>Ultimo comando inviato al gruppo.</summary>
    [JsonPropertyName("action")]
    public DeconzLightStateDto? Action { get; set; }

    /// <summary>Stato riassuntivo delle luci del gruppo.</summary>
    [JsonPropertyName("state")]
    public DeconzGroupStateDto? State { get; set; }

    /// <summary>Scene del gruppo.</summary>
    [JsonPropertyName("scenes")]
    public List<DeconzSceneDto>? Scenes { get; set; }

    /// <summary>Campi non mappati, conservati così come arrivano da deCONZ.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
