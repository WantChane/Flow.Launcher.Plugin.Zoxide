using System.Collections.Generic;
namespace Flow.Launcher.Plugin.Zoxide.Results
{
    public static class ZoxideSetupRequiredResultFactory
    {
        internal const string ZoxideProjectUrl = "https://github.com/ajeetdsouza/zoxide";

        public static List<Result> Create()
        {
            var api = Main.Context.API;
            var ico = Main.Context.CurrentPluginMetadata.IcoPath;

            return
            [
                new Result
                {
                    Title = api.GetTranslation("flowlauncher_plugin_zoxide_result_setup_title"),
                    SubTitle = api.GetTranslation("flowlauncher_plugin_zoxide_result_setup_subtitle"),
                    IcoPath = ico,
                    Score = 42,
                    Action = _ =>
                    {
                        api.OpenSettingDialog();
                        return true;
                    }
                },
                new Result
                {
                    Title = api.GetTranslation("flowlauncher_plugin_zoxide_result_install_title"),
                    SubTitle = api.GetTranslation("flowlauncher_plugin_zoxide_result_install_subtitle"),
                    IcoPath = ico,
                    Score = 41,
                    Action = _ =>
                    {
                        api.OpenUrl(ZoxideProjectUrl);
                        return true;
                    }
                }
            ];
        }
    }
}
