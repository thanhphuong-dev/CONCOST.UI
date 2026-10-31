using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Xunit;

namespace CONCOST.UI.Tests;

public class ComboBoxTests
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
    public void ComboBox_Popup_HasAllowsTransparencyTrue()
    {
        RunOnSTA(() =>
        {
            var res = EnsureResources();

            var cb = new ComboBox
            {
                Style = (Style)res[typeof(ComboBox)],
                Width = 200,
                Height = 34
            };
            cb.Items.Add("Item 1");
            cb.Items.Add("Item 2");

            cb.Measure(new Size(200, 34));
            cb.Arrange(new Rect(0, 0, 200, 34));
            cb.UpdateLayout();

            var popup = (Popup)cb.Template.FindName("PART_Popup", cb);
            Assert.NotNull(popup);
            Assert.True(popup.AllowsTransparency, "ComboBox PART_Popup must have AllowsTransparency=True to prevent black bars and black corners.");
        });
    }

    [Fact]
    public void DatePicker_Popup_HasAllowsTransparencyTrue()
    {
        RunOnSTA(() =>
        {
            var res = EnsureResources();

            var dp = new DatePicker
            {
                Style = (Style)res[typeof(DatePicker)],
                Width = 200,
                Height = 34
            };

            dp.Measure(new Size(200, 34));
            dp.Arrange(new Rect(0, 0, 200, 34));
            dp.UpdateLayout();

            var popup = (Popup)dp.Template.FindName("PART_Popup", dp);
            Assert.NotNull(popup);
            Assert.True(popup.AllowsTransparency, "DatePicker PART_Popup must have AllowsTransparency=True to prevent black bars and black corners.");
        });
    }
}
