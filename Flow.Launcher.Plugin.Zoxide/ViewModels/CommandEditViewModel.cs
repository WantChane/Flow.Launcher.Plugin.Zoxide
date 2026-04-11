using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Launcher.Plugin.Zoxide.Models;
using System;

namespace Flow.Launcher.Plugin.Zoxide.ViewModels
{
    public partial class CommandEditViewModel : ObservableObject
    {
        private readonly Command? _cmd;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OkCommand))]
        private string _name = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OkCommand))]
        private string _executable = string.Empty;

        [ObservableProperty]
        private string _arguments = string.Empty;

        [ObservableProperty]
        private bool _zoxideAddOnSuccess = true;

        public Command? ResultNewCommand { get; private set; }

        public event EventHandler<bool>? CloseRequested;

        public CommandEditViewModel(Command? cmd)
        {
            if (cmd is not null)
            {
                _cmd = cmd;
                Name = cmd.Name;
                Executable = cmd.Executable;
                Arguments = cmd.Arguments;
                ZoxideAddOnSuccess = cmd.ZoxideAddOnSuccess;
            }
        }

        private bool CanOk() =>
            !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Executable);

        [RelayCommand(CanExecute = nameof(CanOk))]
        private void Ok()
        {
            if (_cmd is not null)
            {
                _cmd.Name = Name;
                _cmd.Executable = Executable;
                _cmd.Arguments = Arguments;
                _cmd.ZoxideAddOnSuccess = ZoxideAddOnSuccess;
            }
            else
            {
                ResultNewCommand = new Command
                {
                    Name = Name,
                    Executable = Executable,
                    Arguments = Arguments,
                    IsEnabled = true,
                    ZoxideAddOnSuccess = ZoxideAddOnSuccess,
                };
            }

            CloseRequested?.Invoke(this, true);
        }

        [RelayCommand]
        private void Cancel() => CloseRequested?.Invoke(this, false);
    }
}
