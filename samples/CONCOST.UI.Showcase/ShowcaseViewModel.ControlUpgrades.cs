using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.Controls;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Showcase;

/// <summary>
/// Pages for the controls rebuilt in the 1.2.0 control-upgrade pass — they existed in the assembly
/// before, but four of them had no page (and two of those had no template) so nobody could see them.
/// </summary>
public partial class ShowcaseViewModel
{
    private object CreateOtpInputDemo()
    {
        var root = new StackPanel();

        // Section 1: live verification code with a status line.
        var otp = new OtpInput { Length = 4, HorizontalAlignment = HorizontalAlignment.Left };
        var status = new TextBlock { FontSize = 12, Margin = new Thickness(0, 10, 0, 0) };
        status.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        status.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding
        {
            Path = new PropertyPath(nameof(OtpInput.Value)),
            ElementName = "OtpLive",
            StringFormat = "Giá trị hiện tại: '{0}'"
        });
        otp.Completed += (_, _) => Growl.Success("Mã đã nhập đủ — kiểm tra với máy chủ.");
        otp.Name = "OtpLive";

        var live = new StackPanel();
        live.Children.Add(otp);
        live.Children.Add(status);

        root.Children.Add(DemoSection(
            "Mã xác thực 4 số — gõ để sang ô, Backspace lùi, dán đầy đủ mã",
            live,
            xaml: "<controls:OtpInput Length=\"4\"\n" +
                  "    Completed=\"OnOtpCompleted\" />\n\n" +
                  "<!-- Value là TwoWay: gõ hay gán từ ViewModel đều cập nhật các ô -->\n" +
                  "<controls:OtpInput Value=\"{Binding VerificationCode}\" />",
            csharp: "otp.Completed += (_, _) => Verify(otp.Value);\n" +
                    "otp.Clear();   // xoá trắng và về ô đầu tiên"));

        // Section 2: six-digit pill for MFA.
        var pill = new OtpInput
        {
            Style = (Style)App.Current.Resources["CONCOST.OtpInput.Pill"],
            Length = 6,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        root.Children.Add(DemoSection(
            "Mã MFA 6 số kiểu pill",
            pill,
            xaml: "<controls:OtpInput Style=\"{DynamicResource CONCOST.OtpInput.Pill}\"\n" +
                  "    Length=\"6\" />"));

        // Section 3: alphanumeric licence key with masking.
        var masked = new OtpInput
        {
            Length = 6,
            NumericOnly = false,
            MaskCharacter = '•',
            HorizontalAlignment = HorizontalAlignment.Left
        };
        masked.Value = "K7X2PQ";

        root.Children.Add(DemoSection(
            "Khoá kích hoạt: chữ + số, hiển thị dạng dấu chấm",
            masked,
            xaml: "<controls:OtpInput Length=\"6\"\n" +
                  "    NumericOnly=\"False\"\n" +
                  "    MaskCharacter=\"•\" />",
            csharp: "// Mặt nạ chỉ áp lên màn hình — Value vẫn giữ ký tự thật\n" +
                    "Assert.Equal(\"K7X2PQ\", otp.Value);"));

        return root;
    }

    private object CreateUnitBoxDemo()
    {
        var root = new StackPanel();

        // Section 1: bare field with a unit plate and a step.
        var beam = new UnitBox
        {
            Header = string.Empty,
            Unit = "mm",
            Increment = 25,
            Value = 3200,
            Width = 220,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        var beamStatus = new TextBlock { FontSize = 12, Margin = new Thickness(0, 8, 0, 0) };
        beamStatus.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        beamStatus.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding
        {
            Path = new PropertyPath(nameof(UnitBox.Value)),
            ElementName = "BeamBox",
            StringFormat = "Bề rộng dầm nhận vào: {0} mm"
        });
        beam.Name = "BeamBox";

        var bare = new StackPanel();
        bare.Children.Add(beam);
        bare.Children.Add(beamStatus);

        root.Children.Add(DemoSection(
            "Trường số kèm đơn vị — mũi tên lên/xuống bước theo Increment",
            bare,
            xaml: "<controls:UnitBox Unit=\"mm\" Increment=\"25\"\n" +
                  "    Value=\"{Binding BeamWidth}\" />",
            csharp: "box.ValueChanged += (_, e) => Model.BeamWidth = e.NewValue;\n" +
                    "// Value luôn tính theo đơn vị của Unit — chuyển đổi đơn vị là việc của model"));

