using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Flow.Launcher.Plugin.Zoxide.Helper
{
    public static class IconHelper
    {
        public static readonly string ProgramDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location!)!.ToString();

        private static readonly string ImagesDirectory = Path.Combine(ProgramDirectory, "Images");

        public static readonly string ErrorIcon = Path.Combine(ImagesDirectory, "Error.png");

        public static readonly string SearchIcon = Path.Combine(ImagesDirectory, "search.png");

        public static readonly string SettingsIcon = Path.Combine(ImagesDirectory, "settings.png");

        public static readonly string BrowserIcon = Path.Combine(ImagesDirectory, "Browser.png");

        public static readonly string FolderIcon = Path.Combine(ImagesDirectory, "folder.png");

        public static readonly string ExclamationIcon = Path.Combine(ImagesDirectory, "Exclamation.png");

        public static readonly string OkIcon = Path.Combine(ImagesDirectory, "ok.png");

        public static readonly string CloseIcon = Path.Combine(ImagesDirectory, "close.png");

        public static readonly string CmdIcon = Path.Combine(ImagesDirectory, "cmd.png");

        public static readonly string ImgNotFoundIcon = Path.Combine(ImagesDirectory, "image-not-found.png");

        public static readonly string PluginIcon = Path.Combine(ImagesDirectory, "icon.png");

        public static string ResolveIconPath(string icon)
        {
            if (string.IsNullOrEmpty(icon))
                return PluginIcon;

            var isBareFilename = Path.GetFileName(icon) == icon;
            if (isBareFilename)
            {
                return Path.Combine(ImagesDirectory, icon);
            }

            var isExistingFile = File.Exists(icon);
            if (isExistingFile)
            {
                return icon;
            }
            else
            {
                return ImgNotFoundIcon;
            }
        }
    }

    public record IconOption(string FileName, string DisplayName);

    public static class IconOptions
    {
        public static readonly IReadOnlyList<IconOption> SelectableIcons =
        [
            new("", "(Default)"),
            new("Browser.png", "Browser"),
            new("close.png", "Close"),
            new("cmd.png", "Cmd"),
            new("Error.png", "Error"),
            new("Exclamation.png", "Exclamation"),
            new("folder.png", "Folder"),
            new("ok.png", "OK"),
            new("search.png", "Search"),
            new("settings.png", "Settings"),
        ];
    }
}
