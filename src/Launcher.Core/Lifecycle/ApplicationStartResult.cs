using Launcher.Core.Integrations;

namespace Launcher.Core.Lifecycle;

/// <summary>
/// What happened when the runtime started. It lets a second instance learn that it should exit, and lets the UI show a
/// recoverable hotkey problem without the lifecycle entering an error state (ADR-0026).
/// </summary>
/// <param name="Status">The overall outcome.</param>
/// <param name="SingleInstance">The outcome of trying to become the single instance.</param>
/// <param name="OwnerSignaled">Whether the running owner was asked to activate. Only meaningful when the status is <see cref="ApplicationStartStatus.AnotherInstanceRunning"/>.</param>
/// <param name="Hotkey">The hotkey registration outcome, or <see langword="null"/> when no registration was attempted.</param>
public sealed record ApplicationStartResult(
    ApplicationStartStatus Status,
    SingleInstanceResult SingleInstance,
    bool OwnerSignaled,
    HotkeyRegistrationResult? Hotkey);
