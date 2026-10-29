using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using CONCOST.UI.Controls;

namespace CONCOST.UI.Services;

public enum ToastType
{
    Info,
    Success,
    Warning,
    Error
}

public class ToastItem
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ToastType Type { get; set; } = ToastType.Info;
}

public class ToastService
{
    private static readonly Lazy<ToastService> _current = new(() => new ToastService());
    public static ToastService Current => _current.Value;

    public ObservableCollection<ToastItem> ActiveToasts { get; } = new();

    public void Show(string message, string title = "Thông báo", ToastType type = ToastType.Info, int durationMs = 3000)
    {
        var item = new ToastItem { Title = title, Message = message, Type = type };
        ActiveToasts.Add(item);

        int seconds = Math.Max(1, durationMs / 1000);
        string displayMessage = string.IsNullOrWhiteSpace(title) || title is "Thông báo" or "Thành công" or "Cảnh báo" or "Lỗi" or "Thông tin"
            ? message
            : $"{title}: {message}";

        switch (type)
        {
            case ToastType.Success:
                Growl.Success(displayMessage, seconds);
                break;
            case ToastType.Warning:
                Growl.Warning(displayMessage, seconds);
                break;
            case ToastType.Error:
                Growl.Error(displayMessage, seconds);
                break;
            default:
                Growl.Info(displayMessage, seconds);
                break;
        }

        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(durationMs) };
        timer.Tick += (s, e) =>
        {
            timer.Stop();
            ActiveToasts.Remove(item);
        };
        timer.Start();
    }

    public static void Success(string message, string title = "Thành công") => Current.Show(message, title, ToastType.Success);
    public static void Warning(string message, string title = "Cảnh báo") => Current.Show(message, title, ToastType.Warning);
    public static void Error(string message, string title = "Lỗi") => Current.Show(message, title, ToastType.Error);
    public static void Info(string message, string title = "Thông tin") => Current.Show(message, title, ToastType.Info);
}
