using System;
using System.IO;

namespace WindowResizerApp;

internal static class FileLogger
{
    private static readonly object SyncRoot = new();
    private static readonly string LogDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WindowResizer",
        "logs");
    private static readonly string LogPath = Path.Combine(LogDirectory, "app.log");

    public static void LogInfo(string message)
    {
        Write("INFO", message, null);
    }

    public static void LogError(Exception? exception, string message)
    {
        Write("ERROR", message, exception);
    }

    private static void Write(string level, string message, Exception? exception)
    {
        try
        {
            lock (SyncRoot)
            {
                Directory.CreateDirectory(LogDirectory);
                using var writer = new StreamWriter(LogPath, append: true);
                writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}");

                if (exception is not null)
                {
                    writer.WriteLine(exception);
                }
            }
        }
        catch
        {
            // Ignore logging failures to keep the tray app alive.
        }
    }
}
