# Project State — V3.0

Keep this file short. It is the live handoff state for implementation.

## Current state
- Pack: V3.0 documentation baseline
- Production source repository: solution scaffold created on branch `phase-0-foundation` (14 library projects, no feature code; `Launcher.App` not yet created)
- Current phase: Phase 0 — Architecture & Project Contract
- Phase status: IN PROGRESS (started 2026-10-21; branch `phase-0-foundation`)
- Build status: libraries smoke-built in the agent sandbox with SDK 10.0.113 and plain `net10.0` only (0 warnings, 0 errors). NOT verified with the pinned SDK 10.0.402 or the Windows target framework. No clean build on the owner machine yet.
- Test status: no production tests yet (test projects and framework not yet chosen)

## Current gate
None. The Windows 11 24H2 (build 26100) minimum OS was confirmed by the project owner on 2026-10-21 (ADR-0015).

## Open items
- Windows SDK deviation: installed 10.0.26100.8249 vs documented 10.0.26100.9457 (ADR-0014). Resolve by installing 9457 or amending the documented build via ADR.
- ADR-0013 (coding agent: ChatGPT → Claude) awaits owner sign-off.
- `Launcher.App` (WinUI) must be created from the Visual Studio template on the owner machine.
- Test framework and CI image not yet selected (dependency/ADR decision).
- Reference choices for Diagnostics, Security and Motion are provisional (see PHASE_STATUS.md).

## Verified baseline
- Native WinUI 3/XAML/Composition
- .NET 10 / C# 14
- Windows App SDK 2.5.1 Stable
- Windows SDK 10.0.26100.9457 baseline
- Core unelevated
- Default-deny capabilities
- Out-of-process plugins
- Restricted privileged broker
- SQLite structured storage
- Named Pipes initial plugin IPC

## Next action
On the owner machine: run the Phase 0 verification commands (see PHASE_STATUS.md) and create `Launcher.App` from the WinUI template with unpackaged settings. Then proceed to contracts, diagnostics, tests and CI.

## Rules
Only record verified implementation facts. Do not mark code complete because it exists. Require build + tests + relevant manual verification.

## Rollback point
V3.0 pre-implementation documentation freeze (`main` at commit 043d875).
