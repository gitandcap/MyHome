using System.Text.Json.Serialization;

namespace DCB.Gateway.Dto.Groups;

/// <summary>Stato riassuntivo di un gruppo (<c>state</c> di <see cref="DeconzGroupDto"/>).</summary>
public sealed class DeconzGroupStateDto
{
    /// <summary>Tutte le luci del gruppo sono accese.</summary>
    [JsonPropertyName("all_on")]
    public bool? AllOn { get; set; }

    /// <summary>Almeno una luce del gruppo è accesa.</summary>
    [JsonPropertyName("any_on")]
    public bool? AnyOn { get; set; }
}
