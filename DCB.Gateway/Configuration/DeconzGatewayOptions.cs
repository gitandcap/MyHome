namespace DCB.Gateway.Configuration;

/// <summary>
/// Parametri di connessione a deCONZ usati da tutto il Gateway Connector (<c>DCB-GTW</c>).
/// </summary>
/// <remarks>
/// La libreria non sa da quale file arrivano i valori (ANA-06 §4.6): in MyHome sono letti dalla sezione
/// <see cref="SectionName"/> della configurazione (<c>appsettings.json</c> + <c>cfg.json</c>, ANA-07 §3).
/// I valori predefiniti qui sotto corrispondono a deCONZ in locale sulla stessa macchina.
/// </remarks>
public sealed class DeconzGatewayOptions
{
    /// <summary>Nome della sezione di configurazione che contiene questi parametri.</summary>
    public const string SectionName = "Deconz";

    /// <summary>
    /// Indirizzo (nome o IP) della macchina su cui gira deCONZ.
    /// Predefinito: <c>127.0.0.1</c>. Obbligatorio, non vuoto.
    /// </summary>
    public string Host { get; set; } = "127.0.0.1";

    /// <summary>
    /// Porta HTTP delle REST API di deCONZ.
    /// Predefinito: <c>80</c> (deCONZ 2.33.2 in locale). Valori ammessi: 1–65535.
    /// </summary>
    public int HttpPort { get; set; } = 80;

    /// <summary>
    /// Porta del WebSocket degli eventi. Facoltativa.
    /// Se vuota (<c>null</c>, predefinito) si usa il campo <c>websocketport</c> letto da <c>/config</c>;
    /// se valorizzata ha la precedenza (serve con deCONZ in container e porta pubblicata diversa, ANA-07 §3.1).
    /// Valori ammessi: 1–65535 oppure <c>null</c>.
    /// </summary>
    public int? WsPort { get; set; }

    /// <summary>
    /// Nome con cui il DCB si registra su deCONZ quando chiede una nuova API key (campo <c>devicetype</c> di <c>POST /api</c>).
    /// Predefinito: <c>MyHome#DCB</c>. Obbligatorio, non vuoto.
    /// </summary>
    public string DeviceType { get; set; } = "MyHome#DCB";

    /// <summary>
    /// Tempo massimo di attesa di una risposta REST da deCONZ, in secondi; oltre, deCONZ è considerato non raggiungibile.
    /// Predefinito: <c>10</c>. Valori ammessi: maggiore di 0.
    /// </summary>
    public int RequestTimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// Intervallo tra i tentativi di <c>POST /api</c> durante l'attesa dello sblocco del gateway, in secondi.
    /// Predefinito: <c>5</c> (lo sblocco da Phoscon dura circa 60 s). Valori ammessi: maggiore di 0.
    /// </summary>
    public int UnlockRetrySeconds { get; set; } = 5;
}
