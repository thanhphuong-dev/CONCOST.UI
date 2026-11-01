using System;
using System.Windows.Input;

namespace CONCOST.UI.RevitLike.Shell;

/// <summary>
/// The command type the whole shell is wired with: ribbon buttons, menu rows, the quick access
/// strip and the command line all point at the same instance, so enabling and disabling stays
/// consistent everywhere.
/// </summary>
public sealed class ShellCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    public ShellCommand(Action execute, Func<bool>? canExecute = null)
    {
        if (execute is null) throw new ArgumentNullException(nameof(execute));

        _execute = _ => execute();
        _canExecute = canExecute is null ? null : _ => canExecute();
    }

    public ShellCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    public void Execute(object? parameter)
    {
        if (CanExecute(parameter)) _execute(parameter);
    }

    /// <summary>Re-reads <see cref="CanExecute"/> on every bound target.</summary>
    public void Refresh() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
