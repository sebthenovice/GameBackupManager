# Game Backup Manager

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

### Adding New Games

1. Edit the `games.json` file in your application data directory
2. Add new game definitions following the format above
3. Restart the application to see the new games

### Creating Backups

1. Select a game from the main list
2. Click the "Backup" button
3. The backup will be created in your configured backup location

### Restoring Backups

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

Run the application and test with various games:

```bash
dotnet run --project GameBackupManager/GameBackupManager.csproj
```

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

## License

This project is open source. Feel free to modify and distribute according to your needs.

## Acknowledgments

- Avalonia UI team for the excellent cross-platform framework
- Discord for UI design inspiration
- The .NET team for the powerful development platform