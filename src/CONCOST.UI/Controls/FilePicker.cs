using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace CONCOST.UI.Controls;

public class FilePicker : Control
{
    static FilePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FilePicker), new FrameworkPropertyMetadata(typeof(FilePicker)));
    }

    public static readonly DependencyProperty SelectedPathProperty =
        DependencyProperty.Register(nameof(SelectedPath), typeof(string), typeof(FilePicker),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty FilterProperty =
        DependencyProperty.Register(nameof(Filter), typeof(string), typeof(FilePicker),
            new PropertyMetadata("All Files (*.*)|*.*"));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(FilePicker),
            new PropertyMetadata("Select File"));

    public string SelectedPath
    {
        get => (string)GetValue(SelectedPathProperty);
        set => SetValue(SelectedPathProperty, value);
    }

    public string Filter
    {
        get => (string)GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (GetTemplateChild("PART_BrowseButton") is Button btn)
        {
            btn.Click += OnBrowseClicked;
        }
    }

    private void OnBrowseClicked(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Filter = Filter,
            Title = Title,
            FileName = SelectedPath
        };

        if (dlg.ShowDialog() == true)
        {
            SelectedPath = dlg.FileName;
        }
    }
}
