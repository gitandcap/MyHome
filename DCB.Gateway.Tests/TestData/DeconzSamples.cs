namespace DCB.Gateway.Tests.TestData;

/// <summary>
/// Risposte deCONZ di esempio, ricavate dalla documentazione e dalle prove con Postman (deCONZ 2.33.2).
/// </summary>
/// <remarks>
/// Saranno affiancate dagli esempi reali salvati da Postman (PA-01), senza la <c>whitelist</c>.
/// </remarks>
internal static class DeconzSamples
{
    public const string ApiKey = "1234567890ABCDEF";

    public const string NewApiKey = "FEDCBA0987654321";

    /// <summary><c>GET /api/{key}/config</c> con chiave valida (ridotta; la whitelist serve a verificare che non sia mappata).</summary>
    public const string FullConfig = """
        {
          "name": "Phoscon-GW",
          "bridgeid": "00212EFFFF051A5C",
          "apiversion": "1.16.0",
          "swversion": "2.33.2",
          "websocketport": 80,
          "websocketport_wss": 443,
          "ipaddress": "192.168.1.10",
          "zigbeechannel": 15,
          "whitelist": { "1234567890ABCDEF": { "name": "MyHome#DCB" } }
        }
        """;

    /// <summary><c>GET /api/config</c> senza chiave: mancano websocketport, ipaddress, whitelist…</summary>
    public const string PublicConfig = """
        {
          "name": "Phoscon-GW",
          "bridgeid": "00212EFFFF051A5C",
          "apiversion": "1.16.0",
          "swversion": "2.33.2"
        }
        """;

    public const string UnauthorizedUserError = """
        [{ "error": { "type": 1, "address": "/", "description": "unauthorized user" } }]
        """;

    public const string LinkButtonNotPressedError = """
        [{ "error": { "type": 101, "address": "/", "description": "link button not pressed" } }]
        """;

    public const string NewApiKeySuccess = """
        [{ "success": { "username": "FEDCBA0987654321" } }]
        """;

    public const string MixedResult = """
        [
          { "success": { "/lights/3/state/on": true } },
          { "error": { "type": 7, "address": "/lights/3/state/bri", "description": "invalid value, 300, for parameter, bri" } }
        ]
        """;
}
