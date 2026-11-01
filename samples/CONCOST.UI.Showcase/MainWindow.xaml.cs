using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using CONCOST.UI.Controls;
using CONCOST.UI.Core.Helpers;
using CONCOST.UI.Themes;
using CONCOST.UI.Themes.Tools;
using CONCOST.UI.Services;

namespace CONCOST.UI.Showcase;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Growl.RegisterPanel(PART_GrowlHost);

        Loaded += (_, _) =>
        {
            var workArea = SystemParameters.WorkArea;
            if (Height >= workArea.Height)
            {
                Height = Math.Max(MinHeight, workArea.Height - 60);
                Top = workArea.Top + 30;
            }
            if (Width >= workArea.Width)
            {
                Width = Math.Max(MinWidth, workArea.Width - 60);
                Left = workArea.Left + 30;
            }
        };
    }

    private void OnLanguageSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox selector || selector.SelectedItem is not ComboBoxItem item || item.Tag is not string cultureName)
            return;

        var culture = CultureInfo.GetCultureInfo(cultureName);
        LocalizationManager.Current.Culture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
    }

    private void OnCatalogSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // The stage reuses one ScrollViewer for every demo. Reset its offset after
        // the new content has been measured so each component opens at its title.
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => ShowcaseScrollViewer?.ScrollToTop()));
    }

    private void OnPaletteButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string hex && !string.IsNullOrEmpty(hex))
        {
            Point clickPoint = btn.TransformToAncestor(this).Transform(new Point(btn.ActualWidth / 2.0, btn.ActualHeight / 2.0));
            ThemeAnimationHelper.AnimateThemeChange(this, clickPoint, () =>
            {
                ThemeManager.Current.AccentColor = ColorHelper.FromHex(hex);
            });
        }
    }

    private void OnDarkModeTogglePreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is ShowcaseViewModel vm)
        {
            Point clickPoint = ToggleDarkSwitch.TransformToAncestor(this).Transform(new Point(ToggleDarkSwitch.ActualWidth / 2.0, ToggleDarkSwitch.ActualHeight / 2.0));
            bool newIsDark = !vm.IsDarkMode;

            ThemeAnimationHelper.AnimateThemeChange(this, clickPoint, () =>
            {
                vm.IsDarkMode = newIsDark;
            });
            e.Handled = true;
        }
    }
}
