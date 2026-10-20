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
- Windows SDK 10.0.26100.9457 baseline build
- Minimum supported OS: Windows 11 24H2 (26100) or later, pending the explicit Phase 0 owner gate

## Runtime rule
`global.json` selects the .NET SDK only. The repository separately pins Windows App SDK, Windows SDK, target/minimum platform versions and deployment model. APIs newer than the supported OS floor require runtime guards and compatibility tests.

## Change control
Architecture-boundary changes, security-sensitive changes, new persistent data, major dependency changes and deployment-model changes require an ADR in `DECISIONS.md`.
