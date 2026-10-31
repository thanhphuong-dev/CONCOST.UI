using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

/// <summary>
/// A small window that follows the pointer while a drag is in progress, so the user can see what they
/// are carrying.
/// </summary>
/// <remarks>
/// <para>
/// WPF's own drag feedback is the cursor badge, which cannot show a caption and disappears behind the
/// pointer on a dark background. A ghost window is the usual answer: topmost, transparent, never
/// hit-testable, and offset from the pointer so it does not sit under it.
/// </para>
/// <para>
/// There is one ghost for the whole process — two drags at once is not a thing a user can do, and a
/// pool would only add lifetime bugs.
/// </para>
/// </remarks>
public static class DragGhost
{
    private static Window? _window;
    private static TextBlock? _caption;
    private static Icon? _glyph;

    /// <summary>True while the ghost is on screen.</summary>
    public static bool IsVisible => _window is { IsVisible: true };

    /// <summary>
    /// Shows the ghost with a caption and an optional icon. Calling it again while it is visible just
    /// updates the text.
    /// </summary>
    public static void Show(string caption, IconKind icon = IconKind.None)
    {
        EnsureWindow();

        if (_caption is not null) _caption.Text = caption;
        if (_glyph is not null)
        {
            _glyph.Kind = icon;
            _glyph.Visibility = icon == IconKind.None ? Visibility.Collapsed : Visibility.Visible;
        }

        if (_window is not null && !_window.IsVisible) _window.Show();
    }

    /// <summary>
    /// Places the ghost near a point expressed in a visual's coordinates. Pass the visual the pointer
    /// belongs to; the DPI conversion is handled here so the ghost lands where the pointer is.
    /// </summary>
    public static void Move(Visual? reference, Point point)
    {
        if (_window is null) return;

        double x = point.X;
        double y = point.Y;

        if (reference is not null)
        {
            var screen = reference.PointToScreen(point);
            var dpi = VisualTreeHelper.GetDpi(reference);

            x = screen.X / dpi.DpiScaleX;
            y = screen.Y / dpi.DpiScaleY;
        }

        _window.Left = x + 14;
        _window.Top = y + 14;
    }

    /// <summary>Takes the ghost off screen. Safe to call when it is not showing.</summary>
    public static void Hide()
    {
        if (_window is null) return;

        _window.Hide();
    }

    private static void EnsureWindow()
    {
        if (_window is not null) return;

        _glyph = new Icon
        {
            Kind = IconKind.None,
            Width = 14,
            Height = 14,
            Margin = new Thickness(0, 0, 7, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Visibility = Visibility.Collapsed
        };

        _caption = new TextBlock
        {
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center,
            MaxWidth = 260,
            TextTrimming = TextTrimming.CharacterEllipsis
        };

        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(_glyph);
        row.Children.Add(_caption);

        var frame = new Border
        {
            Child = row,
            Padding = new Thickness(11, 7, 13, 7),
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1)
        };

        frame.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
        frame.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Primary");
        _caption.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        _glyph.SetResourceReference(Icon.ForegroundProperty, "CONCOST.Brush.Primary");

        _window = new Window
        {
            WindowStyle = WindowStyle.None,
            AllowsTransparency = true,
            Background = Brushes.Transparent,
            Topmost = true,
            ShowInTaskbar = false,
            ShowActivated = false,
            ResizeMode = ResizeMode.NoResize,
            SizeToContent = SizeToContent.WidthAndHeight,
            IsHitTestVisible = false,
            Focusable = false,
            Content = frame,
            Opacity = 0.96
        };
    }
}
