using System.Text.Json;
using DCB.Gateway.Dto.Shared;

namespace DCB.Gateway.Core;

/// <summary>
/// Esito di un comando (<c>PUT</c>) verso deCONZ.
/// </summary>
/// <remarks>
/// deCONZ risponde con un elemento per ogni campo del comando: <c>{"success": {"/lights/3/state/on": true}}</c>
/// oppure <c>{"error": {…}}</c>. Un comando può quindi essere applicato <b>solo in parte</b>
/// (es. <c>on</c> applicato, <c>bri</c> rifiutato perché fuori intervallo): in quel caso il risultato riporta
/// entrambi. Se <b>tutti</b> i campi sono rifiutati viene invece sollevata <see cref="DeconzApiException"/>.
/// </remarks>
public sealed class DeconzCommandResult
{
    /// <summary>Crea il risultato.</summary>
    /// <param name="applied">Campi applicati: indirizzo deCONZ → valore.</param>
    /// <param name="errors">Campi rifiutati.</param>
    public DeconzCommandResult(IReadOnlyDictionary<string, JsonElement> applied, IReadOnlyList<DeconzErrorDto> errors)
    {
        Applied = applied;
        Errors = errors;
    }

    /// <summary>Campi applicati: indirizzo deCONZ (es. <c>/lights/3/state/on</c>) → valore confermato.</summary>
    public IReadOnlyDictionary<string, JsonElement> Applied { get; }

    /// <summary>Campi rifiutati da deCONZ.</summary>
    public IReadOnlyList<DeconzErrorDto> Errors { get; }

    /// <summary>Tutto il comando è stato applicato.</summary>
    public bool IsFullySuccessful => Errors.Count == 0;

    /// <summary>Costruisce il risultato dagli elementi della risposta deCONZ.</summary>
    /// <param name="items">Elementi <c>success</c> / <c>error</c> della risposta.</param>
    internal static DeconzCommandResult FromResponseItems(IReadOnlyList<DeconzApiResponseDto> items)
    {
        var applied = new Dictionary<string, JsonElement>();
        foreach (JsonElement success in items.Where(item => item.Success is not null).Select(item => item.Success!.Value))
        {
            AddAppliedFields(applied, success);
        }

        List<DeconzErrorDto> errors = items.Where(item => item.Error is not null).Select(item => item.Error!).ToList();
        return new DeconzCommandResult(applied, errors);
    }

    /// <summary>Ogni <c>success</c> è un oggetto <c>{ "indirizzo": valore }</c>; altri formati vengono ignorati.</summary>
    private static void AddAppliedFields(Dictionary<string, JsonElement> applied, JsonElement success)
    {
        if (success.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (JsonProperty field in success.EnumerateObject())
        {
            applied[field.Name] = field.Value.Clone();
        }
    }
}
