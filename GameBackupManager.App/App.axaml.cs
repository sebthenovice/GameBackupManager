using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GameBackupManager.App.Services;
using GameBackupManager.App.ViewModels;
using GameBackupManager.App.Views;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace GameBackupManager.App
{
    public partial class App : Application, IDisposable
    {
        #region Fields

        private bool _disposed;
        private ILoggerFactory? _loggerFactory;

        #endregion Fields

        #region Properties

        public ObservableCollection<string> AvailableThemes { get; } = new ObservableCollection<string>();

        #endregion Properties

        #region Public Methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            // Setup logging
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // Setup services
            var logger = _loggerFactory.CreateLogger<App>();
            var configService = new JsonConfigurationService(_loggerFactory.CreateLogger<JsonConfigurationService>());
            var backupService = new BackupService(_loggerFactory.CreateLogger<BackupService>(), configService);

            // Apply saved theme early so window shows with correct theme immediately
            try
            {
                var settings = configService.LoadAppSettingsAsync().GetAwaiter().GetResult();
                SetTheme(settings.Theme);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Unable to load app settings at startup; continuing with defaults.");
            }

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(
                        configService,
                        backupService,
                        _loggerFactory.CreateLogger<MainWindowViewModel>()
                    )
                };

                // Handle application exit
                desktop.Exit += OnApplicationExit;
            }

            LoadAvailableThemes();

            base.OnFrameworkInitializationCompleted();
        }

        // Public API for switching theme at runtime
        public void SetTheme(string? theme)
        {
            if (string.IsNullOrWhiteSpace(theme))
                return;

            Styles.Clear();

            if (string.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase))
            {
                Styles.Add(new Avalonia.Markup.Xaml.Styling.StyleInclude(new Uri("avares://GameBackupManager.App/Styles/LightTheme.axaml"))
                {
                    Source = new Uri("avares://GameBackupManager.App/Styles/LightTheme.axaml")
                });
            }
            else if (string.Equals(theme, "Dark", StringComparison.OrdinalIgnoreCase))
            {
                Styles.Add(new Avalonia.Markup.Xaml.Styling.StyleInclude(new Uri("avares://GameBackupManager.App/Styles/DarkTheme.axaml"))
                {
                    Source = new Uri("avares://GameBackupManager.App/Styles/DarkTheme.axaml")
                });
            }
            else
            {
                // attempt to load a theme file matching the provided name
                var candidate = $"avares://GameBackupManager.App/Styles/{theme}.axaml";
                try
                {
                    Styles.Add(new Avalonia.Markup.Xaml.Styling.StyleInclude(new Uri(candidate))
                    {
                        Source = new Uri(candidate)
                    });
                }
                catch
                {
                    // ignore invalid theme names
                }
            }
        }

        #endregion Public Methods

        #region Protected Methods

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _loggerFactory?.Dispose();

                    // Unsubscribe from events
                    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        desktop.Exit -= OnApplicationExit;
                    }
                }

                _disposed = true;
            }
        }

        #endregion Protected Methods

        #region Private Methods

        private void LoadAvailableThemes()
        {
            var themesDirectory = Path.Combine(AppContext.BaseDirectory, "Styles");
            if (Directory.Exists(themesDirectory))
            {
                foreach (var themeFile in Directory.GetFiles(themesDirectory, "*.axaml"))
                {
                    var themeName = Path.GetFileNameWithoutExtension(themeFile);
                    if (!AvailableThemes.Contains(themeName))
                    {
                        AvailableThemes.Add(themeName);
                    }
                }
            }
        }

        private void OnApplicationExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            Dispose();
        }

        #endregion Private Methods
    }
}