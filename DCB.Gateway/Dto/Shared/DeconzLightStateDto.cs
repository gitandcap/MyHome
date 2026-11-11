using System.Text.Json;
using System.Text.Json.Serialization;

namespace DCB.Gateway.Dto.Shared;

/// <summary>
/// Stato di una luce o di un attuatore (<c>state</c> delle luci, <c>action</c> dei gruppi).
/// </summary>
/// <remarks>
/// Tutti i campi sono facoltativi: ogni tipo di dispositivo ne usa solo una parte
/// (una presa ha solo <c>on</c>, una tapparella <c>lift</c> e <c>open</c>, una lampada a colori <c>hue</c>, <c>sat</c>…).
/// Unità deCONZ: <c>bri</c> 0–255, <c>ct</c> in mired, <c>lift</c> in percentuale di chiusura (0 = aperta).
/// I campi non mappati restano in <see cref="ExtensionData"/>.
/// </remarks>
public sealed class DeconzLightStateDto
{
    /// <summary>Acceso / spento.</summary>
    [JsonPropertyName("on")]
    public bool? On { get; set; }

    /// <summary>Luminosità, 0–255.</summary>
    [JsonPropertyName("bri")]
    public int? Brightness { get; set; }

    /// <summary>Il dispositivo risponde sulla rete Zigbee.</summary>
    [JsonPropertyName("reachable")]
    public bool? Reachable { get; set; }

    /// <summary>Tapparelle: percentuale di chiusura, 0 (aperta) – 100 (chiusa).</summary>
    [JsonPropertyName("lift")]
    public int? Lift { get; set; }

    /// <summary>Tapparelle: aperta (anche solo in parte).</summary>
    [JsonPropertyName("open")]
    public bool? Open { get; set; }

    /// <summary>Tonalità del colore, 0–65535.</summary>
    [JsonPropertyName("hue")]
    public int? Hue { get; set; }

    /// <summary>Saturazione del colore, 0–255.</summary>
    [JsonPropertyName("sat")]
    public int? Saturation { get; set; }

    /// <summary>Temperatura di colore, in mired.</summary>
    [JsonPropertyName("ct")]
    public int? ColorTemperature { get; set; }

    /// <summary>Modalità di colore attiva (<c>hs</c>, <c>xy</c>, <c>ct</c>).</summary>
    [JsonPropertyName("colormode")]
    public string? ColorMode { get; set; }

    /// <summary>Campi non mappati, conservati così come arrivano da deCONZ.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
