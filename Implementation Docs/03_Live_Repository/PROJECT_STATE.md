# Project State — V3.0

Keep this file short. It is the live handoff state for implementation.

## Current state
- Pack: V3.0 documentation baseline
- Production source repository: solution scaffold on branch `phase-0-foundation` (14 library projects plus the `Launcher.App` WinUI project, no feature code)
- Current phase: Phase 0 — Architecture & Project Contract
- Phase status: IN PROGRESS (started 2026-10-21; branch `phase-0-foundation`)
- Build status: restore, Debug and Release builds succeeded on the owner machine (SDK 10.0.402, `net10.0-windows10.0.26100.0`, 2026-10-21). The 14 library results were pasted; the later run including `Launcher.App` was reported green by the owner. `Launcher.App` starts and shows an empty window (manual check). No CI-equivalent build yet.
- Test status: 6 architecture tests pass (Launcher.Core.Tests, owner machine, 2026-10-21); no feature tests yet because no feature code exists

## Current gate
None. The Windows 11 24H2 (build 26100) minimum OS was confirmed by the project owner on 2026-10-21 (ADR-0015).

## Open items
- ADR-0013 (coding agent: ChatGPT → Claude) awaits owner sign-off.
- CI image not yet selected.
- `Launcher.Core.Tests` (xUnit v3, ADR-0017): 6 of 6 tests pass on the owner machine (Microsoft Testing Platform via `global.json`).
- Reference choices for Diagnostics, Security and Motion are provisional (see PHASE_STATUS.md).

## Verified baseline
- Native WinUI 3/XAML/Composition
- .NET 10 / C# 14
- Windows App SDK 2.5.1 Stable
- Windows SDK 10.0.26100.8249 baseline (amended from 9457, ADR-0016)
- Core unelevated
- Default-deny capabilities
- Out-of-process plugins
- Restricted privileged broker
- SQLite structured storage
- Named Pipes initial plugin IPC

## Next action
Plan and implement the Phase 0 contracts (LauncherState and the Core/Search/UI/Plugin interfaces), then structured diagnostics and the CI skeleton (see PHASE_STATUS.md).

## Rules
Only record verified implementation facts. Do not mark code complete because it exists. Require build + tests + relevant manual verification.

## Rollback point
V3.0 pre-implementation documentation freeze (`main` at commit 043d875).
