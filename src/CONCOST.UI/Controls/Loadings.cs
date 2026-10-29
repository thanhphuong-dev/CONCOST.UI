using System.Windows;
using System.Windows.Controls;

namespace CONCOST.UI.Controls;

public class LoadingBase : Control
{
    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(LoadingBase),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }
}

public class LoadingLine : LoadingBase
{
    static LoadingLine()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(LoadingLine), new FrameworkPropertyMetadata(typeof(LoadingLine)));
    }

    public static readonly DependencyProperty DotSizeProperty =
        DependencyProperty.Register(nameof(DotSize), typeof(double), typeof(LoadingLine),
            new PropertyMetadata(8.0));

    public double DotSize
    {
        get => (double)GetValue(DotSizeProperty);
        set => SetValue(DotSizeProperty, value);
    }
}

public class LoadingCircle : LoadingBase
{
    static LoadingCircle()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(LoadingCircle), new FrameworkPropertyMetadata(typeof(LoadingCircle)));
    }

    public static readonly DependencyProperty DotSizeProperty =
        DependencyProperty.Register(nameof(DotSize), typeof(double), typeof(LoadingCircle),
            new PropertyMetadata(6.0));

    public double DotSize
    {
        get => (double)GetValue(DotSizeProperty);
        set => SetValue(DotSizeProperty, value);
    }
}

public class WaitLoading : LoadingBase
{
    static WaitLoading()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(WaitLoading), new FrameworkPropertyMetadata(typeof(WaitLoading)));
    }
}

public class RingLoading : LoadingBase
{
    static RingLoading()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RingLoading), new FrameworkPropertyMetadata(typeof(RingLoading)));
    }
}

public class RollLoading : LoadingBase
{
    static RollLoading()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RollLoading), new FrameworkPropertyMetadata(typeof(RollLoading)));
    }
}

public class CycleLoading : LoadingBase
{
    static CycleLoading()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CycleLoading), new FrameworkPropertyMetadata(typeof(CycleLoading)));
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(CycleLoading),
            new PropertyMetadata(65.0));

    public static readonly DependencyProperty LoadTitleProperty =
        DependencyProperty.Register(nameof(LoadTitle), typeof(string), typeof(CycleLoading),
            new PropertyMetadata("Loading..."));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string LoadTitle
    {
        get => (string)GetValue(LoadTitleProperty);
        set => SetValue(LoadTitleProperty, value);
    }
}