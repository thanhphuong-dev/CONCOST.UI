using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Threading;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The four panels that turn a plain <see cref="DataGrid"/> into a data tool: the column chooser,
/// the per-column filter row, the global search box and the summary row.
/// </summary>
public class DataGridFeaturePanelsTests
{
    #region Fixtures

    public class Beam
    {
        public string Name { get; set; } = string.Empty;
        public double Width { get; set; }
        public int Depth { get; set; }
        public string? Grade { get; set; } = string.Empty;
        public string? Note { get; set; }
        public Material Material { get; set; } = new Material();
    }

    public class Material
    {
        public string? Grade { get; set; } = string.Empty;
    }

    private static ObservableCollection<Beam> Beams() => new ObservableCollection<Beam>
    {
        new Beam { Name = "Beam B1", Width = 300, Depth = 500, Grade = "C30", Material = new Material { Grade = "C30" } },
        new Beam { Name = "Beam B2", Width = 400, Depth = 600, Grade = "C35", Material = new Material { Grade = "C35" } },
        new Beam { Name = "Column C1", Width = 500, Depth = 700, Grade = "C40", Material = new Material { Grade = "C40" } }
    };

    private static DataGridTextColumn Text(string header, string path, double width = 120) =>
        new DataGridTextColumn { Header = header, Binding = new Binding(path), Width = width };

    private static DataGrid Grid(System.Collections.IEnumerable source, params DataGridColumn[] columns)
    {
        var grid = new DataGrid { AutoGenerateColumns = false, CanUserAddRows = false, ItemsSource = source };
        foreach (var column in columns) grid.Columns.Add(column);
        return grid;
    }

    /// <summary>
    /// The count of rows the grid is showing. The collection view refreshes at data-binding
    /// priority, so the dispatcher has to run once before the filter is visible.
    /// </summary>
    private static int VisibleRows(DataGrid grid)
    {
        Pump();
        return ((System.Collections.IEnumerable)CollectionViewSource.GetDefaultView(grid.ItemsSource))
            .Cast<object>()
            .Count(row => !ReferenceEquals(row, CollectionView.NewItemPlaceholder));
    }

    private static void RunOnSta(Action action) => WpfApplicationHost.Run(action);

