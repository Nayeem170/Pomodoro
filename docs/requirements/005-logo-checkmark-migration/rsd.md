# RSD: Logo Migration — Ring to Circle+Checkmark, Nav Tab Icon, Loader Redesign

## Status

`Signed off — 2026-08-06`

REQ-5 locked to option (b) — markup special-case, no model change.
Checkmark coordinates are a first-pass proposal; visual iteration
during implementation (same workflow as prior logo RSDs).

## Background

Three changes locked by product owner in the same conversation pass:

1. **New logo shape**: replace the current concentric-ring mark (Silver outer ring /
   Coral middle ring / Red dot, from `003-logo-coral-silver-recolor`) with a **Circle +
   Checkmark** mark — text-prompt spec: *"a thick circular ring in silver `#C0C0C0` as
   the outer frame, with a bold checkmark stroke inside — short stroke coral `#FCA5A5`,
   long stroke red `#F42A41` — checkmark centered inside the ring, rounded line caps,
   centered on transparent background, no text, no gradient, task-complete motif."*
2. **Nav tab icon swap**: the "Focus" nav tab (`Constants.Layout.TimerNavIcon = "⏱️"`,
   a clock emoji) gets replaced with the logo mark itself, in place of the clock.
3. **Loader animation redesign**: the reveal animation needs asymmetric stroke caps —
   the short checkmark stroke ("small bar") uses a **strict/square** cap, while the long
   checkmark stroke ("large bar") **and** the outer circle use **rounded** treatment.

## Current State (verified against codebase)

- `Constants.Layout.LogoPath = "logo-tarkeez.svg"` (`Constants.UI.cs:129`) is the single
  source file, consumed via `<img src="@Constants.Layout.LogoPath">` in 4 places:
  `MainLayout.razor:10` (header), `About.razor:6` (hero), `Index.razor:7` and
  `History.razor:8` (Blazor-rendered loading spinners). Changing `logo-tarkeez.svg`
  itself auto-propagates to all 4 — no per-file edits needed for those.
- `icon-192.svg` / `icon-512.svg` (PWA/manifest icons) are separate files, not derived
  from `logo-tarkeez.svg` — edited independently in `003`.
