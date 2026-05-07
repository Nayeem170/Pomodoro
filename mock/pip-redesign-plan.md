# PiP Window Redesign — Fix Plan

## Issue Reference
- Issue: #74 (PiP + Main Page Timer Redesign)
- Board Item: PVTI_lAHOAJBk4M4BWD1Dzgr_Ra4

---

## Problems

### 1. Session background colors not differentiated
All three sessions (Pomodoro, Short Break, Long Break) use the same gradient in PiP.
Main page uses per-session tinted gradients with colored borders.

### 2. No state-aware hints or footer text
PiP shows no hint text and no meaningful footer when the timer hasn't started.
Three distinct states need different UI:
- **Not started**: play-only controls, "Space to start" hint, footer shows session duration
- **Running**: pause+reset controls, footer shows "Ends at HH:MM"
- **Paused**: play+reset controls, "Space to resume" hint, footer shows "Paused · ends at HH:MM"

### 3. PiP window height needs adjustment
Currently hardcoded at 460px. With 220px ring + task pill + controls + hint text + footer, content is ~426px. Set to 440 for safe padding.

### 4. Tab and footer text too small / not visible enough
Tabs at 12px, footer at 11px with low-contrast colors (#4a5470, #8a97b8).

### 5. Counter font doesn't match main page
Main page uses `font-family: 'Courier New', monospace` on timer digits.
PiP uses default `system-ui` — no monospace font set.

### 6. Play button not centered when timer not started
PiP always renders reset+play+spacer. When not started, play should be alone and centered.

---

## Implementation Order

1. `mock/pip_redesign.html` — visual contract first
2. `src/Pomodoro.Web/Services/PipTimerService.cs` — compute `endsAt` when `IsStarted` (not just `IsRunning`)
3. `src/Pomodoro.Web/wwwroot/js/pipTimer.js` — CSS + JS changes
4. `tests/e2e/pages/pip-window-content.spec.ts` — test coverage

---

## Changes

### Step 1: Update mock — `mock/pip_redesign.html`

Build all three state variants **side by side** in a single file:

- **Not started**: play-only (centered), "Space to start" hint, footer "25 min session"
- **Running**: pause+reset, no hint, footer "Ends at 10:47 AM"
- **Paused**: play+reset, "Space to resume" hint, footer "Paused · ends at 10:47 AM"

Apply all CSS changes (per-session backgrounds, font sizes, font-family, hint styles). This serves as the visual diff target for Steps 2-4 and makes E2E test assertions easier to write.

---

### Step 2: Fix PipTimerService.cs — `src/Pomodoro.Web/Services/PipTimerService.cs`

#### Prerequisite — Compute `endsAt` for paused state (line 152-156)

Currently `endsAt` is only computed when `IsRunning`. When paused, it's `null` and the paused footer would be blank. Change condition to `IsStarted`:

```csharp
// Before
string? endsAt = null;
if (_timerService.IsRunning)
{
    endsAt = DateTime.Now.AddSeconds(remainingSeconds).ToString("h:mm tt");
}

// After
string? endsAt = null;
if (_timerService.IsStarted)
{
    endsAt = DateTime.Now.AddSeconds(remainingSeconds).ToString("h:mm tt");
}
```

---

### Step 3: Fix pipTimer.js — `src/Pomodoro.Web/wwwroot/js/pipTimer.js`

#### Fix #1 — Per-session background colors (CSS, lines 169-177)

Use `180deg` (vertical) instead of `160deg` (diagonal) — PiP window is tall and narrow, vertical gradient reads better.

```css
/* Before: all identical */
.pip-container.pomodoro-theme .pip-timer-area {
    background: linear-gradient(160deg, #1e2a50, #162032);
}
.pip-container.short-break-theme .pip-timer-area {
    background: linear-gradient(160deg, #1e2a50, #162032);
}
.pip-container.long-break-theme .pip-timer-area {
    background: linear-gradient(160deg, #1e2a50, #162032);
}

/* After: per-session tints, vertical gradient */
.pip-container.pomodoro-theme .pip-timer-area {
    background: linear-gradient(180deg, rgba(231,76,60,.12), #162032);
}
.pip-container.short-break-theme .pip-timer-area {
    background: linear-gradient(180deg, rgba(39,174,96,.12), #162032);
}
.pip-container.long-break-theme .pip-timer-area {
    background: linear-gradient(180deg, rgba(52,152,219,.12), #162032);
}
```

#### Fix #2 — Three-state controls, hints, and footer (generateTimerHTML)

**Controls logic:**

| State | Condition | Controls | Hint |
|---|---|---|---|
| Not started | `!isStarted && !isRunning` | Play only (centered) | "Space to start" |
| Running | `isRunning` | Pause + Reset | — |
| Paused | `isStarted && !isRunning` | Play + Reset | "Space to resume" |

```js
// Before: always reset+play+spacer
html += '<div class="pip-ctrl">';
html += '<button class="pip-reset" ...>';
html += '<button class="pip-play" ...>';
html += '<div style="width:36px"></div>';
html += '</div>';

// After: three-state conditional
if (!isStarted && !isRunning) {
    html += '<div class="pip-ctrl">';
    html += '<button class="pip-play ' + sessionClass + '" onclick="window.pipToggleTimer()" aria-label="Start">';
    html += playIcon;
    html += '</button>';
    html += '</div>';
    html += '<div class="pip-hint">Space to start</div>';
} else if (isRunning) {
    html += '<div class="pip-ctrl">';
    html += '<button class="pip-reset" onclick="window.pipResetTimer()" aria-label="Reset timer">';
    html += resetIcon;
    html += '</button>';
    html += '<button class="pip-play ' + sessionClass + '" onclick="window.pipToggleTimer()" aria-label="Pause">';
    html += pauseIcon;
    html += '</button>';
    html += '<div style="width:36px"></div>';
    html += '</div>';
} else {
    html += '<div class="pip-ctrl">';
    html += '<button class="pip-reset" onclick="window.pipResetTimer()" aria-label="Reset timer">';
    html += resetIcon;
    html += '</button>';
    html += '<button class="pip-play ' + sessionClass + '" onclick="window.pipToggleTimer()" aria-label="Resume">';
    html += playIcon;
    html += '</button>';
    html += '<div style="width:36px"></div>';
    html += '</div>';
    html += '<div class="pip-hint">Space to resume</div>';
}
```

**Footer logic:**

| State | Footer content |
|---|---|
| Not started | `"25 min session"` (or short/long break duration) |
| Running | `"Ends at"` + `endsAt` time |
| Paused | `"Paused · ends at"` + `endsAt` time |

```js
// Before: only shown when running
if (isRunning && endsAt) {
    html += '<div class="pip-footer">';
    html += '<span class="lbl">Ends at</span>';
    html += '<span class="val">' + endsAt + '</span>';
    html += '</div>';
}

// After: all three states
var durationMinutes = Math.round((state.totalDurationSeconds || 0) / 60);
html += '<div class="pip-footer">';
if (!isStarted && !isRunning) {
    var durationLabel = sessionType === 0 ? durationMinutes + ' min session'
        : durationMinutes + ' min break';
    html += '<span class="lbl">' + durationLabel + '</span>';
} else if (isRunning && endsAt) {
    html += '<span class="lbl">Ends at</span>';
    html += '<span class="val">' + endsAt + '</span>';
} else if (isStarted && !isRunning && endsAt) {
    html += '<span class="lbl">Paused · ends at</span>';
    html += '<span class="val">' + endsAt + '</span>';
}
html += '</div>';
```

Add CSS for hint:
```css
.pip-hint {
    font-size: 12px;
    color: #6e7a8a;
    text-align: center;
}
```

#### Fix #3 — Adjust PiP height (lines 34, 60)

Content breakdown: titlebar ~32px + tabs ~34px + timer area ~300px + footer ~40px + hint ~20px = ~426px.

Change `height: 460` to `height: 440` (14px safe padding).

#### Fix #4 — Increase tab and footer text (CSS)

```css
/* Before */
.pip-tab { font-size: 12px; }
.pip-footer span { font-size: 11px; }
.pip-footer .lbl { color: #4a5470; }
.pip-footer .val { color: #8a97b8; }
.ring-label { font-size: 11px; }

/* After */
.pip-tab { font-size: 13px; }
.pip-footer span { font-size: 12px; }
.pip-footer .lbl { color: #6e7a8a; }
.pip-footer .val { color: #a0aec0; }
.ring-label { font-size: 12px; }
```

Task pill — explicitly ensure truncation (already has `text-overflow: ellipsis` but add `max-width: 100%`):
```css
.pip-task-name {
    font-size: 13px;
    color: #e8edf8;
    flex: 1;
    max-width: 100%;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}
```

#### Fix #5 — Counter font match (CSS, line 200-207)

Use a cross-platform monospace stack. Main page uses `var(--font-mono)` which resolves to `'Courier New', monospace`. PiP can't access CSS variables so use the same stack directly:

```css
/* Before */
.ring-time {
    font-size: 42px;
    font-weight: 700;
    color: #e8edf8;
    letter-spacing: -1px;
    line-height: 1;
    font-variant-numeric: tabular-nums;
}

/* After: add monospace font stack */
.ring-time {
    font-size: 42px;
    font-weight: 700;
    color: #e8edf8;
    letter-spacing: -1px;
    line-height: 1;
    font-family: 'Courier New', 'Lucida Console', monospace;
    font-variant-numeric: tabular-nums;
}
```

#### Fix #6 — Play button centering (covered by Fix #2)
When not started, only play button renders inside `.pip-ctrl` which has `justify-content: center` — naturally centered.

---

### Step 4: Update E2E tests — `tests/e2e/pages/pip-window-content.spec.ts`

- Assert `.ring-time` has `font-family` containing `monospace`
- Assert `.pip-hint` text is "Space to start" when timer not started
- Assert `.pip-hint` text is "Space to resume" when timer paused
- Assert footer shows session duration when not started (e.g. "25 min session")
- Assert footer shows "Paused · ends at" when paused
- Verify per-session `.pip-timer-area` background differentiation (optional)

---

## Rollback

PiP uses JS-generated HTML (`generateTimerHTML`), not a Razor component. If something breaks it affects the live floating window. Before deploying:

1. Keep a copy of the current `generateTimerHTML` function in a comment block at the bottom of `pipTimer.js`
2. Tag the release commit for easy revert: `git tag pre-pip-fix-v2`

---

## Implementation Notes

### `endsAt` availability (verified)

`PipTimerService.GetTimerState()` (line 152-156) only computes `endsAt` when `IsRunning`:
```csharp
string? endsAt = null;
if (_timerService.IsRunning)
{
    endsAt = DateTime.Now.AddSeconds(remainingSeconds).ToString("h:mm tt");
}
```

This means `endsAt` is `null` when paused — the paused footer would be blank. **Fix**: change the condition to `IsStarted` so `endsAt` is computed for both running and paused states:
```csharp
if (_timerService.IsStarted)
{
    endsAt = DateTime.Now.AddSeconds(remainingSeconds).ToString("h:mm tt");
}
```

### `sessionType` serialisation type (verified)

`PipTimerService.GetTimerState()` explicitly casts to `int` (line 161):
```csharp
sessionType = (int)_timerService.CurrentSessionType,
```

Confirmed: serialises as integer `0/1/2`, not string. The `sessionType === 0` comparison in JS is safe — no runtime type mismatch risk.

### `sessionType` and duration variables (verified)

- `sessionType` — available as `state.sessionType` (line 318 of pipTimer.js) ✅
- `isStarted` — available as `state.isStarted` (line 332 area, needs to be extracted) ✅
- Per-session duration labels — `state.totalDurationSeconds` is available but not split by session name. Derive duration from `totalDurationSeconds / 60`:
```js
var durationMinutes = Math.round((state.totalDurationSeconds || 0) / 60);
var durationLabel = sessionType === 0 ? durationMinutes + ' min session'
    : sessionType === 1 ? durationMinutes + ' min break'
    : durationMinutes + ' min break';
```

### `isStarted` extraction (verified)

Currently `generateTimerHTML` only extracts `isRunning` (line 332). Need to add:
```js
var isStarted = state.isStarted || false;
```

---

## Verification

```bash
dotnet format Pomodoro.sln --verify-no-changes
dotnet test tests/Pomodoro.Web.Tests/Pomodoro.Web.Tests.csproj
npx playwright test tests/e2e/pages/pip-window-content.spec.ts
```
