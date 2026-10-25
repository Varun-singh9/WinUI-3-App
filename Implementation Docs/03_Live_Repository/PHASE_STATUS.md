# Phase Status — V3.0

- Current phase: Phase 1 — Core + Exact Command Bar
- Status: IN PROGRESS — branch `phase-1-core-bar`, started 2026-10-23. Task 1 library code (single instance, hotkey) is implemented and verified on the owner machine; nothing is wired into `Launcher.App` yet.
- Objective: make the launcher open: global hotkey, single instance, active-monitor placement, the activation visual with the readiness coordinator, and the exact black rounded command bar with keyboard-first input and tray access (NEXT_PHASE.md).
- Phase 0: COMPLETE — merged to `main` (merge commit 13482a5) and tagged `phase-0-complete` on 2026-10-22 (ADR-0022). Its exit review is kept below.

## Phase 1 progress

| Task (NEXT_PHASE.md) | State |
|---|---|
| 1. Global hotkey | Implemented and verified on the owner machine (Patch 4, 73 of 73 tests, 0 skipped, so the two real-hotkey tests ran); not wired into Launcher.App; Ctrl+Win+Alt+Space not yet tried on this machine |
| 2. Single-instance ownership | Implemented and verified on the owner machine (Patch 3, 48 of 48 tests passed); not wired into Launcher.App |
| 3. Active-monitor placement, bottom gap, DPI | Planned |
| 4. Activation visual and readiness coordinator | Planned. The readiness coordinator moved to Task 6 (ADR-0026). The visual and the Morphing-to-Ready rule stay here, and Task 4 revisits the provisional timing (ADR-0026 item 7) |
| 5. Command bar UI, tray access | Planned |
| 6. Lifecycle implementation behind Core contracts | Next, by owner decision (ADR-0025). Design recorded in ADR-0026; Patch 1 of 6 (docs only) in progress |
| 7. Diagnostics wiring and log location | Planned |
| 8. Test projects with first real tests | `Launcher.Integration.Tests` verified (Patch 4, 73 of 73 across both assemblies) |

## Phase 1 decisions (2026-10-23)
- ADR-0023: `LauncherState` stays as is; the roadmap state list is a presentation vocabulary (closes carried-forward item 4).
- ADR-0024: orchestration in Core, Win32 hotkey and single-instance in Launcher.Integrations, wired in Launcher.App; two new Core interfaces `IHotkeyService` and `ISingleInstanceOwner`; per-user named mutex plus an `EventWaitHandle` signal; default hotkey Ctrl+Win+Alt+Space, injected; no new packages.
- ADR-0025: the owner chose to implement Task 6 (lifecycle) next, ahead of Tasks 3 to 5 and 7. `NEXT_PHASE.md`, specification section 31 and the roadmap disagree about order, so the deviation and the agent's Task 3 recommendation are recorded. The remaining order is decided at the Task 6 exit review.
- ADR-0026: lifecycle design: Core components, a Phase 1 transition table, cold/warm definition, `IReadinessCoordinator`, `StartAsync` returning `ApplicationStartResult`, provisional presentation timing, failure policy.

## Task 6 patch plan (one patch each; the owner applies it and pastes restore, build and test output before the next)
1. Documentation only: ADR-0025, ADR-0026, state files.
2. New contracts and types (`IReadinessCoordinator`, `ApplicationRuntimeOptions`, `ApplicationStartResult` and its status types, the changed `StartAsync` signature) with contract-shape tests; existing pinned tests unchanged.
3. `LauncherLifecycle` state machine with transition-table, ordering, concurrency and handler-isolation tests.
4. `ReadinessCoordinator` with success, failure, timeout, cancellation and optional-never-gates tests.
5. `ActivationCoordinator` with cold and warm sequence, deactivate, double activate and timeout-to-ErrorRecovery tests.
6. `ApplicationRuntime` (start order, other instance, hotkey failure, shutdown, restart, optional integration not blocking) with tests, then the exit evidence for Task 6.
Nothing is wired into `Launcher.App` in Task 6; wiring and the first manual check come with Task 7.

## Task 1 patch plan (one patch each, applied by the owner, then restore/build/test output pasted)
1. Documentation only: ADR-0023, ADR-0024, state files, stale comment fixes.
2. The two new Core interfaces with result and event types, plus contract-shape tests (existing pinned tests unchanged).
3. Single-instance implementation in Launcher.Integrations and the new `tests/Launcher.Integration.Tests` project with its first real tests.
4. Hotkey service implementation and its tests.

Contract additions made under ADR-0018's "record each addition" rule (all in `Launcher.Core`, namespace `Launcher.Core.Integrations`; shapes only, no behavior):
- Patch 2: `IHotkeyService` (`RegisterAsync`, `UnregisterAsync`, `Current`, `Pressed`), `HotkeyGesture`, `HotkeyModifiers`, `HotkeyPressedEventArgs`, `HotkeyRegistrationResult`, `HotkeyRegistrationStatus`; `ISingleInstanceOwner` (`TryAcquire`, `TrySignalOwner`, `Release`, `IsOwner`, `ActivationRequested`), `SingleInstanceResult`, `SingleInstanceStatus`. The ten pinned Core interfaces and every existing contract-shape test are unchanged; the `ContractChecks` helper methods were made `internal` (visibility only) so the new checks reuse them.

