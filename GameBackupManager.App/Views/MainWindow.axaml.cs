using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GameBackupManager.App.ViewModels;

namespace GameBackupManager.App.Views
{
    public partial class MainWindow : Window
    {
        #region Public Constructors

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion Public Constructors

        #region Private Methods

        private void GameCard_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is Border border && border.DataContext is GameViewModel gameViewModel)
            {
                if (DataContext is MainWindowViewModel mainViewModel)
                {
                    mainViewModel.SelectedGame = gameViewModel;
                    _ = mainViewModel.RefreshGamesCommand.ExecuteAsync(null);
                }
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        #endregion Private Methods
    }
}