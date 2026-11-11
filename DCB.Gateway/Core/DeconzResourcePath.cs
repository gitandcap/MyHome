namespace DCB.Gateway.Core;

/// <summary>
/// Composizione dei percorsi delle risorse deCONZ (es. <c>lights/3/state</c>).
/// </summary>
internal static class DeconzResourcePath
{
    /// <summary>
    /// Unisce i segmenti con <c>/</c>, codificando ognuno: un id che contiene caratteri come <c>/</c> o <c>?</c>
    /// non può così indirizzare una risorsa diversa da quella voluta.
    /// </summary>
    /// <param name="segments">Segmenti del percorso, nell'ordine.</param>
    public static string Combine(params string[] segments)
    {
        foreach (string segment in segments)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(segment, nameof(segments));
        }

        return string.Join('/', segments.Select(Uri.EscapeDataString));
    }
}
