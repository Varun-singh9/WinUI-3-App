# Architecture Decision Records

An ADR is an entry in this file. There is no separate `/docs/adr/` directory.

Use one entry per non-trivial decision. Architecture-boundary changes, major dependency changes, security-sensitive changes, persistent-data changes, and other changes identified by the Constitution require a record.

## ADR-0001 — Native WinUI 3 Production Renderer
- Decision: Use WinUI 3 + XAML + Windows Composition as the production UI/rendering stack.
- Context: The launcher requires native Windows behavior, low-latency activation and high-frequency motion without making WebView2 a default dependency.
- Alternatives: WebView2/HTML/CSS runtime; Electron; other cross-platform UI stacks.
- Consequences: Windows-first implementation and native Composition-based motion. Web technologies remain useful for prototyping/reference only.
- Date: 2026-10-01
- Approved by: Project owner

## ADR-0002 — Out-of-Process Capability-Limited Plugins
- Decision: Plugins run out of process and receive only declared capabilities.
- Context: Optional extensions must not inherit Core privileges or compromise launcher stability.
- Alternatives: In-process plugins; unrestricted helper processes.
- Consequences: Stronger isolation and fault containment with IPC/lifecycle complexity.
- Date: 2026-10-01
- Approved by: Project owner

## ADR-0003 — Named Pipes for Initial Plugin IPC
- Decision: Use Windows Named Pipes for general plugin IPC initially.
- Context: The project is Windows-first and needs local request/response IPC with cancellation, timeouts and process-failure handling.
- Alternatives: TCP localhost; sockets; other local IPC mechanisms.
- Consequences: Native local IPC with an explicit versioned protocol; protocol design and validation become project responsibilities.
- Date: 2026-10-01
- Approved by: Project owner

## ADR-0004 — Unelevated Core with Restricted Privileged Broker
- Decision: Core and ordinary plugins run unelevated; privileged operations go through a restricted broker.
- Context: The launcher can expose powerful Windows operations but must not normalize silent elevation.
- Alternatives: Run the entire launcher elevated; allow plugins to request arbitrary elevation.
- Consequences: Smaller privilege boundary and explicit risky-operation handling; broker must remain narrowly scoped.
- Date: 2026-10-01
- Approved by: Project owner

## ADR-0005 — Default-Deny Capabilities
- Decision: Capabilities are denied unless explicitly declared and permitted.
- Context: Plugins and integrations must not silently gain filesystem, process, network or administrator access.
- Alternatives: Allow-by-default model; trust all local plugins.
- Consequences: More explicit manifests and validation, with safer failure behavior.
- Date: 2026-10-01
- Approved by: Project owner

## ADR-0006 — Semantic Motion System
- Decision: Application state drives motion; feature/business code does not directly orchestrate visual animation.
- Context: Activation, prefix, customization and cursor-reactive interactions must remain responsive and replaceable.
- Alternatives: Feature-specific animation code; plugin-owned UI animation.
- Consequences: A dedicated Motion System and semantic state contracts are required.
- Date: 2026-10-01
- Approved by: Project owner

## ADR-0007 — Local-First Data Architecture
- Decision: Use SQLite for structured local data and file storage for larger artifacts/caches/logs as appropriate.
- Context: V1 is local-first and should not require a cloud service for core operation.
- Alternatives: Cloud-first storage; unstructured configuration files for all data.
- Consequences: Explicit schema/migration management and local privacy controls are required.
- Date: 2026-10-01
- Approved by: Project owner

## ADR-0008 — Technology Baseline
- Decision: Target .NET 10 LTS, C# 14, Windows App SDK 2.5.x Stable, and Windows SDK 10.0.26100.x as the implementation baseline. Phase 0 pins exact patch/package versions mechanically.
- Context: The project needs a stable, supported toolchain rather than a moving preview/experimental target. Windows App SDK 2.5.1 is the current Stable release as of the pre-implementation freeze.
- Alternatives: Windows App SDK preview/experimental; older Windows App SDK line; newer Windows SDK line without explicit evaluation.
- Consequences: Toolchain versions become controlled dependencies. Updating them is a change-control event.
- Date: 2026-10-01
- Approved by: Project owner

