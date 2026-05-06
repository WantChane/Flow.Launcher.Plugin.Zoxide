using Flow.Launcher.Plugin.Zoxide.Helper;
using Flow.Launcher.Plugin.Zoxide.Models;
using System;
using System.Collections.Generic;

using System.Linq;

namespace Flow.Launcher.Plugin.Zoxide.Results
{
    public static class ZoxideQueryResultFactory
    {
        public static List<Result> FromQueryExecution(ZoxideCommandExecutionResult execution)
        {
            var _api = Main.Context.API;

            if (!execution.IsSuccess)
                return FromExecutionFailure(execution);

            var raw = execution.StandardOutput;
            var entries = ZoxideHelper.ParseQueries(raw);
            if (entries.Count > 0)
                return FromEntries(entries);

            if (string.IsNullOrWhiteSpace(raw))
                return NoMatchesResults();

            if (HasLineWithVisibleContent(raw))
            {
                return
                [
                    new Result
                    {
                        Title = _api.GetTranslation("flowlauncher_plugin_zoxide_query_parse_failed_title"),
                        SubTitle = string.Format(
                            _api.GetTranslation("flowlauncher_plugin_zoxide_query_parse_failed_subtitle"),
                            raw),
                        IcoPath = IconHelper.ErrorIcon,
                        AddSelectedCount = false,
                        Score = 42,
                    }
                ];
            }

            return NoMatchesResults();
        }

        internal static List<Result> NoMatchesResults()
        {
            var _api = Main.Context.API;
            return
            [
                new Result
                {
                    Title = _api.GetTranslation("flowlauncher_plugin_zoxide_query_nomatches_title"),
                    SubTitle = _api.GetTranslation("flowlauncher_plugin_zoxide_query_nomatches_subtitle"),
                    IcoPath = IconHelper.ExclamationIcon,
                    AddSelectedCount = false,
                    Score = 42,
                }
            ];
        }

        private static List<Result> FromExecutionFailure(
            ZoxideCommandExecutionResult execution)
        {
            string title;
            string subtitle;

            var _api = Main.Context.API;

            switch (execution.Type)
            {
                case ZoxideCommandExecutionType.TimedOut:
                    title = _api.GetTranslation("flowlauncher_plugin_zoxide_query_timeout_title");
                    subtitle = _api.GetTranslation("flowlauncher_plugin_zoxide_query_timeout_subtitle");
                    break;
                case ZoxideCommandExecutionType.Cancelled:
                    title = _api.GetTranslation("flowlauncher_plugin_zoxide_query_cancelled_title");
                    subtitle = _api.GetTranslation("flowlauncher_plugin_zoxide_query_cancelled_subtitle");
                    break;
                case ZoxideCommandExecutionType.NonZeroExit:
                    title = string.Format(
                        _api.GetTranslation("flowlauncher_plugin_zoxide_query_exit_title"),
                        execution.ExitCode ?? -1);
                    subtitle = string.IsNullOrEmpty(execution.StandardError)
                        ? _api.GetTranslation("flowlauncher_plugin_zoxide_query_exit_subtitle_no_stderr")
                        : string.Format(
                            _api.GetTranslation("flowlauncher_plugin_zoxide_query_exit_subtitle_stderr"),
                            execution.StandardError);
                    break;
                case ZoxideCommandExecutionType.ProcessStartFailed:
                    title = _api.GetTranslation("flowlauncher_plugin_zoxide_query_start_failed_title");
                    subtitle = _api.GetTranslation("flowlauncher_plugin_zoxide_query_start_failed_subtitle");
                    break;
                case ZoxideCommandExecutionType.InvalidZoxidePath:
                    title = _api.GetTranslation("flowlauncher_plugin_zoxide_query_invalid_path_title");
                    subtitle = _api.GetTranslation("flowlauncher_plugin_zoxide_query_invalid_path_subtitle");
                    break;
                case ZoxideCommandExecutionType.Exception:
                    title = _api.GetTranslation("flowlauncher_plugin_zoxide_query_exception_title");
                    subtitle = string.Format(
                        _api.GetTranslation("flowlauncher_plugin_zoxide_query_exception_subtitle"),
                        execution.ExceptionMessage ?? string.Empty);
                    break;
                default:
                    title = _api.GetTranslation("flowlauncher_plugin_zoxide_query_unknown_failure_title");
                    subtitle = _api.GetTranslation("flowlauncher_plugin_zoxide_query_unknown_failure_subtitle");
                    break;
            }

            return
            [
                new Result
                {
                    Title = title,
                    SubTitle = subtitle,
                    IcoPath = IconHelper.ErrorIcon,
                    AddSelectedCount = false,
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

            var defaultCommand = Main.Settings?.Commands
                .FirstOrDefault(c => c.Name == Main.Settings.DefaultCommand && c.IsEnabled);
            var ico = IconHelper.ResolveIconPath(defaultCommand?.Icon ?? "");

            var list = new List<Result>(entries.Count);
            foreach (var entry in entries)
                list.Add(FromEntry(entry, ico));

            return list;
        }

        private static Result FromEntry(ZoxideEntry entry, string ico)
        {
            var path = entry.Path;

            return new Result
            {
                Title = PathFormatter.FormatTitle(path),
                SubTitle = PathFormatter.FormatParentPath(path, Main.Settings),
                IcoPath = ico,
                AddSelectedCount = false,
                Score = entry.Score,
                ContextData = path,
                CopyText = path,
                Action = context =>
                {
                    var outcome = CommandHelper.TryOpenPath(path);
                    CommandHelper.SyncZoxideAfterPathOpen(Main.Settings!.ZoxideExePath, path, outcome);
                    _ = ZoxideCacheHelper.ClearCache();
                    return true;
                }
            };
        }
    }
}
