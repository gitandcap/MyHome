using System.Text.Json.Serialization;

namespace DCB.Gateway.Dto.Shared;

/// <summary>
/// Errore restituito da deCONZ, nella forma <c>{ "error": { "type": …, "address": …, "description": … } }</c>.
/// </summary>
public sealed class DeconzErrorDto
{
    /// <summary>Codice dell'errore deCONZ (vedi <see cref="Core.DeconzErrorTypes"/>).</summary>
    [JsonPropertyName("type")]
    public int Type { get; set; }

    /// <summary>Risorsa a cui si riferisce l'errore (es. <c>/lights/3/state/bri</c>).</summary>
    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    /// <summary>Descrizione dell'errore, in inglese, come fornita da deCONZ.</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <inheritdoc />
    public override string ToString() => $"errore {Type} su {Address}: {Description}";
}
