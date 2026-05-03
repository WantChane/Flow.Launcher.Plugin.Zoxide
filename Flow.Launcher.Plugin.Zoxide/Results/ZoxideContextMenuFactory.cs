using Flow.Launcher.Plugin.Zoxide.Helper;
using Flow.Launcher.Plugin.Zoxide.Models;
using System.Collections.Generic;

namespace Flow.Launcher.Plugin.Zoxide.Results
{
    public static class ZoxideContextMenuFactory
    {
        public static List<Result> Create(Result selectedResult, Settings settings, PluginInitContext context)
        {
            if (selectedResult.ContextData is not string path || string.IsNullOrWhiteSpace(path))
                return [];

            var list = new List<Result>();

            foreach (var cmd in settings.Commands)
            {
                if (!cmd.IsEnabled)
                    continue;

                var command = cmd;
                list.Add(new Result
                {
                    Title = command.Name,
                    SubTitle = command.Executable,
                    AddSelectedCount = false,
                    IcoPath = IconHelper.ResolveIconPath(command.Icon),
                    Action = ctx =>
                    {
                        var outcome = CommandHelper.TryOpenPath(path, command);
                        CommandHelper.SyncZoxideAfterPathOpen(settings.ZoxideExePath, path, outcome, command);
                        return true;
                    }
                });
            }

            return list;
        }
    }
}