## Phase 1 evidence (from output pasted by the owner)
- Patch 1 (docs and comment fixes) applied on the owner machine on 2026-10-23: `dotnet restore`, `dotnet build -c Debug` (16 projects) and `dotnet test -c Debug` succeeded, 23 of 23 tests passed. Release build was not run for this patch.
- Patch 2 (hotkey and single-instance contracts plus contract-shape tests) applied on the owner machine on 2026-10-23: `dotnet restore`, `dotnet build -c Debug` (16 projects) and `dotnet test -c Debug` succeeded, 30 of 30 tests passed. Release build was not run for this patch.
- Patch 3 (single-instance implementation and the new `Launcher.Integration.Tests` project) applied on the owner machine on 2026-10-23: `dotnet restore`, `dotnet build -c Debug` (17 projects) and `dotnet test -c Debug` succeeded; both test assemblies passed, 48 of 48 tests, 0 failed, 0 skipped. Release build was not run for this patch.
- Patch 4 (hotkey service and tests) applied on the owner machine on 2026-10-23: `dotnet restore`, `dotnet build -c Debug` (17 projects) and `dotnet test -c Debug` succeeded; both test assemblies passed, 73 of 73 tests, 0 failed, 0 skipped, so the two `RealHotkeyTests` ran against the real Windows hotkey API. Release build was not run for this patch.

## Patch 3 implementation notes (verified by the owner's run, see evidence above)
- `SingleInstanceOwner` and `SingleInstanceNames` in `Launcher.Integrations/SingleInstance`; no new packages (the ACL APIs are part of the shared framework).
- Ownership is whoever creates the `Local\` named mutex first; the mutex is never waited on, so it has no thread affinity and the OS removes it when the process ends. The event is created before the mutex so a second instance can never signal into a gap. Both objects use an ACL admitting only the current user. Names are injected and must start with `Local\`.
- Failures return `SingleInstanceResult` with a stable code and are logged: `single_instance.access_denied`, `single_instance.create_failed`, `single_instance.signal_failed`, `single_instance.handler_failed`. A handler that throws is logged by exception type only and cannot end the process.
- New test project `tests/Launcher.Integration.Tests` (references Core, Integrations, Diagnostics). Expected after this patch: 48 tests (30 in Launcher.Core.Tests plus 18 here).

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

## Patch 4 implementation notes (verified by the owner's run, see evidence above)
- `HotkeyService`, `HotkeyDefaults`, an internal `IHotkeyNative` seam with the real `Win32HotkeyNative`, and hand-written user32/kernel32 declarations in `Launcher.Integrations/Hotkey`. No new packages, no keyboard hook, no input interception.
- One background message-loop thread (started on the first registration) owns the registration. `RegisterHotKey` is called with no window handle and `MOD_NOREPEAT`. A new gesture is registered before the old one is released, so a failed replacement keeps the old hotkey. A press queued for an old registration is ignored. A throwing `Pressed` handler is logged by exception type only and cannot end the loop or the process.
- Typed results and codes: `hotkey.in_use` (Win32 1409), `hotkey.invalid_gesture` (87 or a rejected gesture: no modifier, no key, a modifier key as the main key, key outside 1-254), `hotkey.register_failed`, `hotkey.disposed`, `hotkey.loop_failed`, `hotkey.handler_failed`. Failures are logged with the Win32 error number.
- Default gesture Ctrl+Win+Alt+Space lives in `HotkeyDefaults.LauncherGesture` and is passed in by the caller. It is not verified on the owner machine until the hotkey is wired in.
- Deviation recorded in ADR-0024: thread-queue registration instead of a message-only window.
- Tests: 25 new in `Launcher.Integration.Tests` (23 in `HotkeyServiceTests` with a fake system layer and the real message loop, 2 in `RealHotkeyTests` that use Ctrl+Alt+Shift+F24, always release it, and skip with a reason if a real hotkey cannot be registered). Presses are simulated by posting `WM_HOTKEY`, so real key delivery is not proven until a manual check after wiring. Expected total after this patch: 73 tests (30 + 43), with the 2 real tests possibly skipped. Result: 73 passed, 0 skipped. Still unverified: Ctrl+Win+Alt+Space on the owner machine, and a real key press reaching the app (both need the wiring into Launcher.App).

## Next action
Apply Task 6 Patch 1 (documentation), then Patch 2 (contracts and shape tests). No code changes in Patch 1. ADR-0013 stays Proposed by the owner's choice.

## Rollback point
Tag `phase-0-complete` (merge commit 13482a5 on `main`). Before Phase 0: commit 043d875, the V3.0 documentation freeze.
