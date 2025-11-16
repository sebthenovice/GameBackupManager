using Avalonia.Controls.Shapes;
using FluentAssertions;
using GameBackupManager.App.Models;
using GameBackupManager.App.Services;
using GameBackupManager.App.ViewModels;
using NSubstitute;
using NUnit.Framework;
using System.Threading.Tasks;

namespace GameBackupManager.Tests.ViewModels;

[TestFixture]
public class GameViewModelTests
{
    #region Fields

    private IBackupService _mockBackupService;
    private GameDefinition _testGame;
    private GameViewModel _viewModel;

    #endregion Fields

    #region Public Methods

    [Test]
    public async Task BackupCommand_ShouldCallBackupService_WhenExecuted()
    {
        // Arrange
        _mockBackupService.CreateBackup(Arg.Any<GameDefinition>(), Arg.Any<AppSettings>())
            .Returns(new BackupResult { Success = true });

        // Act
        await _viewModel.BackupCommand.ExecuteAsync(null);

        // Assert
        await _mockBackupService.Received(1).CreateBackup(_testGame, Arg.Any<AppSettings>());
    }

    [Test]
    public async Task BackupCommand_ShouldSetIsProcessing_DuringExecution()
    {
        // Arrange
        _mockBackupService.CreateBackup(default, default)
            .ReturnsForAnyArgs(new BackupResult { Success = true });

        bool processingStarted = false;
        bool processingEnded = false;

        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.IsProcessing))
            {
                if (_viewModel.IsProcessing) processingStarted = true;
                else processingEnded = true;
            }
        };

        // Act
        await _viewModel.BackupCommand.ExecuteAsync(null);

        // Assert
        processingStarted.Should().BeTrue();
        processingEnded.Should().BeTrue();
    }

    [Test]
    public void Constructor_ShouldInitializeProperties()
    {
        // Assert
        _viewModel.GameTitle.Should().Be("Test Game");
        _viewModel.IsProcessing.Should().BeFalse();
        _viewModel.BackupCommand.Should().NotBeNull();
    }

    [SetUp]
    public void SetUp()
    {
        _mockBackupService = Substitute.For<IBackupService>();
        _testGame = new GameDefinition { GameTitle = "Test Game" };
        _viewModel = new GameViewModel(_testGame, _mockBackupService);
    }

    #endregion Public Methods
}