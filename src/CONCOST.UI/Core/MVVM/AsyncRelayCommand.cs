using System.Windows.Input;

namespace CONCOST.UI.Core.MVVM;

/// <summary>
/// ICommand bất đồng bộ cho phép thực thi tác vụ Async mà không khóa giao diện.
/// </summary>
public class AsyncRelayCommand : ICommand
{
    private readonly Func<object?, Task> _executeAsync;
    private readonly Predicate<object?>? _canExecute;
    private bool _isExecuting;

    public AsyncRelayCommand(Func<object?, Task> executeAsync, Predicate<object?>? canExecute = null)
    {
        _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
        _canExecute = canExecute;
    }

    public AsyncRelayCommand(Func<Task> executeAsync, Func<bool>? canExecute = null)
        : this(_ => executeAsync(), canExecute == null ? null : _ => canExecute())
    {
    }

    public bool IsExecuting
    {
        get => _isExecuting;
        private set
        {
            _isExecuting = value;
            RaiseCanExecuteChanged();
        }
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter)
    {
        if (_isExecuting) return false;
        return _canExecute?.Invoke(parameter) ?? true;
    }

    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter)) return;

        try
        {
            IsExecuting = true;
            await _executeAsync(parameter);
        }
        finally
        {
            IsExecuting = false;
        }
    }

    public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
}
