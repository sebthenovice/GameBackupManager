using FluentAssertions;
using GameBackupManager.App.Models;
using GameBackupManager.App.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using System.IO.Abstractions.TestingHelpers;

namespace GameBackupManager.Tests.Services;

[TestFixture]
public class BackupServiceTests
{
    #region Fields

    private BackupService _backupService;
    private MockFileSystem _mockFileSystem;
    private ILogger<BackupService> _mockLogger;

    #endregion Fields

    #region Public Methods

    [Test]
    public void CreateBackup_ShouldCopySaveFiles_ToBackupLocation()
    {
        // Arrange
        var game = new GameDefinition
        {
            GameTitle = "TestGame",
            SavePath = @"C:\GameSaves\TestGame",
            BackupFolderName = "testgame_backups"
        };

        var settings = new AppSettings { BackupLocation = @"D:\Backups" };

        // Create mock save files
        _mockFileSystem.AddDirectory(game.SavePath);
        _mockFileSystem.AddFile($@"{game.SavePath}\save1.dat", new MockFileData("SAVE DATA 1"));
        _mockFileSystem.AddFile($@"{game.SavePath}\save2.dat", new MockFileData("SAVE DATA 2"));

        // Act
        var result = _backupService.CreateBackup(game, settings);

        // Assert
        Assert.That(result.Success, Is.True);

        var backupPath = $@"{settings.BackupLocation}\{game.BackupFolderName}";
        Assert.That(_mockFileSystem.Directory.Exists(backupPath), Is.True);

        var backupFiles = _mockFileSystem.Directory.GetFiles(backupPath);
        backupFiles.Should().HaveCount(1); // One backup zip/file
    }

    [Test]
    public void DeleteOldBackups_ShouldRespectMaxBackupCount()
    {
        // Arrange
        var game = new GameDefinition { BackupFolderName = "testgame" };
        var settings = new AppSettings { BackupLocation = @"D:\Backups", MaxBackupCount = 3 };

        var backupDir = $@"{settings.BackupLocation}\{game.BackupFolderName}";
        _mockFileSystem.AddDirectory(backupDir);

        // Create 5 backup files
        for (int i = 1; i <= 5; i++)
        {
            _mockFileSystem.AddFile(
                $@"{backupDir}\backup_{i}.zip",
                new MockFileData($"BACKUP {i}")
            );
        }

        // Act
        _backupService.DeleteOldBackups(game, settings);

        // Assert
        var remainingFiles = _mockFileSystem.Directory.GetFiles(backupDir);
        remainingFiles.Should().HaveCount(3);
    }

    [Test]
    public void GetBackups_ShouldReturnEmptyList_WhenNoBackupsExist()
    {
        // Arrange
        var game = new GameDefinition { BackupFolderName = "nonexistent" };
        var settings = new AppSettings { BackupLocation = @"D:\Backups" };

        _mockFileSystem.AddDirectory(settings.BackupLocation);

        // Act
        var backups = _backupService.GetBackups(game, settings);

        // Assert
        backups.Should().BeEmpty();
    }

    [SetUp]
    public void SetUp()
    {
        _mockFileSystem = new MockFileSystem();
        _mockLogger = Substitute.For<ILogger<BackupService>>();
        _backupService = new BackupService(_mockFileSystem, _mockLogger);
    }

    #endregion Public Methods
}