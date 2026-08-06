# RSD: Theme Change — Deep Forest

## Status

`Signed off — 2026-08-06`

All open questions resolved (see Sign-off section). Inventory gaps found during
codebase verification (2 missed `#6e7a8a` literals, expanded PiP scope, ErrorBanner
danger-red consistency) folded into the requirements below.

## Background

App currently ships a navy/indigo dark theme ("Midnight Slate" — `#16213e`/`#1a1a2e`/`#0f3460`,
red/green/blue session accents). Product owner reviewed 7 candidate palettes
(`wwwroot/palette-compare.html`) and picked **#3 Deep Forest**: a green-tinted dark theme.
Session accent colors were iterated live against the comparison page and locked to match
the Tarkeez logo's color story (red/green/silver) with one contrast fix.

This RSD also folds in a logo update: the logo's middle ring currently uses the raw brand
green (`#006A4E`), which — now that it renders in `About`/header on the new green-tinted
Deep Forest surfaces — has the exact same low-contrast-on-green problem the short-break
accent had. Both get the same fix, for the same reason.

## Locked Palette

Confirmed via `palette-compare.html` row 3, red/green-accent iteration, then contrast fix.

| Token | Old (Midnight Slate) | New (Deep Forest) |
|---|---|---|
| `--color-background-primary` | `#16213e` | `#0f1f1c` |
| `--color-background-secondary` | `#1a1a2e` | `#142e28` |
| `--color-background-tertiary` | `#0f3460` | `#1c3d35` |
| `--color-text-primary` | `#ffffff` | `#f0faf7` |
| `--color-text-secondary` | `#a0a0a0` | `#9db8ae` |
| `--color-text-tertiary` | `#6e7a8a` | `#5f7d73` |
| `--color-surface-light` | `#1f3460` | `#234d40` *(proposed — forest-toned equivalent, not yet shown on comparison page)* |
| `--pomodoro-color` (Focus accent) | `#e74c3c` | `#F42A41` *(logo red — unchanged from logo)* |
| `--short-break-color` | `#27ae60` | `#34D399` *(brightened emerald — NOT raw logo green `#006A4E`, see Contrast Fix)* |
| `--long-break-color` | `#3498db` | `#C0C0C0` *(logo silver, repurposed as 3rd accent)* |
| Border tokens (`--color-border-{primary,secondary,tertiary}`) | `rgba(45, 74, 111, 0.8/0.6/0.4)` (blue-tinted) | `rgba(40, 90, 75, 0.8/0.6/0.4)` *(proposed — forest-toned equivalent, not yet shown on comparison page)* |

## Contrast Fix — Why Short-Break Isn't the Raw Logo Green

The logo's middle ring is `#006A4E` (flag green). Deep Forest's background is also green
(`#142e28`). Both are low-luminance dark greens — placing `#006A4E` as a UI accent on this
background makes it nearly disappear (same failure mode as the earlier charcoal-outer-ring-
on-navy-bg bug). Fixed by using `#34D399` (higher-luminance emerald) for the **short-break
accent**, while the **logo's own middle ring** also moves to `#34D399` — same fix, same
reasoning, applied to both since the logo now visually represents the same "green" role
as the short-break accent.

Resulting locked logo palette: outer ring `#C0C0C0` (silver, unchanged) → middle ring
`#34D399` (was `#006A4E`) → center dot `#F42A41` (red, unchanged).

## Requirements (EARS)

### Theme tokens

- **REQ-1**: THE SYSTEM SHALL replace `:root` background/text/surface/border tokens in
  `app.css` (lines 3-23) with the Deep Forest values in the Locked Palette table.
- **REQ-2**: THE SYSTEM SHALL replace `--pomodoro-color`, `--short-break-color`,
  `--long-break-color` with `#F42A41`, `#34D399`, `#C0C0C0` respectively.
- **REQ-3**: THE SYSTEM SHALL update every literal hex duplicate of the old theme colors
  that does not derive from the `:root` custom properties (listed in Affected Files below)
  to the corresponding new value, since these do not update automatically via the
  `:root` change.
- **REQ-4**: THE SYSTEM SHALL update `manifest.webmanifest` `background_color` to
  `#142e28` (new `--color-background-secondary`) and `theme_color` to `#1c3d35` (Deep
  Forest tertiary).
- **REQ-5**: THE SYSTEM SHALL update `index.html` `<meta name="theme-color">` to `#1c3d35`.

### Logo/icon

- **REQ-6**: THE SYSTEM SHALL change the middle ring color from `#006A4E` to `#34D399` in
  `logo-tarkeez.svg`, `icon-192.svg`, `icon-512.svg`, the favicon inline data-URI SVG, and
  the loading-spinner inline SVG in `index.html` — all five currently share the same
  three-color mark and must stay in sync (established pattern from the color-fix work
  earlier in this project).
- **REQ-7**: THE SYSTEM SHALL leave the logo's outer ring (`#C0C0C0`) and center dot
  (`#F42A41`) unchanged.

### Danger / error red (consistency fix)

