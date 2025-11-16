using GameBackupManager.App.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GameBackupManager.App.Services
{
    public class JsonConfigurationService
    {
        #region Fields

        private readonly string _configDirectory;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ILogger<JsonConfigurationService> _logger;

        #endregion Fields

        #region Public Constructors

        public JsonConfigurationService(ILogger<JsonConfigurationService> logger)
        {
            _logger = logger;
            _configDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "GameBackupManager"
            );

            // Ensure config directory exists
            Directory.CreateDirectory(_configDirectory);

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        #endregion Public Constructors

        #region Properties

        public string ActiveGamesPath => Path.Combine(_configDirectory, "activegames.json");
        public string AppSettingsPath => Path.Combine(_configDirectory, "appsettings.json");
        public string GamesConfigurationPath => Path.Combine(_configDirectory, "games.json");

        #endregion Properties

        #region Public Methods

        public async Task<ActiveGames> LoadActiveGamesAsync()
        {
            try
            {
                if (!File.Exists(ActiveGamesPath))
                {
                    _logger.LogInformation("Active games file not found, creating default");
                    var defaultActiveGames = new ActiveGames();
                    await SaveActiveGamesAsync(defaultActiveGames);
                    return defaultActiveGames;
                }

                var json = await File.ReadAllTextAsync(ActiveGamesPath);
                return JsonSerializer.Deserialize<ActiveGames>(json, _jsonOptions) ?? new ActiveGames();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading active games");
                return new ActiveGames();
            }
        }

        public async Task<AppSettings> LoadAppSettingsAsync()
        {
            try
            {
                if (!File.Exists(AppSettingsPath))
                {
                    _logger.LogInformation("App settings file not found, creating default");
                    var defaultSettings = new AppSettings();
                    await SaveAppSettingsAsync(defaultSettings);
                    return defaultSettings;
                }

                var json = await File.ReadAllTextAsync(AppSettingsPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions) ?? new AppSettings();
                settings.EnsureBackupDirectoryExists();
                return settings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading app settings");
                return new AppSettings();
            }
        }

        public async Task<List<GameDefinition>> LoadGameDefinitionsAsync()
        {
            try
            {
                if (!File.Exists(GamesConfigurationPath))
                {
                    _logger.LogInformation("Games configuration file not found, creating default");
                    var defaultGames = CreateDefaultGameDefinitions();
                    await SaveGameDefinitionsAsync(defaultGames);
                    return defaultGames;
                }

                var json = await File.ReadAllTextAsync(GamesConfigurationPath);
                var games = JsonSerializer.Deserialize<List<GameDefinition>>(json, _jsonOptions) ?? new List<GameDefinition>();

                // Check installation status for each game
                foreach (var game in games)
                {
                    game.CheckInstallationStatus();
                }

                return games;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading game definitions");
                return new List<GameDefinition>();
            }
        }

        public async Task SaveActiveGamesAsync(ActiveGames activeGames)
        {
            try
            {
                var json = JsonSerializer.Serialize(activeGames, _jsonOptions);
                await File.WriteAllTextAsync(ActiveGamesPath, json);
                _logger.LogInformation("Active games saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving active games");
                throw;
            }
        }

        public async Task SaveAppSettingsAsync(AppSettings settings)
        {
            try
            {
                var json = JsonSerializer.Serialize(settings, _jsonOptions);
                await File.WriteAllTextAsync(AppSettingsPath, json);
                _logger.LogInformation("App settings saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving app settings");
                throw;
            }
        }

        public async Task SaveGameDefinitionsAsync(List<GameDefinition> games)
        {
            try
            {
                var json = JsonSerializer.Serialize(games, _jsonOptions);
                await File.WriteAllTextAsync(GamesConfigurationPath, json);
                _logger.LogInformation("Game definitions saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving game definitions");
                throw;
            }
        }

        #endregion Public Methods

        #region Private Methods

        private List<GameDefinition> CreateDefaultGameDefinitions()
        {
            return new List<GameDefinition>
            {
                new GameDefinition
                {
                    GameTitle = "The Witcher 3: Wild Hunt",
                    GamePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "The Witcher 3 Wild Hunt"),
                    SavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "The Witcher 3", "gamesaves"),
                    ExecutableName = "witcher3.exe",
                    BackupFolderName = "witcher3_saves"
                },
                new GameDefinition
                {
                    GameTitle = "Cyberpunk 2077",
                    GamePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Cyberpunk 2077"),
                    SavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Saved Games", "CD Projekt Red", "Cyberpunk 2077"),
                    ExecutableName = "Cyberpunk2077.exe",
                    BackupFolderName = "cyberpunk_saves"
                },
                new GameDefinition
                {
                    GameTitle = "Elden Ring",
                    GamePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam", "steamapps", "common", "ELDEN RING"),
                    SavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EldenRing"),
                    ExecutableName = "eldenring.exe",
                    BackupFolderName = "eldenring_saves"
                }
            };
        }

        #endregion Private Methods
    }
}