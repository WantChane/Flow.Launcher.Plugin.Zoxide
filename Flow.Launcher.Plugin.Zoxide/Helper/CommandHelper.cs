using Flow.Launcher.Plugin.SharedCommands;
using Flow.Launcher.Plugin.Zoxide.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Flow.Launcher.Plugin.Zoxide.Helper
{
    internal enum PathOpenOutcome
    {
        Succeeded,
        PathDoesNotExist,
        Failed,
    }

    internal static class CommandHelper
    {
        internal static PathOpenOutcome TryOpenPath(string path, Command? command = null)
        {
            var cmd = command ?? GetDefaultCommand(Main.Settings);
            if (string.IsNullOrWhiteSpace(path))
                return PathOpenOutcome.Failed;
            var trimmed = path.Trim();
            if (!Directory.Exists(trimmed))
                return PathOpenOutcome.PathDoesNotExist;

            if (cmd is null || string.IsNullOrWhiteSpace(cmd.Executable))
                return OpenPathFallback(trimmed);

            var arguments = (cmd.Arguments ?? string.Empty).Replace("{path}", trimmed, StringComparison.Ordinal);
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = cmd.Executable.Trim(),
                    Arguments = arguments,
                    UseShellExecute = true,
                });
                return PathOpenOutcome.Succeeded;
            }
            catch (Exception ex)
            {
                Main.Context.API.LogException("CommandHelper", "Failed to launch command", ex);
                Main.Context.API.ShowMsgError(ex.Message, string.Empty);
                return PathOpenOutcome.Failed;
            }
        }

        private static Command? GetDefaultCommand(Settings settings)
        {
            foreach (var c in settings.Commands)
            {
                if (c.IsEnabled && string.Equals(c.Name, settings.DefaultCommand, StringComparison.Ordinal))
                    return c;
            }

            return null;
        }

        private static PathOpenOutcome OpenPathFallback(string path)
        {
            var failed = false;
            FilesFolders.OpenPath(path, msg =>
            {
                failed = true;
                Main.Context.API.ShowMsgError(msg, string.Empty);
                return MessageBoxResult.OK;
            });
            return failed ? PathOpenOutcome.Failed : PathOpenOutcome.Succeeded;
        }

        internal static void SyncZoxideAfterPathOpen(
            string zoxideExePath,
            string path,
            PathOpenOutcome outcome,
            Command? command = null)
        {
            var cmd = command ?? GetDefaultCommand(Main.Settings);
            var zoxideAddOnSuccess = cmd?.ZoxideAddOnSuccess ?? true;
            switch (outcome)
            {
                case PathOpenOutcome.Succeeded:
                    if (zoxideAddOnSuccess)
                        _ = ZoxideHelper.ZoxideAddAsync(zoxideExePath, path);
                    break;
                case PathOpenOutcome.PathDoesNotExist:
                    _ = ZoxideHelper.ZoxideRemoveAsync(zoxideExePath, path);
                    break;
            }
        }
    }
}
