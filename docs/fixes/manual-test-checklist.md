# Manual Test Checklist — Tasks-not-rendering fix set (Fixes 1–7)

Work top-to-bottom. Each case is discrete with an expected result. Mark `[ ]` → `[x]` when done.

## Prerequisite (do once first)
- [x] **TC0 — Clean bundle.** Stop server → `dotnet build` → run fresh build. DevTools → Application → Service Workers → Unregister → hard-reload (Disable cache on Network). **Expected:** app loads, no stale PWA code.

## Tasks tab & list rendering (Fixes 5, 6, 7)
- [x] **TC1 — Add local task shows immediately.** Click "+ Add", type a name, press Enter. **Expected:** task appears in list; Tasks-tab badge count +1. _Result: PASS._
- [x] **TC2 — Switch Schedule → Tasks.** Click the Schedule tab, then the Tasks tab. **Expected:** local tasks render both times; no empty list. _Result: PASS._
- [x] **TC3 — Repeated click is stable.** Click the Tasks tab 3× rapidly. **Expected:** list stays populated, no flicker to empty. _Result: PASS._
- [ ] **TC4 — Dead-id recovery (the R1 fix).** Connect Google Tasks → pick a Google list → Disconnect → reload → click the Tasks tab. **Expected:** local tasks render (badge = list count); click is not swallowed. _Result: BLOCKED — see Discovered issue #1 (403 scope gap surfaces as a reconnect loop during Connect). Cannot reach a Google-list selection until the Tasks API/scope is granted._
- [ ] **TC5 — Completed tasks separate.** Complete a task. **Expected:** it moves under a "Completed" section; badge count drops by 1.
- [ ] **TC6 — Badge = list count invariant.** With a few local tasks, compare the Tasks-tab badge number to the visible task count. **Expected:** identical.

## Cloud Sync (Fixes 1, 2, 3, 4)
- [ ] **TC7 — Connect.** Settings → Cloud Sync → Connect. **Expected:** connects; connected status; no console errors.
- [ ] **TC8 — Silent re-auth on reload (Fix 4).** While connected, close & reopen the tab. **Expected:** reconnects silently; Google lists load; no reconnect prompt; no 401 storm in console.
- [ ] **TC9 — Reconnect banner (Fix 3).** Revoke access in your Google Account security page → reload → open Cloud Sync settings. **Expected:** "Session expired — Reconnect" banner appears; click Reconnect → re-auths → banner clears.
- [ ] **TC10 — Sync Now.** Settings → Sync Now button. **Expected:** success; Last synced time updates.
- [ ] **TC11 — Tasks auth failure isolates Drive (Fix 2).** With Drive connected, disable the Google Tasks API in GCP (or use an account whose grant lacks the Tasks scope) so Tasks calls 403, then reload. **Expected:** Drive periodic sync still runs (console: one warning, not "init attempt 3 failed"); local tasks render.
- [ ] **TC12 — 401 self-heals (Fix 1).** While connected, let the GIS access token expire (wait ~1h, or exercise it near expiry), then trigger a list refresh. **Expected:** one silent re-auth, lists load — not a hard failure.

## Persistence / lifecycle
- [ ] **TC13 — Selected list survives reload.** Pick a (valid) Google list, reload. **Expected:** same list selected and its tasks render.
- [ ] **TC14 — Stale selected list falls back.** Select a Google list, disconnect, reload. **Expected:** app selects local Tasks list automatically; local tasks render.
- [ ] **TC15 — Disconnect clears state.** Cloud Sync → Disconnect. **Expected:** Google tabs vanish, returns to local list, no errors.

## Discovered issues
1. **Reconnect loop on Connect (403 scope gap).** Immediately after a fresh interactive Connect → sign-in, the "Session expired — Reconnect" banner appears (~0.5 s later) and reappears after each Reconnect. Root cause: the Google Tasks API calls return **403** (either the API is disabled in GCP, or the existing OAuth grant predates the `auth/tasks` scope). Silent re-auth (`prompt: none`) **cannot widen an existing grant**, so Reconnect mints a fresh token of the same (insufficient) scope and the next Tasks refresh 403s again → infinite banner. Drive sync itself connects and works; only the Tasks feature is gated.
   - **Resolution (config — still required to use Tasks):** enable the **Google Tasks API** in the GCP Cloud Console, then **Disconnect + reconnect** interactively so the `auth/tasks` scope is re-granted with full consent.
   - **Code fix (done, abb6976):** a 403 now throws `TasksAccessForbiddenException` instead of `UnauthorizedAccessException`, so the non-UAE filter in `RefreshGoogleListsAsync` swallows it as "Tasks unavailable" — Drive connects and syncs, no misleading reconnect banner / loop. The "Session expired — Reconnect" banner now only fires for a true 401 (token expiry), which Reconnect can fix.