- `index.html` has **two more independent copies** that predate Blazor booting and thus
  can't reference `LogoPath` via Razor: the favicon `data:image/svg+xml` inline SVG
  (`index.html:45`) and the static pre-boot loading spinner inline SVG
  (`index.html:76-79`, distinct from `Index.razor`'s post-boot `<img>` spinner).
- Nav icons are plain emoji strings rendered as text (`Constants.Layout.TimerNavIcon`,
  `HistoryNavIcon`, etc.), consumed via `LayoutPresenterService.GetNavigationLinks()` →
  `NavLinkData.Icon` → rendered as `<span class="nav-icon">@navLink.Icon</span>` in both
  desktop (`MainLayout.razor:14-21`) and mobile (`MainLayout.razor:43-50`) nav loops.
  There is currently no mechanism to render an `<img>`/inline-SVG instead of a text
  glyph for one specific nav link — this is new plumbing, not a value swap.
- Current loader animation (`app.css:1817-1833`) is an opacity step-reveal keyed to the
  old 3-element ring/middle/dot structure (`ls-step-outer`, `ls-step-middle`, 3-phase
  cycle). This whole animation needs retargeting to the new ring/short-bar/long-bar
  element set.

## New Logo Geometry (proposed, 192×192 viewBox — scale proportionally for 512)

| Element | Shape | Color | Cap style |
|---|---|---|---|
| Outer ring | `circle cx=96 cy=96 r=74 stroke-width=14` | Silver `#C0C0C0` | N/A — closed circle has no line ends |
| Short checkmark stroke | `path M60,100 L82,122` (approx, "down" stroke) | Coral `#FCA5A5` | **Butt/square at rest** (`stroke-linecap: butt`) — the "strict" bar |
| Long checkmark stroke | `path M82,122 L136,68` (approx, "up" stroke) | Red `#F42A41` | **Round** (`stroke-linecap: round`) |

Reference image (user-supplied) confirms: closed full-circle ring, checkmark as two
segments sharing the middle vertex `(82,122)` so they join seamlessly, `stroke-linejoin:
round` at that joint for a soft icon feel matching the reference. Two separate `<path>`
elements (not one two-tone path — SVG can't color one path two colors) is the
implementation approach.

Exact checkmark coordinates are a TDS-level detail (need visual proofing at 32px before
locking pixel values) — the table above is a starting proposal, not final.

### Loader-animation cap exception (amends REQ-7)

Static/resting logo keeps the table above (short bar butt-cap). But **while the loader is
actively drawing the mark on** (stroke-dasharray/dashoffset reveal), the moving tip of the
short bar and the circle's opening/closing gap use **round** cap during the animation —
soft in motion, even though the short bar's cap on shape it eventually settles into is
Butt. This supersedes REQ-7's original "cap style must not differ between static and
animated" rule — see REQ-8.

## Requirements (EARS)

### Logo shape

- **REQ-1**: THE SYSTEM SHALL replace the mark in `logo-tarkeez.svg`, `icon-192.svg`,
  `icon-512.svg`, the favicon inline SVG (`index.html:45`), and the pre-boot loading
  spinner inline SVG (`index.html:76-79`) with the new Circle+Checkmark design —
  five-surface sync rule carried over from `001`/`003`.
- **REQ-2**: THE SYSTEM SHALL render the short checkmark stroke with a butt/square line
  cap and the long checkmark stroke and outer ring with a round treatment, per the
  product owner's cap-style instruction.

### Nav tab icon

- **REQ-3**: THE SYSTEM SHALL replace the Focus nav tab's clock emoji
  (`Constants.Layout.TimerNavIcon`) with a logo mark, rendered in both the desktop
  header nav and mobile bottom nav.
- **REQ-3a (revised)**: THE SYSTEM SHALL use the **previous Ring mark** (Silver outer
  ring / Coral middle ring / Red dot — the `003-logo-coral-silver-recolor` design, being
  replaced everywhere else by this RSD) for the Focus nav tab icon specifically, NOT the
  new Circle+Checkmark mark. This requires preserving the current `logo-tarkeez.svg`
  content under a new filename **before** REQ-1 overwrites it, since `LogoPath` will
  point at the new checkmark design once REQ-1 lands and every other consumer (header,
  About, favicon, PWA icons, both loaders) uses the new mark.
- **REQ-4**: THE SYSTEM SHALL leave History/Settings/About nav tab icons
  (📊/⚙️/ℹ️) unchanged — this swap is scoped to the Focus tab only.
- **REQ-5**: THE SYSTEM SHALL special-case `navLink.Href == Constants.Routing.HomeRoute`
  directly in `MainLayout.razor`'s desktop and mobile nav render loops to render
  `<img class="nav-icon nav-icon-logo" src="@Constants.Layout.FocusNavLogoPath" alt="" />`
  (new constant pointing at the preserved Ring-mark file, NOT `LogoPath`) instead of
  `<span class="nav-icon">@navLink.Icon</span>`. No `NavLinkData`/model change — YAGNI,
  this is a one-off need scoped to the Focus tab only; revisit with a proper
  `IsLogoIcon` flag only if a second tab ever needs the same treatment.

### Loader animation

- **REQ-6**: THE SYSTEM SHALL redesign the loader step-reveal animation
  (`app.css:1817-1833`, currently `ls-step-outer`/`ls-step-middle` opacity phases keyed
  to the old ring/middle-ring/dot structure) to animate the new ring/short-bar/long-bar
  elements instead, preserving the existing "sequential reveal, then all-visible, then
  loop" pattern already established for this loader.
- **REQ-7**: THE SYSTEM SHALL apply the REQ-2 cap styling to the loader's animated mark
  at rest (i.e. its final, fully-revealed state matches the static logo exactly).
- **REQ-8 (amends REQ-7)**: THE SYSTEM SHALL use `stroke-linecap: round` for the short
  bar and the circle's reveal edge specifically *while the loader's draw-on animation is
  in progress* (the growing/moving tip during the stroke-dashoffset transition), even
  though the short bar's resting cap is butt/square per REQ-2. The distinction is
  "in motion = rounded, at rest = per REQ-2."

## Affected Files (inventory)

| File | Change |
|---|---|
| `src/Pomodoro.Web/wwwroot/logo-tarkeez-ring.svg` | **New file** — copy of current `logo-tarkeez.svg` content (Ring mark) preserved BEFORE REQ-1 overwrites the original, per REQ-3a |
| `src/Pomodoro.Web/wwwroot/logo-tarkeez.svg` | Replace ring/dot mark with Circle+Checkmark (REQ-1) |
| `src/Pomodoro.Web/wwwroot/icon-192.svg` | Same |
| `src/Pomodoro.Web/wwwroot/icon-512.svg` | Same, scaled |
| `src/Pomodoro.Web/wwwroot/index.html` | Favicon inline SVG (line 45); pre-boot loader inline SVG (lines 76-79) — new markup + element classes for animation targeting |
| `src/Pomodoro.Web/wwwroot/css/app.css` | Loader keyframes (lines 1812-1833) retargeted to new element structure |
| `src/Pomodoro.Web/Constants/Constants.UI.cs` | `TimerNavIcon` removed; new `FocusNavLogoPath = "logo-tarkeez-ring.svg"` constant added |
| `src/Pomodoro.Web/Services/LayoutPresenterService.cs` | No change — `NavLinkData`/`GetNavigationLinks()` untouched per REQ-5 |
| `src/Pomodoro.Web/Layout/MainLayout.razor` | Desktop nav loop (~lines 14-21) and mobile nav loop (~lines 43-50) — `@if (navLink.Href == Constants.Routing.HomeRoute)` conditional, `<img src="@Constants.Layout.FocusNavLogoPath">` vs `<span>` |

## Testable Scenarios (Gate 1 mapping)

| REQ | Scenario | Test location |
|---|---|---|
| REQ-1 | All 5 logo surfaces render identical Circle+Checkmark mark | visual check across header, favicon, PWA icon, both loaders, About page |
| REQ-2/7 | Static logo + loader's fully-settled state: short stroke butt cap, long stroke + ring round | visual check at 32px and 160px (loader size) |
| REQ-8 | During loader draw-on animation, short bar tip and circle reveal edge are round while animating | visual check mid-animation |
| REQ-3/3a/4 | Focus tab shows the OLD Ring mark (`logo-tarkeez-ring.svg`), not the new checkmark; History/Settings/About unchanged | bUnit `MainLayoutTests`, e2e `navigation.spec.ts`/`mobile-nav.spec.ts` |
| REQ-6 | Loader animates ring->short-bar->long-bar->all-visible->loop | visual check, existing loader-animation pattern preserved |

## Open Questions

1. ~~REQ-5 mechanism~~ — **Resolved**: option (b), special-case in markup, no model change.
2. Exact checkmark path coordinates in the Locked Geometry table are a first pass, not
   proofed at actual render sizes (16px favicon vs 160px loader) — needs visual
   iteration before implementation, same as prior logo RSDs' comparison-page workflow.
3. Cleanup: `wwwroot/logo-bitwork-concepts.html` and `wwwroot/logo-outer-ring-compare.html`
   are throwaway comparison artifacts from this and the prior naming exploration — delete
   once this RSD ships.

## Sign-off

- [x] Product owner (Nayeem) approves Circle+Checkmark as the new logo shape
- [x] Product owner approves cap-style treatment (REQ-2/7)
- [x] Product owner decides REQ-5 mechanism: option (b) markup special-case
- [x] Product owner confirms Focus-tab-only scope for nav icon swap (REQ-4)
- [x] RSD signed off -> proceed to implementation
