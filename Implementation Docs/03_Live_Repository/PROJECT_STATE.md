# Project State — V3.0

Keep this file short. It is the live handoff state for implementation.

## Current state
- Pack: V3.0 documentation baseline
- Production source repository: solution on `main` (tag `phase-0-complete`) and on branch `phase-1-core-bar` (14 library projects, the `Launcher.App` WinUI project, `tests/Launcher.Core.Tests` and `tests/Launcher.Integration.Tests`). Contract shapes in `Launcher.Core` and `Launcher.PluginContracts`; structured diagnostics implemented in `Launcher.Diagnostics` (not yet wired in); no feature code.
- Current phase: Phase 0 complete: merged to `main` (13482a5) and tagged `phase-0-complete` on 2026-10-22. Phase 1 (Core + Exact Command Bar) in progress on branch `phase-1-core-bar`; Tasks 1, 2 and 6 are implemented as library code and verified by automated tests on the owner machine (hotkey and single instance in `Launcher.Integrations`; lifecycle, readiness, activation and runtime in `Launcher.Core`; ADR-0024 and ADR-0026). None of it is wired into `Launcher.App` yet.
- Build status: restore, Debug and Release builds succeed on the owner machine (SDK 10.0.402, `net10.0-windows10.0.26100.0`) and in GitHub Actions (Debug and Release, commit 36a445c). Phase 1 Debug builds succeed on the owner machine (latest 2026-10-25). GitHub Actions run #11 on 49175b0 (Debug and Release, pushed 2026-10-26) concluded success, which is the first Release build and test of the Phase 1 code; Release was not run locally.
- Test status: 142 of 142 tests pass on the owner machine (Debug, 2026-10-26, 0 skipped) across `Launcher.Core.Tests` and `Launcher.Integration.Tests` (the output shows only the total). They include the single-instance, hotkey, lifecycle, readiness, activation and runtime tests, and two tests that used the real Windows hotkey API. CI also concluded success on 49175b0 (run #11; per-test counts not visible).

## Current gate
None. Minimum OS confirmed (ADR-0015).

## Open items
See "Carried forward" in PHASE_STATUS.md and ADR-0022. The most important: ADR-0013 sign-off is deferred by the owner (either coding agent may continue); Security capability contracts come before the first provider or plugin; the Phase 1 state list difference is resolved by ADR-0023.

## Verified baseline
- Native WinUI 3/XAML/Composition, unpackaged and framework-dependent, x64
- .NET 10 / C# 14, SDK 10.0.402 pinned
- Windows App SDK 2.5.1 Stable, BuildTools 10.0.26100.8249
- Windows SDK 10.0.26100.8249 baseline (ADR-0016)
- Core unelevated, default-deny capabilities, out-of-process plugins, restricted privileged broker (design)
- SQLite structured storage, Named Pipes initial plugin IPC (design)
- xUnit.net v3 on Microsoft Testing Platform; CI on GitHub Actions `windows-2025`

## Next action
Plan Task 7 (composition root, diagnostics wiring, log location), which gives the first manual check of the hotkey; plan only, no code until approved. Order after Task 6 is ADR-0027: 7, 3, 5, 4, with Task 8 throughout.

## Rules
Only record verified implementation facts. Do not mark code complete because it exists. Require build + tests + relevant manual verification.

## Rollback point
Tag `phase-0-complete` (merge commit 13482a5 on `main`). Before Phase 0: commit 043d875, the V3.0 documentation freeze.
