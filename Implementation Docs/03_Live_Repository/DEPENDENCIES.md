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
| Windows SDK | 10.0.26100.8249 baseline build (amended from 9457, ADR-0016) | Approved baseline |
| Target framework | `net10.0-windows10.0.26100.0` | Phase 0 verify |
| Minimum OS | Windows 11 24H2 / build 26100+ | Approved (owner, 2026-10-21, ADR-0015) |
| Architecture | x64 only for now; Arm64 evaluation before release | Selected (ADR-0014) |
| Deployment | Unpackaged, framework-dependent | Selected (ADR-0014); confirm interpretation before installer work |
| SQLite | Exact library/package selected during implementation | Pending |

## Phase 0 recorded values and deviations (2026-10-21)

| Item | Documented | Observed on owner machine | Treatment |
|---|---|---|---|
| Windows SDK | 10.0.26100.8249 (amended from 9457, ADR-0016) | 10.0.26100.8249 installed | Resolved: documented baseline amended to match; BuildTools pinned to the same build |
| Windows SDK 10.0.28000 | Not adopted | Not present in the Windows Kits Include folder when checked | Must not be used; `Microsoft.Windows.SDK.BuildTools` 10.0.28000.x is excluded too |
| .NET SDK | 10.0.402 | 10.0.402 (also 10.0.401, 9.0.318) | Pinned in `global.json`, rollForward `latestPatch`, allowPrerelease false |
| Windows App SDK | 2.5.1 Stable | 2.5.1 runtime and NuGet package present | Pinned in `Directory.Packages.props`; verify package id on first VS restore |
| Visual Studio | Record at Phase 0 | Community 2026 18.11 stable, WinUI workload | Recorded |
| Host OS | 24H2 floor | Windows 11 25H2 (26200.9457) x64 | Dev machine is above the floor; floor testing needs a 24H2 machine |

### Resolved package versions for Launcher.App (owner machine, 2026-10-21)

Output of `dotnet list package --include-transitive`, pasted by the owner. Direct references are pinned in `Directory.Packages.props`; the rest are resolved transitively through Windows App SDK 2.5.1.

| Package | Resolved version | Origin |
|---|---|---|
| Microsoft.WindowsAppSDK | 2.5.1 | Direct, pinned |
| Microsoft.Windows.SDK.BuildTools | 10.0.26100.8249 | Direct, pinned (see ADR-0014 addendum) |
| Microsoft.WindowsAppSDK.Runtime | 2.5.1 | Transitive |
| Microsoft.WindowsAppSDK.WinUI | 2.3.9 | Transitive |
| Microsoft.WindowsAppSDK.Foundation | 2.3.12 | Transitive |
| Microsoft.WindowsAppSDK.Base | 2.0.4 | Transitive |
| Microsoft.WindowsAppSDK.InteractiveExperiences | 2.1.9 | Transitive |
| Microsoft.WindowsAppSDK.DWrite | 2.1.0 | Transitive |
| Microsoft.WindowsAppSDK.AI | 2.5.5 | Transitive |
| Microsoft.WindowsAppSDK.ML | 2.1.94 | Transitive |
| Microsoft.WindowsAppSDK.Search | 2.5.5 | Transitive |
| Microsoft.WindowsAppSDK.Widgets | 2.0.5 | Transitive |
| Microsoft.Windows.SDK.BuildTools.MSIX | 1.7.251221100 | Transitive |

The Windows App SDK component packages version independently of the 2.5.1 umbrella; the differing numbers above are the observed resolution result. The AI, ML, Search and Widgets component packages arrive with the umbrella package and are not used by any project code. They are platform components, not separate third-party dependencies.

## Third-Party Dependencies

| Dependency | Version | License | Source | Why | Runtime/resource impact | Removal/replacement plan | ADR |
|---|---|---|---|---|---|---|---|
| xunit.v3 | 4.0.1 | Apache-2.0 (confirm at release review) | nuget.org | Test framework, runs on Microsoft Testing Platform v2 (ADR-0017) | Test-time only; not shipped; test projects become executables; transitive MTP packages include telemetry components (ADR-0017 addendum) | Replace with another framework and rewrite test attributes/assertions | ADR-0017 |

Transitive packages pulled in by `xunit.v3` are recorded after the first restore that matches the final package set (`dotnet list package --include-transitive`). The earlier list, taken while the VSTest-only packages were still referenced, is superseded and is not recorded here.

## Release dependency review
Before release verify exact versions, license compatibility, notices, unexpected runtime/network/elevation behavior, package provenance, and final inventory.

## Version-source note
V3.0 was refreshed against Microsoft release information on 2026-10-20. The repository must not treat this document as a substitute for actual SDK/package pin files. Phase 0 records the machine-verified values used for the first reproducible build.
