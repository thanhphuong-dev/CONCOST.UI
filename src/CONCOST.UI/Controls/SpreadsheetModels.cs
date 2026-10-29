using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

public enum SpreadsheetCellFormat
{
    General,
    Number,
    Currency,
    Percentage,
    Date,
    ShortDate,
    Scientific
}

public class SpreadsheetCell : INotifyPropertyChanged
{
    private string _rawValue = string.Empty;
    private object? _computedValue;
    private string _formula = string.Empty;
    private SpreadsheetCellFormat _format = SpreadsheetCellFormat.General;
    private string _customFormat = string.Empty;
    private bool _isBold;
    private bool _isItalic;
    private bool _isUnderline;
    private HorizontalAlignment _horizontalAlignment = HorizontalAlignment.Left;
    private Brush? _foreground;
    private Brush? _background;
    private string? _backgroundResourceKey;
    private string? _foregroundResourceKey;
    private double _fontSize = 12.0;
    private bool _isReadOnly;
    private bool _hasError;
    private string _errorMessage = string.Empty;

    public int Row { get; set; }
    public int Column { get; set; }

    public string RawValue
    {
        get => _rawValue;
        set
        {
            if (_rawValue != value)
            {
                _rawValue = value ?? string.Empty;
                OnPropertyChanged(nameof(RawValue));
                OnPropertyChanged(nameof(DisplayText));
            }
        }
    }

    public string Formula
    {
        get => _formula;
        set
        {
            if (_formula != value)
            {
                _formula = value ?? string.Empty;
                OnPropertyChanged(nameof(Formula));
            }
        }
    }

    public object? ComputedValue
    {
        get => _computedValue;
        set
        {
            if (!Equals(_computedValue, value))
            {
                _computedValue = value;
                OnPropertyChanged(nameof(ComputedValue));
                OnPropertyChanged(nameof(DisplayText));
            }
        }
    }

    public SpreadsheetCellFormat Format
    {
        get => _format;
        set
        {
            if (_format != value)
            {
                _format = value;
                OnPropertyChanged(nameof(Format));
                OnPropertyChanged(nameof(DisplayText));
            }
        }
    }

    public string CustomFormat
    {
        get => _customFormat;
        set
        {
            if (_customFormat != value)
            {
                _customFormat = value;
                OnPropertyChanged(nameof(CustomFormat));
                OnPropertyChanged(nameof(DisplayText));
            }
        }
    }

    public bool IsBold
    {
        get => _isBold;
        set { if (_isBold != value) { _isBold = value; OnPropertyChanged(nameof(IsBold)); } }
    }

    public bool IsItalic
    {
        get => _isItalic;
        set { if (_isItalic != value) { _isItalic = value; OnPropertyChanged(nameof(IsItalic)); } }
    }

    public bool IsUnderline
    {
        get => _isUnderline;
        set { if (_isUnderline != value) { _isUnderline = value; OnPropertyChanged(nameof(IsUnderline)); } }
    }

    public HorizontalAlignment HorizontalAlignment
    {
        get => _horizontalAlignment;
        set { if (_horizontalAlignment != value) { _horizontalAlignment = value; OnPropertyChanged(nameof(HorizontalAlignment)); } }
    }

    public Brush? Foreground
    {
        get => _foreground;
        set { if (_foreground != value) { _foreground = value; OnPropertyChanged(nameof(Foreground)); } }
    }

    public Brush? Background
    {
        get => _background;
        set { if (_background != value) { _background = value; OnPropertyChanged(nameof(Background)); } }
    }

    public string? BackgroundResourceKey
    {
        get => _backgroundResourceKey;
        set { if (_backgroundResourceKey != value) { _backgroundResourceKey = value; OnPropertyChanged(nameof(BackgroundResourceKey)); } }
    }

    public string? ForegroundResourceKey
    {
        get => _foregroundResourceKey;
        set { if (_foregroundResourceKey != value) { _foregroundResourceKey = value; OnPropertyChanged(nameof(ForegroundResourceKey)); } }
    }

    public double FontSize
    {
        get => _fontSize;
        set { if (_fontSize != value) { _fontSize = value; OnPropertyChanged(nameof(FontSize)); } }
    }

    public bool IsReadOnly
    {
        get => _isReadOnly;
        set { if (_isReadOnly != value) { _isReadOnly = value; OnPropertyChanged(nameof(IsReadOnly)); } }
    }

