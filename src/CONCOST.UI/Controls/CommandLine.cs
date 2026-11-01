using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CONCOST.UI.AttachedProperties;

namespace CONCOST.UI.Controls;

/// <summary>How a command line entry reads.</summary>
public enum CommandMessageKind
{
    /// <summary>What the user typed.</summary>
    Input,

    /// <summary>The answer a command gave.</summary>
    Output,

    /// <summary>A command that failed or a value that did not parse.</summary>
    Error,

    /// <summary>A prompt, a hint or a cancellation.</summary>
    Hint
}

/// <summary>How a <see cref="CommandLine"/> is dressed.</summary>
public enum CommandLineVariant
{
    /// <summary>No surface at all: the card or the panel it is put in is the surface.</summary>
    Inline,

    /// <summary>A strip across the bottom of the window, separated by one rule along its top edge.</summary>
    Docked,

    /// <summary>A rounded plate with a shadow, for a command line the user has floated off the frame.</summary>
    Floating
}

/// <summary>One line in the command history.</summary>
public class CommandMessage
{
    public CommandMessage(string text, CommandMessageKind kind = CommandMessageKind.Output)
    {
        Text = text ?? string.Empty;
        Kind = kind;
        Timestamp = DateTime.Now;
    }

    /// <summary>The line itself.</summary>
    public string Text { get; private set; }

    /// <summary>How the line is coloured.</summary>
    public CommandMessageKind Kind { get; }

    /// <summary>When the line was written.</summary>
    public DateTime Timestamp { get; }

    /// <summary>
    /// Appends to the line. A prompt line grows its answer this way, so the transcript reads as the
    /// exchange it was — <c>Specify first point: 0,0</c> — instead of the answer sitting alone.
    /// </summary>
    internal void Append(string text) => Text += text;
}

/// <summary>
/// A command the line can run: a name, optional aliases, and an optional follow-up prompt for the
/// argument the command still needs.
/// </summary>
public class CommandDefinition
{
    public CommandDefinition(string name, string? description = null)
    {
        Name = name ?? string.Empty;
        Description = description;
    }

    /// <summary>The primary name, e.g. <c>LINE</c>.</summary>
    public string Name { get; }

    /// <summary>Other names that resolve to this command, e.g. <c>L</c>.</summary>
    public List<string> Aliases { get; } = new();

    /// <summary>One line shown next to the name in the suggestion list.</summary>
    public string? Description { get; set; }

    /// <summary>
    /// What the command still needs, e.g. "Specify first point". When this is set the line switches to
    /// prompt mode after the command runs and reports the answer through
    /// <see cref="CommandLine.PromptCompleted"/> instead of parsing another command.
    /// </summary>
    public string? Prompt { get; set; }

    /// <summary>
    /// Keywords the prompt accepts, e.g. <c>Undo</c> and <c>Close</c>. They are drawn as
    /// <c>[Undo/Close]</c> after the prompt the way a CAD prompt offers its options, and the capital
    /// letter of a keyword is enough to pick it.
    /// </summary>
    public List<string> Options { get; } = new();

    /// <summary>Adds prompt keywords and returns the definition, so registration stays one expression.</summary>
    public CommandDefinition WithOptions(params string[] options)
    {
        foreach (var option in options)
        {
            if (!string.IsNullOrWhiteSpace(option)) Options.Add(option);
        }

        return this;
    }

    /// <summary>The prompt with its keyword list, e.g. <c>Specify next point or [Undo/Close]</c>.</summary>
    public string PromptText =>
        Options.Count == 0 ? Prompt ?? string.Empty : $"{Prompt} or [{string.Join("/", Options)}]";

    /// <summary>
    /// Resolves what was typed to one of <see cref="Options"/>: the whole keyword, or the capitals a
    /// CAD prompt asks for — <c>U</c> picks <c>Undo</c>, <c>C</c> picks <c>Close</c>.
    /// </summary>
    public string? MatchOption(string? text) => CommandLine.MatchOption(Options, text);

    /// <summary>Anything the host wants to hang off the definition.</summary>
    public object? Tag { get; set; }

    /// <summary>The name and every alias.</summary>
    public IEnumerable<string> Keys
    {
        get
        {
            yield return Name;

            foreach (var alias in Aliases) yield return alias;
        }
    }

