using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>
/// The thin strip along an edge that holds the auto-hidden panels as tabs.
/// </summary>
/// <remarks>
/// Each tab is a label rotated to run along the strip (vertical strips) or written straight across it
/// (top and bottom). Clicking a tab raises <see cref="PaneInvoked"/>; the strip never opens the panel
/// itself, because only the manager knows where the flyout has to go.
/// </remarks>
public class AutoHidePane : FrameworkElement
{
    private readonly List<ToolWindow> _tabs = new();
    private int _hoverIndex = -1;

    public AutoHidePane()
    {
        ClipToBounds = true;
        Cursor = Cursors.Hand;
    }

    public static readonly DependencyProperty PanesProperty =
        DependencyProperty.Register(nameof(Panes), typeof(IEnumerable), typeof(AutoHidePane),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnPanesChanged));

    /// <summary>Panels shown as tabs. Non-<see cref="ToolWindow"/> entries are ignored.</summary>
    public IEnumerable? Panes
    {
        get => (IEnumerable?)GetValue(PanesProperty);
        set => SetValue(PanesProperty, value);
    }

    public static readonly DependencyProperty SideProperty =
        DependencyProperty.Register(nameof(Side), typeof(DockSide), typeof(AutoHidePane),
            new FrameworkPropertyMetadata(DockSide.Left, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Edge this strip belongs to.</summary>
    public DockSide Side
    {
        get => (DockSide)GetValue(SideProperty);
        set => SetValue(SideProperty, value);
    }

    public static readonly DependencyProperty TabSizeProperty =
        DependencyProperty.Register(nameof(TabSize), typeof(double), typeof(AutoHidePane),
            new FrameworkPropertyMetadata(28.0, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Thickness of the strip, in pixels. Default 28.</summary>
    public double TabSize
    {
        get => (double)GetValue(TabSizeProperty);
        set => SetValue(TabSizeProperty, value);
    }

    /// <summary>Raised when a tab is clicked.</summary>
    public event EventHandler<ToolWindow>? PaneInvoked;

    /// <summary>Panels currently shown as tabs.</summary>
    public IReadOnlyList<ToolWindow> Tabs => _tabs;

    private static void OnPanesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AutoHidePane strip)
        {
            strip.RebuildTabs();
            strip.InvalidateVisual();
        }
    }

    private void RebuildTabs()
    {
        _tabs.Clear();
        if (Panes is null) return;

        foreach (var item in Panes)
        {
            if (item is ToolWindow window) _tabs.Add(window);
        }

        _hoverIndex = -1;
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        if (_tabs.Count == 0) return;

        double thickness = Math.Max(12, TabSize);
        bool vertical = Side is DockSide.Left or DockSide.Right;

        var surface = TryFindResource("CONCOST.Brush.SurfaceSecondary") as Brush ?? Brushes.Gainsboro;
        var hover = TryFindResource("CONCOST.Brush.SurfaceHover") as Brush ?? Brushes.LightGray;
        var text = TryFindResource("CONCOST.Brush.TextSecondary") as Brush ?? Brushes.DimGray;
        var accent = TryFindResource("CONCOST.Brush.Primary") as Brush ?? Brushes.DodgerBlue;
        var border = TryFindResource("CONCOST.Brush.Border") as Brush ?? Brushes.Gray;

        double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        double length = vertical ? ActualHeight : ActualWidth;

        dc.DrawRectangle(surface, null, new Rect(0, 0, ActualWidth, ActualHeight));

        double offset = 0;
        for (int i = 0; i < _tabs.Count; i++)
        {
            var pane = _tabs[i];
            string label = pane.Header?.ToString() ?? string.Empty;

            double tabLength = Math.Max(72, Math.Min(180, 16 + label.Length * 7.2));
            if (offset >= length) break;
            if (offset + tabLength > length) tabLength = Math.Max(24, length - offset);

            var tabRect = vertical
                ? new Rect(0, offset, thickness, tabLength)
                : new Rect(offset, 0, tabLength, thickness);

            if (i == _hoverIndex) dc.DrawRectangle(hover, null, tabRect);
            if (pane.IsFlyoutOpen) dc.DrawRectangle(accent, null, vertical
                ? new Rect(0, offset, 2, tabLength)
                : new Rect(offset, 0, tabLength, 2));

            var formatted = new FormattedText(label, CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight, new Typeface("Segoe UI"), 11.5, text, dpi)
            {
                MaxTextWidth = Math.Max(8, tabLength - 12),
                MaxLineCount = 1,
                Trimming = TextTrimming.CharacterEllipsis
            };

            if (vertical)
            {
                dc.PushTransform(new RotateTransform(Side == DockSide.Left ? -90 : 90, thickness / 2, offset + tabLength / 2));
                dc.DrawText(formatted, new Point(thickness / 2 - formatted.Height / 2, offset + tabLength / 2 - formatted.Width / 2));
                dc.Pop();
            }
            else
            {
                dc.DrawText(formatted, new Point(offset + 6, (thickness - formatted.Height) / 2));
            }

            offset += tabLength;
        }

        var pen = new Pen(border, 1);
        if (vertical)
        {
            double x = Side == DockSide.Left ? ActualWidth - 0.5 : 0.5;
            dc.DrawLine(pen, new Point(x, 0), new Point(x, ActualHeight));
        }
        else
        {
            double y = Side == DockSide.Top ? ActualHeight - 0.5 : 0.5;
            dc.DrawLine(pen, new Point(0, y), new Point(ActualWidth, y));
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        int index = IndexAt(e.GetPosition(this));
        if (index == _hoverIndex) return;

        _hoverIndex = index;
        InvalidateVisual();
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (_hoverIndex < 0) return;

        _hoverIndex = -1;
        InvalidateVisual();
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        int index = IndexAt(e.GetPosition(this));
        if (index < 0 || index >= _tabs.Count) return;

        PaneInvoked?.Invoke(this, _tabs[index]);
        e.Handled = true;
    }

    private int IndexAt(Point point)
    {
        bool vertical = Side is DockSide.Left or DockSide.Right;
        double offset = vertical ? point.Y : point.X;

        double running = 0;
        for (int i = 0; i < _tabs.Count; i++)
        {
            string label = _tabs[i].Header?.ToString() ?? string.Empty;
            double tabLength = Math.Max(72, Math.Min(180, 16 + label.Length * 7.2));
            if (offset >= running && offset < running + tabLength) return i;
            running += tabLength;
        }

        return -1;
    }
}
