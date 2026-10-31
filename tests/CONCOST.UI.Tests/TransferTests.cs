using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

public class TransferTests
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
    public void Transfer_Populate_DistributesSourceAndTargetCorrectly()
    {
        RunOnSTA(() =>
        {
            var res = EnsureResources();
            var transfer = new Transfer
            {
                ItemsSource = new[] { "Item 1", "Item 2", "Item 3", "Item 4" },
                TransferredItems = new List<object> { "Item 3" }
            };

            var style = res["CONCOST.Transfer.Base"] as Style;
            Assert.NotNull(style);
            transfer.Style = style;
            transfer.ApplyTemplate();

            Assert.Equal(3, transfer.SourceItems.Count);
            Assert.Single(transfer.TargetItems);
            Assert.Contains("Item 1", transfer.SourceItems);
            Assert.Contains("Item 2", transfer.SourceItems);
            Assert.Contains("Item 4", transfer.SourceItems);
            Assert.Contains("Item 3", transfer.TargetItems);
        });
    }

    [Fact]
    public void Transfer_TransferToTarget_And_ToSource_WorkCorrectly()
    {
        RunOnSTA(() =>
        {
            var res = EnsureResources();
            var transfer = new Transfer
            {
                ItemsSource = new[] { "A", "B", "C" }
            };

            var style = res["CONCOST.Transfer.Base"] as Style;
            transfer.Style = style;
            transfer.ApplyTemplate();

            Assert.Equal(3, transfer.SourceItems.Count);
            Assert.Empty(transfer.TargetItems);

            // Transfer all to target
            transfer.TransferAllToTarget();
            Assert.Empty(transfer.SourceItems);
            Assert.Equal(3, transfer.TargetItems.Count);

            // Transfer all to source
            transfer.TransferAllToSource();
            Assert.Equal(3, transfer.SourceItems.Count);
            Assert.Empty(transfer.TargetItems);
        });
    }

    [Fact]
    public void Transfer_Styles_ExistAndCanApply()
    {
        RunOnSTA(() =>
        {
            var res = EnsureResources();

            var styles = new[]
            {
                "CONCOST.Transfer.Base",
                "CONCOST.Transfer.Compact",
                "CONCOST.Transfer.Spacious"
            };

            foreach (var styleKey in styles)
            {
                var style = res[styleKey] as Style;
                Assert.NotNull(style);

                var transfer = new Transfer
                {
                    Style = style,
                    ItemsSource = new[] { "E1", "E2" }
                };
                transfer.ApplyTemplate();
                Assert.Equal(2, transfer.SourceItems.Count);
            }
        });
    }
}

