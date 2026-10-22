# Project State — V3.0

Keep this file short. It is the live handoff state for implementation.

## Current state
- Pack: V3.0 documentation baseline
- Production source repository: solution on branch `phase-0-foundation` (14 library projects plus the `Launcher.App` WinUI project). Contract shapes in `Launcher.Core` and `Launcher.PluginContracts`; structured diagnostics implemented in `Launcher.Diagnostics` (not yet wired in); no feature code
- Current phase: Phase 0 — Architecture & Project Contract
- Phase status: IN PROGRESS (started 2026-10-21; branch `phase-0-foundation`)
- Build status: restore, Debug and Release builds succeeded on the owner machine (SDK 10.0.402, `net10.0-windows10.0.26100.0`, 2026-10-21). The 14 library results were pasted; the later run including `Launcher.App` was reported green by the owner. `Launcher.App` starts and shows an empty window (manual check). No CI-equivalent build yet.
- Test status: 18 of 18 tests passed on the owner machine (2026-10-21): 7 architecture and 11 contract-shape. 5 diagnostics tests are written and not yet run there. No feature tests because no feature code exists

## Current gate
None. The Windows 11 24H2 (build 26100) minimum OS was confirmed by the project owner on 2026-10-21 (ADR-0015).

## Open items
- ADR-0013 (coding agent: ChatGPT → Claude) awaits owner sign-off.
- CI workflow written (ADR-0021, image `windows-2025`) but has not yet run on GitHub.
- `Launcher.Core.Tests` (xUnit v3, ADR-0017): 18 of 18 tests pass on the owner machine (Microsoft Testing Platform via `global.json`).
- The `Launcher.Motion → Launcher.Core` reference is provisional (see PHASE_STATUS.md). Other reference choices are ratified by ADR-0018 and ADR-0019.

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
Owner applies the diagnostics and CI patch, runs restore, Debug and Release builds and tests, and reports the GitHub Actions result. Then the Phase 0 exit review (see PHASE_STATUS.md).

## Rules
Only record verified implementation facts. Do not mark code complete because it exists. Require build + tests + relevant manual verification.

## Rollback point
V3.0 pre-implementation documentation freeze (`main` at commit 043d875).
