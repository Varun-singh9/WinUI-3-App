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
| Minimum OS | Windows 11 24H2 / build 26100+ | Owner gate |
| Architecture | x64 initially; Arm64 evaluation before release | Phase 0 verify |
| Deployment | To be selected and recorded before scaffold freeze | Phase 0 |
| SQLite | Exact library/package selected during implementation | Pending |

## Third-Party Dependencies

| Dependency | Version | License | Source | Why | Runtime/resource impact | Removal/replacement plan | ADR |
|---|---|---|---|---|---|---|---|
| None approved yet | — | — | — | — | — | — | — |

## Release dependency review
Before release verify exact versions, license compatibility, notices, unexpected runtime/network/elevation behavior, package provenance, and final inventory.

## Version-source note
V3.0 was refreshed against Microsoft release information on 2026-10-20. The repository must not treat this document as a substitute for actual SDK/package pin files. Phase 0 records the machine-verified values used for the first reproducible build.
