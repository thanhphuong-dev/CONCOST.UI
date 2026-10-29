using System;
using System.Threading;
using System.Windows;
using System.Windows.Automation.Peers;
using CONCOST.UI.Controls.Dashboard;
using CONCOST.UI.Icons;
using Xunit;

namespace CONCOST.UI.Tests;

public class StatisticCardTests
{
    private static void RunOnSTA(Action action)
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (exception != null)
        {
            throw new AggregateException("STA test failed", exception);
        }
    }

    [Fact]
    public void StatisticCard_DefaultProperties_MatchSpecifications()
    {
        RunOnSTA(() =>
        {
            var card = new StatisticCard();

            Assert.Equal("Metric Title", card.Title);
            Assert.Equal("0", card.Value);
            Assert.Equal(string.Empty, card.Unit);
            Assert.Equal(string.Empty, card.Trend);
            Assert.Equal(TrendDirection.Up, card.TrendDirection);
            Assert.Equal(IconKind.None, card.Icon);
            Assert.Equal(0.0, card.Progress);
            Assert.Equal(string.Empty, card.Subtitle);
        });
    }

    [Fact]
    public void StatisticCard_CustomProperties_SetAndRetrieveCorrectly()
    {
        RunOnSTA(() =>
        {
            var card = new StatisticCard
            {
                Title = "Concrete Volume",
                Value = "1,250",
                Unit = "m³",
                Trend = "+14% vs baseline",
                TrendDirection = TrendDirection.Down,
                Icon = IconKind.Calculator,
                Progress = 85.5,
                Subtitle = "Phase 2 Core"
            };

            Assert.Equal("Concrete Volume", card.Title);
            Assert.Equal("1,250", card.Value);
            Assert.Equal("m³", card.Unit);
            Assert.Equal("+14% vs baseline", card.Trend);
            Assert.Equal(TrendDirection.Down, card.TrendDirection);
            Assert.Equal(IconKind.Calculator, card.Icon);
            Assert.Equal(85.5, card.Progress);
            Assert.Equal("Phase 2 Core", card.Subtitle);
        });
    }

    [Fact]
    public void StatisticCard_Progress_IsClampedAndNonFiniteValuesAreReset()
    {
        RunOnSTA(() =>
        {
            var card = new StatisticCard();

            card.Progress = -12;
            Assert.Equal(0, card.Progress);

            card.Progress = 125;
            Assert.Equal(100, card.Progress);

            card.Progress = double.NaN;
            Assert.Equal(0, card.Progress);

            card.Progress = double.PositiveInfinity;
            Assert.Equal(0, card.Progress);
        });
    }

    [Fact]
    public void StatisticCard_AutomationNameIncludesMetricAndContext()
    {
        RunOnSTA(() =>
        {
            var card = new StatisticCard
            {
                Title = "Concrete Volume",
                Value = "1,250",
                Unit = "m³",
                Trend = "Up 14 percent",
                Subtitle = "Phase 2"
            };

            var peer = UIElementAutomationPeer.CreatePeerForElement(card);

            Assert.NotNull(peer);
            Assert.Equal("Concrete Volume, 1,250 m³, Up 14 percent, Phase 2", peer!.GetName());
        });
    }
}
