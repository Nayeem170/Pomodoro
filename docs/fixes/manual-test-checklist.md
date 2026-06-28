# Manual Test Checklist — Tasks-not-rendering fix set (Fixes 1–7)

Work top-to-bottom. Each case is discrete with an expected result. Mark `[ ]` → `[x]` when done.

## Prerequisite (do once first)
- [ ] **TC0 — Clean bundle.** Stop server → `dotnet build` → run fresh build. DevTools → Application → Service Workers → Unregister → hard-reload (Disable cache on Network). **Expected:** app loads, no stale PWA code.

## Tasks tab & list rendering (Fixes 5, 6, 7)
- [ ] **TC1 — Add local task shows immediately.** Click "+ Add", type a name, press Enter. **Expected:** task appears in list; Tasks-tab badge count +1.
- [ ] **TC2 — Switch Schedule → Tasks.** Click the Schedule tab, then the Tasks tab. **Expected:** local tasks render both times; no empty list.
- [ ] **TC3 — Repeated click is stable.** Click the Tasks tab 3× rapidly. **Expected:** list stays populated, no flicker to empty.
- [ ] **TC4 — Dead-id recovery (the R1 fix).** Connect Google Tasks → pick a Google list → Disconnect → reload → click the Tasks tab. **Expected:** local tasks render (badge = list count); click is not swallowed.
- [ ] **TC5 — Completed tasks separate.** Complete a task. **Expected:** it moves under a "Completed" section; badge count drops by 1.
- [ ] **TC6 — Badge = list count invariant.** With a few local tasks, compare the Tasks-tab badge number to the visible task count. **Expected:** identical.

## Cloud Sync (Fixes 1, 2, 3, 4)
- [ ] **TC7 — Connect.** Settings → Cloud Sync → Connect. **Expected:** connects; connected status; no console errors.
- [ ] **TC8 — Silent re-auth on reload (Fix 4).** While connected, close & reopen the tab. **Expected:** reconnects silently; Google lists load; no reconnect prompt; no 401 storm in console.
- [ ] **TC9 — Reconnect banner (Fix 3).** Revoke access in your Google Account security page → reload → open Cloud Sync settings. **Expected:** "Session expired — Reconnect" banner appears; click Reconnect → re-auths → banner clears.
- [ ] **TC10 — Sync Now.** Settings → Sync Now button. **Expected:** success; Last synced time updates.
- [ ] **TC11 — Tasks auth failure isolates Drive (Fix 2).** With Drive connected but the Tasks token revoked, reload. **Expected:** Drive periodic sync still runs (console: one warning, not "init attempt 3 failed"); local tasks render.
- [ ] **TC12 — 401 self-heals (Fix 1).** Let the Tasks token expire (wait or revoke). Trigger a list refresh. **Expected:** one silent re-auth, lists load — not a hard failure.

## Persistence / lifecycle
- [ ] **TC13 — Selected list survives reload.** Pick a (valid) Google list, reload. **Expected:** same list selected and its tasks render.
- [ ] **TC14 — Stale selected list falls back.** Select a Google list, disconnect, reload. **Expected:** app selects local Tasks list automatically; local tasks render.
- [ ] **TC15 — Disconnect clears state.** Cloud Sync → Disconnect. **Expected:** Google tabs vanish, returns to local list, no errors.
