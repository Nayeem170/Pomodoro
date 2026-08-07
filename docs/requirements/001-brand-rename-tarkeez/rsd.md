# RSD: Brand Rename — Pomodoro → Tarkeez

## Status

`Approved — signed off`

## Background

Product started as a generic "Pomodoro timer" app. Scope has grown beyond a single timer
(tasks, schedule, history, sync) and is moving under the `bitops` domain/brand. Three
changes are in scope:

1. Home nav tab label `Timer` → `Focus` (already shipped — see [Already Shipped](#already-shipped)).
2. Product/brand name `Pomodoro` → `Tarkeez` (تركيز — Arabic for "concentration/focus")
   across all brand-identity surfaces.
3. App icon/logo swap to the new Tarkeez mark across all icon/favicon surfaces.

## Critical Scope Boundary — Brand Name vs. Domain Term

`Pomodoro` is used in this codebase in **two unrelated senses**. Only one is in scope.

| Sense | Example | In scope? |
|---|---|---|
| **Brand identity** — the product's name | `<title>`, header app name, manifest `name`/`short_name`, About page hero title, meta tags, loading splash text | **Yes — rename to Tarkeez** |
| **Domain term** — the Pomodoro Technique (25-min work interval methodology invented by Francesco Cirillo) | `SessionType.Pomodoro` enum, `--pomodoro-color` CSS var, "Pomodoro Technique" explainer copy on About page, `DefaultPomodoroMinutes`, C# namespace `Pomodoro.Web`, session/test names | **No — do not touch** |

Renaming the domain term or the C# namespace is explicitly **out of scope** — it would
touch ~690 files (namespace alone) for zero user-facing value and break every existing
test/CI reference. This RSD governs brand-identity text only.

## Already Shipped

Documented here for traceability (implemented ahead of this RSD, no spec existed yet):

- `Constants.UI.cs` → `Layout.TimerNavLinkTitle` value changed `"Timer"` → `"Focus"`.
- Consumers: `LayoutPresenterService.GetNavigationLinks()` (desktop header nav + mobile
  bottom nav both read this one constant).
- Tests updated: `LayoutPresenterServiceTests.cs` (renamed test to
  `GetNavigationLinks_FirstLinkIsFocus`, asserts `"Focus"`), `tests/e2e/pages/navigation.spec.ts`,
  `tests/e2e/pages/mobile-nav.spec.ts` (selectors `a[title="Focus"]`).
- `MainLayoutTests.cs` mock fixtures still hardcode `Title = "Timer"` in 3 places — these
  are self-contained test doubles, not wired to the real constant, so they don't assert
  against production behavior. Left as-is; not a defect, just stale fixture text.

## Final Logo Asset

`src/Pomodoro.Web/wwwroot/logo-tarkeez-v2-target-3ring-smalldot.svg` — confirmed final.
Concentric target: gray outer ring (`#8a8a8a`... superseded to contrast `#1a1a1a`), green
ring (`#006A4E`), small red center dot (`#F42A41`, `r=19`), uniform 10px stroke/gap pitch.
Transparent background, 192×192 viewBox, no text — scales to favicon size.

Draft/rejected variants (`logo-tarkeez-v1-ring.svg`, `-v2-target.svg`, `-v2-target-bd.svg`,
`-v2-target-final.svg` [4-ring], `-v3-converge.svg`) exist in the same directory. **Cleanup
task**: delete all non-final variants once this RSD ships (tracked as REQ-11 below).

## Requirements (EARS)

### Brand text

- **REQ-1**: THE SYSTEM SHALL display `Tarkeez` as the application name in the browser
  tab title (`<title>` in `index.html`).
- **REQ-2**: THE SYSTEM SHALL display `Tarkeez` as the app name in the header
  (`Constants.Layout.AppTitle`, rendered in `MainLayout.razor` `.header-text`).
- **REQ-3**: THE SYSTEM SHALL display `Tarkeez` as the app name in the mobile PWA
  install prompt and home-screen label (`manifest.webmanifest` → `name`, `short_name`).
- **REQ-4**: THE SYSTEM SHALL display `Tarkeez` as the hero title on the About page
  (`About.razor` `.about-hero-title`, currently literal `"Pomodoro"`, not a constant —
  convert to a constant as part of this change per project convention).
- **REQ-5**: THE SYSTEM SHALL display `Tarkeez` in social share previews
  (`og:title`, `og:site_name`, `twitter:title` meta tags in `index.html`).
- **REQ-6**: THE SYSTEM SHALL display `Tarkeez` in the WASM loading splash text
  (`index.html` `.loading-text`, currently `"Loading Pomodoro..."`).
- **REQ-7**: THE SYSTEM SHALL retain the literal string `Pomodoro` in all Pomodoro
  Technique domain copy (About page methodology explainer, session-type labels, timer
  duration constants) — these describe the technique, not the product name, and are
  explicitly excluded per the scope boundary above.
- **REQ-8**: THE SYSTEM SHALL retain `SessionType.Pomodoro`, `--pomodoro-color`,
  `Pomodoro.Web` namespace, and all test/CSS/C# identifiers unchanged.

### Logo/icon

- **REQ-9**: THE SYSTEM SHALL use `logo-tarkeez-v2-target-3ring-smalldot.svg` (or a
  renamed copy, e.g. `logo-tarkeez.svg`) as the source for:
  - `icon-192.svg` and `icon-512.svg` (currently tomato-emoji placeholders)
  - the inline data-URI favicon `<link rel="icon">` in `index.html`
  - `apple-touch-icon` links (already point at `icon-192.svg`/`icon-512.svg`, no change
    needed once those files are replaced)
  - `manifest.webmanifest` icon entries (already reference `icon-192.svg`/`icon-512.svg`
    by filename, no path change needed)
- **REQ-10**: THE SYSTEM SHALL replace the header brand emoji (`Constants.Layout.AppIcon`,
  currently `🍅`) with an inline `<img>` of the Tarkeez SVG mark (`Constants.Layout.LogoPath`).
  Decision: inline SVG mark, not text-only, not emoji.
- **REQ-11**: THE SYSTEM SHALL NOT retain draft/rejected logo SVG files
  (`logo-tarkeez-v1-ring.svg`, `-v2-target.svg`, `-v2-target-bd.svg`,
  `-v2-target-final.svg`, `-v3-converge.svg`) in `wwwroot/` after this task ships.

## Affected Files (inventory)

| File | Change |
|---|---|
| `src/Pomodoro.Web/wwwroot/index.html` | `<title>`, `og:title`, `og:site_name`, `twitter:title`, JSON-LD `name`, `.loading-text`, favicon `<link>` |
| `src/Pomodoro.Web/wwwroot/manifest.webmanifest` | `name`, `short_name` |
| `src/Pomodoro.Web/Constants/Constants.UI.cs` | `Layout.AppTitle`, `Layout.AppIcon` (pending REQ-10 decision) |
| `src/Pomodoro.Web/Pages/About.razor` | hero title literal `"Pomodoro"` → new constant |
| `src/Pomodoro.Web/wwwroot/icon-192.svg` | replace tomato emoji with Tarkeez mark |
| `src/Pomodoro.Web/wwwroot/icon-512.svg` | replace tomato emoji with Tarkeez mark |
| `src/Pomodoro.Web/wwwroot/logo-tarkeez-v2-target-3ring-smalldot.svg` | keep as source-of-truth mark; delete other variants |

## Testable Scenarios (Gate 1 mapping)

| REQ | Scenario | Test location |
|---|---|---|
| REQ-1 | Page `<title>` contains `Tarkeez`, not `Pomodoro` | new/updated e2e assertion (likely `about-content.spec.ts` or a new `branding.spec.ts`) |
| REQ-2 | `MainLayout` renders `Tarkeez` in `.header-text` | `MainLayoutTests.cs` (`MainLayout_RendersAppTitle`) |
| REQ-3 | `manifest.webmanifest` parses with `name`/`short_name` = `Tarkeez` | new unit or static-content test |
| REQ-4 | About page markup contains `Tarkeez` hero title | `AboutPageTests` (bUnit) + `about-content.spec.ts` |
| REQ-5 | Meta tag values updated | e2e or static assertion on `index.html` |
| REQ-6 | Loading splash text updated | visual/manual check (pre-WASM-load DOM, hard to assert in bUnit) |
| REQ-7/8 | Existing tests referencing `Pomodoro` as technique term (`SessionType`, About methodology copy, CSS var) still pass unmodified | full existing suite — regression guard, no new test needed |
| REQ-9 | `icon-192.svg`/`icon-512.svg` file content matches new mark | visual check; optionally snapshot test |
| REQ-11 | Draft SVG files absent from `wwwroot/` | file-existence check in gate/CI (optional) |

## Open Questions

1. **REQ-10**: **Resolved** — inline `<img>` of the Tarkeez SVG mark in header and About
   hero. Not text-only, not emoji.
2. **sitemap.xml / robots.txt**: Checked. Both reference only the domain URL
   (`pomodoro.bitops.bd`), no brand-name text. Domain migration is out of scope. No
   changes needed.
3. Domain migration (`bitops` subdomain, DNS, deploy config) — explicitly **out of scope**
   for this RSD; brand text/logo only.

## Sign-off

- [x] Product owner (Nayeem) approves scope boundary (brand vs. technique term split)
- [x] Product owner approves REQ-10 decision (inline SVG `<img>` mark)
- [x] RSD signed off → proceed to implementation
