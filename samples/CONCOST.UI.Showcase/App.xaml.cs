using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace CONCOST.UI.Showcase;

public partial class App : Application
{
    /// <summary>Where unhandled exceptions are appended.</summary>
    public static string CrashLogPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CONCOST.UI.Showcase",
        "crash.log");

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Written to LocalAppData rather than the working directory: the folder next to the exe may be
        // read-only (Program Files, a network share), and a logger that throws while logging leaves
        // nothing behind at all.
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            Write(args.ExceptionObject as Exception, "AppDomain");

        DispatcherUnhandledException += (_, args) =>
        {
            Write(args.Exception, "Dispatcher");

            // A demo should not vanish because one handler threw: report it and keep the window up so
            // the rest of the catalogue stays usable.
            MessageBox.Show(
                $"{args.Exception.GetType().Name}: {args.Exception.Message}\n\nĐã ghi log vào:\n{CrashLogPath}",
                "CONCOST.UI Showcase — lỗi không xử lý",
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
