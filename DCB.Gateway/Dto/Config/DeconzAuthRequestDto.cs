using System.Text.Json.Serialization;

namespace DCB.Gateway.Dto.Config;

/// <summary>Corpo di <c>POST /api</c>, la richiesta di una nuova API key.</summary>
public sealed class DeconzAuthRequestDto
{
    /// <summary>Nome con cui il client si registra su deCONZ (es. <c>MyHome#DCB</c>).</summary>
    [JsonPropertyName("devicetype")]
    public string DeviceType { get; set; } = string.Empty;
}
