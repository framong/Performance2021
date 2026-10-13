# Guida alla lettura del PDF Feed Forward

Il PDF si genera dal tab **Feed Forward** con **Print PDF**. Questa guida descrive la versione a pagine verticali, quella
con la casella **"Pdf: selected bands together"** attiva (scheda Layout). È la versione usata normalmente.

Le scritte nel PDF sono in inglese; qui sono riportate tra virgolette come appaiono.

## L'idea in una frase

Per ogni fascia di vento, il report dice **a quale valore puntare con la prestazione** (per esempio VmgPerc Inertial al
120%) e **quale valore tenere per ogni regolazione** (forestay, jib tack, trim…) per arrivarci. Lo ricava dall'andamento
delle regolazioni man mano che la barca va più forte. In più dice **quali regolazioni contano davvero** per la
prestazione.

## Com'è fatto il PDF

1. **Prima pagina, riepilogo** ("… FeedForward Summary"): due tabelle, il feed forward e la sensitività.
2. **Una pagina per ogni canale** ("<canale> … FeedForward Optimization"): grafico, poi tabella.
3. **Su ogni pagina** c'è un riquadro con le note in fondo, una cornice, il numero di pagina in basso a sinistra e
   "SailingPerformer FeedForward Analysis" in basso a destra.
4. **Le note in fondo** spiegano sempre le sigle della pagina e riportano sorgente dei dati, filtro, fasce, bin e
   modello usati.

## Prima pagina: tabella del feed forward

Una colonna per fascia di vento ("Tws bands"), una riga per canale.

| Riga | Cosa dice |
|---|---|
| "FF target VmgInrPerc_m" | La prestazione a cui puntare in quella fascia, per esempio `122.1 (100%)`. Tra parentesi c'è il riferimento di confronto (impostabile, "Reference X") |
| "Samples" | Quanti campioni ci sono nella fascia |
| "FF <canale>" | Il valore suggerito per quella regolazione, per esempio `H 5.6 (3.9)` |

**Come si legge una cella, per esempio `H 5.6 (3.9)`:**

- **La lettera** indica l'affidabilità del suggerimento: **H** alta, **M** media, **L** bassa. Misura quanto è preciso
  il valore suggerito rispetto alla normale oscillazione del canale; dipende anche da quanti campioni e quanti bin ci
  sono.
- **Il primo numero** è il valore suggerito al target.
- **Tra parentesi** c'è il valore medio osservato al riferimento (di solito Vmg% 100), cioè quello che si fa oggi
  quando la barca va al 100%. Il confronto tra i due dice **di quanto cambiare** la regolazione.
- **"@ x"** compare solo se quel canale ha un target diverso dagli altri (succede solo con gli obiettivi
  Maximize/Minimize Y).

**Colori di sfondo**, cioè dove intervenire:

| Sfondo | Significato |
|---|---|
| **Blu** | Intervenire qui: guadagno potenziale di almeno 1 punto di Vmg% con affidabilità H o M |
| **Blu chiaro** | Da considerare: guadagno di almeno 0,5 punti |
| **Arancione** | Promettente ma con affidabilità L: servono più dati prima di fidarsi |
| nessuno | Guadagno trascurabile, niente da fare |

Il **guadagno potenziale** è (valore suggerito − valore al 100%) × effetto della regolazione sulla Vmg%, preso dalla
sensitività. Se la regolazione non conta per la prestazione, il guadagno è zero anche se il valore suggerito è molto
diverso. È un criterio di priorità, non una previsione garantita.

## Prima pagina: tabella della sensitività

"Sensitivity of VmgInrPerc_m to each channel": quanto ogni regolazione influisce sulla prestazione. I canali sono in
ordine dal più al meno influente.

**Come si legge una cella, per esempio `+0.4 T: +5.1 %`:**

Quando il forestay è più alto di 0,4 T, cioè della sua **variazione tipica** (quanto lo si regola di solito), la Vmg% è
in media più alta di 5,1 punti. Il segno dice il verso: **+** vuol dire più regolazione, più prestazione; **−** il
contrario.

**Colori:**

| Aspetto | Significato |
|---|---|
| **Verde acqua pieno** | La regolazione conta molto (spiega almeno il 15% delle variazioni di Vmg% nella fascia) |
| **Verde acqua chiaro** | Conta (almeno il 5%) |
| nessuno | Conta poco |
| **Testo grigio** | Praticamente non conta (meno del 2%) |
| **Bordo marcato** | È la regolazione più importante di quella fascia |

