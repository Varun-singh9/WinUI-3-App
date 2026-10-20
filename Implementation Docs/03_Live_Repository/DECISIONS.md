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

## Rules
- Architecture-boundary changes require an ADR.
- New dependencies require impact notes and an entry in `DEPENDENCIES.md`.
- Security/privacy changes require an explicit record.
- New persistent data requires a schema/migration plan.
- Changes to the technology baseline require an ADR before implementation.


## ADR-0012 — V3.0 Baseline Refresh and Archive Reorganization
- Status: Prepared as part of the V3.0 documentation refresh; implementation remains gated by the existing Phase 0 owner confirmation.
- Decision: Treat the reorganized V3.0 pack as the active documentation baseline. Refresh platform references to the current verified stable releases while keeping the approved architecture and product scope unchanged.
- Verified platform references on 2026-10-20: .NET 10.0.402 SDK / 10.0.13 runtime, Windows App SDK 2.5.1 Stable, and Windows SDK 10.0.26100.9457 for the selected 26100 baseline family.
- Context: The V2.8 pack was internally coherent but carried historical V2.x labels, mixed archive organization, and platform references that needed a current-date refresh.
- Alternatives: Keep the V2.8 archive unchanged; create a cosmetic V3 copy without reorganizing active repository-facing files.
- Consequences: V3.0 is the active handoff package. Exact machine-installed SDK/runtime/deployment values must still be verified and committed during Phase 0. No product feature or architecture boundary is expanded by this ADR.
- Date: 2026-10-20
