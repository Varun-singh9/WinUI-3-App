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
- Scaffold smoke build (agent sandbox, SDK 10.0.113, plain net10.0, not the pinned toolchain): 14 libraries, 0 warnings, 0 errors. Not a Phase 0 exit result.

## Phase 0 progress
- Done (documents): ADR-0013 (proposed), ADR-0014, ADR-0015; state files and dependency register updated.
- Done (scaffold, unverified on the pinned toolchain): `Launcher.sln`, `global.json`, `Directory.Build.props`, `Directory.Packages.props`, `nuget.config`, 14 `src/` library projects, empty `plugins/ installer/ scripts/ docs/ LICENSES/ tests/`.
- Not done: `Launcher.App`, contracts, diagnostics, test projects, CI skeleton, clean restore/build/test evidence.

## Provisional scaffold decisions (ratify in the contracts step)
- `Launcher.PluginHost` and `Launcher.PrivilegedBroker` are class libraries for now; they become executables when their phases begin.
- `Launcher.Security` and `Launcher.Diagnostics` have no project references yet, to keep the broker's dependency surface minimal. Whether Core depends on Security, and whether Diagnostics implements a Core interface, is decided in the contracts step.
- `Launcher.Motion` references Core (for the semantic `LauncherState`).
- Live-repository documents stay in `Implementation Docs/03_Live_Repository/`. The root-level `DECISIONS.md`, `DEPENDENCIES.md` and `CHANGELOG.md` shown in Implementation Specification §3 are not duplicated, to avoid two sources of truth.

## Known issues / gates
- No gate is open. Minimum OS confirmed (ADR-0015).
- Windows SDK build deviation (26100.8249 installed vs 26100.9457 documented): see ADR-0014.
- No production implementation beyond the empty scaffold; nothing is verified on the pinned toolchain.

## Decisions
See `DECISIONS.md` for ADR-0001 through ADR-0011 and subsequent records.

## Next action
Run on the owner machine: `dotnet --list-sdks`, `dotnet --version` (expect 10.0.402), then `dotnet restore Launcher.sln`, `dotnet build Launcher.sln -c Debug` and `-c Release`. Record the output here, then create `Launcher.App`.

## Rollback point
V3.0 pre-implementation documentation freeze.
