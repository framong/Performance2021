import json, sys, re, collections, datetime
r = json.load(open(sys.argv[1], encoding='utf-8'))
OUT = sys.argv[2]
M = r['members']

# --- classi serializzate in JSON (radici + chiusura sui tipi delle proprieta' pubbliche)
ROOTS = {'clsTgt', 'clsProfile2021', 'clsPeriods2021', 'ParquetChannel', 'clsBenchmarks', 'clsChannel2020',
         'clsParquetFinderStorage', 'clsPeriodsJson', 'clsQualitySettings'}
classes = {m['cls'] for m in M if m['cls']}
graph = collections.defaultdict(set)
for m in M:
    if m['kind'] == 'property' and 'private' not in m['mods'] and m['cls']:
        mm = re.search(r'\bAs\s+(?:New\s+)?(.*)$', m['text'], re.I)
        if mm:
            for t in re.findall(r'[A-Za-z_]\w*', mm.group(1)):
                if t in classes:
                    graph[m['cls']].add(t)
ser, todo = set(), [c for c in ROOTS if c in classes]
while todo:
    c = todo.pop()
    if c in ser: continue
    ser.add(c); todo.extend(graph[c])

names = {i: m for i, m in enumerate(M)}
rows = []
for m in M:
    if not m['dead']:
        continue
    L2 = m.get('secondo_livello', False)
    is_prop = m['kind'] == 'property'
    private = 'private' in m['mods']
    motivi = []
    if L2:
        callers = sorted(set(m['callers']))
        motivi.append('Referenziata solo da codice a sua volta inutilizzato: ' + ', '.join(callers[:4]) + (' …' if len(callers) > 4 else ''))
    else:
        motivi.append('Nessun riferimento nel codice compilato né nei .xaml')
    if m['event_sig']:
        motivi.append('firma da gestore di evento ma senza Handles, AddHandler o collegamento XAML')
    conf = 'alta'
    if is_prop and not private:
        if m['cls'] in ser:
            conf = 'bassa'
            motivi.append(f'proprietà pubblica di {m["cls"]}, classe serializzata in JSON: può servire a salvare/caricare i file')
        else:
            conf = 'media'
            motivi.append('proprietà pubblica: potrebbe essere letta via reflection (binding impostato a runtime, DataGrid AutoGenerateColumns)')
    if L2 and conf == 'alta':
        conf = 'media'
    if m['in_strings']:
        conf = 'bassa'
        motivi.append('il nome compare in una stringa letterale (possibile CallByName, Binding("…") o OnPropertyChanged("…"))')
    rows.append(dict(file=m['file'].replace('\\', '/'), nome=(m['cls'] + '.' if m['cls'] else '') + m['name'],
                     tipo=m['kind'].capitalize(), acc=' '.join(x for x in m['mods'] if x in ('public', 'private', 'friend', 'protected', 'shared')) or 'public',
                     righe=f"{m['start']}–{m['end']}", n=m['end'] - m['start'] + 1, conf=conf, motivo='; '.join(motivi), start=m['start']))

order = {'alta': 0, 'media': 1, 'bassa': 2}
rows.sort(key=lambda x: (order[x['conf']], x['file'].lower(), x['start']))
cnt = collections.Counter(x['conf'] for x in rows)
lines_by = collections.Counter()
for x in rows: lines_by[x['conf']] += x['n']

excl = collections.Counter(m['excl'] for m in M if m['excl'] and m['nrefs'] == 0)
xaml_only = [m for m in M if m['nrefs'] == 0 and not m['excl'] and m['xaml_any']]
xaml_ev = [m for m in xaml_only if m['xaml_event']]
xaml_bind = [m for m in xaml_only if not m['xaml_event']]

def esc(s): return s.replace('|', '\\|')

o = []
o.append('# Analisi del codice inutilizzato\n')
o.append(f'Generato il {datetime.date.today():%d/%m/%Y}. Analisi statica **euristica**, nessun file è stato modificato.\n')
o.append('## Metodo\n')
o.append(f'- Considerati solo i file compilati del `.vbproj`: {len(r["vbfiles"])} file `.vb` e {len(r["xamlfiles"])} file `.xaml`. '
         'I file non inclusi (`ClassiDesuete.vb`, `ctrl_*.xaml`, …) non contano né come dichiarazioni né come riferimenti.')
o.append(f'- Dichiarazioni esaminate: {r["total_decls"]} tra Sub, Function e Property.')
o.append('- Commenti e stringhe esclusi dalla ricerca dei riferimenti. Il confronto è per **nome**, senza distinzione di maiuscole (come VB): '
         'se un altro membro con lo stesso nome è usato, il candidato viene considerato usato. L\'analisi quindi tende a *non* segnalare, mai a segnalare troppo.')
