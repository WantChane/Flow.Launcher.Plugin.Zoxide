using Flow.Launcher.Plugin.Zoxide.Helper;
using Flow.Launcher.Plugin.Zoxide.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Flow.Launcher.Plugin.Zoxide.Results
{
    public static class ZoxideQueryResultFactory
    {
        public static List<Result> FromQueryExecution(ZoxideCommandExecutionResult execution)
        {
            var api = Main.Context.API;
            var ico = Main.Context.CurrentPluginMetadata.IcoPath;

            if (!execution.IsSuccess)
                return FromExecutionFailure(execution, api, ico);

            var raw = execution.StandardOutput;
            var entries = ZoxideHelper.ParseQueries(raw);
            if (entries.Count > 0)
                return FromEntries(entries);

            if (string.IsNullOrWhiteSpace(raw))
                return NoMatchesResults(api, ico);

            if (HasLineWithVisibleContent(raw))
            {
                return
                [
                    new Result
                    {
                        Title = api.GetTranslation("flowlauncher_plugin_zoxide_query_parse_failed_title"),
                        SubTitle = string.Format(
                            api.GetTranslation("flowlauncher_plugin_zoxide_query_parse_failed_subtitle"),
                            raw),
                        IcoPath = ico,
                        Score = 42,
                    }
                ];
            }

            return NoMatchesResults(api, ico);
        }

        private static List<Result> NoMatchesResults(IPublicAPI api, string ico)
        {
            return
            [
                new Result
                {
                    Title = api.GetTranslation("flowlauncher_plugin_zoxide_query_nomatches_title"),
                    SubTitle = api.GetTranslation("flowlauncher_plugin_zoxide_query_nomatches_subtitle"),
                    IcoPath = ico,
                    Score = 42,
                }
            ];
        }

        private static List<Result> FromExecutionFailure(
            ZoxideCommandExecutionResult execution,
            IPublicAPI api,
            string ico)
        {
            string title;
            string subtitle;

            switch (execution.Type)
            {
                case ZoxideCommandExecutionType.TimedOut:
                    title = api.GetTranslation("flowlauncher_plugin_zoxide_query_timeout_title");
                    subtitle = api.GetTranslation("flowlauncher_plugin_zoxide_query_timeout_subtitle");
                    break;
                case ZoxideCommandExecutionType.Cancelled:
                    title = api.GetTranslation("flowlauncher_plugin_zoxide_query_cancelled_title");
                    subtitle = api.GetTranslation("flowlauncher_plugin_zoxide_query_cancelled_subtitle");
                    break;
                case ZoxideCommandExecutionType.NonZeroExit:
                    title = string.Format(
                        api.GetTranslation("flowlauncher_plugin_zoxide_query_exit_title"),
                        execution.ExitCode ?? -1);
                    subtitle = string.IsNullOrEmpty(execution.StandardError)
                        ? api.GetTranslation("flowlauncher_plugin_zoxide_query_exit_subtitle_no_stderr")
                        : string.Format(
                            api.GetTranslation("flowlauncher_plugin_zoxide_query_exit_subtitle_stderr"),
                            execution.StandardError);
                    break;
                case ZoxideCommandExecutionType.ProcessStartFailed:
                    title = api.GetTranslation("flowlauncher_plugin_zoxide_query_start_failed_title");
                    subtitle = api.GetTranslation("flowlauncher_plugin_zoxide_query_start_failed_subtitle");
                    break;
                case ZoxideCommandExecutionType.InvalidZoxidePath:
                    title = api.GetTranslation("flowlauncher_plugin_zoxide_query_invalid_path_title");
                    subtitle = api.GetTranslation("flowlauncher_plugin_zoxide_query_invalid_path_subtitle");
                    break;
                case ZoxideCommandExecutionType.Exception:
                    title = api.GetTranslation("flowlauncher_plugin_zoxide_query_exception_title");
                    subtitle = string.Format(
                        api.GetTranslation("flowlauncher_plugin_zoxide_query_exception_subtitle"),
                        execution.ExceptionMessage ?? string.Empty);
                    break;
                default:
                    title = api.GetTranslation("flowlauncher_plugin_zoxide_query_unknown_failure_title");
                    subtitle = api.GetTranslation("flowlauncher_plugin_zoxide_query_unknown_failure_subtitle");
                    break;
            }

            return
            [
                new Result
                {
                    Title = title,
                    SubTitle = subtitle,
                    IcoPath = ico,
                    Score = 42,
                }
            ];
        }

        private static bool HasLineWithVisibleContent(string? rawOutput)
        {
            if (string.IsNullOrWhiteSpace(rawOutput))
                return false;

            foreach (var line in rawOutput.Split(["\r\n", "\r", "\n"], StringSplitOptions.None))
            {
                if (!string.IsNullOrWhiteSpace(line))
                    return true;
            }

            return false;
        }

        public static List<Result> FromEntries(IReadOnlyList<ZoxideEntry> entries)
        {
            if (entries.Count == 0)
                return [];

            var list = new List<Result>(entries.Count);
            foreach (var entry in entries)
                list.Add(FromEntry(entry));

            return list;
        }

        private static Result FromEntry(ZoxideEntry entry)
        {
            var path = entry.Path;
            return new Result
            {
                Title = FormatTitle(path),
                SubTitle = path,
                IcoPath = Main.Context.CurrentPluginMetadata.IcoPath,
                AddSelectedCount = false,
                Score = entry.Score,
                ContextData = path,
                Action = context =>
                {
                    var outcome = CommandHelper.TryOpenPath(path);
                    CommandHelper.SyncZoxideAfterPathOpen(Main.Settings.ZoxideExePath, path, outcome);
                    return true;
                }
            };
        }

        private static string FormatTitle(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            var trimmed = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var name = Path.GetFileName(trimmed);
            return string.IsNullOrEmpty(name) ? path : name;
        }
    }
}
