using System;
using System.Threading;
using System.Windows.Forms;

namespace WindowResizerApp;

internal static class Program
{
    private const string MutexName = @"Local\WindowResizerApp.Singleton";

    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        using var mutex = new Mutex(true, MutexName, out var createdNew);
        if (!createdNew)
        {
            return;
        }

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            FileLogger.LogError(args.ExceptionObject as Exception, "Unhandled AppDomain exception");

        Application.ThreadException += (_, args) =>
            FileLogger.LogError(args.Exception, "Unhandled UI thread exception");

        Application.Run(new TrayApplicationContext());
    }
}
