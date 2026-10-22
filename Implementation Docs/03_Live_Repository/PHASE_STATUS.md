# Phase Status — V3.0

- Current phase: Phase 0 — Architecture & Project Contract
- Status: IN PROGRESS
- Started: 2026-10-21 (branch `phase-0-foundation`)
- Target completion: —
- Objective: create the production solution, pin the toolchain, establish executable contracts, and produce the first clean build.

## Completed before implementation
- Product definition and canonical overview frozen.
- Architecture boundaries documented.
- Motion/interaction behavior documented.
- Security and privacy models documented.
- Testing and release gates documented.
- Implementation specification established.
- V3.0 archive reorganized and refreshed.

## Tests
- Documentation archive audit: PASS for required-file presence and package structure.
- Production code tests: 18 of 18 tests in `Launcher.Core.Tests` passed on the owner machine on 2026-10-21 (7 architecture, 11 contract-shape). No feature tests exist because no feature code exists.
- Build evidence (owner machine, 2026-10-21): `dotnet --version` = 10.0.402; `dotnet restore Launcher.sln` succeeded; `dotnet build -c Debug` and `-c Release` both succeeded for all 14 projects on `net10.0-windows10.0.26100.0`. Source: output pasted by the owner. Projects contain no code yet, so this proves the pin, target framework, central build policy and project-reference graph, not behavior.
- Earlier agent-sandbox smoke build (SDK 10.0.113, plain net10.0) is superseded by the owner-machine result.

## Phase 0 progress
- Done (documents): ADR-0013 (proposed, awaiting owner sign-off), ADR-0014 to ADR-0017 (accepted), ADR-0018 and ADR-0019 (contract placement and the two new project references, accepted); state files and dependency register updated.
- Done (scaffold, built on the pinned toolchain by the owner): `Launcher.sln`, `global.json`, `Directory.Build.props`, `Directory.Packages.props`, `nuget.config`, 14 `src/` library projects, empty `plugins/ installer/ scripts/ docs/ LICENSES/ tests/`.
- Done (2026-10-21): `Launcher.App` created from the Visual Studio WinUI template and converted to unpackaged, framework-dependent, x64 (see ADR-0014 addendum). Debug and Release builds of the full solution including `Launcher.App` reported green by the owner. The owner started the app once with `dotnet run -c Debug`; an empty window opened with no error. This is a manual smoke check only, not an automated test.
- Owner machine, 2026-10-21 (output pasted): after the Microsoft Testing Platform fix (ADR-0017 addendum) `dotnet restore`, `dotnet build -c Debug` (16 projects including `Launcher.App` and `Launcher.Core.Tests`) and `dotnet test -c Debug` all succeeded. 6 tests ran, 6 passed, 0 failed. The first `dotnet test` before the fix had failed because the .NET 10 SDK does not drive xunit.v3 4.x through VSTest.
- Done and passing (owner machine, 2026-10-21): `tests/Launcher.Core.Tests` with six dependency-direction checks (run before the ADR-0019 rule update).
- Deferred on purpose: the other six test projects from Implementation Specification §3 (created with their first real tests, so there are no placeholder tests).
- Done and passing (owner machine, 2026-10-21, output pasted: `dotnet build -c Debug` of 16 projects and `dotnet test -c Debug`, 18 of 18 passed): Phase 0 contracts. `Launcher.Core` holds the 11-state `LauncherState`, `ActivationKind`, the ten Core interfaces from Implementation Specification section 5, and the search, prefix, action, configuration and diagnostics contract types (shapes only, no behavior). `Launcher.PluginContracts` holds `PluginManifest` (12 canonical fields), `IpcRequest`, `IpcResponse` and `IpcStatus`. Project references added per ADR-0019. `Launcher.Core.Tests` has 7 architecture checks and 11 contract-shape checks.
- Agent-sandbox evidence for the contracts (SDK 10.0.113, plain net10.0, not the pinned toolchain): all 14 libraries build with warnings as errors and public XML documentation required; all architecture and contract checks pass in a console harness; deliberately breaking the contracts (a renamed state, a renamed result field, a renamed manifest field, a removed provider method, a deleted interface) and the references (Core → Search) was detected each time. The xUnit test project and `Launcher.App` could not be built in the sandbox.
- Written, not yet run on the owner machine (2026-10-21): structured diagnostics implementation in `Launcher.Diagnostics` (`DiagnosticsService`, `IDiagnosticsSink`, `MemoryDiagnosticsSink`, `JsonLinesFileSink`, metadata redaction; ADR-0020) with 5 tests. Agent-sandbox evidence: builds under the full warning policy and a harness reproduced all 5 scenarios (SDK 10.0.113, plain net10.0). Not wired into `Launcher.App` yet (Phase 1).
- Written, not yet run (2026-10-21): CI workflow `.github/workflows/ci.yml` (ADR-0021). The YAML parses and its steps and pinned action hashes were checked, but it has never run on GitHub.
- Not done: first CI run result, a Release-configuration run of the full solution with the app and tests (CI runs one), `Launcher.App` project references, the other six test projects, Security capability/permission contracts, Phase 0 exit review.

## Provisional scaffold decisions
- `Launcher.PluginHost` and `Launcher.PrivilegedBroker` are class libraries for now; they become executables when their phases begin.
- `Launcher.Motion` references Core (for the semantic `LauncherState`); confirm when the Motion System is implemented.
- Ratified by ADR-0018 and ADR-0019: contract placement, Core → PluginContracts, Diagnostics → Core, Security and PluginContracts as leaf projects.
- Contract details marked provisional in ADR-0018 (cost scale, lifecycle modes, IPC status values and payload encoding, manifest list element types, action descriptor fields, coordinator streaming shape).
- Live-repository documents stay in `Implementation Docs/03_Live_Repository/`. The root-level `DECISIONS.md`, `DEPENDENCIES.md` and `CHANGELOG.md` shown in Implementation Specification §3 are not duplicated, to avoid two sources of truth.

## Known issues / gates
- No gate is open. Minimum OS confirmed (ADR-0015).
- Windows SDK build number amended to 10.0.26100.8249 (ADR-0016): no open toolchain deviation.
- Deviations from the Visual Studio WinUI template are recorded in the ADR-0014 addendum.
- No production behavior beyond the diagnostics implementation. Restore, Debug build and 18 tests are verified on the owner machine; the diagnostics tests and the CI workflow are not yet verified, and no CI run exists yet.

## Decisions
See `DECISIONS.md` for ADR-0001 through ADR-0021 and subsequent records.

## Next action
Owner: apply the diagnostics and CI patch, then `dotnet restore`, `dotnet build -c Debug`, `dotnet build -c Release` and `dotnet test -c Debug` (expect 23 tests), push, and paste the output plus the result of the GitHub Actions run. Then the Phase 0 exit review against the roadmap checklist, then merge to `main` and tag `phase-0-complete`.

## Rollback point
V3.0 pre-implementation documentation freeze.
