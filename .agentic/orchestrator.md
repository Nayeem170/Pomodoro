# Pomodoro Agentic Pipeline - Project Overrides

Overrides for the global orchestrator (`~/.config/kilo/agent/agentic-orchestrator.md`).
Only project-specific deviations belong here. Everything else is global.

## Setup

Read these files first:
- `.agentic/config.md` - project configuration, structure, and architecture
- `docs/CODE_CONVENTIONS.md` - full coding standards and patterns
- `docs/REVIEW_RULES.md` - project code rules (R01-R21)

## Agent Manager integration

When worktree mode is selected:
- Use `agent_manager` tool with `mode: "worktree"` for the developer (writes code)
- Use `agent_manager` tool with `mode: "local"` for the reviewer (reads code, no worktree needed)
- Developer prompt: load `.agentic/developer.md` and prepend it to the first message
- Reviewer prompt: load `.agentic/reviewer.md` and prepend it to the first message
- Use `action: "prompt"` to send messages to sessions
- Use `agent_manager(action: "list")` to check session status
- Use `action: "stop"` to end sessions
- Model overrides: developer gets `developer_id` + `developer_variant` from config.md, reviewer gets `reviewer_id` + `reviewer_variant` from config.md

## Pipeline steps

Follow the global pipeline steps (S0-S8). Project-specific notes:

- **Step 2.5**: Uses Pencil (pen.dev) for UI mocks. Read `pen_cli` from `.agentic/config.md`.
- **Step 7**: Deploy instruction from `.agentic/config.md` deploy field.

## Relaying

The global orchestrator enforces relay completeness checks (feedback history, 1:1 item mapping).

## Git operations

- Branch from `develop` (not `main`)
