# Test Plan: <task-name>

## Requirement coverage

| # | Requirement point | Test scenario | Type | Edge case? |
|---|------------------|---------------|------|------------|
<!-- | 1 | User can start timer | StartPomodoroAsync_WithTaskId_SetsTaskId | unit (bUnit) | no | -->
<!-- | 2 | Timer displays correctly | timer_shows_remaining_time | e2e (Playwright) | no | -->

## Edge cases

| # | Scenario | What it tests | Input | Expected |
|---|----------|--------------|-------|----------|

## Error paths

| # | Scenario | What it tests | Expected |
|---|----------|--------------|----------|

## WASM-specific risks
<!-- IndexedDB failures, service worker caching, browser API unavailability, JS interop errors -->

| # | Scenario | What it tests | Expected |
|---|----------|--------------|----------|

## Test file placement

| Scenario | Test file | Type | Follows pattern of |
|----------|-----------|------|-------------------|
<!-- | StartPomodoroAsync_WithTaskId_SetsTaskId | tests/Pomodoro.Web.Tests/Services/TimerServiceTests/TimerServiceTests.StartAsync.cs | unit (bUnit) | existing TimerServiceTests | -->
<!-- | timer_shows_remaining_time | tests/e2e/timer.spec.ts | e2e (Playwright) | existing timer e2e tests | -->

## Mock data notes

<!-- What mock data is needed? Must be realistic, not placeholder. -->
<!-- Use plausible timer durations (25:00, 5:00), task names, dates, session counts. -->
