using Flow.Launcher.Plugin.Zoxide.Models;
using System;
using System.Collections.ObjectModel;

namespace Flow.Launcher.Plugin.Zoxide.Helper
{
    internal static class BootstrapHelper
    {
        public static bool EnsureCommandsAndDefault(Settings settings, PluginInitContext context)
        {
            if (settings.Commands.Count == 0)
            {
                settings.Commands.Add(CreateExplorerDefaultCommand(context));
                settings.DefaultCommand = settings.Commands[0].Name;
                return true;
            }

            return EnsureDefaultCommandValid(settings, context);
        }

        private static bool EnsureDefaultCommandValid(Settings settings, PluginInitContext context)
        {
            var firstEnabled = FindFirstEnabledCommand(settings.Commands);
            if (firstEnabled is null)
            {
                var cmd = CreateExplorerDefaultCommand(context);
                settings.Commands.Add(cmd);
                settings.DefaultCommand = cmd.Name;
                return true;
            }

            if (string.IsNullOrWhiteSpace(settings.DefaultCommand)
                || !IsEnabledCommandName(settings.Commands, settings.DefaultCommand))
            {
                settings.DefaultCommand = firstEnabled.Name;
                return true;
            }

            return false;
        }

        private static Command CreateExplorerDefaultCommand(PluginInitContext context) => new()
        {
            Name = context.API.GetTranslation("flowlauncher_plugin_zoxide_command_default_name"),
            Executable = "explorer.exe",
            Arguments = "\"{path}\"",
            IsEnabled = true,
            Icon = "folder.png"
        };

        private static Command? FindFirstEnabledCommand(ObservableCollection<Command> commands)
        {
            foreach (var c in commands)
            {
                if (c.IsEnabled)
                    return c;
            }

            return null;
        }

        private static bool IsEnabledCommandName(ObservableCollection<Command> commands, string name)
        {
            foreach (var c in commands)
            {
                if (c.IsEnabled && string.Equals(c.Name, name, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }
    }
}
