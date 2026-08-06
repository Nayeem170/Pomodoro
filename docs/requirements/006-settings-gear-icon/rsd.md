# RSD: Settings Nav Icon — Gear Mark in 3-Color Theme

## Status

`Signed off — 2026-08-06`

Open Question 1 resolved: add `IsLogoIcon` flag to `NavLinkData` now
(two consumers, not one). Refactors 005's Focus special-case into the
same pattern.

## Background

Product owner supplied a reference image: a gear/cog icon — silver outline teeth, coral
fill body, red center dot. Request: replace the Settings nav tab's current emoji icon
(`Constants.Layout.SettingsNavIcon = "⚙️"`) with this gear mark, using the app's locked
3-color theme (Silver `#C0C0C0` / Coral `#FCA5A5` / Red `#F42A41`).

This is the same shape of change as `005-logo-checkmark-migration` REQ-3a/REQ-5 (Focus
tab getting a dedicated image mark instead of an emoji), applied to a second tab.

## Note: This Is the Second Tab, Not the First — Revisit the YAGNI Call

`005`'s REQ-5 explicitly chose "special-case `Href == HomeRoute` directly in markup, no
`NavLinkData` model change" specifically *because* it was scoped to one tab (Focus) and
adding a `bool IsLogoIcon` flag was called over-engineering for a one-off need. That
justification weakens now that Settings needs the same treatment — this is no longer
one special case, it's a pattern. Flagged as Open Question 1: stick with a second
markup special-case (`Href == SettingsRoute`), or go back and add the `IsLogoIcon`
flag to `NavLinkData` now that there are two consumers. If a third tab (History or
About) ever needs a custom mark too, three special-cases in the same Razor loop starts
to smell — this RSD is the natural point to reconsider, not after a third one shows up.

## Proposed Gear Geometry (192×192 viewBox, matching existing mark conventions)

| Element | Shape | Color |
|---|---|---|
| Outer gear teeth | 8-tooth cog outline, stroke only | Silver `#C0C0C0` |
| Inner body | Filled circle/ring behind the teeth | Coral `#FCA5A5` |
| Center dot | Small solid circle | Red `#F42A41` |

Same three-color role assignment as the Focus tab's Ring mark and the app's main
Checkmark mark (`005`) — silver = outer/frame, coral = body/fill, red = center
accent — keeping a consistent "grammar" across all per-tab marks. Exact tooth count,
tooth width, and radii are a TDS-level detail requiring visual proofing at 24-28px
(the actual rendered nav-icon size) before locking — a gear with too many/thin teeth
will blur into a fuzzy circle at that size.

## Requirements (EARS)

- **REQ-1**: THE SYSTEM SHALL create a new gear-mark SVG file (proposed:
  `logo-tarkeez-gear.svg`) using the geometry/color table above.
- **REQ-2**: THE SYSTEM SHALL replace the Settings nav tab's emoji icon
  (`Constants.Layout.SettingsNavIcon`) with the new gear mark, rendered in both the
  desktop header nav and mobile bottom nav — same rendering mechanism as `005` REQ-5
  (pending Open Question 1's resolution on markup-special-case vs. model-flag approach).
- **REQ-3**: THE SYSTEM SHALL leave Focus (per `005`), History, and About nav tab icons
  unchanged by this RSD — scoped to Settings only.
- **REQ-4**: THE SYSTEM SHALL add `Constants.Layout.SettingsNavLogoPath` (or equivalent,
  matching whatever naming `005` used for `FocusNavLogoPath`) pointing at the new gear
  file.

## Affected Files (inventory, pending Open Question 1)

| File | Change |
|---|---|
| `src/Pomodoro.Web/wwwroot/logo-tarkeez-gear.svg` | **New file** — gear mark per geometry table |
| `src/Pomodoro.Web/Constants/Constants.UI.cs` | `SettingsNavIcon` removed or unused for nav rendering; new `SettingsNavLogoPath` constant added |
| `src/Pomodoro.Web/Services/LayoutPresenterService.cs` | Only changes if Open Question 1 resolves to the `IsLogoIcon` model-flag approach |
| `src/Pomodoro.Web/Layout/MainLayout.razor` | Desktop + mobile nav loops — extend (or refactor, per Open Question 1) the REQ-5-style conditional to also cover `Href == Constants.Routing.SettingsRoute` |

## Testable Scenarios (Gate 1 mapping)

| REQ | Scenario | Test location |
|---|---|---|
| REQ-1 | Gear mark SVG renders correctly at 24-28px nav-icon size, matches 3-color theme | visual check |
| REQ-2/4 | Settings tab shows gear mark, not emoji, in both desktop and mobile nav | bUnit `MainLayoutTests`, e2e `navigation.spec.ts`/`mobile-nav.spec.ts` |
| REQ-3 | Focus/History/About icons unchanged | regression — existing tests unmodified |

## Open Questions

1. **Markup special-case (continue `005`'s pattern) vs. `IsLogoIcon` model flag** — see
   "Note" section above. This is the real decision in this RSD; everything else is
   mechanical once it's made.
2. Exact gear geometry (tooth count/width/radii) needs visual proofing at actual
   render size before locking, same workflow as prior mark RSDs.
3. Filename/constant naming (`logo-tarkeez-gear.svg` / `SettingsNavLogoPath`) are
   proposals matching `005`'s established naming pattern — confirm or rename.

## Sign-off

- [x] Product owner (Nayeem) approves gear mark shape/color assignment
- [x] Product owner decides Open Question 1: add `IsLogoIcon` model flag
- [x] RSD signed off -> proceed to implementation
