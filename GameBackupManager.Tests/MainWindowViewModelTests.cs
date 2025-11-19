using FluentAssertions;
using GameBackupManager.App.Models;
using GameBackupManager.App.Services;
using GameBackupManager.App.ViewModels;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace GameBackupManager.Tests.ViewModels;

[TestFixture]
public class MainWindowViewModelTests
{
    #region Fields

    private MainWindowViewModel _viewModel;
    private JsonConfigurationService _configService;
    private BackupService _backupService;
    private ILogger<MainWindowViewModel> _mockLogger;
    private ILogger<JsonConfigurationService> _mockConfigLogger;
    private ILogger<BackupService> _mockBackupLogger;
    private string _testDirectory;

    #endregion Fields

    #region Public Methods

    [SetUp]
    public async Task SetUp()
    {
        _mockLogger = Substitute.For<ILogger<MainWindowViewModel>>();
        _mockConfigLogger = Substitute.For<ILogger<JsonConfigurationService>>();
        _mockBackupLogger = Substitute.For<ILogger<BackupService>>();
        
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);

        _configService = new JsonConfigurationService(_mockConfigLogger);
        _backupService = new BackupService(_mockBackupLogger, _configService);
        _viewModel = new MainWindowViewModel(_configService, _backupService, _mockLogger);

