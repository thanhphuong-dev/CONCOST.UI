using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CONCOST.UI.Controls;

public class SpreadsheetFormulaEngine
{
    private readonly HashSet<string> _evaluatingCells = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _evaluatedCells = new(StringComparer.OrdinalIgnoreCase);
    private bool _isRecalculating;

    public void RecalculateSheet(SpreadsheetSheet sheet)
    {
        _evaluatingCells.Clear();
        _evaluatedCells.Clear();
        var cells = sheet.GetAllCells().ToArray();
        _isRecalculating = true;
        try
        {
            // Refresh constants first so formulas never read stale ComputedValue fields from a prior pass.
            foreach (var cell in cells)
            {
                if (string.IsNullOrEmpty(cell.Formula) || !cell.Formula.StartsWith("="))
                {
                    if (double.TryParse(cell.RawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double invariant))
                        cell.ComputedValue = invariant;
                    else if (double.TryParse(cell.RawValue, NumberStyles.Any, CultureInfo.CurrentCulture, out double current))
                        cell.ComputedValue = current;
                    else
                        cell.ComputedValue = cell.RawValue;
                    cell.HasError = false;
                    cell.ErrorMessage = string.Empty;
                }
            }

            // Reuse formulas already evaluated through a dependency during this pass.
            foreach (var cell in cells)
            {
                if (!string.IsNullOrEmpty(cell.Formula) && cell.Formula.StartsWith("="))
                    EvaluateCellFormula(sheet, cell);
            }
        }
        finally
        {
            _isRecalculating = false;
            _evaluatingCells.Clear();
        }
    }

    public object? EvaluateCellFormula(SpreadsheetSheet sheet, SpreadsheetCell cell)
    {
        string address = SpreadsheetAddressHelper.ToAddress(cell.Row, cell.Column);
        if (_isRecalculating && _evaluatedCells.Contains(address))
            return cell.ComputedValue;

        if (_evaluatingCells.Contains(address))
        {
            cell.HasError = true;
            cell.ErrorMessage = "#CIRCULAR!";
            cell.ComputedValue = "#CIRCULAR!";
            throw new Exception("#CIRCULAR!");
        }

        _evaluatingCells.Add(address);
        try
        {
            string expression = cell.Formula.TrimStart('=').Trim();
            object? result = EvaluateExpression(sheet, expression);
            cell.ComputedValue = result;
            cell.HasError = IsFormulaError(result);
            cell.ErrorMessage = cell.HasError ? result!.ToString()! : string.Empty;
            if (_isRecalculating) _evaluatedCells.Add(address);
            return result;
        }
        catch (Exception ex)
        {
            cell.HasError = true;
            cell.ErrorMessage = ex.Message.StartsWith("#") ? ex.Message : "#VALUE!";
            cell.ComputedValue = cell.ErrorMessage;
            if (_isRecalculating) _evaluatedCells.Add(address);
            return cell.ErrorMessage;
        }
        finally
        {
            _evaluatingCells.Remove(address);
        }
    }

    public object? EvaluateExpression(SpreadsheetSheet sheet, string expr)
    {
        if (string.IsNullOrWhiteSpace(expr)) return string.Empty;

        var tokens = Tokenize(expr);
        int pos = 0;
        object? result = ParseComparison(sheet, tokens, ref pos);
        if (pos != tokens.Count) throw new Exception("#VALUE!");
        return result;
    }