    /// <summary>Adds aliases and returns the definition, so registration reads as one expression.</summary>
    public CommandDefinition WithAliases(params string[] aliases)
    {
        Aliases.AddRange(aliases);

        return this;
    }

    /// <summary>True when any name or alias starts with <paramref name="text"/>.</summary>
    public bool Matches(string text) =>
        Keys.Any(key => key.StartsWith(text, StringComparison.OrdinalIgnoreCase));

    /// <summary>True when any name or alias is exactly <paramref name="text"/>.</summary>
    public bool MatchesKey(string text) =>
        Keys.Any(key => string.Equals(key, text, StringComparison.OrdinalIgnoreCase));

    /// <inheritdoc />
    public override string ToString() => Name;

    /// <summary>
    /// What the suggestion list shows: the name, its aliases so a two-letter user is not left
    /// guessing, then the description.
    /// </summary>
    public string Display
    {
        get
        {
            string head = Aliases.Count == 0 ? Name : $"{Name} ({string.Join(", ", Aliases)})";

            return string.IsNullOrWhiteSpace(Description) ? head : head + "   " + Description;
        }
    }
}

/// <summary>Carries the command that ran and the arguments typed after it.</summary>
public class CommandInvokedEventArgs : EventArgs
{
    public CommandInvokedEventArgs(CommandDefinition definition, string rawText, IReadOnlyList<string> arguments)
    {
        Definition = definition;
        RawText = rawText;
        Arguments = arguments;
    }

    /// <summary>The command that resolved from what was typed.</summary>
    public CommandDefinition Definition { get; }

    /// <summary>The whole line, as typed.</summary>
    public string RawText { get; }

    /// <summary>Everything after the command name, split on whitespace.</summary>
    public IReadOnlyList<string> Arguments { get; }
}

/// <summary>Carries the answer given to a command prompt.</summary>
public class CommandPromptEventArgs : EventArgs
{
    public CommandPromptEventArgs(string prompt, string text, string? option = null)
    {
        Prompt = prompt;
        Text = text;
        Option = option;
    }

    /// <summary>The prompt that was answered.</summary>
    public string Prompt { get; }

    /// <summary>What the user typed, trimmed.</summary>
    public string Text { get; }

    /// <summary>
    /// The keyword the answer picked, or <c>null</c> when it was a value. Typing <c>U</c> at a
    /// <c>[Undo/Close]</c> prompt reports <c>Undo</c>, so the host branches on the keyword rather
    /// than on a spelling.
    /// </summary>
    public string? Option { get; }
}

/// <summary>
/// Parses the values typed into a command line: lengths with units, coordinates and angles.
/// </summary>
/// <remarks>
/// A bare number is a length in millimetres, the unit the status bar reports; a suffix converts, and
/// <c>3'-6"</c> is read as feet and inches because that is what a drawing in imperial units carries.
/// </remarks>
public static class CadInputParser
{
    private static readonly (string Suffix, double Millimetres)[] Units =
    {
        ("mm", 1.0),
        ("cm", 10.0),
        ("m", 1000.0),
        ("in", 25.4),
        ("ft", 304.8)
    };

    /// <summary>Reads a length and returns it in millimetres.</summary>
    public static bool TryParseLength(string? text, out double millimetres)
    {
        millimetres = 0;

        if (text is null) return false;

        string value = text.Trim();
        if (value.Length == 0) return false;

        int apostrophe = value.IndexOf('\'');

        if (apostrophe > 0)
        {
            // 3'-6" — whole feet before the mark, inches after it, and the inch mark is optional.
            string rest = value[(apostrophe + 1)..].Trim().TrimStart('-').Trim().TrimEnd('"');

            if (!TryNumber(value[..apostrophe], out double feet)) return false;

            double inches = 0;

            if (rest.Length > 0 && !TryNumber(rest, out inches)) return false;

            millimetres = feet * 304.8 + inches * 25.4;

            return true;
        }

        foreach (var (suffix, scale) in Units)
        {
            if (!value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) continue;

            if (!TryNumber(value[..^suffix.Length], out double magnitude)) return false;

            millimetres = magnitude * scale;

            return true;
        }

        if (!TryNumber(value.TrimEnd('"'), out double plain)) return false;

        millimetres = plain;

        return true;
    }

    /// <summary>Reads <c>x,y</c> or <c>x,y,z</c>, with any unit suffix a length accepts.</summary>
    public static bool TryParseCoordinate(string? text, out Point3D point)
    {
        point = default;

        if (text is null) return false;

        var parts = text.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Trim())
            .Where(part => part.Length > 0)
            .ToArray();

