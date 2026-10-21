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
- Not done: `Launcher.App`, contracts, diagnostics, test projects, CI skeleton, clean restore/build/test evidence.

## Provisional scaffold decisions (ratify in the contracts step)
- `Launcher.PluginHost` and `Launcher.PrivilegedBroker` are class libraries for now; they become executables when their phases begin.
- `Launcher.Security` and `Launcher.Diagnostics` have no project references yet, to keep the broker's dependency surface minimal. Whether Core depends on Security, and whether Diagnostics implements a Core interface, is decided in the contracts step.
- `Launcher.Motion` references Core (for the semantic `LauncherState`).
- Live-repository documents stay in `Implementation Docs/03_Live_Repository/`. The root-level `DECISIONS.md`, `DEPENDENCIES.md` and `CHANGELOG.md` shown in Implementation Specification §3 are not duplicated, to avoid two sources of truth.

## Known issues / gates
- No gate is open. Minimum OS confirmed (ADR-0015).
- Windows SDK build deviation (26100.8249 installed vs 26100.9457 documented): see ADR-0014.
- No production implementation beyond the empty scaffold. Restore/Debug/Release builds are verified; no tests exist or have run, and no CI evidence exists yet.

## Decisions
See `DECISIONS.md` for ADR-0001 through ADR-0011 and subsequent records.

## Next action
Create `Launcher.App` (WinUI 3, unpackaged, framework-dependent, x64) from the Visual Studio template inside `src\`, and provide its `.csproj` so it can be reconciled with central package management and `Directory.Build.props`. Then: contracts, diagnostics, test projects (framework pending owner approval) and CI skeleton.

## Rollback point
V3.0 pre-implementation documentation freeze.
