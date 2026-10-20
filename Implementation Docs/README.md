# Windows 11 Launcher — Project Document Pack V3.0

## V3.0 implementation-ready baseline

**Pack version:** 3.0  
**Refresh date:** 2026-10-20  
**Status:** Pre-implementation freeze / ready for Phase 0  
**Production code:** Not included in this pack

This is the reorganized and refreshed V3.0 documentation pack for the Windows 11 Modular Command-Palette / Launcher. It is intended to be the single handoff package used when creating the production repository and when opening a new ChatGPT implementation chat for each phase.

V3.0 keeps the approved product direction and engineering boundaries from V2.8, removes stale active-version wording, reorganizes the archive into clear functional groups, and refreshes the platform baseline against the current Microsoft release information available on 2026-10-20.

## Source-of-truth hierarchy

1. `00_Canonical/00_PROJECT_CONSTITUTION.docx` — highest-level rules and non-negotiables.
2. `00_Canonical/04_Project_Overview.docx` — canonical product, architecture, UX and system behavior.
3. `02_Engineering/Implementation_Specification.docx` — authoritative engineering layout, contracts and implementation rules.
4. `01_Product_and_Roadmap/01_Project_Roadmap.docx`, `02_Project_Progression.docx`, `03_Launcher_Questline.docx` — implementation sequence and phase scope.
5. `02_Engineering/07_SECURITY_MODEL.docx`, `08_TESTING_STRATEGY.docx`, `10_WINDOWS_COMPATIBILITY.docx`, `11_DATA_PRIVACY.docx`, `12_RELEASE_CHECKLIST.docx` — specialized constraints and gates.
6. `02_Engineering/14_MOTION_AND_INTERACTION_SPEC.docx` — detailed interaction and motion behavior.
7. `01_Product_and_Roadmap/15_Future_Roadmap.docx` and `16_Extension_Pack.docx` — deferred scope and extended rules.
8. `03_Live_Repository/*` — live implementation state and records once coding starts.
9. `04_History_and_Tracking/CHANGELOG.md` and `05_Meta/*` — history, package identity and audit metadata.

If a lower-level document conflicts with a higher-level source of truth, stop and record a decision in `03_Live_Repository/DECISIONS.md`.

## V3.0 organization

### 00_Canonical
The product constitution and canonical project overview.

### 01_Product_and_Roadmap
Roadmap, progression, questline, reconstructed conversation archive, future roadmap and extension/development rules.

### 02_Engineering
Implementation specification plus security, testing, workflow, Windows compatibility, privacy, release and motion specifications.

### 03_Live_Repository
Files intended to be copied into the actual software repository and maintained during implementation.

### 04_History_and_Tracking
Changelog and interactive progress tracker.

### 05_Meta
Manifest and package metadata.

## Current technology baseline

- Windows 11 first.
- Native WinUI 3 + XAML + Windows Composition for production UI.
- .NET 10 LTS / C# 14.
- Windows App SDK 2.5.1 Stable.
- Windows SDK 10.0.26100.x family; V3.0 records 10.0.26100.9457 as the current baseline build verified during this refresh.
- Minimum supported OS: Windows 11 24H2 (build 26100) or later, pending the explicit Phase 0 owner gate already recorded in the decision history.
- SQLite for structured local data.
- Named Pipes for initial general plugin IPC.
- Out-of-process, capability-limited plugins.
- Normal user privileges by default.
- WebView2 is not a default UI runtime and must not be introduced merely for visual effects.

Microsoft currently lists Windows App SDK 2.5.1 as the latest Stable release and the 26100-series Windows SDK as a supported SDK family. The .NET 10 download page currently lists SDK 10.0.402 and runtime 10.0.13, released October 13, 2026. Exact repository toolchain files must still be verified against the developer machine and then committed in Phase 0.

## Product non-negotiables

- Core ≠ Feature.
- UI ≠ Functionality.
- Local ≠ Monolithic.
- Everything should be replaceable.
- Search is infrastructure.
- Optional integrations never become hidden dependencies.
- Animation communicates state and never masks slow engineering.
- Critical readiness precedes interactive command-bar use.
- Optional providers never block activation.
- Reduced Motion, Low Power and weak-hardware fallbacks are required.
- Plugins never inherit Core privileges.
- No silent elevation.
- No silent third-party installation or startup modification.
- Risky actions follow the documented confirmation policy.
- Production renderer remains native WinUI 3/XAML/Composition.

## Phase handoff rule

Each phase is implemented in a fresh ChatGPT chat. The new chat should receive the minimum live state first, then read the canonical and engineering documents needed for that phase. The previous chat must leave:

1. files changed;
2. build/test evidence;
3. known issues;
4. decisions made;
5. exact current phase status;
6. one concrete next action.

Do not declare a feature complete because source code exists. Completion requires the applicable build, automated tests, manual verification, and security/performance checks.

## Phase 0 starting gate

Before production implementation:

- confirm the Windows 11 24H2 minimum OS decision;
- verify the installed .NET SDK and pin it in `global.json`;
- verify Windows App SDK 2.5.1 and pin the package/runtime;
- verify the selected Windows SDK build;
- record Visual Studio/MSBuild, workloads, architecture and deployment model;
- create the production solution exactly from `02_Engineering/Implementation_Specification.docx`;
- establish project references, contracts, diagnostics and test projects;
- perform a clean restore/build/test;
- update the live state files.

## What this archive is not

This is not the production source repository, not a compiled application, and not permission to invent new architecture during implementation. It is the controlled design/engineering baseline from which the repository is created.

