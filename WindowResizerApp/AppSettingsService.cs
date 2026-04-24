using System;
using System.IO;
using System.Text.Json;

namespace WindowResizerApp;

internal sealed class AppSettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public AppSettingsService()
    {
        SettingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WindowResizer");
        SettingsPath = Path.Combine(SettingsDirectory, "settings.json");
    }

    public string SettingsDirectory { get; }

    public string SettingsPath { get; }

    public AppSettings Load()
    {
        try
        {
            Directory.CreateDirectory(SettingsDirectory);

            if (!File.Exists(SettingsPath))
            {
                var defaults = new AppSettings();
                Save(defaults);
                return defaults;
            }

            var json = File.ReadAllText(SettingsPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
            settings ??= new AppSettings();
            settings.Normalize();
            return settings;
        }
        catch (Exception ex)
        {
            FileLogger.LogError(ex, "Failed to load settings. Falling back to defaults.");
            var fallback = new AppSettings();
            fallback.Normalize();
            return fallback;
        }
    }

    public void Save(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(SettingsDirectory);
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(SettingsPath, json);
        }
        catch (Exception ex)
        {
            FileLogger.LogError(ex, "Failed to save settings.");
        }
    }
}
