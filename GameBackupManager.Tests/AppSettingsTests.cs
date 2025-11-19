using FluentAssertions;
using GameBackupManager.App.Models;
using NUnit.Framework;
using System;
using System.IO;

namespace GameBackupManager.Tests.Models;

[TestFixture]
public class AppSettingsTests
{
    #region Public Methods

    [Test]
    public void Constructor_ShouldInitializeWithDefaults()
    {
        // Act
        var settings = new AppSettings();

        // Assert
        settings.AutoBackupOnLaunch.Should().BeFalse();
        settings.BackupCompression.Should().BeTrue();
        settings.CheckForUpdates.Should().BeTrue();
        settings.MaxBackupCount.Should().Be(10);
        settings.NotificationSounds.Should().BeTrue();
        settings.Theme.Should().Be("Dark");
    }

    [Test]
    public void BackupLocation_ShouldHaveDefaultValue()
    {
        // Act
        var settings = new AppSettings();

        // Assert
        settings.BackupLocation.Should().NotBeNullOrEmpty();
        settings.BackupLocation.Should().Contain("GameBackups");
    }

    [Test]
    public void BackupLocation_ShouldContainDocumentsFolder()
    {
        // Act
        var settings = new AppSettings();
        var expectedPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "GameBackups"
        );

        // Assert
        settings.BackupLocation.Should().Be(expectedPath);
    }

    [Test]
    public void EnsureBackupDirectoryExists_ShouldCreateDirectory_WhenItDoesNotExist()
    {
        // Arrange
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var settings = new AppSettings { BackupLocation = testDir };

        try
        {
            Directory.Exists(testDir).Should().BeFalse();

            // Act
            settings.EnsureBackupDirectoryExists();

            // Assert
            Directory.Exists(testDir).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(testDir))
                Directory.Delete(testDir, true);
        }
    }

    [Test]
    public void EnsureBackupDirectoryExists_ShouldNotThrow_WhenDirectoryExists()
    {
        // Arrange
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        var settings = new AppSettings { BackupLocation = testDir };

        try
        {
            // Act & Assert - should not throw
            var action = () => settings.EnsureBackupDirectoryExists();
            action.Should().NotThrow();
        }
        finally
        {
            if (Directory.Exists(testDir))
                Directory.Delete(testDir, true);
        }
    }

    [Test]
    public void AutoBackupOnLaunch_ShouldBeModifiable()
    {
        // Arrange
        var settings = new AppSettings { AutoBackupOnLaunch = false };

        // Act
        settings.AutoBackupOnLaunch = true;

        // Assert
        settings.AutoBackupOnLaunch.Should().BeTrue();
    }

    [Test]
    public void BackupCompression_ShouldBeModifiable()
    {
        // Arrange
        var settings = new AppSettings { BackupCompression = true };

        // Act
        settings.BackupCompression = false;

        // Assert
        settings.BackupCompression.Should().BeFalse();
    }

    [Test]
    public void CheckForUpdates_ShouldBeModifiable()
    {
        // Arrange
        var settings = new AppSettings { CheckForUpdates = true };

        // Act
        settings.CheckForUpdates = false;

        // Assert
        settings.CheckForUpdates.Should().BeFalse();
    }

    [Test]
    public void MaxBackupCount_ShouldBeModifiable()
    {
        // Arrange
        var settings = new AppSettings { MaxBackupCount = 10 };

        // Act
        settings.MaxBackupCount = 5;

        // Assert
        settings.MaxBackupCount.Should().Be(5);
    }

    [Test]
    public void NotificationSounds_ShouldBeModifiable()
    {
        // Arrange
        var settings = new AppSettings { NotificationSounds = true };

        // Act
        settings.NotificationSounds = false;

        // Assert
        settings.NotificationSounds.Should().BeFalse();
    }

    [Test]
    public void Theme_ShouldBeModifiable()
    {
        // Arrange
        var settings = new AppSettings { Theme = "Dark" };

        // Act
        settings.Theme = "Light";

        // Assert
        settings.Theme.Should().Be("Light");
    }

    #endregion Public Methods
}
