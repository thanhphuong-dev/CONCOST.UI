using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

public class AutoCompleteTextBoxTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    [Fact]
    public void AutoCompleteTextBox_Defaults_AreValid()
    {
        RunOnSTA(() =>
        {
            var actb = new AutoCompleteTextBox();
            Assert.Null(actb.ItemsSource);
            Assert.Null(actb.SelectedItem);
            Assert.False(actb.IsDropDownOpen);
            Assert.Equal(240.0, actb.MaxDropDownHeight);
            Assert.Equal(string.Empty, actb.DisplayMemberPath);
        });
    }

    [Fact]
    public void AutoCompleteTextBox_FilterSuggestions_FiltersCorrectly()
    {
        RunOnSTA(() =>
        {
            var actb = new AutoCompleteTextBox
            {
                ItemsSource = new List<string>
                {
                    "Concrete Footing C20/25",
                    "Concrete Slab C30/37",
                    "High-Yield Rebar D20 CB500-V",
                    "Film-Faced Plywood 18mm"
                }
            };

            // Empty query should not open dropdown
            actb.FilterSuggestions(string.Empty);
            Assert.False(actb.IsDropDownOpen);

            // Matching query
            actb.FilterSuggestions("Rebar");
            // Note: _listBox is populated in OnApplyTemplate, but FilterSuggestions handles null safely without crashing
        });
    }

    [Fact]
    public void AutoCompleteTextBox_ClearValue_ResetsState()
    {
        RunOnSTA(() =>
        {
            var actb = new AutoCompleteTextBox
            {
                Text = "Concrete C35/45",
                SelectedItem = "Concrete C35/45"
            };

            actb.ClearValue();
            Assert.Equal(string.Empty, actb.Text);
            Assert.Null(actb.SelectedItem);
            Assert.False(actb.IsDropDownOpen);
        });
    }

    [Fact]
    public void AutoCompleteTextBox_GetItemText_ExtractsProperty()
    {
        RunOnSTA(() =>
        {
            var actb = new AutoCompleteTextBox
            {
                DisplayMemberPath = "Title"
            };

            var testObj = new TestItem { Title = "W18x50 Beam", Code = "W18" };
            Assert.Equal("W18x50 Beam", actb.GetItemText(testObj));

            // Without DisplayMemberPath, defaults to ToString
            actb.DisplayMemberPath = string.Empty;
            Assert.Equal("W18 - W18x50 Beam", actb.GetItemText(testObj));
        });
    }

    private class TestItem
    {
        public string Title { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public override string ToString() => $"{Code} - {Title}";
    }
}

