"""Analisi statica (euristica) dei membri VB.NET non referenziati nei file compilati."""
import re, os, sys, json, collections
import xml.etree.ElementTree as ET

ROOT = sys.argv[1]
PROJ = os.path.join(ROOT, 'Performance2021', 'Performance2021.vbproj')
PDIR = os.path.dirname(PROJ)

ns = {'m': 'http://schemas.microsoft.com/developer/msbuild/2003'}
tree = ET.parse(PROJ)
vbfiles, xamlfiles = [], []
for tag in ('Compile', 'Page', 'ApplicationDefinition'):
    for el in tree.getroot().iter('{%s}%s' % (ns['m'], tag)):
        inc = el.get('Include')
        (vbfiles if inc.lower().endswith('.vb') else xamlfiles).append(inc)

def read(p):
    with open(os.path.join(PDIR, p), encoding='utf-8-sig', errors='replace') as f:
        return f.read().splitlines()

STR_RE = re.compile(r'"(?:[^"]|"")*"')

def split_code(line):
    """Restituisce (codice senza stringhe e commenti, lista stringhe)."""
    code, strings, i, n = [], [], 0, len(line)
    while i < n:
        c = line[i]
        if c == '"':
            j = i + 1
            while j < n:
                if line[j] == '"':
                    if j + 1 < n and line[j + 1] == '"':
                        j += 2; continue
                    break
                j += 1
            strings.append(line[i + 1:j])
            code.append('""')
            i = j + 1
            continue
        if c == "'" or c == '‘' or c == '’':
            break
        code.append(c); i += 1
    s = ''.join(code)
    if re.match(r'^\s*REM\b', s, re.I):
        return '', strings
    return s, strings

MODS = r'(?:Public|Private|Friend|Protected|Shared|Overrides|Overridable|Overloads|ReadOnly|WriteOnly|Async|Shadows|MustOverride|NotOverridable|Default|Iterator|Partial|Static|Widening|Narrowing)'
DECL_RE = re.compile(r'^\s*(?:<[^>]*>\s*)*((?:' + MODS + r'\s+)*)(Sub|Function|Property|Operator)\s+\[?(\w+)\]?', re.I)
LAMBDA_ML_RE = re.compile(r'\b(?:Async\s+)?(Sub|Function)\s*\((?:[^()]|\([^()]*\))*\)\s*(?:As\s+[\w.]+(?:\([^()]*\))?)?\s*$', re.I)
END_RE = re.compile(r'^\s*End\s+(Sub|Function|Property|Operator|Get|Set|Interface|Class|Module|Structure)\b', re.I)
TYPE_RE = re.compile(r'^\s*(?:<[^>]*>\s*)*(?:(?:Public|Private|Friend|Protected|Partial|MustInherit|NotInheritable|Shadows|Shared)\s+)*(Class|Module|Structure|Interface)\s+(\w+)', re.I)
TOKEN_RE = re.compile(r'[A-Za-z_]\w*')

decls = []            # membri dichiarati
refs = collections.defaultdict(list)   # nome -> [(file, riga)]
string_tokens = collections.Counter()
file_code = {}

for f in vbfiles:
    raw = read(f)
    code, strs = [], []
    for ln in raw:
        c, s = split_code(ln)
        code.append(c); strs.append(s)
        for st in s:
            for t in TOKEN_RE.findall(st):
                string_tokens[t.lower()] += 1
    file_code[f] = code
    # dichiarazioni con gestione blocchi
    stack = []   # (tipo, decl_index or None)
    type_stack = []
    i = 0
    n = len(code)
    while i < n:
        line = code[i]
        # unione righe di continuazione (esplicita " _" o parentesi aperte)
        full, j = line, i
        while (full.rstrip().endswith(' _') or full.count('(') > full.count(')')) and j + 1 < n and j - i < 30:
            j += 1
            full = full.rstrip().rstrip('_') + ' ' + code[j].strip()
        m_type = TYPE_RE.match(full)
        if m_type:
            type_stack.append((m_type.group(1).lower(), m_type.group(2)))
            i = j + 1; continue
        m_end = END_RE.match(line)
        if m_end:
            kind = m_end.group(1).lower()
            if kind in ('interface', 'class', 'module', 'structure'):
                if type_stack: type_stack.pop()
            elif kind in ('sub', 'function', 'property', 'operator'):
                # chiude il blocco piu' interno dello stesso tipo
                for k in range(len(stack) - 1, -1, -1):
                    if stack[k][0] == kind:
                        _, di = stack.pop(k)
                        if di is not None:
                            decls[di]['end'] = i + 1
                        break
            i += 1; continue
        m = DECL_RE.match(full)
        in_interface = bool(type_stack) and type_stack[-1][0] == 'interface'
        if m and not re.match(r'^\s*(Declare|Delegate|Event|Custom\s+Event)\b', full, re.I):
            mods = m.group(1).lower().split()
            kind = m.group(2).lower()
            name = m.group(3)
            d = dict(file=f, name=name, kind=kind, mods=mods, start=i + 1, end=j + 1,
                     cls=type_stack[-1][1] if type_stack else '',
                     handles=bool(re.search(r'\bHandles\b', full, re.I)),
                     implements=bool(re.search(r'\bImplements\b', full, re.I)),
                     text=full.strip())
            has_body = not in_interface and 'mustoverride' not in mods
            if kind == 'property' and has_body:
                # auto-property se la riga successiva di codice non e' Get/Set
                k = j + 1
                while k < n and code[k].strip() == '':
                    k += 1
                has_body = k < n and re.match(r'^\s*(?:(?:Public|Private|Friend|Protected)\s+)?(Get|Set)\b', code[k], re.I) is not None
            decls.append(d)
            if has_body:
                stack.append((kind, len(decls) - 1))
            i = j + 1; continue
        if LAMBDA_ML_RE.search(line):
            stack.append((LAMBDA_ML_RE.search(line).group(1).lower(), None))
        i += 1

