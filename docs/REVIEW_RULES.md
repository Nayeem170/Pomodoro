# Review Rules

## Active rules

Every review iteration must check ALL active rules below.
Rules are added when a reviewer flags a recurring pattern.
Trigger is noted so the rule can be traced back.

### R01: ASCII-only source files
- No em dash (---), smart quotes, unicode arrows, emoji in .cs/.razor files
- Trigger: initial

### R02: No AI phrasing
- No "seamless", "effortless", "premium", "intuitive", "robust", "leverage", "streamline"
- Trigger: initial

### R03: No dead code or unused imports
- No commented-out code, no unused `using` statements, no placeholder comments
- Trigger: initial

### R04: File size limit
- Files < ~400 lines. Use `partial class` to split.
- Trigger: initial

### R05: Function size limit
- Functions/methods < ~60 lines.
- Trigger: initial

### R06: No fallbacks masking root causes
- If something can fail, handle the failure explicitly. No silent catches that hide bugs.
- Trigger: initial

### R07: Conventional commits
- `feat:`, `fix:`, `chore:`, `docs:`, `refactor:` prefix.
- Trigger: initial

### R08: Branch from develop
- All feature/fix branches must branch from `develop`, not `main`.
- Trigger: initial

### R09: Interface-based DI
- Every service must have an interface. DI registration uses the interface.
- Trigger: initial

### R10: Code-behind separation
- Razor files contain only markup. All logic in `.razor.cs` partial classes.
- Trigger: initial

### R11: Constants extracted
- No magic numbers or strings. All constants in `Constants` partial class.
- Trigger: initial

### R12: Test method naming
- `MethodUnderTest_Scenario_ExpectedBehavior` pattern.
- Trigger: initial

### R13: Arrange/Act/Assert comments
- Every test method has explicit `// Arrange`, `// Act`, `// Assert` comments.
- Trigger: initial

### R14: Test category trait
- Every test class has `[Trait("Category", "...")]`.
- Trigger: initial

### R15: Mock data must be realistic
- No placeholder values. Use plausible timer durations, task names, dates.
- Trigger: initial

### R16: No silent catch blocks
- Every catch must either rethrow, log, or update UI state. Empty catch is BLOCKING.
- Trigger: initial

### R17: Blazor component hierarchy
- Layout > Pages > Components. Services injected at appropriate level.
- Trigger: initial

### R18: SafeTaskRunner for fire-and-forget
- Use `SafeTaskRunner.RunAndForget()` instead of `_ = SomeAsync()` or `Task.Run()`.
- Trigger: initial

### R19: Property clamping on setters
- Numeric properties use `Math.Clamp` to enforce valid ranges.
- Trigger: initial

### R20: E2E selectors use data-testid or stable CSS
- No fragile text-based selectors in Playwright tests.
- Trigger: initial

### R21: FluentAssertions consistent with existing usage
- `Should().Be()`, `Should().Contain()`, `Should().ThrowAsync()` patterns.
- Trigger: initial

### R22: Verify all claims against source
- When reviewing a bug diagnosis, fix plan, or implementation, grep/read the actual source files to confirm every method, property, and code path referenced in the claim actually exists and behaves as described. If any claim references a nonexistent method or property, flag as BLOCKING and demand re-investigation from source.
- Trigger: fix-nested-tasks (prior session diagnosed GetTodayTasksAsync as root cause; method did not exist)

### R23: Edited copy on callbacks that feed change detection
- When a component edits a parameter-supplied model instance (e.g. a state-backed TaskItem) and then invokes an EventCallback, it must pass an edited copy (`WithUpdates`) rather than mutating the shared reference, whenever any downstream handler compares that instance's fields to detect a transition (list-change detection, dirty checks) or the instance backs rendered state. Mutating the shared instance aliases the "before" and "after" values: downstream comparisons see no change, and failed saves leak edits into live state.
- Trigger: fix-task-edit-properties / #163 (panel mutated state-backed Task.GoogleListId before Index read oldListId; MoveTaskToListAsync could never fire)

### R24: Subtree-wide API precondition guards
- When a guard rejects an operation based on a node property (e.g. Google's recurring-task cross-list move restriction), it must evaluate the property across the node AND all descendants before the first mutation, not just the requested node. Mid-cascade rejection after partial mutation splits hierarchy invariants across boundary states.
- Trigger: fix-task-edit-properties / #163 (guard checked only the root; a recurring descendant would have moved the root before the child move was rejected)

### R25: Never build local date strings from ToISOString()
- `DateTime.UtcNow.ToString("o")` / `toISOString()` and slicing produce UTC dates; in UTC+X timezones before 06:00 (for +06:00) they yield yesterday's date. Any date string used for "today" boundaries, seeding, or display must come from local-time components (`DateTime.Today`, `new Date()` + getFullYear/getMonth/getDate), never from a UTC-serialized string.
- Trigger: recurring-task timezone wave / fix-recurring-task-uncheck-timezone + e2e seeding (burned twice in +06:00; e2e specs use a localDate() helper built from local components)

### R26: Flex children with overflow scroll need flex-shrink guard
- A flex item with `overflow-y: auto` (or `overflow: auto`) inside a `flex-direction: column` container has its automatic minimum size zeroed by spec; default `flex-shrink: 1` then crushes the box to a sliver whenever the container overflows. Any such element must set `flex-shrink: 0` (or an explicit min-height). Check every CSS diff that adds overflow to a flex child.
- Trigger: fix-demote-picker-visibility / #175 (.demote-picker crushed to 16px with 401px content in overflowing task lists; initial diagnosis blamed fold clipping, S7 acceptance exposed the crush)

### R27: Patch coverage gate is CI-only - cover changed lines, not just the total
- The local gate (tools/gates/run.py gate5) enforces overall 99.5% line coverage; Codecov additionally enforces patch coverage (95% of changed lines, 0.5% drift) only at PR time. A branch can pass locally and fail codecov/patch. New/changed source lines must ship with tests in the same PR; do not rely on the local total as proof.
- Trigger: fix-availability-auto-tab / #171 (codecov/patch failed after local gate green; +100 test lines needed at f5960ab)

## Adding rules

When a reviewer identifies a recurring issue:
1. Add rule here with next number (R22, R23, etc.)
2. Note the trigger: what code/feedback caused the rule
3. Rules are never removed, only superseded by a higher-numbered rule

## Global conduct rules

D01-D03 (developer conduct) and V01 (reviewer conduct) are defined in the global orchestrator at `~/.config/kilo/agent/agentic-orchestrator.md`. They apply unconditionally to every project.
