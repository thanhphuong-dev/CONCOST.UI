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
            new MasonryNote { Title = Loc.T("Cột C1", "Column C1"), Body = Loc.T("Tiết diện 400×600, thép dọc 8Ø20, đai Ø8a150.", "Section 400×600, 8Ø20 longitudinal rebar, Ø8a150 stirrups."), Height = 132 },
            new MasonryNote { Title = Loc.T("Dầm D2", "Beam D2"), Body = Loc.T("Nhịp 6.2 m, gối tựa tường gạch, kiểm tra võng L/250.", "6.2 m span, bearing on a masonry wall, L/250 deflection check."), Height = 168 },
            new MasonryNote { Title = Loc.T("Sàn tầng 3", "Level 3 slab"), Body = Loc.T("Bê tông B25, dày 120 mm.", "Concrete B25, 120 mm thick."), Height = 96 },
            new MasonryNote { Title = Loc.T("Vách W4", "Wall W4"), Body = Loc.T("Vách cứng 200 mm chịu tải ngang, bố trí thép hai lớp.", "200 mm shear wall carrying lateral load, two layers of reinforcement."), Height = 200 },
            new MasonryNote { Title = Loc.T("Móng M1", "Footing M1"), Body = Loc.T("Bệ cọc 2.4×2.4, 9 cọc Ø300 dài 18 m.", "Pile cap 2.4×2.4, 9 piles Ø300, 18 m long."), Height = 148 },
            new MasonryNote { Title = Loc.T("Cầu thang", "Stair"), Body = Loc.T("Bản thang dày 110, cốn BTCT.", "110 mm waist slab with an RC stringer."), Height = 108 },
            new MasonryNote { Title = Loc.T("Mái", "Roof"), Body = Loc.T("Sàn mái dày 130, chống thấm 2 lớp.", "130 mm roof slab, two waterproofing layers."), Height = 176 }
        };

        var masonry = new MasonryPanel
        {
            MinColumnWidth = 190,
            ColumnSpacing = 14,
            RowSpacing = 14,
            IsBalanced = true
        };

        foreach (MasonryNote note in notes) masonry.Children.Add(CreateMasonryCard(note));

        var auto = new Button { Content = Loc.T("Tự động", "Automatic"), Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(0, 0, 8, 10) };
        auto.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");

        var fixedThree = new Button { Content = Loc.T("3 cột", "3 columns"), Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(0, 0, 8, 10) };
        fixedThree.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");

        var toggleBalance = new Button { Content = Loc.T("Cân bằng chiều cao: bật", "Balance heights: on"), Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(0, 0, 8, 10) };
        toggleBalance.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");

        auto.Click += (_, _) => masonry.Columns = 0;
        fixedThree.Click += (_, _) => masonry.Columns = 3;
        toggleBalance.Click += (_, _) =>
        {
            masonry.IsBalanced = !masonry.IsBalanced;
            toggleBalance.Content = masonry.IsBalanced ? Loc.T("Cân bằng chiều cao: bật", "Balance heights: on") : Loc.T("Cân bằng chiều cao: tắt", "Balance heights: off");
        };

        var actions = new WrapPanel();
        actions.Children.Add(auto);
        actions.Children.Add(fixedThree);
        actions.Children.Add(toggleBalance);

        var root = new StackPanel();
        root.Children.Add(actions);
        root.Children.Add(DemoSection(
            Loc.T("Bố cục thác nước — đo theo cột, xếp vào cột ngắn nhất", "Masonry layout — measured by column, placed in the shortest one"),
            masonry,
            xaml: "<controls:MasonryPanel MinColumnWidth=\"190\"\n" +
                  "                    ColumnSpacing=\"14\" RowSpacing=\"14\"\n" +
                  "                    IsBalanced=\"True\">\n" +
                  Loc.T("    <!-- con nào cũng được, chiều cao tuỳ ý -->\n", "    <!-- any child works, any height -->\n") +
                  "</controls:MasonryPanel>",
            csharp: Loc.T("// Columns = 0 để số cột tính từ MinColumnWidth\n", "// Columns = 0 derives the column count from MinColumnWidth") +
                    "panel.Columns = 0;\n\n" +
                    Loc.T("// Số cột và bề rộng thực tế của một cột\n", "// Actual column count and column width") +
                    "int columns = panel.GetColumnCount(availableWidth);\n" +
                    "double width = panel.GetColumnWidth(availableWidth);\n\n" +
                    Loc.T("// IsBalanced=false thì bám theo chiều cao trung bình,\n", "// IsBalanced=false follows the average height,") +
                    Loc.T("// để nhóm nội dung theo thứ tự đọc", "// so content groups read in order")));

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
        var materials = new ObservableCollection<string> { Loc.T("Bê tông B25", "Concrete B25"), Loc.T("Thép CB400", "Rebar CB400") };

        var tagInput = new TagInput
        {
            ItemsSource = materials,
            Placeholder = Loc.T("Gõ tên vật tư rồi Enter, hoặc dán “bê tông, thép, cốp pha”…", "Type a material and press Enter, or paste “concrete, steel, formwork”…"),
            SuggestionSource = new[]
            {
                Loc.T("Bê tông B20", "Concrete B20"), Loc.T("Bê tông B25", "Concrete B25"), Loc.T("Bê tông B30", "Concrete B30"),
                Loc.T("Thép CB300", "Rebar CB300"), Loc.T("Thép CB400", "Rebar CB400"), Loc.T("Thép CB500", "Rebar CB500"),
                Loc.T("Cốp pha nhôm", "Aluminium formwork"), Loc.T("Cốp pha gỗ", "Timber formwork"), Loc.T("Gạch không nung", "Non-fired brick"), Loc.T("Vữa xi măng", "Cement mortar")
            },
            MaxTags = 6
        };

        var readout = new TextBlock { FontSize = 11.5, Margin = new Thickness(0, 10, 0, 6), TextWrapping = TextWrapping.Wrap };

        void ReadTags()
        {
            readout.Text = materials.Count == 0
                ? Loc.T("Chưa có thẻ nào.", "No tags yet.")
                : Loc.F("{0} thẻ: {1}", "{0} cards: {1}", materials.Count, string.Join(" · ", materials));
        }

        tagInput.TagsChanged += (_, e) =>
        {
            ReadTags();
            ToastService.Info(e.IsAdded ? Loc.F("Thêm “{0}”", "Added “{0}”", e.Text) : Loc.F("Bỏ “{0}”", "Removed “{0}”", e.Text));
        };

        ReadTags();
        readout.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var pickOnly = new TagInput
        {
            Placeholder = Loc.T("Chỉ chọn được trong danh sách — tối đa 3", "Pick from the list only — up to 3"),
            AllowFreeText = false,
            MaxTags = 3,
            SuggestionSource = new[] { Loc.T("Tầng 1", "Level 1"), Loc.T("Tầng 2", "Level 2"), Loc.T("Tầng 3", "Level 3"), Loc.T("Tầng 4", "Level 4"), Loc.T("Tầng mái", "Roof level"), Loc.T("Hầm", "Basement") },
            Margin = new Thickness(0, 4, 0, 0)
        };

        var stack = new StackPanel { MaxWidth = 640, HorizontalAlignment = HorizontalAlignment.Left };
        stack.Children.Add(tagInput);
        stack.Children.Add(readout);

        var caption = new TextBlock
        {
            Text = Loc.T("Bộ lọc không dấu: gõ “be tong” vẫn thấy “Bê tông B25”.", "Accent-insensitive filter: typing “be tong” still finds “Bê tông B25”."),
            FontSize = 11.5,
            Margin = new Thickness(0, 12, 0, 6),
            TextWrapping = TextWrapping.Wrap
        };
        caption.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        stack.Children.Add(caption);
        stack.Children.Add(pickOnly);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Thẻ vật tư — dán nhiều giá trị một lần", "Material tags — paste many values at once"),
            stack,
            xaml: "<controls:TagInput ItemsSource=\"{Binding Materials}\"\n" +
                  "                   SuggestionSource=\"{Binding AllMaterials}\"\n" +
                  Loc.T("                   Placeholder=\"Gõ tên vật tư rồi Enter…\"\n", "                   Placeholder=\"Type a material name, then Enter…\"\n") +
                  "                   MaxTags=\"6\"\n" +
                  "                   TagsChanged=\"OnTagsChanged\" />\n\n" +
                  "<controls:TagInput AllowFreeText=\"False\" MaxTags=\"3\"\n" +
                  "                   SuggestionSource=\"{Binding Floors}\" />",
            csharp: Loc.T("// Danh sách ghi được thì control tự ghi ngược vào\n", "// When the list is writable the control writes back into it") +
                    "tags.ItemsSource = materials;   // ObservableCollection<string>\n\n" +
                    Loc.T("// Tách chuỗi dán vào theo dấu phẩy\n", "// Splits a pasted string on the comma") +
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
            Placeholder = Loc.T("Ký vào đây", "Sign here"),
            ShowGuideline = true
        };

        var status = new TextBlock { FontSize = 11.5, Margin = new Thickness(0, 10, 0, 0) };
        status.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        status.Text = Loc.T("Chưa ký. Giữ chuột và kéo để vẽ nét.", "Not signed yet. Hold and drag to draw a stroke.");

        var undo = new Button { Content = Loc.T("Huỷ nét cuối", "Undo last stroke"), Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(0, 12, 8, 0) };
        undo.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");

        var clear = new Button { Content = Loc.T("Xoá hết", "Clear all"), Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(0, 12, 8, 0) };
        clear.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");

        var export = new Button { Content = Loc.T("Xuất PNG", "Export PNG"), Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(0, 12, 8, 0) };
        export.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Primary");

        var guideline = new CheckBox { Content = Loc.T("Hiện dòng kẻ", "Show guide line"), IsChecked = true, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 12, 0, 0) };

        undo.Click += (_, _) =>
        {
            if (!pad.Undo()) ToastService.Info(Loc.T("Không còn nét nào để huỷ.", "No stroke left to undo."));
        };
        clear.Click += (_, _) => pad.Clear();
        export.Click += (_, _) =>
        {
            if (!pad.HasSignature)
            {
                ToastService.Info(Loc.T("Chưa có chữ ký để xuất.", "Nothing to export yet."));
                return;
            }

            var image = pad.Export(1200, 0, Brushes.White);
            ToastService.Success(Loc.F("Đã xuất {0}×{1} px", "Exported {0}×{1} px", image.PixelWidth, image.PixelHeight));
        };
        guideline.Checked += (_, _) => pad.ShowGuideline = true;
        guideline.Unchecked += (_, _) => pad.ShowGuideline = false;

        pad.SignatureChanged += (_, _) =>
            status.Text = pad.HasSignature
                ? Loc.F("{0} nét — SignatureImage đã sẵn sàng để lưu.", "{0} strokes — SignatureImage is ready to save.", pad.Strokes.Count)
                : Loc.T("Chưa ký. Giữ chuột và kéo để vẽ nét.", "Not signed yet. Hold and drag to draw a stroke.");

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
            Loc.T("Chữ ký tay — xuất thẳng sang bitmap", "Handwritten signature — export straight to bitmap"),
            stack,
            xaml: "<controls:SignaturePad Height=\"220\"\n" +
                  Loc.T("                     Placeholder=\"Ký vào đây\"\n", "                     Placeholder=\"Sign here\"\n") +
                  "                     ShowGuideline=\"True\"\n" +
                  "                     SignatureChanged=\"OnSignatureChanged\" />",
            csharp: Loc.T("// Ảnh cập nhật sau mỗi nét (không phải mỗi lần di chuột)\n", "// The image updates after each stroke (not on every mouse move)") +
                    "BitmapSource? image = pad.SignatureImage;\n\n" +
                    Loc.T("// Xuất nền trắng 1200 px, hoặc nền trong suốt\n", "// Export on a 1200 px white ground, or transparent") +
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
            BackText = Loc.T("Quay lại", "Back"),
            NextText = Loc.T("Tiếp", "Next"),
            FinishText = Loc.T("Hoàn tất", "Finish")
        };

        var projectName = new TextBox { Margin = new Thickness(0, 0, 0, 10), MinWidth = 260 };
        projectName.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.TextBox.Base");

        var projectStep = new WizardStep
        {
            Header = Loc.T("Thông tin dự án", "Project information"),
            Description = Loc.T("Tên công trình và địa điểm", "Project name and location"),
            IsValid = false
        };

        var projectBody = new StackPanel();
        projectBody.Children.Add(DemoCaption(Loc.T("Tên công trình (bắt buộc)", "Project name (required)")));
        projectBody.Children.Add(projectName);
        projectBody.Children.Add(DemoCaption(Loc.T("Địa điểm", "Location")));
        projectBody.Children.Add(new TextBox { Text = Loc.T("Quận 7, TP.HCM", "District 7, HCMC"), MinWidth = 260 });
        projectStep.Content = projectBody;

        projectName.TextChanged += (_, _) =>
            projectStep.IsValid = projectName.Text.Trim().Length > 0;

        var materialStep = new WizardStep
        {
            Header = Loc.T("Vật liệu", "Materials"),
            Description = Loc.T("Cấp bê tông và mác thép", "Concrete and steel grade"),
            IsValid = true
        };

        var grade = new ComboBox { MinWidth = 180, SelectedIndex = 1, HorizontalAlignment = HorizontalAlignment.Left };
        grade.Items.Add("B20");
        grade.Items.Add("B25");
        grade.Items.Add("B30");

        var materialBody = new StackPanel();
        materialBody.Children.Add(DemoCaption(Loc.T("Cấp bê tông", "Concrete grade")));
        materialBody.Children.Add(grade);
        materialStep.Content = materialBody;

        var summaryStep = new WizardStep
        {
            Header = Loc.T("Xác nhận", "Confirm"),
            Description = Loc.T("Kiểm tra lại trước khi tạo hồ sơ", "Review before creating the record"),
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
                ? Loc.T("Wizard chưa có bước nào.", "The wizard has no steps.")
                : Loc.F("Bước {0}/{1} — {2}. ", "Step {0}/{1} — {2}. ", step.Number, wizard.Steps.Count, step.Header) +
                  (wizard.CanGoNext ? Loc.T("Có thể tiếp.", "Ready to continue.") : Loc.T("Cần điền đủ thông tin bắt buộc.", "Fill in the required fields."));
        }

        wizard.StepChanged += (_, _) =>
        {
            UpdateReadout();

            if (wizard.CurrentStep == summaryStep)
            {
                summary.Text = Loc.F("Công trình: {0}\n", "Project: {0}\n", projectName.Text.Trim().Length == 0 ? Loc.T("(chưa đặt tên)", "(unnamed)") : projectName.Text.Trim()) +
                               Loc.T("Địa điểm: Quận 7, TP.HCM\n", "Location: District 7, Ho Chi Minh City\n") +
                               Loc.F("Bê tông: {0}", "Concrete: {0}", grade.SelectedItem);
            }
        };

        wizard.Finished += (_, _) => ToastService.Success(Loc.T("Đã tạo hồ sơ — wizard phát Finished.", "Dossier created — the wizard raised Finished."));

        wizard.Steps.Add(projectStep);
        wizard.Steps.Add(materialStep);
        wizard.Steps.Add(summaryStep);

        UpdateReadout();

        var stack = new StackPanel();
        stack.Children.Add(wizard);
        stack.Children.Add(readout);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Wizard nhiều bước — tiến chỉ khi bước hiện tại hợp lệ", "Multi-step wizard — advance only when the current step is valid"),
            stack,
            xaml: "<controls:Wizard Finished=\"OnFinished\"\n" +
                  Loc.T("                BackText=\"Quay lại\" NextText=\"Tiếp\"\n", "                BackText=\"Back\" NextText=\"Next\"\n") +
                  Loc.T("                FinishText=\"Hoàn tất\">\n", "                FinishText=\"Finish\">\n") +
                  Loc.T("    <controls:WizardStep Header=\"Thông tin dự án\"\n", "    <controls:WizardStep Header=\"Project information\"\n") +
                  Loc.T("                         Description=\"Tên công trình\"\n", "                         Description=\"Project name\"\n") +
                  "                         IsValid=\"{Binding HasProjectName}\">\n" +
                  "        <TextBox Text=\"{Binding ProjectName, UpdateSourceTrigger=PropertyChanged}\" />\n" +
                  "    </controls:WizardStep>\n" +
                  Loc.T("    <controls:WizardStep Header=\"Xác nhận\" IsValid=\"True\">\n", "    <controls:WizardStep Header=\"Confirm\" IsValid=\"True\">\n") +
                  "        <TextBlock Text=\"{Binding Summary}\" />\n" +
                  "    </controls:WizardStep>\n" +
                  "</controls:Wizard>",
            csharp: Loc.T("// IsValid là của từng bước, wizard chỉ đọc\n", "// IsValid belongs to each step; the wizard only reads it") +
                    "step1.IsValid = viewModel.HasProjectName;\n\n" +
                    "wizard.Navigating += (_, e) =>\n" +
                    "{\n" +
                    "    if (e.ToIndex > 0 && !viewModel.Saved)\n" +
                    Loc.T("        e.Cancel = true;      // giữ người dùng lại\n", "        e.Cancel = true;      // keep the user here") +
                    "};\n\n" +
                    "wizard.Finished += (_, _) => CreateRecord();\n" +
                    Loc.T("wizard.GoTo(0);                // quay về đầu\n", "wizard.GoTo(0);                // back to the start") +
                    Loc.T("wizard.CurrentIndex = 1;       // hoặc đặt trực tiếp", "wizard.CurrentIndex = 1;       // or assign it directly")));

        return root;
    }
}
