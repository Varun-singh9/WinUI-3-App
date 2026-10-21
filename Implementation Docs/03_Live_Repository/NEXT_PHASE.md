# Next Phase Handoff — V3.0

## Goal
Create the actual production .NET solution and freeze the executable engineering contracts without introducing new product scope.

## Preconditions
- Read the root `README.md`.
- Read `00_Canonical/00_PROJECT_CONSTITUTION.docx`.
- Read `00_Canonical/04_Project_Overview.docx`.
- Read `02_Engineering/Implementation_Specification.docx`.
- Read `03_Live_Repository/DECISIONS.md`.
- Windows 11 24H2 minimum-OS gate: CONFIRMED by the owner on 2026-10-21 (ADR-0015).
- Verify the exact installed .NET SDK, Windows SDK, Windows App SDK package/runtime, target/minimum platform versions, deployment model, architecture and Visual Studio/MSBuild prerequisites.

## Tasks
1. Create the `.sln`/`.slnx` and project structure from Implementation Specification §3.
2. Create `global.json`, `Directory.Build.props` and `Directory.Packages.props`.
3. Pin the exact verified toolchain.
4. Record exact selected versions in `DEPENDENCIES.md`.
5. Establish project references according to dependency direction.
6. Create Core/UI/Search/Plugin contract interfaces and the semantic lifecycle state model.
7. Establish structured diagnostics/logging.
8. Establish initial test projects and CI build skeleton.
9. Produce the first clean build and test run.

## Task status (2026-10-21)
1. Solution/project structure: created including `Launcher.App`; test projects not yet created. Builds on the pinned toolchain.
2. `global.json`, `Directory.Build.props`, `Directory.Packages.props`: created (plus `nuget.config`).
3. Toolchain pin: values recorded in ADR-0014; machine verification pending.
4. `DEPENDENCIES.md`: Phase 0 values and the SDK deviation recorded.
5. Project references: set per §4.1 with the provisional choices in PHASE_STATUS.md.
6–9. Not started.

## Do not change
- Core ≠ Feature.
- UI ≠ Functionality.
- Local ≠ Monolithic.
- Native WinUI 3/XAML/Composition production renderer.
- Out-of-process capability-limited plugins.
- Unelevated Core with restricted privileged broker.
- Default-deny capability model.
- Semantic Motion System.
- Critical readiness before interactive bar.
- Optional services do not block activation.
- Deferred scope in the Future Roadmap / Extension Pack.

## Completion evidence
Phase 0 is complete only when restore, build and tests pass in a clean CI-equivalent environment, project references meet the approved direction, exact toolchain/deployment values are recorded, and the Phase 1 handoff contains evidence.
