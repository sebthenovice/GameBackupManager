using System;
using System.Diagnostics;
using System.IO;
using Avalonia;

namespace GameBackupManager.App
{
    internal sealed class Program
    {
        #region Public Methods

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
        //                .UseReactiveUI()
        ;

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            // Basic persistent logging to diagnose startup hangs / silent failures. (lives in AppData/GameBackupManager/gbm-startup.log)
            var logFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GameBackupManager", "gbm-startup.log");

            Trace.Listeners.Add(new TextWriterTraceListener(logFile));
            Trace.AutoFlush = true;
            Trace.WriteLine($"[{DateTime.Now:O}] Program.Main - start");

            try
            {
                Trace.WriteLine($"[{DateTime.Now:O}] Building Avalonia app");
                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
                Trace.WriteLine($"[{DateTime.Now:O}] StartWithClassicDesktopLifetime returned");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[{DateTime.Now:O}] Unhandled exception: {ex}");
                try
                {
                    var exceptionLogFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GameBackupManager", "gbm-startup-exception.txt");
                    File.WriteAllText(exceptionLogFile, ex.ToString());
                }
                catch { /* best-effort logging only */ }
                throw;
            }
        }

        #endregion Public Methods
    }
}