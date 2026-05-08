using Flow.Launcher.Plugin.Zoxide.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.Zoxide.Helper
{
    public static class ZoxideCacheHelper
    {
        private const string CacheKey = "zoxide_all_data";

        public static async Task ClearCacheAsync()
        {
            var _context = Main.Context;
            var cacheDirectory = _context.CurrentPluginMetadata.PluginCacheDirectoryPath;
            var cache = await _context.API.LoadCacheBinaryStorageAsync(CacheKey, cacheDirectory, new CachedZoxideEntries());
            cache.CachedAt = DateTime.UtcNow;
            cache.Entries = [];
            await _context.API.SaveCacheBinaryStorageAsync<CachedZoxideEntries>(CacheKey, cacheDirectory);
        }

        public static async Task<IReadOnlyList<ZoxideEntry>> GetEntriesAsync(
            string? searchTerms,
            CancellationToken token)
        {
            var _settings = Main.Settings;
            var _context = Main.Context;
            var cacheDirectory = _context.CurrentPluginMetadata.PluginCacheDirectoryPath;

            var cache = await _context.API.LoadCacheBinaryStorageAsync(CacheKey, cacheDirectory, new CachedZoxideEntries());

            var isExpired = cache.Entries.Count == 0
                || cache.CachedAt.AddSeconds(_settings.CacheExpirationSeconds) < DateTime.UtcNow;

            if (isExpired)
            {
                var result = await ZoxideHelper.ZoxideQueryAsync(_settings.ZoxideExePath, null, token);
                if (result.IsSuccess)
                {
                    cache.Entries = [.. ZoxideHelper.ParseQueries(result.StandardOutput)];
                    cache.CachedAt = DateTime.UtcNow;
                    await _context.API.SaveCacheBinaryStorageAsync<CachedZoxideEntries>(CacheKey, cacheDirectory);
                }
            }

            return FilterEntries(cache.Entries, searchTerms);
        }

        public static IReadOnlyList<ZoxideEntry> FilterEntries(
            IReadOnlyList<ZoxideEntry> entries,
            string? searchTerms)
        {
            if (string.IsNullOrWhiteSpace(searchTerms))
                return [.. entries.OrderByDescending(e => e.Score)];


            var matchedEntries = new List<ZoxideEntry>();

            foreach (var entry in entries)
            {
                var matchResult = Main.Context.API.FuzzySearch(searchTerms, entry.Path);

                if (matchResult.Success)
                {
                    matchedEntries.Add(entry);
                }
            }

            return [.. matchedEntries.OrderByDescending(e => e.Score)];
        }
    }
}
