using Flow.Launcher.Plugin.Zoxide.Helper;
using Flow.Launcher.Plugin.Zoxide.Models;
using Flow.Launcher.Plugin.Zoxide.Results;
using Flow.Launcher.Plugin.Zoxide.ViewModels;
using Flow.Launcher.Plugin.Zoxide.Views;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Flow.Launcher.Plugin.Zoxide
{
    public class Main : IAsyncPlugin, ISettingProvider, IPluginI18n, IContextMenu, IAsyncReloadable, IAsyncDialogJump
    {
        internal static PluginInitContext Context = null!;

        internal static Settings Settings { get; private set; } = null!;

        public Task<List<Result>> QueryAsync(Query query, CancellationToken token)
            => QueryCoreAsync(query, token);

        public async Task<List<DialogJumpResult>> QueryDialogJumpAsync(Query query, CancellationToken token)
        {
            var results = await QueryCoreAsync(query, token).ConfigureAwait(false);
            return results.ConvertAll(r => DialogJumpResult.From(r, r.CopyText ?? string.Empty));
        }

        private static async Task<List<Result>> QueryCoreAsync(Query query, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(Settings.ZoxideExePath) || !ZoxideHelper.IsPathValid)
                return ZoxideSetupRequiredResultFactory.Create();

            if (Settings.CacheExpirationSeconds > 0)
            {
                var entries = await ZoxideCacheHelper.GetEntriesAsync(query.Search, token).ConfigureAwait(false);
                if (entries.Count == 0)
                    return ZoxideQueryResultFactory.NoMatchesResults();
                return ZoxideQueryResultFactory.FromEntries(entries);
            }

            var execution = await ZoxideHelper.ZoxideQueryAsync(Settings.ZoxideExePath, query.Search, token).ConfigureAwait(false);
            return ZoxideQueryResultFactory.FromQueryExecution(execution);
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

            await ZoxideHelper.ValidateAsync(Settings.ZoxideExePath).ConfigureAwait(false);
        }

        public Control CreateSettingPanel()
        {
            var vm = new SettingsViewModel(Settings, Context);
            return new SettingsControl(vm);
        }

        public async Task ReloadDataAsync()
        {
            await ZoxideHelper.ValidateAsync(Settings.ZoxideExePath).ConfigureAwait(false);
            await ZoxideCacheHelper.ClearCacheAsync().ConfigureAwait(false);
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