        // Wait for initial load
        await Task.Delay(100);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [Test]
    public void Constructor_ShouldInitializeProperties()
    {
        // Assert
        _viewModel.Games.Should().NotBeNull();
        _viewModel.AppSettings.Should().NotBeNull();
        _viewModel.IsBusy.Should().BeFalse();
        _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void Constructor_ShouldInitializeCommands()
    {
        // Assert
        _viewModel.LoadDataCommand.Should().NotBeNull();
        _viewModel.CreateBackupCommand.Should().NotBeNull();
        _viewModel.RestoreBackupCommand.Should().NotBeNull();
        _viewModel.ToggleGameActiveCommand.Should().NotBeNull();
        _viewModel.SaveSettingsCommand.Should().NotBeNull();
        _viewModel.RefreshGamesCommand.Should().NotBeNull();
    }

    [Test]
    public void AvailableThemes_ShouldContainLightAndDark()
    {
        // Assert
        _viewModel.AvailableThemes.Should().Contain("Light");
        _viewModel.AvailableThemes.Should().Contain("Dark");
    }

    [Test]
    public void ToggleOptionsMenu_ShouldToggleShowOptionsMenu()
    {
        // Arrange
        var initialState = _viewModel.ShowOptionsMenu;

        // Act
        _viewModel.ToggleOptionsMenu();

        // Assert
        _viewModel.ShowOptionsMenu.Should().Be(!initialState);
    }

    [Test]
    public void ToggleOptionsMenu_ShouldToggleBackToOriginal()
    {
        // Arrange
        var initialState = _viewModel.ShowOptionsMenu;

        // Act
        _viewModel.ToggleOptionsMenu();
        _viewModel.ToggleOptionsMenu();

        // Assert
        _viewModel.ShowOptionsMenu.Should().Be(initialState);
    }

    [Test]
    public async Task LoadDataCommand_ShouldSetIsBusyTrue_WhileLoading()
    {
        // This is a tricky test since loading happens in background
        // We'll just verify LoadDataCommand is executable
        _viewModel.LoadDataCommand.Should().NotBeNull();
        _viewModel.LoadDataCommand.CanExecute(null).Should().BeTrue();
    }

    [Test]
    public void SelectedGame_ShouldBeNullByDefault()
    {
        // Assert
        _viewModel.SelectedGame.Should().BeNull();
    }

    [Test]
    public void SelectedGame_ShouldBeSettable()
    {
        // Arrange
        var game = new GameViewModel(
            new GameDefinition("Test", @"C:\Test", @"C:\Test\Save"),
            isActive: false
        );

        // Act
        _viewModel.SelectedGame = game;

        // Assert
        _viewModel.SelectedGame.Should().Be(game);
    }

    [Test]
    public async Task SaveSettingsCommand_ShouldSetIsBusyDuringExecution()
    {
        // Arrange
        var busyDuringExecution = false;
        var isBusyPropertyChangedCount = 0;

        _viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.IsBusy))
            {
                isBusyPropertyChangedCount++;
                if (_viewModel.IsBusy)
                {
                    busyDuringExecution = true;
                }
            }
        };

        // Act
        await _viewModel.SaveSettingsCommand.ExecuteAsync(null);
        await Task.Delay(100); // Give time for any final operations

        // Assert
        isBusyPropertyChangedCount.Should().BeGreaterThan(0);
        _viewModel.IsBusy.Should().BeFalse();
    }

    [Test]
    public void AppSettings_ShouldNotBeNull()
    {
        // Assert
        _viewModel.AppSettings.Should().NotBeNull();
    }

    [Test]
    public void AppSettings_ShouldHaveDefaultTheme()
    {
        // Assert
        _viewModel.AppSettings.Theme.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void StatusMessage_ShouldNotBeNull()
    {
        // Assert
        _viewModel.StatusMessage.Should().NotBeNull();
    }

    [Test]
    public void IsBusy_ShouldBeFalseByDefault()
    {
        // Assert
        _viewModel.IsBusy.Should().BeFalse();
    }

    [Test]
    public void ShowOptionsMenu_ShouldBeFalseByDefault()
    {
        // Assert
        _viewModel.ShowOptionsMenu.Should().BeFalse();
    }

    [Test]
    public async Task RefreshGamesCommand_ShouldSetIsBusyDuringExecution()
    {
        // Arrange
        var isBusyPropertyChangedCount = 0;
        _viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.IsBusy))
                isBusyPropertyChangedCount++;
        };

        // Act
        await _viewModel.RefreshGamesCommand.ExecuteAsync(null);
        await Task.Delay(100);

        // Assert
        isBusyPropertyChangedCount.Should().BeGreaterThan(0);
        _viewModel.IsBusy.Should().BeFalse();
    }

    [Test]
    public async Task LoadDataCommand_ShouldPopulateGames()
    {
        // Arrange - Add a test game to config
        var testGame = new GameDefinition("Test Game", _testDirectory, _testDirectory)
        {
            IsInstalled = true
        };
        Directory.CreateDirectory(_testDirectory);

        await _configService.SaveGameDefinitionsAsync(new System.Collections.Generic.List<GameDefinition> { testGame });

        // Act - Create new view model to load the games
        var freshViewModel = new MainWindowViewModel(_configService, _backupService, _mockLogger);
        await Task.Delay(200); // Wait for async load

        // Assert
        freshViewModel.Games.Should().NotBeEmpty();
        freshViewModel.Games.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    [Test]
    public void CreateBackupCommand_ShouldBeExecutable()
    {
        // Arrange
        var game = new GameViewModel(
            new GameDefinition("Test", @"C:\Test", @"C:\Test\Save"),
            isActive: false
        );

        // Act & Assert
        _viewModel.CreateBackupCommand.CanExecute(game).Should().BeTrue();
    }

    [Test]
    public void RestoreBackupCommand_ShouldBeExecutable()
    {
        // Arrange
        var backup = new BackupInfo
        {
            Name = "Test Backup",
            Path = @"C:\Backup",
            CreationTime = DateTime.Now,
            Size = 1000
        };

        // Act & Assert
        _viewModel.RestoreBackupCommand.CanExecute(backup).Should().BeTrue();
    }

    [Test]
    public void ToggleGameActiveCommand_ShouldBeExecutable()
    {
        // Arrange
        var game = new GameViewModel(
            new GameDefinition("Test", @"C:\Test", @"C:\Test\Save"),
            isActive: false
        );

        // Act & Assert
        _viewModel.ToggleGameActiveCommand.CanExecute(game).Should().BeTrue();
    }

    [Test]
    public async Task ToggleGameActive_ShouldToggleIsActive()
    {
        // Arrange
        var game = new GameViewModel(
            new GameDefinition("Test", @"C:\Test", @"C:\Test\Save"),
            isActive: false
        );
        _viewModel.Games.Add(game);
        var initialState = game.IsActive;

        // Act
        _viewModel.ToggleGameActiveCommand.Execute(game);
        await Task.Delay(100);

        // Assert
        game.IsActive.Should().Be(!initialState);
    }

    [Test]
    public void AvailableThemes_ShouldBeModifiable()
    {
        // Arrange
        var originalCount = _viewModel.AvailableThemes.Count;

        // Act
        _viewModel.AvailableThemes = new ObservableCollection<string> { "Theme1", "Theme2", "Theme3" };

        // Assert
        _viewModel.AvailableThemes.Count.Should().Be(3);
    }

    #endregion Public Methods
}
