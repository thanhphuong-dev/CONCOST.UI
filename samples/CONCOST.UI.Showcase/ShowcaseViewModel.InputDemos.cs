using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.Controls;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Showcase;

/// <summary>Demos for the interaction utilities: file drop, drop highlight and drag ghost.</summary>
public partial class ShowcaseViewModel
{
    private object CreateFileDropZoneDemo()
    {
        var zone = new FileDropZone
        {
            Filter = "*.dwg;*.dxf;*.xlsx",
            Caption = "Kéo tệp bản vẽ vào đây",
            Hint = "DWG, DXF hoặc XLSX — hoặc bấm để chọn",
            Width = 330,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        var count = new TextBlock
        {
            Text = "Chưa có tệp nào.",
            FontSize = 12,
            Margin = new Thickness(0, 0, 0, 8)
        };
        count.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var log = new TextBlock
        {
            FontFamily = new FontFamily("Consolas"),
            FontSize = 11.5,
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 19
        };
        log.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");

        zone.FilesDropped += (_, _) =>
        {
            count.Text = $"Đã nhận {zone.Files.Count} tệp (lọc *.dwg;*.dxf;*.xlsx):";
            log.Text = string.Join(Environment.NewLine, zone.Files.Select(System.IO.Path.GetFileName));
        };

        zone.BrowseRequested += (_, _) =>
        {
            count.Text = "Host sẽ mở hộp chọn tệp ở đây rồi gọi zone.AddFiles(...).";
        };

        // The card is only a target: DropTargetHighlight draws the outline, while the card keeps
        // owning whether it accepts the data.
        var card = new Border
        {
            Width = 230,
            Height = 150,
            Margin = new Thickness(18, 0, 0, 0),
            CornerRadius = new CornerRadius(10),
            VerticalAlignment = VerticalAlignment.Top,
            Padding = new Thickness(14)
        };
        card.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");

        var cardText = new TextBlock
        {
            Text = "Thẻ này chỉ sáng viền khi có tệp kéo qua — layout không đổi.",
            FontSize = 11.5,
            TextWrapping = TextWrapping.Wrap
        };
        cardText.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        card.Child = cardText;

        DropTargetHighlight.SetIsEnabled(card, true);

        var ghostButton = new Button
        {
            Content = "Xem DragGhost",
            Padding = new Thickness(12, 5, 12, 5),
            Margin = new Thickness(0, 10, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Left
        };
        ghostButton.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");

        ghostButton.Click += (_, _) =>
        {
            DragGhost.Show("Dầm D1 (300×600)", IconKind.Layers);
            DragGhost.Move(ghostButton, new Point(ghostButton.ActualWidth, ghostButton.ActualHeight / 2));

            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1500)
            };

            timer.Tick += (_, _) =>
            {
                timer.Stop();
                DragGhost.Hide();
            };

            timer.Start();
        };

        var targets = new Grid();
        targets.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        targets.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        Grid.SetColumn(zone, 0);
        Grid.SetColumn(card, 1);
        targets.Children.Add(zone);
        targets.Children.Add(card);

        var side = new StackPanel { Margin = new Thickness(18, 0, 0, 0), VerticalAlignment = VerticalAlignment.Top };
        side.Children.Add(count);
        side.Children.Add(log);
        side.Children.Add(ghostButton);

        var body = new Grid();
        body.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        Grid.SetColumn(targets, 0);
        Grid.SetColumn(side, 1);
        body.Children.Add(targets);
        body.Children.Add(side);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Vùng thả tệp, viền gợi ý và ghost",
            body,
            xaml: "<controls:FileDropZone Filter=\"*.dwg;*.dxf\"\n" +
                  "    Caption=\"Kéo tệp vào đây\"\n" +
                  "    FilesDropped=\"OnFilesDropped\" />\n\n" +
                  "<!-- Viền gợi ý cho bất kỳ phần tử nào -->\n" +
                  "<Border controls:DropTargetHighlight.IsEnabled=\"True\" />",
            csharp: "zone.FilesDropped += (_, e) => Load(e.Files);\n\n" +
                    "// Viền nét đứt khi có drag đi qua, không đổi layout\n" +
                    "DropTargetHighlight.SetIsEnabled(card, true);\n\n" +
                    "// Ghost theo con trỏ\n" +
                    "DragGhost.Show(\"Dầm D1\", IconKind.Layers);\n" +
                    "DragGhost.Move(this, e.GetPosition(this));\n" +
                    "DragGhost.Hide();"));

        return root;
    }
}
