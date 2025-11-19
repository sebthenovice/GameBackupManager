using FluentAssertions;
using GameBackupManager.App.Models;
using GameBackupManager.App.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameBackupManager.Tests.Services;

[TestFixture]
public class BackupServiceTests
{
    #region Fields

    private BackupService _backupService;
    private ILogger<BackupService> _mockBackupLogger;
    private ILogger<JsonConfigurationService> _mockConfigLogger;
    private JsonConfigurationService _configService;
    private string _testBackupDirectory;

    #endregion Fields

    #region Public Methods

    [SetUp]
    public void SetUp()
    {
        _mockBackupLogger = Substitute.For<ILogger<BackupService>>();
        _mockConfigLogger = Substitute.For<ILogger<JsonConfigurationService>>();
        _testBackupDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testBackupDirectory);
        
        _configService = new JsonConfigurationService(_mockConfigLogger);
        _backupService = new BackupService(_mockBackupLogger, _configService);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testBackupDirectory))
        {
            Directory.Delete(_testBackupDirectory, true);
        }
    }

    [Test]
    public async Task CreateBackupAsync_ShouldReturnFailure_WhenGameNotInstalled()
    {
        // Arrange
        var game = new GameDefinition
        {
            GameTitle = "TestGame",
            GamePath = @"C:\NonExistent\Path",
            SavePath = @"C:\SavePath",
            IsInstalled = false
        };

        // Act
        var result = await _backupService.CreateBackupAsync(game);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not installed");
    }

    [Test]
    public async Task CreateBackupAsync_ShouldReturnFailure_WhenSavePathDoesNotExist()
    {
        // Arrange
        var game = new GameDefinition
        {
            GameTitle = "TestGame",
            GamePath = @"C:\Games\TestGame",
            SavePath = @"C:\NonExistent\Saves",
            IsInstalled = true
        };

        // Act
        var result = await _backupService.CreateBackupAsync(game);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Save path does not exist");
    }

    [Test]
    public async Task CreateBackupAsync_ShouldCreateBackup_WhenValidGameAndSavePath()
    {
        // Arrange
        var savePath = Path.Combine(_testBackupDirectory, "saves");
        var backupLocation = Path.Combine(_testBackupDirectory, "backups");
        Directory.CreateDirectory(savePath);

        // Create some test save files
        File.WriteAllText(Path.Combine(savePath, "save1.dat"), "test save data");
        File.WriteAllText(Path.Combine(savePath, "save2.dat"), "more save data");

        var game = new GameDefinition
        {
            GameTitle = "TestGame",
            GamePath = _testBackupDirectory,
            SavePath = savePath,
            IsInstalled = true
        };

        // Update settings to use test backup location
        var settings = new AppSettings { BackupLocation = backupLocation, BackupCompression = false };
        await _configService.SaveAppSettingsAsync(settings);

        // Act
        var result = await _backupService.CreateBackupAsync(game);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("successfully");
        result.BackupPath.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task GetAvailableBackupsAsync_ShouldReturnEmptyList_WhenNoBackupsExist()
    {
        // Arrange
        var game = new GameDefinition
        {
            GameTitle = "TestGame",
            GamePath = _testBackupDirectory,
            SavePath = _testBackupDirectory,
            IsInstalled = true
        };

        // Act
        var backups = await _backupService.GetAvailableBackupsAsync(game);

        // Assert
        backups.Should().BeEmpty();
    }

    [Test]
    public async Task RestoreBackupAsync_ShouldReturnFailure_WhenBackupPathNotFound()
    {
        // Arrange
        var game = new GameDefinition
        {
            GameTitle = "TestGame",
            GamePath = _testBackupDirectory,
            SavePath = _testBackupDirectory,
            IsInstalled = true
        };

        // Act
        var result = await _backupService.RestoreBackupAsync(game, @"C:\NonExistent\backup.zip");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    #endregion Public Methods
}