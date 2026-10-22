# Phase Status — V3.0

- Current phase: Phase 0 — Architecture & Project Contract
- Status: COMPLETE — merged to `main` (merge commit 13482a5) and tagged `phase-0-complete` on 2026-10-22 (ADR-0022)
- Started: 2026-10-21
- Completed: 2026-10-22
- Objective: create the production solution, pin the toolchain, establish executable contracts, and produce the first clean build.
- Next phase: Phase 1 — Core + Exact Command Bar (not started; see NEXT_PHASE.md)

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
4. Resolve the roadmap/specification difference in the Phase 1 state list.
5. ADR-0013 (coding agent) stays Proposed: the owner is deferring the decision and may switch agents; rename or genericize documents that still name ChatGPT when it is decided.
6. Confirm the deployment interpretation (ADR-0014); Arm64 evaluation and a Windows 11 24H2 test machine before release. Partial evidence: the CI runner is Windows 10.0.26100, so build and tests ran on the 24H2 build; the app itself has not been run there.
7. CI log checked: SDK 10.0.402 confirmed and the .NET CLI telemetry opt-out is set. The effect of `TESTINGPLATFORM_TELEMETRY_OPTOUT` is not visible in `dotnet --info`; confirm from the test step output or accept as unverified.
8. Provisional contract details listed in ADR-0018; `Launcher.Motion → Launcher.Core` reference; `PluginHost` and `PrivilegedBroker` remain class libraries until their phases.

## Known issues / gates
- No gate is open. Minimum OS confirmed (ADR-0015). Windows SDK build amended (ADR-0016).
- Visual Studio Test Explorer behavior with Microsoft Testing Platform has not been checked; the command line is the evidence path.

## Decisions
See `DECISIONS.md` for ADR-0001 through ADR-0022.

## Next action
Open a fresh chat for Phase 1 using NEXT_PHASE.md. Its first tasks are the carried-forward items that belong to Phase 1 (state-list decision, diagnostics wiring); ADR-0013 stays Proposed by the owner's choice.

## Rollback point
Tag `phase-0-complete` (merge commit 13482a5 on `main`). Before Phase 0: commit 043d875, the V3.0 documentation freeze.
