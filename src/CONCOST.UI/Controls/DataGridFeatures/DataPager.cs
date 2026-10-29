using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CONCOST.UI.Controls;

public class DataPager : Control, INotifyPropertyChanged
{
    public static readonly DependencyProperty PageIndexProperty =
        DependencyProperty.Register(nameof(PageIndex), typeof(int), typeof(DataPager),
            new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPagePropertyChanged, CoercePageIndex));

    public int PageIndex
    {
        get => (int)GetValue(PageIndexProperty);
        set => SetValue(PageIndexProperty, value);
    }

    public static readonly DependencyProperty PageSizeProperty =
        DependencyProperty.Register(nameof(PageSize), typeof(int), typeof(DataPager),
            new FrameworkPropertyMetadata(10, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPagePropertyChanged, CoercePageSize));

    public int PageSize
    {
        get => (int)GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    public static readonly DependencyProperty TotalItemsProperty =
        DependencyProperty.Register(nameof(TotalItems), typeof(int), typeof(DataPager),
            new PropertyMetadata(0, OnPagePropertyChanged, CoerceTotalItems));

    public int TotalItems
    {
        get => (int)GetValue(TotalItemsProperty);
        set => SetValue(TotalItemsProperty, value);
    }

    public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)TotalItems / Math.Max(1, PageSize)));

    public bool CanGoPrev => PageIndex > 1;
    public bool CanGoNext => PageIndex < TotalPages;

    private static void OnPagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DataPager p)
        {
            if (e.Property == TotalItemsProperty || e.Property == PageSizeProperty) p.CoerceValue(PageIndexProperty);
            p.OnPropertyChanged(nameof(TotalPages));
            p.OnPropertyChanged(nameof(CanGoPrev));
            p.OnPropertyChanged(nameof(CanGoNext));
            p.PageChanged?.Invoke(p, EventArgs.Empty);
        }
    }

    private static object CoercePageIndex(DependencyObject d, object value)
    {
        var pager = (DataPager)d;
        return NumericCompatibility.Clamp((int)value, 1, pager.TotalPages);
    }

    private static object CoercePageSize(DependencyObject d, object value) => Math.Max(1, (int)value);
    private static object CoerceTotalItems(DependencyObject d, object value) => Math.Max(0, (int)value);

    public event EventHandler? PageChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

    public void NextPage()
    {
        if (CanGoNext) PageIndex++;
    }

    public void PrevPage()
    {
        if (CanGoPrev) PageIndex--;
    }

    public void FirstPage()
    {
        PageIndex = 1;
    }

    public void LastPage()
    {
        PageIndex = TotalPages;
    }
}