        if (parts.Length < 2 || parts.Length > 3) return false;

        if (!TryParseLength(parts[0], out double x) || !TryParseLength(parts[1], out double y)) return false;

        double z = 0;

        if (parts.Length == 3 && !TryParseLength(parts[2], out z)) return false;

        point = new Point3D(x, y, z);

        return true;
    }

    /// <summary>Reads an angle in degrees; a trailing <c>deg</c> or a degree sign is allowed.</summary>
    public static bool TryParseAngle(string? text, out double degrees)
    {
        degrees = 0;

        if (text is null) return false;

        string value = text.Trim().TrimEnd('\u00B0');

        if (value.EndsWith("deg", StringComparison.OrdinalIgnoreCase)) value = value[..^3].Trim();

        return value.Length > 0 && TryNumber(value, out degrees);
    }

    private static bool TryNumber(string text, out double value) =>
        double.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.CurrentCulture, out value) ||
        double.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
}

/// <summary>
/// The command line: a scrollable history of what was typed and what came back, an input box with
/// command-name suggestions, history recall, and a prompt mode for the value a command still needs.
/// </summary>
/// <remarks>
/// <para>
/// Prompt mode is the piece that makes a command line usable beyond one word: a command that needs a
/// point or a distance sets <see cref="CommandDefinition.Prompt"/>, and the line then treats the next
/// entry as an answer — the same exchange a dynamic input box shows near the cursor — instead of
/// trying to parse it as another command.
/// </para>
/// <para>
/// Enter on an empty line repeats the last command, which is what makes a repetitive command a single
/// keystroke. Up and Down walk the typed history; Tab completes the command name.
/// </para>
/// </remarks>
public class CommandLine : Grid
{
    /// <summary>Identifies the <see cref="Prompt"/> property.</summary>
    public static readonly DependencyProperty PromptProperty = DependencyProperty.Register(
        nameof(Prompt), typeof(string), typeof(CommandLine),
        new FrameworkPropertyMetadata("Command", OnPromptChanged));

    /// <summary>Identifies the <see cref="HistoryHeight"/> property.</summary>
    public static readonly DependencyProperty HistoryHeightProperty = DependencyProperty.Register(
        nameof(HistoryHeight), typeof(double), typeof(CommandLine),
        new FrameworkPropertyMetadata(120.0, OnHistoryHeightChanged));

    /// <summary>Identifies the <see cref="ShowHistory"/> property.</summary>
    public static readonly DependencyProperty ShowHistoryProperty = DependencyProperty.Register(
        nameof(ShowHistory), typeof(bool), typeof(CommandLine),
        new FrameworkPropertyMetadata(true, OnShowHistoryChanged));

    /// <summary>Identifies the <see cref="MaxHistory"/> property.</summary>
    public static readonly DependencyProperty MaxHistoryProperty = DependencyProperty.Register(
        nameof(MaxHistory), typeof(int), typeof(CommandLine), new FrameworkPropertyMetadata(200));

    /// <summary>Identifies the <see cref="Variant"/> property.</summary>
    public static readonly DependencyProperty VariantProperty = DependencyProperty.Register(
        nameof(Variant), typeof(CommandLineVariant), typeof(CommandLine),
        new FrameworkPropertyMetadata(CommandLineVariant.Inline, OnChromeChanged));

