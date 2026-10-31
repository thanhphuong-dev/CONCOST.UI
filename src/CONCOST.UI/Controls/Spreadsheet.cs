using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using CONCOST.UI.Core.MVVM;
using CONCOST.UI.Icons;
using WpfCanvas = System.Windows.Controls.Canvas;

namespace CONCOST.UI.Controls;

public class SpreadsheetGridPresenter : FrameworkElement
{
    private Spreadsheet? _owner;

    public SpreadsheetGridPresenter()
    {
        Focusable = true;
        ClipToBounds = true;
        Cursor = Cursors.Arrow;
    }

    public void AttachOwner(Spreadsheet owner)
    {
        _owner = owner;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (_owner?.ActiveSheet != null)
        {
            var sheet = _owner.ActiveSheet;
            double w = _owner.RowHeaderWidth + sheet.GetColumnOffsets()[sheet.ColumnCount];
            double h = _owner.ColumnHeaderHeight + sheet.GetRowOffsets()[sheet.RowCount];

            return new Size(w, h);
        }
        return new Size(1200, 800);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        return finalSize;
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        _owner?.RenderGrid(dc, this);
    }
}

// Snapshot for Undo/Redo
internal sealed class SheetSnapshot
{
    public Dictionary<(int, int), (string raw, string formula, bool bold, bool italic, bool underline, HorizontalAlignment align, SpreadsheetCellFormat fmt)> Cells { get; }
    public int ActiveRow { get; }
    public int ActiveCol { get; }
    public SpreadsheetRange Selection { get; }
    public int RowCount { get; }
    public int ColumnCount { get; }

    public SheetSnapshot(SpreadsheetSheet sheet, int row, int col, SpreadsheetRange sel)
    {
        ActiveRow = row; ActiveCol = col; Selection = sel;
        RowCount = sheet.RowCount;
        ColumnCount = sheet.ColumnCount;
        Cells = new Dictionary<(int, int), (string, string, bool, bool, bool, HorizontalAlignment, SpreadsheetCellFormat)>();
        foreach (var cell in sheet.GetAllCells())
            Cells[(cell.Row, cell.Column)] = (cell.RawValue ?? "", cell.Formula ?? "", cell.IsBold, cell.IsItalic, cell.IsUnderline, cell.HorizontalAlignment, cell.Format);
    }
}

public class Spreadsheet : Control
{
    private readonly SpreadsheetFormulaEngine _formulaEngine = new();
    private ScrollViewer? _scrollViewer;
    private SpreadsheetGridPresenter? _gridPresenter;
    private TextBox? _inPlaceEditor;
    private TextBox? _formulaTextBox;
    private Button? _formulaCommitButton;
    private Button? _formulaCancelButton;
    private TextBlock? _cellAddressTextBlock;
    private TextBlock? _selectionStatsTextBlock;
    private ListBox? _sheetsListBox;

    // Undo/Redo
    private readonly Stack<SheetSnapshot> _undoStack = new();
    private readonly Stack<SheetSnapshot> _redoStack = new();
    public ICommand UndoCommand { get; private set; } = null!;
    public ICommand RedoCommand { get; private set; } = null!;

    private int _hoveredRow = -1;
    private int _hoveredCol = -1;
    private bool _isSelectingHeader;
    private bool _isSelecting;
    private bool _selectionDragStarted;
    private Point _selectionStartPoint;
    private bool _isEditing;
    private bool _isExtendingSelection;
    private int _selectionAnchorRow;
    private int _selectionAnchorColumn;
    private SpreadsheetSheet? _observedSheet;
    private string? _lastCopiedText;
    private int _lastCopyStartRow;
    private int _lastCopyStartColumn;

