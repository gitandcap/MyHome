using System.Text.Json.Serialization;
using DCB.Gateway.Dto.Shared;

namespace DCB.Gateway.Dto.Lights;

/// <summary>
/// Risorsa <c>lights</c> di deCONZ: luci, prese, relè e tapparelle (<c>Window covering device</c>).
/// </summary>
public sealed class DeconzLightDto : DeconzBaseDeviceDto
{
    /// <summary>Il dispositivo gestisce il colore.</summary>
    [JsonPropertyName("hascolor")]
    public bool? HasColor { get; set; }

    /// <summary>Stato attuale.</summary>
    [JsonPropertyName("state")]
    public DeconzLightStateDto? State { get; set; }
}
