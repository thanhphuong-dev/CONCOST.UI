using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.Controls;
using CONCOST.UI.Icons;
using CONCOST.UI.Services;

namespace CONCOST.UI.Showcase;

public partial class ShowcaseViewModel
{
    // =========================================================================
    // DEMO: layout, tagging, signature and wizard (the 1.2.0 batch)
    // =========================================================================

    /// <summary>A card for the masonry wall — the height is the point, the text is filler.</summary>
    public class MasonryNote
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public double Height { get; set; }
    }

    private object CreateMasonryPanelDemo()
    {
        var notes = new[]
        {
            new MasonryNote { Title = "Cột C1", Body = "Tiết diện 400×600, thép dọc 8Ø20, đai Ø8a150.", Height = 132 },
            new MasonryNote { Title = "Dầm D2", Body = "Nhịp 6.2 m, gối tựa tường gạch, kiểm tra võng L/250.", Height = 168 },
            new MasonryNote { Title = "Sàn tầng 3", Body = "Bê tông B25, dày 120 mm.", Height = 96 },
            new MasonryNote { Title = "Vách W4", Body = "Vách cứng 200 mm chịu tải ngang, bố trí thép hai lớp.", Height = 200 },
            new MasonryNote { Title = "Móng M1", Body = "Bệ cọc 2.4×2.4, 9 cọc Ø300 dài 18 m.", Height = 148 },
            new MasonryNote { Title = "Cầu thang", Body = "Bản thang dày 110, cốn BTCT.", Height = 108 },
            new MasonryNote { Title = "Mái", Body = "Sàn mái dày 130, chống thấm 2 lớp.", Height = 176 }
        };

        var masonry = new MasonryPanel
        {
            MinColumnWidth = 190,
            ColumnSpacing = 14,
            RowSpacing = 14,
            IsBalanced = true
        };

        foreach (MasonryNote note in notes) masonry.Children.Add(CreateMasonryCard(note));

        var auto = new Button { Content = "Tự động", Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(0, 0, 8, 10) };
        auto.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");

        var fixedThree = new Button { Content = "3 cột", Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(0, 0, 8, 10) };
        fixedThree.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");

        var toggleBalance = new Button { Content = "Cân bằng chiều cao: bật", Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(0, 0, 8, 10) };
        toggleBalance.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");

        auto.Click += (_, _) => masonry.Columns = 0;
        fixedThree.Click += (_, _) => masonry.Columns = 3;
        toggleBalance.Click += (_, _) =>
        {
            masonry.IsBalanced = !masonry.IsBalanced;
            toggleBalance.Content = masonry.IsBalanced ? "Cân bằng chiều cao: bật" : "Cân bằng chiều cao: tắt";
        };

        var actions = new WrapPanel();
        actions.Children.Add(auto);
        actions.Children.Add(fixedThree);
        actions.Children.Add(toggleBalance);

        var root = new StackPanel();
        root.Children.Add(actions);
        root.Children.Add(DemoSection(
            "Bố cục thác nước — đo theo cột, xếp vào cột ngắn nhất",
            masonry,
            xaml: "<controls:MasonryPanel MinColumnWidth=\"190\"\n" +
                  "                    ColumnSpacing=\"14\" RowSpacing=\"14\"\n" +
                  "                    IsBalanced=\"True\">\n" +
                  "    <!-- con nào cũng được, chiều cao tuỳ ý -->\n" +
                  "</controls:MasonryPanel>",
            csharp: "// Columns = 0 để số cột tính từ MinColumnWidth\n" +
                    "panel.Columns = 0;\n\n" +
                    "// Số cột và bề rộng thực tế của một cột\n" +
                    "int columns = panel.GetColumnCount(availableWidth);\n" +
                    "double width = panel.GetColumnWidth(availableWidth);\n\n" +
                    "// IsBalanced=false thì bám theo chiều cao trung bình,\n" +
                    "// để nhóm nội dung theo thứ tự đọc"));

        return root;
    }

    private static Border CreateMasonryCard(MasonryNote note)
    {
        var card = new Border
        {
            Height = note.Height,
            CornerRadius = new CornerRadius(10),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(14)
        };
        card.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
        card.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

        var stack = new StackPanel();

        var title = new TextBlock { Text = note.Title, FontSize = 12.5, FontWeight = FontWeights.SemiBold };
        title.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        stack.Children.Add(title);

        var body = new TextBlock
        {
            Text = note.Body,
            FontSize = 11.5,
            Margin = new Thickness(0, 6, 0, 0),
            TextWrapping = TextWrapping.Wrap
        };
        body.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        stack.Children.Add(body);

        card.Child = stack;
        return card;
    }

    private object CreateTagInputDemo()
    {
        var materials = new ObservableCollection<string> { "Bê tông B25", "Thép CB400" };

        var tagInput = new TagInput
        {
            ItemsSource = materials,
            Placeholder = "Gõ tên vật tư rồi Enter, hoặc dán “bê tông, thép, cốp pha”…",
            SuggestionSource = new[]
            {
                "Bê tông B20", "Bê tông B25", "Bê tông B30",
                "Thép CB300", "Thép CB400", "Thép CB500",
                "Cốp pha nhôm", "Cốp pha gỗ", "Gạch không nung", "Vữa xi măng"
            },
            MaxTags = 6
        };

        var readout = new TextBlock { FontSize = 11.5, Margin = new Thickness(0, 10, 0, 6), TextWrapping = TextWrapping.Wrap };

        void ReadTags()
        {
            readout.Text = materials.Count == 0
                ? "Chưa có thẻ nào."
                : $"{materials.Count} thẻ: {string.Join(" · ", materials)}";
        }

        tagInput.TagsChanged += (_, e) =>
        {
            ReadTags();
            ToastService.Info(e.IsAdded ? $"Thêm “{e.Text}”" : $"Bỏ “{e.Text}”");
        };

        ReadTags();
        readout.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var pickOnly = new TagInput
        {
            Placeholder = "Chỉ chọn được trong danh sách — tối đa 3",
            AllowFreeText = false,
            MaxTags = 3,
            SuggestionSource = new[] { "Tầng 1", "Tầng 2", "Tầng 3", "Tầng 4", "Tầng mái", "Hầm" },
            Margin = new Thickness(0, 4, 0, 0)
        };

        var stack = new StackPanel { MaxWidth = 640, HorizontalAlignment = HorizontalAlignment.Left };
        stack.Children.Add(tagInput);
        stack.Children.Add(readout);

        var caption = new TextBlock
        {
            Text = "Bộ lọc không dấu: gõ “be tong” vẫn thấy “Bê tông B25”.",
            FontSize = 11.5,
            Margin = new Thickness(0, 12, 0, 6),
            TextWrapping = TextWrapping.Wrap
        };
        caption.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        stack.Children.Add(caption);
        stack.Children.Add(pickOnly);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Thẻ vật tư — dán nhiều giá trị một lần",
            stack,
            xaml: "<controls:TagInput ItemsSource=\"{Binding Materials}\"\n" +
                  "                   SuggestionSource=\"{Binding AllMaterials}\"\n" +
                  "                   Placeholder=\"Gõ tên vật tư rồi Enter…\"\n" +
                  "                   MaxTags=\"6\"\n" +
                  "                   TagsChanged=\"OnTagsChanged\" />\n\n" +
                  "<controls:TagInput AllowFreeText=\"False\" MaxTags=\"3\"\n" +
                  "                   SuggestionSource=\"{Binding Floors}\" />",
            csharp: "// Danh sách ghi được thì control tự ghi ngược vào\n" +
                    "tags.ItemsSource = materials;   // ObservableCollection<string>\n\n" +
                    "// Tách chuỗi dán vào theo dấu phẩy\n" +
                    "tags.Delimiter = \",\";\n" +
                    "int added = tags.CommitText();\n\n" +
                    "tags.TagsChanged += (_, e) =>\n" +
                    "    Trace.WriteLine($\"{(e.IsAdded ? \"+\" : \"-\")}{e.Text}\");"));

        return root;
    }

    private object CreateSignaturePadDemo()
    {
        var pad = new SignaturePad
        {
            Height = 220,
            MaxWidth = 640,
            HorizontalAlignment = HorizontalAlignment.Left,
            Placeholder = "Ký vào đây",
            ShowGuideline = true
        };

        var status = new TextBlock { FontSize = 11.5, Margin = new Thickness(0, 10, 0, 0) };
        status.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        status.Text = "Chưa ký. Giữ chuột và kéo để vẽ nét.";

        var undo = new Button { Content = "Huỷ nét cuối", Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(0, 12, 8, 0) };
        undo.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");

        var clear = new Button { Content = "Xoá hết", Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(0, 12, 8, 0) };
        clear.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");

        var export = new Button { Content = "Xuất PNG", Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(0, 12, 8, 0) };
        export.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Primary");

        var guideline = new CheckBox { Content = "Hiện dòng kẻ", IsChecked = true, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 12, 0, 0) };

        undo.Click += (_, _) =>
        {
            if (!pad.Undo()) ToastService.Info("Không còn nét nào để huỷ.");
        };
        clear.Click += (_, _) => pad.Clear();
        export.Click += (_, _) =>
        {
            if (!pad.HasSignature)
            {
                ToastService.Info("Chưa có chữ ký để xuất.");
                return;
            }

            var image = pad.Export(1200, 0, Brushes.White);
            ToastService.Success($"Đã xuất {image.PixelWidth}×{image.PixelHeight} px");
        };
        guideline.Checked += (_, _) => pad.ShowGuideline = true;
        guideline.Unchecked += (_, _) => pad.ShowGuideline = false;

        pad.SignatureChanged += (_, _) =>
            status.Text = pad.HasSignature
                ? $"{pad.Strokes.Count} nét — SignatureImage đã sẵn sàng để lưu."
                : "Chưa ký. Giữ chuột và kéo để vẽ nét.";

        var actions = new WrapPanel();
        actions.Children.Add(undo);
        actions.Children.Add(clear);
        actions.Children.Add(export);
        actions.Children.Add(guideline);

        var stack = new StackPanel();
        stack.Children.Add(pad);
        stack.Children.Add(actions);
        stack.Children.Add(status);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Chữ ký tay — xuất thẳng sang bitmap",
            stack,
            xaml: "<controls:SignaturePad Height=\"220\"\n" +
                  "                     Placeholder=\"Ký vào đây\"\n" +
                  "                     ShowGuideline=\"True\"\n" +
                  "                     SignatureChanged=\"OnSignatureChanged\" />",
            csharp: "// Ảnh cập nhật sau mỗi nét (không phải mỗi lần di chuột)\n" +
                    "BitmapSource? image = pad.SignatureImage;\n\n" +
                    "// Xuất nền trắng 1200 px, hoặc nền trong suốt\n" +
                    "BitmapSource png = pad.Export(1200, 0, Brushes.White);\n\n" +
                    "pad.Undo();    // Ctrl+Z\n" +
                    "pad.Clear();   // Delete"));

        return root;
    }

    private object CreateWizardDemo()
    {
        var wizard = new Wizard
        {
            MaxWidth = 640,
            HorizontalAlignment = HorizontalAlignment.Left,
            BackText = "Quay lại",
            NextText = "Tiếp",
            FinishText = "Hoàn tất"
        };

        var projectName = new TextBox { Margin = new Thickness(0, 0, 0, 10), MinWidth = 260 };
        projectName.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.TextBox.Base");

        var projectStep = new WizardStep
        {
            Header = "Thông tin dự án",
            Description = "Tên công trình và địa điểm",
            IsValid = false
        };

        var projectBody = new StackPanel();
        projectBody.Children.Add(DemoCaption("Tên công trình (bắt buộc)"));
        projectBody.Children.Add(projectName);
        projectBody.Children.Add(DemoCaption("Địa điểm"));
        projectBody.Children.Add(new TextBox { Text = "Quận 7, TP.HCM", MinWidth = 260 });
        projectStep.Content = projectBody;

        projectName.TextChanged += (_, _) =>
            projectStep.IsValid = projectName.Text.Trim().Length > 0;

        var materialStep = new WizardStep
        {
            Header = "Vật liệu",
            Description = "Cấp bê tông và mác thép",
            IsValid = true
        };

        var grade = new ComboBox { MinWidth = 180, SelectedIndex = 1, HorizontalAlignment = HorizontalAlignment.Left };
        grade.Items.Add("B20");
        grade.Items.Add("B25");
        grade.Items.Add("B30");

        var materialBody = new StackPanel();
        materialBody.Children.Add(DemoCaption("Cấp bê tông"));
        materialBody.Children.Add(grade);
        materialStep.Content = materialBody;

        var summaryStep = new WizardStep
        {
            Header = "Xác nhận",
            Description = "Kiểm tra lại trước khi tạo hồ sơ",
            IsValid = true
        };

        var summary = new TextBlock { TextWrapping = TextWrapping.Wrap };
        summary.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        summaryStep.Content = summary;

        var readout = new TextBlock { FontSize = 11.5, Margin = new Thickness(0, 12, 0, 0), TextWrapping = TextWrapping.Wrap };
        readout.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        void UpdateReadout()
        {
            WizardStep? step = wizard.CurrentStep;
            readout.Text = step == null
                ? "Wizard chưa có bước nào."
                : $"Bước {step.Number}/{wizard.Steps.Count} — {step.Header}. " +
                  (wizard.CanGoNext ? "Có thể tiếp." : "Cần điền đủ thông tin bắt buộc.");
        }

        wizard.StepChanged += (_, _) =>
        {
            UpdateReadout();

            if (wizard.CurrentStep == summaryStep)
            {
                summary.Text = $"Công trình: {(projectName.Text.Trim().Length == 0 ? "(chưa đặt tên)" : projectName.Text.Trim())}\n" +
                               $"Địa điểm: Quận 7, TP.HCM\n" +
                               $"Bê tông: {grade.SelectedItem}";
            }
        };

        wizard.Finished += (_, _) => ToastService.Success("Đã tạo hồ sơ — wizard phát Finished.");

        wizard.Steps.Add(projectStep);
        wizard.Steps.Add(materialStep);
        wizard.Steps.Add(summaryStep);

        UpdateReadout();

        var stack = new StackPanel();
        stack.Children.Add(wizard);
        stack.Children.Add(readout);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Wizard nhiều bước — tiến chỉ khi bước hiện tại hợp lệ",
            stack,
            xaml: "<controls:Wizard Finished=\"OnFinished\"\n" +
                  "                BackText=\"Quay lại\" NextText=\"Tiếp\"\n" +
                  "                FinishText=\"Hoàn tất\">\n" +
                  "    <controls:WizardStep Header=\"Thông tin dự án\"\n" +
                  "                         Description=\"Tên công trình\"\n" +
                  "                         IsValid=\"{Binding HasProjectName}\">\n" +
                  "        <TextBox Text=\"{Binding ProjectName, UpdateSourceTrigger=PropertyChanged}\" />\n" +
                  "    </controls:WizardStep>\n" +
                  "    <controls:WizardStep Header=\"Xác nhận\" IsValid=\"True\">\n" +
                  "        <TextBlock Text=\"{Binding Summary}\" />\n" +
                  "    </controls:WizardStep>\n" +
                  "</controls:Wizard>",
            csharp: "// IsValid là của từng bước, wizard chỉ đọc\n" +
                    "step1.IsValid = viewModel.HasProjectName;\n\n" +
                    "wizard.Navigating += (_, e) =>\n" +
                    "{\n" +
                    "    if (e.ToIndex > 0 && !viewModel.Saved)\n" +
                    "        e.Cancel = true;      // giữ người dùng lại\n" +
                    "};\n\n" +
                    "wizard.Finished += (_, _) => CreateRecord();\n" +
                    "wizard.GoTo(0);                // quay về đầu\n" +
                    "wizard.CurrentIndex = 1;       // hoặc đặt trực tiếp"));

        return root;
    }
}
