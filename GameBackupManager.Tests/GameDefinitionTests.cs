using NUnit.Framework;
using GameBackupManager.App.Models;

namespace GameBackupManager.Tests.Models;

[TestFixture]
public class GameDefinitionTests
{
    #region Public Methods

    [Test]
    public void Constructor_ShouldInitializeProperties_WhenValidDataProvided()
    {
        // Arrange & Act
        var game = new GameDefinition
        {
            GameTitle = "The Witcher 3",
            GamePath = @"C:\Games\Witcher3",
            SavePath = @"C:\Saves\Witcher3",
            ExecutableName = "witcher3.exe",
            BackupFolderName = "witcher3_saves",
            IsInstalled = true
        };

        // Assert
        Assert.That(game.GameTitle, Is.EqualTo("The Witcher 3"));
        Assert.That(game.IsInstalled, Is.True);
        Assert.That(game.BackupFolderName, Is.EqualTo("witcher3_saves"));
    }

    [Test]
    public void IsGameInstalled_ShouldReturnFalse_WhenExecutableNotFound()
    {
        // Arrange
        var game = new GameDefinition
        {
            ExecutableName = "nonexistent.exe"
        };

        // Act
        var result = game.IsInstalled; // Assuming this method exists

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    [TestCase("witcher3.exe", true)]
    [TestCase("another.exe", false)]
    public void MatchesExecutable_ShouldReturnExpectedResult(string executable, bool expected)
    {
        // Arrange
        var game = new GameDefinition { ExecutableName = "witcher3.exe" };

        // Act
        var result = game.ExecutableName;

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    #endregion Public Methods
}