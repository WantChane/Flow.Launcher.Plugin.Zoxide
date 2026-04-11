using Flow.Launcher.Plugin.Zoxide.Models;
using Flow.Launcher.Plugin.Zoxide.ViewModels;
using System.Windows;

namespace Flow.Launcher.Plugin.Zoxide.Views
{
    public partial class CommandEditWindow : Window
    {
        public Command? ResultNewCommand => (DataContext as CommandEditViewModel)?.ResultNewCommand;

        public CommandEditWindow(CommandEditViewModel viewModel)
        {
            viewModel.CloseRequested += (_, dialogResult) => DialogResult = dialogResult;
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
