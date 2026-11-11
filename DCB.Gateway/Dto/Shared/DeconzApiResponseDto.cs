using System.Text.Json;
using System.Text.Json.Serialization;

namespace DCB.Gateway.Dto.Shared;

/// <summary>
/// Elemento delle risposte "a esito" di deCONZ, che sono array di oggetti <c>success</c> o <c>error</c>
/// (es. <c>[{ "success": { "username": "…" } }]</c> oppure <c>[{ "error": { … } }]</c>).
/// </summary>
/// <remarks>
/// Il contenuto di <c>success</c> cambia con la richiesta, per questo resta un <see cref="JsonElement"/>
/// da interpretare da chi ha fatto la chiamata.
/// </remarks>
public sealed class DeconzApiResponseDto
{
    /// <summary>Esito positivo; <c>null</c> se l'elemento è un errore.</summary>
    [JsonPropertyName("success")]
    public JsonElement? Success { get; set; }

    /// <summary>Errore; <c>null</c> se l'elemento è un esito positivo.</summary>
    [JsonPropertyName("error")]
    public DeconzErrorDto? Error { get; set; }
}
