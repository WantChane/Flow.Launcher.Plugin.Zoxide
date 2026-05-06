using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Launcher.Plugin.Zoxide.Helper;
using Flow.Launcher.Plugin.Zoxide.Models;
using Flow.Launcher.Plugin.Zoxide.Views;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.Zoxide.ViewModels
{
    public partial class SettingsViewModel : ObservableObject, IDisposable
    {
        private readonly Settings _settings;
        private readonly PluginInitContext _context;
        private string? _lastSuccessfulTestPath;
        private bool _disposed;

        public Settings Settings => _settings;

        public SettingsViewModel(Settings settings, PluginInitContext context)
        {
            _settings = settings;
            _context = context;

            if (_settings.CommandTimeoutMs <= 0)
                _settings.CommandTimeoutMs = 0;

            _settings.PropertyChanged += OnSettingsPropertyChanged;
            _settings.Commands.CollectionChanged += OnCommandsCollectionChanged;
            foreach (var cmd in _settings.Commands)
                cmd.PropertyChanged += OnCommandPropertyChanged;

            RefreshEnabledCommandNames();
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;

            _settings.PropertyChanged -= OnSettingsPropertyChanged;
            _settings.Commands.CollectionChanged -= OnCommandsCollectionChanged;
            foreach (var cmd in _settings.Commands)
                cmd.PropertyChanged -= OnCommandPropertyChanged;

            GC.SuppressFinalize(this);
        }

        public ObservableCollection<Command> Commands => _settings.Commands;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(
            nameof(DeleteCustomCommandCommand),
            nameof(ModifyCustomCommandCommand),
            nameof(EnableSelectedCommand),
            nameof(DisableSelectedCommand),
            nameof(MoveSelectedCommandUpCommand),
            nameof(MoveSelectedCommandDownCommand))]
        private Command? _selectedCommand;

        public ObservableCollection<string> EnabledCommandNames { get; } = [];

        private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Settings.ZoxideExePath):
                    SaveCommand.NotifyCanExecuteChanged();
                    break;
                case nameof(Settings.DefaultCommand):
                    DeleteCustomCommandCommand.NotifyCanExecuteChanged();
                    DisableSelectedCommand.NotifyCanExecuteChanged();
                    break;
            }
        }

        [RelayCommand]
        private void Browse()
        {
            var dialog = new OpenFileDialog
            {
                Title = _context.API.GetTranslation("flowlauncher_plugin_zoxide_dialog_browse_title"),
                Filter = _context.API.GetTranslation("flowlauncher_plugin_zoxide_dialog_browse_filter"),
                FileName = "zoxide.exe",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
                _settings.ZoxideExePath = dialog.FileName;
        }

        [RelayCommand]
        private async Task TestAsync()
        {
            var path = _settings.ZoxideExePath;
            var success = await ZoxideHelper.ValidateAsync(path).ConfigureAwait(true);

            if (success)
            {
                _lastSuccessfulTestPath = path;
                var ver = ZoxideHelper.CurrentVersion ?? "?";
                _context.API.ShowMsg(
                    _context.API.GetTranslation("flowlauncher_plugin_zoxide_msg_test_success"),
                    string.Format(_context.API.GetTranslation("flowlauncher_plugin_zoxide_msg_test_success_detail"), ver),
                    IconHelper.OkIcon);
            }
            else
            {
                if (string.Equals(path, _lastSuccessfulTestPath, StringComparison.OrdinalIgnoreCase))
                    _lastSuccessfulTestPath = null;
                _context.API.ShowMsgError(
                    _context.API.GetTranslation("flowlauncher_plugin_zoxide_msg_test_failure"),
                    string.IsNullOrWhiteSpace(path)
                        ? _context.API.GetTranslation("flowlauncher_plugin_zoxide_msg_test_failure_detail_nopath")
                        : string.Format(_context.API.GetTranslation("flowlauncher_plugin_zoxide_msg_test_failure_detail_other"), path));
            }

            SaveCommand.NotifyCanExecuteChanged();
        }

        private bool CanSave() =>
            !string.IsNullOrWhiteSpace(_settings.ZoxideExePath)
            && string.Equals(_settings.ZoxideExePath.Trim(), _lastSuccessfulTestPath?.Trim(), StringComparison.OrdinalIgnoreCase);

        [RelayCommand(CanExecute = nameof(CanSave))]
        private void Save()
        {
            _context.API.SaveSettingJsonStorage<Settings>();
        }

        [RelayCommand]
        private void AddCustomCommand()
        {
            var dlg = new CommandEditWindow(new CommandEditViewModel(null));
            if (dlg.ShowDialog() == true && dlg.ResultNewCommand is { } cmd)
                Commands.Add(cmd);
        }

        private bool CanDeleteCustomCommand()
        {
            if (SelectedCommand is null)
                return false;
            if (IsDefaultCommand(SelectedCommand))
                return false;
            if (!SelectedCommand.IsEnabled)
                return true;
            return CountEnabledExcept(SelectedCommand) >= 1;
        }

        [RelayCommand(CanExecute = nameof(CanDeleteCustomCommand))]
        private void DeleteCustomCommand()
        {
            if (SelectedCommand is null)
                return;
            Commands.Remove(SelectedCommand);
            SelectedCommand = null;
        }

        private bool CanModifyCustomCommand() => SelectedCommand is not null;

        [RelayCommand(CanExecute = nameof(CanModifyCustomCommand))]
        private void ModifyCustomCommand()
        {
            if (SelectedCommand is null)
                return;
            var wasDefault = IsDefaultCommand(SelectedCommand);
            var vm = new CommandEditViewModel(SelectedCommand);
            new CommandEditWindow(vm).ShowDialog();
            if (wasDefault)
                _settings.DefaultCommand = SelectedCommand.Name;
            RefreshEnabledCommandNames();
        }

        private bool CanEnableSelected() =>
            SelectedCommand is not null && !SelectedCommand.IsEnabled;

        [RelayCommand(CanExecute = nameof(CanEnableSelected))]
        private void EnableSelected()
        {
            if (SelectedCommand is null)
                return;
            SelectedCommand.IsEnabled = true;
        }

        private bool CanDisableSelected() =>
            SelectedCommand is not null
            && SelectedCommand.IsEnabled
            && !IsDefaultCommand(SelectedCommand)
            && CountEnabledExcept(SelectedCommand) >= 1;

        [RelayCommand(CanExecute = nameof(CanDisableSelected))]
        private void DisableSelected()
        {
            if (SelectedCommand is null)
                return;
            SelectedCommand.IsEnabled = false;
        }

        private bool CanMoveSelectedCommandUp() =>
            ObservableCollectionHelper.CanMoveUp(Commands, SelectedCommand);

        [RelayCommand(CanExecute = nameof(CanMoveSelectedCommandUp))]
        private void MoveSelectedCommandUp()
        {
            if (SelectedCommand is null)
                return;
            ObservableCollectionHelper.MoveUp(Commands, SelectedCommand);
        }

        private bool CanMoveSelectedCommandDown() =>
            ObservableCollectionHelper.CanMoveDown(Commands, SelectedCommand);

        [RelayCommand(CanExecute = nameof(CanMoveSelectedCommandDown))]
        private void MoveSelectedCommandDown()
        {
            if (SelectedCommand is null)
                return;
            ObservableCollectionHelper.MoveDown(Commands, SelectedCommand);
        }

        [RelayCommand]
        private void SaveCommands()
        {
            _context.API.SaveSettingJsonStorage<Settings>();
        }

        private static int CountEnabledExcept(ObservableCollection<Command> commands, Command? exclude)
        {
            var n = 0;
            foreach (var c in commands)
            {
                if (ReferenceEquals(c, exclude))
                    continue;
                if (c.IsEnabled)
                    n++;
            }
            return n;
        }

        private int CountEnabledExcept(Command? exclude) => CountEnabledExcept(Commands, exclude);

        private bool IsDefaultCommand(Command cmd) =>
            string.Equals(cmd.Name, _settings.DefaultCommand, StringComparison.Ordinal);

        private void RefreshEnabledCommandNames()
        {
            EnabledCommandNames.Clear();
            foreach (var c in Commands)
            {
                if (c.IsEnabled)
                    EnabledCommandNames.Add(c.Name);
            }

            if (EnabledCommandNames.Count == 0)
            {
                if (!string.IsNullOrEmpty(_settings.DefaultCommand))
                    _settings.DefaultCommand = string.Empty;
                return;
            }

            if (string.IsNullOrEmpty(_settings.DefaultCommand) || !EnabledCommandNames.Contains(_settings.DefaultCommand))
                _settings.DefaultCommand = EnabledCommandNames[0];
        }

        private void OnCommandsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is not null)
            {
                foreach (Command c in e.NewItems)
                    c.PropertyChanged += OnCommandPropertyChanged;
            }

            if (e.OldItems is not null)
            {
                foreach (Command c in e.OldItems)
                    c.PropertyChanged -= OnCommandPropertyChanged;
            }

            RefreshEnabledCommandNames();
            DeleteCustomCommandCommand.NotifyCanExecuteChanged();
            EnableSelectedCommand.NotifyCanExecuteChanged();
            DisableSelectedCommand.NotifyCanExecuteChanged();
            MoveSelectedCommandUpCommand.NotifyCanExecuteChanged();
            MoveSelectedCommandDownCommand.NotifyCanExecuteChanged();
        }

        private void OnCommandPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is not (nameof(Command.IsEnabled) or nameof(Command.Name)))
                return;
            RefreshEnabledCommandNames();
            DeleteCustomCommandCommand.NotifyCanExecuteChanged();
            EnableSelectedCommand.NotifyCanExecuteChanged();
            DisableSelectedCommand.NotifyCanExecuteChanged();
        }
    }
}
