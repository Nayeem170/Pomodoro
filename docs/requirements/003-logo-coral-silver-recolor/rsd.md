# RSD: Logo Recolor — Silver / Coral / Red, Applied to Timer Accents

## Status

`Signed off — 2026-08-06`

Single-color scope: short-break emerald -> coral only. Silver and red
unchanged everywhere. No swap-ordering risk, no `.schedule-badge` exclusion
needed.

## Background

Deep Forest theme (`002-theme-deep-forest`) locked the logo/accent triad to
Silver (outer) / Emerald `#34D399` (middle) / Red `#F42A41` (dot). Product owner ran a
live ring-color comparison (`wwwroot/logo-outer-ring-compare.html`) and considered two
placements for a new Coral `#FCA5A5`:

- Coral outer / Silver middle — silver buffers coral from the red dot (recommended, more
  visually distinct at small size)
- **Silver outer / Coral middle** (chosen) — coral sits directly next to the red dot

Product owner picked the second option despite the contrast tradeoff. Net effect: the
**outer ring is unchanged from `002`** (stays Silver `#C0C0C0`) — only the **middle ring**
changes, Emerald -> Coral. This is simpler than originally scoped: no color is reused
across two different ring positions, so there's no swap-ordering risk to manage.

## Locked Colors

| Ring position | Old (002) | New (this RSD) |
|---|---|---|
| Outer ring | Silver `#C0C0C0` | Silver `#C0C0C0` *(unchanged)* |
| Middle ring | Emerald `#34D399` | Coral `#FCA5A5` |
| Center dot | Red `#F42A41` | Red `#F42A41` *(unchanged)* |

### Timer/session accent mapping

Same positional mapping `002` used (dot = Focus, middle = Short break, outer = Long
break):

| Token | Old (002) | New (this RSD) |
|---|---|---|
| `--pomodoro-color` (Focus) | `#F42A41` | `#F42A41` *(unchanged)* |
| `--short-break-color` | `#34D399` (emerald) | `#FCA5A5` (coral) |
| `--long-break-color` | `#C0C0C0` (silver) | `#C0C0C0` *(unchanged)* |

Net scope: **one color changes** (short-break, emerald -> coral). Focus and long-break
are untouched by this RSD.

## Note: This Reverses the 002 Contrast Fix, and That's Fine

`002-theme-deep-forest` moved short-break off the raw logo green `#006A4E` to a brighter
emerald `#34D399` specifically to fix a dark-green-on-dark-green-bg contrast clash.
Short-break is now becoming coral, not a shade of green at all, so that reasoning no
longer applies — flagged so `002`'s "why" isn't mistaken for a constraint still binding
this change.

## Requirements (EARS)

- **REQ-1**: THE SYSTEM SHALL change the logo's middle ring from `#34D399` to `#FCA5A5`
  in every surface that renders the mark: `logo-tarkeez.svg`, `icon-192.svg`,
  `icon-512.svg`, favicon inline SVG (`index.html`), loading-spinner inline SVG
  (`index.html`) — same five-surface sync rule established in `001-brand-rename-tarkeez`
  REQ-6.
- **REQ-2**: THE SYSTEM SHALL leave the outer ring (`#C0C0C0`) and center dot (`#F42A41`)
  unchanged everywhere.
- **REQ-3**: THE SYSTEM SHALL update `--short-break-color` from `#34D399` to `#FCA5A5`
  in `app.css` `:root`. `--pomodoro-color` and `--long-break-color` are unchanged.
- **REQ-4**: THE SYSTEM SHALL update every literal duplicate of the old short-break color
  (`#34D399` hex and its `rgba(52, 211, 153, *)` decimal form) that does not derive from
  the `:root` custom property — same category `002` already identified as not
  auto-cascading: `pipTimer.js` hardcoded short-break hexes, and `app.css` rgba
  duplicates.
- **REQ-5**: THE SYSTEM SHALL NOT touch any `#C0C0C0` / `rgba(192, 192, 192, *)`
  occurrence — outer ring and long-break are unchanged, so unlike the earlier draft of
  this RSD there is no silver-related edit at all, and therefore no need to special-case
  `.schedule-badge` (`app.css:1873`) — it was never in scope once the mapping stopped
  reusing silver.
- **REQ-6**: THE SYSTEM SHALL NOT change background/text/surface tokens or the
  `ErrorBanner`/danger-red treatment — this RSD is a single accent recolor, not a re-run
  of the full `002` theme change.

## Affected Files (inventory)

| File | Change |
|---|---|
| `src/Pomodoro.Web/wwwroot/logo-tarkeez.svg` | middle ring `#34D399`->`#FCA5A5` |
| `src/Pomodoro.Web/wwwroot/icon-192.svg` | same |
| `src/Pomodoro.Web/wwwroot/icon-512.svg` | same |
| `src/Pomodoro.Web/wwwroot/index.html` | favicon inline SVG + loading-spinner inline SVG, same middle-ring swap |
| `src/Pomodoro.Web/wwwroot/css/app.css` | `:root` — `--short-break-color` only; `rgba(52, 211, 153, *)` duplicates -> `rgba(252, 165, 165, *)` |
| `src/Pomodoro.Web/wwwroot/js/pipTimer.js` | short-break hexes (`#34D399`-derived, from `002`'s pipTimer pass) -> `#FCA5A5` |

## Testable Scenarios (Gate 1 mapping)

| REQ | Scenario | Test location |
|---|---|---|
| REQ-1/2 | All 5 logo surfaces render Silver/Coral/Red mark, outer+dot byte-identical to `002` | visual check across header, favicon, PWA icon, loader, About page |
| REQ-3 | `:root` short-break resolves to `#FCA5A5`; Focus/long-break unchanged | e2e/computed-style assertion |
| REQ-4 | No remaining literal `#34D399` or `rgba(52,211,153,*)` outside changelog | grep-based check |
| REQ-5 | `.schedule-badge` and all other `#C0C0C0`/`rgba(192,192,192,*)` usages byte-identical to `002` | regression — existing tests unmodified |
| REQ-6 | Background/text/border tokens and `ErrorBanner` red unchanged from `002` | regression — existing tests unmodified |

## Sign-off

- [x] Product owner (Nayeem) approves locked colors (Silver `#C0C0C0` outer / Coral `#FCA5A5` middle / Red `#F42A41` dot)
- [x] Product owner confirms ring-to-session mapping unchanged from `002`
- [x] RSD signed off -> proceed to implementation
