using Microsoft.UI.Xaml;

namespace Launcher.App
{
    /// <summary>
    /// Empty placeholder window from the project template.
    /// </summary>
    /// <remarks>
    /// Phase 0 only needs a window that opens, to prove the unpackaged WinUI build. The command bar,
    /// activation visual and monitor placement are Phase 1 and Phase 2 work and must not be added here.
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
