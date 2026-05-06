using System.Collections.ObjectModel;

namespace Flow.Launcher.Plugin.Zoxide.Models
{
    public class Settings
    {
        public string ZoxideExePath { get; set; } = string.Empty;

        public string DefaultCommand { get; set; } = string.Empty;

        public int CommandTimeoutMs { get; set; } = 1000;

        public int CacheExpirationSeconds { get; set; } = 0;

        public ObservableCollection<Command> Commands { get; set; } = [];
    }
}
