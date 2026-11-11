using System.Text.Json.Serialization;
using DCB.Gateway.Dto.Config;
using DCB.Gateway.Dto.Groups;
using DCB.Gateway.Dto.Lights;
using DCB.Gateway.Dto.Sensors;

namespace DCB.Gateway.Dto.FullState;

/// <summary>
/// Stato completo del gateway, restituito da <c>GET /api/{key}</c> (<c>DCB-GTW-FUL</c>, ANA-06 §4.5).
/// </summary>
/// <remarks>
/// Composto dai DTO delle singole risorse; i dizionari sono indicizzati per id deCONZ.
/// Sono mappate solo le sezioni che servono a <c>DBZ</c> (<c>lights</c>, <c>sensors</c>, <c>groups</c>) e <c>config</c>:
/// le altre (<c>rules</c>, <c>schedules</c>, <c>scenes</c>, <c>alarmsystems</c>, <c>resourcelinks</c>) sono ignorate
/// finché non arrivano i rispettivi sottomoduli. Volutamente <b>non</b> c'è <c>[JsonExtensionData]</c>: conserverebbe
/// anche la <c>whitelist</c> della configurazione (tutte le API key).
/// </remarks>
public sealed class DeconzFullStateDto
{
    /// <summary>Configurazione del gateway (senza <c>whitelist</c>, vedi <see cref="DeconzConfigDto"/>).</summary>
    [JsonPropertyName("config")]
    public DeconzConfigDto? Config { get; set; }

    /// <summary>Luci, prese, relè e tapparelle, per id deCONZ.</summary>
    [JsonPropertyName("lights")]
    public Dictionary<string, DeconzLightDto> Lights { get; set; } = [];

    /// <summary>Sensori fisici e virtuali, per id deCONZ.</summary>
    [JsonPropertyName("sensors")]
    public Dictionary<string, DeconzSensorDto> Sensors { get; set; } = [];

    /// <summary>Gruppi, per id deCONZ.</summary>
    [JsonPropertyName("groups")]
    public Dictionary<string, DeconzGroupDto> Groups { get; set; } = [];
}
