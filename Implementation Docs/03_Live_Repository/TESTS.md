# Test Matrix — V3.0

Result values: `TODO`, `PASS`, `FAIL`, or `BLOCKED`. Compilation alone never makes a test PASS.

| Area | Test | Result | Notes |
|---|---|---|---|
| Build | Clean restore/build | TODO | |
| Build | Debug build | TODO | |
| Build | Release build | TODO | |
| Build | CI-equivalent build | TODO | |
| Architecture | Project references obey dependency direction | TODO | |
| Runtime | Single-instance behavior | TODO | |
| Hotkey | Opens launcher | TODO | |
| Activation | Cold loop starts immediately | TODO | |
| Activation | Cold loop → critical readiness → morph | TODO | |
| Activation | Warm open | TODO | |
| Activation | Optional providers never block bar | TODO | |
| Monitor | Active-monitor placement | TODO | |
| DPI | Mixed DPI/scaling | TODO | |
| Prefix | Parser | TODO | |
| Prefix | Text + symbol aliases resolve identically | TODO | |
| Prefix | Unknown prefix fallback | TODO | |
| Prefix | Popup keyboard navigation | TODO | |
| Prefix | Popup pointer interaction | TODO | |
| Menu | Customization interaction | TODO | |
| Menu | Customization without hover | TODO | |
| Search | Fuzzy matching | TODO | |
| Search | Central ranking | TODO | |
| Search | Progressive results | TODO | |
| Search | Cancellation | TODO | |
| Search | Stale result protection | TODO | |
| Provider | Health states | TODO | |
| Provider | Optional fallback | TODO | |
| Provider | Crash isolation | TODO | |
| Actions | Primary action | TODO | |
| Actions | Argument validation | TODO | |
| Actions | Medium+ confirmation | TODO | |
| Actions | No animation-only execution delay | TODO | |
| Security | Default-deny capability | TODO | |
| Security | Invalid capability rejected | TODO | |
| Security | Plugin cannot self-expand permissions | TODO | |
| Security | Malformed IPC rejected | TODO | |
| Security | IPC timeout/cancellation | TODO | |
| Security | Plugin death handled | TODO | |
| Security | Broker allowlist | TODO | |
| Security | No silent elevation | TODO | |
| Data | Configuration validation | TODO | |
| Data | Database migration | TODO | |
| Data | Migration recovery | TODO | |
| Data | Export/import round-trip | TODO | |
| Data | Reset behavior | TODO | |
| Privacy | No unintended network activity | TODO | |
| Privacy | Search exclusions/private mode | TODO | |
| Privacy | Secrets not in plain configuration | TODO | |
| UI | Keyboard-only operation | TODO | |
| UI | Focus restoration | TODO | |
| UI | UI Automation names/roles | TODO | |
| Accessibility | High contrast/system accessibility | TODO | |
| Accessibility | Reduced Motion | TODO | |
| Motion | Activation performance | TODO | |
| Motion | Prefix popup animation | TODO | |
| Motion | Customization menu animation | TODO | |
| Motion | Cursor-reactive fallback | TODO | |
| Motion | Cursor-reactive field never changes hit targets | TODO | |
| Performance | Cold activation benchmark | TODO | |
| Performance | Warm activation benchmark | TODO | |
| Performance | Search latency benchmark | TODO | |
| Performance | Idle memory baseline | TODO | |
| Performance | Idle CPU baseline | TODO | |
| Windows | Supported OS matrix | TODO | |
| Windows | API runtime guards | TODO | |
| Installer | Clean install | TODO | |
| Installer | Upgrade | TODO | |
| Installer | Repair | TODO | |
| Installer | Uninstall | TODO | |
| Installer | User-data preservation | TODO | |
| Release | Application/installer signing | TODO | |
| Release | Dependency/license inventory | TODO | |
| Release | User-initiated verified update/recovery | TODO | |
| Release | Rollback | TODO | |
| Release | SHA-256 artifact integrity | TODO | |
| Release | Authenticode publisher verification | TODO | |
| Workflow | Privileged workflow step unavailable before Phase 7 | TODO | |
| Workflow | Denied workflow step terminates visibly | TODO | |

## Rule
A phase is complete only after the applicable automated tests, manual verification, performance checks and security checks pass.