    /// <summary>Identifies the <see cref="CornerRadius"/> property.</summary>
    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        nameof(CornerRadius), typeof(CornerRadius), typeof(CommandLine),
        new FrameworkPropertyMetadata(new CornerRadius(6), OnChromeChanged));

    /// <summary>Identifies the <see cref="Elevation"/> property.</summary>
    public static readonly DependencyProperty ElevationProperty = DependencyProperty.Register(
        nameof(Elevation), typeof(int), typeof(CommandLine),
        new FrameworkPropertyMetadata(2, OnChromeChanged));

    /// <summary>Identifies the <see cref="EchoPrompt"/> property.</summary>
    public static readonly DependencyProperty EchoPromptProperty = DependencyProperty.Register(
        nameof(EchoPrompt), typeof(bool), typeof(CommandLine), new FrameworkPropertyMetadata(true));

    /// <summary>Identifies the <see cref="SpaceSubmits"/> property.</summary>
    public static readonly DependencyProperty SpaceSubmitsProperty = DependencyProperty.Register(
        nameof(SpaceSubmits), typeof(bool), typeof(CommandLine), new FrameworkPropertyMetadata(true));

    /// <summary>Identifies the <see cref="IsHistoryExpanded"/> property.</summary>
    public static readonly DependencyProperty IsHistoryExpandedProperty = DependencyProperty.Register(
        nameof(IsHistoryExpanded), typeof(bool), typeof(CommandLine),
        new FrameworkPropertyMetadata(false, OnHistorySizeChanged));

    /// <summary>Identifies the <see cref="ExpandedHistoryHeight"/> property.</summary>
    public static readonly DependencyProperty ExpandedHistoryHeightProperty = DependencyProperty.Register(
        nameof(ExpandedHistoryHeight), typeof(double), typeof(CommandLine),
        new FrameworkPropertyMetadata(320.0, OnHistorySizeChanged));

    private static readonly FontFamily MonoFont = new("Consolas");

    private readonly Border _frame;
    private readonly ScrollViewer _feedScroll;
    private readonly StackPanel _feed;
    private readonly TextBlock _prompt;
    private readonly TextBox _input;
    private readonly Popup _suggestionPopup;
    private readonly ListBox _suggestions;
    private readonly List<string> _entered = new();
    private readonly List<string> _promptOptions = new();
    private readonly List<CommandDefinition> _currentSuggestions = new();

    private TextBlock? _promptLine;
    private CommandMessage? _promptMessage;
    private bool _promptActive;
    private int _historyIndex = -1;

    public CommandLine()
    {
        // The two rows live one level in, inside the frame: the variant draws the surface, and a
        // border cannot be its own parent's child.
        var inner = new Grid();

        inner.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        inner.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        _feed = new StackPanel { Margin = new Thickness(8, 4, 8, 2) };

        _feedScroll = new ScrollViewer
        {
            MaxHeight = HistoryHeight,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = _feed
        };

        SetRow(_feedScroll, 0);
        inner.Children.Add(_feedScroll);

        // ---------------------------------------------------------------- input row
        var inputRow = new Grid { Margin = new Thickness(8, 0, 8, 6) };
        inputRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        inputRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        _prompt = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 6, 0),
            FontFamily = MonoFont,
            FontSize = 12,
            Text = "Command:"
        };

        _prompt.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.Primary");

        SetColumn(_prompt, 0);
        inputRow.Children.Add(_prompt);

        _input = new TextBox { Height = 26, FontFamily = MonoFont, FontSize = 12 };

        // A boxed input turns the bottom of the window into a form field. A command line is a line:
        // the rule under it says where to type, and nothing else is drawn around the text.
        _input.SetResourceReference(StyleProperty, "CONCOST.TextBox.Underline");
        _input.PreviewKeyDown += OnInputKeyDown;
        _input.TextChanged += (_, _) => RefreshSuggestions();
        _input.LostKeyboardFocus += (_, _) => HideSuggestions();

        SetColumn(_input, 1);
        inputRow.Children.Add(_input);

        SetRow(inputRow, 1);
        inner.Children.Add(inputRow);

        // The line paints nothing of its own by default: the host docks it on the window's backdrop.
        // The docked and floating variants do get a surface, because there the bar *is* the surface.
        _frame = new Border { Child = inner };

        Children.Add(_frame);

        // ---------------------------------------------------------------- suggestions
        _suggestions = new ListBox
        {
            FontFamily = MonoFont,
            FontSize = 12,
            MaxHeight = 168,
            BorderThickness = new Thickness(0),
            DisplayMemberPath = nameof(CommandDefinition.Display)
        };

        _suggestions.MouseLeftButtonUp += (_, _) => Complete();

        var frame = new Border
        {
            Width = 260,
            Padding = new Thickness(2),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Child = _suggestions
        };

        frame.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
        frame.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

        _suggestionPopup = new Popup
        {
            PlacementTarget = _input,
            Placement = PlacementMode.Top,
            StaysOpen = false,
            Child = frame,
            AllowsTransparency = true
        };

        _prompt.Focusable = false;

        ApplyChrome();
    }

    /// <summary>The commands the line can run.</summary>
    public ObservableCollection<CommandDefinition> Commands { get; } = new();

    /// <summary>The history, oldest first, limited by <see cref="MaxHistory"/>.</summary>
    public ObservableCollection<CommandMessage> Messages { get; } = new();

    /// <summary>The label in front of the input; a command prompt replaces it while prompt mode is on.</summary>
    public string Prompt
    {
        get => (string)GetValue(PromptProperty);
        set => SetValue(PromptProperty, value);
    }

    /// <summary>How tall the history scrolls before it scrolls.</summary>
    public double HistoryHeight
    {
        get => (double)GetValue(HistoryHeightProperty);
        set => SetValue(HistoryHeightProperty, value);
    }

    /// <summary>Hides the history, for a command line that is only ever one line tall.</summary>
    public bool ShowHistory
    {
        get => (bool)GetValue(ShowHistoryProperty);
        set => SetValue(ShowHistoryProperty, value);
    }

    /// <summary>How many lines are kept before the oldest is dropped.</summary>
    public int MaxHistory
    {
        get => (int)GetValue(MaxHistoryProperty);
        set => SetValue(MaxHistoryProperty, value);
    }

    /// <summary>
    /// How the line is dressed: <see cref="CommandLineVariant.Inline"/> inside a card,
    /// <see cref="CommandLineVariant.Docked"/> as the strip along the bottom of the window, or
    /// <see cref="CommandLineVariant.Floating"/> as a plate the user has floated off the frame.
    /// </summary>
    public CommandLineVariant Variant
    {
        get => (CommandLineVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>Corner radius of the floating surface.</summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>Shadow depth of the floating surface, 0 to 4.</summary>
    public int Elevation
    {
        get => (int)GetValue(ElevationProperty);
        set => SetValue(ElevationProperty, value);
    }

    /// <summary>
    /// Writes the prompt into the transcript and puts the answer on the same line, which is what a
    /// CAD command line shows: the dialogue, not a log of results with the question hidden in a
    /// label. Turn it off for a read-only log.
    /// </summary>
    public bool EchoPrompt
    {
        get => (bool)GetValue(EchoPromptProperty);
        set => SetValue(EchoPromptProperty, value);
    }

    /// <summary>
    /// Space submits the line, the way a CAD command line does. Set it to <c>false</c> for a prompt
    /// whose answer can contain a space — a room name, a note.
    /// </summary>
    public bool SpaceSubmits
    {
        get => (bool)GetValue(SpaceSubmitsProperty);
        set => SetValue(SpaceSubmitsProperty, value);
    }

    /// <summary>The full history, the window F2 raises.</summary>
    public bool IsHistoryExpanded
    {
        get => (bool)GetValue(IsHistoryExpandedProperty);
        set => SetValue(IsHistoryExpandedProperty, value);
    }

    /// <summary>How tall the history grows while <see cref="IsHistoryExpanded"/> is on.</summary>
    public double ExpandedHistoryHeight
    {
        get => (double)GetValue(ExpandedHistoryHeightProperty);
        set => SetValue(ExpandedHistoryHeightProperty, value);
    }

    /// <summary>The commands the suggestion list is offering, best first.</summary>
    public IReadOnlyList<CommandDefinition> Suggestions => _currentSuggestions;

    /// <summary>The prompt as it is drawn, with its keyword list and the colon.</summary>
    public string PromptText => _prompt.Text;

    /// <summary>Raises or folds the full history, which is what F2 does.</summary>
    public void ToggleHistory() => IsHistoryExpanded = !IsHistoryExpanded;

    /// <summary>The text in the input box.</summary>
    public string Text
    {
        get => _input.Text ?? string.Empty;
        set => _input.Text = value ?? string.Empty;
    }

    /// <summary>True while the line is waiting for the answer to a command prompt.</summary>
    public bool IsPromptActive => _promptActive;

    /// <summary>The command that ran last, which an empty Enter repeats.</summary>
    public CommandDefinition? LastCommand { get; private set; }

    /// <summary>Raised when a typed line resolves to a command.</summary>
    public event EventHandler<CommandInvokedEventArgs>? CommandInvoked;

    /// <summary>Raised with the text a command typed that resolved to nothing.</summary>
    public event EventHandler<string>? UnknownCommand;

    /// <summary>Raised when a command prompt is answered.</summary>
    public event EventHandler<CommandPromptEventArgs>? PromptCompleted;

    /// <summary>Raised when a command prompt is cancelled with Escape.</summary>
    public event EventHandler? PromptCancelled;

    /// <summary>Adds a command.</summary>
    public void AddCommand(CommandDefinition definition)
    {
        if (definition is null) return;

        Commands.Add(definition);
    }

    /// <summary>Adds several commands at once.</summary>
    public void AddCommands(IEnumerable<CommandDefinition> definitions)
    {
        foreach (var definition in definitions) AddCommand(definition);
    }

    /// <summary>
    /// Resolves a typed line to a command: an exact name or alias wins, otherwise the first command
    /// whose name or alias starts with what was typed.
    /// </summary>
    public CommandDefinition? Resolve(string rawText)
    {
        string token = FirstToken(rawText);

        if (token.Length == 0) return null;

        return Commands.FirstOrDefault(command => command.MatchesKey(token))
            ?? Commands.FirstOrDefault(command => command.Matches(token));
    }

    /// <summary>Runs whatever is in the input box.</summary>
    /// <summary>Sets the input text and submits the command immediately.</summary>
    public void Execute(string command)
    {
        Text = command;
        Submit();
    }

    public void Submit()
    {
        string raw = Text.Trim();

        if (_promptActive)
        {
            AnswerPrompt(raw);
            return;
        }

        if (raw.Length == 0)
        {
            // Enter on an empty line repeats the last command, the way a CAD command line does.
            if (LastCommand is null) return;

            raw = LastCommand.Name;
        }

        HideSuggestions();
        WriteLine("> " + raw, CommandMessageKind.Input);

        _entered.Add(raw);
        _historyIndex = -1;
        _input.Clear();

        var definition = Resolve(raw);

        if (definition is null)
        {
            WriteLine($"Unknown command \"{raw}\".", CommandMessageKind.Error);
            UnknownCommand?.Invoke(this, raw);

            return;
        }

        LastCommand = definition;

        var arguments = raw.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
            .Skip(1)
            .ToArray();

        CommandInvoked?.Invoke(this, new CommandInvokedEventArgs(definition, raw, arguments));

        if (!string.IsNullOrEmpty(definition.Prompt)) BeginPrompt(definition.Prompt!, definition.Options);
    }

    /// <summary>Switches the line into prompt mode, e.g. to ask for a point.</summary>
    public void BeginPrompt(string prompt) => BeginPrompt(prompt, null);

    /// <summary>
    /// Switches the line into prompt mode with the keywords the prompt accepts, and echoes the prompt
    /// into the transcript so the answer has a question above it.
    /// </summary>
    public void BeginPrompt(string prompt, IEnumerable<string>? options)
    {
        _promptActive = true;

        _promptOptions.Clear();

        if (options is not null)
        {
            foreach (var option in options)
            {
                if (!string.IsNullOrWhiteSpace(option)) _promptOptions.Add(option);
            }
        }

        Prompt = prompt;

        RefreshPromptLabel();
        HideSuggestions();

        if (EchoPrompt)
        {
            _promptLine = WriteLine(_prompt.Text, CommandMessageKind.Hint);
            _promptMessage = Messages.Count > 0 ? Messages[Messages.Count - 1] : null;
        }

        _input.Focus();
    }

    /// <summary>Leaves prompt mode without an answer.</summary>
    public void CancelPrompt()
    {
        if (!_promptActive) return;

        _promptActive = false;

        _promptOptions.Clear();
        _promptLine = null;
        _promptMessage = null;

        Prompt = "Command";

        RefreshPromptLabel();

        _input.Clear();

        WriteLine("*Cancel*", CommandMessageKind.Hint);

        PromptCancelled?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Completes the typed command name from the first suggestion.</summary>
    public void Complete()
    {
        if (_suggestions.Items.Count == 0) return;

        if (_suggestions.Items[0] is not CommandDefinition definition) return;

        Text = definition.Name + " ";

        _input.CaretIndex = Text.Length;

        HideSuggestions();
    }

    /// <summary>Appends a line to the history and returns the row it created.</summary>
    public TextBlock WriteLine(string text, CommandMessageKind kind = CommandMessageKind.Output)
    {
        var message = new CommandMessage(text, kind);
        var row = CreateLine(message);

        Messages.Add(message);
        _feed.Children.Add(row);

        int limit = Math.Max(1, MaxHistory);

        while (_feed.Children.Count > limit)
        {
            _feed.Children.RemoveAt(0);

            if (Messages.Count > 0) Messages.RemoveAt(0);
        }

        _feedScroll.ScrollToEnd();

        return row;
    }

    /// <summary>Clears the visible history.</summary>
    public void Clear()
    {
        Messages.Clear();
        _feed.Children.Clear();

        _promptLine = null;
        _promptMessage = null;
    }

    /// <summary>Gives the input box focus, e.g. when the window opens.</summary>
    public void FocusInput() => _input.Focus();

    private void AnswerPrompt(string raw)
    {
        string prompt = Prompt;
        string? option = MatchOption(_promptOptions, raw);

        _promptActive = false;

        _promptOptions.Clear();

        Prompt = "Command";

        RefreshPromptLabel();

        _input.Clear();

        if (_promptLine is null)
        {
            WriteLine(raw, CommandMessageKind.Input);
        }
        else
        {
            // The answer joins the question on the same line — "Specify first point: 0,0" — which is
            // the shape a CAD transcript has; an answer on a line of its own reads as an orphan.
            // An empty answer is Enter ending the command, and it leaves the question as it stands.
            if (raw.Length > 0)
            {
                _promptLine.Text += " " + raw;

                _promptMessage?.Append(" " + raw);
            }

            _promptLine.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

            _promptLine = null;
            _promptMessage = null;
        }

        PromptCompleted?.Invoke(this, new CommandPromptEventArgs(prompt, raw, option));
    }

    /// <summary>
    /// Resolves what was typed to one of <paramref name="options"/>: the whole keyword, or the
    /// capitals a CAD prompt asks for — <c>U</c> for <c>Undo</c>, <c>C</c> for <c>Close</c> — or a
    /// beginning that fits exactly one keyword. Returns <c>null</c> when the text is a value.
    /// </summary>
    public static string? MatchOption(IEnumerable<string>? options, string? text)
    {
        if (options is null) return null;

        string value = (text ?? string.Empty).Trim();

        if (value.Length == 0) return null;

        var list = options as IList<string> ?? options.ToList();

        var exact = list.Where(option => string.Equals(option, value, StringComparison.OrdinalIgnoreCase)).ToList();

        if (exact.Count == 1) return exact[0];

        // The capitals are the shortcut a CAD prompt advertises. Two keywords sharing one is a
        // malformed list, and a guess would run the wrong command, so the answer falls through.
        var byCapital = list.Where(option =>
        {
            var capitals = new string(option.Where(char.IsUpper).ToArray());

            return capitals.Length > 0 && string.Equals(capitals, value, StringComparison.OrdinalIgnoreCase);
        }).ToList();

        if (byCapital.Count == 1) return byCapital[0];

        var candidates = list.Where(option => option.StartsWith(value, StringComparison.OrdinalIgnoreCase)).ToList();

        // A beginning that fits more than one keyword is not an answer; the prompt asks again.
        return candidates.Count == 1 ? candidates[0] : null;
    }

    private void OnInputKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                Submit();
                e.Handled = true;
                break;

            case Key.Escape:
                if (_promptActive) CancelPrompt();
                else
                {
                    _input.Clear();
                    HideSuggestions();
                }

                e.Handled = true;
                break;

            case Key.Tab:
                Complete();
                e.Handled = true;
                break;

            case Key.Up:
                MoveSuggestion(-1);
                e.Handled = true;
                break;

            case Key.Down:
                MoveSuggestion(1);
                e.Handled = true;
                break;

            case Key.Space:
                // Space is Enter on a CAD command line, so a command is one word and one keystroke.
                // Only at the end of the text: editing in the middle of a prompt has to keep the space.
                if (SpaceSubmits && _input.CaretIndex == _input.Text.Length &&
                    (_promptActive || Text.Trim().Length > 0 || LastCommand is not null))
                {
                    Submit();
                    e.Handled = true;
                }

                break;

            case Key.F2:
                ToggleHistory();
                e.Handled = true;
                break;
        }
    }

    private void MoveSuggestion(int delta)
    {
        if (_suggestionPopup.IsOpen && _suggestions.Items.Count > 0)
        {
            int index = _suggestions.SelectedIndex + delta;

            _suggestions.SelectedIndex = Math.Max(0, Math.Min(_suggestions.Items.Count - 1, index));

            return;
        }

        if (_promptActive || _entered.Count == 0) return;

        // Up walks back through what was typed; Down walks forward and off the end clears the box.
        _historyIndex = Math.Max(-1, Math.Min(_entered.Count - 1, _historyIndex + delta));
        _input.Text = _historyIndex < 0 ? string.Empty : _entered[_entered.Count - 1 - _historyIndex];
        _input.CaretIndex = _input.Text.Length;
    }

    private void RefreshSuggestions()
    {
        if (_promptActive)
        {
            HideSuggestions();
            return;
        }

        string text = Text;
        string token = FirstToken(text);

        // Only the command name is completed — once a space is typed the user is on the arguments.
        if (token.Length == 0 || text.Contains(' ') || text.Contains('\t'))
        {
            HideSuggestions();
            return;
        }

        // An exact name or alias first, then the names that start with what was typed, then the
        // aliases: typing L should offer LINE before a command whose alias merely begins with L.
        var matches = Commands
            .Where(command => command.Matches(token))
            .OrderBy(command => command.MatchesKey(token) ? 0 : 1)
            .ThenBy(command => command.Name.StartsWith(token, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(command => command.Name, StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .ToList();

        _currentSuggestions.Clear();

        if (matches.Count == 0)
        {
            HideSuggestions();
            return;
        }

        _currentSuggestions.AddRange(matches);
        _suggestions.ItemsSource = matches;
        _suggestions.SelectedIndex = 0;

        _suggestionPopup.IsOpen = true;
    }

    private void HideSuggestions()
    {
        _suggestionPopup.IsOpen = false;
        _currentSuggestions.Clear();
    }

    private void RefreshPromptLabel()
    {
        string prompt = string.IsNullOrWhiteSpace(Prompt) ? "Command" : Prompt;
        string options = _promptOptions.Count == 0 ? string.Empty : $" or [{string.Join("/", _promptOptions)}]";

        _prompt.Text = prompt + options + ":";
    }

    private void ApplyChrome()
    {
        _frame.CornerRadius = new CornerRadius(0);
        _frame.BorderThickness = new Thickness(0);
        _frame.Opacity = 1;

        VisualElement.SetElevation(_frame, 0);

        if (Variant == CommandLineVariant.Inline)
        {
            _frame.SetValue(Border.BackgroundProperty, Brushes.Transparent);
            _frame.SetValue(Border.BorderBrushProperty, Brushes.Transparent);
            return;
        }

        _frame.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
        _frame.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

        if (Variant == CommandLineVariant.Docked)
        {
            // One rule along the top edge only: the sides and the bottom are the window's own frame
            // in a CAD shell, and drawing them again reads as a box inside a box.
            _frame.BorderThickness = new Thickness(0, 1, 0, 0);
            return;
        }

        _frame.BorderThickness = new Thickness(1);
        _frame.CornerRadius = CornerRadius;
        VisualElement.SetElevation(_frame, Elevation);
    }

    private void ApplyHistoryHeight() =>
        _feedScroll.MaxHeight = IsHistoryExpanded
            ? Math.Max(HistoryHeight, ExpandedHistoryHeight)
            : HistoryHeight;

    private static string FirstToken(string text) =>
        (text ?? string.Empty).Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()
        ?? string.Empty;

    private static TextBlock CreateLine(CommandMessage message)
    {
        var block = new TextBlock
        {
            Text = message.Text,
            TextWrapping = TextWrapping.Wrap,
            FontFamily = MonoFont,
            FontSize = 11,
            Margin = new Thickness(0, 0, 0, 2)
        };

        block.SetResourceReference(TextBlock.ForegroundProperty, BrushFor(message.Kind));

        return block;
    }

    private static string BrushFor(CommandMessageKind kind) => kind switch
    {
        CommandMessageKind.Input => "CONCOST.Brush.TextPrimary",
        CommandMessageKind.Error => "CONCOST.Brush.Danger",
        CommandMessageKind.Hint => "CONCOST.Brush.Primary",
        _ => "CONCOST.Brush.TextSecondary"
    };

    private static void OnPromptChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CommandLine line) line.RefreshPromptLabel();
    }

    private static void OnHistoryHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CommandLine line) line.ApplyHistoryHeight();
    }

    private static void OnHistorySizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CommandLine line) line.ApplyHistoryHeight();
    }

    private static void OnChromeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CommandLine line) line.ApplyChrome();
    }

    private static void OnShowHistoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CommandLine line)
        {
            line._feedScroll.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
