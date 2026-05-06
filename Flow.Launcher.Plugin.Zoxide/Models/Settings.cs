using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Flow.Launcher.Plugin.Zoxide.Models
{
    public partial class Settings : ObservableObject
    {
        [ObservableProperty]
        private string _zoxideExePath = string.Empty;

        [ObservableProperty]
        private string _defaultCommand = string.Empty;

        [ObservableProperty]
        private int _commandTimeoutMs = 1000;

        [ObservableProperty]
        private int _cacheExpirationSeconds;

        [ObservableProperty]
        private bool _fishStyleEnabled;

        [ObservableProperty]
        private int _pathTruncationLength;

        [ObservableProperty]
        private string _truncationSymbol = "…";

        public ObservableCollection<Command> Commands { get; set; } = [];
    }
}
