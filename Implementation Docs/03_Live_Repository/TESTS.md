# Test Matrix — V3.0

Result values: `TODO`, `PASS`, `FAIL`, or `BLOCKED`. Compilation alone never makes a test PASS.

| Area | Test | Result | Notes |
|---|---|---|---|
| Build | Clean restore/build | PASS | Fresh scaffold checkout; Owner machine, 2026-10-21, SDK 10.0.402, net10.0-windows10.0.26100.0; 14 projects; output pasted by owner; no warnings shown |
| Build | Debug build | PASS | Owner machine, SDK 10.0.402, net10.0-windows10.0.26100.0, output pasted: full solution (16 projects) built in Debug after the diagnostics patch; also built green in GitHub Actions (Debug job) |
| Build | Release build | PASS | Owner machine, SDK 10.0.402, net10.0-windows10.0.26100.0, output pasted: full solution (16 projects including Launcher.App and Launcher.Core.Tests) built in Release after the diagnostics patch; also built green in GitHub Actions (Release job) |
| Build | CI-equivalent build | PASS | GitHub Actions run #1 on commit 36a445c (branch phase-0-foundation): build-and-test (Debug) success in 2m 6s and build-and-test (Release) success in 1m 47s, total 2m 8s; evidence is the owner's screenshot of the run summary. Run log (`dotnet --info`, pasted by the owner): SDK 10.0.402 from global.json, runner Windows 10.0.26100 win-x64, DOTNET_CLI_TELEMETRY_OPTOUT=1 and DOTNET_NOLOGO=1 set; TESTINGPLATFORM_TELEMETRY_OPTOUT not shown by that command Phase 1: GitHub Actions run on push d098816 (branch phase-1-core-bar, created 2026-10-25T04:34Z) concluded success according to the GitHub API, fetched by the coding agent; ci.yml runs restore, build and test in Debug and Release, so that covers both configurations; per-test counts are not visible in the API response. The earlier run on a2ec042 (2026-10-24) also concluded success |
| Architecture | Project references obey dependency direction | PASS | Owner machine, 2026-10-21: `dotnet test Launcher.sln -c Debug` ran Launcher.Core.Tests, 6 passed, 0 failed (output pasted). Covers the allowed-reference table, Core isolation, no references to the app, host/broker isolation and no cycles. Re-run after the ADR-0019 rule update (2026-10-21, output pasted): `dotnet test Launcher.sln -c Debug`, 18 tests in Launcher.Core.Tests passed, including 7 architecture checks |
| Architecture | Contract shapes match specification | PASS | Owner machine, 2026-10-21 (output pasted): 11 contract-shape checks passed as part of 18 of 18 |
| Architecture | Hotkey and single-instance contract shapes match ADR-0024 | PASS | Owner machine, 2026-10-23 (output pasted): after Patch 2, `dotnet test Launcher.sln -c Debug` ran 30 tests in Launcher.Core.Tests, 30 passed, 0 failed (23 earlier plus the 7 new `IntegrationContractTests`) |
| Architecture | Lifecycle contract shapes match ADR-0026 | PASS | Owner machine, 2026-10-25 (output pasted): after Task 6 Patch 2, `dotnet test Launcher.sln -c Debug` ran both test assemblies, 80 tests total, 80 passed, 0 failed, 0 skipped (73 earlier plus the 7 new `LifecycleContractTests`) |
| Lifecycle | Transition table matches ADR-0026 exactly | PASS | Owner machine, 2026-10-26 (output pasted): after Task 6 Patch 3, `dotnet test Launcher.sln -c Debug` ran both test assemblies, 91 tests total, 91 passed, 0 failed, 0 skipped (80 earlier plus the 11 new `LauncherLifecycleTests`; all 121 state pairs checked against the 15 legal transitions) |
| Lifecycle | Cold and warm paths work, illegal and repeated transitions are rejected and logged, never thrown | PASS | Owner machine, 2026-10-26 (output pasted): after Task 6 Patch 3, `dotnet test Launcher.sln -c Debug` ran both test assemblies, 91 tests total, 91 passed, 0 failed, 0 skipped (`LauncherLifecycleTests`) |
| Lifecycle | Events arrive in commit order, a throwing handler is isolated, re-entrant and concurrent transitions are safe | PASS | Owner machine, 2026-10-26 (output pasted): after Task 6 Patch 3, `dotnet test Launcher.sln -c Debug` ran both test assemblies, 91 tests total, 91 passed, 0 failed, 0 skipped (`LauncherLifecycleTests`, including the 8-thread concurrency test) |
| Readiness | Critical gate: all critical ready, failure, recovery, timeout and cancellation return typed results | TODO | Phase 1 Task 6 Patch 4: `ReadinessCoordinatorTests`. PASS only from the owner's pasted output or CI |
| Readiness | Optional capabilities never gate or fail activation | TODO | Phase 1 Task 6 Patch 4: `ReadinessCoordinatorTests`. PASS only from the owner's pasted output or CI |
| Readiness | Concurrent reports, multiple waiters, and bad arguments | TODO | Phase 1 Task 6 Patch 4: `ReadinessCoordinatorTests`. PASS only from the owner's pasted output or CI |
| Diagnostics | Level filter, redaction, sink isolation, JSON-lines file, roll over | PASS | Owner machine, output pasted: 23 of 23 tests passed, including 5 diagnostics tests; also green in GitHub Actions |
| Runtime | Single-instance behavior | PASS | Owner machine, 2026-10-23 (output pasted): after Patch 3, `dotnet test Launcher.sln -c Debug` ran both test assemblies, 48 tests total, 48 passed, 0 failed, 0 skipped (`SingleInstanceOwnerTests`: owner, second instance, repeat acquire, release, signal raises `ActivationRequested`, no-owner and after-release signal return false) |
| Runtime | Single-instance failure is typed and logged, failing handler cannot crash the process | PASS | Owner machine, 2026-10-23 (output pasted): after Patch 3, `dotnet test Launcher.sln -c Debug` ran both test assemblies, 48 tests total, 48 passed, 0 failed, 0 skipped (`SingleInstanceOwnerTests`) |
| Security | Single-instance mutex and signal event are limited to the current user | PASS | Owner machine, 2026-10-23 (output pasted): after Patch 3, `dotnet test Launcher.sln -c Debug` ran both test assemblies, 48 tests total, 48 passed, 0 failed, 0 skipped (ACL tests in `SingleInstanceOwnerTests`, name validation in `SingleInstanceNamesTests`) |
| Hotkey | Opens launcher | TODO | |
| Hotkey | Registration result is typed and logged, failed replacement keeps the old hotkey, stale presses are ignored, a throwing handler cannot end the loop | PASS | Owner machine, 2026-10-23 (output pasted): after Patch 4, `dotnet test Launcher.sln -c Debug` ran both test assemblies, 73 tests total, 73 passed, 0 failed, 0 skipped (`HotkeyServiceTests`, fake system layer with the real message loop) |
| Hotkey | Real registration is released, real conflict reports in use | PASS | Owner machine, 2026-10-23 (output pasted): after Patch 4, `dotnet test Launcher.sln -c Debug` ran both test assemblies, 73 tests total, 73 passed, 0 failed, 0 skipped. Because nothing was skipped, both `RealHotkeyTests` ran against the real Windows hotkey API with Ctrl+Alt+Shift+F24 |
| Hotkey | A real key press reaches the launcher | TODO | Manual, after the hotkey is wired into Launcher.App in a later Phase 1 task. Patch 4 tests post a synthetic WM_HOTKEY and do not prove real key delivery |
| Activation | Cold loop starts immediately | TODO | |
| Activation | Cold loop → critical readiness → morph | TODO | |
| Activation | Warm open | TODO | |
| Activation | Optional providers never block bar | TODO | |
| Monitor | Active-monitor placement | TODO | |
| DPI | Mixed DPI/scaling | TODO | |
| Prefix | Parser | TODO | |
| Prefix | Text + symbol aliases resolve identically | TODO | |
| Prefix | Unknown prefix fallback | TODO | |
| Prefix | Popup keyboard navigation | TODO | |
| Prefix | Popup pointer interaction | TODO | |
| Menu | Customization interaction | TODO | |
| Menu | Customization without hover | TODO | |
| Search | Fuzzy matching | TODO | |
| Search | Central ranking | TODO | |
| Search | Progressive results | TODO | |
| Search | Cancellation | TODO | |
| Search | Stale result protection | TODO | |
| Provider | Health states | TODO | |
| Provider | Optional fallback | TODO | |
| Provider | Crash isolation | TODO | |
| Actions | Primary action | TODO | |
| Actions | Argument validation | TODO | |
| Actions | Medium+ confirmation | TODO | |
| Actions | No animation-only execution delay | TODO | |
| Security | Default-deny capability | TODO | |
| Security | Invalid capability rejected | TODO | |
| Security | Plugin cannot self-expand permissions | TODO | |
| Security | Malformed IPC rejected | TODO | |
| Security | IPC timeout/cancellation | TODO | |
| Security | Plugin death handled | TODO | |
| Security | Broker allowlist | TODO | |
| Security | No silent elevation | TODO | |
| Data | Configuration validation | TODO | |
| Data | Database migration | TODO | |
| Data | Migration recovery | TODO | |
| Data | Export/import round-trip | TODO | |
| Data | Reset behavior | TODO | |
| Privacy | No unintended network activity | TODO | |
| Privacy | Search exclusions/private mode | TODO | |
| Privacy | Secrets not in plain configuration | TODO | |
| UI | Keyboard-only operation | TODO | |
| UI | Focus restoration | TODO | |
| UI | UI Automation names/roles | TODO | |
| Accessibility | High contrast/system accessibility | TODO | |
| Accessibility | Reduced Motion | TODO | |
| Motion | Activation performance | TODO | |
| Motion | Prefix popup animation | TODO | |
| Motion | Customization menu animation | TODO | |
| Motion | Cursor-reactive fallback | TODO | |
| Motion | Cursor-reactive field never changes hit targets | TODO | |
| Performance | Cold activation benchmark | TODO | |
| Performance | Warm activation benchmark | TODO | |
| Performance | Search latency benchmark | TODO | |
| Performance | Idle memory baseline | TODO | |
| Performance | Idle CPU baseline | TODO | |
| Windows | Supported OS matrix | TODO | |
| Windows | API runtime guards | TODO | |
| Installer | Clean install | TODO | |
| Installer | Upgrade | TODO | |
| Installer | Repair | TODO | |
| Installer | Uninstall | TODO | |
| Installer | User-data preservation | TODO | |
| Release | Application/installer signing | TODO | |
| Release | Dependency/license inventory | TODO | |
| Release | User-initiated verified update/recovery | TODO | |
| Release | Rollback | TODO | |
| Release | SHA-256 artifact integrity | TODO | |
| Release | Authenticode publisher verification | TODO | |
| Workflow | Privileged workflow step unavailable before Phase 7 | TODO | |
| Workflow | Denied workflow step terminates visibly | TODO | |

## Rule
A phase is complete only after the applicable automated tests, manual verification, performance checks and security checks pass.
