using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace GameBackupManager.App.Models
{
    public class ActiveGames
    {
        #region Properties

        [JsonPropertyName("activeGameIds")]
        public List<string> ActiveGameIds { get; set; } = new();

        [JsonPropertyName("lastUpdated")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        #endregion Properties

        #region Public Methods

        public void ClearAllActiveGames()
        {
            ActiveGameIds.Clear();
            LastUpdated = DateTime.UtcNow;
        }

        public bool IsGameActive(string gameTitle)
        {
            return ActiveGameIds.Contains(gameTitle, StringComparer.OrdinalIgnoreCase);
        }

        public void SetGameActive(string gameTitle, bool isActive)
        {
            if (isActive && !ActiveGameIds.Contains(gameTitle, StringComparer.OrdinalIgnoreCase))
            {
                ActiveGameIds.Add(gameTitle);
            }
            else if (!isActive && ActiveGameIds.Contains(gameTitle, StringComparer.OrdinalIgnoreCase))
            {
                ActiveGameIds.RemoveAll(id => id.Equals(gameTitle, StringComparison.OrdinalIgnoreCase));
            }
            LastUpdated = DateTime.UtcNow;
        }

        #endregion Public Methods
    }
}