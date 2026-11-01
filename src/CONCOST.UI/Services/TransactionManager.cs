using System;
using System.Collections.Generic;
using CONCOST.UI.Core.UndoRedo;

namespace CONCOST.UI.Services;

/// <summary>Carries the name and size of a transaction that has been committed or rolled back.</summary>
public class TransactionEventArgs : EventArgs
{
    public TransactionEventArgs(string name, int changeCount)
    {
        Name = name;
        ChangeCount = changeCount;
    }

    /// <summary>Name shown in the undo menu, e.g. <c>Change parameter</c>.</summary>
    public string Name { get; }

    /// <summary>How many individual edits the transaction grouped.</summary>
    public int ChangeCount { get; }
}

/// <summary>Several edits that undo as one.</summary>
internal sealed class CompositeCommand : IUndoableCommand
{
    private readonly List<IUndoableCommand> _commands = new();

    public CompositeCommand(string name) => Name = name;

    public string Name { get; }

    public int Count => _commands.Count;

    public void Add(IUndoableCommand command) => _commands.Add(command);

    public void Execute()
    {
        foreach (var command in _commands) command.Execute();
    }

    public void Undo()
    {
        // Reverse order: the last edit may depend on the state the earlier ones left behind.
        for (int i = _commands.Count - 1; i >= 0; i--) _commands[i].Undo();
    }
}

/// <summary>
/// A group of edits that reaches the undo stack as a single step.
/// </summary>
/// <remarks>
/// Each edit is applied the moment it is recorded, so the viewport and the properties palette update
/// while the user is still working, and rolled back by replaying the recorded inverses. Only the
/// composite goes onto the undo stack, which is what makes "change parameter" one Ctrl+Z and not one
/// per keystroke.
/// </remarks>
public sealed class Transaction : IDisposable
{
    private readonly TransactionManager _owner;
    private readonly CompositeCommand _composite;
    private readonly List<IUndoableCommand> _applied = new();

    private bool _completed;

    internal Transaction(TransactionManager owner, string name, UndoRedoManager undo)
    {
        _owner = owner;
        _composite = new CompositeCommand(name);
        Undo = undo;
    }

    private UndoRedoManager Undo { get; }

    /// <summary>Name that will appear on the undo stack.</summary>
    public string Name => _composite.Name;

    /// <summary>How many edits were recorded.</summary>
    public int ChangeCount => _composite.Count;

    /// <summary>Whether the transaction was committed or rolled back.</summary>
    public bool IsCompleted => _completed;

    /// <summary>Applies an edit now and records its inverse for undo and rollback.</summary>
    public void Record(string name, Action apply, Action revert)
    {
        if (_completed) throw new InvalidOperationException("The transaction has already been completed.");
        if (apply is null || revert is null) return;

        var pair = new DelegateCommand(name, apply, revert);
        pair.Execute();

        _applied.Add(pair);
        _composite.Add(pair);
    }

    /// <summary>Records an already executed command, so an edit made elsewhere joins this transaction.</summary>
    public void Record(IUndoableCommand command)
    {
        if (_completed || command is null) return;

        _applied.Add(command);
        _composite.Add(command);
    }

    /// <summary>Applies nothing further and puts the whole group on the undo stack as one step.</summary>
    public void Commit()
    {
        if (_completed) return;

        _completed = true;
        _owner.Complete(this, committed: true);

        if (_composite.Count == 0) return;

        // The edits already ran; pushing them onto the stack is what makes Ctrl+Z work.
        Undo.Record(_composite);
    }

    /// <summary>Replays the inverses in reverse order and leaves the undo stack alone.</summary>
    public void Rollback()
    {
        if (_completed) return;

        _completed = true;

        for (int i = _applied.Count - 1; i >= 0; i--) _applied[i].Undo();

        _owner.Complete(this, committed: false);
    }

    /// <summary>Commits unless the caller already did something else with it.</summary>
    public void Dispose() => Commit();

    private sealed class DelegateCommand : IUndoableCommand
    {
        private readonly Action _apply;
        private readonly Action _revert;

        public DelegateCommand(string name, Action apply, Action revert)
        {
            Name = name;
            _apply = apply;
            _revert = revert;
        }

        public string Name { get; }

        public void Execute() => _apply();

        public void Undo() => _revert();
    }
}

/// <summary>
/// Opens transactions and hands the finished group to the undo stack.
/// </summary>
/// <remarks>
/// One transaction at a time, deliberately: a nested group would either have to merge into its parent
/// — making the inner rollback semantics murky — or become a second undo step, which is what the
/// caller was trying to avoid. Code that needs both opens two transactions in sequence.
/// </remarks>
public class TransactionManager
{
    private static readonly Lazy<TransactionManager> Shared = new(() => new TransactionManager());

    private readonly UndoRedoManager _undo;

    /// <summary>Creates a manager over its own undo stack, or over the shared one.</summary>
    public TransactionManager(UndoRedoManager? undo = null) => _undo = undo ?? UndoRedoManager.Current;

    /// <summary>The instance a host shares between its commands.</summary>
    public static TransactionManager Current => Shared.Value;

    /// <summary>Raised after a transaction commits.</summary>
    public event EventHandler<TransactionEventArgs>? Committed;

    /// <summary>Raised after a transaction rolls back.</summary>
    public event EventHandler<TransactionEventArgs>? RolledBack;

    /// <summary>Raised after either outcome, so a view can refresh once instead of per edit.</summary>
    public event EventHandler? Changed;

    /// <summary>The transaction that is currently open, if any.</summary>
    public Transaction? Active { get; private set; }

    /// <summary>Whether a transaction is open.</summary>
    public bool IsInTransaction => Active is not null;

    /// <summary>Opens a transaction. Dispose it or call <see cref="Transaction.Commit"/>.</summary>
    public Transaction Begin(string name = "Change")
    {
        if (Active is not null)
        {
            throw new InvalidOperationException(
                $"Transaction '{Active.Name}' is still open; commit or roll it back before starting '{name}'.");
        }

        Active = new Transaction(this, string.IsNullOrWhiteSpace(name) ? "Change" : name, _undo);

        return Active;
    }

    /// <summary>Runs <paramref name="edit"/> inside a transaction and commits it.</summary>
    public void Run(string name, Action<Transaction> edit)
    {
        if (edit is null) return;

        using var transaction = Begin(name);

        edit(transaction);
    }

    internal void Complete(Transaction transaction, bool committed)
    {
        if (ReferenceEquals(Active, transaction)) Active = null;

        var args = new TransactionEventArgs(transaction.Name, transaction.ChangeCount);

        if (committed) Committed?.Invoke(this, args);
        else RolledBack?.Invoke(this, args);

        Changed?.Invoke(this, EventArgs.Empty);
    }
}
