using System;
using System.IO;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GameBackupManager.App.Models
{
    public partial class AppSettings : ObservableObject
    {
        #region Properties

        [JsonPropertyName("autoBackupOnLaunch")]
        public bool AutoBackupOnLaunch { get; set; } = false;

        [JsonPropertyName("backupCompression")]
        public bool BackupCompression { get; set; } = true;

        [JsonPropertyName("backupLocation")]
        public string BackupLocation { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "GameBackups"
        );

        [JsonPropertyName("checkForUpdates")]
        public bool CheckForUpdates { get; set; } = true;

        [JsonPropertyName("maxBackupCount")]
        public int MaxBackupCount { get; set; } = 10;

        [JsonPropertyName("notificationSounds")]
        public bool NotificationSounds { get; set; } = true;

        [ObservableProperty]
        [JsonPropertyName("theme")]
        private string theme = "Dark";

        #endregion Properties

        #region Public Methods

        public void EnsureBackupDirectoryExists()
        {
            if (!Directory.Exists(BackupLocation))
            {
                Directory.CreateDirectory(BackupLocation);
            }
        }

        #endregion Public Methods
    }
}