# Punti aperti di coding

**Aggiornato al:** 11/11/2026
**Stato:** documento permanente (nome fisso, senza progressivo), aggiornato ogni volta che un punto di coding si apre,
cambia o si chiude

---

Elenco dei punti **operativi** della fase di coding (attività da fare, verifiche, materiale da preparare).
I punti che richiedono una decisione o un'analisi sono in [`PuntiApertiAnalisi.md`](../analisi/PuntiApertiAnalisi.md).

**Identificativi**: la numerazione `PA-nn` è **unica per i due documenti** (un ID non si riutilizza e non si duplica):
per un nuovo punto si usa il numero successivo al più alto presente in uno dei due.
Quando un punto è risolto passa nella sezione 2, con data ed esito.

## 1. Punti aperti

| ID | Punto | Origine | Progetto / sottomodulo | Blocca | Aperto il |
| --- | --- | --- | --- | --- | --- |
| PA-02 | **Richiesta WebSocket in Postman** verso `{{deconzWsUrl}}`, da creare a mano e salvare nella raccolta | sessione 09/11/2026 | `DCB-PMN` *(già previsto)* | – | 09/11/2026 |
| PA-18 | **Risposta di deCONZ a una chiave non valida su `/config`**: verificare con Postman (`GET /api/<chiave inventata>/config`) se deCONZ restituisce l'errore 1 o la configurazione ridotta con HTTP 200. Oggi `ConfigClient` gestisce entrambi i casi (manca `websocketport` → chiave non valida); se l'errore 1 è garantito, il controllo si può togliere | sessione 11/11/2026 | `DCB-GTW-CFG`, `DCB-GTW-AUT` *(già previsti)* | – | 11/11/2026 |

## 2. Punti chiusi

| ID | Punto | Chiuso il | Esito |
| --- | --- | --- | --- |
| PA-01 | Esempi JSON reali di deCONZ per i test dei DTO | 11/11/2026 | Salvato da Postman lo "Stato completo" (deCONZ 2.33.2, 11 luci, 6 sensori, 1 gruppo); copia senza `whitelist` in `DCB.Gateway.Tests\TestData\Deconz\full-state.json`. Gli elenchi di lights/sensors/groups sono sottoinsiemi dello stato completo: non servono file separati |
