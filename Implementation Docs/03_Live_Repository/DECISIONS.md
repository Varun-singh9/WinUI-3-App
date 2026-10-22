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
- Owner note (2026-10-22): the owner is deliberately not deciding this now and may switch back to ChatGPT for a period if the free Claude tier becomes too limiting. Until decided, the status stays Proposed and either coding agent may implement a phase. Because project state lives in the repository (state files, ADRs, tests, CI), not in any chat, a change of agent between phases needs no migration; the phase handoff rule in the README applies to both.
- Date: 2026-10-21
- Approved by: Pending project owner (decision deferred by the owner)

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
- Verification (2026-10-21, owner machine, output pasted): after the fix, `dotnet restore`, `dotnet build -c Debug` and `dotnet test -c Debug` succeeded; `Launcher.Core.Tests` ran 6 tests, 6 passed. Resolved packages are listed in DEPENDENCIES.md.
- Addendum (privacy): the MTP packages pulled in transitively include `Microsoft.Testing.Extensions.Telemetry` and `Microsoft.ApplicationInsights`. They run only in development and CI, are not shipped, and do not touch the launcher's runtime network behavior. Telemetry opt-out for developer and CI environments is handled in the CI skeleton step.
- Date: 2026-10-21
- Approved by: Project owner

## ADR-0018 — Contract Placement: Interfaces in Launcher.Core, Implementations in Feature Projects
- Status: Accepted.
- Decision: Interfaces and data contracts for lifecycle, search, actions, configuration, diagnostics, plugin management and integrations live in `Launcher.Core`. The plugin wire contracts (`PluginManifest`, the IPC request/response and status) live in `Launcher.PluginContracts`. Implementations (prefix parser, routing, ranking, aggregation, providers, action implementations, configuration storage, the diagnostics sink) live in the feature projects and are introduced by the phase that owns them. Phase 0 defines shapes only: interfaces, records and enums, with no behavior.
- Context: The UI, providers and actions may depend only on Core contracts (Implementation Specification section 4.1), and the Constitution forbids feature implementations in Core. Putting the contracts in Core lets every consumer reference one small project without referencing feature code.
- Alternatives: Spread contracts across the feature projects (for example search types in `Launcher.Search`), which forces the UI and providers to reference feature projects; one separate contracts project for everything.
- Consequences:
  - First-cut member sets are the smallest shapes implied by the specification. The implementing phase may add members; each addition is recorded in PHASE_STATUS.md, and anything that changes a project boundary needs an ADR.
  - Task-returning members use the `Async` suffix (`ExportAsync`, `ImportAsync`, `MigrateAsync`, `InitializeAsync`, `SearchAsync`, `ShutdownAsync`); the specification's shorthand `Export()`, `Import()` and `Migrate()` is read as these names.
  - Provisional parts, to be confirmed by the phase that implements them: the `ResourceCost` scale and `ProviderLifecycleMode` values in `ProviderInfo`; the values of `IpcStatus`; the encoding of IPC payloads (text for now); the list element types of `PluginManifest.Providers`, `Commands` and `Settings` (identifiers for now); `ActionDescriptor.Target` and `Effect`; the streaming shape of `ISearchCoordinator.SearchAsync`.
  - The `PrefixParser` implementation named in the specification is not part of Phase 0.
  - Capability and permission-policy contracts for Security are not yet defined; they are added before the first provider or plugin needs them.
  - `Launcher.Core.Tests` pins the contract names and states (contract-shape tests) so accidental change fails a test.
- Date: 2026-10-21
- Approved by: Project owner

