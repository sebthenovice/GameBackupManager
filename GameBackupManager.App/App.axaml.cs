using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GameBackupManager.App.Services;
using GameBackupManager.App.ViewModels;
using GameBackupManager.App.Views;
using Microsoft.Extensions.Logging;
using System;

namespace GameBackupManager.App
{
    public partial class App : Application, IDisposable
    {
        #region Fields

        private ILoggerFactory? _loggerFactory;
        private bool _disposed;

        #endregion Fields

        #region Public Methods

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

            base.OnFrameworkInitializationCompleted();
        }

        #endregion Public Methods

        #region IDisposable Implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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

        private void OnApplicationExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            Dispose();
        }

        #endregion IDisposable Implementation
    }
}