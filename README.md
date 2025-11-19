# Game Backup Manager

[![CI - Dev Branch](https://github.com/sebthenovice/GameBackupManager/workflows/CI%20-%20Dev%20Branch/badge.svg)](https://github.com/sebthenovice/GameBackupManager/actions/workflows/ci-dev.yaml)
[![Release](https://github.com/sebthenovice/GameBackupManager/workflows/Release%20-%20Create%20on%20PR%20Merge/badge.svg)](https://github.com/sebthenovice/GameBackupManager/actions/workflows/release.yaml)
[![codecov](https://codecov.io/gh/sebthenovice/GameBackupManager/branch/dev-ui/graph/badge.svg)](https://codecov.io/gh/sebthenovice/GameBackupManager)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.txt)

A modern, cross-platform game save backup manager built with C# .NET 10 and Avalonia UI. Automatically detect installed games, manage save backups, and restore previous save states with ease.

## Features

- 🎮 **Automatic Game Detection**: Scans for installed games and their save locations
- 💾 **Smart Backup System**: Create compressed or uncompressed backups with timestamps
- 🔄 **Easy Restore**: One-click restore to previous save states
- 📊 **Backup Management**: Automatic cleanup of old backups with configurable retention
- 🎨 **Modern UI**: Clean, Discord-inspired dark theme with smooth animations
- ⚙️ **Configurable Settings**: Customize backup location, compression, and more
- 🎯 **Selective Management**: Choose which games to actively manage

## Requirements

- .NET 10.0 SDK
- Windows 10/11, macOS, or Linux
- Avalonia UI dependencies (automatically installed)

## Installation

1. Clone or download this repository
2. Navigate to the project directory
3. Run the application:

```bash
dotnet run --project GameBackupManager/GameBackupManager.csproj
```

Or build and run:

```bash
dotnet build
dotnet run --project GameBackupManager/GameBackupManager.csproj
```

## Configuration

The application uses three JSON configuration files stored in your application data directory:

### 1. Games Configuration (`games.json`)
Defines the games to manage:

```json
[
  {
    "gameTitle": "The Witcher 3: Wild Hunt",
    "gamePath": "C:\\Program Files\\The Witcher 3 Wild Hunt",
    "savePath": "C:\\Users\\[Username]\\Documents\\The Witcher 3\\gamesaves",
    "executableName": "witcher3.exe",
    "backupFolderName": "witcher3_saves",
    "isInstalled": true
  }
]
```

### 2. App Settings (`appsettings.json`)
Application-wide settings:

```json
{
  "backupLocation": "C:\\Users\\[Username]\\Documents\\GameBackups",
  "autoBackupOnLaunch": false,
  "backupCompression": true,
  "maxBackupCount": 10,
  "theme": "Dark",
  "checkForUpdates": true,
  "notificationSounds": true
}
```

### 3. Active Games (`activegames.json`)
Tracks which games are currently being managed:

```json
{
  "activeGameIds": ["The Witcher 3: Wild Hunt", "Cyberpunk 2077"],
  "lastUpdated": "2025-11-16T10:30:00Z"
}
```

## Usage

### User Guide — Quickstart (for end users)

Follow these quick steps to install, run, back up saves, and restore them.

Install and run

- Make sure you have the .NET 10.0 SDK installed.
- Download or clone this repository and run the published app or build it locally.
- To run from source: `dotnet run --project GameBackupManager/GameBackupManager.App.csproj`.

Configure the app (first run)

- On first run the app will create its configuration files in your application data folder (see the three JSON files described above).
- Review `games.json` to add any games the app didn't auto-detect. Provide the executable name and save path where required.

Create a backup

- Open the app and choose the game you want to back up from the main list.
- Click the "Backup" button (or use the context menu) to create a timestamped backup.
- Backups are stored under your configured backup location (e.g., `C:\Users\\<you>\\Documents\\GameBackups`).

Restore a backup

- Select a game, pick a backup from the list, and click "Restore".
- The selected save state will be copied back to the game's save folder; the app creates a pre-restore backup automatically.

Manage backups and retention

- Change retention and compression settings in the app settings (max backups, compression on/off).
- Use the automatic cleanup setting to delete older backups beyond the retention limit.

Troubleshooting (quick)

- If a game isn't detected, verify the `gamePath` and `savePath` in `games.json` and restart the app.
- If a backup fails, check that the save path exists and is writable.
- For permission problems on Windows, run the app with elevated privileges only when necessary.

### Creating Backups (details)

1. Select a game from the main list
2. Click the "Backup" button
3. The backup will be created in your configured backup location

### Restoring Backups (details)

1. Select a game from the main list
2. Choose a backup from the right panel
3. Click "Restore" to revert to that save state

### Managing Active Games

- Use the checkboxes to enable/disable games for backup management
- Only active games will be included in automatic operations
- Settings are automatically saved when changed

## Default Game Locations

The application comes pre-configured with several popular games:

- **The Witcher 3: Wild Hunt**
- **Cyberpunk 2077**
- **Elden Ring**

You can add more games by editing the configuration files.

## Backup Structure

Backups are organized in the following structure:

```
GameBackups/
├── witcher3_saves/
│   ├── The Witcher 3_backup_2025-11-16_10-30-45.zip
│   └── The Witcher 3_backup_2025-11-15_14-22-10.zip
├── cyberpunk_saves/
│   ├── Cyberpunk 2077_backup_2025-11-16_09-15-30.zip
│   └── ...
```

## Technical Details

### Architecture

- **UI Framework**: Avalonia UI 11.0.10
- **Target Framework**: .NET 10.0
- **Design Pattern**: MVVM (Model-View-ViewModel)
- **Dependency Injection**: Built-in .NET DI container
- **Logging**: Microsoft.Extensions.Logging

### Key Components

- **Models**: GameDefinition, AppSettings, ActiveGames
- **Services**: JsonConfigurationService, BackupService
- **ViewModels**: MainViewModel, GameViewModel
- **Views**: MainWindow

### Styling

- Custom dark theme inspired by Discord
- Responsive design with smooth transitions
- Modern typography using Inter font
- Consistent color palette and spacing

## Development

### Building from Source

1. Ensure you have .NET 10.0 SDK installed
2. Clone the repository
3. Build the project:

```bash
dotnet build GameBackupManager/GameBackupManager.csproj
```

### Adding New Features

The application is designed to be extensible:

- Add new models in the `Models/` directory
- Implement services in the `Services/` directory
- Create view models in the `ViewModels/` directory
- Design views in the `Views/` directory

### Testing

The project includes comprehensive unit tests using NUnit, FluentAssertions, and NSubstitute.

#### Running Tests Locally

```bash
dotnet test GameBackupManager.Tests/GameBackupManager.Tests.csproj
```

#### Running Tests with Coverage Report

```bash
dotnet test GameBackupManager.Tests/GameBackupManager.Tests.csproj \
  --collect:"XPlat Code Coverage" \
  --results-directory ./coverage
```

#### Coverage Reports

Code coverage is automatically generated on every push to the `dev-ui` branch and uploaded to [Codecov](https://codecov.io/gh/sebthenovice/GameBackupManager). You can view the coverage reports on the [Codecov dashboard](https://codecov.io/gh/sebthenovice/GameBackupManager).

#### Test Structure

- **GameDefinitionTests**: Tests for game model validation and properties
- **BackupServiceTests**: Tests for backup creation, restoration, and management
- **JsonConfigurationServiceTests**: Tests for configuration file I/O and serialization
- **GameViewModelTests**: Tests for view model logic and data binding

## Troubleshooting

### Common Issues

1. **Games not detected**: Ensure the game paths in `games.json` are correct
2. **Backup fails**: Check that the save paths exist and are accessible
3. **Permission errors**: Run the application with appropriate permissions
4. **UI not responsive**: Ensure all Avalonia dependencies are properly installed

### Log Files

The application logs to the console by default. For detailed logging, check the console output when running the application.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

For developer-specific contribution guidelines, branch naming conventions, CI expectations (which branch names trigger CI and how releases are created), and local test/build commands, see: `.github/DEVELOPER_GUIDE.md`.

## License

This project is open source. Feel free to modify and distribute according to your needs.

## Acknowledgments

- Avalonia UI team for the excellent cross-platform framework
- Discord for UI design inspiration
- The .NET team for the powerful development platform
