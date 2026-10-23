using Microsoft.UI.Xaml;

namespace Launcher.App
{
    /// <summary>
    /// Empty placeholder window from the project template.
    /// </summary>
    /// <remarks>
    /// Phase 0 only needs a window that opens, to prove the unpackaged WinUI build. The command bar,
    /// activation visual and monitor placement are Phase 1 work and are added in the Phase 1 tasks, not here.
    /// </remarks>
    public sealed partial class MainWindow : Window
    {
        /// <summary>
        /// Creates the window and builds its XAML content.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
