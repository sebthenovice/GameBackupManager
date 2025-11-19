using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameBackupManager.App.Models;
using GameBackupManager.App.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using System.ComponentModel;

namespace GameBackupManager.App.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        #region Fields

        private readonly BackupService _backupService;
        private readonly JsonConfigurationService _configService;
        private readonly ILogger<MainWindowViewModel> _logger;

        // Track previous AppSettings instance so we can unsubscribe PropertyChanged
        private AppSettings? _previousAppSettings;

        // Debounce token for saving settings when theme changes
        private CancellationTokenSource? _themeSaveCts;

        [ObservableProperty]
        private AppSettings appSettings = new();

        private ObservableCollection<string> availableThemes = new ObservableCollection<string> { "Light", "Dark" };

        [ObservableProperty]
        private ObservableCollection<GameViewModel> games = new();

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private GameViewModel? selectedGame;

        [ObservableProperty]
        private bool showOptionsMenu;

        [ObservableProperty]
        private string statusMessage = "Ready";

        #endregion Fields

        #region Public Constructors

        public MainWindowViewModel(
            JsonConfigurationService configService,
            BackupService backupService,
            ILogger<MainWindowViewModel> logger)
        {
            _configService = configService;
            _backupService = backupService;
            _logger = logger;

            LoadAvailableThemes();

            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
            CreateBackupCommand = new AsyncRelayCommand<GameViewModel>(CreateBackupAsync);
            RestoreBackupCommand = new AsyncRelayCommand<BackupInfo>(RestoreBackupAsync);
            ToggleGameActiveCommand = new RelayCommand<GameViewModel>(ToggleGameActive);
            SaveSettingsCommand = new AsyncRelayCommand(SaveSettingsAsync);
            RefreshGamesCommand = new AsyncRelayCommand(RefreshGamesAsync);

            _ = LoadDataAsync();
        }

        #endregion Public Constructors

        #region Properties

        public ObservableCollection<string> AvailableThemes
        {
            get => availableThemes;
            set
            {
                availableThemes = value;
                OnPropertyChanged(nameof(AvailableThemes));
            }
        }

        public IAsyncRelayCommand CreateBackupCommand { get; }
        public IAsyncRelayCommand LoadDataCommand { get; }
        public IAsyncRelayCommand RefreshGamesCommand { get; }
        public IAsyncRelayCommand RestoreBackupCommand { get; }
        public IAsyncRelayCommand SaveSettingsCommand { get; }
        public IRelayCommand ToggleGameActiveCommand { get; }

        #endregion Properties

        #region Public Methods

        [RelayCommand]
        public void ToggleOptionsMenu()
        {
            ShowOptionsMenu = !ShowOptionsMenu;
        }

        #endregion Public Methods

        #region Private Methods

        // Subscribe/unsubscribe to AppSettings.PropertyChanged so we can react when Theme changes
        private void AppSettings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e?.PropertyName == nameof(AppSettings.Theme) && sender is AppSettings settings)
            {
                // Delegate theme switching to App so styles persist across the application's lifetime
                if (Application.Current is App app)
                {
                    app.SetTheme(settings.Theme);
                }

                // Debounced save to avoid many disk writes when user toggles rapidly
                DebounceSaveSettings();
            }
        }

        private async Task CreateBackupAsync(GameViewModel? gameViewModel)
        {
            if (gameViewModel == null) return;

            IsBusy = true;
            StatusMessage = $"Creating backup for {gameViewModel.GameTitle}...";

            try
            {
                var result = await _backupService.CreateBackupAsync(gameViewModel.GameDefinition);

                if (result.Success)
                {
                    StatusMessage = $"Backup created successfully for {gameViewModel.GameTitle}";
                    await RefreshGameBackupsAsync(gameViewModel);
                }
                else
                {
                    StatusMessage = $"Failed to create backup: {result.Message}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating backup");
                StatusMessage = "Error creating backup";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void DebounceSaveSettings(int millisecondsDelay = 500)
        {
            _themeSaveCts?.Cancel();
            _themeSaveCts = new CancellationTokenSource();
            var ct = _themeSaveCts.Token;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(millisecondsDelay, ct);
                    await SaveSettingsAsync();
                }
                catch (OperationCanceledException)
                {
                    // ignored - new change arrived
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving settings after theme change");
                }
            }, ct);
        }

        private void LoadAvailableThemes()
        {
            try
            {
                // Clear existing themes
                AvailableThemes.Clear();

                // Load themes from the Styles directory
                var lightThemePath = "avares://GameBackupManager.App/Styles/LightTheme.axaml";
                var darkThemePath = "avares://GameBackupManager.App/Styles/DarkTheme.axaml";

                // Add themes to the collection
                if (!string.IsNullOrEmpty(lightThemePath))
                {
                    AvailableThemes.Add("Light");
                }

                if (!string.IsNullOrEmpty(darkThemePath))
                {
                    AvailableThemes.Add("Dark");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading available themes");
            }
        }

        private async Task LoadDataAsync()
        {
            IsBusy = true;
            StatusMessage = "Loading games...";

            try
            {
                // Load app settings
                AppSettings = await _configService.LoadAppSettingsAsync();

                // Load games and active games
                var games = await _configService.LoadGameDefinitionsAsync();
                var activeGames = await _configService.LoadActiveGamesAsync();

                Games.Clear();
                foreach (var game in games)
                {
                    var gameViewModel = new GameViewModel(game, activeGames.IsGameActive(game.GameTitle));
                    Games.Add(gameViewModel);
                }

                StatusMessage = $"Loaded {Games.Count} games";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading data");
                StatusMessage = "Error loading data";
            }
            finally
            {
                IsBusy = false;
            }
        }

        partial void OnAppSettingsChanged(AppSettings value)
        {
            // Unsubscribe previous
            if (_previousAppSettings != null)
            {
                _previousAppSettings.PropertyChanged -= AppSettings_PropertyChanged;
            }

            // Subscribe new
            if (value != null)
            {
                value.PropertyChanged += AppSettings_PropertyChanged;

                // Ensure App matches settings (in case App didn't already apply)
                if (Application.Current is App app)
                {
                    app.SetTheme(value.Theme);
                }
            }

            _previousAppSettings = value;
        }

        private async Task RefreshGameBackupsAsync(GameViewModel gameViewModel)
        {
            try
            {
                var backups = await _backupService.GetAvailableBackupsAsync(gameViewModel.GameDefinition);
                gameViewModel.Backups.Clear();
                foreach (var backup in backups)
                {
                    gameViewModel.Backups.Add(backup);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing backups for {GameTitle}", gameViewModel.GameTitle);
            }
        }

        private async Task RefreshGamesAsync()
        {
            IsBusy = true;
            StatusMessage = "Refreshing games...";

            try
            {
                foreach (var game in Games)
                {
                    game.GameDefinition.CheckInstallationStatus();
                    await RefreshGameBackupsAsync(game);
                }
                StatusMessage = "Games refreshed successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing games");
                StatusMessage = "Error refreshing games";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task RestoreBackupAsync(BackupInfo? backupInfo)
        {
            if (backupInfo == null || SelectedGame == null) return;

            IsBusy = true;
            StatusMessage = $"Restoring backup for {SelectedGame.GameTitle}...";

            try
            {
                var result = await _backupService.RestoreBackupAsync(SelectedGame.GameDefinition, backupInfo.Path);

                if (result.Success)
                {
                    StatusMessage = $"Backup restored successfully for {SelectedGame.GameTitle}";
                }
                else
                {
                    StatusMessage = $"Failed to restore backup: {result.Message}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring backup");
                StatusMessage = "Error restoring backup";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SaveSettingsAsync()
        {
            IsBusy = true;
            StatusMessage = "Saving settings...";

            try
            {
                await _configService.SaveAppSettingsAsync(AppSettings);
                await UpdateActiveGamesAsync();
                StatusMessage = "Settings saved successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving settings");
                StatusMessage = "Error saving settings";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ToggleGameActive(GameViewModel? gameViewModel)
        {
            if (gameViewModel == null) return;

            gameViewModel.IsActive = !gameViewModel.IsActive;
            _ = UpdateActiveGamesAsync();
        }

        private async Task UpdateActiveGamesAsync()
        {
            try
            {
                var activeGames = new ActiveGames();
                foreach (var game in Games.Where(g => g.IsActive))
                {
                    activeGames.SetGameActive(game.GameTitle, true);
                }
                await _configService.SaveActiveGamesAsync(activeGames);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating active games");
            }
        }

        #endregion Private Methods
    }
}