    private List<string> Tokenize(string expr)
    {
        var tokens = new List<string>();
        int i = 0;
        while (i < expr.Length)
        {
            char c = expr[i];
            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            if (c == '"')
            {
                int start = ++i;
                while (i < expr.Length && expr[i] != '"') i++;
                tokens.Add($"\"{expr[start..i]}\"");
                if (i < expr.Length) i++; // skip closing quote
                continue;
            }

            if ("(),:;+-*/^%&".Contains(c))
            {
                tokens.Add(c.ToString());
                i++;
                continue;
            }

            if (c is '<' or '>' or '=')
            {
                if (i + 1 < expr.Length && ((c == '<' && (expr[i + 1] == '=' || expr[i + 1] == '>')) || (c == '>' && expr[i + 1] == '=')))
                {
                    tokens.Add(expr.Substring(i, 2));
                    i += 2;
                }
                else
                {
                    tokens.Add(c.ToString());
                    i++;
                }
                continue;
            }

            // Word, number, cell ref, or function
            int wordStart = i;
            while (i < expr.Length && !char.IsWhiteSpace(expr[i]) && !"(),:;+-*/^%<>=&\"".Contains(expr[i]))
            {
                i++;
            }
            tokens.Add(expr[wordStart..i]);
        }
        return tokens;
    }

    private object? ParseComparison(SpreadsheetSheet sheet, List<string> tokens, ref int pos)
    {
        var left = ParseConcatenation(sheet, tokens, ref pos);

        while (pos < tokens.Count)
        {
            string op = tokens[pos];
            if (op is "=" or "<>" or "<" or "<=" or ">" or ">=")
            {
                pos++;
                var right = ParseConcatenation(sheet, tokens, ref pos);
                left = CompareValues(left, right, op);
            }
            else
            {
                break;
            }
        }

        return left;
    }

    private object? ParseConcatenation(SpreadsheetSheet sheet, List<string> tokens, ref int pos)
    {
        var left = ParseAdditive(sheet, tokens, ref pos);

        while (pos < tokens.Count && tokens[pos] == "&")
        {
            pos++;
            var right = ParseAdditive(sheet, tokens, ref pos);
            left = $"{left?.ToString() ?? string.Empty}{right?.ToString() ?? string.Empty}";
        }

        return left;
    }

    private object? ParseAdditive(SpreadsheetSheet sheet, List<string> tokens, ref int pos)
    {
        var left = ParseMultiplicative(sheet, tokens, ref pos);

        while (pos < tokens.Count)
        {
            string op = tokens[pos];
            if (op is "+" or "-")
            {
                pos++;
                var right = ParseMultiplicative(sheet, tokens, ref pos);
                double l = ToDouble(left);
                double r = ToDouble(right);
                left = op == "+" ? (l + r) : (l - r);
            }
            else
            {
                break;
            }
        }

        return left;
    }

    private object? ParseMultiplicative(SpreadsheetSheet sheet, List<string> tokens, ref int pos)
    {
        var left = ParseExponent(sheet, tokens, ref pos);

        while (pos < tokens.Count)
        {
            string op = tokens[pos];
            if (op is "*" or "/")
            {
                pos++;
                var right = ParseExponent(sheet, tokens, ref pos);
                double l = ToDouble(left);
                double r = ToDouble(right);
                if (op == "/")
                {
                    if (Math.Abs(r) < 1e-12) throw new Exception("#DIV/0!");
                    left = l / r;
                }
                else
                {
                    left = l * r;
                }
            }
            else
            {
                break;
            }
        }

        return left;
    }

    private object? ParseExponent(SpreadsheetSheet sheet, List<string> tokens, ref int pos)
    {
        var left = ParsePostfix(sheet, tokens, ref pos);

        while (pos < tokens.Count && tokens[pos] == "^")
        {
            pos++;
            var right = ParsePostfix(sheet, tokens, ref pos);
            left = Math.Pow(ToDouble(left), ToDouble(right));
        }

        return left;
    }

    private object? ParsePostfix(SpreadsheetSheet sheet, List<string> tokens, ref int pos)
    {
        var val = ParsePrimary(sheet, tokens, ref pos);

        // Support Excel percentage postfix operator: 10% -> 0.1
        while (pos < tokens.Count && tokens[pos] == "%")
        {
            pos++;
            val = ToDouble(val) / 100.0;
        }

        return val;
    }

    private object? ParsePrimary(SpreadsheetSheet sheet, List<string> tokens, ref int pos)
    {
        if (pos >= tokens.Count) throw new Exception("#VALUE!");

        string tok = tokens[pos++];

