using System.Collections.ObjectModel;

namespace Flow.Launcher.Plugin.Zoxide.Helper
{
    public static class ObservableCollectionHelper
    {
        public static bool CanMoveUp<T>(ObservableCollection<T> list, T? item) where T : class
        {
            if (item is null)
                return false;
            var i = list.IndexOf(item);
            return i > 0;
        }

        public static bool CanMoveDown<T>(ObservableCollection<T> list, T? item) where T : class
        {
            if (item is null)
                return false;
            var i = list.IndexOf(item);
            return i >= 0 && i < list.Count - 1;
        }

        public static void MoveUp<T>(ObservableCollection<T> list, T? item) where T : class
        {
            if (item is null)
                return;
            var i = list.IndexOf(item);
            if (i > 0)
                list.Move(i, i - 1);
        }

        public static void MoveDown<T>(ObservableCollection<T> list, T? item) where T : class
        {
            if (item is null)
                return;
            var i = list.IndexOf(item);
            if (i >= 0 && i < list.Count - 1)
                list.Move(i, i + 1);
        }
    }
}
