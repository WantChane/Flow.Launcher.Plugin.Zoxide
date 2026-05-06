using Flow.Launcher.Plugin.Zoxide.Helper;
using Flow.Launcher.Plugin.Zoxide.Models;
using Flow.Launcher.Plugin.Zoxide.Results;
using Flow.Launcher.Plugin.Zoxide.ViewModels;
using Flow.Launcher.Plugin.Zoxide.Views;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Flow.Launcher.Plugin.Zoxide
{
    public class Main : IAsyncPlugin, ISettingProvider, IPluginI18n, IContextMenu, IAsyncReloadable, IAsyncDialogJump
    {
        internal static PluginInitContext Context = null!;

        internal static Settings Settings { get; private set; } = null!;

        public async Task<List<Result>> QueryAsync(Query query, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(Settings.ZoxideExePath) || !ZoxideHelper.IsPathValid)
                return ZoxideSetupRequiredResultFactory.Create();

            if (Settings.CacheExpirationSeconds > 0)
            {
                var entries = await ZoxideCacheHelper.GetEntriesAsync(
                    query.Search,
                    token);
                if (entries.Count == 0)
                {
                    return ZoxideQueryResultFactory.NoMatchesResults();
                }
                return ZoxideQueryResultFactory.FromEntries(entries);
            }

            var execution = await ZoxideHelper.ZoxideQueryAsync(
                Settings.ZoxideExePath,
                query.Search,
                token);

            return ZoxideQueryResultFactory.FromQueryExecution(execution);
        }

        public async Task<List<DialogJumpResult>> QueryDialogJumpAsync(Query query, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(Settings.ZoxideExePath) || !ZoxideHelper.IsPathValid)
                return [.. ZoxideSetupRequiredResultFactory.Create().Select(r => DialogJumpResult.From(r, string.Empty))];

            if (Settings.CacheExpirationSeconds > 0)
            {
                var entries = await ZoxideCacheHelper.GetEntriesAsync(
                    query.Search,
                    token);
                if (entries.Count == 0)
                {
                    var noMatch = ZoxideQueryResultFactory.NoMatchesResults();
                    return [.. noMatch.Select(r => DialogJumpResult.From(r, string.Empty))];
                }
                var cachedResults = ZoxideQueryResultFactory.FromEntries(entries);
                return [.. cachedResults.Select(r => DialogJumpResult.From(r, r.CopyText))];
            }

            var execution = await ZoxideHelper.ZoxideQueryAsync(
                Settings.ZoxideExePath,
                query.Search,
                token);
            var results = ZoxideQueryResultFactory.FromQueryExecution(execution);
            return [.. results.Select(r => DialogJumpResult.From(r, r.CopyText))];
        }

        public List<Result> LoadContextMenus(Result selectedResult)
        {
            return ZoxideContextMenuFactory.Create(selectedResult, Settings, Context);
        }

        public async Task InitAsync(PluginInitContext context)
        {
            Context = context;
            Settings = context.API.LoadSettingJsonStorage<Settings>();

            if (BootstrapHelper.EnsureCommandsAndDefault(Settings, context))
                context.API.SaveSettingJsonStorage<Settings>();

            await ZoxideHelper.ValidateAsync(Settings.ZoxideExePath);
        }

        public Control CreateSettingPanel()
        {
            var vm = new SettingsViewModel(Settings, Context);
            return new SettingsControl(vm);
        }

        public async Task ReloadDataAsync()
        {
            await ZoxideHelper.ValidateAsync(Settings.ZoxideExePath);
        }

        public string GetTranslatedPluginTitle()
        {
            return Context.API.GetTranslation("flowlauncher_plugin_zoxide_name");
        }

        public string GetTranslatedPluginDescription()
        {
            return Context.API.GetTranslation("flowlauncher_plugin_zoxide_description");
        }
    }
}