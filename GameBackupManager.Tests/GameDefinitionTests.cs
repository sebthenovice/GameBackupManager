using NUnit.Framework;
using GameBackupManager.App.Models;
using System.IO;

namespace GameBackupManager.Tests.Models;

[TestFixture]
public class GameDefinitionTests
{
    #region Public Methods

    [Test]
    public void Constructor_ParameterizedConstructor_ShouldInitializeProperties()
    {
        // Arrange & Act
        var game = new GameDefinition("The Witcher 3", @"C:\Games\Witcher3", @"C:\Saves\Witcher3");

        // Assert
        Assert.That(game.GameTitle, Is.EqualTo("The Witcher 3"));
        Assert.That(game.GamePath, Is.EqualTo(@"C:\Games\Witcher3"));
        Assert.That(game.SavePath, Is.EqualTo(@"C:\Saves\Witcher3"));
    }

    [Test]
    public void Constructor_DefaultConstructor_ShouldInitializeWithDefaults()
    {
        // Act
        var game = new GameDefinition();

        // Assert
        Assert.That(game.GameTitle, Is.EqualTo(string.Empty));
        Assert.That(game.GamePath, Is.EqualTo(string.Empty));
        Assert.That(game.SavePath, Is.EqualTo(string.Empty));
        Assert.That(game.IsInstalled, Is.False);
    }

    [Test]
    public void CheckInstallationStatus_ShouldSetIsInstalledTrue_WhenGamePathExists()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        
        try
        {
            var game = new GameDefinition
            {
                GameTitle = "Test Game",
                GamePath = tempDir,
                SavePath = @"C:\Saves"
            };
            game.IsInstalled = false;

            // Act
            game.CheckInstallationStatus();

            // Assert
            Assert.That(game.IsInstalled, Is.True);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public void CheckInstallationStatus_ShouldSetIsInstalledFalse_WhenGamePathDoesNotExist()
    {
        // Arrange
        var game = new GameDefinition
        {
            GameTitle = "Test Game",
            GamePath = @"C:\NonExistentPath\Games",
            SavePath = @"C:\Saves"
        };
        game.IsInstalled = true;

        // Act
        game.CheckInstallationStatus();

        // Assert
        Assert.That(game.IsInstalled, Is.False);
    }

    [Test]
    public void Status_ShouldReturnInstalled_WhenIsInstalledIsTrue()
    {
        // Arrange
        var game = new GameDefinition { IsInstalled = true };

        // Act
        var status = game.Status;

        // Assert
        Assert.That(status, Is.EqualTo("Installed"));
    }

    [Test]
    public void DisplayName_ShouldReturnGameTitle()
    {
        // Arrange
        var game = new GameDefinition { GameTitle = "The Witcher 3" };

        // Act
        var displayName = game.DisplayName;

        // Assert
        Assert.That(displayName, Is.EqualTo("The Witcher 3"));
    }

    [Test]
    public void Status_ShouldReturnNotFound_WhenIsInstalledIsFalse()
    {
        // Arrange
        var game = new GameDefinition { IsInstalled = false };

        // Act
        var status = game.Status;

        // Assert
        Assert.That(status, Is.EqualTo("NotFound"));
    }

    [Test]
    public void GameTitle_ShouldBeModifiable()
    {
        // Arrange
        var game = new GameDefinition { GameTitle = "Original Title" };

        // Act
        game.GameTitle = "New Title";

        // Assert
        Assert.That(game.GameTitle, Is.EqualTo("New Title"));
    }

    [Test]
    public void GamePath_ShouldBeModifiable()
    {
        // Arrange
        var game = new GameDefinition { GamePath = @"C:\Games" };

        // Act
        game.GamePath = @"D:\Games";

        // Assert
        Assert.That(game.GamePath, Is.EqualTo(@"D:\Games"));
    }

    [Test]
    public void SavePath_ShouldBeModifiable()
    {
        // Arrange
        var game = new GameDefinition { SavePath = @"C:\Saves" };

        // Act
        game.SavePath = @"D:\Saves";

        // Assert
        Assert.That(game.SavePath, Is.EqualTo(@"D:\Saves"));
    }

    [Test]
    public void ExecutableName_ShouldBeModifiable()
    {
        // Arrange
        var game = new GameDefinition { ExecutableName = "game.exe" };

        // Act
        game.ExecutableName = "newgame.exe";

        // Assert
        Assert.That(game.ExecutableName, Is.EqualTo("newgame.exe"));
    }

    [Test]
    public void BackupFolderName_ShouldBeModifiable()
    {
        // Arrange
        var game = new GameDefinition { BackupFolderName = "old_backups" };

        // Act
        game.BackupFolderName = "new_backups";

        // Assert
        Assert.That(game.BackupFolderName, Is.EqualTo("new_backups"));
    }

    [Test]
    public void IsInstalled_ShouldBeModifiable()
    {
        // Arrange
        var game = new GameDefinition { IsInstalled = false };

        // Act
        game.IsInstalled = true;

        // Assert
        Assert.That(game.IsInstalled, Is.True);
    }

    #endregion Public Methods
}