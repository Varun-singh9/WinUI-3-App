# Phase Status — V3.0

- Current phase: Phase 1 — Core + Exact Command Bar
- Status: IN PROGRESS — branch `phase-1-core-bar`, started 2026-10-23. No Phase 1 code exists yet.
- Objective: make the launcher open: global hotkey, single instance, active-monitor placement, the activation visual with the readiness coordinator, and the exact black rounded command bar with keyboard-first input and tray access (NEXT_PHASE.md).
- Phase 0: COMPLETE — merged to `main` (merge commit 13482a5) and tagged `phase-0-complete` on 2026-10-22 (ADR-0022). Its exit review is kept below.

## Phase 1 progress

| Task (NEXT_PHASE.md) | State |
|---|---|
| 1. Global hotkey | Designed (ADR-0024). Contract added in Patch 2 (shape only; awaiting the owner's build/test run). Implementation is Patch 4 |
| 2. Single-instance ownership | Designed (ADR-0024). Contract added in Patch 2 (shape only; awaiting the owner's build/test run). Implementation is Patch 3 |
| 3. Active-monitor placement, bottom gap, DPI | Planned |
| 4. Activation visual and readiness coordinator | Planned (open point: what moves Morphing to Ready, ADR-0023) |
| 5. Command bar UI, tray access | Planned |
| 6. Lifecycle implementation behind Core contracts | Planned (location decided in ADR-0024) |
| 7. Diagnostics wiring and log location | Planned |
| 8. Test projects with first real tests | Planned (`Launcher.Integration.Tests` in Task 1, Patch 3) |

## Phase 1 decisions (2026-10-23)
- ADR-0023: `LauncherState` stays as is; the roadmap state list is a presentation vocabulary (closes carried-forward item 4).
- ADR-0024: orchestration in Core, Win32 hotkey and single-instance in Launcher.Integrations, wired in Launcher.App; two new Core interfaces `IHotkeyService` and `ISingleInstanceOwner`; per-user named mutex plus an `EventWaitHandle` signal; default hotkey Ctrl+Win+Alt+Space, injected; no new packages.

## Task 1 patch plan (one patch each, applied by the owner, then restore/build/test output pasted)
1. Documentation only: ADR-0023, ADR-0024, state files, stale comment fixes.
2. The two new Core interfaces with result and event types, plus contract-shape tests (existing pinned tests unchanged).
3. Single-instance implementation in Launcher.Integrations and the new `tests/Launcher.Integration.Tests` project with its first real tests.
4. Hotkey service implementation and its tests.

Contract additions made under ADR-0018's "record each addition" rule (all in `Launcher.Core`, namespace `Launcher.Core.Integrations`; shapes only, no behavior):
- Patch 2: `IHotkeyService` (`RegisterAsync`, `UnregisterAsync`, `Current`, `Pressed`), `HotkeyGesture`, `HotkeyModifiers`, `HotkeyPressedEventArgs`, `HotkeyRegistrationResult`, `HotkeyRegistrationStatus`; `ISingleInstanceOwner` (`TryAcquire`, `TrySignalOwner`, `Release`, `IsOwner`, `ActivationRequested`), `SingleInstanceResult`, `SingleInstanceStatus`. The ten pinned Core interfaces and every existing contract-shape test are unchanged; the `ContractChecks` helper methods were made `internal` (visibility only) so the new checks reuse them.

## Phase 1 evidence (from output pasted by the owner)
- Patch 1 (docs and comment fixes) applied on the owner machine on 2026-10-23: `dotnet restore`, `dotnet build -c Debug` (16 projects) and `dotnet test -c Debug` succeeded, 23 of 23 tests passed. Release build was not run for this patch.

## Exit review (V3.0 Phase 0 exit gate)

| Gate | Evidence | Result |
|---|---|---|
| Solution restores, builds and tests | Owner machine, output pasted: SDK 10.0.402, `dotnet restore`, `dotnet build` in Debug and Release (16 projects including `Launcher.App` and `Launcher.Core.Tests`), `dotnet test` 23 of 23 passed | PASS |
| Exact toolchain and deployment values recorded | ADR-0014 (and addenda), ADR-0015, ADR-0016, ADR-0017; DEPENDENCIES.md; `global.json`, `Directory.Build.props`, `Directory.Packages.props` | PASS |
| Project references comply with the approved direction | `tests/Launcher.Core.Tests/DependencyRules.cs` enforced by 7 architecture tests (ADR-0018, ADR-0019); tests pass | PASS |
| CI smoke evidence | GitHub Actions run #1, commit 36a445c, `build-and-test (Debug)` 2m 6s and `build-and-test (Release)` 1m 47s both successful (owner screenshot) | PASS |
| Phase 1 handoff contains evidence | NEXT_PHASE.md, this file, TESTS.md | PASS |

## Phase 0 scope items (roadmap bullets)

| Item | State |
|---|---|
| Core/UI lifecycle contracts and semantic state model | Done: `LauncherState` (11 states), `ActivationKind`, the ten Core interfaces |
| IPC model | Done as shapes: `IpcRequest`, `IpcResponse`, `IpcStatus` (payload encoding and status values provisional) |
| Permissions | Declared in the contracts (manifest capabilities, provider permissions, risk levels); enforcement contract carried forward (ADR-0022 item 1) |
| Provider abstraction, prefix registry, result/action model, configuration | Done as shapes (ADR-0018) |
| Logging | Done and tested: structured diagnostics (ADR-0020), not wired into the app yet |
| Readiness states | Done as lifecycle states (`CriticalReady`); the readiness coordinator is Phase 1 work |
| Motion System and Design System as UI infrastructure; cold-start activation design; optional providers never block activation; native WinUI 3 renderer rule | Documented (specification sections 7 to 9, ADR-0001, ADR-0006, contract comments); implementation is Phase 1 and later by design |

## Tests
- 23 tests in `Launcher.Core.Tests`: 7 architecture, 11 contract-shape, 5 diagnostics. All pass on the owner machine and in GitHub Actions.
- No feature tests exist because no feature code exists.
- Manual smoke check: `Launcher.App` starts and shows an empty window (owner, `dotnet run`).

## Carried forward (ADR-0022)
1. Security capability and permission-policy contracts and capability catalog, before the first provider or plugin needs them.
2. The other six test projects, with their first real tests.
3. Wire diagnostics into `Launcher.App`; decide the log file location.
4. RESOLVED (ADR-0023, 2026-10-23): roadmap vs specification Phase 1 state list; `LauncherState` unchanged.
5. ADR-0013 (coding agent) stays Proposed: the owner is deferring the decision and may switch agents; rename or genericize documents that still name ChatGPT when it is decided.
6. Confirm the deployment interpretation (ADR-0014); Arm64 evaluation and a Windows 11 24H2 test machine before release. Partial evidence: the CI runner is Windows 10.0.26100, so build and tests ran on the 24H2 build; the app itself has not been run there.
7. CI log checked: SDK 10.0.402 confirmed and the .NET CLI telemetry opt-out is set. The effect of `TESTINGPLATFORM_TELEMETRY_OPTOUT` is not visible in `dotnet --info`; confirm from the test step output or accept as unverified.
8. Provisional contract details listed in ADR-0018; `Launcher.Motion → Launcher.Core` reference; `PluginHost` and `PrivilegedBroker` remain class libraries until their phases.

## Known issues / gates
- No gate is open. Minimum OS confirmed (ADR-0015). Windows SDK build amended (ADR-0016).
- Visual Studio Test Explorer behavior with Microsoft Testing Platform has not been checked; the command line is the evidence path.

## Decisions
See `DECISIONS.md` for ADR-0001 through ADR-0024.

## Next action
Apply Patch 1 (documentation), then Patch 2 (the two new Core interfaces and their contract-shape tests). ADR-0013 stays Proposed by the owner's choice.

## Rollback point
Tag `phase-0-complete` (merge commit 13482a5 on `main`). Before Phase 0: commit 043d875, the V3.0 documentation freeze.