        // Section 2: a labelled form row.
        var form = new StackPanel { Margin = new Thickness(0, 0, 0, 0) };
        form.Children.Add(new UnitBox { Header = "Chiều dài nhịp", Unit = "mm", Value = 6000, Increment = 100, Width = 240, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 0, 0, 10) });
        form.Children.Add(new UnitBox { Header = "Cường độ bê tông", Unit = "MPa", DecimalPlaces = 0, Value = 30, Width = 240, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 0, 0, 10) });
        form.Children.Add(new UnitBox { Header = "Tải phân bố", Unit = "kN/m", DecimalPlaces = 2, Value = 12.5, Increment = 0.5, Width = 240, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 0, 0, 10) });
        form.Children.Add(new UnitBox { Header = "Chế độ xem", Unit = "°", DecimalPlaces = 0, Value = 0, Minimum = -90, Maximum = 90, Width = 240, HorizontalAlignment = HorizontalAlignment.Left });

        root.Children.Add(DemoSection(
            "Có Header — nhãn phía trên, ô số giữ chiều cao chuẩn",
            form,
            xaml: "<controls:UnitBox Header=\"Chiều dài nhịp\"\n" +
                  "    Unit=\"mm\" Increment=\"100\" Value=\"6000\" />\n\n" +
                  "<!-- Header rỗng: hàng nhãn tự thu lại, chỉ còn ô số -->"));

        // Section 3: bounded and read-only.
        var bounded = new UnitBox
        {
            Header = "Hệ số an toàn (1.0 – 3.0)",
            Unit = "×",
            DecimalPlaces = 2,
            Value = 1.5,
            Minimum = 1.0,
            Maximum = 3.0,
            Increment = 0.1,
            Width = 240,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        root.Children.Add(DemoSection(
            "Chặn khoảng giá trị và khoá chỉnh sửa",
            bounded,
            xaml: "<controls:UnitBox Header=\"Hệ số an toàn\"\n" +
                  "    Unit=\"×\" Minimum=\"1.0\" Maximum=\"3.0\"\n" +
                  "    Increment=\"0.1\" Value=\"1.5\" />\n\n" +
                  "<!-- IsReadOnly chặn gõ nhưng vẫn copy được -->\n" +
                  "<controls:UnitBox IsReadOnly=\"True\" />"));

        return root;
    }

    private object CreateCompassIndicatorDemo()
    {
        var root = new StackPanel();

        // Section 1: live angle dial.
        var compass = new CompassIndicator
        {
            Width = 96,
            Height = 96,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 0, 12)
        };
        compass.Name = "CompassLive";

        var slider = new Slider
        {
            Minimum = 0,
            Maximum = 360,
            Width = 260,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        slider.SetBinding(System.Windows.Controls.Primitives.RangeBase.ValueProperty,
            new System.Windows.Data.Binding(nameof(CompassIndicator.Angle)) { ElementName = "CompassLive", Mode = System.Windows.Data.BindingMode.TwoWay });

        var readout = new TextBlock { FontSize = 12, Margin = new Thickness(0, 8, 0, 0) };
        readout.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        readout.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding
        {
            Path = new PropertyPath(nameof(CompassIndicator.Angle)),
            ElementName = "CompassLive",
            StringFormat = "Góc quay bản vẽ: {0:0}°"
        });

        var live = new StackPanel();
        live.Children.Add(compass);
        live.Children.Add(slider);
        live.Children.Add(readout);

        root.Children.Add(DemoSection(
            "Kim bám góc bản vẽ — chữ N chỉ hướng bắc thật của mặt cắt",
            live,
            xaml: "<controls:CompassIndicator Angle=\"{Binding ViewRotation}\" />\n\n" +
                  "<!-- Angle %= 360, số âm quy về dương -->",
            csharp: "compass.Angle = viewport.ViewRotation;\n" +
                    "viewport.ViewRotationChanged += (_, a) => compass.Angle = a;"));

        // Section 2: quiet variants.
        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(new CompassIndicator { Width = 42, Height = 42, Margin = new Thickness(0, 0, 16, 0), Angle = 30 });
        row.Children.Add(new CompassIndicator { Width = 42, Height = 42, Margin = new Thickness(0, 0, 16, 0), Angle = 90, ShowCardinals = false });
        row.Children.Add(new CompassIndicator
        {
            Width = 42,
            Height = 42,
            Angle = 315,
            NorthBrush = new SolidColorBrush(Color.FromRgb(0x25, 0x96, 0xD0)),
            CardinalBrush = new SolidColorBrush(Color.FromRgb(0x8A, 0x8F, 0x98))
        });

        root.Children.Add(DemoSection(
            "Gọn cho thanh trạng thái: không chữ, đổi màu kim và chữ",
            row,
            xaml: "<controls:CompassIndicator Width=\"42\" Height=\"42\"\n" +
                  "    Angle=\"90\" ShowCardinals=\"False\"\n" +
                  "    NorthBrush=\"#2596D0\"\n" +
                  "    CardinalBrush=\"#8A8F98\" />"));

        return root;
    }

    private object CreateLoadingIndicatorDemo()
    {
        var root = new StackPanel();

        // Section 1: the four looks side by side.
        var types = new WrapPanel
        {
            Margin = new Thickness(0, 0, 0, 4)
        };

        foreach (LoadingIndicatorType type in Enum.GetValues(typeof(LoadingIndicatorType)))
        {
            var item = new StackPanel { Margin = new Thickness(0, 0, 28, 12) };
            item.Children.Add(new LoadingIndicator
            {
                IndicatorType = type,
                Width = 96,
                Height = 44,
                HorizontalAlignment = HorizontalAlignment.Left
            });

            var caption = new TextBlock
            {
                Text = type.ToString(),
                FontSize = 11.5,
                Margin = new Thickness(0, 6, 0, 0)
            };
            caption.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
            item.Children.Add(caption);

            types.Children.Add(item);
        }

        root.Children.Add(DemoSection(
            "Bốn kiểu indeterminate trong một control — đổi IndicatorType",
            types,
            xaml: "<controls:LoadingIndicator IndicatorType=\"ThreeDotsWave\" />\n" +
                  "<controls:LoadingIndicator IndicatorType=\"CircularDotsRing\" />\n" +
                  "<controls:LoadingIndicator IndicatorType=\"LinePulse\" />\n" +
                  "<controls:LoadingIndicator IndicatorType=\"PulseCircle\" />"));

        // Section 2: sizes and colours.
        var sized = new WrapPanel { VerticalAlignment = VerticalAlignment.Center };
        sized.Children.Add(new LoadingIndicator { IndicatorType = LoadingIndicatorType.CircularDotsRing, Width = 30, Height = 30, Margin = new Thickness(0, 0, 24, 0), VerticalAlignment = VerticalAlignment.Center });
        sized.Children.Add(new LoadingIndicator { IndicatorType = LoadingIndicatorType.CircularDotsRing, Width = 48, Height = 48, Margin = new Thickness(0, 0, 24, 0), VerticalAlignment = VerticalAlignment.Center, DotSize = 12 });

        var teal = new LoadingIndicator
        {
            IndicatorType = LoadingIndicatorType.ThreeDotsWave,
            Width = 60,
            Height = 26,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = new SolidColorBrush(Color.FromRgb(0x0E, 0x9F, 0x6E))
        };
        sized.Children.Add(teal);

        root.Children.Add(DemoSection(
            "Cỡ và màu — DotSize giãn hạt, Foreground nhận màu nhấn của host",
            sized,
            xaml: "<controls:LoadingIndicator IndicatorType=\"CircularDotsRing\"\n" +
                  "    Width=\"48\" Height=\"48\" DotSize=\"12\" />\n\n" +
                  "<controls:LoadingIndicator IndicatorType=\"ThreeDotsWave\"\n" +
                  "    Foreground=\"#0E9F6E\" />",
            csharp: "// Đựng trong AnimatedEffectBase: Windows tắt \"Show animations\"\n" +
                    "// hoặc MotionSettings.ForceDisabled = true → glyph đứng ở khung đầu tiên."));

        return root;
    }
}
