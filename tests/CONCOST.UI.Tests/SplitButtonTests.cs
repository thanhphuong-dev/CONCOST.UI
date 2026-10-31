using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using CONCOST.UI.Controls;
using CONCOST.UI.Icons;
using Xunit;

namespace CONCOST.UI.Tests;

public class SplitButtonTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    [Fact]
    public void SplitButton_DefaultProperties_MatchSpecifications()
    {
        RunOnSTA(() =>
        {
            var sb = new SplitButton();

            Assert.Equal(PlacementMode.Bottom, sb.Placement);
            Assert.Equal(SplitButtonHitMode.Split, sb.HitMode);
            Assert.True(sb.AutoCloseOnClick);
            Assert.False(sb.StaysOpen);
            Assert.False(sb.IsOpenOnHover);
            Assert.Equal(14.0, sb.IconSize);
            Assert.Equal(new CornerRadius(6), sb.CornerRadius);
            Assert.Equal(Dock.Right, sb.DropDownButtonPosition);
            Assert.Equal(Dock.Left, sb.IconPlacement);
            Assert.Equal(IconKind.None, sb.Icon);
            Assert.Null(sb.Content);
            Assert.Null(sb.DropDownContent);
            Assert.False(sb.IsDropDownOpen);
        });
    }

    [Fact]
    public void SplitButton_DropDownOpenedAndClosedEvents_FireOnStateChange()
    {
        RunOnSTA(() =>
        {
            var sb = new SplitButton();
            bool openedFired = false;
            bool closedFired = false;

            sb.DropDownOpened += (s, e) => openedFired = true;
            sb.DropDownClosed += (s, e) => closedFired = true;

            sb.IsDropDownOpen = true;
            Assert.True(openedFired);
            Assert.False(closedFired);

            sb.IsDropDownOpen = false;
            Assert.True(closedFired);
        });
    }

    [Fact]
    public void SplitButton_HitMode_CanBeConfigured()
    {
        RunOnSTA(() =>
        {
            var sb = new SplitButton
            {
                HitMode = SplitButtonHitMode.DropDown,
                DropDownButtonPosition = Dock.Left,
                IconPlacement = Dock.Right,
                AutoCloseOnClick = false,
                StaysOpen = true,
                IsOpenOnHover = true
            };

            Assert.Equal(SplitButtonHitMode.DropDown, sb.HitMode);
            Assert.Equal(Dock.Left, sb.DropDownButtonPosition);
            Assert.Equal(Dock.Right, sb.IconPlacement);
            Assert.False(sb.AutoCloseOnClick);
            Assert.True(sb.StaysOpen);
            Assert.True(sb.IsOpenOnHover);
        });
    }

    [Fact]
    public void SplitButton_Command_UpdatesIsEnabled_BasedOnCanExecute()
    {
        RunOnSTA(() =>
        {
            var sb = new SplitButton();
            bool canExecute = false;

            var cmd = new TestRelayCommand(() => { }, () => canExecute);
            sb.Command = cmd;

            Assert.False(sb.IsEnabled);

            canExecute = true;
            cmd.RaiseCanExecuteChanged();
            Assert.True(sb.IsEnabled);
        });
    }

    private class TestRelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public TestRelayCommand(Action execute, Func<bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => _canExecute();
        public void Execute(object? parameter) => _execute();

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
