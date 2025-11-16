using CommunityToolkit.Mvvm.ComponentModel;
using GameBackupManager.App.Models;
using GameBackupManager.App.Services;
using System.Collections.ObjectModel;

namespace GameBackupManager.App.ViewModels
{
    public partial class GameViewModel : ObservableObject
    {
        #region Fields

        private readonly GameDefinition _gameDefinition;

        [ObservableProperty]
        private ObservableCollection<BackupInfo> backups = new();

        [ObservableProperty]
        private bool isActive;

        #endregion Fields

        #region Public Constructors

        public GameViewModel(GameDefinition gameDefinition, bool isActive)
        {
            _gameDefinition = gameDefinition;
            IsActive = isActive;
        }

        #endregion Public Constructors

        #region Properties

        public GameDefinition GameDefinition => _gameDefinition;
        public string GamePath => _gameDefinition.GamePath;
        public string GameTitle => _gameDefinition.GameTitle;
        public bool IsInstalled => _gameDefinition.IsInstalled;
        public string SavePath => _gameDefinition.SavePath;
        public string Status => _gameDefinition.Status;

        #endregion Properties
    }
}