# Gate Pipeline

Deterministic quality gates matching CI exactly. No LLM, no cost, instant feedback.

## Quick start

```bash
python tools/gates/run.py
```

Runs format check, build, and test sequentially. Writes state files to `.gates/`. Halts on first failure.

## Gates

| Gate | Name | Command | CI Reference |
|------|------|---------|-------------|
| 2 | `gate2.format` | `dotnet format Pomodoro.sln --verify-no-changes` | `ci.yml` lint step |
| 3 | `gate3.build` | `dotnet build Pomododoro.sln --configuration Release` | `ci.yml` build job |
| 4 | `gate4.test` | `dotnet test Pomodoro.sln --configuration Release --verbosity normal` | `ci.yml` unit-test job |

## State file protocol

Each gate writes `.gates/<gate-name>.json`. Schema in `tools/gates/schema.json`.

```json
{
  "gate": "gate3.build",
  "description": "Build solution in Release configuration",
  "command": "dotnet build Pomodoro.sln --configuration Release",
  "status": "passed",
  "startedAt": "2026-07-31T01:00:00+00:00",
  "completedAt": "2026-07-31T01:00:12+00:00",
  "exitCode": 0,
  "stdout": null,
  "stderr": null
}
```

Status values: `running` -> `passed` | `failed` | `timeout` | `skipped`

Agents read these files to decide what happened without re-running commands.

## Options

```bash
python tools/gates/run.py --skip-format    # skip gate2.format
python tools/gates/run.py --workdir /path  # run against arbitrary directory
```

## Worktree isolation

```bash
python tools/gates/run.py --ticket T-001 --base develop
```

Spawns an isolated worktree at `../.worktrees-pomodoro/ticket-T-001/` on branch
`ticket/T-001` (from `develop`), runs all gates, and:

- **On pass**: keeps the worktree for implementation work
- **On fail**: tears down the worktree and deletes the branch

Cleanup: `python tools/gates/run.py --ticket T-001 --cleanup`

Worktrees live outside the repo at `../.worktrees-pomodoro/` because git forbids nesting
a worktree inside its own repository.

## Why this exists

CI already runs these commands on push. This script lets agents run the same gates
locally and read deterministic state files instead of parsing terminal output.

## Roadmap

- Phase 1: Gate orchestrator with format + build + test
- Phase 2: Worktree lifecycle (spawn/teardown)
- Phase 3: Gate 1 - spec validation (LLM agent + JSON Schema)
- Phase 4: Gate 2 - TDD validation (Playwright RED check)
- Phase 5: Self-heal loop with retry caps
- Phase 6: Cross-harness review agent
