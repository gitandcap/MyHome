using System.Text.Json;
using System.Text.Json.Serialization;

namespace DCB.Gateway.Dto.Groups;

/// <summary>Scena di un gruppo, come elencata in <see cref="DeconzGroupDto.Scenes"/>.</summary>
public sealed class DeconzSceneDto
{
    /// <summary>Identificativo della scena all'interno del gruppo.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>Nome della scena.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Campi non mappati, conservati così come arrivano da deCONZ.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
