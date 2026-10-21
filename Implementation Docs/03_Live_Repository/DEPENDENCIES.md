# Dependency and License Register — V3.0

## Status
Pre-implementation baseline. No third-party production package has been approved beyond the platform/toolchain requirements listed below.

## Rules
- Record every non-platform dependency.
- Record exact version, license, source, purpose and runtime/resource impact.
- Record the removal/replacement plan for non-foundational dependencies.
- No preview/experimental packages in production.
- Do not commit credentials, signing keys or other secrets.
- New dependencies follow change-control rules and receive an ADR when required.

## Platform / Toolchain Baseline

| Component | V3.0 baseline | Status |
|---|---|---|
| .NET | 10 LTS; current runtime 10.0.13 | Approved |
| .NET SDK | 10.0.402 listed by Microsoft on 2026-10-20; exact installed SDK is pinned in `global.json` at Phase 0 | Pin at Phase 0 |
| C# | 14 | Approved |
| Windows App SDK | 2.5.1 Stable | Approved |
| Windows SDK | 10.0.26100.9457 baseline build | Approved baseline |
| Target framework | `net10.0-windows10.0.26100.0` | Phase 0 verify |
| Minimum OS | Windows 11 24H2 / build 26100+ | Approved (owner, 2026-10-21, ADR-0015) |
| Architecture | x64 only for now; Arm64 evaluation before release | Selected (ADR-0014) |
| Deployment | Unpackaged, framework-dependent | Selected (ADR-0014); confirm interpretation before installer work |
| SQLite | Exact library/package selected during implementation | Pending |

## Phase 0 recorded values and deviations (2026-10-21)

| Item | Documented | Observed on owner machine | Treatment |
|---|---|---|---|
| Windows SDK | 10.0.26100.9457 | 10.0.26100.8249 installed | Deviation recorded in ADR-0014; baseline text above intentionally left unchanged |
| Windows SDK 10.0.28000 | Not adopted | Installed | Must not be used |
| .NET SDK | 10.0.402 | 10.0.402 (also 10.0.401, 9.0.318) | Pinned in `global.json`, rollForward `latestPatch`, allowPrerelease false |
| Windows App SDK | 2.5.1 Stable | 2.5.1 runtime and NuGet package present | Pinned in `Directory.Packages.props`; verify package id on first VS restore |
| Visual Studio | Record at Phase 0 | Community 2026 18.11 stable, WinUI workload | Recorded |
| Host OS | 24H2 floor | Windows 11 25H2 (26200.9457) x64 | Dev machine is above the floor; floor testing needs a 24H2 machine |

## Third-Party Dependencies

| Dependency | Version | License | Source | Why | Runtime/resource impact | Removal/replacement plan | ADR |
|---|---|---|---|---|---|---|---|
| None approved yet | — | — | — | — | — | — | — |

## Release dependency review
Before release verify exact versions, license compatibility, notices, unexpected runtime/network/elevation behavior, package provenance, and final inventory.

## Version-source note
V3.0 was refreshed against Microsoft release information on 2026-10-20. The repository must not treat this document as a substitute for actual SDK/package pin files. Phase 0 records the machine-verified values used for the first reproducible build.
