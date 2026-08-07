# RSD: History Nav Icon — 3-Bar Mark in 3-Color Theme

## Status

`Signed off — 2026-08-06`

Third tab to get a custom mark (after Focus=ring, Settings=gear). Uses
the already-established `IsLogoIcon` flag pattern from `006` — no new
plumbing decision needed.

## Background

Product owner picked a 3-ascending-bars mark (image supplied) to replace the
History nav tab's current emoji icon (`Constants.Layout.HistoryNavIcon = "📊"`),
using the locked 3-color theme (Silver `#C0C0C0` / Coral `#FCA5A5` / Red `#F42A41`).

Same shape of change as `005` (Focus) and `006` (Settings) — a per-tab image
mark instead of an emoji glyph, rendered via the existing `NavLinkData.IsLogoIcon`
flag and `<img class="nav-icon nav-icon-logo">` markup already wired in
`MainLayout.razor` for both desktop and mobile nav loops.

## Approved Mark Geometry (from supplied reference image)

Three vertical bars, ascending height left to right, rounded tops, shared
baseline, even spacing:

| Bar | Height | Outline | Fill |
|---|---|---|---|
| 1 (shortest, left) | short | Silver `#C0C0C0` | Coral `#FCA5A5` |
| 2 (mid) | medium | Coral `#FCA5A5` | Silver `#C0C0C0` |
| 3 (tallest, right) | tall | none | Solid Red `#F42A41` |

Same "grammar" as prior marks — silver = frame, coral = fill, red = single
solid accent (here: the accent is the whole tallest bar rather than a dot).
Exact bar widths/gaps/corner radii are a visual-proofing detail at 24-28px,
same workflow as `005`/`006`.

## Requirements (EARS)

- **REQ-1**: THE SYSTEM SHALL create a new 3-bar mark SVG file (proposed:
  `logo-tarkeez-history.svg`) using the geometry/color table above, 192×192
  viewBox matching existing mark conventions.
- **REQ-2**: THE SYSTEM SHALL replace the History nav tab's emoji icon
  (`Constants.Layout.HistoryNavIcon`) with the new bar mark, rendered in both
  desktop header nav and mobile bottom nav, via the existing `IsLogoIcon` flag
  mechanism (`LayoutPresenterService.GetNavigationLinks()` /
  `MainLayout.razor`'s `@if (navLink.IsLogoIcon)` branch) — no new markup
  pattern needed.
- **REQ-3**: THE SYSTEM SHALL leave Focus (ring), Settings (gear), and About
  (ℹ️) nav tab icons unchanged — scoped to History only.
- **REQ-4**: THE SYSTEM SHALL add `Constants.Layout.HistoryNavLogoPath`
  (matching `FocusNavLogoPath`/`SettingsNavLogoPath` naming) pointing at the
  new bar-mark file.

## Affected Files

| File | Change |
|---|---|
| `src/Pomodoro.Web/wwwroot/logo-tarkeez-history.svg` | **New file** — 3-bar mark per geometry table |
| `src/Pomodoro.Web/Constants/Constants.UI.cs` | `HistoryNavIcon` removed from nav rendering; new `HistoryNavLogoPath` constant added |
| `src/Pomodoro.Web/Services/LayoutPresenterService.cs` | History link entry: `Icon = Constants.Layout.HistoryNavLogoPath, IsLogoIcon = true` |
| `src/Pomodoro.Web/Layout/MainLayout.razor` | No change — existing `IsLogoIcon` conditional already covers this |

## Testable Scenarios (Gate 1 mapping)

| REQ | Scenario | Test location |
|---|---|---|
| REQ-1 | Bar mark renders correctly at 24-28px nav-icon size, matches 3-color theme | visual check |
| REQ-2/4 | History tab shows bar mark, not emoji, in both desktop and mobile nav | bUnit `MainLayoutTests`, e2e `navigation.spec.ts`/`mobile-nav.spec.ts` |
| REQ-3 | Focus/Settings/About icons unchanged | regression — existing tests unmodified |

## Open Questions

1. Exact bar widths/gaps/corner radii need visual proofing at actual render
   size before locking — same workflow as `006`'s gear iteration.
2. Filename/constant naming (`logo-tarkeez-history.svg` / `HistoryNavLogoPath`)
   are proposals matching established pattern — confirm or rename.

## Sign-off

- [x] Product owner (Nayeem) approves 3-bar mark shape/color assignment
- [x] RSD signed off -> proceed to implementation
