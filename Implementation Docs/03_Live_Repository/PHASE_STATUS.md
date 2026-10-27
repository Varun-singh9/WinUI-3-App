# Phase Status — V3.0

- Current phase: Phase 1 — Core + Exact Command Bar
- Status: IN PROGRESS — branch `phase-1-core-bar`, started 2026-10-23. Tasks 1, 2 and 6 are implemented as library code and verified by automated tests on the owner machine (142 of 142); nothing is wired into `Launcher.App` yet.
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
| 6. Lifecycle implementation behind Core contracts | Implemented and verified by automated tests on the owner machine (Patches 1 to 6, 142 of 142). Not verified end to end: nothing is wired into Launcher.App (Task 7). Exit review below |
| 7. Diagnostics wiring and log location | Next (ADR-0027); includes the composition root. Plan first |
| 8. Test projects with first real tests | `Launcher.Integration.Tests` verified (Patch 4, 73 of 73 across both assemblies) |

Order after Task 6 (ADR-0027): 7, then 3, 5, 4; Task 8 throughout. The task numbers are the original list, not the order.

## Phase 1 decisions (2026-10-23)
- ADR-0023: `LauncherState` stays as is; the roadmap state list is a presentation vocabulary (closes carried-forward item 4).
- ADR-0024: orchestration in Core, Win32 hotkey and single-instance in Launcher.Integrations, wired in Launcher.App; two new Core interfaces `IHotkeyService` and `ISingleInstanceOwner`; per-user named mutex plus an `EventWaitHandle` signal; default hotkey Ctrl+Win+Alt+Space, injected; no new packages.
- ADR-0025: the owner chose to implement Task 6 (lifecycle) next, ahead of Tasks 3 to 5 and 7. `NEXT_PHASE.md`, specification section 31 and the roadmap disagree about order, so the deviation and the agent's Task 3 recommendation are recorded. The remaining order is decided at the Task 6 exit review.
- ADR-0026: lifecycle design: Core components, a Phase 1 transition table, cold/warm definition, `IReadinessCoordinator`, `StartAsync` returning `ApplicationStartResult`, provisional presentation timing, failure policy.
- ADR-0027 (2026-10-26): order after Task 6 is 7, 3, 5, 4 with Task 8 throughout, based on specification section 31 and the Questline; approved by the owner.

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
- Task 6 Patch 2 (ADR-0026, all in `Launcher.Core`, namespace `Launcher.Core.Lifecycle`; shapes only): new `IReadinessCoordinator` (`IsCriticalReady`, `ReportReady`, `ReportFailed`, `WaitForCriticalAsync`), `ReadinessStatus` (Ready, Failed, TimedOut, Cancelled), `ReadinessResult`, `ApplicationRuntimeOptions` (with the owner-approved 5 s `DefaultCriticalReadinessTimeout`), `ApplicationStartStatus` (Started, AnotherInstanceRunning) and `ApplicationStartResult`. Changed: `IApplicationRuntime.StartAsync` now returns `Task<ApplicationStartResult>` (a change beyond ADR-0018's additions, recorded in ADR-0026 item 6; nothing used the old shape). The existing pinned contract-shape tests are unchanged.

## Phase 1 evidence (from output pasted by the owner)
- Patch 1 (docs and comment fixes) applied on the owner machine on 2026-10-23: `dotnet restore`, `dotnet build -c Debug` (16 projects) and `dotnet test -c Debug` succeeded, 23 of 23 tests passed. Release build was not run for this patch.
- Patch 2 (hotkey and single-instance contracts plus contract-shape tests) applied on the owner machine on 2026-10-23: `dotnet restore`, `dotnet build -c Debug` (16 projects) and `dotnet test -c Debug` succeeded, 30 of 30 tests passed. Release build was not run for this patch.
- Patch 3 (single-instance implementation and the new `Launcher.Integration.Tests` project) applied on the owner machine on 2026-10-23: `dotnet restore`, `dotnet build -c Debug` (17 projects) and `dotnet test -c Debug` succeeded; both test assemblies passed, 48 of 48 tests, 0 failed, 0 skipped. Release build was not run for this patch.
- Patch 4 (hotkey service and tests) applied on the owner machine on 2026-10-23: `dotnet restore`, `dotnet build -c Debug` (17 projects) and `dotnet test -c Debug` succeeded; both test assemblies passed, 73 of 73 tests, 0 failed, 0 skipped, so the two `RealHotkeyTests` ran against the real Windows hotkey API. Release build was not run for this patch.
- Task 6 Patch 1 (ADR-0025 and ADR-0026, docs only) applied by the owner and pushed. Verified by the coding agent against the remote on 2026-10-25: origin/phase-1-core-bar holds all six Phase 1 commits with content identical to the agent's tree, `main` is unchanged at f7d4175, and tag `phase-0-complete` exists. CI: GitHub Actions run on push d098816 (branch phase-1-core-bar, created 2026-10-25T04:34Z) concluded success according to the GitHub API, fetched by the coding agent; ci.yml runs restore, build and test in Debug and Release, so that covers both configurations; per-test counts are not visible in the API response. The earlier run on a2ec042 (2026-10-24) also concluded success.
- Task 6 Patch 2 (lifecycle contracts and shape tests) applied on the owner machine on 2026-10-25: `dotnet restore`, `dotnet build -c Debug` (17 projects) and `dotnet test -c Debug` succeeded; both test assemblies passed, 80 of 80 tests, 0 failed, 0 skipped. Release build was not run for this patch.
- Task 6 Patch 3 (`LauncherLifecycle` state machine) applied on the owner machine on 2026-10-26: `dotnet restore`, `dotnet build -c Debug` (17 projects) and `dotnet test -c Debug` succeeded; both test assemblies passed, 91 of 91 tests, 0 failed, 0 skipped. Release build was not run for this patch.
- Task 6 Patch 4 (`ReadinessCoordinator`) applied on the owner machine on 2026-10-26: `dotnet restore`, `dotnet build -c Debug` (17 projects) and `dotnet test -c Debug` succeeded; both test assemblies passed, 107 of 107 tests, 0 failed, 0 skipped. Release build was not run for this patch.
- Task 6 Patch 5 (`ActivationCoordinator`) applied on the owner machine on 2026-10-26: `dotnet restore`, `dotnet build -c Debug` (17 projects) and `dotnet test -c Debug` succeeded; both test assemblies passed, 122 of 122 tests, 0 failed, 0 skipped. Release build was not run for this patch.
- Task 6 Patch 6 (`ApplicationRuntime`) applied on the owner machine on 2026-10-26: `dotnet restore`, `dotnet build -c Debug` (17 projects) and `dotnet test -c Debug` succeeded; both test assemblies passed, 142 of 142 tests, 0 failed, 0 skipped. Release build was not run for this patch.
- CI for the Phase 1 commits (owner's screenshot, 2026-10-26): run #11 on `49175b0`, triggered by the owner's push to `phase-1-core-bar`, concluded Success in 1m 49s, with `build-and-test (Debug)` and `build-and-test (Release)` both green. This is the first Release build and test of the Phase 1 code.

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

## Task 6 Patch 3 implementation notes (not yet verified by the owner)
- `LauncherLifecycle` (public sealed, in `Launcher.Core/Lifecycle`) implements `ILauncherLifecycle`. It starts Hidden. Its transition method is internal and `Launcher.Core` exposes internals to `Launcher.Core.Tests` only, so the UI can observe the state but never set it.
- `LifecycleTransitions` holds the 15 legal Phase 1 transitions from ADR-0026. Anything else, including entering the current state again, is rejected with a Warning log (`lifecycle.transition_rejected`, with from, to and reason) and no state change. It never throws; a blank reason is a programming error and does throw.
- Events are raised in commit order, outside the lock. A transition made inside a handler, or while another thread is delivering, is queued and delivered in order by the thread already delivering, so an event can arrive on a different thread from its cause, and `State` can be ahead of the event being delivered; handlers should read the event's `Current`. A handler that throws is logged by exception type only (`lifecycle.handler_failed`) and the other handlers still run.
- Every accepted transition is logged as `lifecycle.state_changed` with from, to and a short non-sensitive reason. A small internal `LifecycleLog` helper will be reused by the coordinators in the next patches.
- Tests: 11 new in `LauncherLifecycleTests` (`Launcher.Core.Tests`), including an exhaustive check of all state pairs against the table and an 8-thread concurrency test. Expected total after this patch: 91 tests (80 + 11).

## Task 6 Patch 4 implementation notes (not yet verified by the owner)
- `ReadinessCoordinator` (public sealed, `Launcher.Core/Lifecycle`) implements `IReadinessCoordinator`. The composition root names the critical capabilities, a positive timeout and the diagnostics when it creates it. An empty critical set is ready immediately; blank or duplicate critical names and a non-positive timeout are programming errors that throw from the constructor.
- A capability whose name is not in the critical set is optional: reports about it are logged (`readiness.capability_ready` / `readiness.capability_failed`, with the capability name and whether it is critical) but never change a wait, so an optional provider can neither block nor fail activation.
- A wait ends with the first of: all critical capabilities ready (Ready), a critical failure (Failed, with the capability and the reported code), the timeout (TimedOut, with the first still-pending critical capability and no error code, logged as `readiness.timed_out`), or cancellation (Cancelled). None of these throws. The timeout is measured from the start of each wait. A wait that is already satisfiable returns immediately, including a failure reported earlier. A failed capability that later reports ready clears its failure.
- Every wait that had to wait is logged as `readiness.wait_completed` with its status, duration and, for a failure or timeout, a Warning and the error code. Reports are logged outside the lock, so a slow diagnostics sink cannot stall readiness reports. `readiness.critical_ready` is logged once when critical readiness becomes complete.
- Tests: 16 new in `ReadinessCoordinatorTests` (`Launcher.Core.Tests`), including concurrent reports from 40 threads, several simultaneous waiters, timeout and cancellation. Expected total after this patch: 107 tests (91 + 16). Two of the new tests wait about 100 ms for a real timeout.

## Task 6 Patch 5 implementation notes (not yet verified by the owner)
- `ActivationCoordinator` (public sealed, `Launcher.Core/Lifecycle`) implements `IActivationCoordinator` on top of `LauncherLifecycle` and `IReadinessCoordinator`. It never touches UI, animation or providers.
- Cold is the first activation of the process, or any activation while critical readiness is not complete (`ActivationKind.Cold`). Cold path: Hidden, Activating, wait for critical readiness, CriticalReady, Morphing, Ready. Warm path (a later activation with critical readiness complete): Hidden to Ready. A request while the launcher is visible or in a transition changes nothing and is logged as `activation.ignored`; a concurrent request that loses the race to enter Activating is also ignored.
- Provisional presentation timing (ADR-0026 item 7): after critical readiness, Morphing and Ready follow at once, and Closing is followed by Hidden at once. Core does not wait for animation. Task 4 revisits this with its own ADR.
- A critical readiness failure or timeout enters ErrorRecovery, is logged as `activation.failed` (Error for a failure, Warning for a timeout; codes `activation.critical_readiness_failed` and `activation.critical_readiness_timed_out`, with the status and capability), then goes to Hidden. The next activation retries as cold. Cancelling the caller's token during the cold wait, or deactivating during it, returns to Hidden through Closing; the activation task then completes normally. `DeactivateAsync` is immediate and ignores its token so a shutting-down launcher can always close.
- Diagnostics: `activation.requested` (with the kind), `activation.completed` (with the kind and duration), `activation.ignored`, `activation.cancelled`, `activation.deactivated`, `activation.aborted` and `activation.failed`. Hotkey-to-ready timing from the press timestamp is not wired yet (Task 7).
- Tests: 15 new in `ActivationCoordinatorTests` (`Launcher.Core.Tests`), including 8 concurrent activations and a 6-thread activate/deactivate stress test that checks every transition is legal and chained. Expected total after this patch: 122 tests (107 + 15). One test waits about 100 ms for a real timeout.

## Task 6 Patch 6 implementation notes (verified by the owner's run, see evidence above)
- `ApplicationRuntime` (public sealed, `Launcher.Core/Lifecycle`) implements `IApplicationRuntime` using only Core interfaces: the options, the lifecycle, `IActivationCoordinator`, `IHotkeyService`, `ISingleInstanceOwner`, diagnostics, and an optional `IIntegrationManager` (null while none exists). `Lifecycle` exposes the lifecycle.
- Start order (spec section 6, ADR-0026 item 5): subscribe to the hotkey `Pressed` and second-instance `ActivationRequested` events first, acquire single-instance ownership, register the hotkey from the options, start the optional integrations on a background task, stay Hidden. A second instance (`OtherInstanceRunning`) signals the owner, unsubscribes and returns `AnotherInstanceRunning` without registering the hotkey. A single-instance `Failed` is logged as a Warning and start continues. A failed hotkey registration is logged (`runtime.hotkey_unavailable`) and carried in the result; start still succeeds and the lifecycle does not enter ErrorRecovery.
- A hotkey press or a second-instance signal returns at once and starts an observed background activation (`runtime.activation_triggered` with the source); an exception is logged as `runtime.activation_failed` by type, never lost. After shutdown, presses do nothing.
- Optional integrations run on `Task.Run`, so even one that blocks its thread cannot delay start or activation. Failures are logged by exception type only (`runtime.integrations_failed`). Shutdown cancels them and waits only a bounded time (2 s by default); a timeout is logged as `runtime.integrations_shutdown_timeout`.
- Shutdown is idempotent: stop listening, deactivate (which cancels a cold wait), cancel the run, unregister the hotkey, release ownership in a `finally`, then stop integrations. Restart is shutdown followed by start in the same process. Start, shutdown and restart are serialized. A start that is cancelled or fails part-way unsubscribes, unregisters, releases ownership, and rethrows. Starting twice returns the same result.
- Not done here, by design: nothing is wired into `Launcher.App` (Task 7); the press timestamp is not yet used for hotkey-to-ready timing (Task 7); the `IConfigurationService` is not used (options are injected).
- Tests: 20 new in `ApplicationRuntimeTests` (`Launcher.Core.Tests`) with small fakes in `RuntimeFakes.cs`. Expected total after this patch: 142 tests (122 + 20). One test waits about 100 ms for a shutdown grace period.

## Task 6 exit review (2026-10-26)
Status: implemented and verified by automated tests on the owner machine (Patches 1 to 6, 142 of 142 tests, 0 skipped). Not verified end to end: nothing is wired into `Launcher.App` until Task 7, so no real key press has reached the lifecycle and nothing can be tried by hand. Release was not run locally; GitHub Actions run #11 on 49175b0 built and tested Debug and Release successfully (owner's screenshot, per-test counts not shown).

| ADR-0026 item | Where it is implemented | Evidence |
|---|---|---|
| 1. Core components, internal transition method, internals visible to the tests only | `LauncherLifecycle`, `ReadinessCoordinator`, `ActivationCoordinator`, `ApplicationRuntime` in `Launcher.Core/Lifecycle` | Architecture tests (Core references only PluginContracts) and the lifecycle tests, all passing |
| 2. Phase 1 transition table, illegal transitions rejected and logged | `LifecycleTransitions` | `LauncherLifecycleTests`: all 121 state pairs checked against the 15 legal transitions |
| 3. Cold and warm paths, visible-state requests change nothing | `ActivationCoordinator` | `ActivationCoordinatorTests` |
| 4. `IReadinessCoordinator`, optional capabilities never gate, 5 s default timeout | `ReadinessCoordinator`, `ApplicationRuntimeOptions` | `ReadinessCoordinatorTests`, `LifecycleContractTests` (the 5 s default is pinned) |
| 5. Runtime start order, injected options, optional integration manager | `ApplicationRuntime` | `ApplicationRuntimeTests` |
| 6. `StartAsync` returns `ApplicationStartResult` | `IApplicationRuntime`, `ApplicationStartResult` | `LifecycleContractTests`, `ApplicationRuntimeTests` |
| 7. Provisional presentation timing | `ActivationCoordinator` | `ActivationCoordinatorTests`; revisited in Task 4 |
| 8. Failure policy | `ApplicationRuntime`, `ActivationCoordinator` | `ApplicationRuntimeTests`, `ActivationCoordinatorTests` |
| 9. Ordered events, handler isolation, diagnostics | `LauncherLifecycle`, `LifecycleLog` | `LauncherLifecycleTests` (including 8-thread concurrency) |

Open items carried forward (also in ADR-0027):
1. A signal that re-focuses the input when an activation arrives while the launcher is visible: proposed in Task 5.
2. The provisional presentation timing is revisited with its own ADR in Task 4.
3. Hotkey-to-ready timing from the press timestamp, the names of the critical readiness capabilities and who reports them, how a second instance exits before the UI starts, and the log location through the data layer: decided in the Task 7 plan.
4. Ctrl+Win+Alt+Space has not been tried on the owner machine, and no real key press has reached the lifecycle: the Task 7 manual check.
5. RESOLVED (2026-10-26): the owner pushed `phase-1-core-bar` (tip 49175b0) and GitHub Actions run #11 succeeded in Debug and Release (1m 49s), per the owner's screenshot; per-test counts are not shown.
6. Persisting the hotkey and other options through `IConfigurationService` is a later task; the options are injected for now.

## Next action
Plan Task 7 (composition root, diagnostics wiring, log location); plan only, no code until it is approved. ADR-0013 stays Proposed by the owner's choice.

## Rollback point
Tag `phase-0-complete` (merge commit 13482a5 on `main`). Before Phase 0: commit 043d875, the V3.0 documentation freeze.