- **REQ-8**: THE SYSTEM SHALL update both danger-tint surfaces that reuse the old
  pomodoro-red `#e74c3c` for error/delete semantics to `#F42A41`-derived equivalents,
  keeping the same alpha channel:
  - `ErrorBanner.razor.css`: `#e74c3c1f` -> `#F42A411f`, `#e74c3c4d` -> `#F42A414d`,
    `#e08b7f` -> `#f08a92` (proportionally desaturated text variant)
  - `app.css:1339` (`.task-action-btn.delete`): `#e74c3c1f` -> `#F42A411f`
  - **Rationale**: these share the same hex and same "danger" semantic as the Focus
    accent; keeping them in sync avoids divergence when the accent shifts.

### Out of scope

- **REQ-9**: THE SYSTEM SHALL NOT change `SessionType.Pomodoro` enum, C# namespace, or any
  other identifier — this RSD is visual/theme-token only, consistent with the scope
  boundary established in `001-brand-rename-tarkeez`.

## Affected Files (inventory)

| File | Change |
|---|---|
| `src/Pomodoro.Web/wwwroot/css/app.css` | `:root` tokens (lines 3-23); literal duplicates at lines 345, 1242, 1266-1267, 1339, 1513, 1518, 1975, 2055, 2088, 2475 (4 "fake" `var()` fallbacks at 1975/2055/2088/2475 reference `--deep-blue`/`--color-surface-dark` which are never defined, so they always resolve to the fallback hex); **also 25 literal `#2d4a6f` border-color occurrences** (lines 346, 361, 620, 716, 782, 807, 820, 858, 996, 1352, 1394, 1405, 1449, 1503, 1562, 1588, 1636, 1666, 1714, 1756, 1909, 1939, 2056, 2089, 2675) — `#2d4a6f` = `rgb(45,74,111)` = same color as border tokens, hardcoded as solid hex, must change to `#285a4b` (`rgb(40,90,75)`) |
| `src/Pomodoro.Web/wwwroot/manifest.webmanifest` | `background_color` -> `#142e28`, `theme_color` -> `#1c3d35` |
| `src/Pomodoro.Web/wwwroot/index.html` | `<meta name="theme-color">` -> `#1c3d35`; favicon inline SVG middle ring `#006A4E` -> `#34D399`; loading-spinner inline SVG middle ring `#006A4E` -> `#34D399` |
| `src/Pomodoro.Web/wwwroot/logo-tarkeez.svg` | middle ring `#006A4E` -> `#34D399` |
| `src/Pomodoro.Web/wwwroot/icon-192.svg` | middle ring `#006A4E` -> `#34D399` |
| `src/Pomodoro.Web/wwwroot/icon-512.svg` | middle ring `#006A4E` -> `#34D399` |
| `src/Pomodoro.Web/wwwroot/js/pipTimer.js` | **Full Deep Forest palette** — all ~29 literal hexes, not just accents: background `#162032`->`#142e28` (6 occ), surface/ring `#1e2a40`->`#1c3d35` (2 occ), text `#8a97b8`->`#5f7d73`, `#e8edf8`->`#9db8ae` (3 occ), `#a0aec0`->`#9db8ae`, `#6e7a8a`->`#5f7d73` (2 occ), accent reds `#e74c3c`->`#F42A41` (5 occ), accent greens `#27ae60`->`#34D399` (4 occ), accent blues `#3498db`->`#C0C0C0` (5 occ), gradient rgba pairs (lines 170/173/176) |
| `src/Pomodoro.Web/Components/Shared/ErrorBanner.razor.css` | Danger red tracks new Focus red: `#e74c3c1f`->`#F42A411f`, `#e74c3c4d`->`#F42A414d`, `#e08b7f`->`#f08a92` |

## Testable Scenarios (Gate 1 mapping)

| REQ | Scenario | Test location |
|---|---|---|
| REQ-1/2 | `:root` computed styles resolve to new hex values | e2e visual/computed-style assertion, or new CSS custom-property unit check |
| REQ-3 | No remaining literal old-theme hex strings in `app.css` outside the changelog/history | grep-based CI check (optional) or manual diff review |
| REQ-4/5 | `manifest.webmanifest` and `index.html` meta tag values match | static content test |
| REQ-6/7 | All 5 logo surfaces render identical 3-color mark | visual check across header, favicon, PWA icon, loader, About page |
| REQ-8 | `ErrorBanner` and delete button render with new `#F42A41`-derived danger tint | existing `ErrorBanner` tests updated to new hex |
| PiP colors | Picture-in-Picture popup ring/tab colors match new session accents | `pip-timer.spec.ts` / `pip-window-content.spec.ts` (existing e2e specs) — need visual re-check since these are JS string literals, not CSS-asserted |

## Open Questions

All resolved during sign-off (2026-08-06):

1. **`--color-surface-light`** (`#234d40`) and **border tokens** (`rgba(40,90,75,*)`):
   accepted as computed values. Can be tweaked live post-implementation if needed.
2. **`theme_color`**: → `#1c3d35` (Deep Forest tertiary). Matches app background.
3. **`ErrorBanner.razor.css`** red tint: confirmed danger red should track the new
   Focus red. REQ-8 updated to include both ErrorBanner and `.task-action-btn.delete`.
4. **`pipTimer.js`**: full Deep Forest palette applied to all hexes, not just accents.

## Sign-off

- [x] Product owner (Nayeem) approves locked palette table
- [x] Product owner confirms `--color-surface-light` / border token proposed values
- [x] Product owner decides `theme_color` → `#1c3d35` (Open Question 2)
- [x] Product owner confirms `ErrorBanner` red tracks new Focus red (Open Question 3)
- [x] RSD signed off → proceed to TDS
