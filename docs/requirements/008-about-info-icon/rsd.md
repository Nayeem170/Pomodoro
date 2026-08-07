# RSD: About Nav Icon — Info Circle Mark in 3-Color Theme

## Status

`Signed off — 2026-08-06`

Fourth tab to get a custom mark (Focus=ring, Settings=gear, History=bars).
Uses the existing `IsLogoIcon` flag pattern from `006`/`007` — no new
plumbing decision needed.

## Background

Product owner picked an info-circle mark (image supplied) to replace the
About nav tab's current emoji icon (`Constants.Layout.AboutNavIcon = "ℹ️"`),
using the locked 3-color theme (Silver `#C0C0C0` / Coral `#FCA5A5` / Red
`#F42A41`).

Same shape of change as `005`/`006`/`007` — per-tab image mark instead of
emoji glyph, rendered via `NavLinkData.IsLogoIcon` and the existing
`<img class="nav-icon nav-icon-logo">` markup already wired in both nav
loops in `MainLayout.razor`.

## Approved Mark Geometry (from supplied reference image)

| Element | Shape | Color |
|---|---|---|
| Outer ring | Circle outline, thick stroke | Silver `#C0C0C0` |
| Body | Filled circle behind ring | Coral `#FCA5A5` |
| 'i' dot | Small solid circle, top | Red `#F42A41` |
| 'i' stem | Rounded vertical bar, below dot | Silver `#C0C0C0` |

Same grammar as prior marks — silver = frame, coral = fill, red = single
accent. Exact stem/dot proportions and ring thickness are a visual-proofing
detail at 24-28px, same workflow as `006`/`007`.

## Requirements (EARS)

- **REQ-1**: THE SYSTEM SHALL create a new info-circle mark SVG file
  (proposed: `logo-tarkeez-about.svg`) using the geometry/color table above,
  192×192 viewBox matching existing mark conventions.
- **REQ-2**: THE SYSTEM SHALL replace the About nav tab's emoji icon
  (`Constants.Layout.AboutNavIcon`) with the new info-circle mark, rendered
  in both desktop header nav and mobile bottom nav, via the existing
  `IsLogoIcon` flag mechanism — no new markup pattern needed.
- **REQ-3**: THE SYSTEM SHALL leave Focus (ring), Settings (gear), and
  History (bars) nav tab icons unchanged — scoped to About only.
- **REQ-4**: THE SYSTEM SHALL add `Constants.Layout.AboutNavLogoPath`
  (matching `FocusNavLogoPath`/`SettingsNavLogoPath`/`HistoryNavLogoPath`
  naming) pointing at the new mark file.

## Affected Files

| File | Change |
|---|---|
| `src/Pomodoro.Web/wwwroot/logo-tarkeez-about.svg` | **New file** — info-circle mark per geometry table |
| `src/Pomodoro.Web/Constants/Constants.UI.cs` | `AboutNavIcon` removed from nav rendering; new `AboutNavLogoPath` constant added |
| `src/Pomodoro.Web/Services/LayoutPresenterService.cs` | About link entry: `Icon = Constants.Layout.AboutNavLogoPath, IsLogoIcon = true` |
| `src/Pomodoro.Web/Layout/MainLayout.razor` | No change — existing `IsLogoIcon` conditional already covers this |

## Testable Scenarios (Gate 1 mapping)

| REQ | Scenario | Test location |
|---|---|---|
| REQ-1 | Info-circle mark renders correctly at 24-28px nav-icon size, matches 3-color theme | visual check |
| REQ-2/4 | About tab shows info-circle mark, not emoji, in both desktop and mobile nav | bUnit `MainLayoutTests`, e2e `navigation.spec.ts`/`mobile-nav.spec.ts` |
| REQ-3 | Focus/Settings/History icons unchanged | regression — existing tests unmodified |

## Open Questions

1. Exact stem width/dot radius/ring thickness need visual proofing at
   actual render size before locking — same workflow as `006`/`007`.
2. Filename/constant naming (`logo-tarkeez-about.svg` / `AboutNavLogoPath`)
   are proposals matching established pattern — confirm or rename.

## Sign-off

- [x] Product owner (Nayeem) approves info-circle mark shape/color assignment
- [x] RSD signed off -> proceed to implementation