        if (tok == "+") return ParsePrimary(sheet, tokens, ref pos);
        if (tok == "-") return -ToDouble(ParsePrimary(sheet, tokens, ref pos));

        if (tok == "(")
        {
            var res = ParseComparison(sheet, tokens, ref pos);
            if (pos >= tokens.Count || tokens[pos] != ")") throw new Exception("#VALUE!");
            pos++;
            return res;
        }

        if (tok.StartsWith("\"") && tok.EndsWith("\""))
        {
            return tok.Length >= 2 ? tok[1..^1] : string.Empty;
        }

        if (double.TryParse(tok, NumberStyles.Any, CultureInfo.InvariantCulture, out double num))
        {
            return num;
        }

        if (double.TryParse(tok, NumberStyles.Any, CultureInfo.CurrentCulture, out double numCurr))
        {
            return numCurr;
        }

        // Check Function Call: NAME(arg1, arg2...)
        if (pos < tokens.Count && tokens[pos] == "(")
        {
            string funcName = tok.ToUpperInvariant();
            pos++; // skip '('

            var args = new List<object?>();
            while (pos < tokens.Count && tokens[pos] != ")")
            {
                // Check if argument is Range e.g. A1:B10
                if (pos + 2 < tokens.Count && tokens[pos + 1] == ":")
                {
                    string rStart = tokens[pos];
                    string rEnd = tokens[pos + 2];
                    pos += 3;
                    args.Add(new SpreadsheetRangeRef(rStart, rEnd));
                }
                else
                {
                    args.Add(ParseComparison(sheet, tokens, ref pos));
                }

                if (pos < tokens.Count && (tokens[pos] == "," || tokens[pos] == ";"))
                {
                    pos++;
                }
            }

            if (pos < tokens.Count && tokens[pos] == ")") pos++;
            return ExecuteFunction(sheet, funcName, args);
        }

        // Check Cell Range without function: A1:A5 (could be standalone)
        if (pos + 1 < tokens.Count && tokens[pos] == ":")
        {
            string start = tok;
            pos++; // skip ':'
            string end = tokens[pos++];
            return GetRangeValues(sheet, new SpreadsheetRangeRef(start, end));
        }

        // Check Single Cell Reference: A1, B12, etc.
        if (IsCellReference(tok))
        {
            var (r, c) = SpreadsheetAddressHelper.ParseAddress(tok);
            var cell = sheet.GetCell(r, c);
            if (cell == null) return 0.0;

            if (!string.IsNullOrEmpty(cell.Formula) && cell.Formula.StartsWith("="))
            {
                return EvaluateCellFormula(sheet, cell);
            }
            return cell.ComputedValue ?? (TryConvertToDouble(cell.RawValue, out double val) ? val : cell.RawValue);
        }

        if (bool.TryParse(tok, out bool b)) return b;

        return tok;
    }

    private object? ExecuteFunction(SpreadsheetSheet sheet, string funcName, List<object?> args)
    {
        var flatNumbers = FlattenToNumbers(sheet, args);

