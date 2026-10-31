using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using CONCOST.UI.AttachedProperties;
using CONCOST.UI.Controls;
using CONCOST.UI.Icons;
using Xunit;

namespace CONCOST.UI.Tests;

public class TextBoxTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    private static readonly object _appLock = new();

    private static void EnsureApplication()
    {
        lock (_appLock)
        {
            if (Application.Current == null)
            {
                try
                {
                    new Application();
                }
                catch (InvalidOperationException)
                {
                }
            }
        }
    }

    [Theory]
    [InlineData(IconKind.Search, 10, 0)]
    [InlineData(IconKind.Search, 14, 0)]
    [InlineData(IconKind.Search, 6, 0)]
    [InlineData(IconKind.None, 10, 0)]
    [InlineData(IconKind.None, 14, 0)]
    [InlineData(IconKind.None, 6, 0)]
    public void TextBox_PlaceholderAndCaret_ArePixelPerfectAligned(IconKind iconKind, double padH, double padV)
    {
        RunOnSTA(() =>
        {
            EnsureApplication();
            var app = Application.Current;
            var dict = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/CONCOST.UI;component/Themes/Generic.xaml", UriKind.Absolute)
            };
            app.Resources.MergedDictionaries.Add(dict);

            var tb = new TextBox
            {
                Style = (Style)app.Resources[typeof(TextBox)],
                Width = 250,
                Height = 34,
                Padding = new Thickness(padH, padV, padH, padV)
            };
            InfoElement.SetPlaceholder(tb, "Search components...");
            IconElement.SetKind(tb, iconKind);

            tb.Measure(new Size(250, 34));
            tb.Arrange(new Rect(0, 0, 250, 34));
            tb.UpdateLayout();

            var sv = (ScrollViewer)tb.Template.FindName("PART_ContentHost", tb);
            var placeholder = (TextBlock)tb.Template.FindName("placeholder", tb);

            Assert.NotNull(sv);
            Assert.NotNull(placeholder);

            var svTransform = sv.TransformToAncestor(tb).Transform(new Point(0, 0));
            var phTransform = placeholder.TransformToAncestor(tb).Transform(new Point(0, 0));

            double caretX = svTransform.X + sv.Padding.Left + 2;
            double placeholderX = phTransform.X;

            Assert.Equal(placeholderX, caretX, precision: 1);
        });
    }

    [Theory]
    [InlineData(TextBoxInputType.DigitsOnly, "12345", true)]
    [InlineData(TextBoxInputType.DigitsOnly, "12a45", false)]
    [InlineData(TextBoxInputType.PositiveInteger, "500", true)]
    [InlineData(TextBoxInputType.PositiveInteger, "-50", false)]
    [InlineData(TextBoxInputType.Integer, "-500", true)]
    [InlineData(TextBoxInputType.Integer, "500", true)]
    [InlineData(TextBoxInputType.Integer, "5.5", false)]
    [InlineData(TextBoxInputType.PositiveDecimal, "350.25", true)]
    [InlineData(TextBoxInputType.PositiveDecimal, "350,25", true)]
    [InlineData(TextBoxInputType.PositiveDecimal, "-350.25", false)]
    [InlineData(TextBoxInputType.Decimal, "-350.25", true)]
    [InlineData(TextBoxInputType.Alphanumeric, "Beam01B", true)]
    [InlineData(TextBoxInputType.Alphanumeric, "Beam_01", false)]
    [InlineData(TextBoxInputType.Formula, "2*(L1+L2)/5.0", true)]
    public void TextBoxHelper_InputType_FilteringValidation(TextBoxInputType inputType, string input, bool expected)
    {
        RunOnSTA(() =>
        {
            var tb = new TextBox();
            bool result = TextBoxHelper.IsTextAllowed(tb, input, inputType);
            Assert.Equal(expected, result);
        });
    }

    [Fact]
    public void TextBoxHelper_CharacterCount_CalculatesLengthAccurately()
    {
        RunOnSTA(() =>
        {
            var tb = new TextBox { MaxLength = 50 };
            InfoElement.SetShowCharacterCount(tb, true);

            tb.Text = "Concrete C30/37";
            Assert.Equal("15/50", TextBoxHelper.GetCharacterCountText(tb));

            tb.MaxLength = 0;
            TextBoxHelper.UpdateCharacterCount(tb);
            Assert.Equal("15", TextBoxHelper.GetCharacterCountText(tb));
        });
    }

    [Fact]
    public void TextBoxHelper_AttachedProperties_CanBeSetAndRetrieved()
    {
        RunOnSTA(() =>
        {
            var tb = new TextBox();

            TextBoxHelper.SetSelectAllOnFocus(tb, true);
            Assert.True(TextBoxHelper.GetSelectAllOnFocus(tb));

            TextBoxHelper.SetClearOnEscape(tb, true);
            Assert.True(TextBoxHelper.GetClearOnEscape(tb));

            TextBoxHelper.SetCommitOnEnter(tb, true);
            Assert.True(TextBoxHelper.GetCommitOnEnter(tb));

            TextBoxHelper.SetInputType(tb, TextBoxInputType.PositiveDecimal);
            Assert.Equal(TextBoxInputType.PositiveDecimal, TextBoxHelper.GetInputType(tb));

            InfoElement.SetShowCharacterCount(tb, true);
            Assert.True(InfoElement.GetShowCharacterCount(tb));

            InfoElement.SetIsClearButtonAlwaysVisible(tb, true);
            Assert.True(InfoElement.GetIsClearButtonAlwaysVisible(tb));

            InfoElement.SetTitle(tb, "Cross Section Height");
            Assert.Equal("Cross Section Height", InfoElement.GetTitle(tb));

            InfoElement.SetRequired(tb, true);
            Assert.True(InfoElement.GetRequired(tb));

            InfoElement.SetTitlePlacement(tb, Dock.Left);
            Assert.Equal(Dock.Left, InfoElement.GetTitlePlacement(tb));
        });
    }
}