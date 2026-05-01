using Flow.Launcher.Plugin.Zoxide.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.Zoxide.Helper
{
    public partial class ZoxideHelper
    {
        private static readonly Regex VersionRegex = VersionReg();
        private const int DefaultTimeout = 1000;
        internal const int MinCommandTimeoutMs = 100;
        internal const int MaxCommandTimeoutMs = 300_000;
        internal static bool IsPathValid { get; private set; } = false;
        internal static string? CurrentVersion { get; private set; }

        /// <summary>
        /// 使用 <see cref="ZoxideVersionAsync"/> 验证给定的 zoxide 可执行文件路径是否有效，并更新 <see cref="IsPathValid"/> 和 <see cref="CurrentVersion"/> 状态。
        /// </summary>
        /// <param name="zoxidePath">zoxide 可执行文件路径</param>
        /// <returns>如果路径有效且能够获取版本信息，则返回 <c>true</c>；否则返回 <c>false</c></returns>
        public static async Task<bool> ValidateAsync(string zoxidePath)
        {
            IsPathValid = false;
            CurrentVersion = null;
            if (string.IsNullOrWhiteSpace(zoxidePath))
            {
                return false;
            }

            var versionString = await ZoxideVersionAsync(zoxidePath);
            if (string.IsNullOrWhiteSpace(versionString))
            {
                return false;
            }

            var version = ParseVersion(versionString);
            if (string.IsNullOrWhiteSpace(version))
            {
                return false;
            }

            IsPathValid = true;
            CurrentVersion = version;
            return true;

        }

        /// <summary>
        /// 运行 <c>zoxide query -l -s [query]</c>，列出匹配目录（含分数）。
        /// </summary>
        /// <param name="zoxidePath">zoxide 可执行文件路径</param>
        /// <param name="query">搜索关键词；为空或仅空白时列出全部</param>
        public static Task<ZoxideCommandExecutionResult> ZoxideQueryAsync(
            string zoxidePath,
            string? query,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<string> args = string.IsNullOrWhiteSpace(query)
                ? ["query", "-l", "-s"]
                : ["query", "-l", "-s", .. query.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)];

            return ExecuteCommandAsync(zoxidePath, args, cancellationToken);
        }

        /// <summary>
        /// 运行 <c>zoxide add &lt;path&gt;</c>，将目录路径加入 zoxide 数据库。
        /// </summary>
        /// <param name="zoxidePath">zoxide 可执行文件路径</param>
        /// <param name="path">要加入的目录路径</param>
        public static Task<ZoxideCommandExecutionResult> ZoxideAddAsync(
            string zoxidePath,
            string path,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(path))
                return Task.FromResult(ZoxideCommandExecutionResult.InvalidPath());

            return ExecuteCommandAsync(zoxidePath, ["add", path.Trim()], cancellationToken);
        }

        /// <summary>
        /// 运行 <c>zoxide remove &lt;path&gt;</c>，从 zoxide 数据库中移除指定目录。
        /// </summary>
        /// <param name="zoxidePath">zoxide 可执行文件路径</param>
        /// <param name="path">要从数据库中移除的目录路径</param>
        public static Task<ZoxideCommandExecutionResult> ZoxideRemoveAsync(
            string zoxidePath,
            string path,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(path))
                return Task.FromResult(ZoxideCommandExecutionResult.InvalidPath());

            return ExecuteCommandAsync(zoxidePath, ["remove", path.Trim()], cancellationToken);
        }

        /// <summary>
        /// 将 <see cref="ZoxideQueryAsync"/> 的文本输出按行解析为 <see cref="ZoxideEntry"/> 列表；
        /// 无法解析的行会被跳过。
        /// </summary>
        /// <param name="queryListOutput"><see cref="ZoxideQueryAsync"/> 的文本输出</param>
        /// <returns>解析得到的 <see cref="ZoxideEntry"/> 列表；如果输入为空或无有效行，则返回空列表</returns>
        public static IReadOnlyList<ZoxideEntry> ParseQueries(string? queryListOutput)
        {
            if (string.IsNullOrWhiteSpace(queryListOutput))
                return [];

            var lines = queryListOutput.Split(
                ["\r\n", "\r", "\n"],
                StringSplitOptions.RemoveEmptyEntries);

            var list = new List<ZoxideEntry>();
            foreach (var line in lines)
            {
                var entry = ParseQuery(line);
                if (entry != null)
                    list.Add(entry);
            }

            return list;
        }

        private static ZoxideEntry? ParseQuery(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return null;

            const char separator = ' ';
            var trimmed = line.Trim();
            var spaceIndex = trimmed.IndexOf(separator);

            if (spaceIndex <= 0 || spaceIndex >= trimmed.Length - 1)
                return null;

            var scoreStr = trimmed[..spaceIndex];
            var pathStr = trimmed[(spaceIndex + 1)..];

            if (!double.TryParse(scoreStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var score) || score <= 0)
                return null;

            return new ZoxideEntry
            {
                Score = (int)Math.Round(score * 10),
                Path = pathStr
            };
        }

        private static async Task<string?> ZoxideVersionAsync(string zoxidePath)
        {
            var r = await ExecuteCommandAsync(zoxidePath, ["--version"]);
            return r.IsSuccess ? r.StandardOutput?.Trim() : null;
        }

        private static string? ParseVersion(string versionOutput)
        {
            if (string.IsNullOrWhiteSpace(versionOutput))
                return null;

            var m = VersionRegex.Match(versionOutput.Trim());
            return m.Success ? m.Value : null;
        }

        public static int NormalizeCommandTimeoutMs(int configuredMs)
        {
            if (configuredMs <= 0)
                return DefaultTimeout;
            return Math.Clamp(configuredMs, MinCommandTimeoutMs, MaxCommandTimeoutMs);
        }

        private static int GetCommandTimeoutMs()
        {
            var s = Main.Settings;
            return s is null ? DefaultTimeout : NormalizeCommandTimeoutMs(s.CommandTimeoutMs);
        }

        private static async Task<ZoxideCommandExecutionResult> ExecuteCommandAsync(
            string zoxidePath,
            IReadOnlyList<string> arguments,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(zoxidePath))
                return ZoxideCommandExecutionResult.InvalidPath();

            var timeoutMs = GetCommandTimeoutMs();

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = zoxidePath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                foreach (var arg in arguments)
                    startInfo.ArgumentList.Add(arg);

                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    Main.Context?.API.LogWarn("ZoxideHelper", $"Process.Start returned null: {zoxidePath}");
                    return ZoxideCommandExecutionResult.StartFailed();
                }

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(timeoutMs);

                var outputTask = process.StandardOutput.ReadToEndAsync(cts.Token);
                var errorTask = process.StandardError.ReadToEndAsync(cts.Token);
                var exitTask = process.WaitForExitAsync(cts.Token);

                try
                {
                    await Task.WhenAll(outputTask, errorTask, exitTask);
                }
                catch (OperationCanceledException)
                {
                    TryKillProcessTree(process);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Main.Context?.API.LogWarn("ZoxideHelper", $"Command cancelled: {string.Join(" ", arguments)}");
                        return ZoxideCommandExecutionResult.Cancelled();
                    }

                    Main.Context?.API.LogWarn("ZoxideHelper", $"Command timeout: {string.Join(" ", arguments)}");
                    return ZoxideCommandExecutionResult.TimedOut();
                }

                var output = await outputTask;
                var error = await errorTask;

                if (process.ExitCode != 0)
                {
                    Main.Context?.API.LogWarn(
                        "ZoxideHelper",
                        $"Command failed with exit code {process.ExitCode}: {error}");
                    return ZoxideCommandExecutionResult.BadExit(process.ExitCode, error);
                }

                return ZoxideCommandExecutionResult.Ok(output.Trim());
            }
            catch (Exception ex)
            {
                Main.Context?.API.LogException(
                    "ZoxideHelper",
                    $"Command error: {string.Join(" ", arguments)}",
                    ex);
                return ZoxideCommandExecutionResult.FromException(ex);
            }
        }

        private static void TryKillProcessTree(Process process)
        {
            try
            {
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException)
            {
            }
        }

        [GeneratedRegex(@"\d+\.\d+\.\d+", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
        private static partial Regex VersionReg();
    }
}