        switch (funcName)
        {
            case "SUM":
                return flatNumbers.Sum();

            case "AVERAGE" or "AVG":
                return flatNumbers.Count > 0 ? flatNumbers.Average() : throw new Exception("#DIV/0!");

            case "MIN":
                return flatNumbers.Count > 0 ? flatNumbers.Min() : 0.0;

            case "MAX":
                return flatNumbers.Count > 0 ? flatNumbers.Max() : 0.0;

            case "COUNT":
                return (double)flatNumbers.Count;

            case "COUNTA":
                var allFlat = FlattenToObjects(sheet, args);
                return (double)allFlat.Count(x => x != null && !string.IsNullOrEmpty(x.ToString()));

            case "PRODUCT":
                return flatNumbers.Count > 0 ? flatNumbers.Aggregate(1.0, (acc, val) => acc * val) : 0.0;

            case "SUMIF":
                if (args.Count >= 2)
                {
                    var rangeRef = args[0] as SpreadsheetRangeRef;
                    string criteria = args[1]?.ToString()?.Trim() ?? string.Empty;
                    var sumRangeRef = args.Count >= 3 ? args[2] as SpreadsheetRangeRef : rangeRef;

                    if (rangeRef != null && sumRangeRef != null)
                    {
                        var checkVals = GetRangeValues(sheet, rangeRef);
                        var sumVals = GetRangeValues(sheet, sumRangeRef);
                        double total = 0.0;

                        for (int i = 0; i < Math.Min(checkVals.Count, sumVals.Count); i++)
                        {
                            if (MatchesCriteria(checkVals[i], criteria))
                            {
                                if (TryConvertToDouble(sumVals[i], out double d)) total += d;
                            }
                        }
                        return total;
                    }
                }
                return 0.0;

            case "COUNTIF":
                if (args.Count >= 2 && args[0] is SpreadsheetRangeRef cRef)
                {
                    string criteria = args[1]?.ToString()?.Trim() ?? string.Empty;
                    var vals = GetRangeValues(sheet, cRef);
                    return (double)vals.Count(v => MatchesCriteria(v, criteria));
                }
                return 0.0;

            case "VLOOKUP":
                if (args.Count >= 3 && args[1] is SpreadsheetRangeRef tableRef)
                {
                    object? lookupVal = args[0];
                    int colIndex = (int)ToDouble(args[2]); // 1-based
                    bool exactMatch = args.Count >= 4 ? ToBoolean(args[3]) : true;

                    var tableRange = SpreadsheetRange.Parse($"{tableRef.Start}:{tableRef.End}");
                    for (int r = tableRange.MinRow; r <= tableRange.MaxRow; r++)
                    {
                        var firstCell = sheet.GetCell(r, tableRange.MinCol);
                        object? cellVal = firstCell?.ComputedValue ?? firstCell?.RawValue;
                        if (EqualsValues(cellVal, lookupVal))
                        {
                            int targetCol = tableRange.MinCol + colIndex - 1;
                            var resCell = sheet.GetCell(r, targetCol);
                            return resCell?.ComputedValue ?? resCell?.RawValue;
                        }
                    }
                    throw new Exception("#N/A");
                }
                return 0.0;

            case "ROUND":
                if (args.Count >= 2)
                    return Math.Round(ToDouble(args[0]), (int)ToDouble(args[1]));
                if (args.Count == 1)
                    return Math.Round(ToDouble(args[0]));
                return 0.0;

            case "ABS":
                return args.Count > 0 ? Math.Abs(ToDouble(args[0])) : 0.0;

            case "SQRT":
                double sq = args.Count > 0 ? ToDouble(args[0]) : 0.0;
                if (sq < 0) throw new Exception("#NUM!");
                return Math.Sqrt(sq);

            case "POWER":
                return args.Count >= 2 ? Math.Pow(ToDouble(args[0]), ToDouble(args[1])) : 0.0;

            case "MOD":
                if (args.Count >= 2)
                {
                    double divisor = ToDouble(args[1]);
                    if (Math.Abs(divisor) < 1e-12) throw new Exception("#DIV/0!");
                    return ToDouble(args[0]) % divisor;
                }
                return 0.0;

            case "INT":
                return args.Count > 0 ? Math.Floor(ToDouble(args[0])) : 0.0;

            case "CEILING":
                return args.Count > 0 ? Math.Ceiling(ToDouble(args[0])) : 0.0;

            case "FLOOR":
                return args.Count > 0 ? Math.Floor(ToDouble(args[0])) : 0.0;

            case "IF":
                if (args.Count >= 2)
                {
                    bool cond = ToBoolean(args[0]);
                    if (cond) return args[1];
                    return args.Count >= 3 ? args[2] : 0.0;
                }
                return 0.0;

            case "IFERROR":
                if (args.Count >= 2)
                {
                    try
                    {
                        var val = args[0];
                        if (val != null && val.ToString()!.StartsWith("#")) return args[1];
                        return val;
                    }
                    catch
                    {
                        return args[1];
                    }
                }
                return args.Count > 0 ? args[0] : 0.0;

            case "CONCAT" or "CONCATENATE":
                var sb = new StringBuilder();
                foreach (var item in FlattenToObjects(sheet, args))
                {
                    sb.Append(item?.ToString() ?? string.Empty);
                }
                return sb.ToString();

            case "LEFT":
                if (args.Count >= 2)
                {
                    string str = args[0]?.ToString() ?? string.Empty;
                    int len = Math.Min((int)ToDouble(args[1]), str.Length);
                    return str[..len];
                }
                return args.Count > 0 ? (args[0]?.ToString()?.Substring(0, 1) ?? string.Empty) : string.Empty;

            case "RIGHT":
                if (args.Count >= 2)
                {
                    string str = args[0]?.ToString() ?? string.Empty;
                    int len = Math.Min((int)ToDouble(args[1]), str.Length);
                    return str.Substring(str.Length - len);
                }
                return string.Empty;

            case "MID":
                if (args.Count >= 3)
                {
                    string str = args[0]?.ToString() ?? string.Empty;
                    int start = Math.Max(0, (int)ToDouble(args[1]) - 1);
                    int len = (int)ToDouble(args[2]);
                    if (start >= str.Length) return string.Empty;
                    return str.Substring(start, Math.Min(len, str.Length - start));
                }
                return string.Empty;

            case "TRIM":
                return args.Count > 0 ? (args[0]?.ToString()?.Trim() ?? string.Empty) : string.Empty;

            case "UPPER":
                return args.Count > 0 ? (args[0]?.ToString()?.ToUpperInvariant() ?? string.Empty) : string.Empty;

            case "LOWER":
                return args.Count > 0 ? (args[0]?.ToString()?.ToLowerInvariant() ?? string.Empty) : string.Empty;

            case "LEN":
                return args.Count > 0 ? (double)(args[0]?.ToString()?.Length ?? 0) : 0.0;

            case "NOW":
                return DateTime.Now;

            case "TODAY":
                return DateTime.Today;

            case "AND":
                return args.All(ToBoolean);

            case "OR":
                return args.Any(ToBoolean);

            case "NOT":
                return args.Count > 0 ? !ToBoolean(args[0]) : true;

            case "HLOOKUP":
                if (args.Count >= 3 && args[1] is SpreadsheetRangeRef hTableRef)
                {
                    object? lookupVal = args[0];
                    int rowIndex = (int)ToDouble(args[2]); // 1-based row index
                    var hTableRange = SpreadsheetRange.Parse($"{hTableRef.Start}:{hTableRef.End}");
                    for (int c = hTableRange.MinCol; c <= hTableRange.MaxCol; c++)
                    {
                        var firstCell = sheet.GetCell(hTableRange.MinRow, c);
                        object? cellVal = firstCell?.ComputedValue ?? firstCell?.RawValue;
                        if (EqualsValues(cellVal, lookupVal))
                        {
                            int targetRow = hTableRange.MinRow + rowIndex - 1;
                            var resCell = sheet.GetCell(targetRow, c);
                            return resCell?.ComputedValue ?? resCell?.RawValue;
                        }
                    }
                    throw new Exception("#N/A");
                }
                return 0.0;

            case "MATCH":
                if (args.Count >= 2 && args[1] is SpreadsheetRangeRef matchRef)
                {
                    object? lookupVal = args[0];
                    var mVals = GetRangeValues(sheet, matchRef);
                    for (int idx = 0; idx < mVals.Count; idx++)
                    {
                        if (EqualsValues(mVals[idx], lookupVal)) return (double)(idx + 1); // 1-based
                    }
                    throw new Exception("#N/A");
                }
                return 0.0;

            case "INDEX":
                if (args.Count >= 2 && args[0] is SpreadsheetRangeRef idxRef)
                {
                    var idxRange = SpreadsheetRange.Parse($"{idxRef.Start}:{idxRef.End}");
                    int rOffset = (int)ToDouble(args[1]); // 1-based
                    int cOffset = args.Count >= 3 ? (int)ToDouble(args[2]) : 1; // 1-based

                    int targetRow = idxRange.MinRow + Math.Max(1, rOffset) - 1;
                    int targetCol = idxRange.MinCol + Math.Max(1, cOffset) - 1;
                    var c = sheet.GetCell(targetRow, targetCol);
                    return c?.ComputedValue ?? c?.RawValue;
                }
                return 0.0;

            case "ISNUMBER":
                return args.Count > 0 && TryConvertToDouble(args[0], out _);

            case "ISTEXT":
                return args.Count > 0 && args[0] is string s && !TryConvertToDouble(s, out _);

            case "ISBLANK":
                return args.Count == 0 || args[0] == null || string.IsNullOrEmpty(args[0]?.ToString());

            case "DATE":
                if (args.Count >= 3)
                {
                    int y = (int)ToDouble(args[0]);
                    int m = (int)ToDouble(args[1]);
                    int d = (int)ToDouble(args[2]);
                    return new DateTime(Math.Max(1, y), NumericCompatibility.Clamp(m, 1, 12), NumericCompatibility.Clamp(d, 1, 31));
                }
                return DateTime.Today;

            case "YEAR":
                if (args.Count > 0 && DateTime.TryParse(args[0]?.ToString(), out var dYear)) return (double)dYear.Year;
                return 0.0;

            case "MONTH":
                if (args.Count > 0 && DateTime.TryParse(args[0]?.ToString(), out var dMonth)) return (double)dMonth.Month;
                return 0.0;

            case "DAY":
                if (args.Count > 0 && DateTime.TryParse(args[0]?.ToString(), out var dDay)) return (double)dDay.Day;
                return 0.0;

            case "TEXT":
                if (args.Count >= 2)
                {
                    string fmt = args[1]?.ToString()?.Trim('"') ?? "G";
                    if (TryConvertToDouble(args[0], out double dNum))
                        return dNum.ToString(fmt, CultureInfo.CurrentCulture);
                    if (DateTime.TryParse(args[0]?.ToString(), out var dt))
                        return dt.ToString(fmt, CultureInfo.CurrentCulture);
                }
                return args.Count > 0 ? args[0]?.ToString() : string.Empty;

            default:
                throw new Exception("#NAME?");
        }
    }