## ADR-0009 — Authoritative Implementation Layout
- Decision: `Implementation_Specification.docx` §3 defines the production .NET solution/project layout. The historical tree in `16_Extension_Pack.docx` is retained only as historical context.
- Context: The project evolved from an early conceptual tree to a proper per-concern .NET solution.
- Alternatives: Continue using the historical single-tree sketch.
- Consequences: Phase 0 creates the solution according to the implementation specification and does not silently recreate the historical layout.
- Date: 2026-10-01
- Approved by: Project owner

## ADR-0010 — Coding-Agent Target Switched to ChatGPT
- Decision: All coding-agent-facing instructions, rules, and workflow documents target ChatGPT instead of Gemini. `09_GEMINI_WORKFLOW.docx` is renamed to `09_CHATGPT_WORKFLOW.docx`, and every cross-reference to it elsewhere in the pack is updated to match.
- Context: The project owner is switching which AI coding tool implements each phase. This is a terminology/target change only — no architecture, phase, quest, technology baseline, or Definition of Done content was altered.
- Alternatives: Keep Gemini-specific phrasing and maintain a separate ChatGPT variant in parallel; genericize all references to "the coding agent" instead of naming a specific tool.
- Consequences: Every future phase prompt is written for ChatGPT. If a different or additional coding agent is introduced later, this ADR should be revisited rather than silently overridden.
- Date: 2026-10-02
- Approved by: Project owner

## ADR-0011 — V2.7 Consistency and Implementation-Readiness Correction Pass
- Decision: Adopt the V2.7 corrections as the approved baseline — most significantly, narrowing the minimum supported OS from Windows 11 22H2 (build 22621) to Windows 11 24H2 (build 26100) to match the already-pinned Windows SDK 10.0.26100.x; reframing the technology baseline as "approved version families" pending exact Phase 0 pinning; separating checksum integrity from Authenticode publisher authenticity in artifact verification; reconciling the plugin manifest schema onto the Implementation Specification's field names (`ApiVersion`/`Version`, superseding `sdkVersion`/`pluginVersion`); restricting Phase 6 workflows to linear, non-privileged steps with no branching, scheduling, nesting, parallelism, or arbitrary expressions; and setting concrete per-tier confirmation requirements (Medium+ requires confirmation; High/Critical must display target and effect).
- Context: A documentation-consistency audit found these ten items inconsistent, ambiguous, or unverifiable across the pack. All ten are corrected together in V2.7 rather than as separate ADRs, since they were found and resolved as one review pass.
- Alternatives: Leave the ambiguities for Phase 0 to resolve ad hoc; file ten separate ADRs instead of one combined entry.
- Consequences: The OS floor change excludes Windows 11 22H2/23H2 machines that were previously in scope — confirm this is acceptable before Phase 0 begins. The workflow restriction means Q08/Phase 6 ships a smaller feature than earlier drafts implied; broader workflow capability remains available later per `15_Future_Roadmap.docx` Tier 1. No requirement, roadmap item, or deferred-scope statement was removed — V2.7 adds corrections and clarifications only.
- Date: 2026-10-06
- Approved by: Project owner

## ADR-0012 — V3.0 Baseline Refresh and Archive Reorganization
- Status: Prepared as part of the V3.0 documentation refresh; implementation was gated by the Phase 0 owner confirmation, which ADR-0015 later cleared.
- Decision: Treat the reorganized V3.0 pack as the active documentation baseline. Refresh platform references to the current verified stable releases while keeping the approved architecture and product scope unchanged.
- Verified platform references on 2026-10-20: .NET 10.0.402 SDK / 10.0.13 runtime, Windows App SDK 2.5.1 Stable, and Windows SDK 10.0.26100.9457 for the selected 26100 baseline family.
- Context: The V2.8 pack was internally coherent but carried historical V2.x labels, mixed archive organization, and platform references that needed a current-date refresh.
- Alternatives: Keep the V2.8 archive unchanged; create a cosmetic V3 copy without reorganizing active repository-facing files.
- Consequences: V3.0 is the active handoff package. Exact machine-installed SDK/runtime/deployment values must still be verified and committed during Phase 0. No product feature or architecture boundary is expanded by this ADR.
- Date: 2026-10-20

