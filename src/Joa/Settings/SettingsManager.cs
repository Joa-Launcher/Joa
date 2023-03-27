﻿using System.Text.Json;
using Joa.PluginCore;
using JoaLauncher.Api.Injectables;
using LogLevel = JoaLauncher.Api.Injectables.LogLevel;

namespace Joa.Settings;

public class SettingsManager
{
    private readonly PluginManager _pluginManager;
    private readonly IJoaLogger _logger;
    private readonly FileSystemManager _fileSystemManager;
    private readonly JsonSerializerOptions _options;
    private readonly FileWatcher _fileWatcher;

    public Action SettingsChangedOutsideOfUi { get; set; }
    public GeneralSettings GeneralSettings { get; set; } = new();

    public SettingsManager(PluginManager pluginManager, IJoaLogger logger, FileSystemManager fileSystemManager)
    {
        _pluginManager = pluginManager;
        _logger = logger;
        _fileSystemManager = fileSystemManager;
        _options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        _fileWatcher = new FileWatcher(fileSystemManager.GetSettingsLocation(), () =>
        {
            Sync();
            SettingsChangedOutsideOfUi();
        }, 10);
        Sync();
        pluginManager.UpdateIndexesAsync().GetAwaiter().GetResult();
    }

    private void Sync()
    {
        _logger.Log("Synchronizing the settings.", LogLevel.Info);
        UpdateSettingsFromJson();
        SaveSettingsToJsonAsync().GetAwaiter().GetResult();
    }

    public async Task SaveSettingsToJsonAsync()
    {
        _fileWatcher.Disable();
        using var _ = _logger.TimedOperation(nameof(SaveSettingsToJsonAsync));

        // _globalHotKey.RegisterUiHotKey();

        try
        {
            var dtoSetting = new DtoSettings(_pluginManager.Plugins, GeneralSettings);
            var jsonString = JsonSerializer.Serialize(dtoSetting, _options);
            await File.WriteAllTextAsync(_fileSystemManager.GetSettingsLocation(), jsonString);
        }
        catch (Exception e)
        {
            _logger.Log(
                $"There was an exception thrown while Saving the Settings with the following exception{Environment.NewLine}{e}",
                LogLevel.Error);
        }
        finally
        {
            _fileWatcher.Enable();
        }
    }

    private void UpdateSettingsFromJson()
    {
        using var _ = _logger.TimedOperation(nameof(UpdateSettingsFromJson));

        try
        {
            var jsonString = File.ReadAllText(_fileSystemManager.GetSettingsLocation());
            if (string.IsNullOrEmpty(jsonString))
                return;

            var result = JsonSerializer.Deserialize<DtoSettings>(jsonString);
            if (result is null)
                throw new JsonException();

            JsonUtilities.PopulateObject(jsonString, GeneralSettings);

            foreach (var pluginDefinition in _pluginManager.Plugins)
            {
                UpdatePluginDefinition(pluginDefinition, result);
            }
        }
        catch (Exception e)
        {
            _logger.Log(
                $"There was an exception thrown while Updating the Settings from the settings.json with the following exception{Environment.NewLine}{e}",
                LogLevel.Error);
        }
    }

    private void UpdatePluginDefinition(PluginDefinition pluginDefinition, DtoSettings newDtoSettings)
    {
        if (!newDtoSettings.Plugins.TryGetValue(pluginDefinition.Manifest.Id, out var newPlugin))
            return;

        JsonUtilities.PopulateObject(newPlugin.Setting, pluginDefinition.Setting);
    }
}