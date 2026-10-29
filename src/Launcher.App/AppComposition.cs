using Launcher.Core.Diagnostics;
using Launcher.Core.Lifecycle;
using Launcher.Data;
using Launcher.Diagnostics;
using Launcher.Integrations.Hotkey;
using Launcher.Integrations.SingleInstance;

namespace Launcher.App;

/// <summary>
/// The composition root: it builds the launcher's runtime from the types that already exist and owns the objects the
/// runtime only borrows (ADR-0028). It adds no behavior of its own. Everything it logs is logged by the components.
/// </summary>
/// <remarks>
/// The critical readiness set is empty until the command bar arrives in Task 5, so a cold activation completes at once.
/// The hotkey is <see cref="HotkeyDefaults.LauncherGesture"/> and the readiness timeout is the 5 second default.
/// </remarks>
internal sealed class AppComposition : IAsyncDisposable
{
    private readonly HotkeyService _hotkey;
    private readonly SingleInstanceOwner _singleInstance;

    private AppComposition(IApplicationRuntime runtime, IDiagnostics diagnostics, HotkeyService hotkey, SingleInstanceOwner singleInstance)
    {
        Runtime = runtime;
        Diagnostics = diagnostics;
        _hotkey = hotkey;
        _singleInstance = singleInstance;
    }

    /// <summary>Gets the application runtime.</summary>
    public IApplicationRuntime Runtime { get; }

    /// <summary>Gets the diagnostics every component writes to, for the few entries the app itself must add.</summary>
    public IDiagnostics Diagnostics { get; }

    /// <summary>
    /// Builds the runtime for the current user.
    /// </summary>
    /// <param name="locations">Where data lives. When null, the current user's default location is used.</param>
    /// <returns>The composed runtime and the objects it borrows.</returns>
    public static AppComposition Create(DataLocations? locations = null)
    {
        locations ??= DataLocations.ForCurrentUser();

        var diagnostics = new DiagnosticsService([new JsonLinesFileSink(locations.LogFilePath)]);
        var lifecycle = new LauncherLifecycle(diagnostics);
        var readiness = new ReadinessCoordinator([], ApplicationRuntimeOptions.DefaultCriticalReadinessTimeout, diagnostics);
        var activation = new ActivationCoordinator(lifecycle, readiness, diagnostics);
        var singleInstance = new SingleInstanceOwner(SingleInstanceNames.Default, diagnostics);
        var hotkey = new HotkeyService(diagnostics);
        var options = new ApplicationRuntimeOptions(HotkeyDefaults.LauncherGesture, ApplicationRuntimeOptions.DefaultCriticalReadinessTimeout);
        var runtime = new ApplicationRuntime(options, lifecycle, activation, hotkey, singleInstance, diagnostics);

        return new AppComposition(runtime, diagnostics, hotkey, singleInstance);
    }

    /// <summary>
    /// Shuts the runtime down, then releases the hotkey service and the single-instance owner. Never throws.
    /// </summary>
    /// <returns>A task that completes when everything is released.</returns>
    public async ValueTask DisposeAsync()
    {
        try
        {
            // Idempotent: a no-op when the runtime never started or another instance owns the launcher.
            await Runtime.ShutdownAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            // The process is ending. The failure is recorded and must not stop the rest of the cleanup.
            CrashLog.Write(Diagnostics, "shutdown", exception, fatal: false);
        }

        _hotkey.Dispose();
        _singleInstance.Dispose();
    }
}
