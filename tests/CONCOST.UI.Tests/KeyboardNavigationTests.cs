using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Reflection;
using System.Windows.Media;
using CONCOST.UI.Controls;
using CONCOST.UI.Themes;
using Xunit;

namespace CONCOST.UI.Tests;

public class KeyboardNavigationTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    private static ResourceDictionary EnsureResources()
    {
        var app = Application.Current;
        if (app == null)
        {
            try { app = new Application(); } catch { app = Application.Current; }
        }

        var uri = new Uri("pack://application:,,,/CONCOST.UI;component/Themes/Generic.xaml", UriKind.Absolute);
        foreach (ResourceDictionary md in app.Resources.MergedDictionaries)
        {
            if (md.Source == uri) return app.Resources;
        }

        var dict = new ResourceDictionary { Source = uri };
        app.Resources.MergedDictionaries.Add(dict);
        return app.Resources;
    }

    [Fact]
    public void DataGrid_DirectionalNavigation_IsContained()
    {
        RunOnSTA(() =>
        {
            var res = EnsureResources();
            var style = res["CONCOST.DataGrid.Base"] as Style;
            Assert.NotNull(style);

            var dg = new DataGrid { Style = style };
            var nav = KeyboardNavigation.GetDirectionalNavigation(dg);
            Assert.Equal(KeyboardNavigationMode.Contained, nav);
        });
    }

    [Fact]
    public void StackedHeaderDataGrid_DirectionalNavigation_IsContained()
    {
        RunOnSTA(() =>
        {
            var res = EnsureResources();
            var style = res["CONCOST.StackedHeaderDataGrid.Base"] as Style;
            Assert.NotNull(style);

            var dg = new StackedHeaderDataGrid { Style = style };
            var nav = KeyboardNavigation.GetDirectionalNavigation(dg);
            Assert.Equal(KeyboardNavigationMode.Contained, nav);
        });
    }

    [Fact]
    public void StackedHeaderDataGrid_RebuildsWhenNestedColumnTreeChanges()
    {
        RunOnSTA(() =>
        {
            EnsureResources();
            var grid = new StackedHeaderDataGrid();
            var group = new ColumnGroup { HeaderText = "Geometry" };
            var first = new ColumnLeaf { HeaderText = "Width", BindingPath = "Width" };
            group.Children.Add(first);
            grid.ColumnHeaderTree.Add(group);

            Assert.Single(grid.Columns);

            group.Children.Add(new ColumnLeaf { HeaderText = "Height", BindingPath = "Height" });
            Assert.Equal(2, grid.Columns.Count);

            first.HeaderText = "Updated width";
            Assert.Equal("Updated width", grid.Columns[0].Header);

            grid.ColumnHeaderTree.Clear();
            Assert.Empty(grid.Columns);
        });
    }

    [Fact]
    public void StackedHeaderDataGrid_ReadOnlyAnonymousProperties_DoNotEnterEditMode()
    {
        RunOnSTA(() =>
        {
            var grid = new StackedHeaderDataGrid { AutoGenerateColumns = false };
            grid.ColumnHeaderTree.Add(new ColumnLeaf { HeaderText = "Floor Level", BindingPath = "FloorLevel" });
            grid.ItemsSource = new[] { new { FloorLevel = "LEVEL 1" } };

            var column = Assert.IsType<DataGridTextColumn>(grid.Columns[0]);
            Assert.True(column.IsReadOnly);
            Assert.Equal(BindingMode.OneWay, Assert.IsType<Binding>(column.Binding).Mode);
        });
    }

    [Fact]
    public void StackedHeaderDataGrid_WritableProperties_RemainEditable()
    {
        RunOnSTA(() =>
        {
            var grid = new StackedHeaderDataGrid { AutoGenerateColumns = false };
            grid.ColumnHeaderTree.Add(new ColumnLeaf { HeaderText = "Floor Level", BindingPath = nameof(EditableGridRow.FloorLevel) });
            grid.ItemsSource = new[] { new EditableGridRow { FloorLevel = "LEVEL 1" } };

            var column = Assert.IsType<DataGridTextColumn>(grid.Columns[0]);
            Assert.False(column.IsReadOnly);
            Assert.Equal(BindingMode.TwoWay, Assert.IsType<Binding>(column.Binding).Mode);
        });
    }

    private sealed class EditableGridRow
    {
        public string FloorLevel { get; set; } = string.Empty;
    }

    [Fact]
    public void Spreadsheet_ArrowKeysMoveActiveCellWhenGridHasFocus()
    {
        RunOnSTA(() =>
        {
            var resources = EnsureResources();
            var spreadsheet = new Spreadsheet
            {
                Style = (Style)resources["CONCOST.Spreadsheet.Base"],
                ActiveSheet = new SpreadsheetSheet { RowCount = 10, ColumnCount = 10 }
            };
            var window = new Window { Width = 500, Height = 350, Content = spreadsheet, ShowInTaskbar = false };
            try
            {
                window.Show();
                window.UpdateLayout();
                var presenter = (SpreadsheetGridPresenter)spreadsheet.Template.FindName("PART_GridPresenter", spreadsheet);
                Assert.True(presenter.Focus());
                RaisePreviewKey(presenter, Key.Right);
                RaisePreviewKey(presenter, Key.Down);

                Assert.Equal(1, spreadsheet.ActiveColumn);
                Assert.Equal(1, spreadsheet.ActiveRow);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Spreadsheet_ClickJitter_DoesNotStartRangeSelectionDrag()
    {
        RunOnSTA(() =>
        {
            var spreadsheet = new Spreadsheet();
            var start = new Point(100, 100);
            typeof(Spreadsheet).GetField("_selectionStartPoint", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(spreadsheet, start);

            var tryStartDrag = typeof(Spreadsheet).GetMethod("TryStartSelectionDrag", BindingFlags.Instance | BindingFlags.NonPublic)!;
            bool startedByClickJitter = (bool)tryStartDrag.Invoke(spreadsheet, new object[] { new Point(101, 101) })!;
            Assert.False(startedByClickJitter);

            var dragPoint = new Point(start.X, start.Y + SystemParameters.MinimumVerticalDragDistance + 1);
            bool startedAfterRealDrag = (bool)tryStartDrag.Invoke(spreadsheet, new object[] { dragPoint })!;
            Assert.True(startedAfterRealDrag);
        });
    }

    [Fact]
    public void SpreadsheetTheme_DynamicBrushesUpdateWhenSwitchingLightAndDark()
    {
        RunOnSTA(() =>
        {
            var resources = EnsureResources();
            var spreadsheet = new Spreadsheet
            {
                Style = (Style)resources["CONCOST.Spreadsheet.Base"],
                ActiveSheet = new SpreadsheetSheet { RowCount = 4, ColumnCount = 4 }
            };
            var window = new Window { Width = 500, Height = 350, Content = spreadsheet, ShowInTaskbar = false };
            try
            {
                ThemeManager.Current.ApplyTheme(window, AppTheme.Light);
                window.Show();
                window.UpdateLayout();
                Assert.Equal(Color.FromRgb(241, 245, 249), Assert.IsType<SolidColorBrush>(spreadsheet.HeaderBackground).Color);

                ThemeManager.Current.ApplyTheme(window, AppTheme.Dark);
                window.UpdateLayout();
                Assert.Equal(Color.FromRgb(30, 41, 59), Assert.IsType<SolidColorBrush>(window.TryFindResource("CONCOST.Brush.SurfaceSecondary")).Color);
                Assert.Equal(Color.FromRgb(30, 41, 59), Assert.IsType<SolidColorBrush>(spreadsheet.HeaderBackground).Color);
                Assert.Equal(Color.FromRgb(21, 29, 42), Assert.IsType<SolidColorBrush>(spreadsheet.Background).Color);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static void RaisePreviewKey(UIElement target, Key key)
    {
        var source = PresentationSource.FromVisual(target);
        Assert.NotNull(source);
        var args = new KeyEventArgs(Keyboard.PrimaryDevice, source, Environment.TickCount, key)
        {
            RoutedEvent = Keyboard.PreviewKeyDownEvent
        };
        target.RaiseEvent(args);
    }

    [Fact]
    public void ListBox_DirectionalNavigation_IsContained()
    {
        RunOnSTA(() =>
        {
            var res = EnsureResources();
            var style = res["CONCOST.ListBox.Base"] as Style;
            Assert.NotNull(style);

            var lb = new ListBox { Style = style };
            var nav = KeyboardNavigation.GetDirectionalNavigation(lb);
            Assert.Equal(KeyboardNavigationMode.Contained, nav);
        });
    }

    [Fact]
    public void ListView_DirectionalNavigation_IsContained()
    {
        RunOnSTA(() =>
        {
            var res = EnsureResources();
            var style = res["CONCOST.ListView.Base"] as Style;
            Assert.NotNull(style);

            var lv = new ListView { Style = style };
            var nav = KeyboardNavigation.GetDirectionalNavigation(lv);
            Assert.Equal(KeyboardNavigationMode.Contained, nav);
        });
    }

    [Fact]
    public void TreeView_DirectionalNavigation_IsContained()
    {
        RunOnSTA(() =>
        {
            var res = EnsureResources();
            var style = res["CONCOST.TreeView.Base"] as Style;
            Assert.NotNull(style);

            var tv = new TreeView { Style = style };
            var nav = KeyboardNavigation.GetDirectionalNavigation(tv);
            Assert.Equal(KeyboardNavigationMode.Contained, nav);
        });
    }

    [Fact]
    public void AutoCompleteTextBox_Style_AppliesHorizontalContentPadding()
    {
        RunOnSTA(() =>
        {
            var res = EnsureResources();
            var style = res["CONCOST.AutoCompleteTextBox.Base"] as Style;
            Assert.NotNull(style);

            var actb = new AutoCompleteTextBox { Style = style };
            Assert.Equal(new Thickness(10, 0, 10, 0), actb.Padding);
        });
    }
}
