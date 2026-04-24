using System;
using Microsoft.Win32;

namespace WindowResizerPlugin;

public static class StartupManager
{
    private const string RunKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string DefaultAppName = "WindowResizerApp";

    public static bool SetStartup(string appPath, string appName = DefaultAppName)
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
            if (key is null)
            {
                return false;
            }

            key.SetValue(appName, $"\"{appPath}\"");
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool RemoveStartup(string appName = DefaultAppName)
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
            if (key is null || key.GetValue(appName) is null)
            {
                return false;
            }

            key.DeleteValue(appName);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsStartupEnabled(string appName = DefaultAppName)
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKeyPath);
            return key?.GetValue(appName) is not null;
        }
        catch
        {
            return false;
        }
    }
}
