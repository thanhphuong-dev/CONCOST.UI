using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

public enum CellType
{
    Text,
    Numeric,
    Progress,
    Badge,
    Rating,
    Date,
    Hyperlink,
    RowExpander,
    CheckBox
}

public class DetailsVisibilityToBoolConverter : IValueConverter
{
    public static readonly DetailsVisibilityToBoolConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility v && v == Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value is bool b && b) ? Visibility.Visible : Visibility.Collapsed;
    }
}

public class StatusToBackgroundConverter : IValueConverter
{
    public static readonly StatusToBackgroundConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string status = value?.ToString()?.Trim() ?? "";

        return status.ToUpperInvariant() switch
        {
            "APPROVED" or "OK" or "DONE" or "HOÀN THÀNH" => new SolidColorBrush(Color.FromRgb(220, 252, 231)),   // #DCFCE7 Emerald light
            "IN REVIEW" or "REVIEW" or "CHỜ DUYỆT"        => new SolidColorBrush(Color.FromRgb(224, 242, 254)),   // #E0F2FE Sky light
            "IN PROGRESS" or "ĐANG LÀM"                  => new SolidColorBrush(Color.FromRgb(237, 233, 254)),   // #EDE9FE Violet light
            "PENDING" or "WARNING" or "TẠM DỪNG"        => new SolidColorBrush(Color.FromRgb(254, 243, 199)),   // #FEF3C7 Amber light
            "REJECTED" or "ERROR" or "HUỶ"              => new SolidColorBrush(Color.FromRgb(254, 226, 226)),   // #FEE2E2 Red light
            _                                           => new SolidColorBrush(Color.FromRgb(241, 245, 249))    // #F1F5F9 Slate light
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class StatusToForegroundConverter : IValueConverter
{
    public static readonly StatusToForegroundConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string status = value?.ToString()?.Trim() ?? "";

        return status.ToUpperInvariant() switch
        {
            "APPROVED" or "OK" or "DONE" or "HOÀN THÀNH" => new SolidColorBrush(Color.FromRgb(22, 101, 52)),     // #166534 Emerald dark
            "IN REVIEW" or "REVIEW" or "CHỜ DUYỆT"        => new SolidColorBrush(Color.FromRgb(7, 89, 133)),      // #075985 Sky dark
            "IN PROGRESS" or "ĐANG LÀM"                  => new SolidColorBrush(Color.FromRgb(107, 33, 168)),    // #6B21A8 Violet dark
            "PENDING" or "WARNING" or "TẠM DỪNG"        => new SolidColorBrush(Color.FromRgb(180, 83, 9)),      // #B45309 Amber dark
            "REJECTED" or "ERROR" or "HUỶ"              => new SolidColorBrush(Color.FromRgb(153, 27, 27)),     // #991B1B Red dark
            _                                           => new SolidColorBrush(Color.FromRgb(71, 85, 105))      // #475569 Slate dark
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class RatingToStarsConverter : IValueConverter
{
    public static readonly RatingToStarsConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        int rating = 0;
        if (value is int r) rating = r;
        else if (value is double d) rating = (int)Math.Round(d);
        else if (value != null && int.TryParse(value.ToString(), out int r2)) rating = r2;

        rating = Math.Max(0, Math.Min(5, rating));
        string filled = new string('★', rating);
        string empty = new string('☆', 5 - rating);
        return filled + empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public static class DataGridCellTemplateFactory
{
    public static DataTemplate CreateProgressTemplate(string bindingPath, double max = 100)
    {
        var dt = new DataTemplate();
        var rootGrid = new FrameworkElementFactory(typeof(Grid));
        rootGrid.SetValue(FrameworkElement.MarginProperty, new Thickness(10, 0, 10, 0));
        rootGrid.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

        var col0 = new FrameworkElementFactory(typeof(ColumnDefinition));
        col0.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
        var col1 = new FrameworkElementFactory(typeof(ColumnDefinition));
        col1.SetValue(ColumnDefinition.WidthProperty, GridLength.Auto);

        rootGrid.AppendChild(col0);
        rootGrid.AppendChild(col1);

        // Thin Sleek ProgressBar without inner text
        var pbFactory = new FrameworkElementFactory(typeof(ProgressBar));
        pbFactory.SetValue(Grid.ColumnProperty, 0);
        pbFactory.SetValue(ProgressBar.MaximumProperty, max);
        pbFactory.SetValue(ProgressBar.MinimumProperty, 0.0);
        pbFactory.SetValue(FrameworkElement.HeightProperty, 8.0);
        pbFactory.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 0, 8, 0));
        pbFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        pbFactory.SetValue(CONCOST.UI.AttachedProperties.ProgressBarElement.ShowTextProperty, false);
        pbFactory.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.ProgressBar.Thin");
        pbFactory.SetBinding(ProgressBar.ValueProperty, new Binding(bindingPath));
        rootGrid.AppendChild(pbFactory);

        // Percentage Text cleanly on the right
        var tbFactory = new FrameworkElementFactory(typeof(TextBlock));
        tbFactory.SetValue(Grid.ColumnProperty, 1);
        tbFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
        tbFactory.SetValue(TextBlock.FontSizeProperty, 11.0);
        tbFactory.SetValue(TextBlock.FontWeightProperty, FontWeights.Normal);
        tbFactory.SetValue(TextBlock.MinWidthProperty, 34.0);
        tbFactory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Right);
        tbFactory.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        tbFactory.SetBinding(TextBlock.TextProperty, new Binding(bindingPath) { StringFormat = "{0:0}%" });
        rootGrid.AppendChild(tbFactory);

        dt.VisualTree = rootGrid;
        return dt;
    }

    public static DataTemplate CreateBadgeTemplate(string bindingPath)
    {
        var dt = new DataTemplate();
        var borderFactory = new FrameworkElementFactory(typeof(Border));
        borderFactory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        borderFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(12));
        borderFactory.SetValue(Border.PaddingProperty, new Thickness(10, 3, 10, 3));
        borderFactory.SetBinding(Border.BackgroundProperty, new Binding(bindingPath) { Converter = StatusToBackgroundConverter.Instance });

        var tbFactory = new FrameworkElementFactory(typeof(TextBlock));
        tbFactory.SetValue(TextBlock.FontSizeProperty, 11.0);
        tbFactory.SetValue(TextBlock.FontWeightProperty, FontWeights.Normal);
        tbFactory.SetBinding(TextBlock.ForegroundProperty, new Binding(bindingPath) { Converter = StatusToForegroundConverter.Instance });
        tbFactory.SetBinding(TextBlock.TextProperty, new Binding(bindingPath));

        borderFactory.AppendChild(tbFactory);
        dt.VisualTree = borderFactory;
        return dt;
    }

    public static DataTemplate CreateRatingTemplate(string bindingPath)
    {
        var dt = new DataTemplate();
        var tbFactory = new FrameworkElementFactory(typeof(TextBlock));
        tbFactory.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        tbFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
        tbFactory.SetValue(TextBlock.FontSizeProperty, 13.5);
        tbFactory.SetValue(TextBlock.FontWeightProperty, FontWeights.Normal);
        tbFactory.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(Color.FromRgb(217, 119, 6))); // #D97706 Amber Dark
        tbFactory.SetBinding(TextBlock.TextProperty, new Binding(bindingPath) { Converter = RatingToStarsConverter.Instance });
        dt.VisualTree = tbFactory;
        return dt;
    }

