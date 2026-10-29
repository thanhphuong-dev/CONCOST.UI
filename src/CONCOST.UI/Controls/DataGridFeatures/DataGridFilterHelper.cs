using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

public static class DataGridFilterHelper
{
    private static readonly Dictionary<StackedHeaderDataGrid, Dictionary<string, HashSet<string>>> _gridFilters = new();

    public static void ShowFilterPopup(StackedHeaderDataGrid grid, ColumnLeaf leaf, FrameworkElement targetPlacement)
    {
        if (grid == null || leaf == null || grid.ItemsSource == null) return;
        string bindingPath = leaf.BindingPath;
        if (string.IsNullOrEmpty(bindingPath)) return;

        // Collect distinct values from ItemsSource
        var allItems = new List<object>();
        foreach (var item in grid.ItemsSource)
        {
            if (item != null) allItems.Add(item);
        }

        var distinctValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in allItems)
        {
            var val = GetPropertyValue(item, bindingPath);
            distinctValues.Add(val?.ToString() ?? "(Blanks)");
        }

        var sortedValues = distinctValues.OrderBy(x => x).ToList();

        // Get currently selected filter values for this column
        if (!_gridFilters.TryGetValue(grid, out var colFilters))
        {
            colFilters = new Dictionary<string, HashSet<string>>();
            _gridFilters[grid] = colFilters;
        }

        HashSet<string> selectedValues;
        if (colFilters.TryGetValue(bindingPath, out var existingSelection))
        {
            selectedValues = new HashSet<string>(existingSelection, StringComparer.OrdinalIgnoreCase);
        }
        else
        {
            selectedValues = new HashSet<string>(distinctValues, StringComparer.OrdinalIgnoreCase);
        }

        // Build Popup UI
        var popup = new Popup
        {
            PlacementTarget = targetPlacement,
            Placement = PlacementMode.Bottom,
            StaysOpen = false,
            AllowsTransparency = true
        };

        var border = new Border
        {
            Width = 220,
            MaxHeight = 320,
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(10)
        };
        border.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
        border.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

        var mainPanel = new StackPanel();

        // Title
        var titleTb = new TextBlock
        {
            Text = $"Filter: {leaf.HeaderText}",
            FontWeight = FontWeights.Normal,
            FontSize = 12.0,
            Margin = new Thickness(0, 0, 0, 8)
        };
        titleTb.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.Primary");
        mainPanel.Children.Add(titleTb);

        // Search box
        var searchBox = new TextBox
        {
            Height = 28,
            Margin = new Thickness(0, 0, 0, 8),
            VerticalContentAlignment = VerticalAlignment.Center,
            Padding = new Thickness(6, 0, 6, 0)
        };
        mainPanel.Children.Add(searchBox);

        // Checkbox List Box
        var scrollViewer = new ScrollViewer { MaxHeight = 150, Margin = new Thickness(0, 0, 0, 8) };
        var checkListPanel = new StackPanel();

        var selectAllCb = new CheckBox
        {
            Content = "(Select All)",
            IsChecked = selectedValues.Count == distinctValues.Count,
            Margin = new Thickness(0, 2, 0, 4),
            FontWeight = FontWeights.Normal
        };
        checkListPanel.Children.Add(selectAllCb);

        var itemCheckboxes = new List<CheckBox>();
        foreach (var val in sortedValues)
        {
            var cb = new CheckBox
            {
                Content = val,
                IsChecked = selectedValues.Contains(val),
                Margin = new Thickness(0, 2, 0, 2)
            };
            itemCheckboxes.Add(cb);
            checkListPanel.Children.Add(cb);
        }

        selectAllCb.Click += (s, e) =>
        {
            bool check = selectAllCb.IsChecked == true;
            foreach (var cb in itemCheckboxes) cb.IsChecked = check;
        };

        searchBox.TextChanged += (s, e) =>
        {
            string q = searchBox.Text?.Trim() ?? "";
            foreach (var cb in itemCheckboxes)
            {
                string text = cb.Content?.ToString() ?? "";
                cb.Visibility = (string.IsNullOrEmpty(q) || text.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        };

        scrollViewer.Content = checkListPanel;
        mainPanel.Children.Add(scrollViewer);

        // Buttons Panel (Apply / Clear)
        var btnPanel = new UniformGrid { Columns = 2, Margin = new Thickness(0, 4, 0, 0) };
        var applyBtn = new Button { Content = "Apply", Height = 26, Margin = new Thickness(0, 0, 4, 0) };
        var clearBtn = new Button { Content = "Clear", Height = 26, Margin = new Thickness(4, 0, 0, 0) };

        applyBtn.Click += (s, e) =>
        {
            var newSelection = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var cb in itemCheckboxes)
            {
                if (cb.IsChecked == true)
                    newSelection.Add(cb.Content?.ToString() ?? "");
            }

            colFilters[bindingPath] = newSelection;
            ApplyAllFilters(grid);
            popup.IsOpen = false;
        };

        clearBtn.Click += (s, e) =>
        {
            colFilters.Remove(bindingPath);
            ApplyAllFilters(grid);
            popup.IsOpen = false;
        };

        btnPanel.Children.Add(applyBtn);
        btnPanel.Children.Add(clearBtn);
        mainPanel.Children.Add(btnPanel);

        border.Child = mainPanel;
        popup.Child = border;
        popup.IsOpen = true;
    }

    private static void ApplyAllFilters(StackedHeaderDataGrid grid)
    {
        var view = CollectionViewSource.GetDefaultView(grid.ItemsSource);
        if (view == null) return;

        if (!_gridFilters.TryGetValue(grid, out var colFilters) || colFilters.Count == 0)
        {
            view.Filter = null;
            grid.RebuildFooterSummary();
            return;
        }

        view.Filter = item =>
        {
            if (item == null) return false;
            foreach (var kvp in colFilters)
            {
                string path = kvp.Key;
                var allowed = kvp.Value;
                var val = GetPropertyValue(item, path);
                string strVal = val?.ToString() ?? "(Blanks)";
                if (!allowed.Contains(strVal)) return false;
            }
            return true;
        };

        grid.RebuildFooterSummary();
    }

    private static object? GetPropertyValue(object obj, string propertyName)
    {
        if (obj == null || string.IsNullOrEmpty(propertyName)) return null;
        try
        {
            var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            return prop?.GetValue(obj);
        }
        catch { return null; }
    }
}
