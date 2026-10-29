namespace CONCOST.UI.Services;

public enum DialogResult
{
    None,
    OK,
    Cancel,
    Yes,
    No
}

public enum DialogType
{
    Information,
    Warning,
    Error,
    Question
}

public interface IDialogService
{
    Task<DialogResult> ShowMessageAsync(string title, string message, DialogType type = DialogType.Information);
    Task<bool> ShowConfirmAsync(string title, string message);
}

public class DialogService : IDialogService
{
    private static readonly Lazy<DialogService> _current = new(() => new DialogService());
    public static DialogService Current => _current.Value;

    public Task<DialogResult> ShowMessageAsync(string title, string message, DialogType type = DialogType.Information)
    {
        var icon = type switch
        {
            DialogType.Error => System.Windows.MessageBoxImage.Error,
            DialogType.Warning => System.Windows.MessageBoxImage.Warning,
            DialogType.Question => System.Windows.MessageBoxImage.Question,
            _ => System.Windows.MessageBoxImage.Information
        };

        var result = System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, icon);
        return Task.FromResult(result == System.Windows.MessageBoxResult.OK ? DialogResult.OK : DialogResult.Cancel);
    }

    public Task<bool> ShowConfirmAsync(string title, string message)
    {
        var result = System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
        return Task.FromResult(result == System.Windows.MessageBoxResult.Yes);
    }
}
