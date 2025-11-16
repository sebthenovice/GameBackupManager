using FluentAssertions;
using GameBackupManager.App.Models;
using GameBackupManager.App.Services;
using NUnit.Framework;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;

namespace GameBackupManager.Tests.Services;

[TestFixture]
public class JsonConfigurationServiceTests
{
    #region Fields

    private const string TestConfigPath = @"C:\test\config.json";
    private MockFileSystem _mockFileSystem;
    private JsonConfigurationService _service;

    #endregion Fields

    #region Public Methods

    [Test]
    public void LoadConfiguration_ShouldReturnDefault_WhenFileDoesNotExist()
    {
        // Act
        var result = _service.LoadConfiguration<AppSettings>(TestConfigPath);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.BackupLocation, Is.EqualTo(@"C:\GameBackups")); // Default value
    }

    [Test]
    public void LoadConfiguration_ShouldReturnDeserializedObject_WhenFileExists()
    {
        // Arrange
        var expectedSettings = new AppSettings
        {
            BackupLocation = @"D:\Backups",
            MaxBackupCount = 5
        };
        var json = JsonSerializer.Serialize(expectedSettings);
        _mockFileSystem.AddFile(TestConfigPath, new MockFileData(json));

        // Act
        var result = _service.LoadConfiguration<AppSettings>(TestConfigPath);

        // Assert
        result.Should().BeEquivalentTo(expectedSettings);
    }

    [Test]
    public void SaveConfiguration_ShouldCreateFile_WithCorrectJson()
    {
        // Arrange
        var settings = new AppSettings { BackupLocation = @"E:\MyBackups", Theme = "Light" };

        // Act
        _service.SaveConfiguration(TestConfigPath, settings);

        // Assert
        Assert.That(_mockFileSystem.FileExists(TestConfigPath), Is.True);
        var savedContent = _mockFileSystem.GetFile(TestConfigPath).TextContents;
        savedContent.Should().Contain("E:\\MyBackups").And.Contain("Light");
    }

    [Test]
    public void SaveConfiguration_ShouldOverwriteExistingFile()
    {
        // Arrange
        _mockFileSystem.AddFile(TestConfigPath, new MockFileData("old content"));
        var newSettings = new AppSettings { Theme = "Dark" };

        // Act
        _service.SaveConfiguration(TestConfigPath, newSettings);

        // Assert
        var savedContent = _mockFileSystem.GetFile(TestConfigPath).TextContents;
        savedContent.Should().NotContain("old content").And.Contain("Dark");
    }

    [SetUp]
    public void SetUp()
    {
        // Arrange: Create fresh mock file system for each test
        _mockFileSystem = new MockFileSystem();
        _service = new JsonConfigurationService(_mockFileSystem);
    }

    #endregion Public Methods
}