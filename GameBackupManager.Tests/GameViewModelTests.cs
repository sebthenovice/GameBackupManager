using FluentAssertions;
using GameBackupManager.App.Models;
using GameBackupManager.App.ViewModels;
using NUnit.Framework;

namespace GameBackupManager.Tests.ViewModels;

[TestFixture]
public class GameViewModelTests
{
    #region Fields

    private GameDefinition _testGame;
    private GameViewModel _viewModel;

    #endregion Fields

    #region Public Methods

    [SetUp]
    public void SetUp()
    {
        _testGame = new GameDefinition("Test Game", @"C:\Games\TestGame", @"C:\Saves\TestGame")
        {
            ExecutableName = "game.exe",
            BackupFolderName = "test_game_backups",
            IsInstalled = true
        };
        _viewModel = new GameViewModel(_testGame, isActive: false);
    }

    [Test]
    public void Constructor_ShouldInitializeProperties()
    {
        // Assert
        _viewModel.GameTitle.Should().Be("Test Game");
        _viewModel.GamePath.Should().Be(@"C:\Games\TestGame");
        _viewModel.SavePath.Should().Be(@"C:\Saves\TestGame");
        _viewModel.IsInstalled.Should().BeTrue();
        _viewModel.IsActive.Should().BeFalse();
    }

    [Test]
    public void GameDefinition_ShouldReturnCorrectGameDefinition()
    {
        // Act
        var gameDef = _viewModel.GameDefinition;

        // Assert
        gameDef.Should().Be(_testGame);
        gameDef.GameTitle.Should().Be("Test Game");
    }

    [Test]
    public void Status_ShouldReflectInstallationStatus()
    {
        // Assert
        _viewModel.Status.Should().Be("Installed");
    }

    [Test]
    public void Status_ShouldReturnNotFound_WhenGameNotInstalled()
    {
        // Arrange
        var uninstalledGame = new GameDefinition("Not Installed", @"C:\NonExistent", @"C:\Saves")
        {
            IsInstalled = false
        };
        var viewModel = new GameViewModel(uninstalledGame, isActive: false);

        // Act & Assert
        viewModel.Status.Should().Be("NotFound");
    }

    [Test]
    public void IsActive_ShouldBeTrue_WhenSetActive()
    {
        // Arrange
        var viewModel = new GameViewModel(_testGame, isActive: true);

        // Act & Assert
        viewModel.IsActive.Should().BeTrue();
    }

    [Test]
    public void IsActive_ShouldBeFalse_WhenSetInactive()
    {
        // Arrange
        var viewModel = new GameViewModel(_testGame, isActive: false);

        // Act & Assert
        viewModel.IsActive.Should().BeFalse();
    }

    [Test]
    public void Backups_ShouldBeEmptyCollection_ByDefault()
    {
        // Assert
        _viewModel.Backups.Should().NotBeNull();
        _viewModel.Backups.Should().BeEmpty();
    }

    [Test]
    public void DisplayName_ShouldReturnGameTitle()
    {
        // Act & Assert
        _viewModel.GameDefinition.DisplayName.Should().Be("Test Game");
    }

    #endregion Public Methods
}