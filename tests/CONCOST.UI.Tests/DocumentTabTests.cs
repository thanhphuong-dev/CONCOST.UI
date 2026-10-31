using System;
using System.Windows;
using System.Windows.Controls;
using CONCOST.UI.Controls;
using CONCOST.UI.Core.MVVM;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// <see cref="DocumentTabControl"/> and <see cref="DocumentTabItem"/>: closing is a request that the
/// tab control resolves, so a host can veto it.
/// </summary>
public class DocumentTabTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    private static DocumentTabControl Build(params string[] headers)
    {
        var control = new DocumentTabControl { Width = 400, Height = 200 };

        foreach (var header in headers)
        {
            control.Items.Add(new DocumentTabItem { Header = header, Content = new Border() });
        }

        return control;
    }

    private static void Arrange(UIElement element)
    {
        var host = new Border { Child = element };
        host.Measure(new Size(400, 200));
        host.Arrange(new Rect(0, 0, 400, 200));
        host.UpdateLayout();
    }

    [Fact]
    public void DocumentTabItem_Defaults_MatchSpecifications()
    {
        RunOnSTA(() =>
        {
            var tab = new DocumentTabItem();

            Assert.False(tab.IsDirty);
            Assert.True(tab.CanClose);
            Assert.Null(tab.CloseCommand);
            Assert.Equal(CONCOST.UI.Icons.IconKind.None, tab.Icon);
        });
    }

    [Fact]
    public void CloseTab_RemovesTheTab()
    {
        RunOnSTA(() =>
        {
            var control = Build("A", "B", "C");
            var second = (DocumentTabItem)control.Items[1];

            control.CloseTab(second);

            Assert.Equal(2, control.Items.Count);
            Assert.DoesNotContain(second, control.Documents);
        });
    }

    [Fact]
    public void CloseTab_WithACommand_RunsItAndKeepsTheTab()
    {
        RunOnSTA(() =>
        {
            var control = Build("A", "B");
            object? executed = null;

            var tab = (DocumentTabItem)control.Items[0];
            tab.CloseCommand = new RelayCommand<object>(o => executed = o);

            control.CloseTab(tab);

            Assert.Same(tab, executed);
            Assert.Equal(2, control.Items.Count);
        });
    }

    [Fact]
    public void CloseTab_RespectsCanClose()
    {
        RunOnSTA(() =>
        {
            var control = Build("A", "B");
            var locked = (DocumentTabItem)control.Items[0];
            locked.CanClose = false;

            control.CloseTab(locked);

            Assert.Equal(2, control.Items.Count);
        });
    }

    [Fact]
    public void CloseOtherTabs_AndCloseAllTabs()
    {
        RunOnSTA(() =>
        {
            var control = Build("A", "B", "C");
            var keep = (DocumentTabItem)control.Items[1];

            control.CloseOtherTabs(keep);
            Assert.Single(control.Items);
            Assert.Same(keep, control.Items[0]);

            control.CloseAllTabs();
            Assert.Empty(control.Items);
        });
    }

    [Fact]
    public void TabClose_RaisedFromTheTab_ReachesTheControl()
    {
        RunOnSTA(() =>
        {
            var control = Build("A", "B");
            Arrange(control);

            var tab = (DocumentTabItem)control.Items[0];
            tab.Close();

            Assert.Single(control.Items);
            Assert.Same(control.Items[0], (DocumentTabItem)control.Items[0]);
        });
    }

    [Fact]
    public void DocumentTabControl_AppliesItsTemplate()
    {
        RunOnSTA(() =>
        {
            var control = Build("A", "B");
            Arrange(control);

            Assert.NotNull(control.Template);
            Assert.NotNull(control.ItemContainerStyle);
            Assert.Equal(typeof(DocumentTabItem), control.ItemContainerStyle.TargetType);
        });
    }
}
