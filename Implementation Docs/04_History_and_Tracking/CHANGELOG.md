# Changelog — Windows 11 Launcher Project Document Pack

## Phase 0 — In progress — 2026-10-21

### Added
- ADR-0013 (coding agent ChatGPT → Claude, proposed), ADR-0014 (Phase 0 toolchain/deployment values and Windows SDK deviation), ADR-0015 (24H2 minimum OS confirmed).
- Solution scaffold: `Launcher.sln`, `global.json`, `Directory.Build.props`, `Directory.Packages.props`, `nuget.config`, 14 `src/` library projects, root README.

### Verified
- Owner-machine restore, Debug and Release builds of all 14 scaffold projects succeeded on SDK 10.0.402 (TESTS.md build rows updated to PASS).

### Added (Launcher.App)
- `Launcher.App` WinUI project (unpackaged, framework-dependent, x64) with an empty placeholder window; template defaults replaced (ADR-0014 addendum).
- Resolved Windows App SDK package versions recorded in DEPENDENCIES.md.

### Added (tests)
- ADR-0016 (documented Windows SDK baseline build amended to 10.0.26100.8249) and ADR-0017 (xUnit.net v3 as the test framework).
- `tests/Launcher.Core.Tests` with dependency-direction architecture checks; xUnit packages pinned in `Directory.Packages.props` and registered in DEPENDENCIES.md.

### Fixed
- `dotnet test` failed on .NET 10 with xunit.v3 4.x (VSTest vs Microsoft Testing Platform). `global.json` now opts in to Microsoft Testing Platform and the VSTest-only packages (`Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`) are removed (ADR-0017 addendum).

### Changed
- Documented Windows SDK build amended from 10.0.26100.9457 to 10.0.26100.8249 in DEPENDENCIES.md, ARCHITECTURE.md, PROJECT_STATE.md, README.md and PACK_MANIFEST.json (ADR-0016).
- `Microsoft.Windows.SDK.BuildTools` pinned to 10.0.26100.8249 (Visual Studio had inserted the excluded 10.0.28000.2705).
- State files, dependency register and gate wording updated for the cleared 24H2 gate. ADR-0012 moved above the Rules section in DECISIONS.md (content unchanged).
- Baseline documents were not edited for the Windows SDK build-number deviation; it is recorded in ADR-0014 and DEPENDENCIES.md.

## V3.0 — Documentation baseline refresh — 2026-10-20

### Added
- Reorganized the archive into canonical, product/roadmap, engineering, live-repository, history/tracking and metadata groups.
- Added a root README as the primary entry point.
- Added explicit V3.0 release notes and an archive index.
- Added a current-date platform verification record.

### Changed
- Normalized active document titles and references from V2.x to V3.0 where they described the current pack.
- Refreshed the live repository templates for the V3.0 handoff.
- Consolidated Phase 0 prerequisites, evidence requirements and toolchain pinning rules.
- Strengthened the test matrix with explicit Debug/Release/CI, runtime-guard, artifact-integrity and workflow-boundary checks.
- Kept historical V2.x decisions and correction history instead of silently rewriting project history.

### Platform refresh
- Windows App SDK 2.5.1 remains the latest Stable release listed by Microsoft on 2026-10-20.
- .NET 10 remains LTS; Microsoft lists SDK 10.0.402 and runtime 10.0.13 for the October 13, 2026 servicing release.
- Windows SDK 10.0.26100.9457 is recorded as the selected 26100-series baseline build; the newer 28000 family is not adopted automatically.

### Preserved
- Product scope, roadmap phases, Core/UI/Plugin boundaries, security model, motion system, local-first policy, installer direction, and deferred-scope rules.
- The historical V2.7/V2.8 correction trail.

## V2.8 and earlier
The V2.8 audit and earlier releases remain historical records in the source archive and are represented by the current decision history. No historical decision is treated as a current requirement unless it remains in the V3.0 active documents.
