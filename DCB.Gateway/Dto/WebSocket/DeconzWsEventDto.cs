using System.Text.Json;
using System.Text.Json.Serialization;
using DCB.Gateway.Dto.Groups;
using DCB.Gateway.Dto.Lights;
using DCB.Gateway.Dto.Sensors;

namespace DCB.Gateway.Dto.WebSocket;

/// <summary>
/// Evento ricevuto dal WebSocket di deCONZ (<c>DCB-GTW-WSS</c>).
/// </summary>
/// <remarks>
/// Esempi reali:
/// <c>{"t":"event","e":"changed","r":"lights","id":"5","uniqueid":"…","attr":{"lastseen":"2026-11-11T19:49Z"}}</c>;
/// <c>{"t":"event","e":"changed","r":"sensors","id":"6","state":{"buttonevent":1002,…}}</c>.
/// Un evento <c>changed</c> contiene solo i campi cambiati, in <see cref="State"/>, <see cref="Config"/> o
/// <see cref="Attributes"/>; un evento <c>added</c> contiene la risorsa intera.
/// Il formato è quello grezzo di deCONZ: l'interpretazione spetta a <c>DCB-SYN</c> (ANA-06 §4.7).
/// </remarks>
public sealed class DeconzWsEventDto
{
    /// <summary>Tipo di messaggio; per gli eventi vale <c>event</c>.</summary>
    [JsonPropertyName("t")]
    public string? MessageType { get; set; }

    /// <summary>Tipo di evento: <c>changed</c>, <c>added</c>, <c>deleted</c>, <c>scene-called</c>.</summary>
    [JsonPropertyName("e")]
    public string? EventType { get; set; }

    /// <summary>Tipo di risorsa: <c>lights</c>, <c>sensors</c>, <c>groups</c>, <c>scenes</c>.</summary>
    [JsonPropertyName("r")]
    public string? ResourceType { get; set; }

    /// <summary>Id deCONZ della risorsa.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>Identificativo stabile della risorsa (luci e sensori).</summary>
    [JsonPropertyName("uniqueid")]
    public string? UniqueId { get; set; }

    /// <summary>Campi di stato cambiati (<c>changed</c>).</summary>
    [JsonPropertyName("state")]
    public Dictionary<string, JsonElement>? State { get; set; }

    /// <summary>Campi di configurazione cambiati (<c>changed</c>, sensori).</summary>
    [JsonPropertyName("config")]
    public Dictionary<string, JsonElement>? Config { get; set; }

    /// <summary>Attributi cambiati, es. <c>lastseen</c>, <c>name</c> (<c>changed</c>).</summary>
    [JsonPropertyName("attr")]
    public Dictionary<string, JsonElement>? Attributes { get; set; }

    /// <summary>Luce intera (<c>added</c> con <c>r = lights</c>).</summary>
    [JsonPropertyName("light")]
    public DeconzLightDto? Light { get; set; }

    /// <summary>Sensore intero (<c>added</c> con <c>r = sensors</c>).</summary>
    [JsonPropertyName("sensor")]
    public DeconzSensorDto? Sensor { get; set; }

    /// <summary>Gruppo intero (<c>added</c> con <c>r = groups</c>).</summary>
    [JsonPropertyName("group")]
    public DeconzGroupDto? Group { get; set; }

    /// <summary>Id del gruppo (<c>scene-called</c>).</summary>
    [JsonPropertyName("gid")]
    public string? GroupId { get; set; }

    /// <summary>Id della scena nel gruppo (<c>scene-called</c>).</summary>
    [JsonPropertyName("scid")]
    public string? SceneId { get; set; }

    /// <summary>Campi non mappati, conservati così come arrivano da deCONZ.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
