using Flow.Launcher.Plugin.Zoxide.Models;
using System;
using System.IO;

namespace Flow.Launcher.Plugin.Zoxide.Helper
{
    public static class PathFormatter
    {
        public static string TruncatePath(string path, Settings? settings)
        {
            if (string.IsNullOrEmpty(path) || settings == null || settings.PathTruncationLength <= 0)
                return path;

            var root = Path.GetPathRoot(path) ?? string.Empty;
            if (root.Length >= path.Length)
                return path;

            var remaining = path[root.Length..];
            var sep = Path.DirectorySeparatorChar;
            var parts = remaining.Split(
                [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
                return path;

            if (settings.FishStyleEnabled)
                return TruncateFishStyle(root, parts, settings.PathTruncationLength, sep);
            else
                return TruncateHead(root, parts, settings.PathTruncationLength, settings.TruncationSymbol, sep);
        }

        private static string TruncateFishStyle(string root, string[] parts, int length, char sep)
        {
            var shortened = new string[parts.Length];
            for (var i = 0; i < parts.Length - 1; i++)
            {
                var p = parts[i];
                shortened[i] = p.Length > length ? p[..length] : p;
            }
            shortened[^1] = parts[^1];
            return root + string.Join(sep, shortened);
        }

        private static string TruncateHead(string root, string[] parts, int length, string symbol, char sep)
        {
            if (length >= parts.Length)
                return root + string.Join(sep, parts);

            var kept = parts[^length..];
            return symbol + sep + string.Join(sep, kept);
        }

        public static string FormatTitle(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            var trimmed = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var name = Path.GetFileName(trimmed);
            return string.IsNullOrEmpty(name) ? path : name;
        }

        public static string FormatParentPath(string path, Settings? settings)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            var trimmed = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var parent = Path.GetDirectoryName(trimmed);
            return string.IsNullOrEmpty(parent) ? path : TruncatePath(parent, settings);
        }
    }
}
