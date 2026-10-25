namespace Launcher.Core.Lifecycle;

/// <summary>
/// The legal transitions between <see cref="LauncherState"/> values in Phase 1 (ADR-0026). A state that is not listed
/// as a source has no transitions yet: PrefixOpen, MenuOpen, Searching and Executing arrive with their phases, each
/// with its own record and tests. Anything not in this table is rejected, so the lifecycle fails closed.
/// </summary>
internal static class LifecycleTransitions
{
    private static readonly Dictionary<LauncherState, LauncherState[]> Table = new()
    {
        // Cold activation starts at Activating; warm activation goes straight to Ready.
        [LauncherState.Hidden] = [LauncherState.Activating, LauncherState.Ready],
        [LauncherState.Activating] = [LauncherState.CriticalReady, LauncherState.Closing, LauncherState.ErrorRecovery],
        [LauncherState.CriticalReady] = [LauncherState.Morphing, LauncherState.Closing, LauncherState.ErrorRecovery],
        [LauncherState.Morphing] = [LauncherState.Ready, LauncherState.Closing, LauncherState.ErrorRecovery],
        [LauncherState.Ready] = [LauncherState.Closing, LauncherState.ErrorRecovery],
        [LauncherState.Closing] = [LauncherState.Hidden],
        [LauncherState.ErrorRecovery] = [LauncherState.Hidden],
    };

    public static bool IsAllowed(LauncherState from, LauncherState to)
    {
        return Table.TryGetValue(from, out var targets) && targets.Contains(to);
    }
}
