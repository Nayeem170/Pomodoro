---
description: Build a knowledge graph from the codebase
---

Build a queryable knowledge graph from the codebase using Graphify.

Run the full graphify pipeline on the project root:

```powershell
python -c "import graphify" 2>$null
if ($LASTEXITCODE -ne 0) { pip install graphifyy -q 2>&1 | Select-Object -Last 3 }
python -c "import sys; open('.graphify_python', 'w').write(sys.executable)"
```

Then run graphify on `src/` (source code only, skipping node_modules/bin/obj):

```powershell
$python = Get-Content .graphify_python
& $python -m graphify src/ --no-viz
```

If the user provided `$ARGUMENTS`:
- If it contains a path like `src/` or `tests/`, use that path instead
- If it contains `--deep`, add `--mode deep`
- If it contains `--update`, add `--update`
- If it contains `--watch`, add `--watch`
- If it contains `query` followed by a question, run: `graphify query "<question>"`
- If it contains `path` followed by two node names, run: `graphify path "Node1" "Node2"`

After graphify completes, read `graphify-out/GRAPH_REPORT.md` and summarize:
1. Total nodes, edges, and communities found
2. Top 5 god nodes (highest connectivity)
3. Any surprise edges (unexpected cross-domain connections)
4. Suggested questions worth investigating

Do NOT send raw source code to any LLM. Graphify only sends semantic descriptions.
