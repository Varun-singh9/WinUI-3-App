using Microsoft.UI.Xaml;

namespace Launcher.App
{
    /// <summary>
    /// WinUI application object and entry point for the launcher process.
    /// </summary>
    /// <remarks>
    /// Phase 0 placeholder: it only proves that the unpackaged WinUI project builds and starts.
    /// Single-instance ownership, hotkey registration and the activation lifecycle belong to
    /// Launcher.Core and are introduced in Phase 1, not here.
    /// </remarks>
    public partial class App : Application
    {
        /// <summary>
        /// Placeholder window created from the project template; replaced by the real launcher
        /// window in Phase 1. Held in a field so the window reference lives as long as the app.
        /// </summary>
        private Window? _window;

        /// <summary>
        /// Initializes the application object. Runs before any window exists and is the
        /// equivalent of Main() for a WinUI app.
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called by the framework when the process is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _window.Activate();
        }
    }
}
