using Flow.Launcher.Plugin.Zoxide.Models;
using System;
using System.Collections.Generic;

namespace Flow.Launcher.Plugin.Zoxide.Helper
{
    public static class FuzzyMatchHelper
    {
        public static IReadOnlyList<ZoxideEntry> Filter(
            IReadOnlyList<ZoxideEntry> entries,
            string searchTerms)
        {
            var keywords = SplitKeywords(searchTerms);

            var matched = new List<ZoxideEntry>();

            foreach (var entry in entries)
            {
                if (keywords.Length == 0 || IsMatchZoxide(keywords, entry.Path))
                {
                    matched.Add(entry);
                }
                else if (HasNonAscii(entry.Path) && IsMatchPinyin(keywords, entry.Path))
                {
                    matched.Add(entry);
                }
            }

            return matched;
        }

        private static string[] SplitKeywords(string searchTerms)
        {
            var trimmed = searchTerms.Trim();
            if (trimmed.Length == 0)
                return [];

            var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
                parts[i] = parts[i].ToLowerInvariant();
            return parts;
        }

        private static bool IsMatchZoxide(string[] keywords, string path)
        {
            if (keywords.Length == 0) return true;

            string lower = path.ToLowerInvariant();

            for (int i = keywords.Length - 1; i >= 0; i--)
            {
                int idx = lower.LastIndexOf(keywords[i], StringComparison.Ordinal);
                if (idx < 0) return false;

                if (i == keywords.Length - 1)
                {
                    int after = idx + keywords[i].Length;
                    for (int k = after; k < lower.Length; k++)
                    {
                        if (lower[k] == '/' || lower[k] == '\\')
                            return false;
                    }
                }

                lower = lower[..idx];
            }

            return true;
        }

        private static bool HasNonAscii(string text)
        {
            foreach (char c in text)
            {
                if (c > 127) return true;
            }
            return false;
        }

        private static bool IsMatchPinyin(string[] keywords, string path)
        {
            var api = Main.Context.API;
            for (int i = 0; i < keywords.Length; i++)
            {
                var result = api.FuzzySearch(keywords[i], path);
                if (!result.Success)
                    return false;
            }
            return true;
        }
    }
}