    private static bool MatchesCriteria(object? val, string criteria)
    {
        if (string.IsNullOrEmpty(criteria)) return true;
        string valStr = val?.ToString() ?? string.Empty;

        if (criteria.StartsWith(">="))
            return TryConvertToDouble(val, out double d) && double.TryParse(criteria[2..], out double t) && d >= t;
        if (criteria.StartsWith("<="))
            return TryConvertToDouble(val, out double d) && double.TryParse(criteria[2..], out double t) && d <= t;
        if (criteria.StartsWith("<>"))
            return !Equals(valStr, criteria[2..]);
        if (criteria.StartsWith(">"))
            return TryConvertToDouble(val, out double d) && double.TryParse(criteria[1..], out double t) && d > t;
        if (criteria.StartsWith("<"))
            return TryConvertToDouble(val, out double d) && double.TryParse(criteria[1..], out double t) && d < t;
        if (criteria.StartsWith("="))
            return Equals(valStr, criteria[1..]);

        return string.Equals(valStr, criteria, StringComparison.OrdinalIgnoreCase);
    }

    private static bool EqualsValues(object? a, object? b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        if (TryConvertToDouble(a, out double da) && TryConvertToDouble(b, out double db))
            return Math.Abs(da - db) < 1e-9;
        return string.Equals(a.ToString()?.Trim(), b.ToString()?.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private List<double> FlattenToNumbers(SpreadsheetSheet sheet, List<object?> args)
    {
        var list = new List<double>();
        foreach (var arg in args)
        {
            if (arg is SpreadsheetRangeRef rangeRef)
            {
                var values = GetRangeValues(sheet, rangeRef);
                foreach (var v in values)
                {
                    if (IsFormulaError(v)) throw new Exception(v!.ToString());
                    if (TryConvertToDouble(v, out double d)) list.Add(d);
                }
            }
            else if (arg is IEnumerable<object?> enumObj)
            {
                foreach (var v in enumObj)
                {
                    if (TryConvertToDouble(v, out double d)) list.Add(d);
                }
            }
            else if (TryConvertToDouble(arg, out double d))
            {
                list.Add(d);
            }
        }
        return list;
    }

    private List<object?> FlattenToObjects(SpreadsheetSheet sheet, List<object?> args)
    {
        var list = new List<object?>();
        foreach (var arg in args)
        {
            if (arg is SpreadsheetRangeRef rangeRef)
            {
                list.AddRange(GetRangeValues(sheet, rangeRef));
            }
            else if (arg is IEnumerable<object?> enumObj)
            {
                list.AddRange(enumObj);
            }
            else
            {
                list.Add(arg);
            }
        }
        return list;
    }

    private List<object?> GetRangeValues(SpreadsheetSheet sheet, SpreadsheetRangeRef rRef)
    {
        var range = SpreadsheetRange.Parse($"{rRef.Start}:{rRef.End}");
        var results = new List<object?>();

        for (int r = range.MinRow; r <= range.MaxRow; r++)
        {
            for (int c = range.MinCol; c <= range.MaxCol; c++)
            {
                var cell = sheet.GetCell(r, c);
                if (cell != null)
                {
                    if (!string.IsNullOrEmpty(cell.Formula) && cell.Formula.StartsWith("="))
                    {
                        results.Add(EvaluateCellFormula(sheet, cell));
                    }
                    else
                    {
                        results.Add(cell.ComputedValue ?? cell.RawValue);
                    }
                }
                else
                {
                    results.Add(null);
                }
            }
        }
        return results;
    }

    private bool IsCellReference(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        return Regex.IsMatch(s.Trim(), @"^\$?[A-Za-z]+\$?[1-9][0-9]*$");
    }

    private bool CompareValues(object? left, object? right, string op)
    {
        double dl = ToDouble(left);
        double dr = ToDouble(right);

        return op switch
        {
            "=" => Equals(left?.ToString(), right?.ToString()) || Math.Abs(dl - dr) < 1e-9,
            "<>" => !Equals(left?.ToString(), right?.ToString()) && Math.Abs(dl - dr) >= 1e-9,
            "<" => dl < dr,
            "<=" => dl <= dr,
            ">" => dl > dr,
            ">=" => dl >= dr,
            _ => false
        };
    }

    private static double ToDouble(object? val)
    {
        if (IsFormulaError(val)) throw new Exception(val!.ToString());
        if (val == null) return 0.0;
        if (val is double d) return d;
        if (val is int i) return i;
        if (val is float f) return f;
        if (val is decimal dec) return (double)dec;
        if (val is bool b) return b ? 1.0 : 0.0;
        if (double.TryParse(val.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double res)) return res;
        if (double.TryParse(val.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out double resCurr)) return resCurr;
        return 0.0;
    }

    private static bool IsFormulaError(object? value) =>
        value is string text && text.StartsWith("#", StringComparison.Ordinal);

    private static bool TryConvertToDouble(object? val, out double result)
    {
        result = 0.0;
        if (val == null) return false;
        if (val is double d) { result = d; return true; }
        if (val is int i) { result = i; return true; }
        if (val is float f) { result = f; return true; }
        if (val is decimal dec) { result = (double)dec; return true; }
        if (double.TryParse(val.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double res)) { result = res; return true; }
        if (double.TryParse(val.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out double resCurr)) { result = resCurr; return true; }
        return false;
    }

    private bool ToBoolean(object? val)
    {
        if (val == null) return false;
        if (val is bool b) return b;
        if (val is double d) return Math.Abs(d) > 1e-9;
        if (val is int i) return i != 0;
        if (bool.TryParse(val.ToString(), out bool pb)) return pb;
        return !string.IsNullOrEmpty(val.ToString());
    }

    private sealed class SpreadsheetRangeRef
    {
        public string Start { get; }
        public string End { get; }
        public SpreadsheetRangeRef(string start, string end)
        {
            Start = start;
            End = end;
        }
    }
}
