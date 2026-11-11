using System.Text.Json;
using System.Text.Json.Serialization;
using DCB.Gateway.Dto.Shared;

namespace DCB.Gateway.Dto.Sensors;

/// <summary>
/// Risorsa <c>sensors</c> di deCONZ: sensori fisici (pulsanti, movimento, temperatura, valvole TRV…)
/// e sensori virtuali del gateway (es. <c>Daylight</c>, <c>CLIP*</c>).
/// </summary>
/// <remarks>
/// <c>state</c> e <c>config</c> cambiano completamente con il tipo di sensore, per questo sono dizionari
/// (ANA-06 §4.5); i valori si interpretano a partire da <see cref="DeconzBaseDeviceDto.Type"/>.
/// </remarks>
public sealed class DeconzSensorDto : DeconzBaseDeviceDto
{
    /// <summary>Endpoint Zigbee del sensore; assente nei sensori virtuali.</summary>
    [JsonPropertyName("ep")]
    public int? Endpoint { get; set; }

    /// <summary>Stato attuale (es. <c>buttonevent</c>, <c>presence</c>, <c>temperature</c>, <c>lastupdated</c>).</summary>
    [JsonPropertyName("state")]
    public Dictionary<string, JsonElement>? State { get; set; }

    /// <summary>Configurazione (es. <c>on</c>, <c>reachable</c>, <c>battery</c>, <c>heatsetpoint</c>).</summary>
    [JsonPropertyName("config")]
    public Dictionary<string, JsonElement>? Config { get; set; }
}