# indice riga -> membro contenitore (il piu' interno)
enclosing = {}
for di, d in enumerate(decls):
    for ln in range(d['start'], d['end'] + 1):
        key = (d['file'], ln)
        prev = enclosing.get(key)
        if prev is None or (decls[prev]['end'] - decls[prev]['start']) > (d['end'] - d['start']):
            enclosing[key] = di

decl_lines = collections.defaultdict(set)
for d in decls:
    decl_lines[(d['file'], d['start'])].add(d['name'].lower())

for f, code in file_code.items():
    for idx, line in enumerate(code, 1):
        for t in TOKEN_RE.findall(line):
            tl = t.lower()
            if tl in decl_lines.get((f, idx), ()) and re.search(r'\b(Sub|Function|Property|Operator)\s+\[?' + re.escape(t) + r'\b', line, re.I):
                continue   # il nome sulla propria riga di dichiarazione
            refs[tl].append((f, idx))

# XAML
xaml_event, xaml_other = collections.Counter(), collections.Counter()
for f in xamlfiles:
    txt = '\n'.join(read(f))
    for a, v in re.findall(r'\b([\w.:]+)\s*=\s*"([A-Za-z_]\w*)"', txt):
        xaml_event[v.lower()] += 1
    for t in TOKEN_RE.findall(txt):
        xaml_other[t.lower()] += 1

EVENT_SIG = re.compile(r'\(\s*\w+\s+As\s+(Object|System\.Object)\s*,\s*\w+\s+As\s+[\w.]*EventArgs\s*\)', re.I)
SKIP = {'new', 'finalize', 'main'}

out = []
for di, d in enumerate(decls):
    nl = d['name'].lower()
    if nl in SKIP or d['kind'] == 'operator':
        continue
    external = [r for r in refs.get(nl, []) if enclosing.get(r) != di and not (d['start'] <= r[1] <= d['end'] and r[0] == d['file'])]
    excl = None
    if d['handles']: excl = 'gestore evento (Handles)'
    elif d['implements']: excl = 'Implements (interfaccia/converter)'
    elif 'overrides' in d['mods']: excl = 'Overrides'
    elif 'mustoverride' in d['mods']: excl = 'MustOverride'
    out.append(dict(d, idx=di, nrefs=len(external),
                    ref_members=sorted({enclosing.get(r, -1) for r in external}),
                    xaml_event=xaml_event.get(nl, 0), xaml_any=xaml_other.get(nl, 0),
                    in_strings=string_tokens.get(nl, 0), excl=excl,
                    event_sig=bool(EVENT_SIG.search(d['text']))))

# punto fisso: membri referenziati solo da membri gia' inutilizzati
dead = {o['idx'] for o in out if o['nrefs'] == 0 and not o['excl'] and o['xaml_any'] == 0}
changed = True
while changed:
    changed = False
    for o in out:
        if o['idx'] in dead or o['excl'] or o['xaml_any'] or o['nrefs'] == 0:
            continue
        if all(m in dead for m in o['ref_members'] if m != -1) and -1 not in o['ref_members']:
            dead.add(o['idx']); o['secondo_livello'] = True; changed = True

res = dict(vbfiles=vbfiles, xamlfiles=xamlfiles, total_decls=len(decls),
           members=[{k: v for k, v in o.items() if k not in ('ref_members',)} | {'dead': o['idx'] in dead,
                    'callers': [decls[m]['name'] for m in o['ref_members'] if m != -1]} for o in out])
json.dump(res, open(sys.argv[2], 'w', encoding='utf-8'), ensure_ascii=False, indent=1)
print('file vb:', len(vbfiles), 'xaml:', len(xamlfiles), 'dichiarazioni:', len(decls), 'morti:', len(dead))
