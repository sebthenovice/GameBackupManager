using System.Text.Json.Serialization;

namespace GameBackupManager.App.Models
{
    public class GameDefinition
    {
        #region Public Constructors

        public GameDefinition()
        { }

        public GameDefinition(string title, string gamePath, string savePath)
        {
            GameTitle = title;
            GamePath = gamePath;
            SavePath = savePath;
        }

        #endregion Public Constructors

        #region Properties

        [JsonPropertyName("backupFolderName")]
        public string? BackupFolderName { get; set; }

        [JsonIgnore]
        public string DisplayName => GameTitle;

        [JsonPropertyName("executableName")]
        public string? ExecutableName { get; set; }

        [JsonPropertyName("gamePath")]
        public string GamePath { get; set; } = string.Empty;

        [JsonPropertyName("gameTitle")]
        public string GameTitle { get; set; } = string.Empty;

        [JsonPropertyName("isInstalled")]
        public bool IsInstalled { get; set; }

        [JsonPropertyName("savePath")]
        public string SavePath { get; set; } = string.Empty;

        [JsonIgnore]
        public string Status => IsInstalled ? "Installed" : "NotFound";

        #endregion Properties

        #region Public Methods

        public void CheckInstallationStatus()
        {
            IsInstalled = !string.IsNullOrEmpty(GamePath) &&
                         System.IO.Directory.Exists(GamePath);
        }

        #endregion Public Methods
    }
}