using System.Text.Json;
using System.Text.Json.Serialization;

namespace DCB.Gateway.Core;

/// <summary>
/// Opzioni JSON comuni a tutte le chiamate verso deCONZ.
/// </summary>
internal static class DeconzJson
{
    /// <summary>
    /// Nessuna politica di nomi (i DTO dichiarano i nomi deCONZ con <c>[JsonPropertyName]</c>) e campi <c>null</c>
    /// esclusi in scrittura: deCONZ applica ogni campo presente nel corpo, quindi si inviano solo quelli valorizzati (PA-15).
    /// </summary>
    public static JsonSerializerOptions Options { get; } = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
}
