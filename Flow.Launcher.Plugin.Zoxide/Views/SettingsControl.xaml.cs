using Flow.Launcher.Plugin.Zoxide.ViewModels;
using System.Windows.Controls;

namespace Flow.Launcher.Plugin.Zoxide.Views
{
    public partial class SettingsControl : UserControl
    {
        public SettingsControl(SettingsViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
            Unloaded += (_, _) => viewModel.Dispose();
        }
    }
}