    public bool HasError
    {
        get => _hasError;
        set { if (_hasError != value) { _hasError = value; OnPropertyChanged(nameof(HasError)); } }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set { if (_errorMessage != value) { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); } }
    }

    public string DisplayText
    {
        get
        {
            if (HasError) return string.IsNullOrEmpty(ErrorMessage) ? "#ERROR!" : ErrorMessage;

            object? val = ComputedValue ?? RawValue;
            if (val == null) return string.Empty;

            // If raw value is a string representation of number and format is numeric/currency
            if (val is string s && (Format == SpreadsheetCellFormat.Number || Format == SpreadsheetCellFormat.Currency || Format == SpreadsheetCellFormat.Percentage || Format == SpreadsheetCellFormat.Scientific))
            {
                if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedD))
                {
                    val = parsedD;
                }
                else if (double.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out double parsedDc))
                {
                    val = parsedDc;
                }
            }

            if (!string.IsNullOrEmpty(CustomFormat))
            {
                if (val is IFormattable formattable)
                {
                    try { return formattable.ToString(CustomFormat, CultureInfo.InvariantCulture); }
                    catch { }
                }
            }

            if (val is double d)
            {
                return Format switch
                {
                    SpreadsheetCellFormat.Number => d.ToString("N2", CultureInfo.InvariantCulture),
                    SpreadsheetCellFormat.Currency => FormatCurrency(d),
                    SpreadsheetCellFormat.Percentage => (d * 100).ToString("0.##", CultureInfo.InvariantCulture) + "%",
                    SpreadsheetCellFormat.Scientific => d.ToString("E2", CultureInfo.InvariantCulture),
                    _ => d.ToString("G", CultureInfo.InvariantCulture)
                };
            }

            if (val is int i)
            {
                return Format switch
                {
                    SpreadsheetCellFormat.Number => i.ToString("N0", CultureInfo.InvariantCulture),
                    SpreadsheetCellFormat.Currency => FormatCurrency(i),
                    SpreadsheetCellFormat.Percentage => (i * 100).ToString("0", CultureInfo.InvariantCulture) + "%",
                    _ => i.ToString(CultureInfo.InvariantCulture)
                };
            }

            if (val is DateTime dt)
            {
                return Format switch
                {
                    SpreadsheetCellFormat.ShortDate => dt.ToString("d", CultureInfo.InvariantCulture),
                    _ => dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                };
            }

            return val.ToString() ?? string.Empty;
        }
    }

    private static string FormatCurrency(double d)
    {
        bool isNegative = d < 0;
        double abs = Math.Abs(d);
        bool hasDecimals = Math.Abs(abs - Math.Round(abs)) > 0.001;
        string numStr = hasDecimals 
            ? abs.ToString("N2", CultureInfo.InvariantCulture) 
            : abs.ToString("N0", CultureInfo.InvariantCulture);
        return isNegative ? $"-${numStr}" : $"${numStr}";
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}

public struct SpreadsheetRange : IEquatable<SpreadsheetRange>
{
    public int StartRow { get; }
    public int StartCol { get; }
    public int EndRow { get; }
    public int EndCol { get; }

    public int MinRow => Math.Min(StartRow, EndRow);
    public int MaxRow => Math.Max(StartRow, EndRow);
    public int MinCol => Math.Min(StartCol, EndCol);
    public int MaxCol => Math.Max(StartCol, EndCol);

    public int RowCount => MaxRow - MinRow + 1;
    public int ColCount => MaxCol - MinCol + 1;
    public int TotalCells => RowCount * ColCount;
    public bool IsSingleCell => RowCount == 1 && ColCount == 1;

    public SpreadsheetRange(int startRow, int startCol, int endRow, int endCol)
    {
        StartRow = Math.Max(0, startRow);
        StartCol = Math.Max(0, startCol);
        EndRow = Math.Max(0, endRow);
        EndCol = Math.Max(0, endCol);
    }

    public SpreadsheetRange(int row, int col) : this(row, col, row, col) { }

    public bool Contains(int row, int col)
    {
        return row >= MinRow && row <= MaxRow && col >= MinCol && col <= MaxCol;
    }

    public string ToAddress()
    {
        string start = SpreadsheetAddressHelper.ToAddress(MinRow, MinCol);
        if (IsSingleCell) return start;
        string end = SpreadsheetAddressHelper.ToAddress(MaxRow, MaxCol);
        return $"{start}:{end}";
    }