    public static readonly DependencyProperty ActiveSheetProperty =
        DependencyProperty.Register(nameof(ActiveSheet), typeof(SpreadsheetSheet), typeof(Spreadsheet),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnActiveSheetChanged));

    public static readonly DependencyProperty SheetsProperty =
        DependencyProperty.Register(nameof(Sheets), typeof(ObservableCollection<SpreadsheetSheet>), typeof(Spreadsheet),
            new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty SelectionRangeProperty =
        DependencyProperty.Register(nameof(SelectionRange), typeof(SpreadsheetRange), typeof(Spreadsheet),
            new FrameworkPropertyMetadata(new SpreadsheetRange(0, 0), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectionRangeChanged));

    public static readonly DependencyProperty ActiveRowProperty =
        DependencyProperty.Register(nameof(ActiveRow), typeof(int), typeof(Spreadsheet),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnActiveCellCoordsChanged));

    public static readonly DependencyProperty ActiveColumnProperty =
        DependencyProperty.Register(nameof(ActiveColumn), typeof(int), typeof(Spreadsheet),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnActiveCellCoordsChanged));

    public static readonly DependencyProperty FormulaTextProperty =
        DependencyProperty.Register(nameof(FormulaText), typeof(string), typeof(Spreadsheet),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnFormulaTextChanged));

    public static readonly DependencyProperty ShowToolbarProperty =
        DependencyProperty.Register(nameof(ShowToolbar), typeof(bool), typeof(Spreadsheet), new PropertyMetadata(true));

    public static readonly DependencyProperty ShowFormulaBarProperty =
        DependencyProperty.Register(nameof(ShowFormulaBar), typeof(bool), typeof(Spreadsheet), new PropertyMetadata(true));

    public static readonly DependencyProperty ShowSheetTabsProperty =
        DependencyProperty.Register(nameof(ShowSheetTabs), typeof(bool), typeof(Spreadsheet), new PropertyMetadata(true));

    public static readonly DependencyProperty ShowStatusBarProperty =
        DependencyProperty.Register(nameof(ShowStatusBar), typeof(bool), typeof(Spreadsheet), new PropertyMetadata(true));

    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(Spreadsheet), new PropertyMetadata(false));

    public static readonly DependencyProperty RowHeaderWidthProperty =
        DependencyProperty.Register(nameof(RowHeaderWidth), typeof(double), typeof(Spreadsheet), new PropertyMetadata(45.0));

    public static readonly DependencyProperty ColumnHeaderHeightProperty =
        DependencyProperty.Register(nameof(ColumnHeaderHeight), typeof(double), typeof(Spreadsheet), new PropertyMetadata(26.0));

    public static readonly DependencyProperty GridLineBrushProperty =
        DependencyProperty.Register(nameof(GridLineBrush), typeof(Brush), typeof(Spreadsheet), new PropertyMetadata(null));

    public static readonly DependencyProperty HeaderBackgroundProperty =
        DependencyProperty.Register(nameof(HeaderBackground), typeof(Brush), typeof(Spreadsheet), new PropertyMetadata(null));

    public static readonly DependencyProperty SelectionBrushProperty =
        DependencyProperty.Register(nameof(SelectionBrush), typeof(Brush), typeof(Spreadsheet), new PropertyMetadata(null));

    public static readonly DependencyProperty SelectionBorderBrushProperty =
        DependencyProperty.Register(nameof(SelectionBorderBrush), typeof(Brush), typeof(Spreadsheet), new PropertyMetadata(null));

    public SpreadsheetSheet? ActiveSheet
    {
        get => (SpreadsheetSheet?)GetValue(ActiveSheetProperty);
        set => SetValue(ActiveSheetProperty, value);
    }

    public ObservableCollection<SpreadsheetSheet> Sheets
    {
        get => (ObservableCollection<SpreadsheetSheet>)GetValue(SheetsProperty);
        set => SetValue(SheetsProperty, value);
    }

    public SpreadsheetRange SelectionRange
    {
        get => (SpreadsheetRange)GetValue(SelectionRangeProperty);
        set => SetValue(SelectionRangeProperty, value);
    }

    public int ActiveRow
    {
        get => (int)GetValue(ActiveRowProperty);
        set => SetValue(ActiveRowProperty, value);
    }

    public int ActiveColumn
    {
        get => (int)GetValue(ActiveColumnProperty);
        set => SetValue(ActiveColumnProperty, value);
    }

    public string FormulaText
    {
        get => (string)GetValue(FormulaTextProperty);
        set => SetValue(FormulaTextProperty, value);
    }

    public bool ShowToolbar
    {
        get => (bool)GetValue(ShowToolbarProperty);
        set => SetValue(ShowToolbarProperty, value);
    }

    public bool ShowFormulaBar
    {
        get => (bool)GetValue(ShowFormulaBarProperty);
        set => SetValue(ShowFormulaBarProperty, value);
    }

    public bool ShowSheetTabs
    {
        get => (bool)GetValue(ShowSheetTabsProperty);
        set => SetValue(ShowSheetTabsProperty, value);
    }

    public bool ShowStatusBar
    {
        get => (bool)GetValue(ShowStatusBarProperty);
        set => SetValue(ShowStatusBarProperty, value);
    }

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public double RowHeaderWidth
    {
        get => (double)GetValue(RowHeaderWidthProperty);
        set => SetValue(RowHeaderWidthProperty, value);
    }

    public double ColumnHeaderHeight
    {
        get => (double)GetValue(ColumnHeaderHeightProperty);
        set => SetValue(ColumnHeaderHeightProperty, value);
    }

    public Brush? GridLineBrush
    {
        get => (Brush?)GetValue(GridLineBrushProperty);
        set => SetValue(GridLineBrushProperty, value);
    }

    public Brush? HeaderBackground
    {
        get => (Brush?)GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }

    public Brush? SelectionBrush
    {
        get => (Brush?)GetValue(SelectionBrushProperty);
        set => SetValue(SelectionBrushProperty, value);
    }

    public Brush? SelectionBorderBrush
    {
        get => (Brush?)GetValue(SelectionBorderBrushProperty);
        set => SetValue(SelectionBorderBrushProperty, value);
    }

    public ICommand AddSheetCommand { get; }
    public ICommand DeleteSheetCommand { get; }
    public ICommand ToggleBoldCommand { get; }
    public ICommand ToggleItalicCommand { get; }
    public ICommand ToggleUnderlineCommand { get; }
    public ICommand SetAlignmentCommand { get; }
    public ICommand SetFormatCommand { get; }
    public ICommand SetCellBackgroundCommand { get; }
    public ICommand MergeCellsCommand { get; }
    public ICommand UnmergeCellsCommand { get; }
    public ICommand InsertRowCommand { get; }
    public ICommand DeleteRowCommand { get; }
    public ICommand InsertColumnCommand { get; }
    public ICommand DeleteColumnCommand { get; }
    public ICommand ExportCsvCommand { get; }
    public ICommand ImportCsvCommand { get; }
    public ICommand CopyCommand { get; }
    public ICommand PasteCommand { get; }
    public ICommand CutCommand { get; }
    public ICommand ClearSelectionCommand { get; }
    public ICommand AutoSumCommand { get; }
    public ICommand RecalculateCommand { get; }

    static Spreadsheet()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Spreadsheet), new FrameworkPropertyMetadata(typeof(Spreadsheet)));
    }

    public Spreadsheet()
    {
        Sheets = new ObservableCollection<SpreadsheetSheet>();
        var defaultSheet = new SpreadsheetSheet { Name = "Sheet1" };
        Sheets.Add(defaultSheet);
        ActiveSheet = defaultSheet;

        AddSheetCommand = new RelayCommand(ExecuteAddSheet);
        DeleteSheetCommand = new RelayCommand(ExecuteDeleteSheet, () => Sheets.Count > 1);
        ToggleBoldCommand = new RelayCommand(() => ApplyFormatting(c => c.IsBold = !c.IsBold));
        ToggleItalicCommand = new RelayCommand(() => ApplyFormatting(c => c.IsItalic = !c.IsItalic));
        ToggleUnderlineCommand = new RelayCommand(() => ApplyFormatting(c => c.IsUnderline = !c.IsUnderline));
        SetAlignmentCommand = new RelayCommand<string>(alignStr =>
        {
            if (Enum.TryParse<HorizontalAlignment>(alignStr, true, out var align))
                ApplyFormatting(c => c.HorizontalAlignment = align);
        });
        SetFormatCommand = new RelayCommand<string>(formatStr =>
        {
            if (Enum.TryParse<SpreadsheetCellFormat>(formatStr, true, out var fmt))
                ApplyFormatting(c => c.Format = fmt);
        });
        SetCellBackgroundCommand = new RelayCommand<Brush>(brush => ApplyFormatting(c => c.Background = brush));
        MergeCellsCommand = new RelayCommand(ExecuteMergeCells);
        UnmergeCellsCommand = new RelayCommand(ExecuteUnmergeCells);
        InsertRowCommand = new RelayCommand(ExecuteInsertRow);
        DeleteRowCommand = new RelayCommand(ExecuteDeleteRow);
        InsertColumnCommand = new RelayCommand(ExecuteInsertColumn);
        DeleteColumnCommand = new RelayCommand(ExecuteDeleteColumn);
        ExportCsvCommand = new RelayCommand(ExecuteExportCsv);
        ImportCsvCommand = new RelayCommand(ExecuteImportCsv);

        CopyCommand = new RelayCommand(ExecuteCopy);
        PasteCommand = new RelayCommand(ExecutePaste);
        CutCommand = new RelayCommand(ExecuteCut);
        ClearSelectionCommand = new RelayCommand(() => { PushUndo(); ExecuteClear(); });
        AutoSumCommand = new RelayCommand(ExecuteAutoSum);
        RecalculateCommand = new RelayCommand(() => Recalculate());
        UndoCommand = new RelayCommand(ExecuteUndo, () => _undoStack.Count > 0);
        RedoCommand = new RelayCommand(ExecuteRedo, () => _redoStack.Count > 0);

        Loaded += (s, e) => { Recalculate(); _gridPresenter?.InvalidateMeasure(); _gridPresenter?.InvalidateVisual(); };
    }

    private bool _isResizingCol;
    private int _resizingColIndex = -1;
    private double _resizeColStartPos;
    private double _resizeColStartWidth;

    private bool _isResizingRow;
    private int _resizingRowIndex = -1;
    private double _resizeRowStartPos;
    private double _resizeRowStartHeight;

    private bool _isAutoFilling;
    private SpreadsheetRange _autoFillStartRange;

    private System.Windows.Controls.Primitives.Popup? _autoCompletePopup;
    private ListBox? _autoCompleteListBox;

    private static readonly (string Name, string Syntax, string Desc)[] FormulaCatalog = new[]
    {
        ("SUM", "SUM(number1, [number2], ...)", "Tính tổng tất cả các số hoặc vùng tham chiếu"),
        ("AVERAGE", "AVERAGE(number1, [number2], ...)", "Tính trung bình cộng của các đối số"),
        ("SUMIF", "SUMIF(range, criteria, [sum_range])", "Tính tổng các ô thỏa mãn điều kiện nhất định"),
        ("COUNTIF", "COUNTIF(range, criteria)", "Đếm số ô trong một vùng thỏa mãn điều kiện"),
        ("COUNT", "COUNT(value1, [value2], ...)", "Đếm số ô có chứa giá trị số"),
        ("COUNTA", "COUNTA(value1, [value2], ...)", "Đếm số ô không rỗng trong danh sách"),
        ("MIN", "MIN(number1, [number2], ...)", "Trả về giá trị nhỏ nhất trong tập số"),
        ("MAX", "MAX(number1, [number2], ...)", "Trả về giá trị lớn nhất trong tập số"),
        ("VLOOKUP", "VLOOKUP(lookup_value, table_array, col_index, [range_lookup])", "Tìm kiếm giá trị theo cột dọc trong bảng"),
        ("HLOOKUP", "HLOOKUP(lookup_value, table_array, row_index, [range_lookup])", "Tìm kiếm giá trị theo hàng ngang trong bảng"),
        ("MATCH", "MATCH(lookup_value, lookup_array, [match_type])", "Trả về vị trí tương đối của giá trị tìm kiếm"),
        ("INDEX", "INDEX(array, row_num, [col_num])", "Trả về giá trị tại giao điểm dòng và cột"),
        ("IF", "IF(logical_test, value_if_true, [value_if_false])", "Kiểm tra điều kiện và trả về kết quả tương ứng"),
        ("IFERROR", "IFERROR(value, value_if_error)", "Bẫy lỗi và trả về giá trị thay thế nếu có lỗi"),
        ("AND", "AND(logical1, [logical2], ...)", "Trả về TRUE nếu tất cả các đối số là TRUE"),
        ("OR", "OR(logical1, [logical2], ...)", "Trả về TRUE nếu có ít nhất một đối số là TRUE"),
        ("NOT", "NOT(logical)", "Đảo ngược giá trị logic của đối số"),
        ("ROUND", "ROUND(number, num_digits)", "Làm tròn số đến số chữ số thập phân chỉ định"),
        ("ABS", "ABS(number)", "Trả về giá trị tuyệt đối của một số"),
        ("SQRT", "SQRT(number)", "Trả về căn bậc hai của một số dương"),
        ("POWER", "POWER(number, power)", "Tính lũy thừa cơ số với số mũ"),
        ("MOD", "MOD(number, divisor)", "Trả về phần dư của phép chia"),
        ("INT", "INT(number)", "Làm tròn số xuống số nguyên gần nhất"),
        ("CEILING", "CEILING(number)", "Làm tròn số lên số nguyên trần"),
        ("FLOOR", "FLOOR(number)", "Làm tròn số xuống số nguyên sàn"),
        ("CONCAT", "CONCAT(text1, [text2], ...)", "Nối hai hoặc nhiều chuỗi văn bản thành một"),
        ("LEFT", "LEFT(text, [num_chars])", "Lấy các ký tự bên trái của chuỗi văn bản"),
        ("RIGHT", "RIGHT(text, [num_chars])", "Lấy các ký tự bên phải của chuỗi văn bản"),
        ("MID", "MID(text, start_num, num_chars)", "Lấy các ký tự từ giữa chuỗi văn bản"),
        ("TRIM", "TRIM(text)", "Cắt bỏ khoảng trắng thừa ở đầu và cuối chuỗi"),
        ("UPPER", "UPPER(text)", "Chuyển toàn bộ chuỗi sang chữ in HOA"),
        ("LOWER", "LOWER(text)", "Chuyển toàn bộ chuỗi sang chữ in thường"),
        ("LEN", "LEN(text)", "Trả về độ dài số lượng ký tự của chuỗi"),
        ("TEXT", "TEXT(value, format_text)", "Định dạng số theo chuỗi định dạng hiển thị"),
        ("TODAY", "TODAY()", "Trả về ngày hiện tại của hệ thống"),
        ("NOW", "NOW()", "Trả về ngày và giờ hiện tại của hệ thống"),
        ("DATE", "DATE(year, month, day)", "Tạo ngày hợp lệ từ năm, tháng, ngày"),
        ("YEAR", "YEAR(serial_number)", "Lấy số năm từ một giá trị ngày"),
        ("MONTH", "MONTH(serial_number)", "Lấy số tháng từ một giá trị ngày (1-12)"),
        ("DAY", "DAY(serial_number)", "Lấy số ngày trong tháng từ một ngày (1-31)"),
        ("ISNUMBER", "ISNUMBER(value)", "Kiểm tra xem giá trị có phải là số không"),
        ("ISTEXT", "ISTEXT(value)", "Kiểm tra xem giá trị có phải là văn bản không"),
        ("ISBLANK", "ISBLANK(value)", "Kiểm tra xem ô có trống không")
    };

    public override void OnApplyTemplate()
    {
        if (_scrollViewer != null)
            _scrollViewer.ScrollChanged -= OnScrollViewerScrollChanged;
        if (_gridPresenter != null)
        {
            _gridPresenter.MouseLeftButtonDown -= OnGridMouseDown;
            _gridPresenter.MouseMove -= OnGridMouseMove;
            _gridPresenter.MouseLeftButtonUp -= OnGridMouseUp;
            _gridPresenter.MouseRightButtonDown -= OnGridRightMouseDown;
        }

        base.OnApplyTemplate();

        _scrollViewer = GetTemplateChild("PART_ScrollViewer") as ScrollViewer;
        if (_scrollViewer != null)
        {
            _scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
        }
        _gridPresenter = GetTemplateChild("PART_GridPresenter") as SpreadsheetGridPresenter;
        _inPlaceEditor = GetTemplateChild("PART_InPlaceEditor") as TextBox;
        _formulaTextBox = GetTemplateChild("PART_FormulaTextBox") as TextBox;
        _cellAddressTextBlock = GetTemplateChild("PART_CellAddress") as TextBlock;
        _selectionStatsTextBlock = GetTemplateChild("PART_SelectionStats") as TextBlock;
        _sheetsListBox = GetTemplateChild("PART_SheetsListBox") as ListBox;

        _formulaCommitButton = GetTemplateChild("PART_FormulaCommit") as Button;
        _formulaCancelButton = GetTemplateChild("PART_FormulaCancel") as Button;

        if (_formulaCommitButton != null)
            _formulaCommitButton.Click += (s, e) => { CommitFormulaBarEdit(); Focus(); };
        if (_formulaCancelButton != null)
            _formulaCancelButton.Click += (s, e) => { CancelFormulaBarEdit(); Focus(); };

        if (_gridPresenter != null)
        {
            _gridPresenter.AttachOwner(this);
            _gridPresenter.MouseLeftButtonDown += OnGridMouseDown;
            _gridPresenter.MouseMove += OnGridMouseMove;
            _gridPresenter.MouseLeftButtonUp += OnGridMouseUp;
            _gridPresenter.MouseRightButtonDown += OnGridRightMouseDown;
            _gridPresenter.ContextMenu = CreateGridContextMenu();
        }

        if (_inPlaceEditor != null)
        {
            _inPlaceEditor.KeyDown += OnEditorKeyDown;
            _inPlaceEditor.TextChanged += OnEditorTextChanged;
            _inPlaceEditor.LostFocus += (s, e) => CommitInPlaceEdit();
        }

        if (_formulaTextBox != null)
        {
            _formulaTextBox.KeyDown += OnFormulaTextBoxKeyDown;
            _formulaTextBox.TextChanged += (s, e) =>
            {
                if (_isEditing && _inPlaceEditor != null && !_inPlaceEditor.IsFocused && _formulaTextBox.IsFocused)
                {
                    _inPlaceEditor.Text = _formulaTextBox.Text;
                }
            };
            _formulaTextBox.LostFocus += (s, e) => CommitFormulaBarEdit();
        }

        Loaded += (s, e) =>
        {
            Recalculate();
            _gridPresenter?.InvalidateMeasure();
            _gridPresenter?.InvalidateArrange();
            _gridPresenter?.InvalidateVisual();
            InvalidateVisual();
        };

        UpdateCellAddressDisplay();
        UpdateSelectionStats();
        InvalidateVisual();
    }

    private void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (e.HorizontalChange != 0 || e.VerticalChange != 0 || e.ViewportWidthChange != 0 || e.ViewportHeightChange != 0)
            _gridPresenter?.InvalidateVisual();
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == BackgroundProperty || e.Property == ForegroundProperty ||
            e.Property == HeaderBackgroundProperty || e.Property == GridLineBrushProperty ||
            e.Property == SelectionBrushProperty || e.Property == SelectionBorderBrushProperty)
        {
            _gridPresenter?.InvalidateVisual();
            if (_isEditing) UpdateInPlaceEditorTheme();
        }
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        if (e.Handled || _isEditing || _formulaTextBox?.IsKeyboardFocusWithin == true)
            return;

        // Handle navigation before the containing ScrollViewer consumes arrow keys for scrolling.
        if (!IsKeyboardFocused && _gridPresenter?.IsKeyboardFocusWithin != true)
            return;

        bool ctrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
        bool shift = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);

        if (ctrl)
        {
            if (e.Key == Key.C) { ExecuteCopy(); e.Handled = true; return; }
            if (e.Key == Key.X) { ExecuteCut(); e.Handled = true; return; }
            if (e.Key == Key.V) { ExecutePaste(); e.Handled = true; return; }
            if (e.Key == Key.Z) { ExecuteUndo(); e.Handled = true; return; }
            if (e.Key == Key.Y) { ExecuteRedo(); e.Handled = true; return; }
            if (e.Key == Key.B) { PushUndo(); ToggleBoldCommand.Execute(null); e.Handled = true; return; }
            if (e.Key == Key.I) { PushUndo(); ToggleItalicCommand.Execute(null); e.Handled = true; return; }
            if (e.Key == Key.U) { PushUndo(); ToggleUnderlineCommand.Execute(null); e.Handled = true; return; }
            if (e.Key == Key.M) { PushUndo(); MergeCellsCommand.Execute(null); e.Handled = true; return; }
            if (e.Key == Key.A) { SelectAll(); e.Handled = true; return; }
            // Ctrl+Arrow: jump to edge of data
            if (e.Key == Key.Left)  { JumpToEdge(0, -1, shift); e.Handled = true; return; }
            if (e.Key == Key.Right) { JumpToEdge(0, +1, shift); e.Handled = true; return; }
            if (e.Key == Key.Up)    { JumpToEdge(-1, 0, shift); e.Handled = true; return; }
            if (e.Key == Key.Down)  { JumpToEdge(+1, 0, shift); e.Handled = true; return; }
            if (e.Key == Key.Home)  { NavigateTo(0, 0, shift); e.Handled = true; return; }
            if (e.Key == Key.End)   { NavigateTo(ActiveSheet?.RowCount - 1 ?? 0, ActiveSheet?.ColumnCount - 1 ?? 0, shift); e.Handled = true; return; }
        }

        if (e.Key == Key.Delete || e.Key == Key.Back)
        {
            PushUndo(); ExecuteClear();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Home)  { NavigateTo(ActiveRow, 0, shift); e.Handled = true; return; }
        if (e.Key == Key.End)   { NavigateTo(ActiveRow, (ActiveSheet?.ColumnCount ?? 1) - 1, shift); e.Handled = true; return; }
        if (e.Key == Key.PageDown) { NavigateTo(Math.Min((ActiveSheet?.RowCount ?? 1) - 1, ActiveRow + 20), ActiveColumn, shift); e.Handled = true; return; }
        if (e.Key == Key.PageUp)   { NavigateTo(Math.Max(0, ActiveRow - 20), ActiveColumn, shift); e.Handled = true; return; }

        if (shift)
        {
            // Shift+Arrow: extend selection
            if (e.Key == Key.Left)  { ExtendSelection(0, -1); e.Handled = true; return; }
            if (e.Key == Key.Right) { ExtendSelection(0, +1); e.Handled = true; return; }
            if (e.Key == Key.Up)    { ExtendSelection(-1, 0); e.Handled = true; return; }
            if (e.Key == Key.Down)  { ExtendSelection(+1, 0); e.Handled = true; return; }
            if (e.Key == Key.Tab)   { MoveActiveCell(0, -1); e.Handled = true; return; }
        }

        if (e.Key == Key.Left)  { MoveActiveCell(0, -1); e.Handled = true; }
        else if (e.Key == Key.Right || e.Key == Key.Tab) { MoveActiveCell(0, 1); e.Handled = true; }
        else if (e.Key == Key.Up) { MoveActiveCell(-1, 0); e.Handled = true; }
        else if (e.Key == Key.Down) { MoveActiveCell(1, 0); e.Handled = true; }
        else if (e.Key == Key.Enter) { if (shift) MoveActiveCell(-1, 0); else MoveActiveCell(1, 0); e.Handled = true; }
        else if (e.Key == Key.Escape) { Focus(); e.Handled = true; }
        else if (e.Key == Key.F2) { BeginInPlaceEdit(clearText: false); e.Handled = true; }
        else if (!char.IsControl((char)KeyInterop.VirtualKeyFromKey(e.Key)) && !IsReadOnly)
        {
            BeginInPlaceEdit(clearText: true);
        }
    }

    private void SelectAll()
    {
        if (ActiveSheet == null) return;
        _isExtendingSelection = false;
        _selectionAnchorRow = 0;
        _selectionAnchorColumn = 0;
        SelectionRange = new SpreadsheetRange(0, 0, ActiveSheet.RowCount - 1, ActiveSheet.ColumnCount - 1);
        UpdateSelectionStats();
        _gridPresenter?.InvalidateVisual();
    }

    private void ExtendSelection(int rowDelta, int colDelta)
    {
        if (ActiveSheet == null) return;
        if (!_isExtendingSelection)
        {
            _selectionAnchorRow = ActiveRow;
            _selectionAnchorColumn = ActiveColumn;
            _isExtendingSelection = true;
        }

        int newRow = NumericCompatibility.Clamp(ActiveRow + rowDelta, 0, ActiveSheet.RowCount - 1);
        int newColumn = NumericCompatibility.Clamp(ActiveColumn + colDelta, 0, ActiveSheet.ColumnCount - 1);
        ActiveRow = newRow;
        ActiveColumn = newColumn;
        SelectionRange = new SpreadsheetRange(_selectionAnchorRow, _selectionAnchorColumn, newRow, newColumn);
        UpdateFormulaBarFromActiveCell();
        UpdateSelectionStats();
        UpdateCellAddressDisplay();
        _gridPresenter?.InvalidateVisual();
    }

    private void JumpToEdge(int rowDir, int colDir, bool extend)
    {
        if (ActiveSheet == null) return;
        int r = ActiveRow, c = ActiveColumn;
        // Move until we hit empty or boundary
        while (true)
        {
            int nr = r + rowDir, nc = c + colDir;
            if (nr < 0 || nr >= ActiveSheet.RowCount || nc < 0 || nc >= ActiveSheet.ColumnCount) break;
            r = nr; c = nc;
            var cell = ActiveSheet.GetCell(r, c);
            bool hasData = cell != null && !string.IsNullOrEmpty(cell.RawValue);
            if (!hasData) break;
        }
        NavigateTo(r, c, extend);
    }

    private void NavigateTo(int row, int col, bool extend)
    {
        if (ActiveSheet == null) return;
        row = NumericCompatibility.Clamp(row, 0, ActiveSheet.RowCount - 1);
        col = NumericCompatibility.Clamp(col, 0, ActiveSheet.ColumnCount - 1);
        if (extend)
        {
            if (!_isExtendingSelection)
            {
                _selectionAnchorRow = ActiveRow;
                _selectionAnchorColumn = ActiveColumn;
                _isExtendingSelection = true;
            }
            ActiveRow = row;
            ActiveColumn = col;
            SelectionRange = new SpreadsheetRange(_selectionAnchorRow, _selectionAnchorColumn, row, col);
        }
        else
        {
            _isExtendingSelection = false;
            _selectionAnchorRow = row;
            _selectionAnchorColumn = col;
            ActiveRow = row; ActiveColumn = col;
            SelectionRange = new SpreadsheetRange(row, col);
        }
        UpdateFormulaBarFromActiveCell();
        UpdateCellAddressDisplay();
        UpdateSelectionStats();
        _gridPresenter?.InvalidateVisual();
    }

    private void MoveActiveCell(int rowDelta, int colDelta)
    {
        if (ActiveSheet == null) return;
        int newRow = NumericCompatibility.Clamp(ActiveRow + rowDelta, 0, ActiveSheet.RowCount - 1);
        int newCol = NumericCompatibility.Clamp(ActiveColumn + colDelta, 0, ActiveSheet.ColumnCount - 1);

        _isExtendingSelection = false;
        _selectionAnchorRow = newRow;
        _selectionAnchorColumn = newCol;
        ActiveRow = newRow;
        ActiveColumn = newCol;
        SelectionRange = new SpreadsheetRange(newRow, newCol);
        UpdateFormulaBarFromActiveCell();
        UpdateCellAddressDisplay();
        UpdateSelectionStats();
        _gridPresenter?.InvalidateVisual();
        ScrollCellIntoView(newRow, newCol);
    }

    private void ScrollCellIntoView(int row, int col)
    {
        if (_scrollViewer == null || ActiveSheet == null) return;
        var bounds = GetCellBounds(row, col);
        if (bounds.IsEmpty) return;
        double svW = _scrollViewer.ViewportWidth;
        double svH = _scrollViewer.ViewportHeight;
        double ox = _scrollViewer.HorizontalOffset;
        double oy = _scrollViewer.VerticalOffset;
        if (bounds.Right > ox + svW) _scrollViewer.ScrollToHorizontalOffset(bounds.Right - svW + 20);
        else if (bounds.Left < ox + RowHeaderWidth) _scrollViewer.ScrollToHorizontalOffset(Math.Max(0, bounds.Left - RowHeaderWidth));
        if (bounds.Bottom > oy + svH) _scrollViewer.ScrollToVerticalOffset(bounds.Bottom - svH + 20);
        else if (bounds.Top < oy + ColumnHeaderHeight) _scrollViewer.ScrollToVerticalOffset(Math.Max(0, bounds.Top - ColumnHeaderHeight));
    }

    private void OnGridMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (ActiveSheet == null || _gridPresenter == null) return;

        if (!_gridPresenter.Focus()) Focus();
        Point pt = e.GetPosition(_gridPresenter);
        bool shift = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);

        // 1. Top-Left corner click → select all
        if (pt.X <= RowHeaderWidth && pt.Y <= ColumnHeaderHeight)
        {
            SelectAll();
            return;
        }

        // 2. Column Header click → select full column (or resize / auto-fit)
        if (pt.Y <= ColumnHeaderHeight && pt.X >= RowHeaderWidth)
        {
            int sepCol = HitTestColSeparator(pt.X);
            if (sepCol >= 0)
            {
                if (e.ClickCount == 2)
                {
                    AutoFitColumn(sepCol);
                    return;
                }
                _isResizingCol = true;
                _resizingColIndex = sepCol;
                _resizeColStartPos = pt.X;
                _resizeColStartWidth = ActiveSheet.GetColumnWidth(sepCol);
                _gridPresenter.CaptureMouse();
                return;
            }
            int clickedCol = HitTestColHeader(pt.X);
            if (clickedCol >= 0)
            {
                if (shift)
                    SelectionRange = new SpreadsheetRange(SelectionRange.MinRow, SelectionRange.MinCol, ActiveSheet.RowCount - 1, clickedCol);
                else
                {
                    ActiveRow = 0; ActiveColumn = clickedCol;
                    SelectionRange = new SpreadsheetRange(0, clickedCol, ActiveSheet.RowCount - 1, clickedCol);
                }
                _isSelectingHeader = true;
                _selectionDragStarted = false;
                _selectionStartPoint = pt;
                _gridPresenter.CaptureMouse();
                UpdateSelectionStats(); _gridPresenter.InvalidateVisual();
                return;
            }
        }

        // 3. Row Header click → select full row (or resize / auto-fit)
        if (pt.X <= RowHeaderWidth && pt.Y >= ColumnHeaderHeight)
        {
            int sepRow = HitTestRowSeparator(pt.Y);
            if (sepRow >= 0)
            {
                if (e.ClickCount == 2)
                {
                    AutoFitRow(sepRow);
                    return;
                }
                _isResizingRow = true;
                _resizingRowIndex = sepRow;
                _resizeRowStartPos = pt.Y;
                _resizeRowStartHeight = ActiveSheet.GetRowHeight(sepRow);
                _gridPresenter.CaptureMouse();
                return;
            }
            int clickedRow = HitTestRowHeader(pt.Y);
            if (clickedRow >= 0)
            {
                if (shift)
                    SelectionRange = new SpreadsheetRange(SelectionRange.MinRow, SelectionRange.MinCol, clickedRow, ActiveSheet.ColumnCount - 1);
                else
                {
                    ActiveRow = clickedRow; ActiveColumn = 0;
                    SelectionRange = new SpreadsheetRange(clickedRow, 0, clickedRow, ActiveSheet.ColumnCount - 1);
                }
                _isSelectingHeader = true;
                _selectionDragStarted = false;
                _selectionStartPoint = pt;
                _gridPresenter.CaptureMouse();
                UpdateSelectionStats(); _gridPresenter.InvalidateVisual();
                return;
            }
        }

        // 4. Check AutoFill Handle Drag
        Rect rangeRect = GetRangeBoundingRect(SelectionRange);
        if (!rangeRect.IsEmpty)
        {
            Rect handleRect = new Rect(rangeRect.Right - 8, rangeRect.Bottom - 8, 16, 16);
            if (handleRect.Contains(pt))
            {
                _isAutoFilling = true;
                _autoFillStartRange = SelectionRange;
                _gridPresenter.CaptureMouse();
                return;
            }
        }

        // 5. Normal Cell Selection & Edit
        var (row, col) = HitTestCell(pt);
        if (row >= 0 && col >= 0)
        {
            // Excel Point-and-Click Formula Reference: If editing a formula (starts with '='), clicking another cell inserts its address!
            if (_isEditing && _inPlaceEditor != null && !string.IsNullOrEmpty(_inPlaceEditor.Text) && _inPlaceEditor.Text.StartsWith("="))
            {
                string clickedAddr = SpreadsheetAddressHelper.ToAddress(row, col);
                // Insert cell address at caret position or append
                int caret = _inPlaceEditor.CaretIndex;
                string cur = _inPlaceEditor.Text;
                if (caret >= 0 && caret <= cur.Length)
                {
                    _inPlaceEditor.Text = cur.Insert(caret, clickedAddr);
                    _inPlaceEditor.CaretIndex = caret + clickedAddr.Length;
                }
                else
                {
                    _inPlaceEditor.Text += clickedAddr;
                    _inPlaceEditor.CaretIndex = _inPlaceEditor.Text.Length;
                }
                _inPlaceEditor.Focus();
                return;
            }

            if (e.ClickCount == 2 && !IsReadOnly)
            {
                BeginInPlaceEdit(clearText: false);
                return;
            }

            // Excel behaviour: clicking an ALREADY selected single cell enters edit mode
            bool alreadySelected = SelectionRange.IsSingleCell
                                   && ActiveRow == row && ActiveColumn == col
                                   && e.ClickCount == 1;

            _isSelecting = true;
            _selectionDragStarted = false;
            _selectionStartPoint = pt;
            if (shift)
            {
                if (!_isExtendingSelection)
                {
                    _selectionAnchorRow = ActiveRow;
                    _selectionAnchorColumn = ActiveColumn;
                    _isExtendingSelection = true;
                }
                ActiveRow = row;
                ActiveColumn = col;
                SelectionRange = new SpreadsheetRange(_selectionAnchorRow, _selectionAnchorColumn, row, col);
            }
            else
            {
                _isExtendingSelection = false;
                _selectionAnchorRow = row;
                _selectionAnchorColumn = col;
                ActiveRow = row; ActiveColumn = col;
                SelectionRange = new SpreadsheetRange(row, col);
            }

            UpdateFormulaBarFromActiveCell();
            UpdateCellAddressDisplay();
            UpdateSelectionStats();
            _gridPresenter.CaptureMouse();
            _gridPresenter.InvalidateVisual();

            if (alreadySelected && !IsReadOnly)
            {
                BeginInPlaceEdit(clearText: false);
            }
        }
    }

    private void OnGridMouseMove(object sender, MouseEventArgs e)
    {
        if (ActiveSheet == null || _gridPresenter == null) return;

        Point pt = e.GetPosition(_gridPresenter);

        // Resize Column Dragging
        if (_isResizingCol && _resizingColIndex >= 0)
        {
            double delta = pt.X - _resizeColStartPos;
            double newWidth = Math.Max(25.0, _resizeColStartWidth + delta);
            ActiveSheet.SetColumnWidth(_resizingColIndex, newWidth);
            _gridPresenter.InvalidateMeasure();
            _gridPresenter.InvalidateVisual();
            return;
        }

        // Resize Row Dragging
        if (_isResizingRow && _resizingRowIndex >= 0)
        {
            double delta = pt.Y - _resizeRowStartPos;
            double newHeight = Math.Max(16.0, _resizeRowStartHeight + delta);
            ActiveSheet.SetRowHeight(_resizingRowIndex, newHeight);
            _gridPresenter.InvalidateMeasure();
            _gridPresenter.InvalidateVisual();
            return;
        }

        // AutoFill Dragging (Excel standard: supports both vertical down and horizontal right)
        if (_isAutoFilling)
        {
            var (row, col) = HitTestCell(pt);
            if (row >= 0 && col >= 0)
            {
                int rDelta = row - _autoFillStartRange.MaxRow;
                int cDelta = col - _autoFillStartRange.MaxCol;

                if (rDelta >= cDelta && rDelta > 0)
                {
                    // Extend downwards
                    SelectionRange = new SpreadsheetRange(_autoFillStartRange.MinRow, _autoFillStartRange.MinCol, row, _autoFillStartRange.MaxCol);
                }
                else if (cDelta > 0)
                {
                    // Extend rightwards
                    SelectionRange = new SpreadsheetRange(_autoFillStartRange.MinRow, _autoFillStartRange.MinCol, _autoFillStartRange.MaxRow, col);
                }
                UpdateSelectionStats();
                _gridPresenter.InvalidateVisual();
            }
            return;
        }

        // Header selection drag
        if (_isSelectingHeader && e.LeftButton == MouseButtonState.Pressed)
        {
            if (!TryStartSelectionDrag(pt)) return;

            if (pt.Y <= ColumnHeaderHeight) // column drag
            {
                int c = HitTestColHeader(pt.X);
                if (c >= 0)
                {
                    SelectionRange = new SpreadsheetRange(0, Math.Min(SelectionRange.MinCol, c), ActiveSheet.RowCount - 1, Math.Max(SelectionRange.MaxCol, c));
                    UpdateSelectionStats(); _gridPresenter.InvalidateVisual();
                }
            }
            else if (pt.X <= RowHeaderWidth) // row drag
            {
                int r = HitTestRowHeader(pt.Y);
                if (r >= 0)
                {
                    SelectionRange = new SpreadsheetRange(Math.Min(SelectionRange.MinRow, r), 0, Math.Max(SelectionRange.MaxRow, r), ActiveSheet.ColumnCount - 1);
                    UpdateSelectionStats(); _gridPresenter.InvalidateVisual();
                }
            }
            return;
        }

        // Normal Mouse Hover Cursor update + hover highlight
        int prevHovRow = _hoveredRow, prevHovCol = _hoveredCol;
        if (!_isSelecting && !_isAutoFilling)
        {
            Rect curRangeRect = GetRangeBoundingRect(SelectionRange);
            bool onHandle = !curRangeRect.IsEmpty && new Rect(curRangeRect.Right - 8, curRangeRect.Bottom - 8, 16, 16).Contains(pt);

            if (onHandle)
                _gridPresenter.Cursor = Cursors.Cross;
            else if (pt.Y <= ColumnHeaderHeight && pt.X >= RowHeaderWidth && HitTestColSeparator(pt.X) >= 0)
                _gridPresenter.Cursor = Cursors.SizeWE;
            else if (pt.X <= RowHeaderWidth && pt.Y >= ColumnHeaderHeight && HitTestRowSeparator(pt.Y) >= 0)
                _gridPresenter.Cursor = Cursors.SizeNS;
            else if (pt.Y <= ColumnHeaderHeight && pt.X >= RowHeaderWidth)
                _gridPresenter.Cursor = Cursors.Arrow;
            else if (pt.X <= RowHeaderWidth && pt.Y >= ColumnHeaderHeight)
                _gridPresenter.Cursor = Cursors.Arrow;
            else
                _gridPresenter.Cursor = Cursors.Arrow; // Excel standard cell hover cursor
        }

        var (hRow, hCol) = HitTestCell(pt);
        _hoveredRow = hRow;
        _hoveredCol = hCol;
        // redraw only if hover changed
        if (_hoveredRow != prevHovRow || _hoveredCol != prevHovCol)
            _gridPresenter.InvalidateVisual();

        if (_isSelecting && e.LeftButton == MouseButtonState.Pressed && hRow >= 0 && hCol >= 0 && TryStartSelectionDrag(pt))
        {
            var (startRow, startCol) = HitTestCell(_selectionStartPoint);
            SelectionRange = new SpreadsheetRange(startRow, startCol, hRow, hCol);
            UpdateSelectionStats();
            _gridPresenter.InvalidateVisual();
        }
    }

    private void OnGridMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_isResizingCol || _isResizingRow)
        {
            _isResizingCol = false;
            _isResizingRow = false;
            _gridPresenter?.ReleaseMouseCapture();
            _gridPresenter?.InvalidateMeasure();
            _gridPresenter?.InvalidateVisual();
            return;
        }

        if (_isAutoFilling)
        {
            _isAutoFilling = false;
            _gridPresenter?.ReleaseMouseCapture();
            PushUndo();
            ExecuteAutoFillApply(_autoFillStartRange, SelectionRange);
            return;
        }

        if (_isSelectingHeader)
        {
            _isSelectingHeader = false;
            _selectionDragStarted = false;
            _gridPresenter?.ReleaseMouseCapture();
            return;
        }

        if (_isSelecting)
        {
            _isSelecting = false;
            _selectionDragStarted = false;
            _gridPresenter?.ReleaseMouseCapture();
            _gridPresenter?.InvalidateVisual();
        }
    }

    private bool TryStartSelectionDrag(Point currentPoint)
    {
        if (_selectionDragStarted) return true;

        double deltaX = Math.Abs(currentPoint.X - _selectionStartPoint.X);
        double deltaY = Math.Abs(currentPoint.Y - _selectionStartPoint.Y);
        if (deltaX < SystemParameters.MinimumHorizontalDragDistance &&
            deltaY < SystemParameters.MinimumVerticalDragDistance)
            return false;

        _selectionDragStarted = true;
        return true;
    }

    private int HitTestColSeparator(double x)
    {
        if (ActiveSheet == null) return -1;
        double curX = RowHeaderWidth;
        for (int c = 0; c < ActiveSheet.ColumnCount; c++)
        {
            curX += ActiveSheet.GetColumnWidth(c);
            if (Math.Abs(x - curX) <= 5.0) return c;
        }
        return -1;
    }

    private int HitTestRowSeparator(double y)
    {
        if (ActiveSheet == null) return -1;
        double curY = ColumnHeaderHeight;
        for (int r = 0; r < ActiveSheet.RowCount; r++)
        {
            curY += ActiveSheet.GetRowHeight(r);
            if (Math.Abs(y - curY) <= 5.0) return r;
        }
        return -1;
    }

    private int HitTestColHeader(double x)
    {
        if (ActiveSheet == null) return -1;
        double curX = RowHeaderWidth;
        for (int c = 0; c < ActiveSheet.ColumnCount; c++)
        {
            double w = ActiveSheet.GetColumnWidth(c);
            if (x >= curX && x < curX + w) return c;
            curX += w;
        }
        return -1;
    }

    private int HitTestRowHeader(double y)
    {
        if (ActiveSheet == null) return -1;
        double curY = ColumnHeaderHeight;
        for (int r = 0; r < ActiveSheet.RowCount; r++)
        {
            double h = ActiveSheet.GetRowHeight(r);
            if (y >= curY && y < curY + h) return r;
            curY += h;
        }
        return -1;
    }

    private void ExecuteAutoFillApply(SpreadsheetRange source, SpreadsheetRange target)
    {
        if (ActiveSheet == null) return;

        // Vertical AutoFill (Extend downwards)
        if (target.RowCount > source.RowCount)
        {
            int srcRowCount = source.RowCount;
            for (int r = source.MaxRow + 1; r <= target.MaxRow; r++)
            {
                int offset = r - source.MinRow;
                int srcR = source.MinRow + (offset % srcRowCount);
                int rowShift = r - srcR;

                for (int c = source.MinCol; c <= source.MaxCol; c++)
                {
                    var srcCell = ActiveSheet.GetCell(srcR, c);
                    var dstCell = ActiveSheet.GetOrCreateCell(r, c);

                    if (srcCell != null)
                    {
                        dstCell.Format = srcCell.Format;
                        dstCell.IsBold = srcCell.IsBold;
                        dstCell.IsItalic = srcCell.IsItalic;
                        dstCell.HorizontalAlignment = srcCell.HorizontalAlignment;
                        dstCell.Background = srcCell.Background;
                        dstCell.Foreground = srcCell.Foreground;

                        if (!string.IsNullOrEmpty(srcCell.Formula) && srcCell.Formula.StartsWith("="))
                        {
                            dstCell.Formula = ShiftFormulaRows(srcCell.Formula, rowShift);
                            dstCell.RawValue = dstCell.Formula;
                        }
                        else if (double.TryParse(srcCell.RawValue, out double dVal))
                        {
                            dstCell.RawValue = (dVal + (r - srcR)).ToString(CultureInfo.InvariantCulture);
                            dstCell.Formula = string.Empty;
                        }
                        else
                        {
                            dstCell.RawValue = srcCell.RawValue;
                            dstCell.Formula = string.Empty;
                        }
                    }
                }
            }
        }
        // Horizontal AutoFill (Extend rightwards)
        else if (target.ColCount > source.ColCount)
        {
            int srcColCount = source.ColCount;
            for (int c = source.MaxCol + 1; c <= target.MaxCol; c++)
            {
                int offset = c - source.MinCol;
                int srcC = source.MinCol + (offset % srcColCount);
                int colShift = c - srcC;

                for (int r = source.MinRow; r <= source.MaxRow; r++)
                {
                    var srcCell = ActiveSheet.GetCell(r, srcC);
                    var dstCell = ActiveSheet.GetOrCreateCell(r, c);

                    if (srcCell != null)
                    {
                        dstCell.Format = srcCell.Format;
                        dstCell.IsBold = srcCell.IsBold;
                        dstCell.IsItalic = srcCell.IsItalic;
                        dstCell.HorizontalAlignment = srcCell.HorizontalAlignment;
                        dstCell.Background = srcCell.Background;
                        dstCell.Foreground = srcCell.Foreground;

                        if (!string.IsNullOrEmpty(srcCell.Formula) && srcCell.Formula.StartsWith("="))
                        {
                            dstCell.Formula = ShiftFormulaCols(srcCell.Formula, colShift);
                            dstCell.RawValue = dstCell.Formula;
                        }
                        else if (double.TryParse(srcCell.RawValue, out double dVal))
                        {
                            dstCell.RawValue = (dVal + (c - srcC)).ToString(CultureInfo.InvariantCulture);
                            dstCell.Formula = string.Empty;
                        }
                        else
                        {
                            dstCell.RawValue = srcCell.RawValue;
                            dstCell.Formula = string.Empty;
                        }
                    }
                }
            }
        }

        Recalculate();
    }

    private static string ShiftFormulaRows(string formula, int rowShift)
    {
        return System.Text.RegularExpressions.Regex.Replace(formula, @"(\$?)([A-Za-z]+)(\$?)([1-9][0-9]*)", m =>
        {
            string colLock = m.Groups[1].Value;
            string col = m.Groups[2].Value;
            string rowLock = m.Groups[3].Value;
            string rowStr = m.Groups[4].Value;

            if (rowLock == "$") // absolute row lock: e.g. A$1 or $A$1
                return $"{colLock}{col}{rowLock}{rowStr}";

            int row = int.Parse(rowStr) + rowShift;
            return $"{colLock}{col}{Math.Max(1, row)}";
        });
    }

    private static string ShiftFormulaCols(string formula, int colShift)
    {
        return System.Text.RegularExpressions.Regex.Replace(formula, @"(\$?)([A-Za-z]+)(\$?)([1-9][0-9]*)", m =>
        {
            string colLock = m.Groups[1].Value;
            string colStr = m.Groups[2].Value;
            string rowLock = m.Groups[3].Value;
            string rowStr = m.Groups[4].Value;

            if (colLock == "$") // absolute column lock: e.g. $A1 or $A$1
                return $"{colLock}{colStr}{rowLock}{rowStr}";

            int colIdx = SpreadsheetAddressHelper.ColumnNameToIndex(colStr) + colShift;
            return $"{SpreadsheetAddressHelper.ColumnIndexToName(Math.Max(0, colIdx))}{rowLock}{rowStr}";
        });
    }

    private void OnGridRightMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (ActiveSheet == null || _gridPresenter == null) return;

        Point pt = e.GetPosition(_gridPresenter);
        var (row, col) = HitTestCell(pt);
        if (row >= 0 && col >= 0 && !SelectionRange.Contains(row, col))
        {
            ActiveRow = row;
            ActiveColumn = col;
            SelectionRange = new SpreadsheetRange(row, col);
            UpdateFormulaBarFromActiveCell();
            UpdateCellAddressDisplay();
            UpdateSelectionStats();
            _gridPresenter.InvalidateVisual();
        }
    }

    private (int row, int col) HitTestCell(Point pt)
    {
        if (ActiveSheet == null) return (-1, -1);
        int row = HitTestOffset(ActiveSheet.GetRowOffsets(), pt.Y - ColumnHeaderHeight);
        int column = HitTestOffset(ActiveSheet.GetColumnOffsets(), pt.X - RowHeaderWidth);
        return (row, column);
    }

    private static int HitTestOffset(double[] offsets, double coordinate)
    {
        if (coordinate < 0 || coordinate > offsets[^1]) return -1;
        int index = FindFirstCellEndingAtOrAfter(offsets, coordinate);
        if (index < offsets.Length - 2 && coordinate >= offsets[index + 1]) index++;
        return index >= 0 && index < offsets.Length - 1 && coordinate >= offsets[index] && coordinate <= offsets[index + 1]
            ? index
            : -1;
    }

    public Rect GetCellBounds(int row, int col)
    {
        if (ActiveSheet == null) return Rect.Empty;
        if (row < 0 || row >= ActiveSheet.RowCount || col < 0 || col >= ActiveSheet.ColumnCount)
            return Rect.Empty;

        var columnOffsets = ActiveSheet.GetColumnOffsets();
        var rowOffsets = ActiveSheet.GetRowOffsets();
        return new Rect(
            RowHeaderWidth + columnOffsets[col],
            ColumnHeaderHeight + rowOffsets[row],
            columnOffsets[col + 1] - columnOffsets[col],
            rowOffsets[row + 1] - rowOffsets[row]);
    }

    private static int FindFirstCellEndingAtOrAfter(double[] offsets, double coordinate)
    {
        int cellCount = offsets.Length - 1;
        int low = 0;
        int high = cellCount;
        while (low < high)
        {
            int middle = low + ((high - low) / 2);
            if (offsets[middle + 1] < coordinate) low = middle + 1;
            else high = middle;
        }
        return Math.Min(low, cellCount - 1);
    }

    private static int FindFirstCellStartingAfter(double[] offsets, double coordinate)
    {
        int cellCount = offsets.Length - 1;
        int low = 0;
        int high = cellCount;
        while (low < high)
        {
            int middle = low + ((high - low) / 2);
            if (offsets[middle] <= coordinate) low = middle + 1;
            else high = middle;
        }
        return Math.Min(low, cellCount - 1);
    }

    private string _editOriginalText = string.Empty;

    private void BeginInPlaceEdit(bool clearText)
    {
        if (IsReadOnly || ActiveSheet == null || _inPlaceEditor == null) return;

        var cell = ActiveSheet.GetOrCreateCell(ActiveRow, ActiveColumn);
        if (cell.IsReadOnly) return;

        _isEditing = true;

        // Position exactly over the cell on PART_EditorCanvas (inside the ScrollViewer's content Grid)
        Rect bounds = GetCellBounds(ActiveRow, ActiveColumn);

        // In Excel, the active cell border is 2px thick and sits directly centered on grid lines
        WpfCanvas.SetLeft(_inPlaceEditor, bounds.Left - 1);
        WpfCanvas.SetTop (_inPlaceEditor, bounds.Top - 1);
        _inPlaceEditor.Width  = bounds.Width + 2;
        _inPlaceEditor.Height = bounds.Height + 2;

        // Sync cell style
        double fontSize = cell.FontSize > 0 ? cell.FontSize : 11.0;
        _inPlaceEditor.FontSize   = fontSize;
        _inPlaceEditor.FontWeight = cell.IsBold   ? FontWeights.SemiBold : FontWeights.Normal;
        _inPlaceEditor.FontStyle  = cell.IsItalic ? FontStyles.Italic    : FontStyles.Normal;
        Brush? editorBg = !string.IsNullOrEmpty(cell.BackgroundResourceKey)
            ? (Brush?)TryFindResource(cell.BackgroundResourceKey)
            : cell.Background;
        if (editorBg == null)
        {
            editorBg = (Brush?)TryFindResource("CONCOST.Brush.InputBackground")
                       ?? Background
                       ?? (Brush?)TryFindResource("CONCOST.Brush.Surface")
                       ?? Brushes.White;
        }
        _inPlaceEditor.Background = editorBg;

        Brush? editorFg = !string.IsNullOrEmpty(cell.ForegroundResourceKey)
            ? (Brush?)TryFindResource(cell.ForegroundResourceKey)
            : cell.Foreground;
        if (editorFg == null)
        {
            if (editorBg is SolidColorBrush scbBg && scbBg.Color.A > 40)
            {
                double lum = (0.299 * scbBg.Color.R + 0.587 * scbBg.Color.G + 0.114 * scbBg.Color.B) / 255.0;
                editorFg = lum > 0.55
                    ? new SolidColorBrush(Color.FromRgb(15, 23, 42))
                    : new SolidColorBrush(Color.FromRgb(248, 250, 252));
            }
            else
            {
                editorFg = Foreground ?? (Brush?)TryFindResource("CONCOST.Brush.TextPrimary") ?? Brushes.Black;
            }
        }
        _inPlaceEditor.Foreground = editorFg;
        _inPlaceEditor.TextAlignment = cell.HorizontalAlignment switch
        {
            HorizontalAlignment.Right  => TextAlignment.Right,
            HorizontalAlignment.Center => TextAlignment.Center,
            _                          => TextAlignment.Left,
        };
        _inPlaceEditor.Padding = new Thickness(4, 0, 4, 0);

        var themeAccent = SelectionBorderBrush 
                          ?? (Brush?)TryFindResource("CONCOST.Brush.Primary") 
                          ?? new SolidColorBrush(Color.FromRgb(234, 88, 12));
        _inPlaceEditor.BorderBrush = themeAccent;
        _inPlaceEditor.CaretBrush = themeAccent;
        _inPlaceEditor.SelectionBrush = themeAccent;

        _editOriginalText = !string.IsNullOrEmpty(cell.Formula) ? cell.Formula
                            : (cell.RawValue ?? string.Empty);
        _inPlaceEditor.Text = clearText ? string.Empty : _editOriginalText;
        _inPlaceEditor.Visibility = Visibility.Visible;
        _gridPresenter?.InvalidateVisual();

        // Enable hit-test on canvas
        if (_inPlaceEditor.Parent is System.Windows.Controls.Canvas canvas)
            canvas.IsHitTestVisible = true;

        Dispatcher.InvokeAsync(() =>
        {
            _inPlaceEditor.Focus();
            Keyboard.Focus(_inPlaceEditor);
            if (clearText)
                _inPlaceEditor.CaretIndex = _inPlaceEditor.Text.Length;
            else
                _inPlaceEditor.SelectAll();
        }, System.Windows.Threading.DispatcherPriority.Input);
    }

    private void UpdateInPlaceEditorTheme()
    {
        if (!_isEditing || _inPlaceEditor == null) return;

        var cell = ActiveSheet?.GetCell(ActiveRow, ActiveColumn);
        string? backgroundKey = cell?.BackgroundResourceKey;
        Brush? background = backgroundKey is { Length: > 0 }
            ? TryFindResource(backgroundKey) as Brush
            : cell?.Background;
        background ??= (Brush?)TryFindResource("CONCOST.Brush.InputBackground")
                       ?? Background
                       ?? Brushes.White;
        _inPlaceEditor.Background = background;

        string? foregroundKey = cell?.ForegroundResourceKey;
        Brush? foreground = foregroundKey is { Length: > 0 }
            ? TryFindResource(foregroundKey) as Brush
            : cell?.Foreground;
        if (foreground == null && background is SolidColorBrush solid && solid.Color.A > 40)
        {
            double luminance = (0.299 * solid.Color.R + 0.587 * solid.Color.G + 0.114 * solid.Color.B) / 255.0;
            foreground = luminance > 0.55
                ? new SolidColorBrush(Color.FromRgb(15, 23, 42))
                : new SolidColorBrush(Color.FromRgb(248, 250, 252));
        }
        _inPlaceEditor.Foreground = foreground
            ?? (Brush?)TryFindResource("CONCOST.Brush.TextPrimary")
            ?? Foreground
            ?? Brushes.Black;

        var accent = SelectionBorderBrush
                     ?? (Brush?)TryFindResource("CONCOST.Brush.Primary")
                     ?? new SolidColorBrush(Color.FromRgb(234, 88, 12));
        _inPlaceEditor.BorderBrush = accent;
        _inPlaceEditor.CaretBrush = accent;
        _inPlaceEditor.SelectionBrush = accent;
    }

    private void CommitInPlaceEdit()
    {
        if (!_isEditing || ActiveSheet == null || _inPlaceEditor == null) return;

        _isEditing = false;
        _inPlaceEditor.Visibility = Visibility.Collapsed;
        if (_inPlaceEditor.Parent is System.Windows.Controls.Canvas cv)
            cv.IsHitTestVisible = false;

        string text = _inPlaceEditor.Text ?? string.Empty;
        PushUndo();
        SetCellValueAndRecalculate(ActiveRow, ActiveColumn, text);

        UpdateFormulaBarFromActiveCell();
        UpdateSelectionStats();
        _gridPresenter?.InvalidateVisual();
        Focus();
    }

    private void CancelInPlaceEdit()
    {
        if (!_isEditing || _inPlaceEditor == null) return;
        _isEditing = false;
        _inPlaceEditor.Visibility = Visibility.Collapsed;
        if (_inPlaceEditor.Parent is System.Windows.Controls.Canvas cv)
            cv.IsHitTestVisible = false;
        // Restore formula bar to original text
        if (_formulaTextBox != null) _formulaTextBox.Text = _editOriginalText;
        _gridPresenter?.InvalidateVisual();
        Focus();
    }

    private void OnEditorTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        // Live-sync formula bar while typing
        if (_formulaTextBox != null && _isEditing)
            _formulaTextBox.Text = _inPlaceEditor?.Text ?? string.Empty;

        UpdateFormulaAutoComplete();
        _gridPresenter?.InvalidateVisual();
    }

    private void UpdateFormulaAutoComplete()
    {
        if (_inPlaceEditor == null || !_isEditing)
        {
            CloseAutoComplete();
            return;
        }

        string text = _inPlaceEditor.Text ?? string.Empty;
        int caret = NumericCompatibility.Clamp(_inPlaceEditor.CaretIndex, 0, text.Length);

        // Only trigger if formula starts with '='
        if (!text.StartsWith("="))
        {
            CloseAutoComplete();
            return;
        }

        // Find function name prefix right before caret: e.g. =S or =SUM(A1)+V
        string beforeCaret = text[..caret];
        int lastNonAlpha = beforeCaret.Length - 1;
        while (lastNonAlpha >= 0 && char.IsLetter(beforeCaret[lastNonAlpha]))
        {
            lastNonAlpha--;
        }

        string prefix = beforeCaret[(lastNonAlpha + 1)..].ToUpperInvariant();

        // If prefix is at least 1 character and preceding char is '=' or an operator '+', '-', '*', '/', '(', ','
        char prevChar = lastNonAlpha >= 0 ? beforeCaret[lastNonAlpha] : '\0';
        bool canBeFunction = prevChar is '=' or '+' or '-' or '*' or '/' or '(' or ',' or ';' || lastNonAlpha == -1;

        if (canBeFunction && !string.IsNullOrEmpty(prefix))
        {
            var matches = FormulaCatalog.Where(f => f.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).Take(8).ToList();
            if (matches.Count > 0)
            {
                ShowAutoComplete(matches, prefix);
                return;
            }
        }

        CloseAutoComplete();
    }

    private void ShowAutoComplete(List<(string Name, string Syntax, string Desc)> matches, string prefix)
    {
        if (_inPlaceEditor == null) return;

        if (_autoCompletePopup == null)
        {
            _autoCompletePopup = new System.Windows.Controls.Primitives.Popup
            {
                StaysOpen = false,
                AllowsTransparency = true,
                Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom,
                PlacementTarget = _inPlaceEditor,
                HorizontalOffset = 0,
                VerticalOffset = 2
            };

            var border = new Border
            {
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(4),
                Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 8, ShadowDepth = 2, Opacity = 0.15 }
            };
            border.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
            border.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

            _autoCompleteListBox = new ListBox
            {
                BorderThickness = new Thickness(0),
                MaxHeight = 180,
                MinWidth = 240
            };
            _autoCompleteListBox.SetResourceReference(Control.BackgroundProperty, "CONCOST.Brush.Surface");
            ScrollViewer.SetHorizontalScrollBarVisibility(_autoCompleteListBox, ScrollBarVisibility.Disabled);

            _autoCompleteListBox.MouseDoubleClick += (s, e) => InsertSelectedAutoComplete();

            border.Child = _autoCompleteListBox;
            _autoCompletePopup.Child = border;
        }

        _autoCompleteListBox!.ItemsSource = matches.Select(m => new FormulaItem
        {
            Name = m.Name,
            Display = $"{m.Name} — {m.Desc}",
            Syntax = m.Syntax
        }).ToList();

        _autoCompleteListBox.DisplayMemberPath = "Display";
        _autoCompleteListBox.SelectedIndex = 0;
        _autoCompletePopup.IsOpen = true;
    }

    private void CloseAutoComplete()
    {
        if (_autoCompletePopup != null && _autoCompletePopup.IsOpen)
        {
            _autoCompletePopup.IsOpen = false;
        }
    }

    private void InsertSelectedAutoComplete()
    {
        if (_inPlaceEditor == null || _autoCompleteListBox?.SelectedItem is not FormulaItem selected) return;

        string funcName = selected.Name;

        string text = _inPlaceEditor.Text ?? string.Empty;
        int caret = NumericCompatibility.Clamp(_inPlaceEditor.CaretIndex, 0, text.Length);

        string beforeCaret = text[..caret];
        int lastNonAlpha = beforeCaret.Length - 1;
        while (lastNonAlpha >= 0 && char.IsLetter(beforeCaret[lastNonAlpha]))
        {
            lastNonAlpha--;
        }

        string newBefore = text[..(lastNonAlpha + 1)] + funcName + "(";
        string afterCaret = text[caret..];

        _inPlaceEditor.Text = newBefore + afterCaret;
        _inPlaceEditor.CaretIndex = newBefore.Length;
        CloseAutoComplete();
        _inPlaceEditor.Focus();
    }

    private void OnEditorKeyDown(object sender, KeyEventArgs e)
    {
        // Handle AutoComplete popup navigation
        if (_autoCompletePopup != null && _autoCompletePopup.IsOpen && _autoCompleteListBox != null)
        {
            if (e.Key == Key.Down)
            {
                if (_autoCompleteListBox.SelectedIndex < _autoCompleteListBox.Items.Count - 1)
                    _autoCompleteListBox.SelectedIndex++;
                e.Handled = true;
                return;
            }
            if (e.Key == Key.Up)
            {
                if (_autoCompleteListBox.SelectedIndex > 0)
                    _autoCompleteListBox.SelectedIndex--;
                e.Handled = true;
                return;
            }
            if (e.Key is Key.Tab or Key.Enter && _autoCompleteListBox.SelectedItem != null)
            {
                InsertSelectedAutoComplete();
                e.Handled = true;
                return;
            }
            if (e.Key == Key.Escape)
            {
                CloseAutoComplete();
                e.Handled = true;
                return;
            }
        }

        if (e.Key == Key.Enter)
        {
            CloseAutoComplete();
            bool shift = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
            CommitInPlaceEdit();
            if (shift) MoveActiveCell(-1, 0); else MoveActiveCell(1, 0);
            e.Handled = true;
        }
        else if (e.Key == Key.Tab)
        {
            CloseAutoComplete();
            bool shift = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
            CommitInPlaceEdit();
            if (shift) MoveActiveCell(0, -1); else MoveActiveCell(0, 1);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            CloseAutoComplete();
            _isEditing = false;
            if (_inPlaceEditor != null) _inPlaceEditor.Visibility = Visibility.Collapsed;
            Focus();
            e.Handled = true;
        }
        else if (e.Key == Key.F4)
        {
            ToggleCellReferenceAbsoluteMode();
            e.Handled = true;
        }
    }

    private void ToggleCellReferenceAbsoluteMode()
    {
        if (_inPlaceEditor == null || string.IsNullOrEmpty(_inPlaceEditor.Text)) return;

        string text = _inPlaceEditor.Text;
        int caret = NumericCompatibility.Clamp(_inPlaceEditor.CaretIndex, 0, text.Length);

        // Find cell reference around caret: e.g. A1, $A$1, A$1, $A1
        var matches = System.Text.RegularExpressions.Regex.Matches(text, @"\$?[A-Za-z]+\$?[1-9][0-9]*");
        foreach (System.Text.RegularExpressions.Match m in matches)
        {
            if (caret >= m.Index && caret <= m.Index + m.Length)
            {
                string orig = m.Value;
                // Parse col and row components with $
                var match = System.Text.RegularExpressions.Regex.Match(orig, @"(\$?)([A-Za-z]+)(\$?)([1-9][0-9]*)");
                if (match.Success)
                {
                    bool hasColLock = match.Groups[1].Value == "$";
                    string col = match.Groups[2].Value;
                    bool hasRowLock = match.Groups[3].Value == "$";
                    string row = match.Groups[4].Value;

                    // Cycle Excel F4: A1 -> $A$1 -> A$1 -> $A1 -> A1
                    string next;
                    if (!hasColLock && !hasRowLock)       next = $"${col}${row}";
                    else if (hasColLock && hasRowLock)    next = $"{col}${row}";
                    else if (!hasColLock && hasRowLock)   next = $"${col}{row}";
                    else                                  next = $"{col}{row}";

                    _inPlaceEditor.Text = text.Remove(m.Index, m.Length).Insert(m.Index, next);
                    _inPlaceEditor.CaretIndex = m.Index + next.Length;
                    return;
                }
            }
        }
    }

    private void CommitFormulaBarEdit()
    {
        if (ActiveSheet == null || _formulaTextBox == null) return;
        string text = _formulaTextBox.Text?.Trim() ?? string.Empty;
        SetCellValueAndRecalculate(ActiveRow, ActiveColumn, text);
        UpdateSelectionStats();
        _gridPresenter?.InvalidateVisual();
    }

    private void CancelFormulaBarEdit()
    {
        UpdateFormulaBarFromActiveCell();
        _gridPresenter?.InvalidateVisual();
    }

    private void OnFormulaTextBoxKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            CommitFormulaBarEdit();
            Focus();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            CancelFormulaBarEdit();
            Focus();
            e.Handled = true;
        }
    }

    public void SetCellValueAndRecalculate(int row, int col, string input)
    {
        if (ActiveSheet == null) return;

        var cell = ActiveSheet.GetOrCreateCell(row, col);
        if (input.StartsWith("="))
        {
            cell.Formula = input;
            cell.RawValue = input;
        }
        else
        {
            cell.Formula = string.Empty;
            cell.RawValue = input;
        }

        Recalculate();
    }

    public void Recalculate()
    {
        if (ActiveSheet == null) return;
        _formulaEngine.RecalculateSheet(ActiveSheet);
        UpdateSelectionStats();
        _gridPresenter?.InvalidateVisual();
        InvalidateVisual();
    }

    internal void RenderGrid(DrawingContext dc, FrameworkElement presenter)
    {
        if (ActiveSheet == null) return;

        // Dynamic Theme Accent Palette (Defaults to active application Theme, e.g. CONCOST Orange #EA580C)
        var surfaceBrush = Background
            ?? (Brush?)TryFindResource("CONCOST.Brush.Surface")
            ?? Brushes.White;
        var headerNormalBg = HeaderBackground
            ?? (Brush?)TryFindResource("CONCOST.Brush.SurfaceSecondary")
            ?? Brushes.LightGray;
        var headerHoverBg = (Brush?)TryFindResource("CONCOST.Brush.SurfaceHover")
            ?? headerNormalBg;

        // Resolve active theme primary brush
        var themePrimaryBrush = SelectionBorderBrush 
                                ?? (Brush?)TryFindResource("CONCOST.Brush.Primary") 
                                ?? new SolidColorBrush(Color.FromRgb(234, 88, 12));

        Color primaryColor = Colors.DarkOrange;
        if (themePrimaryBrush is SolidColorBrush scb) primaryColor = scb.Color;

        var headerSelectedBg = new SolidColorBrush(Color.FromArgb(24, primaryColor.R, primaryColor.G, primaryColor.B)); // Soft tint of theme primary
        var headerFullSelectedBg = new SolidColorBrush(Color.FromArgb(50, primaryColor.R, primaryColor.G, primaryColor.B)); // Full header selection tint
        
        var borderBrush = GridLineBrush
            ?? (Brush?)TryFindResource("CONCOST.Brush.Border")
            ?? Brushes.LightGray;
        var textPrimaryBrush = Foreground
            ?? (Brush?)TryFindResource("CONCOST.Brush.TextPrimary")
            ?? Brushes.Black;
        var textHeaderBrush = (Brush?)TryFindResource("CONCOST.Brush.TextSecondary")
            ?? Brushes.DimGray;
        var accentBrush = themePrimaryBrush;
        var selRangeBgBrush = SelectionBrush ?? new SolidColorBrush(Color.FromArgb(28, primaryColor.R, primaryColor.G, primaryColor.B));

        var gridPen = new Pen(borderBrush, 0.75); gridPen.Freeze();
        var headerBorderPen = new Pen(borderBrush, 1.0); headerBorderPen.Freeze();
        var accentPen = new Pen(accentBrush, 2.0); accentPen.Freeze();
        var headerAccentPen = new Pen(accentBrush, 2.5); headerAccentPen.Freeze();

        var colOffsets = ActiveSheet.GetColumnOffsets();
        var rowOffsets = ActiveSheet.GetRowOffsets();
        double totalWidth = RowHeaderWidth + colOffsets[ActiveSheet.ColumnCount];
        double totalHeight = ColumnHeaderHeight + rowOffsets[ActiveSheet.RowCount];

        // Draw Canvas Background
        dc.DrawRectangle(surfaceBrush, null, new Rect(0, 0, totalWidth, totalHeight));

        // Determine visible viewport for culling / virtualization
        double vpLeft = _scrollViewer?.HorizontalOffset ?? 0;
        double vpTop = _scrollViewer?.VerticalOffset ?? 0;
        double vpRight = vpLeft + (_scrollViewer?.ViewportWidth > 0 ? _scrollViewer.ViewportWidth : totalWidth);
        double vpBottom = vpTop + (_scrollViewer?.ViewportHeight > 0 ? _scrollViewer.ViewportHeight : totalHeight);

        // Prefix offsets are cached by the sheet; binary search keeps large sheets responsive while scrolling.
        double bodyLeft = Math.Max(0, vpLeft - RowHeaderWidth);
        double bodyTop = Math.Max(0, vpTop - ColumnHeaderHeight);
        double bodyRight = Math.Max(bodyLeft, vpRight - RowHeaderWidth);
        double bodyBottom = Math.Max(bodyTop, vpBottom - ColumnHeaderHeight);
        int minVisCol = Math.Max(0, FindFirstCellEndingAtOrAfter(colOffsets, bodyLeft) - 1);
        int maxVisCol = Math.Min(ActiveSheet.ColumnCount - 1, FindFirstCellStartingAfter(colOffsets, bodyRight));
        int minVisRow = Math.Max(0, FindFirstCellEndingAtOrAfter(rowOffsets, bodyTop) - 1);
        int maxVisRow = Math.Min(ActiveSheet.RowCount - 1, FindFirstCellStartingAfter(rowOffsets, bodyBottom));

        // 1. Draw Column Headers (Virtualized: minVisCol..maxVisCol)
        for (int c = minVisCol; c <= maxVisCol; c++)
        {
            double curX = RowHeaderWidth + colOffsets[c];
            double colW = ActiveSheet.GetColumnWidth(c);
            bool isSelectedCol = c >= SelectionRange.MinCol && c <= SelectionRange.MaxCol;
            bool isHovered = c == _hoveredCol && !_isSelecting;
            bool isFullCol = isSelectedCol && SelectionRange.MinRow == 0 && SelectionRange.MaxRow >= ActiveSheet.RowCount - 1;

            Brush hBg = isFullCol ? headerFullSelectedBg : (isSelectedCol ? headerSelectedBg : (isHovered ? headerHoverBg : headerNormalBg));
            dc.DrawRectangle(hBg, headerBorderPen, new Rect(curX, 0, colW, ColumnHeaderHeight));

            if (isSelectedCol)
                dc.DrawLine(headerAccentPen, new Point(curX, ColumnHeaderHeight - 1), new Point(curX + colW, ColumnHeaderHeight - 1));

            string colName = SpreadsheetAddressHelper.ColumnIndexToName(c);
            var colFg = isSelectedCol ? accentBrush : textHeaderBrush;
            var ft = CreateFormattedText(colName, colFg, 11, isSelectedCol);
            dc.DrawText(ft, new Point(curX + (colW - ft.Width) / 2, (ColumnHeaderHeight - ft.Height) / 2));
        }

        // 2. Draw Row Headers (Virtualized: minVisRow..maxVisRow)
        for (int r = minVisRow; r <= maxVisRow; r++)
        {
            double curY = ColumnHeaderHeight + rowOffsets[r];
            double rowH = ActiveSheet.GetRowHeight(r);
            bool isSelectedRow = r >= SelectionRange.MinRow && r <= SelectionRange.MaxRow;
            bool isHovered = r == _hoveredRow && !_isSelecting;
            bool isFullRow = isSelectedRow && SelectionRange.MinCol == 0 && SelectionRange.MaxCol >= ActiveSheet.ColumnCount - 1;

            Brush rBg = isFullRow ? headerFullSelectedBg : (isSelectedRow ? headerSelectedBg : (isHovered ? headerHoverBg : headerNormalBg));
            dc.DrawRectangle(rBg, headerBorderPen, new Rect(0, curY, RowHeaderWidth, rowH));

            if (isSelectedRow)
                dc.DrawLine(headerAccentPen, new Point(RowHeaderWidth - 2, curY), new Point(RowHeaderWidth - 2, curY + rowH));

            string rowName = (r + 1).ToString();
            var rowFg = isSelectedRow ? accentBrush : textHeaderBrush;
            var ft = CreateFormattedText(rowName, rowFg, 11, isSelectedRow);
            dc.DrawText(ft, new Point(RowHeaderWidth - ft.Width - 6, curY + (rowH - ft.Height) / 2));
        }

        // 3. Top-Left Corner Box (select-all indicator)
        bool allSelected = SelectionRange.MinRow == 0 && SelectionRange.MinCol == 0 &&
                           SelectionRange.MaxRow >= ActiveSheet.RowCount - 1 && SelectionRange.MaxCol >= ActiveSheet.ColumnCount - 1;
        dc.DrawRectangle(allSelected ? headerFullSelectedBg : headerNormalBg, headerBorderPen, new Rect(0, 0, RowHeaderWidth, ColumnHeaderHeight));
        var tri = new StreamGeometry();
        using (var ctx = tri.Open()) { ctx.BeginFigure(new Point(RowHeaderWidth - 8, ColumnHeaderHeight - 2), true, true); ctx.LineTo(new Point(RowHeaderWidth - 2, ColumnHeaderHeight - 2), true, false); ctx.LineTo(new Point(RowHeaderWidth - 2, ColumnHeaderHeight - 8), true, false); }
        dc.DrawGeometry(allSelected ? accentBrush : borderBrush, null, tri);

        // 4. Draw Cells & Grid lines (Virtualized)
        for (int r = minVisRow; r <= maxVisRow; r++)
        {
            double curY = ColumnHeaderHeight + rowOffsets[r];
            double rowH = ActiveSheet.GetRowHeight(r);

            for (int c = minVisCol; c <= maxVisCol; c++)
            {
                double curX = RowHeaderWidth + colOffsets[c];
                double colW = ActiveSheet.GetColumnWidth(c);

                // Skip non-master merged cells
                if (ActiveSheet.IsMergedNonMaster(r, c)) continue;

                var mergedRange = ActiveSheet.FindMergedRangeContaining(r, c);
                Rect cellRect = mergedRange.HasValue ? GetRangeBoundingRect(mergedRange.Value) : new Rect(curX, curY, colW, rowH);
                var cell = ActiveSheet.GetCell(r, c);

                // Resolve cell background dynamically if ResourceKey is specified
                Brush? cellBg = null;
                string? cellBgKey = cell?.BackgroundResourceKey;
                if (cellBgKey is { Length: > 0 })
                {
                    cellBg = (Brush?)TryFindResource(cellBgKey);
                }
                if (cellBg == null)
                {
                    cellBg = cell?.Background;
                }

                // Cell custom background
                if (cellBg != null)
                {
                    dc.DrawRectangle(cellBg, null, cellRect);
                }

                // Grid lines for cell
                dc.DrawRectangle(null, gridPen, cellRect);

                // Cell Text Display (skip active cell text when editing, as _inPlaceEditor renders it)
                if (cell != null && !(_isEditing && r == ActiveRow && c == ActiveColumn))
                {
                    string display = cell.DisplayText;
                    if (!string.IsNullOrEmpty(display))
                    {
                        Brush? fg = null;
                        if (cell.HasError)
                        {
                            fg = (Brush?)TryFindResource("CONCOST.Brush.Danger") ?? Brushes.Red;
                        }
                        else if (!string.IsNullOrEmpty(cell.ForegroundResourceKey))
                        {
                            fg = (Brush?)TryFindResource(cell.ForegroundResourceKey);
                        }

                        if (fg == null)
                        {
                            fg = cell.Foreground;
                        }

                        // If no explicit foreground is set, compute high-contrast text color against cell background
                        if (fg == null)
                        {
                            if (cellBg is SolidColorBrush scbBg && scbBg.Color.A > 40)
                            {
                                double lum = (0.299 * scbBg.Color.R + 0.587 * scbBg.Color.G + 0.114 * scbBg.Color.B) / 255.0;
                                fg = lum > 0.55
                                    ? new SolidColorBrush(Color.FromRgb(15, 23, 42))
                                    : new SolidColorBrush(Color.FromRgb(248, 250, 252));
                            }
                            else
                            {
                                fg = textPrimaryBrush;
                            }
                        }

                        double fontSize = cell.FontSize > 0 ? cell.FontSize : 11.0;
                        var ft = CreateFormattedText(display, fg, fontSize, cell.IsBold, cell.IsItalic);

                        double textX = cellRect.Left + 6;
                        if (cell.HorizontalAlignment == HorizontalAlignment.Right)
                            textX = Math.Max(cellRect.Left + 2, cellRect.Right - ft.Width - 6);
                        else if (cell.HorizontalAlignment == HorizontalAlignment.Center)
                            textX = cellRect.Left + Math.Max(0, (cellRect.Width - ft.Width) / 2);

                        double textY = cellRect.Top + Math.Max(0, (cellRect.Height - ft.Height) / 2);

                        dc.DrawText(ft, new Point(textX, textY));
                        if (cell.IsUnderline)
                        {
                            dc.DrawLine(new Pen(fg, 1), new Point(textX, textY + ft.Height), new Point(textX + ft.Width, textY + ft.Height));
                        }
                    }
                }
            }
        }

        // 5. Multi-Color Formula Range Highlighting (When in edit mode and formula starts with '=')
        if (_isEditing && _inPlaceEditor != null && !string.IsNullOrEmpty(_inPlaceEditor.Text) && _inPlaceEditor.Text.StartsWith("="))
        {
            var highlightPens = new[]
            {
                new Pen(new SolidColorBrush(Color.FromRgb(30, 144, 255)), 1.75), // DodgerBlue
                new Pen(new SolidColorBrush(Color.FromRgb(220, 20, 60)), 1.75),  // Crimson
                new Pen(new SolidColorBrush(Color.FromRgb(147, 51, 234)), 1.75), // Purple
                new Pen(new SolidColorBrush(Color.FromRgb(16, 185, 129)), 1.75), // Emerald
                new Pen(new SolidColorBrush(Color.FromRgb(245, 158, 11)), 1.75)  // Amber
            };
            var highlightFills = new[]
            {
                new SolidColorBrush(Color.FromArgb(28, 30, 144, 255)),
                new SolidColorBrush(Color.FromArgb(28, 220, 20, 60)),
                new SolidColorBrush(Color.FromArgb(28, 147, 51, 234)),
                new SolidColorBrush(Color.FromArgb(28, 16, 185, 129)),
                new SolidColorBrush(Color.FromArgb(28, 245, 158, 11))
            };

            // Find all cell and range references: e.g. A1:B10 or C5
            var formulaStr = _inPlaceEditor.Text;
            var rangeMatches = System.Text.RegularExpressions.Regex.Matches(formulaStr, @"(\$?([A-Za-z]+)\$?([1-9][0-9]*)):(\$?([A-Za-z]+)\$?([1-9][0-9]*))");
            var singleMatches = System.Text.RegularExpressions.Regex.Matches(formulaStr, @"(?<!:)\b(\$?([A-Za-z]+)\$?([1-9][0-9]*))\b(?!:)");

            int colorIdx = 0;
            var highlightedRanges = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (System.Text.RegularExpressions.Match m in rangeMatches)
            {
                try
                {
                    var range = SpreadsheetRange.Parse(m.Value.Replace("$", ""));
                    string key = $"{range.MinRow},{range.MinCol}:{range.MaxRow},{range.MaxCol}";
                    if (!highlightedRanges.Contains(key))
                    {
                        highlightedRanges.Add(key);
                        Rect rRect = GetRangeBoundingRect(range);
                        if (!rRect.IsEmpty)
                        {
                            var pen = highlightPens[colorIdx % highlightPens.Length];
                            var fill = highlightFills[colorIdx % highlightFills.Length];
                            dc.DrawRectangle(fill, pen, rRect);
                            colorIdx++;
                        }
                    }
                }
                catch { }
            }

            foreach (System.Text.RegularExpressions.Match m in singleMatches)
            {
                try
                {
                    var (r, c) = SpreadsheetAddressHelper.ParseAddress(m.Value);
                    string key = $"{r},{c}";
                    if (!highlightedRanges.Contains(key))
                    {
                        highlightedRanges.Add(key);
                        Rect cRect = GetCellBounds(r, c);
                        if (!cRect.IsEmpty)
                        {
                            var pen = highlightPens[colorIdx % highlightPens.Length];
                            var fill = highlightFills[colorIdx % highlightFills.Length];
                            dc.DrawRectangle(fill, pen, cRect);
                            colorIdx++;
                        }
                    }
                }
                catch { }
            }
        }

        // 6. Draw Range Selection
        if (!SelectionRange.IsSingleCell)
        {
            Rect rangeRect = GetRangeBoundingRect(SelectionRange);
            if (!rangeRect.IsEmpty)
            {
                dc.DrawRectangle(selRangeBgBrush, accentPen, rangeRect);

                // Draw AutoFill handle (Accent colored square with 1px white border)
                double handleSize = 6;
                Rect handleRect = new Rect(rangeRect.Right - handleSize / 2, rangeRect.Bottom - handleSize / 2, handleSize, handleSize);
                dc.DrawRectangle(accentBrush, new Pen(Brushes.White, 0.75), handleRect);
            }
        }

        // 7. Draw Active Cell Focus Border (when not in edit mode; in edit mode, _inPlaceEditor provides the border)
        if (!_isEditing)
        {
            var activeMerged = ActiveSheet.FindMergedRangeContaining(ActiveRow, ActiveColumn);
            Rect activeBounds = activeMerged.HasValue ? GetRangeBoundingRect(activeMerged.Value) : GetCellBounds(ActiveRow, ActiveColumn);
            if (!activeBounds.IsEmpty)
            {
                dc.DrawRectangle(null, accentPen, activeBounds);

                // AutoFill handle on active cell if single selection
                if (SelectionRange.IsSingleCell)
                {
                    double handleSize = 6;
                    Rect handleRect = new Rect(activeBounds.Right - handleSize / 2, activeBounds.Bottom - handleSize / 2, handleSize, handleSize);
                    dc.DrawRectangle(accentBrush, new Pen(Brushes.White, 0.75), handleRect);
                }
            }
        }
    }

    private Rect GetRangeBoundingRect(SpreadsheetRange range)
    {
        if (ActiveSheet == null) return Rect.Empty;

        var startRect = GetCellBounds(range.MinRow, range.MinCol);
        var endRect = GetCellBounds(range.MaxRow, range.MaxCol);

        return new Rect(startRect.TopLeft, endRect.BottomRight);
    }

    private FormattedText CreateFormattedText(string text, Brush brush, double size = 11, bool bold = false, bool italic = false)
    {
        var style = italic ? FontStyles.Italic : FontStyles.Normal;
        var weight = bold ? FontWeights.SemiBold : FontWeights.Normal;
        var typeface = new Typeface(new FontFamily("Segoe UI, Inter, Arial"), style, weight, FontStretches.Normal);
        return new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, Math.Max(8, size), brush, 1.0);
    }

    private void ApplyFormatting(Action<SpreadsheetCell> action)
    {
        if (ActiveSheet == null) return;

        for (int r = SelectionRange.MinRow; r <= SelectionRange.MaxRow; r++)
        {
            for (int c = SelectionRange.MinCol; c <= SelectionRange.MaxCol; c++)
            {
                var cell = ActiveSheet.GetOrCreateCell(r, c);
                action(cell);
            }
        }
        _gridPresenter?.InvalidateVisual();
        InvalidateVisual();
    }

    private void ExecuteMergeCells()
    {
        if (ActiveSheet == null || SelectionRange.IsSingleCell) return;
        ActiveSheet.MergeRange(SelectionRange);

        // Center master cell content
        var masterCell = ActiveSheet.GetOrCreateCell(SelectionRange.MinRow, SelectionRange.MinCol);
        masterCell.HorizontalAlignment = HorizontalAlignment.Center;

        _gridPresenter?.InvalidateVisual();
        InvalidateVisual();
    }

    private void ExecuteUnmergeCells()
    {
        if (ActiveSheet == null) return;
        ActiveSheet.UnmergeRange(SelectionRange);
        _gridPresenter?.InvalidateVisual();
        InvalidateVisual();
    }

    private void ExecuteInsertRow()
    {
        if (ActiveSheet == null) return;
        PushUndo();
        // Shift all rows down from ActiveRow
        int insertAt = SelectionRange.MinRow;
        for (int r = ActiveSheet.RowCount - 1; r >= insertAt; r--)
            for (int c = 0; c < ActiveSheet.ColumnCount; c++)
            {
                var src = ActiveSheet.GetCell(r, c);
                if (src != null) { var dst = ActiveSheet.GetOrCreateCell(r + 1, c); dst.RawValue = src.RawValue; dst.Formula = src.Formula; dst.IsBold = src.IsBold; dst.IsItalic = src.IsItalic; dst.HorizontalAlignment = src.HorizontalAlignment; dst.Format = src.Format; dst.Background = src.Background; dst.Foreground = src.Foreground; }
                ActiveSheet.ClearCell(r, c);
                if (src == null) ActiveSheet.ClearCell(r + 1, c);
            }
        ActiveSheet.RowCount += 1;
        Recalculate();
        _gridPresenter?.InvalidateMeasure();
        _gridPresenter?.InvalidateVisual();
    }

    private void ExecuteDeleteRow()
    {
        if (ActiveSheet == null || ActiveSheet.RowCount <= 1) return;
        PushUndo();
        int deleteAt = SelectionRange.MinRow;
        // Shift rows up
        for (int r = deleteAt; r < ActiveSheet.RowCount - 1; r++)
            for (int c = 0; c < ActiveSheet.ColumnCount; c++)
            {
                var src = ActiveSheet.GetCell(r + 1, c);
                if (src != null) { var dst = ActiveSheet.GetOrCreateCell(r, c); dst.RawValue = src.RawValue; dst.Formula = src.Formula; dst.IsBold = src.IsBold; dst.IsItalic = src.IsItalic; dst.HorizontalAlignment = src.HorizontalAlignment; dst.Format = src.Format; dst.Background = src.Background; dst.Foreground = src.Foreground; }
                else ActiveSheet.ClearCell(r, c);
            }
        // Clear last row
        for (int c = 0; c < ActiveSheet.ColumnCount; c++) ActiveSheet.ClearCell(ActiveSheet.RowCount - 1, c);
        ActiveSheet.RowCount = Math.Max(1, ActiveSheet.RowCount - 1);
        if (ActiveRow >= ActiveSheet.RowCount) ActiveRow = ActiveSheet.RowCount - 1;
        SelectionRange = new SpreadsheetRange(Math.Min(SelectionRange.MinRow, ActiveSheet.RowCount - 1), SelectionRange.MinCol, Math.Min(SelectionRange.MaxRow, ActiveSheet.RowCount - 1), SelectionRange.MaxCol);
        Recalculate();
        _gridPresenter?.InvalidateMeasure();
        _gridPresenter?.InvalidateVisual();
    }

    private void ExecuteInsertColumn()
    {
        if (ActiveSheet == null) return;
        PushUndo();
        int insertAt = SelectionRange.MinCol;
        for (int c = ActiveSheet.ColumnCount - 1; c >= insertAt; c--)
            for (int r = 0; r < ActiveSheet.RowCount; r++)
            {
                var src = ActiveSheet.GetCell(r, c);
                if (src != null) { var dst = ActiveSheet.GetOrCreateCell(r, c + 1); dst.RawValue = src.RawValue; dst.Formula = src.Formula; dst.IsBold = src.IsBold; dst.IsItalic = src.IsItalic; dst.HorizontalAlignment = src.HorizontalAlignment; dst.Format = src.Format; dst.Background = src.Background; dst.Foreground = src.Foreground; }
                ActiveSheet.ClearCell(r, c);
                if (src == null) ActiveSheet.ClearCell(r, c + 1);
            }
        ActiveSheet.ColumnCount += 1;
        Recalculate();
        _gridPresenter?.InvalidateMeasure();
        _gridPresenter?.InvalidateVisual();
    }

    private void ExecuteDeleteColumn()
    {
        if (ActiveSheet == null || ActiveSheet.ColumnCount <= 1) return;
        PushUndo();
        int deleteAt = SelectionRange.MinCol;
        for (int c = deleteAt; c < ActiveSheet.ColumnCount - 1; c++)
            for (int r = 0; r < ActiveSheet.RowCount; r++)
            {
                var src = ActiveSheet.GetCell(r, c + 1);
                if (src != null) { var dst = ActiveSheet.GetOrCreateCell(r, c); dst.RawValue = src.RawValue; dst.Formula = src.Formula; dst.IsBold = src.IsBold; dst.IsItalic = src.IsItalic; dst.HorizontalAlignment = src.HorizontalAlignment; dst.Format = src.Format; dst.Background = src.Background; dst.Foreground = src.Foreground; }
                else ActiveSheet.ClearCell(r, c);
            }
        for (int r = 0; r < ActiveSheet.RowCount; r++) ActiveSheet.ClearCell(r, ActiveSheet.ColumnCount - 1);
        ActiveSheet.ColumnCount = Math.Max(1, ActiveSheet.ColumnCount - 1);
        if (ActiveColumn >= ActiveSheet.ColumnCount) ActiveColumn = ActiveSheet.ColumnCount - 1;
        SelectionRange = new SpreadsheetRange(SelectionRange.MinRow, Math.Min(SelectionRange.MinCol, ActiveSheet.ColumnCount - 1), SelectionRange.MaxRow, Math.Min(SelectionRange.MaxCol, ActiveSheet.ColumnCount - 1));
        Recalculate();
        _gridPresenter?.InvalidateMeasure();
        _gridPresenter?.InvalidateVisual();
    }

    public void AutoFitColumn(int columnIndex)
    {
        if (ActiveSheet == null || columnIndex < 0 || columnIndex >= ActiveSheet.ColumnCount) return;

        double maxW = 50.0; // Minimum column width
        var typeface = new Typeface(FontFamily ?? new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        double pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;

        // Header text width
        string colHeader = SpreadsheetAddressHelper.ColumnIndexToName(columnIndex);
        var headerFt = new FormattedText(colHeader, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 11, Brushes.Black, pixelsPerDip);
        maxW = Math.Max(maxW, headerFt.Width + 24);

        for (int r = 0; r < ActiveSheet.RowCount; r++)
        {
            var cell = ActiveSheet.GetCell(r, columnIndex);
            if (cell != null && !string.IsNullOrEmpty(cell.DisplayText))
            {
                var cellTypeface = new Typeface(FontFamily ?? new FontFamily("Segoe UI"), 
                    cell.IsItalic ? FontStyles.Italic : FontStyles.Normal,
                    cell.IsBold ? FontWeights.Bold : FontWeights.Normal,
                    FontStretches.Normal);
                double fSize = cell.FontSize > 0 ? cell.FontSize : 11.0;
                var ft = new FormattedText(cell.DisplayText, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, cellTypeface, fSize, Brushes.Black, pixelsPerDip);
                maxW = Math.Max(maxW, ft.Width + 18);
            }
        }

        ActiveSheet.SetColumnWidth(columnIndex, Math.Min(500.0, Math.Ceiling(maxW)));
        _gridPresenter?.InvalidateMeasure();
        _gridPresenter?.InvalidateVisual();
        InvalidateVisual();
    }

    public void AutoFitRow(int rowIndex)
    {
        if (ActiveSheet == null || rowIndex < 0 || rowIndex >= ActiveSheet.RowCount) return;

        double maxH = 20.0; // Minimum row height
        var typeface = new Typeface(FontFamily ?? new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        double pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;

        for (int c = 0; c < ActiveSheet.ColumnCount; c++)
        {
            var cell = ActiveSheet.GetCell(rowIndex, c);
            if (cell != null && !string.IsNullOrEmpty(cell.DisplayText))
            {
                double fSize = cell.FontSize > 0 ? cell.FontSize : 11.0;
                var ft = new FormattedText(cell.DisplayText, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, fSize, Brushes.Black, pixelsPerDip);
                maxH = Math.Max(maxH, ft.Height + 8);
            }
        }

        ActiveSheet.SetRowHeight(rowIndex, Math.Min(200.0, Math.Ceiling(maxH)));
        _gridPresenter?.InvalidateMeasure();
        _gridPresenter?.InvalidateVisual();
        InvalidateVisual();
    }

    private void ExecuteExportCsv()
    {
        if (ActiveSheet == null) return;
        try
        {
            var sfd = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                FileName = $"{ActiveSheet.Name}.csv"
            };
            if (sfd.ShowDialog() == true)
            {
                string csv = ActiveSheet.ExportToCsv();
                File.WriteAllText(sfd.FileName, csv, Encoding.UTF8);
            }
        }
        catch { }
    }

    private void ExecuteImportCsv()
    {
        if (ActiveSheet == null) return;
        try
        {
            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*"
            };
            if (ofd.ShowDialog() == true)
            {
                string csv = File.ReadAllText(ofd.FileName, Encoding.UTF8);
                ActiveSheet.ImportFromCsv(csv);
                Recalculate();
                _gridPresenter?.InvalidateMeasure();
                _gridPresenter?.InvalidateVisual();
            }
        }
        catch { }
    }

    private void ExecuteCut()
    {
        PushUndo();
        ExecuteCopy();
        ExecuteClear();
    }

    private void ExecuteCopy()
    {
        if (ActiveSheet == null) return;

        var sb = new StringBuilder();
        for (int r = SelectionRange.MinRow; r <= SelectionRange.MaxRow; r++)
        {
            var rowValues = new List<string>();
            for (int c = SelectionRange.MinCol; c <= SelectionRange.MaxCol; c++)
            {
                var cell = ActiveSheet.GetCell(r, c);
                string val = cell != null ? (!string.IsNullOrEmpty(cell.Formula) ? cell.Formula : cell.RawValue) : string.Empty;
                rowValues.Add(val);
            }
            sb.AppendLine(string.Join("\t", rowValues));
        }

        try
        {
            _lastCopiedText = sb.ToString();
            _lastCopyStartRow = SelectionRange.MinRow;
            _lastCopyStartColumn = SelectionRange.MinCol;
            Clipboard.SetText(_lastCopiedText);
        }
        catch { }
    }

    private void ExecutePaste()
    {
        if (IsReadOnly || ActiveSheet == null) return;

        try
        {
            if (!Clipboard.ContainsText()) return;
            string clip = Clipboard.GetText();
            var lines = clip.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            if (lines.Length > 1 && lines[^1].Length == 0)
                lines = lines.Take(lines.Length - 1).ToArray();
            if (lines.Length == 0) return;

            var rows = lines.Select(line => line.Split('\t')).ToArray();
            int pastedColumnCount = rows.Max(row => row.Length);
            if (pastedColumnCount == 0) return;

            int startRow = ActiveRow;
            int startCol = ActiveColumn;
            bool isInternalCopy = string.Equals(clip, _lastCopiedText, StringComparison.Ordinal);
            int rowOffset = isInternalCopy ? startRow - _lastCopyStartRow : 0;
            int columnOffset = isInternalCopy ? startCol - _lastCopyStartColumn : 0;

            PushUndo();
            ActiveSheet.RowCount = Math.Max(ActiveSheet.RowCount, startRow + rows.Length);
            ActiveSheet.ColumnCount = Math.Max(ActiveSheet.ColumnCount, startCol + pastedColumnCount);

            for (int r = 0; r < rows.Length; r++)
            {
                var cols = rows[r];
                for (int c = 0; c < cols.Length; c++)
                {
                    int targetRow = startRow + r;
                    int targetCol = startCol + c;
                    if (targetRow >= 0 && targetCol >= 0)
                    {
                        var cell = ActiveSheet.GetOrCreateCell(targetRow, targetCol);
                        string val = cols[c];
                        if (isInternalCopy && val.StartsWith("=", StringComparison.Ordinal))
                            val = SpreadsheetAddressHelper.TranslateFormulaReferences(val, rowOffset, columnOffset);
                        if (val.StartsWith("="))
                        {
                            cell.Formula = val;
                            cell.RawValue = val;
                        }
                        else
                        {
                            cell.Formula = string.Empty;
                            cell.RawValue = val;
                        }
                    }
                }
            }

            SelectionRange = new SpreadsheetRange(startRow, startCol,
                startRow + rows.Length - 1, startCol + pastedColumnCount - 1);
            Recalculate();
        }
        catch { }
    }

    // ── Undo / Redo ────────────────────────────────────────────
    private void PushUndo()
    {
        if (ActiveSheet == null) return;
        _undoStack.Push(new SheetSnapshot(ActiveSheet, ActiveRow, ActiveColumn, SelectionRange));
        if (_undoStack.Count > 100) { var arr = _undoStack.ToArray(); _undoStack.Clear(); for (int i = Math.Min(arr.Length - 1, 99); i >= 0; i--) _undoStack.Push(arr[i]); }
        _redoStack.Clear();
    }

    private void RestoreSnapshot(SheetSnapshot snap)
    {
        if (ActiveSheet == null) return;
        // Clear all cells first
        ActiveSheet.RowCount = snap.RowCount;
        ActiveSheet.ColumnCount = snap.ColumnCount;
        foreach (var cell in ActiveSheet.GetAllCells().ToList()) ActiveSheet.ClearCell(cell.Row, cell.Column);
        // Restore
        foreach (var kv in snap.Cells)
        {
            var c = ActiveSheet.GetOrCreateCell(kv.Key.Item1, kv.Key.Item2);
            c.RawValue = kv.Value.raw; c.Formula = kv.Value.formula;
            c.IsBold = kv.Value.bold; c.IsItalic = kv.Value.italic; c.IsUnderline = kv.Value.underline;
            c.HorizontalAlignment = kv.Value.align; c.Format = kv.Value.fmt;
        }
        ActiveRow = snap.ActiveRow; ActiveColumn = snap.ActiveCol;
        SelectionRange = snap.Selection;
        Recalculate();
    }

    private void ExecuteUndo()
    {
        if (ActiveSheet == null || _undoStack.Count == 0) return;
        _redoStack.Push(new SheetSnapshot(ActiveSheet, ActiveRow, ActiveColumn, SelectionRange));
        RestoreSnapshot(_undoStack.Pop());
    }

    private void ExecuteRedo()
    {
        if (ActiveSheet == null || _redoStack.Count == 0) return;
        _undoStack.Push(new SheetSnapshot(ActiveSheet, ActiveRow, ActiveColumn, SelectionRange));
        RestoreSnapshot(_redoStack.Pop());
    }

    // ─────────────────────────────────────────────────────────────

    private void ExecuteClear()
    {
        if (IsReadOnly || ActiveSheet == null) return;

        ActiveSheet.ClearRange(SelectionRange);
        Recalculate();
    }

    private void ExecuteAutoSum()
    {
        if (IsReadOnly || ActiveSheet == null) return;

        int targetRow = SelectionRange.MaxRow + 1;
        if (targetRow >= ActiveSheet.RowCount) return;

        for (int c = SelectionRange.MinCol; c <= SelectionRange.MaxCol; c++)
        {
            string startAddr = SpreadsheetAddressHelper.ToAddress(SelectionRange.MinRow, c);
            string endAddr = SpreadsheetAddressHelper.ToAddress(SelectionRange.MaxRow, c);
            string formula = $"=SUM({startAddr}:{endAddr})";

            var targetCell = ActiveSheet.GetOrCreateCell(targetRow, c);
            targetCell.Formula = formula;
            targetCell.RawValue = formula;
            targetCell.IsBold = true;
        }

        Recalculate();
    }

    private void ExecuteAddSheet()
    {
        string newName = $"Sheet{Sheets.Count + 1}";
        var newSheet = new SpreadsheetSheet { Name = newName };
        Sheets.Add(newSheet);
        ActiveSheet = newSheet;
    }

    private void ExecuteDeleteSheet()
    {
        if (Sheets.Count <= 1 || ActiveSheet == null) return;
        int idx = Sheets.IndexOf(ActiveSheet);
        Sheets.Remove(ActiveSheet);
        ActiveSheet = Sheets[NumericCompatibility.Clamp(idx, 0, Sheets.Count - 1)];
    }

    private void UpdateFormulaBarFromActiveCell()
    {
        if (ActiveSheet == null) return;
        var cell = ActiveSheet.GetCell(ActiveRow, ActiveColumn);
        FormulaText = cell != null ? (!string.IsNullOrEmpty(cell.Formula) ? cell.Formula : cell.RawValue) : string.Empty;
        if (_formulaTextBox != null) _formulaTextBox.Text = FormulaText;
    }

    private void UpdateCellAddressDisplay()
    {
        if (_cellAddressTextBlock != null)
        {
            _cellAddressTextBlock.Text = SpreadsheetAddressHelper.ToAddress(ActiveRow, ActiveColumn);
        }
    }

    private void UpdateSelectionStats()
    {
        if (_selectionStatsTextBlock == null || ActiveSheet == null) return;

        if (SelectionRange.IsSingleCell)
        {
            _selectionStatsTextBlock.Text = "Ready";
            return;
        }

        double sum = 0.0;
        int numCount = 0;
        int totalNonEmpty = 0;

        foreach (var cell in ActiveSheet.GetAllCells())
        {
            if (!SelectionRange.Contains(cell.Row, cell.Column) || string.IsNullOrEmpty(cell.DisplayText))
                continue;

            totalNonEmpty++;
            if (cell.ComputedValue is double d) { sum += d; numCount++; }
            else if (double.TryParse(cell.RawValue, out double rd)) { sum += rd; numCount++; }
        }

        if (numCount > 0)
        {
            double avg = sum / numCount;
            _selectionStatsTextBlock.Text = $"Count: {totalNonEmpty} | Numerical: {numCount} | Average: {avg:N2} | Sum: {sum:N2}";
        }
        else
        {
            _selectionStatsTextBlock.Text = $"Count: {totalNonEmpty}";
        }
    }

    private static void OnActiveSheetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Spreadsheet sp)
        {
            if (sp._observedSheet != null)
                sp._observedSheet.PropertyChanged -= sp.OnActiveSheetPropertyChanged;

            if (e.NewValue is SpreadsheetSheet newSheet && !sp.Sheets.Contains(newSheet))
            {
                sp.Sheets.Add(newSheet);
            }
            sp._observedSheet = e.NewValue as SpreadsheetSheet;
            if (sp._observedSheet != null)
                sp._observedSheet.PropertyChanged += sp.OnActiveSheetPropertyChanged;

            sp.Recalculate();
            sp._scrollViewer?.InvalidateMeasure();
            sp._gridPresenter?.InvalidateMeasure();
            sp.UpdateFormulaBarFromActiveCell();
            sp._gridPresenter?.InvalidateMeasure();
            sp._gridPresenter?.InvalidateArrange();
            sp._gridPresenter?.InvalidateVisual();
            sp.InvalidateVisual();
        }
    }

    private void OnActiveSheetPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not (nameof(SpreadsheetSheet.RowCount)
            or nameof(SpreadsheetSheet.ColumnCount)
            or nameof(SpreadsheetSheet.DefaultRowHeight)
            or nameof(SpreadsheetSheet.DefaultColumnWidth)
            or "RowHeight"
            or "ColumnWidth"))
            return;

        if (ActiveSheet != null)
        {
            ActiveRow = NumericCompatibility.Clamp(ActiveRow, 0, ActiveSheet.RowCount - 1);
            ActiveColumn = NumericCompatibility.Clamp(ActiveColumn, 0, ActiveSheet.ColumnCount - 1);
        }

        _scrollViewer?.InvalidateMeasure();
        _gridPresenter?.InvalidateMeasure();
        _gridPresenter?.InvalidateVisual();
    }

    private static void OnSelectionRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Spreadsheet sp)
        {
            sp.UpdateSelectionStats();
            sp._gridPresenter?.InvalidateVisual();
            sp.InvalidateVisual();
        }
    }

    private static void OnActiveCellCoordsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Spreadsheet sp)
        {
            sp.UpdateCellAddressDisplay();
            sp.UpdateFormulaBarFromActiveCell();
            sp._gridPresenter?.InvalidateVisual();
            sp.InvalidateVisual();
        }
    }

    private static void OnFormulaTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Spreadsheet sp && sp._formulaTextBox != null && !sp._formulaTextBox.IsFocused)
        {
            sp._formulaTextBox.Text = (string)e.NewValue ?? string.Empty;
        }
    }

    private ContextMenu CreateGridContextMenu()
    {
        var menu = new ContextMenu();

        var miCut = new MenuItem { Header = "Cut", InputGestureText = "Ctrl+X" };
        miCut.Click += (s, e) => ExecuteCut();
        menu.Items.Add(miCut);

        var miCopy = new MenuItem { Header = "Copy", InputGestureText = "Ctrl+C" };
        miCopy.Click += (s, e) => ExecuteCopy();
        menu.Items.Add(miCopy);

        var miPaste = new MenuItem { Header = "Paste", InputGestureText = "Ctrl+V" };
        miPaste.Click += (s, e) => ExecutePaste();
        menu.Items.Add(miPaste);

        menu.Items.Add(new Separator());

        var miInsertRow = new MenuItem { Header = "Insert Row" };
        miInsertRow.Click += (s, e) => ExecuteInsertRow();
        menu.Items.Add(miInsertRow);

        var miInsertCol = new MenuItem { Header = "Insert Column" };
        miInsertCol.Click += (s, e) => ExecuteInsertColumn();
        menu.Items.Add(miInsertCol);

        var miDeleteRow = new MenuItem { Header = "Delete Row" };
        miDeleteRow.Click += (s, e) => ExecuteDeleteRow();
        menu.Items.Add(miDeleteRow);

        var miDeleteCol = new MenuItem { Header = "Delete Column" };
        miDeleteCol.Click += (s, e) => ExecuteDeleteColumn();
        menu.Items.Add(miDeleteCol);

        menu.Items.Add(new Separator());

        var miClear = new MenuItem { Header = "Clear Contents", InputGestureText = "Delete" };
        miClear.Click += (s, e) => { PushUndo(); ExecuteClear(); };
        menu.Items.Add(miClear);

        var miAutoSum = new MenuItem { Header = "AutoSum (∑)" };
        miAutoSum.Click += (s, e) => { PushUndo(); ExecuteAutoSum(); };
        menu.Items.Add(miAutoSum);

        var miMerge = new MenuItem { Header = "Merge / Unmerge Cells", InputGestureText = "Ctrl+M" };
        miMerge.Click += (s, e) =>
        {
            PushUndo();
            if (ActiveSheet != null && ActiveSheet.FindMergedRangeContaining(SelectionRange.MinRow, SelectionRange.MinCol).HasValue)
                ExecuteUnmergeCells();
            else
                ExecuteMergeCells();
        };
        menu.Items.Add(miMerge);

        return menu;
    }

    private sealed class FormulaItem
    {
        public string Name { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
        public string Syntax { get; set; } = string.Empty;
    }
}
