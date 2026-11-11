using System.Text.Json.Serialization;

namespace DCB.Gateway.Dto.Config;

/// <summary>
/// Configurazione del gateway restituita da <c>GET /api/config</c> (senza chiave, campi ridotti)
/// o da <c>GET /api/{key}/config</c> (completa).
/// </summary>
/// <remarks>
/// Sono mappati solo i campi che servono al DCB; gli altri sono ignorati.
/// La <c>whitelist</c> (elenco di tutte le API key registrate) <b>non è mappata di proposito</b>:
/// così non può finire nei log né nei file del DCB (ANA-06 §4.5).
/// Nella risposta senza chiave mancano diversi campi (es. <c>websocketport</c>): per questo sono facoltativi.
/// </remarks>
public sealed class DeconzConfigDto
{
    /// <summary>Nome del gateway.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Identificativo univoco del gateway (es. <c>00212EFFFF051A5C</c>).</summary>
    [JsonPropertyName("bridgeid")]
    public string? BridgeId { get; set; }

    /// <summary>Versione delle REST API (es. <c>1.16.0</c>).</summary>
    [JsonPropertyName("apiversion")]
    public string? ApiVersion { get; set; }

    /// <summary>Versione del software deCONZ (es. <c>2.33.2</c>).</summary>
    [JsonPropertyName("swversion")]
    public string? SoftwareVersion { get; set; }

    /// <summary>
    /// Porta del WebSocket degli eventi senza cifratura (<c>ws://</c>); presente solo nella risposta con chiave.
    /// Su deCONZ 2.33.2 in locale coincide con la porta REST (80).
    /// </summary>
    [JsonPropertyName("websocketport")]
    public int? WebSocketPort { get; set; }

    /// <summary>
    /// Porta del WebSocket cifrato (<c>wss://</c>), se deCONZ la espone (443 su deCONZ 2.33.2 in locale).
    /// Quale delle due usare è il punto aperto PA-19.
    /// </summary>
    [JsonPropertyName("websocketport_wss")]
    public int? WebSocketSecurePort { get; set; }

    /// <summary>Indirizzo IP del gateway; presente solo nella risposta con chiave.</summary>
    [JsonPropertyName("ipaddress")]
    public string? IpAddress { get; set; }

    /// <summary>Canale Zigbee in uso; presente solo nella risposta con chiave.</summary>
    [JsonPropertyName("zigbeechannel")]
    public int? ZigbeeChannel { get; set; }
}