o.append('- Le chiamate ricorsive (riferimenti dentro il corpo del membro stesso) non contano come uso.')
o.append('- **Secondo livello**: membri referenziati solo da altri membri inutilizzati (calcolo iterativo fino a punto fisso).')
o.append('- Esclusi dalla tabella e riepilogati sotto: gestori con `Handles`, `Overrides`, `Implements` (interfacce e converter), `Sub New`, '
         'e qualsiasi membro il cui nome compare in un `.xaml` (event handler `Click=`, `Loaded=`, … oppure binding).')
o.append('- Proprietà pubbliche delle classi serializzate con Newtonsoft (`clsKillerSeriale.Save/LoadConfigurationGeneric`) sono a confidenza bassa: '
         'Json.NET le legge e scrive via reflection. Classi considerate serializzate (radici + tipi delle loro proprietà): '
         + ', '.join(f'`{c}`' for c in sorted(ser)) + '.')
o.append('- `CallByName` è usato solo in `CurrentCalibration_dHdg.vb` sulle proprietà `bs`, `sog`, `cse`, `cog`; nessun uso di `GetMethod`/`GetProperty`/`InvokeMember`. '
         'Con `Option Strict Off` il late binding è possibile, ma il nome compare comunque nel codice e viene quindi contato.\n')
o.append('### Legenda confidenza\n')
o.append('- **alta**: Sub/Function (o proprietà privata) senza alcun riferimento: rimovibile con rischio minimo.')
o.append('- **media**: usata solo da altro codice inutilizzato (va rimossa insieme ai chiamanti), oppure proprietà pubblica non serializzata.')
o.append('- **bassa**: il nome compare in stringhe letterali, oppure proprietà pubblica di una classe salvata in JSON.\n')
o.append('## Riepilogo\n')
o.append('| Confidenza | Membri | Righe (circa) |')
o.append('|---|---:|---:|')
for c in ('alta', 'media', 'bassa'):
    o.append(f'| {c} | {cnt[c]} | {lines_by[c]} |')
o.append(f'| **totale** | **{len(rows)}** | **{sum(lines_by.values())}** |\n')
per_file = collections.Counter(x['file'] for x in rows)
o.append('File con più candidati: ' + ', '.join(f'`{f}` ({n})' for f, n in per_file.most_common(8)) + '.\n')

o.append('## Candidati\n')
o.append('| File | Nome | Tipo | Accesso | Righe | Confidenza | Motivo |')
o.append('|---|---|---|---|---|---|---|')
for x in rows:
    o.append(f"| `{x['file']}` | `{esc(x['nome'])}` | {x['tipo']} | {x['acc']} | {x['righe']} | {x['conf']} | {esc(x['motivo'])} |")

o.append('\n## Segnalati a parte (esclusi dalla tabella)\n')
o.append(f'- Gestori con `Handles` senza chiamate dirette: {excl.get("gestore evento (Handles)", 0)} (normali: li invoca WPF).')
o.append(f'- `Overrides`: {excl.get("Overrides", 0)}; `Implements`: {excl.get("Implements (interfaccia/converter)", 0)} (tra cui `Convert`/`ConvertBack` dei converter).')
o.append(f'- Membri senza riferimenti nel codice ma con il nome presente in un `.xaml`: {len(xaml_only)}, di cui {len(xaml_ev)} come valore di attributo semplice '
         f'(tipicamente event handler `Click="…"`) e {len(xaml_bind)} in binding o altri contesti. '
         'Il confronto sui `.xaml` è per nome: nomi generici (es. `Name`, `Width`) possono coincidere con proprietà dei controlli e nascondere membri realmente inutilizzati.\n')
orphans = [x for x in rows if 'firma da gestore' in x['motivo']]
if orphans:
    o.append(f'### Gestori di evento orfani ({len(orphans)})\n')
    o.append('Hanno la firma `(sender As Object, e As …EventArgs)` ma non sono collegati né con `Handles`, né con `AddHandler`, né in XAML: '
             'probabilmente sono rimasti dopo la rimozione di un controllo. Sono già nella tabella principale.\n')
    for x in orphans:
        o.append(f"- `{x['file']}` `{x['nome']}` (righe {x['righe']})")
    o.append('')
o.append('## Limiti\n')
o.append('- Parser basato su espressioni regolari, non sul compilatore: blocchi scritti in modo insolito possono avere righe di fine imprecise.')
o.append('- Un membro usato solo da un gestore di evento orfano viene considerato usato, perché i gestori sono esclusi dal calcolo del secondo livello; '
         'lo stesso vale per il codice raggiungibile solo da membri esclusi (XAML, Overrides).')
o.append('- Non sono analizzate classi intere inutilizzate né campi e variabili.')
o.append('- Prima di rimuovere un candidato: cercarlo nella soluzione, compilare, e per le proprietà verificare i file JSON salvati.')
open(OUT, 'w', encoding='utf-8').write('\n'.join(o) + '\n')
print(dict(cnt), dict(lines_by), 'xaml_only', len(xaml_only), len(xaml_ev), 'orfani', len(orphans), 'ser', sorted(ser))
print('top file', per_file.most_common(8))