    public static SpreadsheetRange Parse(string rangeStr)
    {
        if (string.IsNullOrWhiteSpace(rangeStr)) return new SpreadsheetRange(0, 0);
        rangeStr = rangeStr.Trim().ToUpperInvariant();
        if (rangeStr.Contains(':'))
        {
            var parts = rangeStr.Split(':');
            var (r1, c1) = SpreadsheetAddressHelper.ParseAddress(parts[0]);
            var (r2, c2) = SpreadsheetAddressHelper.ParseAddress(parts[1]);
            return new SpreadsheetRange(r1, c1, r2, c2);
        }
        else
        {
            var (r, c) = SpreadsheetAddressHelper.ParseAddress(rangeStr);
            return new SpreadsheetRange(r, c);
        }
    }

    public bool Equals(SpreadsheetRange other) =>
        StartRow == other.StartRow && StartCol == other.StartCol && EndRow == other.EndRow && EndCol == other.EndCol;

    public override bool Equals(object? obj) => obj is SpreadsheetRange other && Equals(other);
    public override int GetHashCode()
    {
        unchecked
        {
            var hash = StartRow;
            hash = (hash * 397) ^ StartCol;
            hash = (hash * 397) ^ EndRow;
            return (hash * 397) ^ EndCol;
        }
    }
    public static bool operator ==(SpreadsheetRange left, SpreadsheetRange right) => left.Equals(right);
    public static bool operator !=(SpreadsheetRange left, SpreadsheetRange right) => !left.Equals(right);
}

public static class SpreadsheetAddressHelper
{
    private static readonly Regex FormulaReferenceRegex = new(
        "(?<string>\"(?:[^\"]|\"\")*\")|(?<![A-Za-z0-9_.])(?<columnLock>\\$?)(?<column>[A-Za-z]+)(?<rowLock>\\$?)(?<row>[1-9][0-9]*)(?![A-Za-z0-9_])(?!\\s*\\()",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static string ColumnIndexToName(int columnIndex)
    {
        columnIndex++; // 1-based
        string columnName = string.Empty;
        while (columnIndex > 0)
        {
            int modulo = (columnIndex - 1) % 26;
            columnName = Convert.ToChar('A' + modulo) + columnName;
            columnIndex = (columnIndex - modulo) / 26;
        }
        return columnName;
    }

    public static int ColumnNameToIndex(string columnName)
    {
        if (string.IsNullOrEmpty(columnName)) return 0;
        columnName = columnName.ToUpperInvariant();
        int sum = 0;
        foreach (char c in columnName)
        {
            if (c < 'A' || c > 'Z') break;
            sum *= 26;
            sum += (c - 'A' + 1);
        }
        return Math.Max(0, sum - 1);
    }

    public static string ToAddress(int row, int col)
    {
        return $"{ColumnIndexToName(col)}{row + 1}";
    }

    public static (int row, int col) ParseAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address)) return (0, 0);
        address = address.Trim().ToUpperInvariant().Replace("$", "");

        int i = 0;
        while (i < address.Length && char.IsLetter(address[i])) i++;

        string colStr = address[..i];
        string rowStr = address[i..];

        int col = ColumnNameToIndex(colStr);
        int row = int.TryParse(rowStr, out int r) ? Math.Max(0, r - 1) : 0;
        return (row, col);
    }

    /// <summary>Adjusts relative cell references when a copied formula is pasted at a new location.</summary>
    public static string TranslateFormulaReferences(string formula, int rowOffset, int columnOffset)
    {
        if (string.IsNullOrEmpty(formula) || formula[0] != '=' || rowOffset == 0 && columnOffset == 0)
            return formula;

        return FormulaReferenceRegex.Replace(formula, match =>
        {
            if (match.Groups["string"].Success) return match.Value;

            string columnText = match.Groups["column"].Value;
            string rowText = match.Groups["row"].Value;
            bool columnLocked = match.Groups["columnLock"].Value == "$";
            bool rowLocked = match.Groups["rowLock"].Value == "$";

            if (!int.TryParse(rowText, NumberStyles.None, CultureInfo.InvariantCulture, out int oneBasedRow))
                return match.Value;

            int column = ColumnNameToIndex(columnText);
            long newColumn = columnLocked ? column : (long)column + columnOffset;
            long newRow = rowLocked ? oneBasedRow : (long)oneBasedRow + rowOffset;
            if (newColumn < 0 || newRow < 1 || newColumn >= int.MaxValue || newRow > int.MaxValue)
                return "#REF!";

            return $"{(columnLocked ? "$" : string.Empty)}{ColumnIndexToName((int)newColumn)}" +
                   $"{(rowLocked ? "$" : string.Empty)}{newRow.ToString(CultureInfo.InvariantCulture)}";
        });
    }
}

