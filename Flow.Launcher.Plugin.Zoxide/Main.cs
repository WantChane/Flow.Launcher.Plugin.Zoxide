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
    public class Main : IAsyncPlugin, ISettingProvider, IPluginI18n, IContextMenu, IAsyncReloadable
    {
        internal static PluginInitContext Context = null!;

        internal static Settings Settings { get; private set; } = null!;


        public async Task<List<Result>> QueryAsync(Query query, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(Settings.ZoxideExePath) || !ZoxideHelper.IsPathValid)
                return ZoxideSetupRequiredResultFactory.Create();

            var execution = await ZoxideHelper.ZoxideQueryAsync(
                Settings.ZoxideExePath,
                query.Search,
                token);

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

            await ZoxideHelper.ValidateAsync(Settings.ZoxideExePath);
        }

        public Control CreateSettingPanel()
        {
            var vm = new SettingsViewModel(Settings, Context);
            return new SettingsControl(vm);
        }

        public string GetTranslatedPluginDescription()
        {
            return Context.API.GetTranslation("flowlauncher_plugin_zoxide_description");
        }

        public string GetTranslatedPluginTitle()
        {
            return Context.API.GetTranslation("flowlauncher_plugin_zoxide_name");
        }

        public async Task ReloadDataAsync()
        {
            await ZoxideHelper.ValidateAsync(Settings.ZoxideExePath);
        }
    }
}