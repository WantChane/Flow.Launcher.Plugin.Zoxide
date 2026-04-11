using System;

namespace Flow.Launcher.Plugin.Zoxide.Models
{
    public enum ZoxideCommandExecutionType
    {
        Success,
        TimedOut,
        Cancelled,
        NonZeroExit,
        ProcessStartFailed,
        InvalidZoxidePath,
        Exception,
    }

    public sealed class ZoxideCommandExecutionResult
    {
        public ZoxideCommandExecutionType Type { get; init; }

        public string? StandardOutput { get; init; }

        public string? StandardError { get; init; }

        public int? ExitCode { get; init; }

        public string? ExceptionMessage { get; init; }

        public bool IsSuccess => Type == ZoxideCommandExecutionType.Success;

        public static ZoxideCommandExecutionResult Ok(string? output) => new()
        {
            Type = ZoxideCommandExecutionType.Success,
            StandardOutput = output,
        };

        public static ZoxideCommandExecutionResult TimedOut() => new()
        {
            Type = ZoxideCommandExecutionType.TimedOut,
        };

        public static ZoxideCommandExecutionResult Cancelled() => new()
        {
            Type = ZoxideCommandExecutionType.Cancelled,
        };

        public static ZoxideCommandExecutionResult BadExit(int code, string? stderr) => new()
        {
            Type = ZoxideCommandExecutionType.NonZeroExit,
            ExitCode = code,
            StandardError = string.IsNullOrWhiteSpace(stderr) ? null : stderr.Trim(),
        };

        public static ZoxideCommandExecutionResult StartFailed() => new()
        {
            Type = ZoxideCommandExecutionType.ProcessStartFailed,
        };

        public static ZoxideCommandExecutionResult InvalidPath() => new()
        {
            Type = ZoxideCommandExecutionType.InvalidZoxidePath,
        };

        public static ZoxideCommandExecutionResult FromException(Exception ex) => new()
        {
            Type = ZoxideCommandExecutionType.Exception,
            ExceptionMessage = ex.Message,
        };
    }
}
