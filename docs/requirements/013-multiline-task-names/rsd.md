# RSD: Multiline Task Names

## Status

`Accepted (retroactive) — 2026-08-14`

Branch: `integration/ui-features`

Retroactive spec: the behavior shipped via live iteration in
`f23f1bc` before this document existed. Written to record the
deliberate decisions most likely to be "fixed" wrongly later,
especially the Enter interception.

## Background

Task name fields (add-task input, subtask input, edit panel name)
were single-line `<input>` elements. Long names overflow-clipped or
force-scrolled horizontally; there was no way to see a full long name
while composing it. All task name fields are now `<textarea>` with
auto-grow, and rendered task rows wrap to multiple lines with controls
pinned to the first line.

## Requirements (EARS)

WHEN the user presses Enter in a task name textarea THE SYSTEM SHALL
commit the task (or open inline edit) without inserting a newline.
Enter is deliberately swallowed by a global keydown handler in
`app-init.js`; commit itself is handled in Blazor. Do not remove the
`preventDefault()` — removing it lets newlines into stored task names.

WHEN the user presses Shift+Enter in the add-task input THE SYSTEM
SHALL expand the more-options panel (`_isMoreExpanded = true`,
`TaskList.razor.cs:267-269`) instead of committing.

WHEN the user types in a task name textarea THE SYSTEM SHALL grow
the field to fit content up to `max-height: 132px`
(`app.css` shared textarea rule) and scroll beyond that. Growth is
driven by `app-init.js` setting `style.height = scrollHeight + borders`
(border widths are compensated because `scrollHeight` excludes borders
while `box-sizing: border-box` includes them; omitting the compensation
produces a ~2px overflow scrollbar).

WHEN a task name contains an unbroken string longer than the row
width THE SYSTEM SHALL wrap it via `overflow-wrap: anywhere`
(`.task-text`, `.item-title`, `.subtask-title`). `word-break:
break-word` is deliberately NOT paired with it; the pair breaks more
eagerly than either alone.

WHEN a task row is taller than one line THE SYSTEM SHALL keep the
checkbox, toggle, actions, and pomodoro count aligned with the first
text line (`align-items: flex-start` on `.task-row`, per-control
margin offsets), not vertically centered.

WHEN the add-task input is empty THE SYSTEM SHALL reserve right
padding (`130px`, via `:placeholder-shown`) for the "Shift+Enter for
details" hint and show the hint; WHEN it has content THE SYSTEM SHALL
use full width and hide the hint (pure CSS, no Blazor state).

## Out of Scope

- Multiline names in the timer/clock task path (single line,
  wrap-anywhere, covered by RSD 010).
- Google Tasks sync of embedded newlines (none can be entered).
