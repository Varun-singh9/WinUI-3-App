using Launcher.Core.Lifecycle;
using Microsoft.UI.Xaml;

namespace Launcher.App
{
    /// <summary>
    /// WinUI application object and entry point for the launcher process.
    /// </summary>
    /// <remarks>
    /// Starts the runtime built by <see cref="AppComposition"/> (ADR-0028). A second copy of the launcher asks the
    /// running one to activate and then exits. There is no custom Main, so a second copy pays the WinUI startup cost
    /// before it exits. Everything about startup, the hotkey and activation is logged by the runtime and its
    /// components; this class adds only the unhandled-exception entries.
    /// </remarks>
    public partial class App : Application
    {
        private AppComposition? _composition;

        /// <summary>
        /// A window that is created but never activated, so the process has a UI host and stays alive until the real
        /// launcher window replaces it in Task 5. Held in a field so the reference lives as long as the app.
        /// </summary>
        private Window? _window;

        /// <summary>
        /// Initializes the application object. Runs before any window exists and is the
        /// equivalent of Main() for a WinUI app.
        /// </summary>
        public App()
        {
            InitializeComponent();
            UnhandledException += OnUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        /// <summary>
        /// Called by the framework when the process is launched. Starts the runtime. If another launcher already
        /// owns the user's session, this process exits.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            try
            {
                _composition = AppComposition.Create();
                var result = await _composition.Runtime.StartAsync(CancellationToken.None);

                if (result.Status == ApplicationStartStatus.AnotherInstanceRunning)
                {
                    // The running launcher was asked to activate and the runtime has logged it. Nothing is left to do.
                    await ReleaseAndExitAsync();
                    return;
                }

                _window = new MainWindow();
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"Launcher startup failed: {exception.GetType().Name}");
                if (_composition is not null)
                {
                    CrashLog.Write(_composition.Diagnostics, "startup", exception, fatal: true);
                }

                Environment.Exit(1);
            }
        }

        private async Task ReleaseAndExitAsync()
        {
            if (_composition is not null)
            {
                await _composition.DisposeAsync();
                _composition = null;
            }

            Exit();
        }

        private void OnUnhandledException(object? sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            // Handled is left false on purpose: a real fault is logged and still ends the process.
            LogUnhandled("application", e.Exception, fatal: true);
        }

        private void OnAppDomainUnhandledException(object? sender, System.UnhandledExceptionEventArgs e)
        {
            LogUnhandled("app_domain", e.ExceptionObject as Exception, e.IsTerminating);
        }

        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            LogUnhandled("task_scheduler", e.Exception, fatal: false);
        }

        private void LogUnhandled(string source, Exception? exception, bool fatal)
        {
            var diagnostics = _composition?.Diagnostics;
            if (diagnostics is not null)
            {
                CrashLog.Write(diagnostics, source, exception, fatal);
            }
        }
    }
}
