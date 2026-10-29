using System.Windows;
using System.Windows.Input;

namespace CONCOST.UI.Services;

public static class HotkeyManager
{
    private static readonly Dictionary<string, ICommand> _bindings = new();

    public static void Register(string gesture, ICommand command)
    {
        if (string.IsNullOrWhiteSpace(gesture) || command == null) return;
        _bindings[gesture.ToUpperInvariant()] = command;
    }

    public static void Unregister(string gesture)
    {
        if (string.IsNullOrWhiteSpace(gesture)) return;
        _bindings.Remove(gesture.ToUpperInvariant());
    }

    public static void AttachToWindow(Window window)
    {
        window.PreviewKeyDown += (s, e) =>
        {
            var keyStr = e.Key.ToString();
            var mods = Keyboard.Modifiers;

            string combo = "";
            if (mods.HasFlag(ModifierKeys.Control)) combo += "CTRL+";
            if (mods.HasFlag(ModifierKeys.Shift)) combo += "SHIFT+";
            if (mods.HasFlag(ModifierKeys.Alt)) combo += "ALT+";
            combo += keyStr;

            combo = combo.ToUpperInvariant();
            if (_bindings.TryGetValue(combo, out var cmd) && cmd.CanExecute(null))
            {
                cmd.Execute(null);
                e.Handled = true;
            }
        };
    }
}
