using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace CONCOST.UI.RevitLike;

public partial class App : Application
{
    /// <summary>Where unhandled exceptions are appended.</summary>
    public static string CrashLogPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CONCOST.UI.RevitLike",
        "crash.log");

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Logged to LocalAppData: the folder next to the executable may be read-only, and a logger
        // that throws while logging would hide the failure it was meant to record.
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            Write(args.ExceptionObject as Exception, "AppDomain");

        DispatcherUnhandledException += (_, args) =>
        {
            Write(args.Exception, "Dispatcher");

            MessageBox.Show(
                $"{args.Exception.GetType().Name}: {args.Exception.Message}\n\nLog written to:\n{CrashLogPath}",
                "CONCOST Revit-Like — unhandled error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            args.Handled = true;
        };
    }

    private static void Write(Exception? exception, string source)
    {
        if (exception is null) return;

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(CrashLogPath)!);

            var text = new StringBuilder()
                .AppendLine("────────────────────────────────────────────")
                .AppendLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  [{source}]  v{typeof(App).Assembly.GetName().Version}")
                .AppendLine(exception.ToString())
                .ToString();

            File.AppendAllText(CrashLogPath, text);
            Trace.WriteLine(text);
        }
        catch (IOException)
        {
            // Nothing sensible left to do — never mask the original failure with a logging one.
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
