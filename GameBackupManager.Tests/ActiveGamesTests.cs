using FluentAssertions;
using GameBackupManager.App.Models;
using NUnit.Framework;
using System;

namespace GameBackupManager.Tests.Models;

[TestFixture]
public class ActiveGamesTests
{
    #region Public Methods

    [Test]
    public void Constructor_ShouldInitializeWithEmptyList()
    {
        // Act
        var activeGames = new ActiveGames();

        // Assert
        activeGames.ActiveGameIds.Should().BeEmpty();
    }

    [Test]
    public void Constructor_ShouldSetLastUpdatedToUtcNow()
    {
        // Act
        var beforeCreation = DateTime.UtcNow;
        var activeGames = new ActiveGames();
        var afterCreation = DateTime.UtcNow;

        // Assert
        activeGames.LastUpdated.Should().BeOnOrAfter(beforeCreation);
        activeGames.LastUpdated.Should().BeOnOrBefore(afterCreation.AddSeconds(1));
    }

    [Test]
    public void SetGameActive_ShouldAddGame_WhenSettingToTrue()
    {
        // Arrange
        var activeGames = new ActiveGames();
        var gameTitle = "Test Game";

        // Act
        activeGames.SetGameActive(gameTitle, true);

        // Assert
        activeGames.ActiveGameIds.Should().Contain(gameTitle);
    }

    [Test]
    public void SetGameActive_ShouldNotAddDuplicate_WhenGameAlreadyActive()
    {
        // Arrange
        var activeGames = new ActiveGames();
        var gameTitle = "Test Game";
        activeGames.SetGameActive(gameTitle, true);

        // Act
        activeGames.SetGameActive(gameTitle, true);

        // Assert
        activeGames.ActiveGameIds.Count(g => g == gameTitle).Should().Be(1);
    }

    [Test]
    public void SetGameActive_ShouldRemoveGame_WhenSettingToFalse()
    {
        // Arrange
        var activeGames = new ActiveGames();
        var gameTitle = "Test Game";
        activeGames.SetGameActive(gameTitle, true);

        // Act
        activeGames.SetGameActive(gameTitle, false);

        // Assert
        activeGames.ActiveGameIds.Should().NotContain(gameTitle);
    }

    [Test]
    public void SetGameActive_ShouldBeCaseInsensitive_WhenRemovingGame()
    {
        // Arrange
        var activeGames = new ActiveGames();
        var gameTitle = "Test Game";
        activeGames.SetGameActive(gameTitle, true);

        // Act
        activeGames.SetGameActive("test game", false);

        // Assert
        activeGames.ActiveGameIds.Should().BeEmpty();
    }

    [Test]
    public void SetGameActive_ShouldUpdateLastUpdated()
    {
        // Arrange
        var activeGames = new ActiveGames();
        var originalLastUpdated = activeGames.LastUpdated;
        System.Threading.Thread.Sleep(10); // Ensure time has passed

        // Act
        activeGames.SetGameActive("Test Game", true);

        // Assert
        activeGames.LastUpdated.Should().BeAfter(originalLastUpdated);
    }

    [Test]
    public void IsGameActive_ShouldReturnTrue_WhenGameIsActive()
    {
        // Arrange
        var activeGames = new ActiveGames();
        var gameTitle = "Test Game";
        activeGames.SetGameActive(gameTitle, true);

        // Act
        var result = activeGames.IsGameActive(gameTitle);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void IsGameActive_ShouldReturnFalse_WhenGameIsNotActive()
    {
        // Arrange
        var activeGames = new ActiveGames();

        // Act
        var result = activeGames.IsGameActive("Non Existent Game");

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void IsGameActive_ShouldBeCaseInsensitive()
    {
        // Arrange
        var activeGames = new ActiveGames();
        activeGames.SetGameActive("Test Game", true);

        // Act
        var result = activeGames.IsGameActive("test game");

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void ClearAllActiveGames_ShouldRemoveAllGames()
    {
        // Arrange
        var activeGames = new ActiveGames();
        activeGames.SetGameActive("Game 1", true);
        activeGames.SetGameActive("Game 2", true);
        activeGames.SetGameActive("Game 3", true);

        // Act
        activeGames.ClearAllActiveGames();

        // Assert
        activeGames.ActiveGameIds.Should().BeEmpty();
    }

    [Test]
    public void ClearAllActiveGames_ShouldUpdateLastUpdated()
    {
        // Arrange
        var activeGames = new ActiveGames();
        activeGames.SetGameActive("Game 1", true);
        var originalLastUpdated = activeGames.LastUpdated;
        System.Threading.Thread.Sleep(10);

        // Act
        activeGames.ClearAllActiveGames();

        // Assert
        activeGames.LastUpdated.Should().BeAfter(originalLastUpdated);
    }

    #endregion Public Methods
}
