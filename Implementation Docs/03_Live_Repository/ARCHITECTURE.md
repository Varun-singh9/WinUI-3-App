# Architecture — V3.0

## Status
Pre-implementation freeze. This is the concise repository-facing architecture summary. The detailed engineering contract is `../02_Engineering/Implementation_Specification.docx`.

## Boundaries
- Core
- Search Infrastructure
- UI System
- Motion System
- Provider Manager
- Integration Manager
- Plugin Contracts / Plugin System
- Configuration / Data Layer
- Diagnostics
- Security / Capability Enforcement
- Restricted Privileged Broker

## Dependency direction
- Core coordinates; it does not contain feature implementations.
- Interfaces and data contracts live in Launcher.Core; plugin wire contracts live in Launcher.PluginContracts; implementations live in the feature projects (ADR-0018).
- Core may reference only Launcher.PluginContracts. Launcher.Diagnostics references Core to implement its diagnostics contract. Launcher.Security and Launcher.PluginContracts are leaf projects (ADR-0019). The allowed references are enforced by tests.
- Search owns parsing, routing, cancellation, aggregation and ranking.
- Providers supply capability-specific results through contracts.
- UI consumes semantic results/actions and does not contain provider-specific business logic.
- Motion renders semantic UI state; business logic must not directly orchestrate animation details.
- Plugins are out-of-process and capability-limited.
- Privileged operations are isolated behind a restricted broker.
- Persistent data is accessed through explicit services/repositories.
- Diagnostics are cross-cutting infrastructure and must not become a hidden product dependency.

## Core rule
**Core must not contain feature implementations.**

Clarification (ADR-0024): Core's own orchestration (the runtime coordinator, the lifecycle state machine and the activation coordinator) is allowed in Core per specification section 5. OS-facing pieces such as the global hotkey and single-instance ownership live in `Launcher.Integrations` behind Core interfaces and are wired in `Launcher.App`.

## Renderer
Production V1: **WinUI 3 + XAML + Windows Composition**.

React/TSX/HTML/CSS may be used for prototyping/reference work. WebView2 is not a default runtime and must not be added merely for visual effects.

## Plugin and IPC rules
Plugins are out-of-process and capability-limited. They never inherit Core privileges and cannot expand their own permissions.

Named Pipes are the initial general plugin IPC mechanism. The protocol must be versioned, request-correlated, cancellable, timeout-bounded, size-bounded, validated and resilient to process failure.

## Security
Default deny. Core runs unelevated. Privileged operations use a narrowly allowlisted broker. Risky actions require the documented confirmation/elevation policy.

## Technology baseline
- .NET 10 LTS
- C# 14
- .NET SDK 10.0.402 current release, exact installed SDK pinned in `global.json` during Phase 0
- Windows App SDK 2.5.1 Stable
- Windows SDK 10.0.26100.8249 baseline build (amended from 9457, ADR-0016)
- Minimum supported OS: Windows 11 24H2 (26100) or later (confirmed by the owner 2026-10-21, ADR-0015)

## Runtime rule
`global.json` selects the .NET SDK only. The repository separately pins Windows App SDK, Windows SDK, target/minimum platform versions and deployment model. APIs newer than the supported OS floor require runtime guards and compatibility tests.

## Change control
Architecture-boundary changes, security-sensitive changes, new persistent data, major dependency changes and deployment-model changes require an ADR in `DECISIONS.md`.
