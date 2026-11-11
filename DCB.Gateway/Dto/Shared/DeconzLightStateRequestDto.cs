using System.Text.Json.Serialization;

namespace DCB.Gateway.Dto.Shared;

/// <summary>
/// Comando sullo stato di una luce o di un attuatore: corpo di <c>PUT /lights/{id}/state</c>
/// (e base di <see cref="Groups.DeconzGroupActionRequestDto"/> per i gruppi).
/// </summary>
/// <remarks>
/// deCONZ applica <b>ogni campo presente</b> nel corpo: tutte le proprietà sono facoltative e quelle <c>null</c>
/// non vengono inviate (PA-15). Esempio: per accendere basta <c>new() { On = true }</c> → <c>{"on":true}</c>.
/// Le unità sono quelle di deCONZ: la conversione dalle unità di MyHome e la validazione degli intervalli spettano
/// a <c>DCB.Server</c>; un valore fuori intervallo produce l'errore deCONZ 7 (<i>invalid value</i>).
/// </remarks>
public class DeconzLightStateRequestDto
{
    /// <summary>Accende (<c>true</c>) o spegne (<c>false</c>).</summary>
    [JsonPropertyName("on")]
    public bool? On { get; set; }

    /// <summary>Luminosità, 0–255.</summary>
    [JsonPropertyName("bri")]
    public int? Brightness { get; set; }

    /// <summary>Tonalità del colore, 0–65535.</summary>
    [JsonPropertyName("hue")]
    public int? Hue { get; set; }

    /// <summary>Saturazione del colore, 0–255.</summary>
    [JsonPropertyName("sat")]
    public int? Saturation { get; set; }

    /// <summary>Temperatura di colore, in mired (es. 153 = 6500 K, 500 = 2000 K).</summary>
    [JsonPropertyName("ct")]
    public int? ColorTemperature { get; set; }

    /// <summary>Colore come coordinate CIE <c>[x, y]</c>, ciascuna tra 0 e 1.</summary>
    [JsonPropertyName("xy")]
    public double[]? Xy { get; set; }

    /// <summary>Segnalazione visiva: <c>none</c>, <c>select</c> (un lampeggio), <c>lselect</c> (lampeggi per 15 s).</summary>
    [JsonPropertyName("alert")]
    public string? Alert { get; set; }

    /// <summary>Effetto: <c>none</c> oppure <c>colorloop</c>.</summary>
    [JsonPropertyName("effect")]
    public string? Effect { get; set; }

    /// <summary>Durata della transizione, in decimi di secondo (es. 10 = 1 s).</summary>
    [JsonPropertyName("transitiontime")]
    public int? TransitionTime { get; set; }

    /// <summary>Tapparelle: porta alla percentuale di chiusura indicata, 0 (aperta) – 100 (chiusa).</summary>
    [JsonPropertyName("lift")]
    public int? Lift { get; set; }

    /// <summary>Tapparelle: apre del tutto (<c>true</c>) o chiude del tutto (<c>false</c>).</summary>
    [JsonPropertyName("open")]
    public bool? Open { get; set; }

    /// <summary>Tapparelle: ferma il movimento in corso (si invia solo <c>true</c>).</summary>
    [JsonPropertyName("stop")]
    public bool? Stop { get; set; }
}