## ADR-0013 — Coding-Agent Target Switched from ChatGPT to Claude
- Status: Proposed — pending project-owner sign-off.
- Decision: Claude, operating as the engineering assistant in the "CMDPAL V3" Claude Project, is the coding agent for all phases starting with Phase 0. This supersedes the coding-agent target set in ADR-0010; ADR-0010 is retained as history.
- Context: The project owner is switching which AI tool implements each phase. This is a tooling change only: no architecture, phase scope, technology baseline, security model or Definition of Done content is altered. The Claude Project instructions carry the role, inspect-first, no-fake-completion and handoff rules.
- Alternatives: Keep ChatGPT as the target; maintain parallel ChatGPT and Claude workflow documents; genericize every reference to "the coding agent".
- Consequences:
  - Documents that name ChatGPT (README.md, 04_Project_Overview §13, 09_CHATGPT_WORKFLOW.docx and its manifest entry, 16_Extension_Pack §1/§5/§9, and wording in the changelog) are now stale. They are not rewritten in Phase 0. A follow-up task renames or genericizes them under change control. Until then, "ChatGPT" in those documents is read as "the coding agent."
  - Each chat starts by cloning the repository, reading `git log` and the Live_Repository state files, and comparing them with the documentation. If the clone fails, the owner supplies a zip.
  - The coding agent's sandbox cannot restore or build WinUI. Build/test evidence for WinUI and the Windows-targeted projects must come from the owner's machine or CI, and must be recorded as such.
- Date: 2026-10-21
- Approved by: Pending project owner

## ADR-0014 — Phase 0 Recorded Toolchain, Deployment and Platform Values
- Status: Accepted for the owner-selected items below; the Windows SDK deviation is recorded and unresolved.
- Decision: Record these Phase 0 values as the exact baseline for the first reproducible build.
  - .NET SDK 10.0.402 pinned in `global.json` with `rollForward: latestPatch` and `allowPrerelease: false`. SDKs 10.0.401 and 9.0.318 are installed on the owner machine and are excluded by the pin.
  - Target framework `net10.0-windows10.0.26100.0`; `SupportedOSPlatformVersion` 10.0.26100.0; C# 14.
  - Architecture: x64 only for now; Arm64 is evaluated before release.
  - Deployment: unpackaged, framework-dependent (Windows App SDK runtime is a prerequisite that the installer must detect or provide). The owner chose a "flexible deployment model"; this is the interpretation used. Confirm or correct it before installer work; changing it requires a new ADR.
  - Windows App SDK 2.5.1 Stable (package id and version to be verified on the first Visual Studio restore). Package source: nuget.org only (`nuget.config`).
  - IDE/build: Visual Studio Community 2026 18.11 stable (not prerelease) with the WinUI application development workload.
  - Windows SDK: 10.0.28000 is installed but must not be used. CI image is selected in the CI step and recorded then.
  - Build policy: `TreatWarningsAsErrors` is on. NU1900 (vulnerability feed unreachable) is exempt so offline builds do not fail; NU1901–NU1904 (actual vulnerabilities) remain errors.
