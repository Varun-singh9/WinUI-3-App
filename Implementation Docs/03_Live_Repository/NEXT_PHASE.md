# Next Phase Handoff — Phase 1: Core + Exact Command Bar

## Goal
Make the launcher open: global hotkey, single instance, active-monitor placement, the activation visual with the readiness coordinator, and the exact black rounded command bar with keyboard-first input and tray access. Implement the semantic lifecycle behind the existing contracts without adding Phase 2+ scope. (Roadmap Phase 1; Questline Q01 and Q02.)

## Preconditions
- Phase 0 is complete and merged to `main`, tagged `phase-0-complete` (see PHASE_STATUS.md and ADR-0022). Check the repository, not this file, for whether that happened.
- Read, in order: root `README.md`, the Constitution, the Project Overview, `Implementation_Specification.docx` (sections 4 to 9 and 14), `DECISIONS.md`, `PROJECT_STATE.md`, `PHASE_STATUS.md`, `DEPENDENCIES.md`, `TESTS.md`.
- Clone the repository, read `git log`, and compare the live state files with the repository before starting.

## Resolved (ADR-0023, ADR-0024)
The roadmap/specification state-list difference is resolved: `LauncherState` is unchanged and the roadmap list is a presentation vocabulary (ADR-0023). Task 1 (hotkey and single instance) is designed in ADR-0024: orchestration in Core, Win32 pieces in Launcher.Integrations, wired in Launcher.App. The owner chose to implement Task 6 next (ADR-0025) and the lifecycle design is recorded in ADR-0026. The `Morphing` to `Ready` rule is provisional in ADR-0026 and Task 4 revisits it. The order of the remaining tasks is decided at the Task 6 exit review.

## Order (ADR-0025, ADR-0027)
Task 6 is done as library code (ADR-0026). The order of the remaining tasks is 7, then 3, then 5, then 4, with Task 8 throughout. The numbering below is the original list, not the order.

## Tasks (from the roadmap and specification)
1. Global hotkey through a dedicated integration component; explicit, recoverable registration failure.
2. Single-instance ownership.
3. Active-monitor placement, fixed bottom display gap, DPI awareness.
4. Activation visual layer and readiness coordinator: cold activation loops while critical startup runs, then morphs into the bar; warm activation reuses existing state. Optional providers never block activation. (The readiness coordinator is implemented in Task 6, ADR-0026.)
5. Command bar UI: exact black rounded bar, keyboard-first input, focus returned to the input, tray access.
6. Implement the lifecycle (`IApplicationRuntime`, `ILauncherLifecycle`, `IActivationCoordinator`) behind the Phase 0 contracts; semantic states drive the UI, and business code never starts animations directly. (Done as library code, verified by automated tests; wiring it into the app is Task 7.)
7. Wire diagnostics into the app and decide the log file location through the data layer; distinguish cold and warm activation in diagnostics. (Planned in ADR-0028: reuse the existing diagnostics entries, no new logging mechanism.)
8. Create the test projects that gain their first real tests, and add the corresponding `TESTS.md` rows.

## Do not build yet
- Prefix parsing, aliases and the prefix popup (Phase 2).
- Search, fuzzy matching, ranking, providers (Phases 3 and 4).
- Actions, the action rail and the customization menu (Phase 5).
- Clipboard, snippets, workflows, scripts, elevation and the broker (Phases 6 and 7).
- Cursor-reactive motion and motion profiles beyond what the activation visual needs.
- WebView2 or any web runtime as a UI renderer; anything on the deferred list in the Extension Pack.

## Do not change
Core ≠ Feature; UI ≠ Functionality; native WinUI 3/XAML/Composition; out-of-process capability-limited plugins; unelevated Core; default-deny capabilities; semantic Motion System; critical readiness before the interactive bar; optional services never block activation; the project reference rules in `tests/Launcher.Core.Tests/DependencyRules.cs` (changes need an ADR).

## Completion evidence
Phase 1 is complete only with: automated tests for single-instance behavior, hotkey activation, cold and warm activation and optional-provider non-blocking; manual verification of monitor placement and mixed DPI; activation performance measurements against the TESTS.md performance rows; green restore, build and test on the owner machine and in CI; updated state files and a handoff with evidence for Phase 2.
