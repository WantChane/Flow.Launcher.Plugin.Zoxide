using CommunityToolkit.Mvvm.ComponentModel;

namespace Flow.Launcher.Plugin.Zoxide.Models
{
    public partial class Command : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _executable = string.Empty;

        [ObservableProperty]
        private string _arguments = string.Empty;

        [ObservableProperty]
        private bool _isEnabled = true;

        [ObservableProperty]
        private bool _zoxideAddOnSuccess = true;

        [ObservableProperty]
        private string _icon = string.Empty;

        public static Command Clone(Command source) => new()
        {
            Name = source.Name,
            Executable = source.Executable,
            Arguments = source.Arguments,
            IsEnabled = source.IsEnabled,
            ZoxideAddOnSuccess = source.ZoxideAddOnSuccess,
            Icon = source.Icon,
        };

        public void CopyFrom(Command other)
        {
            Name = other.Name;
            Executable = other.Executable;
            Arguments = other.Arguments;
            IsEnabled = other.IsEnabled;
            ZoxideAddOnSuccess = other.ZoxideAddOnSuccess;
            Icon = other.Icon;
        }
    }
}