**Cautele:**

- È una **relazione osservata, non una causa**. Esempio: il forestay viene mollato quando la barca è lenta per dare
  potenza. Il legame c'è, ma è l'equipaggio che reagisce alla velocità, non il forestay che fa andare la barca. Usare un
  filtro sui tratti stabili rende il dato più affidabile.
- **TwaAbs e Aws** risultano sempre importanti in parte per costruzione: la Vmg si calcola dalla TWA, e una barca più
  veloce ha più vento apparente. Vanno letti con questa consapevolezza, oppure tolti dalla lista dei canali.

## Pagine dei canali

**Il grafico.**

- **Asse X** (orizzontale, nome in grande sullo sfondo in basso): la prestazione, per esempio VmgPerc Inertial.
- **Asse Y** (nome in grande a destra): il canale della pagina.
- **Un colore per ogni fascia di vento**; la legenda è sotto il grafico.
- **Puntini piccoli**: i singoli campioni.
- **Cerchi pieni**: la media del canale in ogni bin di prestazione (bin "densi", usati nel calcolo). **Cerchi vuoti**:
  bin "radi", con pochi campioni, mostrati ma non usati (si possono nascondere con "Show sparse bins").
- **Linea continua**: l'andamento stimato dentro i dati. **Linea tratteggiata**: l'estrapolazione oltre l'ultimo bin
  denso.
- **Punto giallo**: il valore suggerito al target.
- **Bande verticali grigie** sullo sfondo: i bin di prestazione.

**La tabella sotto il grafico** ha una colonna per fascia.

| Riga | Cosa dice |
|---|---|
| "FF target VmgInrPerc_m" | Prestazione a cui puntare; `*` = estrapolato oltre i dati; tra parentesi il riferimento (100%) |
| "FF expected <canale>" | Valore suggerito; tra parentesi il valore medio al 100%. Sfondo colorato come nel riepilogo |
| "FF reliability (H/M/L)" | Affidabilità del suggerimento |
| "Samples" | Campioni della fascia |
| "Sensitivity of VmgInrPerc_m" | Come nel riepilogo, per esempio `+0.4 T: +5.1 %`, con gli stessi colori e bordo |
| "Best <canale> (gain)" | Il valore della regolazione nel gruppo di campioni con la prestazione migliore e quanto guadagna rispetto alla media. "edge" = è il valore più basso o più alto usato: il meglio potrebbe stare oltre |
| "Potential gain of VmgInrPerc_m" | Il guadagno potenziale in punti di Vmg% (quello che decide i colori blu e arancione) |
| Righe "VmgInrPerc_m 120 - 122" ecc. | La media del canale in ogni bin di prestazione, dal più alto al più basso. **Grigio** = bin rado, non usato nel calcolo |

## Come usarlo in pratica

1. **Riepilogo, tabella della sensitività:** guardare i bordi marcati e il verde acqua per capire quali regolazioni
   contano, fascia per fascia.
2. **Riepilogo, tabella del feed forward:** cercare le celle **blu**, cioè le regolazioni da cambiare. Il confronto tra
   valore suggerito e valore tra parentesi dice di quanto.
3. **Celle arancioni:** sono idee promettenti da verificare con più dati, prima di cambiare le abitudini.
4. **Pagina del canale:** controllare sul grafico che la linea segua bene i cerchi pieni e che il punto giallo non sia
   troppo lontano dai dati.

## Impostazioni che cambiano il risultato

Tutte nel tab Feed Forward, salvate con il report.

| Impostazione | Dove | Effetto |
|---|---|---|
| Goal "Push X to its maximum" | Analysis, Model | Il target è la prestazione più alta raggiunta (ultimo bin denso più l'estrapolazione) |
| "Bins beyond the data" | Analysis, Model | Di quanti bin il target può andare oltre i dati |
| "Reference X" | Analysis, Model | Il riferimento tra parentesi (default 100) |
| "Min samples", "Min share %" | Analysis, X bins | Quando un bin è denso e viene usato (default: almeno 20 campioni e almeno il 2% della fascia) |
| Soglie dei colori | Layout, Highlights | Quando una cella diventa blu, blu chiaro, arancione o verde acqua |
| Metodo dei canali Inertial | File Utilities del primo tab | Velocità di equilibrio (default, usa l'inerzia della barca in "Boat Inertia") oppure il vecchio metodo a finestre |
