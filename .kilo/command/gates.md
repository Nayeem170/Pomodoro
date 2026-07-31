---
description: Run gate pipeline (format + build + test)
---
Run the deterministic gate pipeline.

Execute `python tools/gates/run.py` which runs:
1. gate2.format: `dotnet format Pomodoro.sln --verify-no-changes`
2. gate3.build: `dotnet build Pomodoro.sln --configuration Release`
3. gate4.test: `dotnet test Pomodoro.sln --configuration Release --verbosity normal`

Each gate writes a state file to `.gates/`. The pipeline halts on first failure.

Options:
- `--skip-format` to skip gate2.format
- `--ticket T-001 --base develop` for isolated worktree runs
- `--ticket T-001 --cleanup` to tear down a worktree

If a gate fails, read the `.gates/*.json` file for error details, fix, and re-run.

See `tools/gates/README.md` for the protocol and `docs/requirements/pipeline.md` for gate definitions.
