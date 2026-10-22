# Project State — V3.0

Keep this file short. It is the live handoff state for implementation.

## Current state
- Pack: V3.0 documentation baseline
- Production source repository: solution on branch `phase-0-foundation` (14 library projects, the `Launcher.App` WinUI project and `tests/Launcher.Core.Tests`). Contract shapes in `Launcher.Core` and `Launcher.PluginContracts`; structured diagnostics implemented in `Launcher.Diagnostics` (not yet wired in); no feature code.
- Current phase: Phase 0 complete (2026-10-22), pending merge to `main` and the `phase-0-complete` tag. Phase 1 not started.
- Build status: restore, Debug and Release builds succeed on the owner machine (SDK 10.0.402, `net10.0-windows10.0.26100.0`) and in GitHub Actions (Debug and Release, commit 36a445c).
- Test status: 23 of 23 tests pass (7 architecture, 11 contract-shape, 5 diagnostics) on the owner machine and in CI. No feature tests because no feature code exists.

## Current gate
None. Minimum OS confirmed (ADR-0015).

## Open items
See "Carried forward" in PHASE_STATUS.md and ADR-0022. The most important: ADR-0013 awaits owner sign-off; Security capability contracts come before the first provider or plugin; the Phase 1 state list differs between the roadmap and the specification.

## Verified baseline
- Native WinUI 3/XAML/Composition, unpackaged and framework-dependent, x64
- .NET 10 / C# 14, SDK 10.0.402 pinned
- Windows App SDK 2.5.1 Stable, BuildTools 10.0.26100.8249
- Windows SDK 10.0.26100.8249 baseline (ADR-0016)
- Core unelevated, default-deny capabilities, out-of-process plugins, restricted privileged broker (design)
- SQLite structured storage, Named Pipes initial plugin IPC (design)
- xUnit.net v3 on Microsoft Testing Platform; CI on GitHub Actions `windows-2025`

## Next action
Owner merges Phase 0 and tags it after the CI run on the exit commit is green; then Phase 1 starts in a fresh chat from NEXT_PHASE.md.

## Rules
Only record verified implementation facts. Do not mark code complete because it exists. Require build + tests + relevant manual verification.

## Rollback point
`main` at commit 043d875 (V3.0 documentation freeze) until Phase 0 is merged; afterwards the `phase-0-complete` tag.
