# Definition of Done: <task-name>

## Build
- [ ] `dotnet build Pomodoro.sln -c Release` succeeds with no warnings from new code
- [ ] `dotnet format Pomodoro.sln --verify-no-changes` passes
- [ ] `dotnet test tests/Pomodoro.Web.Tests -c Release` passes (ALL unit tests, not just new ones)
- [ ] `npx playwright test` passes (ALL e2e tests)

## Convention compliance
- [ ] ASCII-only in all source files (no em dash, smart quotes, unicode)
- [ ] No comments unless explaining non-obvious WHY
- [ ] No fallback implementations, no silent catch blocks
- [ ] File-scoped namespaces used
- [ ] Interface-based DI (new services have interfaces, scoped registration)
- [ ] SafeTaskRunner for fire-and-forget
- [ ] Constants extracted to Constants partial class
- [ ] Conventional commit messages
- [ ] Branch named type/<task-name>

## Requirement coverage
<!-- One item per requirement point. Map to specific code. -->
- [ ] <requirement point 1> - implemented in <file:method>
- [ ] <requirement point 2> - implemented in <file:method>

## Test coverage
- [ ] New feature has bUnit unit tests with [Trait("Category", "...")]
- [ ] New feature has Playwright e2e tests where UI is affected
- [ ] Edge cases covered (WASM-specific, timer/scheduling, IndexedDB, concurrency)
- [ ] Tests use MethodUnderTest_Scenario_ExpectedBehavior naming
- [ ] Tests have explicit // Arrange, // Act, // Assert comments
- [ ] Tests follow existing patterns in the codebase

## Scope
- [ ] No changes outside plan.md scope
- [ ] No unrelated refactoring