public class SpreadsheetSheet : INotifyPropertyChanged
{
    private string _name = "Sheet1";
    private readonly Dictionary<(int r, int c), SpreadsheetCell> _cells = new();
    private readonly Dictionary<int, double> _rowHeights = new();
    private readonly Dictionary<int, double> _colWidths = new();
    private double[]? _rowOffsets;
    private double[]? _columnOffsets;
    private int _rowCount = 100;
    private int _columnCount = 26;
    private double _defaultRowHeight = 24.0;
    private double _defaultColumnWidth = 85.0;

    public string Name
    {
        get => _name;
        set { if (_name != value) { _name = value; OnPropertyChanged(nameof(Name)); } }
    }

    public int RowCount
    {
        get => _rowCount;
        set
        {
            int newValue = Math.Max(1, value);
            if (_rowCount == newValue) return;
            _rowCount = newValue;
            _rowOffsets = null;
            OnPropertyChanged(nameof(RowCount));
        }
    }

    public int ColumnCount
    {
        get => _columnCount;
        set
        {
            int newValue = Math.Max(1, value);
            if (_columnCount == newValue) return;
            _columnCount = newValue;
            _columnOffsets = null;
            OnPropertyChanged(nameof(ColumnCount));
        }
    }

    public double DefaultRowHeight
    {
        get => _defaultRowHeight;
        set
        {
            double newValue = Math.Max(12.0, value);
            if (_defaultRowHeight.Equals(newValue)) return;
            _defaultRowHeight = newValue;
            _rowOffsets = null;
            OnPropertyChanged(nameof(DefaultRowHeight));
        }
    }

    public double DefaultColumnWidth
    {
        get => _defaultColumnWidth;
        set
        {
            double newValue = Math.Max(20.0, value);
            if (_defaultColumnWidth.Equals(newValue)) return;
            _defaultColumnWidth = newValue;
            _columnOffsets = null;
            OnPropertyChanged(nameof(DefaultColumnWidth));
        }
    }

    private readonly List<SpreadsheetRange> _mergedRanges = new();
    public IReadOnlyList<SpreadsheetRange> MergedRanges => _mergedRanges;

    public void MergeRange(SpreadsheetRange range)
    {
        if (range.IsSingleCell) return;
        UnmergeRange(range);
        _mergedRanges.Add(range);
    }

    public void UnmergeRange(SpreadsheetRange range)
    {
        _mergedRanges.RemoveAll(r => r.MinRow <= range.MaxRow && r.MaxRow >= range.MinRow &&
                                     r.MinCol <= range.MaxCol && r.MaxCol >= range.MinCol);
    }

    public SpreadsheetRange? FindMergedRangeContaining(int row, int col)
    {
        // CRITICAL: Do NOT use FirstOrDefault here — SpreadsheetRange is a struct.
        // FirstOrDefault on List<struct> returns default(SpreadsheetRange) = (0,0,0,0)
        // when nothing matches, which wraps into Nullable<SpreadsheetRange> with HasValue=true!
        // That bug caused EVERY cell to draw at A1, leaving the rest of the grid blank.
        for (int i = 0; i < _mergedRanges.Count; i++)
        {
            if (_mergedRanges[i].Contains(row, col))
                return _mergedRanges[i];
        }
        return null; // explicitly null = no merged range found
    }

    public bool IsMergedNonMaster(int row, int col)
    {
        var mr = FindMergedRangeContaining(row, col);
        return mr.HasValue && (row != mr.Value.MinRow || col != mr.Value.MinCol);
    }

    public SpreadsheetCell GetOrCreateCell(int row, int col)
    {
        if (!_cells.TryGetValue((row, col), out var cell))
        {
            cell = new SpreadsheetCell { Row = row, Column = col };
            _cells[(row, col)] = cell;
        }
        return cell;
    }

