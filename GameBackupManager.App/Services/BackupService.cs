using GameBackupManager.App.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace GameBackupManager.App.Services
{
    public class BackupInfo
    {
        #region Properties

        public DateTime CreationTime { get; set; }
        public string FormattedDate => CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
        public string FormattedSize => FormatFileSize(Size);
        public bool IsCompressed { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public long Size { get; set; }

        #endregion Properties

        #region Private Methods

        private string FormatFileSize(long bytes)
        {
            const int scale = 1024;
            string[] units = { "B", "KB", "MB", "GB", "TB" };

            long max = (long)Math.Pow(scale, units.Length - 1);

            foreach (var unit in units)
            {
                if (bytes > max)
                    return $"{decimal.Divide(bytes, max):##.##} {unit}";

                max /= scale;
            }

            return "0 B";
        }

        #endregion Private Methods
    }

    public class BackupResult
    {
        #region Properties

        public string? BackupPath { get; set; }
        public long BackupSize { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; }

        #endregion Properties
    }

    public class BackupService
    {
        #region Fields

        private readonly JsonConfigurationService _configService;
        private readonly ILogger<BackupService> _logger;

        #endregion Fields

        #region Public Constructors

        public BackupService(ILogger<BackupService> logger, JsonConfigurationService configService)
        {
            _logger = logger;
            _configService = configService;
        }

        #endregion Public Constructors

        #region Public Methods

        public async Task<BackupResult> CreateBackupAsync(GameDefinition game)
        {
            try
            {
                if (!game.IsInstalled)
                {
                    return new BackupResult { Success = false, Message = "Game is not installed" };
                }

                if (!Directory.Exists(game.SavePath))
                {
                    return new BackupResult { Success = false, Message = "Save path does not exist" };
                }

                var settings = await _configService.LoadAppSettingsAsync();
                var backupFolderName = game.BackupFolderName ?? game.GameTitle.Replace(" ", "_").ToLower();
                var backupPath = Path.Combine(settings.BackupLocation, backupFolderName);

                // Create backup directory if it doesn't exist
                Directory.CreateDirectory(backupPath);

                // Generate timestamp for backup
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                var backupName = $"{game.GameTitle}_backup_{timestamp}";

                string backupFilePath;
                if (settings.BackupCompression)
                {
                    backupFilePath = Path.Combine(backupPath, $"{backupName}.zip");
                    await CreateCompressedBackup(game.SavePath, backupFilePath);
                }
                else
                {
                    backupFilePath = Path.Combine(backupPath, backupName);
                    await CreateDirectoryBackup(game.SavePath, backupFilePath);
                }

                // Clean up old backups
                await CleanupOldBackups(backupPath, settings.MaxBackupCount);

                return new BackupResult
                {
                    Success = true,
                    Message = "Backup created successfully",
                    BackupPath = backupFilePath,
                    BackupSize = GetBackupSize(backupFilePath)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating backup for game {GameTitle}", game.GameTitle);
                return new BackupResult { Success = false, Message = ex.Message };
            }
        }

        public async Task<List<BackupInfo>> GetAvailableBackupsAsync(GameDefinition game)
        {
            var settings = await _configService.LoadAppSettingsAsync();
            var backupFolderName = game.BackupFolderName ?? game.GameTitle.Replace(" ", "_").ToLower();
            var backupPath = Path.Combine(settings.BackupLocation, backupFolderName);

            var backups = new List<BackupInfo>();

            if (!Directory.Exists(backupPath))
            {
                return backups;
            }

            // Get both compressed and directory backups
            var zipFiles = Directory.GetFiles(backupPath, "*.zip");
            var directories = Directory.GetDirectories(backupPath)
                .Where(dir => !Path.GetFileName(dir).StartsWith("."));

            foreach (var zipFile in zipFiles)
            {
                var fileInfo = new FileInfo(zipFile);
                backups.Add(new BackupInfo
                {
                    Name = Path.GetFileNameWithoutExtension(zipFile),
                    Path = zipFile,
                    CreationTime = fileInfo.CreationTime,
                    Size = fileInfo.Length,
                    IsCompressed = true
                });
            }

            foreach (var directory in directories)
            {
                var dirInfo = new DirectoryInfo(directory);
                backups.Add(new BackupInfo
                {
                    Name = Path.GetFileName(directory),
                    Path = directory,
                    CreationTime = dirInfo.CreationTime,
                    Size = GetDirectorySize(directory),
                    IsCompressed = false
                });
            }

            return backups.OrderByDescending(b => b.CreationTime).ToList();
        }

        public async Task<BackupResult> RestoreBackupAsync(GameDefinition game, string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath) && !Directory.Exists(backupPath))
                {
                    return new BackupResult { Success = false, Message = "Backup file not found" };
                }

                // Create backup of current saves before restoring
                var currentBackupResult = await CreateBackupAsync(game);
                if (!currentBackupResult.Success)
                {
                    _logger.LogWarning("Failed to create current saves backup before restore");
                }

                if (Path.GetExtension(backupPath).ToLower() == ".zip")
                {
                    await RestoreCompressedBackup(backupPath, game.SavePath);
                }
                else
                {
                    await RestoreDirectoryBackup(backupPath, game.SavePath);
                }

                return new BackupResult
                {
                    Success = true,
                    Message = "Backup restored successfully",
                    BackupPath = backupPath
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring backup for game {GameTitle}", game.GameTitle);
                return new BackupResult { Success = false, Message = ex.Message };
            }
        }

        #endregion Public Methods

        #region Private Methods

        private async Task CleanupOldBackups(string backupPath, int maxCount)
        {
            await Task.Run(() =>
            {
                var backupFiles = Directory.GetFiles(backupPath, "*.zip")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .Skip(maxCount);

                var backupDirs = Directory.GetDirectories(backupPath)
                    .Select(d => new DirectoryInfo(d))
                    .OrderByDescending(d => d.CreationTime)
                    .Skip(maxCount);

                foreach (var oldFile in backupFiles)
                {
                    oldFile.Delete();
                }

                foreach (var oldDir in backupDirs)
                {
                    oldDir.Delete(true);
                }
            });
        }

        private void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, destSubDir);
            }
        }

        private async Task CreateCompressedBackup(string sourcePath, string destinationPath)
        {
            await Task.Run(() =>
            {
                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }

                ZipFile.CreateFromDirectory(sourcePath, destinationPath, CompressionLevel.Optimal, false);
            });
        }

        private async Task CreateDirectoryBackup(string sourcePath, string destinationPath)
        {
            await Task.Run(() =>
            {
                if (Directory.Exists(destinationPath))
                {
                    Directory.Delete(destinationPath, true);
                }

                CopyDirectory(sourcePath, destinationPath);
            });
        }

        private long GetBackupSize(string backupPath)
        {
            if (File.Exists(backupPath))
            {
                return new FileInfo(backupPath).Length;
            }
            else if (Directory.Exists(backupPath))
            {
                return GetDirectorySize(backupPath);
            }
            return 0;
        }

        private long GetDirectorySize(string path)
        {
            return Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                .Sum(file => new FileInfo(file).Length);
        }

        private async Task RestoreCompressedBackup(string backupPath, string destinationPath)
        {
            await Task.Run(() =>
            {
                if (Directory.Exists(destinationPath))
                {
                    Directory.Delete(destinationPath, true);
                }

                Directory.CreateDirectory(destinationPath);
                ZipFile.ExtractToDirectory(backupPath, destinationPath);
            });
        }

        private async Task RestoreDirectoryBackup(string backupPath, string destinationPath)
        {
            await Task.Run(() =>
            {
                if (Directory.Exists(destinationPath))
                {
                    Directory.Delete(destinationPath, true);
                }

                CopyDirectory(backupPath, destinationPath);
            });
        }

        #endregion Private Methods
    }
}