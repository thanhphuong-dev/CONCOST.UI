using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls.Dashboard;

public enum TrendDirection
{
    Up,
    Down,
    Neutral
}

public class StatisticCard : Control
{
    static StatisticCard()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StatisticCard), new FrameworkPropertyMetadata(typeof(StatisticCard)));
    }

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(StatisticCard),
            new PropertyMetadata("Metric Title"));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(string), typeof(StatisticCard),
            new PropertyMetadata("0"));

    public static readonly DependencyProperty UnitProperty =
        DependencyProperty.Register(nameof(Unit), typeof(string), typeof(StatisticCard),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty TrendProperty =
        DependencyProperty.Register(nameof(Trend), typeof(string), typeof(StatisticCard),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty TrendDirectionProperty =
        DependencyProperty.Register(nameof(TrendDirection), typeof(TrendDirection), typeof(StatisticCard),
            new PropertyMetadata(TrendDirection.Up));

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(IconKind), typeof(StatisticCard),
            new PropertyMetadata(IconKind.None));

    public static readonly DependencyProperty ProgressProperty =
        DependencyProperty.Register(nameof(Progress), typeof(double), typeof(StatisticCard),
            new FrameworkPropertyMetadata(0.0, null, CoerceProgress));

    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(StatisticCard),
            new PropertyMetadata(string.Empty));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Unit
    {
        get => (string)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public string Trend
    {
        get => (string)GetValue(TrendProperty);
        set => SetValue(TrendProperty, value);
    }

    public TrendDirection TrendDirection
    {
        get => (TrendDirection)GetValue(TrendDirectionProperty);
        set => SetValue(TrendDirectionProperty, value);
    }

    public IconKind Icon
    {
        get => (IconKind)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new StatisticCardAutomationPeer(this);

    private static object CoerceProgress(DependencyObject d, object baseValue)
    {
        double value = (double)baseValue;
        return NumericCompatibility.IsFinite(value) ? NumericCompatibility.Clamp(value, 0.0, 100.0) : 0.0;
    }

    private sealed class StatisticCardAutomationPeer : FrameworkElementAutomationPeer
    {
        public StatisticCardAutomationPeer(StatisticCard owner) : base(owner) { }

        protected override string GetClassNameCore() => nameof(StatisticCard);

        protected override string GetNameCore()
        {
            var card = (StatisticCard)Owner;
            var value = string.Join(" ", new[] { card.Value, card.Unit }.Where(part => !string.IsNullOrWhiteSpace(part)));
            return string.Join(", ", new[] { card.Title, value, card.Trend, card.Subtitle }
                .Where(part => !string.IsNullOrWhiteSpace(part)));
        }
    }
}
