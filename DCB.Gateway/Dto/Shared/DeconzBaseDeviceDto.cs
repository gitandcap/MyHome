using System.Text.Json;
using System.Text.Json.Serialization;

namespace DCB.Gateway.Dto.Shared;

/// <summary>
/// Campi comuni alle risorse "dispositivo" di deCONZ (luci e sensori).
/// </summary>
/// <remarks>
/// Le date (<c>lastseen</c>, <c>lastannounced</c>) restano stringhe grezze: deCONZ usa formati diversi
/// (es. <c>2026-11-11T18:27Z</c> senza secondi) e valori come <c>none</c>, e <c>DBZ</c> è una copia grezza (ANA-07 §4).
/// I campi non mappati sono conservati in <see cref="ExtensionData"/>, così una lettura seguita da una scrittura
/// (es. in <c>DBZ</c>) non perde informazioni.
/// </remarks>
public abstract class DeconzBaseDeviceDto
{
    /// <summary>Nome assegnato in Phoscon (es. <c>TapparellaSalotto</c>).</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Tipo deCONZ della risorsa (es. <c>Window covering device</c>, <c>ZHASwitch</c>).</summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Identificativo stabile della risorsa: indirizzo MAC del dispositivo più endpoint (e cluster per i sensori).
    /// È la chiave di collegamento con <c>MDZ</c>; l'id numerico deCONZ invece può cambiare.
    /// </summary>
    [JsonPropertyName("uniqueid")]
    public string? UniqueId { get; set; }

    /// <summary>Modello del dispositivo.</summary>
    [JsonPropertyName("modelid")]
    public string? ModelId { get; set; }

    /// <summary>Produttore del dispositivo.</summary>
    [JsonPropertyName("manufacturername")]
    public string? ManufacturerName { get; set; }

    /// <summary>Versione del firmware (può essere vuota).</summary>
    [JsonPropertyName("swversion")]
    public string? SoftwareVersion { get; set; }

    /// <summary>Ultimo contatto con il dispositivo, nel formato grezzo di deCONZ.</summary>
    [JsonPropertyName("lastseen")]
    public string? LastSeen { get; set; }

    /// <summary>Campi non mappati, conservati così come arrivano da deCONZ.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
