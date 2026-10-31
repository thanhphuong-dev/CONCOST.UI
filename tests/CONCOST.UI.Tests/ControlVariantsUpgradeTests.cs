using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using CONCOST.UI.Controls;
using CONCOST.UI.Icons;
using Xunit;

namespace CONCOST.UI.Tests;

public class ControlVariantsUpgradeTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    [Fact]
    public void Badge_DefaultProperties_AreCorrect()
    {
        RunOnSTA(() =>
        {
            var badge = new Badge();

            Assert.Equal(BadgeAppearance.Primary, badge.Appearance);
            Assert.Equal(new CornerRadius(10), badge.CornerRadius);
            Assert.Null(badge.Value);
            Assert.False(badge.IsDot);
            Assert.True(badge.ShowBadge);
            Assert.Equal(new Thickness(0, -6, -8, 0), badge.BadgeMargin);
            Assert.Null(badge.Status);
            Assert.False(badge.HasBadge);
        });
    }

    [Fact]
    public void Badge_ValueOrIsDot_UpdatesHasBadgeProperty()
    {
        RunOnSTA(() =>
        {
            var badge = new Badge();
            Assert.False(badge.HasBadge);

            badge.Value = "5";
            Assert.True(badge.HasBadge);

            badge.Value = null;
            Assert.False(badge.HasBadge);

            badge.IsDot = true;
            Assert.True(badge.HasBadge);

            badge.IsDot = false;
            Assert.False(badge.HasBadge);

            badge.Value = 99;
            badge.IsDot = true;
            Assert.True(badge.HasBadge);
        });
    }

    [Fact]
    public void Badge_Appearance_SupportsAllEnumValues()
    {
        RunOnSTA(() =>
        {
            var badge = new Badge();

            foreach (BadgeAppearance appearance in Enum.GetValues(typeof(BadgeAppearance)))
            {
                badge.Appearance = appearance;
                Assert.Equal(appearance, badge.Appearance);
            }
        });
    }

    [Fact]
    public void Expander_ExpandDirection_SupportsRightAndAllDirections()
    {
        RunOnSTA(() =>
        {
            var expander = new System.Windows.Controls.Expander();
            Assert.Equal(ExpandDirection.Down, expander.ExpandDirection);

            expander.ExpandDirection = ExpandDirection.Right;
            Assert.Equal(ExpandDirection.Right, expander.ExpandDirection);

            expander.ExpandDirection = ExpandDirection.Left;
            Assert.Equal(ExpandDirection.Left, expander.ExpandDirection);

            expander.ExpandDirection = ExpandDirection.Up;
            Assert.Equal(ExpandDirection.Up, expander.ExpandDirection);

            expander.IsExpanded = true;
            Assert.True(expander.IsExpanded);

            var customExpander = new CONCOST.UI.Controls.Expander();
            Assert.True(customExpander.IsExpanded);
            Assert.Equal(ExpandDirection.Down, customExpander.ExpandDirection);
            Assert.Equal(new CornerRadius(8), customExpander.CornerRadius);

            customExpander.ExpandDirection = ExpandDirection.Right;
            Assert.Equal(ExpandDirection.Right, customExpander.ExpandDirection);
        });
    }

    [Fact]
    public void Calendar_SelectionModes_AreSupported()
    {
        RunOnSTA(() =>
        {
            var cal = new Calendar();
            Assert.Equal(CalendarSelectionMode.SingleDate, cal.SelectionMode);

            cal.SelectionMode = CalendarSelectionMode.MultipleRange;
            Assert.Equal(CalendarSelectionMode.MultipleRange, cal.SelectionMode);

            cal.SelectedDates.AddRange(DateTime.Today, DateTime.Today.AddDays(7));
            Assert.Equal(8, cal.SelectedDates.Count);

            cal.BlackoutDates.AddDatesInPast();
            Assert.NotEmpty(cal.BlackoutDates);
        });
    }

    [Fact]
    public void TabControl_TabStripPlacement_SupportsLeftAndAllPlacements()
    {
        RunOnSTA(() =>
        {
            var tc = new TabControl();
            Assert.Equal(Dock.Top, tc.TabStripPlacement);

            tc.TabStripPlacement = Dock.Left;
            Assert.Equal(Dock.Left, tc.TabStripPlacement);

            var t1 = new TabItem { Header = "Overview" };
            var t2 = new TabItem { Header = "Details" };
            tc.Items.Add(t1);
            tc.Items.Add(t2);

            Assert.Equal(2, tc.Items.Count);
            tc.SelectedItem = t2;
            Assert.Equal(t2, tc.SelectedItem);
        });
    }

    [Fact]
    public void TreeView_HierarchyAndExpansion_WorkProperly()
    {
        RunOnSTA(() =>
        {
            var tv = new TreeView();
            var root = new TreeViewItem { Header = "Root Project", IsExpanded = true };
            var child = new TreeViewItem { Header = "Child Element" };
            root.Items.Add(child);
            tv.Items.Add(root);

            Assert.Single(tv.Items);
            Assert.True(root.IsExpanded);
            Assert.Single(root.Items);
        });
    }

    [Fact]
    public void GroupBox_HeaderAndContent_AssignCorrectly()
    {
        RunOnSTA(() =>
        {
            var gb = new GroupBox
            {
                Header = "Member Analysis Parameters",
                Content = new TextBlock { Text = "MEd = 245 kNm" }
            };

            Assert.Equal("Member Analysis Parameters", gb.Header);
            Assert.IsType<TextBlock>(gb.Content);
        });
    }

    [Fact]
    public void Menu_HierarchyAndGestures_AssignCorrectly()
    {
        RunOnSTA(() =>
        {
            var menu = new Menu();
            var rootMenu = new MenuItem { Header = "_File" };
            var subItem = new MenuItem { Header = "Save", InputGestureText = "Ctrl+S" };
            rootMenu.Items.Add(subItem);
            menu.Items.Add(rootMenu);

            Assert.Single(menu.Items);
            Assert.Single(rootMenu.Items);
            Assert.Equal("Ctrl+S", subItem.InputGestureText);
        });
    }

    [Fact]
    public void Expander_CornerRadiusConverter_PreventsCornerBleed()
    {
        RunOnSTA(() =>
        {
            var conv = new CONCOST.UI.Core.Converters.ExpanderCornerRadiusConverter();

            // Down, Expanded (Primary Accent 10px outer border, thickness 1px)
            object[] downExpandedValues = [new CornerRadius(10), true, ExpandDirection.Down, new Thickness(1)];
            var headerCr = (CornerRadius)conv.Convert(downExpandedValues, typeof(CornerRadius), null, null!);
            Assert.Equal(new CornerRadius(9, 9, 0, 0), headerCr);

            var contentCr = (CornerRadius)conv.Convert(downExpandedValues, typeof(CornerRadius), "Content", null!);
            Assert.Equal(new CornerRadius(0, 0, 9, 9), contentCr);

            // Down, Collapsed (Header fills whole expander)
            object[] downCollapsedValues = [new CornerRadius(10), false, ExpandDirection.Down, new Thickness(1)];
            var collapsedCr = (CornerRadius)conv.Convert(downCollapsedValues, typeof(CornerRadius), null, null!);
            Assert.Equal(new CornerRadius(9, 9, 9, 9), collapsedCr);

            // Right, Expanded (Horizontal Expander)
            object[] rightExpandedValues = [new CornerRadius(8), true, ExpandDirection.Right, new Thickness(1)];
            var headerRightCr = (CornerRadius)conv.Convert(rightExpandedValues, typeof(CornerRadius), null, null!);
            Assert.Equal(new CornerRadius(7, 0, 0, 7), headerRightCr);

            var contentRightCr = (CornerRadius)conv.Convert(rightExpandedValues, typeof(CornerRadius), "Content", null!);
            Assert.Equal(new CornerRadius(0, 7, 7, 0), contentRightCr);

            // Clean Expander (0px)
            object[] cleanValues = [new CornerRadius(0), true, ExpandDirection.Down, new Thickness(1)];
            var cleanCr = (CornerRadius)conv.Convert(cleanValues, typeof(CornerRadius), null, null!);
            Assert.Equal(new CornerRadius(0, 0, 0, 0), cleanCr);
        });
    }

    [Fact]
    public void Expander_VerticalStrips_HeightsAreIdentical()
    {
        RunOnSTA(() =>
        {
            var app = Application.Current ?? new Application();
            var dict = new ResourceDictionary { Source = new Uri("pack://application:,,,/CONCOST.UI;component/Themes/Generic.xaml") };
            app.Resources.MergedDictionaries.Add(dict);

            var expDef = new System.Windows.Controls.Expander
            {
                Header = "Left: Default",
                ExpandDirection = ExpandDirection.Left
            };
            var expPri = new System.Windows.Controls.Expander
            {
                Header = "Left: Primary",
                ExpandDirection = ExpandDirection.Left
            };
            expPri.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Expander.Primary");

            var sp = new StackPanel { Orientation = Orientation.Horizontal, Height = 280 };
            sp.Children.Add(expDef);
            sp.Children.Add(expPri);

            sp.Measure(new Size(800, 600));
            sp.Arrange(new Rect(0, 0, 800, 600));

            expDef.ApplyTemplate();
            expPri.ApplyTemplate();

            var btnDef = expDef.Template.FindName("ToggleButton", expDef) as System.Windows.Controls.Primitives.ToggleButton;
            var btnPri = expPri.Template.FindName("ToggleButton", expPri) as System.Windows.Controls.Primitives.ToggleButton;

            btnDef?.ApplyTemplate();
            btnPri?.ApplyTemplate();

            var borderDef = btnDef?.Template.FindName("templateRoot", btnDef) as Border;
            Assert.Equal(280, btnDef?.ActualHeight);
            Assert.Equal(280, btnPri?.ActualHeight);
        });
    }

    [Fact]
    public void Calendar_Styling_LoadsCorrectly()
    {
        RunOnSTA(() =>
        {
            var app = Application.Current ?? new Application();
            var dict = new ResourceDictionary { Source = new Uri("pack://application:,,,/CONCOST.UI;component/Themes/Generic.xaml") };
            app.Resources.MergedDictionaries.Add(dict);

            var calBase = new System.Windows.Controls.Calendar
            {
                SelectedDate = DateTime.Today,
                DisplayDate = DateTime.Today,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var calPrimary = new System.Windows.Controls.Calendar
            {
                SelectedDate = DateTime.Today.AddDays(3),
                DisplayDate = DateTime.Today,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            calPrimary.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Calendar.Primary");

            var grid1 = new Grid();
            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });
            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Grid.SetColumn(calBase, 0); grid1.Children.Add(calBase);
            Grid.SetColumn(calPrimary, 2); grid1.Children.Add(calPrimary);

            grid1.Measure(new Size(800, 600));
            grid1.Arrange(new Rect(0, 0, 800, 600));

            calBase.ApplyTemplate();
            calPrimary.ApplyTemplate();

            Assert.NotNull(calBase.CalendarItemStyle);
            Assert.NotNull(calPrimary.CalendarItemStyle);
            Assert.True(calBase.ActualWidth > 0);
            Assert.True(calPrimary.ActualWidth > 0);
        });
    }

    [Fact]
    public void SpreadsheetCell_CurrencyFormatting_AlwaysUsesStandardDollarAndCents()
    {
        RunOnSTA(() =>
        {
            var cellInt = new SpreadsheetCell
            {
                RawValue = "45",
                ComputedValue = 45.0,
                Format = SpreadsheetCellFormat.Currency
            };
            Assert.Equal("$45", cellInt.DisplayText);

            var cellCents = new SpreadsheetCell
            {
                RawValue = "0.85",
                ComputedValue = 0.85,
                Format = SpreadsheetCellFormat.Currency
            };
            Assert.Equal("$0.85", cellCents.DisplayText);

            var cellLarge = new SpreadsheetCell
            {
                RawValue = "32804",
                ComputedValue = 32804.0,
                Format = SpreadsheetCellFormat.Currency
            };
            Assert.Equal("$32,804", cellLarge.DisplayText);
        });
    }

    [Fact]
    public void SpreadsheetCell_DynamicResourceKeys_StoreAndNotify()
    {
        RunOnSTA(() =>
        {
            var cell = new SpreadsheetCell();
            string? changedProp = null;
            cell.PropertyChanged += (s, e) => changedProp = e.PropertyName;

            cell.BackgroundResourceKey = "CONCOST.Brush.SurfaceSecondary";
            Assert.Equal("CONCOST.Brush.SurfaceSecondary", cell.BackgroundResourceKey);
            Assert.Equal("BackgroundResourceKey", changedProp);

            cell.ForegroundResourceKey = "CONCOST.Brush.Primary";
            Assert.Equal("CONCOST.Brush.Primary", cell.ForegroundResourceKey);
            Assert.Equal("ForegroundResourceKey", changedProp);
        });
    }

    [Fact]
    public void SpreadsheetFormulaEngine_InvariantNumberParsing_ComputesAccurateTotal()
    {
        RunOnSTA(() =>
        {
            var sheet = new SpreadsheetSheet { RowCount = 5, ColumnCount = 5 };
            var qtyCell = sheet.GetOrCreateCell(0, 0); // A1
            qtyCell.RawValue = "1850.0";
            qtyCell.Format = SpreadsheetCellFormat.Number;

            var priceCell = sheet.GetOrCreateCell(0, 1); // B1
            priceCell.RawValue = "0.85";
            priceCell.Format = SpreadsheetCellFormat.Currency;

            var totalCell = sheet.GetOrCreateCell(0, 2); // C1
            totalCell.Formula = "=A1*B1";
            totalCell.Format = SpreadsheetCellFormat.Currency;

            var engine = new SpreadsheetFormulaEngine();
            engine.RecalculateSheet(sheet);

            Assert.Equal(1572.5, totalCell.ComputedValue);
            Assert.Equal("$1,572.50", totalCell.DisplayText);
        });
    }

    [Fact]
    public void SpreadsheetFormulaEngine_PropagatesErrorsAndDetectsCircularReferences()
    {
        RunOnSTA(() =>
        {
            var sheet = new SpreadsheetSheet { RowCount = 5, ColumnCount = 5 };
            var division = sheet.GetOrCreateCell(0, 0);
            division.Formula = "=1/0";
            var dependent = sheet.GetOrCreateCell(0, 1);
            dependent.Formula = "=A1+1";

            var engine = new SpreadsheetFormulaEngine();
            engine.RecalculateSheet(sheet);

            Assert.Equal("#DIV/0!", division.ComputedValue);
            Assert.True(division.HasError);
            Assert.Equal("#DIV/0!", dependent.ComputedValue);
            Assert.True(dependent.HasError);

            division.Formula = "=B1";
            dependent.Formula = "=A1";
            engine.RecalculateSheet(sheet);

            Assert.Equal("#CIRCULAR!", division.ComputedValue);
            Assert.True(division.HasError);
            Assert.Equal("#CIRCULAR!", dependent.ComputedValue);
            Assert.True(dependent.HasError);
        });
    }

    [Fact]
    public void SpreadsheetFormulaEngine_RejectsIncompleteExpressions()
    {
        RunOnSTA(() =>
        {
            var sheet = new SpreadsheetSheet { RowCount = 2, ColumnCount = 2 };
            var formula = sheet.GetOrCreateCell(0, 0);
            formula.Formula = "=1+";

            new SpreadsheetFormulaEngine().RecalculateSheet(sheet);

            Assert.Equal("#VALUE!", formula.ComputedValue);
            Assert.True(formula.HasError);
        });
    }

    [Fact]
    public void SpreadsheetFormulaEngine_RecalculateRefreshesInputsBeforeDependentFormulas()
    {
        RunOnSTA(() =>
        {
            var sheet = new SpreadsheetSheet { RowCount = 4, ColumnCount = 4 };
            var dependent = sheet.GetOrCreateCell(0, 1); // B1 inserted before A1 to exercise dictionary order.
            dependent.Formula = "=A1*2";
            var transitive = sheet.GetOrCreateCell(0, 2);
            transitive.Formula = "=B1+1";
            var input = sheet.GetOrCreateCell(0, 0);
            input.RawValue = "2";

            var engine = new SpreadsheetFormulaEngine();
            engine.RecalculateSheet(sheet);
            Assert.Equal(5.0, transitive.ComputedValue);

            input.RawValue = "10";
            engine.RecalculateSheet(sheet);
            Assert.Equal(20.0, dependent.ComputedValue);
            Assert.Equal(21.0, transitive.ComputedValue);
        });
    }

    [Fact]
    public void SpreadsheetAddressHelper_TranslatesRelativeReferencesAndPreservesAbsoluteReferences()
    {
        const string formula = "=A1+$B1+C$3+$D$4+SUM(A1:A3)+\"A1\"+LOG10(A1)";

        var translated = SpreadsheetAddressHelper.TranslateFormulaReferences(formula, 2, 1);

        Assert.Equal("=B3+$B3+D$3+$D$4+SUM(B3:B5)+\"A1\"+LOG10(B3)", translated);
    }

    [Fact]
    public void SpreadsheetAddressHelper_TranslatesReferencesPastOriginToRefError()
    {
        Assert.Equal("=#REF!+A1", SpreadsheetAddressHelper.TranslateFormulaReferences("=A1+B2", -1, -1));
    }

    [Fact]
    public void SpreadsheetSheet_ClearRange_VisitsOnlyPopulatedCellsAndPreservesOutsideCells()
    {
        RunOnSTA(() =>
        {
            var sheet = new SpreadsheetSheet { RowCount = 1_000_000, ColumnCount = 1_000 };
            sheet.GetOrCreateCell(2, 3).RawValue = "inside";
            sheet.GetOrCreateCell(900_000, 900).RawValue = "outside";

            int cleared = sheet.ClearRange(new SpreadsheetRange(0, 0, 10, 10));

            Assert.Equal(1, cleared);
            Assert.Equal(string.Empty, sheet.GetCell(2, 3)!.RawValue);
            Assert.Equal("outside", sheet.GetCell(900_000, 900)!.RawValue);
        });
    }
}

