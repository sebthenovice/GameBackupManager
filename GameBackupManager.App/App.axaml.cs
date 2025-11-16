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
            LogStartup($"[{DateTime.Now:O}] App.Initialize - start");
            try
            {
                AvaloniaXamlLoader.Load(this);
                LogStartup($"[{DateTime.Now:O}] App.Initialize - AvaloniaXamlLoader.Load completed");
            }
            catch (Exception ex)
            {
                LogStartup($"[{DateTime.Now:O}] App.Initialize - Exception: {ex}");
                throw;
            }
        }

        public override void OnFrameworkInitializationCompleted()
        {
            LogStartup($"[{DateTime.Now:O}] App.OnFrameworkInitializationCompleted - start");
            
            try
            {
                // Setup logging
                LogStartup($"[{DateTime.Now:O}] App.OnFrameworkInitializationCompleted - creating LoggerFactory");
                _loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });
                LogStartup($"[{DateTime.Now:O}] App.OnFrameworkInitializationCompleted - LoggerFactory created");

                // Setup services
                LogStartup($"[{DateTime.Now:O}] App.OnFrameworkInitializationCompleted - creating logger instance");
                var logger = _loggerFactory.CreateLogger<App>();
                
                LogStartup($"[{DateTime.Now:O}] App.OnFrameworkInitializationCompleted - creating JsonConfigurationService");
                var configService = new JsonConfigurationService(_loggerFactory.CreateLogger<JsonConfigurationService>());
                
                LogStartup($"[{DateTime.Now:O}] App.OnFrameworkInitializationCompleted - creating BackupService");
                var backupService = new BackupService(_loggerFactory.CreateLogger<BackupService>(), configService);
                LogStartup($"[{DateTime.Now:O}] App.OnFrameworkInitializationCompleted - services created");

                // Apply saved theme early so window shows with correct theme immediately
                try
                {
                    LogStartup($"[{DateTime.Now:O}] App.OnFrameworkInitializationCompleted - loading app settings");
                    var settings = configService.LoadAppSettingsAsync().GetAwaiter().GetResult();
                    LogStartup($"[{DateTime.Now:O}] App.OnFrameworkInitializationCompleted - app settings loaded, theme={settings.Theme}");
                    
                    LogStartup($"[{DateTime.Now:O}] App.OnFrameworkInitializationCompleted - setting theme");
                    SetTheme(settings.Theme);
                    LogStartup($"[{DateTime.Now:O}] App.OnFrameworkInitializationCompleted - theme set");
                }
                catch (Exception ex)
                {
                    LogStartup($"[{DateTime.Now:O}] App.OnFrameworkInitializationCompleted - Exception loading settings: {ex}");
                    logger.LogWarning(ex, "Unable to load app settings at startup; continuing with defaults.");
                }

                LogStartup($"[{DateTime.Now:O}] App.OnFrameworkInitializationCompleted - checking ApplicationLifetime");
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    LogStartup($"[{DateTime.Now:O}] App.OnFrameworkInitializationCompleted - creating MainWindow");
                    desktop.MainWindow = new MainWindow
                    {
                        DataContext = new MainWindowViewModel(
                            configService,
                            backupService,
                            _loggerFactory.CreateLogger<MainWindowViewModel>()
                        )
                    };
                    LogStartup($"[{DateTime.Now:O}] App.OnFrameworkInitializationCompleted - MainWindow created and assigned");

                    // Handle application exit
                    LogStartup($"[{DateTime.Now:O}] App.OnFrameworkInitializationCompleted - subscribing to Exit event");
                    desktop.Exit += OnApplicationExit;
                    LogStartup($"[{DateTime.Now:O}] App.OnFrameworkInitializationCompleted - Exit event subscribed");
                }
                else
                {
                    LogStartup($"[{DateTime.Now:O}] App.OnFrameworkInitializationCompleted - ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime");
                }

                LogStartup($"[{DateTime.Now:O}] App.OnFrameworkInitializationCompleted - loading available themes");
                LoadAvailableThemes();
                LogStartup($"[{DateTime.Now:O}] App.OnFrameworkInitializationCompleted - available themes loaded");

                LogStartup($"[{DateTime.Now:O}] App.OnFrameworkInitializationCompleted - calling base.OnFrameworkInitializationCompleted");
                base.OnFrameworkInitializationCompleted();
                LogStartup($"[{DateTime.Now:O}] App.OnFrameworkInitializationCompleted - completed successfully");
            }
            catch (Exception ex)
            {
                LogStartup($"[{DateTime.Now:O}] App.OnFrameworkInitializationCompleted - Unhandled exception: {ex}");
                throw;
            }
        }

        // Public API for switching theme at runtime
        public void SetTheme(string? theme)
        {
            LogStartup($"[{DateTime.Now:O}] App.SetTheme - start, theme={theme}");
            
            if (string.IsNullOrWhiteSpace(theme))
            {
                LogStartup($"[{DateTime.Now:O}] App.SetTheme - theme is null or whitespace, returning early");
                return;
            }

            try
            {
                LogStartup($"[{DateTime.Now:O}] App.SetTheme - clearing existing styles");
                Styles.Clear();
                LogStartup($"[{DateTime.Now:O}] App.SetTheme - styles cleared");

                if (string.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase))
                {
                    LogStartup($"[{DateTime.Now:O}] App.SetTheme - loading Light theme");
                    Styles.Add(new Avalonia.Markup.Xaml.Styling.StyleInclude(new Uri("avares://GameBackupManager.App/Styles/LightTheme.axaml"))
                    {
                        Source = new Uri("avares://GameBackupManager.App/Styles/LightTheme.axaml")
                    });
                    LogStartup($"[{DateTime.Now:O}] App.SetTheme - Light theme loaded");
                }
                else if (string.Equals(theme, "Dark", StringComparison.OrdinalIgnoreCase))
                {
                    LogStartup($"[{DateTime.Now:O}] App.SetTheme - loading Dark theme");
                    Styles.Add(new Avalonia.Markup.Xaml.Styling.StyleInclude(new Uri("avares://GameBackupManager.App/Styles/DarkTheme.axaml"))
                    {
                        Source = new Uri("avares://GameBackupManager.App/Styles/DarkTheme.axaml")
                    });
                    LogStartup($"[{DateTime.Now:O}] App.SetTheme - Dark theme loaded");
                }
                else
                {
                    // attempt to load a theme file matching the provided name
                    LogStartup($"[{DateTime.Now:O}] App.SetTheme - loading custom theme: {theme}");
                    var candidate = $"avares://GameBackupManager.App/Styles/{theme}.axaml";
                    try
                    {
                        Styles.Add(new Avalonia.Markup.Xaml.Styling.StyleInclude(new Uri(candidate))
                        {
                            Source = new Uri(candidate)
                        });
                        LogStartup($"[{DateTime.Now:O}] App.SetTheme - custom theme loaded: {theme}");
                    }
                    catch (Exception ex)
                    {
                        LogStartup($"[{DateTime.Now:O}] App.SetTheme - failed to load custom theme: {theme}, exception: {ex}");
                        // ignore invalid theme names
                    }
                }
                
                LogStartup($"[{DateTime.Now:O}] App.SetTheme - completed successfully");
            }
            catch (Exception ex)
            {
                LogStartup($"[{DateTime.Now:O}] App.SetTheme - Unhandled exception: {ex}");
                throw;
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

        private static void LogStartup(string message)
        {
            if (Program.EnableStartupLogging)
            {
                System.Diagnostics.Trace.WriteLine(message);
            }
        }

        private void LoadAvailableThemes()
        {
            LogStartup($"[{DateTime.Now:O}] App.LoadAvailableThemes - start");
            try
            {
                var themesDirectory = Path.Combine(AppContext.BaseDirectory, "Styles");
                LogStartup($"[{DateTime.Now:O}] App.LoadAvailableThemes - themes directory: {themesDirectory}");
                
                if (Directory.Exists(themesDirectory))
                {
                    LogStartup($"[{DateTime.Now:O}] App.LoadAvailableThemes - directory exists, scanning for theme files");
                    foreach (var themeFile in Directory.GetFiles(themesDirectory, "*.axaml"))
                    {
                        var themeName = Path.GetFileNameWithoutExtension(themeFile);
                        LogStartup($"[{DateTime.Now:O}] App.LoadAvailableThemes - found theme file: {themeName}");
                        
                        if (!AvailableThemes.Contains(themeName))
                        {
                            AvailableThemes.Add(themeName);
                            LogStartup($"[{DateTime.Now:O}] App.LoadAvailableThemes - added theme: {themeName}");
                        }
                    }
                    LogStartup($"[{DateTime.Now:O}] App.LoadAvailableThemes - found {AvailableThemes.Count} themes total");
                }
                else
                {
                    LogStartup($"[{DateTime.Now:O}] App.LoadAvailableThemes - themes directory does not exist");
                }
                
                LogStartup($"[{DateTime.Now:O}] App.LoadAvailableThemes - completed");
            }
            catch (Exception ex)
            {
                LogStartup($"[{DateTime.Now:O}] App.LoadAvailableThemes - Exception: {ex}");
            }
        }

        private void OnApplicationExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            LogStartup($"[{DateTime.Now:O}] App.OnApplicationExit - application exiting");
            Dispose();
        }

        #endregion Private Methods
    }
}