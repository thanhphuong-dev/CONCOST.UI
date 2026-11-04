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
            Caption = Loc.T("Kéo tệp bản vẽ vào đây", "Drop a drawing file here"),
            Hint = Loc.T("DWG, DXF hoặc XLSX — hoặc bấm để chọn", "DWG, DXF or XLSX — or click to browse"),
            Width = 330,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        var count = new TextBlock
        {
            Text = Loc.T("Chưa có tệp nào.", "No files yet."),
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
            count.Text = Loc.F("Đã nhận {0} tệp (lọc *.dwg;*.dxf;*.xlsx):", "Received {0} files (filter *.dwg;*.dxf;*.xlsx):", zone.Files.Count);
            log.Text = string.Join(Environment.NewLine, zone.Files.Select(System.IO.Path.GetFileName));
        };

        zone.BrowseRequested += (_, _) =>
        {
            count.Text = Loc.T("Host sẽ mở hộp chọn tệp ở đây rồi gọi zone.AddFiles(...).", "The host opens a file dialog here and then calls zone.AddFiles(...).");
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
            Text = Loc.T("Thẻ này chỉ sáng viền khi có tệp kéo qua — layout không đổi.", "This card only lights its border while a file is dragged over it — the layout never moves."),
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
            DragGhost.Show(Loc.T("Dầm D1 (300×600)", "Beam D1 (300×600)"), IconKind.Layers);
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
            Loc.T("Vùng thả tệp, viền gợi ý và ghost", "Drop zone, hint border and ghost"),
            body,
            xaml: "<controls:FileDropZone Filter=\"*.dwg;*.dxf\"\n" +
                  Loc.T("    Caption=\"Kéo tệp vào đây\"\n", "    Caption=\"Drop a file here\"\n") +
                  "    FilesDropped=\"OnFilesDropped\" />\n\n" +
                  Loc.T("<!-- Viền gợi ý cho bất kỳ phần tử nào -->\n", "<!-- Hint border for any element -->\n") +
                  "<Border controls:DropTargetHighlight.IsEnabled=\"True\" />",
            csharp: "zone.FilesDropped += (_, e) => Load(e.Files);\n\n" +
                    Loc.T("// Viền nét đứt khi có drag đi qua, không đổi layout\n", "// Dashed border while a drag passes over, layout unchanged\n") +
                    "DropTargetHighlight.SetIsEnabled(card, true);\n\n" +
                    Loc.T("// Ghost theo con trỏ\n", "// Ghost following the pointer\n") +
                    Loc.T("DragGhost.Show(\"Dầm D1\", IconKind.Layers);\n", "DragGhost.Show(\"Beam D1\", IconKind.Layers);\n") +
                    "DragGhost.Move(this, e.GetPosition(this));\n" +
                    "DragGhost.Hide();"));

        return root;
    }
}
