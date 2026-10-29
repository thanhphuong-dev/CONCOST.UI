namespace CONCOST.UI.Core.UndoRedo;

public interface IUndoableCommand
{
    string Name { get; }
    void Execute();
    void Undo();
}

public class ChangeValueCommand<T> : IUndoableCommand
{
    private readonly Action<T> _setter;
    private readonly T _oldValue;
    private readonly T _newValue;
    public string Name { get; }

    public ChangeValueCommand(string name, Action<T> setter, T oldValue, T newValue)
    {
        Name = name;
        _setter = setter;
        _oldValue = oldValue;
        _newValue = newValue;
    }

    public void Execute() => _setter(_newValue);
    public void Undo() => _setter(_oldValue);
}

public class UndoRedoManager : MVVM.ObservableObject
{
    private static readonly Lazy<UndoRedoManager> _current = new(() => new UndoRedoManager());
    public static UndoRedoManager Current => _current.Value;

    private readonly Stack<IUndoableCommand> _undoStack = new();
    private readonly Stack<IUndoableCommand> _redoStack = new();
    private int _maxCapacity = 100;

    public int MaxCapacity
    {
        get => _maxCapacity;
        set => SetProperty(ref _maxCapacity, Math.Max(1, value));
    }

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    public void Execute(IUndoableCommand command)
    {
        if (command == null) return;
        command.Execute();

        _undoStack.Push(command);
        _redoStack.Clear();

        if (_undoStack.Count > MaxCapacity)
        {
            var list = _undoStack.ToList();
            list.RemoveAt(list.Count - 1);
            _undoStack.Clear();
            for (int i = list.Count - 1; i >= 0; i--)
            {
                _undoStack.Push(list[i]);
            }
        }

        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(CanRedo));
    }

    public void Undo()
    {
        if (!CanUndo) return;
        var cmd = _undoStack.Pop();
        cmd.Undo();
        _redoStack.Push(cmd);

        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(CanRedo));
    }

    public void Redo()
    {
        if (!CanRedo) return;
        var cmd = _redoStack.Pop();
        cmd.Execute();
        _undoStack.Push(cmd);

        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(CanRedo));
    }

    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(CanRedo));
    }
}
