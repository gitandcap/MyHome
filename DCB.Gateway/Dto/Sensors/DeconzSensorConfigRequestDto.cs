using System.Text.Json.Serialization;

namespace DCB.Gateway.Dto.Sensors;

/// <summary>
/// Modifica della configurazione di un sensore: corpo di <c>PUT /sensors/{id}/config</c>.
/// </summary>
/// <remarks>
/// I campi della configurazione cambiano con il tipo di sensore, per questo il corpo è un dizionario (PA-15):
/// es. per una valvola TRV <c>new DeconzSensorConfigRequestDto().Set("heatsetpoint", 2100)</c> → <c>{"heatsetpoint":2100}</c>
/// (temperatura in centesimi di grado). Un DTO tipizzato per le TRV si aggiungerà con la loro mappatura in <c>MDZ</c>.
/// </remarks>
public sealed class DeconzSensorConfigRequestDto
{
    /// <summary>Campi da modificare, con il nome deCONZ; nel JSON compaiono al primo livello del corpo.</summary>
    [JsonExtensionData]
    public Dictionary<string, object> Fields { get; } = [];

    /// <summary>Imposta un campo della configurazione.</summary>
    /// <param name="name">Nome deCONZ del campo (es. <c>heatsetpoint</c>).</param>
    /// <param name="value">Valore, nelle unità di deCONZ.</param>
    /// <returns>Lo stesso oggetto, per impostare più campi di seguito.</returns>
    public DeconzSensorConfigRequestDto Set(string name, object value)
    {
        Fields[name] = value;
        return this;
    }
}
