namespace DCB.Gateway.Tests.TestData;

/// <summary>
/// Accesso alle risposte reali di deCONZ salvate in <c>TestData\Deconz</c> (copiate nella cartella di output).
/// </summary>
internal static class DeconzSampleFiles
{
    /// <summary>
    /// <c>GET /api/{key}</c> su deCONZ 2.33.2 dell'impianto reale (11/11/2026), senza <c>whitelist</c>:
    /// 11 luci (tapparelle Vimar, prese), 6 sensori (pulsanti IKEA, Daylight, CLIP), 1 gruppo.
    /// </summary>
    public const string FullState = "full-state.json";

    public static string Read(string fileName) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestData", "Deconz", fileName));
}
