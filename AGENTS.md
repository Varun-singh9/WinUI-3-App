# Instructions for coding agents

These rules apply to any AI coding agent or human contributor working in this repository (Claude, ChatGPT or another tool). They point at the project documents instead of repeating them; if this file and a document disagree, follow the document and fix this file.

## Source of truth
- The repository is the source of truth for implementation state. Chat history is not.
- Read `Implementation Docs/README.md` first. It defines the document hierarchy: Constitution, then Project Overview, then Implementation Specification, then the roadmap and the specialized specifications. If a lower document conflicts with a higher one, stop and record a decision in `Implementation Docs/03_Live_Repository/DECISIONS.md`.
- Live state: `Implementation Docs/03_Live_Repository/` (`NEXT_PHASE.md`, `PHASE_STATUS.md`, `PROJECT_STATE.md`, `DECISIONS.md`, `DEPENDENCIES.md`, `TESTS.md`, `ARCHITECTURE.md`).

## Start of every session
1. Clone or update the repository and read `git log --oneline -10`.
2. Read `NEXT_PHASE.md`, `PHASE_STATUS.md`, `PROJECT_STATE.md` and `DECISIONS.md`, and compare them with the repository. Do not assume an earlier session finished anything that the repository does not show.
3. Say what phase you are in and what you will do, and plan before writing code for anything non-trivial.

## Working rules
- One branch per phase (for example `phase-1-core-bar`). Do not commit to `main` until the phase exit review passes and the owner merges.
- Work in small, reviewable steps: one patch or commit per task.
- Implement only the current phase. Respect the "Do not build yet" list in `NEXT_PHASE.md`.
- Never invent architecture. If a task needs a decision that is not documented, stop and propose an ADR in `DECISIONS.md`.
- Never report something as built, tested or verified unless it was. Record evidence only from real command output or CI, and say who ran it. Compilation alone does not make a test pass.
- Keep code, tests and documents in sync in the same change. Update the state files at the end of substantial work.

## Build and test
The .NET SDK is pinned in `global.json` (10.0.402). Tests run on Microsoft Testing Platform through `dotnet test`.
```
dotnet restore Launcher.sln
dotnet build Launcher.sln -c Debug
dotnet build Launcher.sln -c Release
dotnet test Launcher.sln -c Debug
```
CI (`.github/workflows/ci.yml`) runs the same steps in Debug and Release.

## Standards that the build enforces
- Warnings are errors. Public members in `src/` need XML documentation. Test projects are exempt.
- Project references must follow `tests/Launcher.Core.Tests/DependencyRules.cs`. Changing a reference is an architecture-boundary change: update that table and add an ADR.
- Interfaces and data contracts live in `Launcher.Core` (plugin wire contracts in `Launcher.PluginContracts`); implementations live in the feature projects (ADR-0018, ADR-0019).
- Contract names and states are pinned by contract-shape tests. Change them deliberately, with an ADR.

## Non-negotiables (see the Constitution)
Core is not a feature. UI is not functionality. Native WinUI 3, XAML and Composition for the production UI; no WebView2 for visual effects. Plugins are out of process and capability-limited. Core runs unelevated; privileged work goes through a restricted broker. Default-deny capabilities. Optional providers never block activation. No silent elevation or silent installation. No secrets, keys or credentials in the repository.

## Dependencies and toolchain
Do not upgrade or add packages, SDKs or CI actions casually. Every non-platform dependency needs an entry in `DEPENDENCIES.md` (exact version, license, purpose, impact, removal plan), and major changes need an ADR. Pins live in `global.json`, `Directory.Packages.props` and the workflow file.

## Delivering changes
An agent that cannot push to GitHub delivers small git patches that apply cleanly to the pushed branch (verify with a fresh clone). The owner applies them with `git am` or `git apply`, runs restore, build and test, and pastes the output.

## Coding-agent choice
ADR-0013 (Claude as coding agent) is Proposed and the owner may switch agents between phases. Any agent may continue a phase; project state lives in this repository.