    public static DataTemplate CreateDateTemplate(string bindingPath, string format = "dd/MM/yyyy")
    {
        var dt = new DataTemplate();
        var tbFactory = new FrameworkElementFactory(typeof(TextBlock));
        tbFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
        tbFactory.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        tbFactory.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        tbFactory.SetBinding(TextBlock.TextProperty, new Binding(bindingPath) { StringFormat = format });
        dt.VisualTree = tbFactory;
        return dt;
    }

    public static DataTemplate CreateNumericTemplate(string bindingPath, string format = "N0", HorizontalAlignment alignment = HorizontalAlignment.Right)
    {
        var dt = new DataTemplate();
        var tbFactory = new FrameworkElementFactory(typeof(TextBlock));
        tbFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
        tbFactory.SetValue(TextBlock.HorizontalAlignmentProperty, alignment);
        tbFactory.SetValue(TextBlock.PaddingProperty, new Thickness(0, 0, 8, 0));
        tbFactory.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        tbFactory.SetBinding(TextBlock.TextProperty, new Binding(bindingPath) { StringFormat = "{0:" + format + "}" });
        dt.VisualTree = tbFactory;
        return dt;
    }

    public static DataTemplate CreateHyperlinkTemplate(string bindingPath)
    {
        var dt = new DataTemplate();
        var tbFactory = new FrameworkElementFactory(typeof(TextBlock));
        tbFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
        tbFactory.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        tbFactory.SetValue(TextBlock.CursorProperty, System.Windows.Input.Cursors.Hand);
        tbFactory.SetValue(TextBlock.TextDecorationsProperty, TextDecorations.Underline);
        tbFactory.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.Primary");
        tbFactory.SetBinding(TextBlock.TextProperty, new Binding(bindingPath));
        dt.VisualTree = tbFactory;
        return dt;
    }

    public static DataTemplate CreateRowExpanderTemplate()
    {
        var dt = new DataTemplate();
        var factory = new FrameworkElementFactory(typeof(System.Windows.Controls.Primitives.ToggleButton));
        factory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        factory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        factory.SetValue(FrameworkElement.WidthProperty, 22.0);
        factory.SetValue(FrameworkElement.HeightProperty, 22.0);
        factory.SetValue(FrameworkElement.CursorProperty, System.Windows.Input.Cursors.Hand);
        factory.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.RowExpander.Button");

        factory.SetBinding(System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty, new Binding
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGridRow), 1),
            Path = new PropertyPath(nameof(DataGridRow.DetailsVisibility)),
            Converter = DetailsVisibilityToBoolConverter.Instance,
            Mode = BindingMode.TwoWay
        });

        dt.VisualTree = factory;
        return dt;
    }

    public static DataTemplate CreateCheckBoxTemplate(string bindingPath)
    {
        var dt = new DataTemplate();
        var cbFactory = new FrameworkElementFactory(typeof(CheckBox));
        cbFactory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        cbFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        cbFactory.SetBinding(System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty, new Binding(bindingPath) { Mode = BindingMode.TwoWay });
        dt.VisualTree = cbFactory;
        return dt;
    }
}