- Deviation (resolved by ADR-0016 on 2026-10-21): The documents list Windows SDK 10.0.26100.9457. The owner machine has 10.0.26100.8249. The baseline family (26100.x) is unchanged and the documents are not edited. Resolve by either installing 26100.9457 or amending the documented build number through a follow-up ADR. Note: the `net10.0-windows10.0.26100.0` target framework resolves its reference assemblies through the .NET SDK, so the installed Windows SDK build mainly affects the WinUI application and its tooling. Verify this on the first Visual Studio build.
- Machine record: Windows 11 25H2 (26200.9457), x64, Git 2.55. The development machine is newer than the 24H2 floor, so it cannot by itself prove floor compatibility (see ADR-0015).
- Addendum (2026-10-21, after `Launcher.App` was created): `Microsoft.Windows.SDK.BuildTools` is pinned to 10.0.26100.8249 in `Directory.Packages.props`, matching the installed Windows SDK build. Visual Studio's "Add New Project" had inserted 10.0.28000.2705 (the excluded SDK family); that was reverted. Resolved package versions on the owner machine are listed in DEPENDENCIES.md. The Windows App SDK 2.5.1 / BuildTools 26100.8249 combination restored and built without conflict. The 10.0.28000 SDK was not present in the Windows Kits Include folder when checked (only 10.0.26100.0), so it is no longer treated as installed. The Windows SDK build-number deviation (documents 9457 vs installed/pinned 8249) remains open.
- Addendum (2026-10-21): `Launcher.App` uses `WindowsPackageType=None`, `WindowsAppSDKSelfContained=false`, platform x64, `RuntimeIdentifier=win-x64`, `TargetPlatformMinVersion=10.0.26100.0`. The Visual Studio template defaults (net8.0, 17763, x86/x64/ARM64, MSIX tooling menus, PublishTrimmed) were removed. The owner started the app once with `dotnet run` and an empty window opened.
- Context: Phase 0 must record exact toolchain and deployment values before scaffold freeze (Constitution V3.0 Pinning Rule; Implementation Specification §3 and V2.7 scaffold contract).
- Alternatives: Packaged MSIX; packaged with external location; self-contained runtime; x64+Arm64 from the start; `rollForward: latestFeature`.
- Consequences: Unpackaged apps have no package identity, so APIs that require identity need explicit workarounds or are unavailable. Framework-dependent delivery makes the Windows App SDK runtime an install-time dependency. Updating any recorded value is a Change Control event.
- Date: 2026-10-21
- Approved by: Project owner (deployment, architecture and rollForward choices, 2026-10-21); the Windows SDK deviation is recorded for follow-up

## ADR-0015 — Windows 11 24H2 (Build 26100) Minimum OS Confirmed
- Status: Accepted.
- Decision: Windows 11 version 24H2 (build 26100) or later is the minimum supported OS. This clears the Phase 0 owner gate recorded in ADR-0011.
- Context: The project owner confirmed the floor in the Phase 0 kickoff on 2026-10-21.
- Alternatives: Windows 11 22H2 (build 22621), as before V2.7.
- Consequences: Windows 11 22H2 and 23H2 machines are out of support. Any API newer than build 26100 still needs a runtime availability guard and a compatibility test. The development machine runs 25H2 (26200), so floor compatibility needs a 24H2 test machine or VM before release. Gate wording in PROJECT_STATE.md, PHASE_STATUS.md, NEXT_PHASE.md, ARCHITECTURE.md, DEPENDENCIES.md, README.md and PACK_MANIFEST.json is updated in the same change. RELEASE_NOTES and SOURCE_AUDIT are V3.0 release records and are left as written.
- Date: 2026-10-21
- Approved by: Project owner

## ADR-0016 — Windows SDK Baseline Build Amended to 10.0.26100.8249
- Status: Accepted.
- Decision: The documented Windows SDK baseline build is amended from 10.0.26100.9457 to 10.0.26100.8249. The 26100.x family and the 24H2 / build 26100 minimum OS (ADR-0015) are unchanged. This resolves the deviation recorded in ADR-0014.
- Context: The owner machine has Windows SDK 10.0.26100.8249 and chose not to install 9457. `Microsoft.Windows.SDK.BuildTools` is pinned to 10.0.26100.8249 and the solution restores and builds on it. The 9457 figure was a documentation-refresh value that was never verified on the owner machine.
- Alternatives: Install 10.0.26100.9457 and keep the documents unchanged; keep 9457 documented with a standing deviation.
- Consequences: The documented build number now matches the pinned BuildTools package and the installed SDK. Documents amended: DEPENDENCIES.md, ARCHITECTURE.md, PROJECT_STATE.md, README.md, PACK_MANIFEST.json, CHANGELOG.md. Records of what was checked on 2026-10-20 (ADR-0012, V3.0_RELEASE_NOTES.md, the V3.0 changelog entry, SOURCE_AUDIT.md) are left as written because they describe that refresh, not the current baseline. Moving to a different 26100.x build later is a change-control event.
- Date: 2026-10-21
- Approved by: Project owner