## ADR-0019 — Two New Project References: Core → PluginContracts and Diagnostics → Core
- Status: Accepted.
- Decision: `Launcher.Core` may reference `Launcher.PluginContracts` so that `IPluginManager` can use `PluginManifest`. `Launcher.Diagnostics` references `Launcher.Core` so that it can implement `IDiagnostics`. `Launcher.Security` and `Launcher.PluginContracts` remain leaf projects with no project references. Plugin capabilities are named by strings in the manifest and checked by Security; no Security type is shared with Core.
- Context: ADR-0018 places `IPluginManager` in Core, and its manifest type belongs to the plugin wire contracts. The privileged broker must keep a deliberately small attack surface (Implementation Specification section 17), so Security must not gain dependencies.
- Alternatives: Define a separate manifest summary type in Core (more types and a mapping); let Security reference PluginContracts (widens the broker's dependency surface); keep Diagnostics free of references and duplicate `LogEntry`.
- Consequences: The allowed-reference table in `tests/Launcher.Core.Tests/DependencyRules.cs` is updated and enforced by tests: Core may reference only `Launcher.PluginContracts`; `Launcher.PluginHost` and `Launcher.PrivilegedBroker` may not reference Core. `Launcher.Motion → Launcher.Core` remains provisional until the Motion System is implemented.
- Date: 2026-10-21
- Approved by: Project owner

## ADR-0020 — Structured Diagnostics Implementation and Log File Schema
- Status: Accepted.
- Decision: `Launcher.Diagnostics` implements `IDiagnostics` with no new dependency. `DiagnosticsService` filters by minimum level, redacts sensitive metadata, fans entries out to `IDiagnosticsSink` implementations, and never throws into the caller (a failing sink does not stop the others). Two sinks exist: `MemoryDiagnosticsSink` (tests and a future in-app view) and `JsonLinesFileSink`, which uses `System.Text.Json` from the platform. Nothing is wired into `Launcher.App` yet; Phase 1 does that.
- Persistent data and schema: the file sink writes one JSON object per line using the `LogEntry` fields (`Timestamp` in ISO 8601, `Level` as its name, `Component`, `EventId`, `CorrelationId`, `Duration` as a time span, `ErrorCode`, `Metadata`); null fields are omitted. Retention: when the file passes a size limit (5 MB by default) it is moved to `<path>.1`, replacing the previous rolled file, so at most two files exist. Migration plan: none is needed because logs are append-only, disposable and never read back by the product; a schema change may add fields but never changes existing ones. The file location belongs to the data layer and is not decided yet; the sink only receives a path.
- Privacy: entries never carry raw search text or secrets. As a safety net, metadata values are replaced with `[redacted]` when the key equals `query`, `searchtext`, `rawinput`, `input` or `text` (case-insensitive) or contains `password`, `passwd`, `secret`, `token`, `apikey`, `api_key`, `credential` or `authorization`. This is a heuristic by key name, not a guarantee; callers must keep such data out of entries.
- Context: Phase 0 must establish structured diagnostics (NEXT_PHASE.md task 7) and diagnostics must not become a hidden product dependency (ARCHITECTURE.md). New persistent data needs a schema and migration plan (DECISIONS.md rules), which this record provides.
- Alternatives: Serilog or Microsoft.Extensions.Logging (new dependency); EventSource/ETW; plain-text logs.
- Consequences: No new dependency. A different logging backend can replace the sinks behind `IDiagnostics` later. User-facing log viewing and export are not part of Phase 0.
- Date: 2026-10-21
- Approved by: Project owner

## ADR-0021 — CI Skeleton on GitHub Actions
- Status: Accepted. The workflow is unproven until its first run on GitHub.
- Decision: `.github/workflows/ci.yml` runs on pushes to `main` and `phase-*` branches and on pull requests. One job, on the GitHub-hosted `windows-2025` image, runs for both Debug and Release: checkout, install the .NET SDK from `global.json`, `dotnet --info`, `dotnet restore`, `dotnet build --no-restore`, `dotnet test --no-build`. Permissions are `contents: read`; in-progress runs for the same ref are cancelled; the job times out after 30 minutes. Telemetry is opted out with `DOTNET_CLI_TELEMETRY_OPTOUT`, `DOTNET_NOLOGO` and `TESTINGPLATFORM_TELEMETRY_OPTOUT`. The two actions are pinned to full commit hashes: `actions/checkout` v7.0.1 and `actions/setup-dotnet` v6.0.0.
- Context: Phase 0 requires a CI skeleton and a clean CI-equivalent restore, build and test run (NEXT_PHASE.md tasks 8 and 9; TESTS.md "CI-equivalent build").
- Alternatives: the moving `windows-latest` label; no CI; Azure Pipelines.
- Consequences: `windows-2025` is recorded as the CI image; changing it is change control. Both actions are CI-only third-party dependencies registered in DEPENDENCIES.md. The telemetry variable names come from documentation and should be confirmed in the first run's log. No secrets, signing or artifact publishing exist in this workflow. Updating an action pin is a dependency change.
- Verification (2026-10-22, from the `dotnet --info` output of the first run, pasted by the owner): the runner installed SDK 10.0.402 and used `global.json` from the checkout; the runner OS is Windows 10.0.26100 (win-x64); `DOTNET_CLI_TELEMETRY_OPTOUT=1` and `DOTNET_NOLOGO=1` are set. `dotnet --info` does not list `TESTINGPLATFORM_TELEMETRY_OPTOUT`, so that variable's effect is not confirmed.
- Date: 2026-10-21
- Approved by: Project owner

## ADR-0022 — Phase 0 Exit and Carry-Over Items
- Status: Accepted — the project owner merged `phase-0-foundation` into `main` (merge commit 13482a5) and tagged `phase-0-complete` on 2026-10-22.
- Decision: Phase 0 is complete against the V3.0 exit gate (solution restores, builds and tests with recorded exact toolchain and deployment values; project references comply with the approved direction; CI smoke evidence exists). The evidence is listed in PHASE_STATUS.md. The items below are carried forward on purpose and do not block the exit:
  1. Security capability and permission-policy contracts and the capability catalog. Phase 0 encodes permissions declaratively (manifest `Capabilities`, `ProviderInfo.Permissions`, `RiskLevel`, `ActionDescriptor`); the enforcement contract is added, with its own ADR, before the first provider or plugin consumes it. The specification places enforcement in the security-hardening phase.
  2. The other six test projects from Implementation Specification section 3, created with their first real tests.
  3. Wiring diagnostics into `Launcher.App` and deciding the log file location (Phase 1 and the data layer).
  4. Resolving the roadmap/specification difference in the Phase 1 UI state list (see NEXT_PHASE.md).
  5. Owner sign-off on ADR-0013, and renaming or genericizing the documents that still name ChatGPT.
  6. Confirming the deployment interpretation (ADR-0014) before installer work, evaluating Arm64 and testing on a Windows 11 24H2 machine before release.
  7. Confirming the SDK version and telemetry opt-out in the first CI run log.
- Context: The Phase 0 gate is evidence-based. Carrying the items forward explicitly keeps the exit honest instead of declaring everything done.
- Alternatives: Hold Phase 0 open until the security contracts and all test projects exist; close Phase 0 without recording the carry-over.
- Consequences: Phase 1 starts from a green, CI-verified base. Each carried item has an owner phase and must be resolved there or re-deferred by a new ADR.
- Date: 2026-10-22
- Approved by: Project owner (by merging and tagging, 2026-10-22)

## Rules
- Architecture-boundary changes require an ADR.
- New dependencies require impact notes and an entry in `DEPENDENCIES.md`.
- Security/privacy changes require an explicit record.
- New persistent data requires a schema/migration plan.
- Changes to the technology baseline require an ADR before implementation.