    private static void Pump()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ContextIdle,
            new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
    }

    /// <summary>Hosts content in a real window — template parts and column widths need layout.</summary>
    private sealed class Host : IDisposable
    {
        private readonly Window _window;

        internal Host(double width, FrameworkElement content)
        {
            _window = new Window
            {
                Width = width,
                Height = 400,
                Content = content,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None
            };

            _window.Show();
            _window.UpdateLayout();
            Pump();
            _window.UpdateLayout();
        }

        public void Dispose()
        {
            _window.Content = null;
            _window.Close();
        }
    }

    #endregion

    #region Filter panel

    [Fact]
    public void Filter_row_narrows_the_grid_through_its_default_view()
    {
        RunOnSta(() =>
        {
            var beams = Beams();
            var grid = Grid(beams, Text("Name", nameof(Beam.Name)));
            var panel = new DataGridFilterPanel { TargetGrid = grid };

            panel.Filters.Single(f => f.Header == "Name").FilterText = "beam";

            Assert.Equal(2, VisibleRows(grid));
        });
    }

    [Fact]
    public void Filter_row_is_case_insensitive_and_matches_anywhere_in_the_cell()
    {
        RunOnSta(() =>
        {
            var beams = Beams();
            var grid = Grid(beams, Text("Name", nameof(Beam.Name)));
            var panel = new DataGridFilterPanel { TargetGrid = grid };

            panel.Filters.Single(f => f.Header == "Name").FilterText = "b2";

            Assert.Equal(1, VisibleRows(grid));
        });
    }

    [Fact]
    public void Filter_row_follows_dotted_binding_paths()
    {
        RunOnSta(() =>
        {
            var beams = Beams();
            var grid = Grid(beams, Text("Grade", "Material.Grade"));
            var panel = new DataGridFilterPanel { TargetGrid = grid };

            panel.Filters.Single().FilterText = "C35";

            Assert.Equal(1, VisibleRows(grid));
        });
    }

    [Fact]
    public void Clearing_the_filter_row_restores_every_row()
    {
        RunOnSta(() =>
        {
            var beams = Beams();
            var grid = Grid(beams, Text("Name", nameof(Beam.Name)), Text("Grade", nameof(Beam.Grade)));
            var panel = new DataGridFilterPanel { TargetGrid = grid };

            panel.Filters[0].FilterText = "beam";
            panel.Filters[1].FilterText = "C30";
            Assert.Equal(1, VisibleRows(grid));

            panel.ClearFilters();

            Assert.Equal(3, VisibleRows(grid));
            Assert.All(panel.Filters, f => Assert.Equal(string.Empty, f.FilterText));
        });
    }

    [Fact]
    public void Filter_row_rebuilds_when_a_column_is_added()
    {
        RunOnSta(() =>
        {
            var beams = Beams();
            var grid = Grid(beams, Text("Name", nameof(Beam.Name)));
            var panel = new DataGridFilterPanel { TargetGrid = grid };

            Assert.Single(panel.Filters);

            grid.Columns.Add(Text("Depth", nameof(Beam.Depth)));

            Assert.Equal(2, panel.Filters.Count);
            Assert.Equal("Depth", panel.Filters[1].Header);
        });
    }

    [Fact]
    public void Filter_row_clears_its_predicate_when_the_target_goes_away()
    {
        RunOnSta(() =>
        {
            var beams = Beams();
            var grid = Grid(beams, Text("Name", nameof(Beam.Name)));
            var panel = new DataGridFilterPanel { TargetGrid = grid };

            panel.Filters[0].FilterText = "beam";
            Assert.Equal(2, VisibleRows(grid));

            panel.TargetGrid = null;

            Assert.Equal(3, VisibleRows(grid));
        });
    }

    #endregion

    #region Search panel

    [Fact]
    public void Search_matches_any_visible_column()
    {
        RunOnSta(() =>
        {
            var beams = Beams();
            var grid = Grid(beams, Text("Name", nameof(Beam.Name)), Text("Grade", nameof(Beam.Grade)));
            var panel = new DataGridSearchPanel { TargetGrid = grid };

            panel.SearchText = "C40";

            Assert.Equal(1, VisibleRows(grid));
            Assert.True(panel.IsSearching);
        });
    }

    [Fact]
    public void Search_ignores_hidden_columns()
    {
        RunOnSta(() =>
        {
            var beams = Beams();
            var grade = Text("Grade", nameof(Beam.Grade));
            var grid = Grid(beams, Text("Name", nameof(Beam.Name)), grade);
            var panel = new DataGridSearchPanel { TargetGrid = grid };

            grade.Visibility = Visibility.Collapsed;
            panel.SearchText = "C40";

            Assert.Equal(0, VisibleRows(grid));
        });
    }

    [Fact]
    public void Clearing_the_search_restores_every_row()
    {
        RunOnSta(() =>
        {
            var beams = Beams();
            var grid = Grid(beams, Text("Name", nameof(Beam.Name)));
            var panel = new DataGridSearchPanel { TargetGrid = grid };

            panel.SearchText = "column";
            Assert.Equal(1, VisibleRows(grid));

            panel.ClearSearch();

            Assert.Equal(3, VisibleRows(grid));
            Assert.False(panel.IsSearching);
        });
    }

    [Fact]
    public void Search_and_filter_row_narrow_each_other_instead_of_replacing()
    {
        RunOnSta(() =>
        {
            var beams = Beams();
            var grid = Grid(beams,
                Text("Name", nameof(Beam.Name)),
                Text("Grade", nameof(Beam.Grade)));

            var filter = new DataGridFilterPanel { TargetGrid = grid };
            var search = new DataGridSearchPanel { TargetGrid = grid };

            // "Beam" keeps B1 and B2; "C30" keeps only B1. Either alone leaves two rows.
            filter.Filters.Single(f => f.Header == "Grade").FilterText = "C30";
            search.SearchText = "Beam";

            Assert.Equal(1, VisibleRows(grid));

            // Dropping the search must leave the column filter in place.
            search.ClearSearch();

            Assert.Equal(1, VisibleRows(grid));

            filter.ClearFilters();

            Assert.Equal(3, VisibleRows(grid));
        });
    }

    #endregion

    #region Column chooser

    [Fact]
    public void Column_chooser_hides_and_restores_a_column()
    {
        RunOnSta(() =>
        {
            var beams = Beams();
            var grade = Text("Grade", nameof(Beam.Grade));
            var grid = Grid(beams, Text("Name", nameof(Beam.Name)), grade);
            var chooser = new DataGridColumnChooser { TargetGrid = grid };

            Assert.Equal(2, chooser.Columns.Count);

            chooser.Columns.Single(c => c.Header == "Grade").IsVisible = false;

            Assert.Equal(Visibility.Collapsed, grade.Visibility);

            chooser.Columns.Single(c => c.Header == "Grade").IsVisible = true;

            Assert.Equal(Visibility.Visible, grade.Visibility);
        });
    }

    [Fact]
    public void Column_chooser_show_all_brings_every_column_back()
    {
        RunOnSta(() =>
        {
            var beams = Beams();
            var grid = Grid(beams,
                Text("Name", nameof(Beam.Name)),
                Text("Grade", nameof(Beam.Grade)),
                Text("Depth", nameof(Beam.Depth)));
            var chooser = new DataGridColumnChooser { TargetGrid = grid };

            chooser.HideAll();
            Assert.All(grid.Columns, c => Assert.Equal(Visibility.Collapsed, c.Visibility));

            chooser.ShowAll();

            Assert.All(grid.Columns, c => Assert.Equal(Visibility.Visible, c.Visibility));
            Assert.All(chooser.Columns, c => Assert.True(c.IsVisible));
        });
    }

    [Fact]
    public void Column_chooser_tracks_columns_added_after_binding()
    {
        RunOnSta(() =>
        {
            var beams = Beams();
            var grid = Grid(beams, Text("Name", nameof(Beam.Name)));
            var chooser = new DataGridColumnChooser { TargetGrid = grid };

            grid.Columns.Add(Text("Depth", nameof(Beam.Depth)));

            Assert.Equal(2, chooser.Columns.Count);
        });
    }

    [Fact]
    public void Column_chooser_reflects_a_column_hidden_by_the_host()
    {
        RunOnSta(() =>
        {
            var beams = Beams();
            var grade = Text("Grade", nameof(Beam.Grade));
            var grid = Grid(beams, Text("Name", nameof(Beam.Name)), grade);
            var chooser = new DataGridColumnChooser { TargetGrid = grid };

            grade.Visibility = Visibility.Collapsed;

            // The list is re-read when the drop-down opens, so the host's own menu stays in sync.
            chooser.IsOpen = true;

            Assert.False(chooser.Columns.Single(c => c.Header == "Grade").IsVisible);
        });
    }

    [Fact]
    public void Column_chooser_opens_its_popup()
    {
        RunOnSta(() =>
        {
            var beams = Beams();
            var grid = Grid(beams, Text("Name", nameof(Beam.Name)), Text("Grade", nameof(Beam.Grade)));
            var chooser = new DataGridColumnChooser { TargetGrid = grid, HeaderText = "Columns" };
            var stack = new StackPanel();
            stack.Children.Add(chooser);
            stack.Children.Add(grid);

            using var host = new Host(480, stack);

            chooser.IsOpen = true;
            chooser.UpdateLayout();
            Pump();

            var toggle = chooser.Template.FindName("PART_Toggle", chooser) as ToggleButton;
            var popup = chooser.Template.FindName("PART_Popup", chooser) as Popup;

            Assert.NotNull(toggle);
            Assert.NotNull(popup);
            Assert.True(toggle!.IsChecked);
            Assert.True(popup!.IsOpen);

            chooser.IsOpen = false;
            Pump();

            Assert.False(toggle.IsChecked);
            Assert.False(popup.IsOpen);
        });
    }

    #endregion

    #region Summary row

    [Fact]
    public void Summary_sums_numeric_columns_and_counts_text_columns()
    {
        RunOnSta(() =>
        {
            var beams = Beams();
            var grid = Grid(beams,
                Text("Name", nameof(Beam.Name)),
                Text("Width", nameof(Beam.Width)),
                Text("Depth", nameof(Beam.Depth)));

            var summary = new DataGridSummary { TargetGrid = grid };
            summary.RefreshSummaries();

            var width = summary.Summaries.Single(s => s.Header == "Width");
            Assert.Equal(1200, width.Sum);
            Assert.Equal(400, width.Average);
            Assert.Equal(300, width.Min);
            Assert.Equal(500, width.Max);
            Assert.Equal(DataGridSummaryAggregate.Sum, width.Mode);

            var name = summary.Summaries.Single(s => s.Header == "Name");
            Assert.False(name.IsNumeric);
            Assert.Equal(DataGridSummaryAggregate.Count, name.Mode);
            Assert.Equal(3, name.Count);
            Assert.Contains("3", name.DisplayText);
        });
    }

    [Fact]
    public void Summary_honours_a_per_column_aggregate()
    {
        RunOnSta(() =>
        {
            var beams = Beams();
            var depth = Text("Depth", nameof(Beam.Depth));
            var grid = Grid(beams, depth);

            DataGridSummary.SetAggregateOnColumn(depth, DataGridSummaryAggregate.Average);

            var summary = new DataGridSummary { TargetGrid = grid };
            summary.RefreshSummaries();

            var item = summary.Summaries.Single();
            Assert.Equal(DataGridSummaryAggregate.Average, item.Mode);
            Assert.StartsWith("Avg", item.DisplayText);
        });
    }

    [Fact]
    public void Summary_follows_the_filtered_view()
    {
        RunOnSta(() =>
        {
            var beams = Beams();
            var grid = Grid(beams, Text("Name", nameof(Beam.Name)), Text("Width", nameof(Beam.Width)));
            var filter = new DataGridFilterPanel { TargetGrid = grid };
            var summary = new DataGridSummary { TargetGrid = grid };
            summary.RefreshSummaries();

            Assert.Equal(1200, summary.Summaries.Single(s => s.Header == "Width").Sum);

            filter.Filters.Single(f => f.Header == "Name").FilterText = "Beam";

            Assert.Equal(700, summary.Summaries.Single(s => s.Header == "Width").Sum);
        });
    }

    [Fact]
    public void Summary_reads_comma_decimals_in_the_current_culture()
    {
        RunOnSta(() =>
        {
            var previous = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");

                var beams = new ObservableCollection<Beam>
                {
                    new Beam { Name = "B1", Grade = "1,5" },
                    new Beam { Name = "B2", Grade = "2,5" }
                };

                var grid = Grid(beams, Text("Grade", nameof(Beam.Grade)));
                var summary = new DataGridSummary { TargetGrid = grid };
                summary.RefreshSummaries();

                var item = summary.Summaries.Single();
                Assert.True(item.IsNumeric);
                Assert.Equal(4, item.Sum);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = previous;
            }
        });
    }

    [Fact]
    public void Summary_tolerates_empty_and_text_cells()
    {
        RunOnSta(() =>
        {
            var beams = new ObservableCollection<Beam>
            {
                new Beam { Name = "B1", Grade = "C30" },
                new Beam { Name = "B2", Grade = null },
                new Beam { Name = "B3", Grade = "C35" }
            };

            var grid = Grid(beams, Text("Grade", nameof(Beam.Grade)));
            var summary = new DataGridSummary { TargetGrid = grid };
            summary.RefreshSummaries();

            var item = summary.Summaries.Single();
            Assert.False(item.IsNumeric);
            Assert.Equal(3, item.Count);
            Assert.Equal(DataGridSummaryAggregate.Count, item.Mode);
        });
    }

    [Fact]
    public void Summary_mirrors_the_column_widths_of_the_grid()
    {
        RunOnSta(() =>
        {
            var beams = Beams();
            var name = new DataGridTextColumn
            {
                Header = "Name",
                Binding = new Binding(nameof(Beam.Name)),
                Width = new DataGridLength(2, DataGridLengthUnitType.Star)
            };
            var grade = new DataGridTextColumn
            {
                Header = "Grade",
                Binding = new Binding(nameof(Beam.Grade)),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            };

            var grid = Grid(beams, name, grade);
            var summary = new DataGridSummary { TargetGrid = grid };

            var stack = new StackPanel();
            stack.Children.Add(grid);
            stack.Children.Add(summary);

            using var host = new Host(480, stack);

            summary.RefreshSummaries();
            grid.UpdateLayout();
            Pump();
            summary.RefreshSummaries();

            var nameCell = summary.Summaries.Single(s => s.Header == "Name");
            var gradeCell = summary.Summaries.Single(s => s.Header == "Grade");

            Assert.True(name.ActualWidth > 0);
            Assert.Equal(name.ActualWidth, nameCell.Width, 0);
            Assert.Equal(grade.ActualWidth, gradeCell.Width, 0);
            Assert.True(nameCell.Width > gradeCell.Width);
        });
    }

    #endregion
}
