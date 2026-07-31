# Pipeline Configuration

Resolved gate configuration for the Pomodoro project. Single-repo layout.

## Gate Definitions

### Gate 1 - Spec Validation (LLM)

| Field | Value |
|-------|-------|
| **Name** | `gate1.spec` |
| **Input** | `docs/requirements/NNN-task-slug/rsd.md` |
| **Validation** | Every AC uses EARS notation and maps to >= 1 testable scenario |
| **Schema** | `tools/gates/schemas/gate1.spec.schema.json` (Phase 3) |
| **Status** | Not yet implemented |

### Gate 2 - Format

| Field | Value |
|-------|-------|
| **Name** | `gate2.format` |
| **Command** | `dotnet format Pomodoro.sln --verify-no-changes` |
| **CI Reference** | `ci.yml` lint step (line 29) |
| **Status** | Implemented (`tools/gates/run.py`) |

### Gate 3 - Build

| Field | Value |
|-------|-------|
| **Name** | `gate3.build` |
| **Command** | `dotnet build Pomodoro.sln --configuration Release` |
| **CI Reference** | `ci.yml` build job (line 30) |
| **Status** | Implemented (`tools/gates/run.py`) |

### Gate 4 - Test

| Field | Value |
|-------|-------|
| **Name** | `gate4.test` |
| **Command** | `dotnet test Pomodoro.sln --configuration Release --verbosity normal` |
| **CI Reference** | `ci.yml` unit-test job (lines 58-59) |
| **Status** | Implemented (`tools/gates/run.py`) |

### Gate 5 - TDD (Playwright RED)

| Field | Value |
|-------|-------|
| **Name** | `gate5.tdd` |
| **Input** | Playwright test files tagged with `@T-NNN` |
| **Validation** | Tests compile and fail (RED) before implementation exists |
| **Command** | `cd tests/e2e && npx playwright test --grep @T-NNN` |
| **Status** | Not yet implemented |

## CI Reference Commands

These are the exact commands from `ci.yml` that the gates mirror:

```bash
dotnet format Pomodoro.sln --verify-no-changes          # gate2
dotnet build Pomodoro.sln --configuration Release        # gate3
dotnet test Pomodoro.sln --configuration Release        # gate4 (CI adds --collect and --results-directory)
dotnet publish src/Pomodoro.Web -c Release -o bin/e2e-publish  # e2e build
npx playwright test --shard=N/8 --reporter=list         # e2e test (8 shards)
```

Note: CI uses `--no-restore` on build/test (separate restore step). The gate script
runs restore implicitly via dotnet CLI.

## Issue Tracker

GitHub Issues - `Nayeem170/Pomodoro`

## Workspace

Single repo at `D:\Programming\.Net\Pomodoro`. Source: `src/Pomodoro.Web/`.

## Phase Roadmap

| Phase | What | Status |
|-------|------|--------|
| 1 | Gate orchestrator (format + build + test), state files | Done |
| 2 | Worktree spawn/teardown | Done |
| 3 | Gate 1 - PRD parser + schema validation | Planned |
| 4 | Gate 5 - TDD RED check (Playwright) | Planned |
| 5 | Self-heal loop + retry caps | Planned |
| 6 | Cross-harness review agent | Planned |
