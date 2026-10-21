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
- Production code tests: NOT STARTED.
- Build evidence (owner machine, 2026-10-21): `dotnet --version` = 10.0.402; `dotnet restore Launcher.sln` succeeded; `dotnet build -c Debug` and `-c Release` both succeeded for all 14 projects on `net10.0-windows10.0.26100.0`. Source: output pasted by the owner. Projects contain no code yet, so this proves the pin, target framework, central build policy and project-reference graph, not behavior.
- Earlier agent-sandbox smoke build (SDK 10.0.113, plain net10.0) is superseded by the owner-machine result.

## Phase 0 progress
- Done (documents): ADR-0013 (proposed), ADR-0014, ADR-0015; state files and dependency register updated.
- Done (scaffold, built on the pinned toolchain by the owner): `Launcher.sln`, `global.json`, `Directory.Build.props`, `Directory.Packages.props`, `nuget.config`, 14 `src/` library projects, empty `plugins/ installer/ scripts/ docs/ LICENSES/ tests/`.
- Done (2026-10-21): `Launcher.App` created from the Visual Studio WinUI template and converted to unpackaged, framework-dependent, x64 (see ADR-0014 addendum). Debug and Release builds of the full solution including `Launcher.App` reported green by the owner. The owner started the app once with `dotnet run -c Debug`; an empty window opened with no error. This is a manual smoke check only, not an automated test.
- Written, not yet run on the owner machine (2026-10-21): xUnit v3 test tooling (ADR-0017) and `tests/Launcher.Core.Tests` with six dependency-direction checks. The check logic was run in the agent sandbox against the real project files (all pass) and against deliberately broken copies (all violations detected), using a plain console harness. The xUnit packages could not be restored in the sandbox, so the test project itself is unbuilt.
- Deferred on purpose: the other six test projects from Implementation Specification §3 (created with their first real tests, so there are no placeholder tests).
- Not done: contracts, diagnostics, CI skeleton, a CI-equivalent clean restore/build/test run, `Launcher.App` project references.

## Provisional scaffold decisions (ratify in the contracts step)
- `Launcher.PluginHost` and `Launcher.PrivilegedBroker` are class libraries for now; they become executables when their phases begin.
- `Launcher.Security` and `Launcher.Diagnostics` have no project references yet, to keep the broker's dependency surface minimal. Whether Core depends on Security, and whether Diagnostics implements a Core interface, is decided in the contracts step.
- `Launcher.Motion` references Core (for the semantic `LauncherState`).
- Live-repository documents stay in `Implementation Docs/03_Live_Repository/`. The root-level `DECISIONS.md`, `DEPENDENCIES.md` and `CHANGELOG.md` shown in Implementation Specification §3 are not duplicated, to avoid two sources of truth.

## Known issues / gates
- No gate is open. Minimum OS confirmed (ADR-0015).
- Windows SDK build number amended to 10.0.26100.8249 (ADR-0016): no open toolchain deviation.
- Deviations from the Visual Studio WinUI template are recorded in the ADR-0014 addendum.
- No production implementation beyond the empty scaffold. Restore/Debug/Release builds are verified; no tests exist or have run, and no CI evidence exists yet.

## Decisions
See `DECISIONS.md` for ADR-0001 through ADR-0011 and subsequent records.

## Next action
Owner: `dotnet restore`, `dotnet build` (Debug and Release) and `dotnet test` on the solution; paste the output. Then: Core/UI/Search/Plugin contracts with the `LauncherState` model, structured diagnostics, and the CI skeleton.

## Rollback point
V3.0 pre-implementation documentation freeze.
