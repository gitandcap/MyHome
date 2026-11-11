using System.Text.Json;
using DCB.Gateway.Dto.Shared;

namespace DCB.Gateway.Core;

/// <summary>
/// Legge gli errori deCONZ da una risposta JSON.
/// </summary>
/// <remarks>
/// deCONZ segnala gli errori con un array di oggetti <c>{ "error": { … } }</c>, a volte con codice HTTP 2xx:
/// per questo il corpo va controllato anche quando il codice è corretto (stessa logica degli script Postman <c>DCB-PMN</c>).
/// </remarks>
internal static class DeconzErrorReader
{
    /// <summary>Restituisce gli elementi della risposta se questa è un array "a esito", altrimenti un elenco vuoto.</summary>
    /// <param name="root">Radice del JSON di risposta.</param>
    public static IReadOnlyList<DeconzApiResponseDto> ReadResponseItems(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return root.Deserialize<List<DeconzApiResponseDto>>(DeconzJson.Options) ?? [];
    }

    /// <summary>Restituisce gli errori contenuti nella risposta (vuoto se non ce ne sono).</summary>
    /// <param name="root">Radice del JSON di risposta.</param>
    public static IReadOnlyList<DeconzErrorDto> ReadErrors(JsonElement root) =>
        ReadResponseItems(root)
            .Where(item => item.Error is not null)
            .Select(item => item.Error!)
            .ToList();
}
