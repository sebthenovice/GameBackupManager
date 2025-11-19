using FluentAssertions;
using GameBackupManager.App.Models;
using GameBackupManager.App.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameBackupManager.Tests.Services;

[TestFixture]
public class JsonConfigurationServiceTests
{
    #region Fields

    private ILogger<JsonConfigurationService> _mockLogger;
    private JsonConfigurationService _service;
    private string _testConfigDirectory;

    #endregion Fields

    #region Public Methods

    [SetUp]
    public void SetUp()
    {
        _mockLogger = Substitute.For<ILogger<JsonConfigurationService>>();
        _testConfigDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testConfigDirectory);
        
        _service = new JsonConfigurationService(_mockLogger);
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up test files from the actual AppData folder
        try
        {
            if (File.Exists(_service.AppSettingsPath))
                File.Delete(_service.AppSettingsPath);
            if (File.Exists(_service.GamesConfigurationPath))
                File.Delete(_service.GamesConfigurationPath);
            if (File.Exists(_service.ActiveGamesPath))
                File.Delete(_service.ActiveGamesPath);
        }
        catch { /* Ignore cleanup errors */ }

        if (Directory.Exists(_testConfigDirectory))
        {
            Directory.Delete(_testConfigDirectory, true);
        }
    }

    [Test]
    public async Task LoadAppSettingsAsync_ShouldReturnDefault_WhenFileDoesNotExist()
    {
        // Clean up any existing file first
        if (File.Exists(_service.AppSettingsPath))
            File.Delete(_service.AppSettingsPath);

        // Act
        var result = await _service.LoadAppSettingsAsync();

        // Assert
        result.Should().NotBeNull();
        result.MaxBackupCount.Should().Be(10);
        result.Theme.Should().Be("Dark");
        result.BackupLocation.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task LoadAppSettingsAsync_ShouldReturnDeserializedObject_WhenFileExists()
    {
        // Arrange
        var settings = new AppSettings
        {
            BackupLocation = @"D:\Backups",
            MaxBackupCount = 5,
            Theme = "Light"
        };
        await _service.SaveAppSettingsAsync(settings);

        // Act
        var result = await _service.LoadAppSettingsAsync();

        // Assert
        result.Should().NotBeNull();
        result.MaxBackupCount.Should().Be(5);
        result.Theme.Should().Be("Light");
    }

    [Test]
    public async Task SaveAppSettingsAsync_ShouldCreateFile_WithCorrectData()
    {
        // Arrange
        var settings = new AppSettings { Theme = "Light", MaxBackupCount = 7 };

        // Act
        await _service.SaveAppSettingsAsync(settings);

        // Assert
        File.Exists(_service.AppSettingsPath).Should().BeTrue();
        var loadedSettings = await _service.LoadAppSettingsAsync();
        loadedSettings.Theme.Should().Be("Light");
        loadedSettings.MaxBackupCount.Should().Be(7);
    }

    [Test]
    public async Task SaveAppSettingsAsync_ShouldOverwriteExistingFile()
    {
        // Arrange
        var settings1 = new AppSettings { Theme = "Dark" };
        await _service.SaveAppSettingsAsync(settings1);

        var settings2 = new AppSettings { Theme = "Light" };

        // Act
        await _service.SaveAppSettingsAsync(settings2);

        // Assert
        var loadedSettings = await _service.LoadAppSettingsAsync();
        loadedSettings.Theme.Should().Be("Light");
    }

    [Test]
    public async Task LoadGameDefinitionsAsync_ShouldReturnDefault_WhenFileDoesNotExist()
    {
        // Clean up any existing file first
        if (File.Exists(_service.GamesConfigurationPath))
            File.Delete(_service.GamesConfigurationPath);

        // Act
        var result = await _service.LoadGameDefinitionsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<List<GameDefinition>>();
    }

    [Test]
    public async Task SaveGameDefinitionsAsync_ShouldCreateFile_WithCorrectData()
    {
        // Arrange
        var games = new List<GameDefinition>
        {
            new GameDefinition("Game 1", @"C:\Game1", @"C:\Saves1"),
            new GameDefinition("Game 2", @"C:\Game2", @"C:\Saves2")
        };

        // Act
        await _service.SaveGameDefinitionsAsync(games);

        // Assert
        File.Exists(_service.GamesConfigurationPath).Should().BeTrue();
        var loadedGames = await _service.LoadGameDefinitionsAsync();
        loadedGames.Should().HaveCount(2);
        loadedGames[0].GameTitle.Should().Be("Game 1");
        loadedGames[1].GameTitle.Should().Be("Game 2");
    }

    [Test]
    public async Task LoadActiveGamesAsync_ShouldReturnDefault_WhenFileDoesNotExist()
    {
        // Clean up any existing file first
        if (File.Exists(_service.ActiveGamesPath))
            File.Delete(_service.ActiveGamesPath);

        // Act
        var result = await _service.LoadActiveGamesAsync();

        // Assert
        result.Should().NotBeNull();
        result.ActiveGameIds.Should().BeEmpty();
    }

    [Test]
    public async Task SaveActiveGamesAsync_ShouldCreateFile_WithCorrectData()
    {
        // Clean up any existing file first
        if (File.Exists(_service.ActiveGamesPath))
            File.Delete(_service.ActiveGamesPath);

        // Arrange
        var activeGames = new ActiveGames();
        activeGames.SetGameActive("Game 1", true);
        activeGames.SetGameActive("Game 2", true);

        // Act
        await _service.SaveActiveGamesAsync(activeGames);

        // Assert
        File.Exists(_service.ActiveGamesPath).Should().BeTrue();
        var loadedActiveGames = await _service.LoadActiveGamesAsync();
        loadedActiveGames.ActiveGameIds.Should().Contain("Game 1");
        loadedActiveGames.ActiveGameIds.Should().Contain("Game 2");
    }

    [Test]
    public async Task EnsureBackupDirectoryExists_ShouldCreateDirectory_WhenCalledViaSettings()
    {
        // Arrange
        var settings = new AppSettings { BackupLocation = Path.Combine(_testConfigDirectory, "newBackupFolder") };

        // Act
        settings.EnsureBackupDirectoryExists();

        // Assert
        Directory.Exists(settings.BackupLocation).Should().BeTrue();
    }

    #endregion Public Methods
}