    public SpreadsheetCell? GetCell(int row, int col)
    {
        _cells.TryGetValue((row, col), out var cell);
        return cell;
    }

    public void SetCell(int row, int col, SpreadsheetCell cell)
    {
        cell.Row = row;
        cell.Column = col;
        _cells[(row, col)] = cell;
    }

    public IEnumerable<SpreadsheetCell> GetAllCells() => _cells.Values;

    public double GetRowHeight(int row)
    {
        return _rowHeights.TryGetValue(row, out double h) ? h : DefaultRowHeight;
    }

    public void SetRowHeight(int row, double height)
    {
        double newHeight = Math.Max(12.0, height);
        if (_rowHeights.TryGetValue(row, out double current) && current.Equals(newHeight)) return;
        _rowHeights[row] = newHeight;
        _rowOffsets = null;
        OnPropertyChanged("RowHeight");
    }

    public double GetColumnWidth(int col)
    {
        return _colWidths.TryGetValue(col, out double w) ? w : DefaultColumnWidth;
    }

    public void SetColumnWidth(int col, double width)
    {
        double newWidth = Math.Max(20.0, width);
        if (_colWidths.TryGetValue(col, out double current) && current.Equals(newWidth)) return;
        _colWidths[col] = newWidth;
        _columnOffsets = null;
        OnPropertyChanged("ColumnWidth");
    }

    internal double[] GetRowOffsets()
    {
        if (_rowOffsets is not null) return _rowOffsets;

        var offsets = new double[RowCount + 1];
        for (int row = 0; row < RowCount; row++)
            offsets[row + 1] = offsets[row] + GetRowHeight(row);
        _rowOffsets = offsets;
        return offsets;
    }

    internal double[] GetColumnOffsets()
    {
        if (_columnOffsets is not null) return _columnOffsets;

        var offsets = new double[ColumnCount + 1];
        for (int column = 0; column < ColumnCount; column++)
            offsets[column + 1] = offsets[column] + GetColumnWidth(column);
        _columnOffsets = offsets;
        return offsets;
    }

    public void ClearCell(int row, int col)
    {
        if (_cells.TryGetValue((row, col), out var cell))
        {
            cell.RawValue = string.Empty;
            cell.Formula = string.Empty;
            cell.ComputedValue = null;
            cell.HasError = false;
            cell.ErrorMessage = string.Empty;
        }
    }

    public int ClearRange(SpreadsheetRange range)
    {
        int cleared = 0;
        foreach (var cell in _cells.Values)
        {
            if (!range.Contains(cell.Row, cell.Column)) continue;
            cell.RawValue = string.Empty;
            cell.Formula = string.Empty;
            cell.ComputedValue = null;
            cell.HasError = false;
            cell.ErrorMessage = string.Empty;
            cleared++;
        }
        return cleared;
    }

    public string ExportToCsv()
    {
        var sb = new StringBuilder();
        for (int r = 0; r < RowCount; r++)
        {
            var rowVals = new List<string>();
            bool hasData = false;
            for (int c = 0; c < ColumnCount; c++)
            {
                var cell = GetCell(r, c);
                string text = cell != null ? (!string.IsNullOrEmpty(cell.Formula) ? cell.Formula : cell.DisplayText) : string.Empty;
                if (!string.IsNullOrEmpty(text)) hasData = true;
                if (text.Contains(',') || text.Contains('"') || text.Contains('\n'))
                {
                    text = $"\"{text.Replace("\"", "\"\"")}\"";
                }
                rowVals.Add(text);
            }
            if (hasData || r < 30)
            {
                sb.AppendLine(string.Join(",", rowVals));
            }
        }
        return sb.ToString();
    }

    public void ImportFromCsv(string csvContent)
    {
        if (string.IsNullOrWhiteSpace(csvContent)) return;

        var lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        for (int r = 0; r < lines.Length; r++)
        {
            if (string.IsNullOrWhiteSpace(lines[r])) continue;
            var tokens = ParseCsvLine(lines[r]);
            for (int c = 0; c < tokens.Count; c++)
            {
                if (r < RowCount && c < ColumnCount)
                {
                    var cell = GetOrCreateCell(r, c);
                    string val = tokens[c];
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
    }

    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        var cur = new StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    cur.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(cur.ToString());
                cur.Clear();
            }
            else
            {
                cur.Append(c);
            }
        }
        result.Add(cur.ToString());
        return result;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}