## ADR-0017 — xUnit.net v3 as the Test Framework
- Amended 2026-10-21: the Visual Studio Test Platform parts of this record are superseded by the addendum at the end of this entry. The framework choice (xUnit.net v3) is unchanged.
- Status: Accepted (framework choice by the project owner). The exact package versions below were looked up on 2026-10-21 and are confirmed by the first successful restore on the owner machine.
- Decision: Use xUnit.net v3 for all test projects, run through the Visual Studio Test Platform (`dotnet test`, Visual Studio Test Explorer). Packages are pinned centrally in `Directory.Packages.props`: `xunit.v3` 4.0.1, `Microsoft.NET.Test.Sdk` 18.10.0, `xunit.runner.visualstudio` 4.0.1. Test projects live under `tests/`, inherit `tests/Directory.Build.props`, and are exempt from the public XML-documentation requirement. The first test project is `Launcher.Core.Tests` (dependency-direction architecture checks). The other six projects in Implementation Specification §3 are created when each has its first real test, so no placeholder tests exist.
- Context: Phase 0 must establish initial test projects (NEXT_PHASE.md task 8). The specification names the test projects but no framework. The packages are build-time and test-time only and are never shipped in the launcher.
- Alternatives: MSTest; NUnit; xUnit.net v2 (maintenance line); `xunit.v3` 4.0.2 (published 2026-10-09, twelve days old at the time of choice).
- Consequences: xUnit v3 test projects are executables (`OutputType` Exe). Visual Studio Test Platform mode is used for now; moving to Microsoft Testing Platform mode is a possible later change and needs its own decision. Transitive test packages are recorded in DEPENDENCIES.md after the first restore. The `xunit.runner.visualstudio` version comes from the xunit.net release notes and was not independently confirmed on nuget.org; the license values (Apache-2.0 for the xUnit packages, MIT for Microsoft.NET.Test.Sdk) are to be confirmed against the package metadata at the release dependency review.
- Addendum (2026-10-21, after the first `dotnet test` on the owner machine): `dotnet test` failed with "Testing with VSTest target is no longer supported by Microsoft.Testing.Platform on .NET 10 SDK and later". xunit.v3 4.x runs on Microsoft Testing Platform (MTP) v2, and the .NET 10 SDK only drives MTP from `dotnet test` when `global.json` opts in. Corrected decision: `global.json` sets `"test": { "runner": "Microsoft.Testing.Platform" }`, and `Microsoft.NET.Test.Sdk` and `xunit.runner.visualstudio` are removed because they exist only for VSTest mode. The only test package is `xunit.v3` 4.0.1. Tests are run with `dotnet test` (MTP mode). Visual Studio 18.11 Test Explorer is expected to run them through MTP; the CLI is the evidence path. The original "Visual Studio Test Platform mode" decision was wrong because it did not account for xunit.v3 4.x defaulting to MTP v2.
- Addendum (privacy): the MTP packages pulled in transitively include `Microsoft.Testing.Extensions.Telemetry` and `Microsoft.ApplicationInsights`. They run only in development and CI, are not shipped, and do not touch the launcher's runtime network behavior. Telemetry opt-out for developer and CI environments is handled in the CI skeleton step.
- Date: 2026-10-21
- Approved by: Project owner

## Rules
- Architecture-boundary changes require an ADR.
- New dependencies require impact notes and an entry in `DEPENDENCIES.md`.
- Security/privacy changes require an explicit record.
- New persistent data requires a schema/migration plan.
- Changes to the technology baseline require an ADR before implementation.
