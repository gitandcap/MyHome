using System.Text.Json.Serialization;

namespace DCB.Gateway.Dto.Config;

/// <summary>
/// Contenuto di <c>success</c> nella risposta a <c>POST /api</c>: <c>[{ "success": { "username": "…" } }]</c>.
/// </summary>
public sealed class DeconzAuthResponseDto
{
    /// <summary>La nuova API key (deCONZ la chiama <c>username</c>).</summary>
    [JsonPropertyName("username")]
    public string UserName { get; set; } = string.Empty;
}
