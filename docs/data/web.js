// Controls ported from modern web UI patterns (spotlight card, number ticker,
// sparkline, avatar stack).
const DOCS_WEB = {
    spotlightcard: {
        title: "SpotlightCard — thẻ có vệt sáng theo chuột",
        badge: "Hiệu ứng & Trang trí",
        desc: "Bề mặt thẻ có <b>vệt sáng bám theo con trỏ</b> và viền gradient hiện dần khi hover — pattern “spotlight card” quen thuộc trên các landing page hiện đại, port sang WPF bằng <code>RadialGradientBrush</code> + <code>Canvas</code> nên không tốn GPU layer.",
        keywords: ["spotlight", "glow", "hover", "gradient border", "card"],
        variants: [
            { key: "CONCOST.SpotlightCard.Base", desc: "Nền surface, viền 1px, bo góc 14px, spotlight 260px." },
            { key: "CONCOST.SpotlightCard.Glass", desc: "Nền surface phụ, không viền, bo 18px, spotlight đậm hơn (0.28)." }
        ],
        sections: [
            {
                title: "Thẻ KPI có vệt sáng",
                desc: "Đặt nội dung bên trong như một <code>ContentControl</code>. Vệt sáng tắt hoàn toàn khi chuột rời thẻ; <code>IsSpotlightEnabled=\"False\"</code> để tắt hẳn hiệu ứng.",
                xaml: `<ui:SpotlightCard Width="280" Style="{DynamicResource CONCOST.SpotlightCard.Base}">
    <StackPanel>
        <TextBlock Text="Tổng khối lượng bê tông"
                   Style="{DynamicResource CONCOST.TextBlock.Caption}" />
        <ui:OdometerNumber Value="1284.6" Format="N1" Suffix=" m³"
                           Style="{DynamicResource CONCOST.OdometerNumber.Hero}"
                           Margin="0,6,0,0" />
        <ui:Sparkline Values="{Binding ConcreteTrend}" Height="36" Margin="0,10,0,0" />
    </StackPanel>
</ui:SpotlightCard>`,
                csharp: `// Tinh chỉnh hiệu ứng theo ngữ cảnh
var card = new SpotlightCard
{
    SpotlightSize = 320,
    SpotlightOpacity = 0.24,
    CornerRadius = new CornerRadius(16),
    SpotlightBrush = (Brush)FindResource("CONCOST.Brush.Primary")
};

// Tắt hiệu ứng trên danh sách dài để giữ 60 FPS
foreach (var item in ItemsPanel.Children.OfType<SpotlightCard>())
    item.IsSpotlightEnabled = false;`
            },
            {
                title: "Viền gradient khi hover",
                desc: "<code>BorderGradient</code> nhận mọi <code>Brush</code>; dùng <code>LinearGradientBrush</code> để có viền chuyển màu. <code>ShowHoverBorder=\"False\"</code> giữ spotlight nhưng bỏ viền.",
                xaml: `<ui:SpotlightCard Style="{DynamicResource CONCOST.SpotlightCard.Glass}" Padding="24">
    <ui:SpotlightCard.BorderGradient>
        <LinearGradientBrush StartPoint="0,0" EndPoint="1,1">
            <GradientStop Color="#FF7900" Offset="0" />
            <GradientStop Color="#FFB066" Offset="0.45" />
            <GradientStop Color="#0284C7" Offset="1" />
        </LinearGradientBrush>
    </ui:SpotlightCard.BorderGradient>

    <TextBlock Text="Hover để thấy viền gradient"
               Style="{DynamicResource CONCOST.TextBlock.Body}" />
</ui:SpotlightCard>`,
                csharp: `// Đổi màu spotlight theo trạng thái nghiệp vụ
card.SpotlightBrush = isValid ? Brushes.SeaGreen : Brushes.IndianRed;

// Chỉ giữ vệt sáng, bỏ viền gradient
card.ShowHoverBorder = false;`
            }
        ],
        related: ["card", "ripple", "odometernumber"]
    },

    odometernumber: {
        title: "OdometerNumber — số tự chạy",
        badge: "Dữ liệu & Dashboard",
        desc: "Số liệu tự “cuộn” từ giá trị cũ sang giá trị mới với easing, dùng cho KPI, dashboard và bảng tổng hợp. Không cần viết <code>Storyboard</code> cho từng lần cập nhật — chỉ cần bind <code>Value</code>.",
        keywords: ["counter", "number ticker", "kpi", "animation", "odometer"],
        variants: [
            { key: "CONCOST.OdometerNumber.Base", desc: "28px, Bold — mặc định cho thẻ số liệu." },
            { key: "CONCOST.OdometerNumber.Hero", desc: "42px, ExtraBold — số chính của dashboard." },
            { key: "CONCOST.OdometerNumber.Compact", desc: "15px, SemiBold — dùng trong bảng hoặc toolbar." }
        ],
        sections: [
            {
                title: "Bind giá trị, để control tự chạy",
                desc: "<code>Format</code> là chuỗi định dạng .NET (<code>N0</code>, <code>N2</code>, <code>0.###</code>). <code>Prefix</code>/<code>Suffix</code> ghép đơn vị. <code>Duration</code> đặt <code>0</code> để tắt animation.",
                xaml: `<StackPanel Orientation="Horizontal" VerticalAlignment="Center">
    <ui:OdometerNumber Value="{Binding ConcreteM3}" Format="N1" Suffix=" m³"
                       Style="{DynamicResource CONCOST.OdometerNumber.Hero}" />

    <ui:OdometerNumber Value="{Binding SteelKg}" Format="N0" Suffix=" kg"
                       Prefix="+" Duration="0:0:1.2" Margin="24,0,0,0" />
</StackPanel>`,
                csharp: `// Cập nhật khi dữ liệu về — control tự chạy từ giá trị cũ
ConcreteM3 = summary.ConcreteM3;

// Muốn hiện ngay, không chạy số
SteelKgNumber.Duration = new Duration(TimeSpan.Zero);

// Lần load đầu có thể bỏ animation
SteelKgNumber.AnimateOnLoad = false;`
            }
        ],
        related: ["statisticcard", "sparkline", "spotlightcard"]
    },

    sparkline: {
        title: "Sparkline — biểu đồ mini trong dòng",
        badge: "Dữ liệu & Dashboard",
        desc: "Đường xu hướng cực gọn để nhét vào ô bảng, thẻ KPI hay thanh trạng thái: tự co giãn theo khung, có miền tô gradient và chấm đánh dấu điểm cuối.",
        keywords: ["sparkline", "mini chart", "trend", "inline chart"],
        sections: [
            {
                title: "Đường xu hướng trong bảng khối lượng",
                desc: "Chỉ cần truyền dãy số. <code>Smooth</code> vẽ đường cong Bezier, <code>ShowArea</code> tô miền dưới, <code>ShowLastDot</code> đánh dấu giá trị mới nhất.",
                xaml: `<DataGridTemplateColumn Header="7 ngày" Width="120">
    <DataGridTemplateColumn.CellTemplate>
        <DataTemplate>
            <ui:Sparkline Values="{Binding Last7Days}"
                          Height="28" Margin="4,2"
                          Smooth="True" ShowArea="True" ShowLastDot="True"
                          Stroke="{DynamicResource CONCOST.Brush.Primary}" />
        </DataTemplate>
    </DataGridTemplateColumn.CellTemplate>
</DataGridTemplateColumn>`,
                csharp: `// Nạp dãy số từ dữ liệu quan trắc
var values = measurements.Select(m => m.SettlementMm).ToList();

var spark = new Sparkline
{
    Values = values,
    Smooth = true,
    ShowArea = true,
    StrokeThickness = 1.8,
    Height = 32,
    Width = 120
};

// Đổi màu theo xu hướng
spark.Stroke = values.Last() >= values.First() ? Brushes.SeaGreen : Brushes.IndianRed;`,
                callout: {
                    type: "tip",
                    body: "Với bảng rất dài, đặt <code>ShowArea=\"False\"</code> và <code>ShowLastDot=\"False\"</code> để giảm số hình phải vẽ mỗi lần scroll."
                }
            }
        ],
        related: ["charts", "odometernumber", "datagrid"]
    },

    avatarstack: {
        title: "AvatarStack — chồng avatar",
        badge: "Dữ liệu & Dashboard",
        desc: "Hiển thị nhóm người phụ trách dưới dạng avatar chồng lên nhau kèm chip <b>+N</b> — pattern quen thuộc của bảng quản lý công việc. Nhận chuỗi (tự lấy chữ cái đầu) hoặc <code>ImageSource</code>.",
        keywords: ["avatar", "stack", "team", "assignee", "group"],
        sections: [
            {
                title: "Nhóm kỹ sư phụ trách",
                desc: "Truyền <code>Items</code> là danh sách chuỗi hoặc ảnh. <code>MaxVisible</code> giới hạn số avatar hiện; phần còn lại gộp vào chip <code>+N</code> (<code>OverflowTextFormat</code> đổi định dạng).",
                xaml: `<StackPanel Orientation="Horizontal">
    <ui:AvatarStack Items="{Binding Reviewers}"
                    MaxVisible="4" Size="34" Overlap="12" />

    <TextBlock Text="{Binding ReviewerSummary}"
               Style="{DynamicResource CONCOST.TextBlock.Caption}"
               VerticalAlignment="Center" Margin="12,0,0,0" />
</StackPanel>`,
                csharp: `// Chuỗi → tự lấy chữ cái đầu ("Nguyen Van A" → "NA")
Reviewers = new[] { "Nguyen Van A", "Tran Thi B", "Le C", "Pham D", "Hoang E" };

// Ảnh cũng dùng được
stack.Items = new object[]
{
    new BitmapImage(new Uri("pack://application:,,,/Avatars/lead.png")),
    "Tran Thi B"
};

// Tinh chỉnh
stack.Size = 40;
stack.Overlap = 14;
stack.OverflowTextFormat = "còn {0}";`
            }
        ],
        related: ["badge", "tag", "statisticcard"]
    },

    heatmapcalendar: {
        title: "HeatmapCalendar — mật độ theo ngày",
        badge: "Dữ liệu & Dashboard",
        desc: "Lưới ô vuông kiểu GitHub contribution: mỗi ô là một ngày, màu đậm nhạt theo số lượng. Dùng cho nhật ký đổ bê tông, số lượt nghiệm thu, mật độ quan trắc — nhìn một cái là thấy chỗ nào dày, chỗ nào trống.",
        keywords: ["heatmap", "calendar", "contribution", "activity", "mật độ"],
        sections: [
            {
                title: "Mỗi phần tử là một ngày",
                desc: "<code>Values</code> là dãy số nguyên, phần tử đầu ứng với <code>StartDate</code>. Màu chia 4 mức theo tỉ lệ so với giá trị lớn nhất; ô 0 dùng <code>BaseBrush</code>. Cột là tuần, hàng là thứ (bắt đầu thứ Hai).",
                xaml: `<ui:HeatmapCalendar Values="{Binding PourCounts}"
                    StartDate="{Binding StartDate}"
                    CellSize="13" CellGap="3"
                    ShowMonthLabels="True" />`,
                csharp: `// Gom nhật ký đổ bê tông theo ngày
var counts = pours
    .GroupBy(p => p.Date.Date)
    .OrderBy(g => g.Key)
    .Select(g => g.Count())
    .ToList();

var heatmap = new HeatmapCalendar
{
    Values = counts,
    StartDate = DateTime.Today.AddDays(-counts.Count + 1),
    CellSize = 13,
    CellGap = 3
};

// Đổi thang màu theo bộ màu thương hiệu
heatmap.Level1Brush = new SolidColorBrush(Color.FromArgb(90, 0xFF, 0x79, 0x00));
heatmap.Level4Brush = new SolidColorBrush(Color.FromRgb(0xFF, 0x79, 0x00));`
            }
        ],
        related: ["sparkline", "statisticcard", "timeline"]
    },

    marqueeticker: {
        title: "MarqueeTicker — dòng thông báo chạy ngang",
        badge: "Hiệu ứng & Trang trí",
        desc: "Dải chữ chạy liên tục cho thanh trạng thái, bảng tin công trường hoặc ticker chứng khoán. Render trực tiếp và chạy bằng <code>CompositionTarget.Rendering</code> nên mượt, không tạo Storyboard cho từng item.",
        keywords: ["marquee", "ticker", "scroll text", "chạy chữ", "status"],
        sections: [
            {
                title: "Ticker cho thanh trạng thái",
                desc: "<code>Speed</code> tính bằng pixel/giây. <code>Separator</code> chèn giữa các lần lặp. Đặt <code>IsRunning=&quot;False&quot;</code> để tạm dừng mà không mất vị trí.",
                xaml: `<Border CornerRadius="8" Padding="12,0" BorderThickness="1"
        Background="{DynamicResource CONCOST.Brush.SurfaceSecondary}"
        BorderBrush="{DynamicResource CONCOST.Brush.Border}">
    <ui:MarqueeTicker Text="{Binding SiteNotice}" Speed="55" Height="30" />
</Border>`,
                csharp: `var ticker = new MarqueeTicker
{
    Text = "Công trường A — đổ bê tông sàn tầng 12 · 3 mũi thi công đang hoạt động",
    Speed = 55,        // px / giây
    Separator = "  •  ",
    FontSize = 13
};

// Tạm dừng / chạy tiếp
ticker.IsRunning = false;`,
                callout: {
                    type: "tip",
                    body: "Nếu ticker nằm trong danh sách cuộn dài, đặt <code>IsRunning=&quot;False&quot;</code> khi item không hiển thị để tránh vẽ thừa mỗi frame."
                }
            }
        ],
        related: ["growl", "spotlightcard", "timeline"]
    },

    aurorabackground: {
        title: "AuroraBackground — nền gradient động",
        badge: "Hiệu ứng & Trang trí",
        desc: "Nền aurora gồm vài quầng sáng lớn trôi chậm, vẽ trực tiếp bằng <code>RadialGradientBrush</code> nên <b>không dùng BlurEffect</b> — vẫn mượt trên máy yếu. Dùng cho splash screen, hero panel, cửa sổ đăng nhập.",
        keywords: ["aurora", "gradient", "background", "hero", "splash"],
        sections: [
            {
                title: "Đặt sau nội dung trong một Grid",
                desc: "Đặt <code>AuroraBackground</code> làm lớp dưới cùng, nội dung đè lên trên. Control tự <code>ClipToBounds</code> nên không tràn ra ngoài khung.",
                xaml: `<Grid Height="170" ClipToBounds="True" Background="#0B0F17">
    <ui:AuroraBackground BlobCount="5" BlobOpacity="0.45" />
    <TextBlock Text="CONCOST STUDIO" Foreground="White"
               HorizontalAlignment="Center" VerticalAlignment="Center" />
</Grid>`,
                csharp: `var aurora = new AuroraBackground
{
    BlobCount = 5,
    BlobOpacity = 0.45,
    Speed = 1.0,               // hệ số tốc độ trôi
    AccentBrush = Brushes.OrangeRed
};

// Tạm dừng khi cửa sổ bị ẩn để không vẽ thừa
aurora.IsAnimated = false;`,
                callout: {
                    type: "tip",
                    body: "Aurora hợp nhất trên nền tối. Nếu dùng trên nền sáng, giảm <code>BlobOpacity</code> xuống 0.25–0.3 để chữ vẫn đạt tương phản AA."
                }
            }
        ],
        related: ["spotlightcard", "card", "marqueeticker"]
    },

    borderbeam: {
        title: "BorderBeam — vệt sáng chạy quanh viền",
        badge: "Hiệu ứng & Trang trí",
        desc: "Một đốm sáng chạy dọc chu vi khung bo góc, kiểu “border beam” trên các landing page hiện đại. Bọc quanh nội dung bất kỳ, dùng cho item mới, trạng thái đang xử lý hoặc thẻ cần chú ý.",
        keywords: ["border beam", "glow", "viền sáng", "comet", "shimmer border"],
        sections: [
            {
                title: "Bọc quanh một thẻ",
                desc: "<code>BorderBeam</code> là <code>Decorator</code>: đặt nội dung vào <code>Child</code>. <code>BeamLength</code> là tỉ lệ chu vi được chiếu sáng (0–1), <code>RevolutionSeconds</code> là thời gian một vòng.",
                xaml: `<ui:BorderBeam CornerRadius="12" Padding="18"
               BeamThickness="1.8" BeamLength="0.30" RevolutionSeconds="3.5"
               SurfaceBrush="{DynamicResource CONCOST.Brush.Surface}">
    <StackPanel>
        <TextBlock Text="Nghiệm thu đợt 3" FontWeight="SemiBold" />
        <TextBlock Text="Đang xử lý..." Foreground="Gray" />
    </StackPanel>
</ui:BorderBeam>`,
                csharp: `var beam = new BorderBeam
{
    CornerRadius = new CornerRadius(12),
    BeamThickness = 1.8,
    BeamLength = 0.30,          // 30% chu vi
    RevolutionSeconds = 3.5,    // một vòng mỗi 3.5 giây
    BeamBrush = Brushes.Orange
};

beam.Child = new TextBlock { Text = "Đang xử lý..." };`,
                callout: {
                    type: "warn",
                    body: "Beam vẽ 3 lớp (glow + mid + core) mỗi frame. Trên danh sách dài, chỉ bật cho item đang được chọn thay vì mọi item."
                }
            }
        ],
        related: ["spotlightcard", "card", "badge"]
    },

    confettiburst: {
        title: "ConfettiBurst — hiệu ứng ăn mừng",
        badge: "Hiệu ứng & Trang trí",
        desc: "Bắn một đợt pháo giấy ngắn khi thao tác thành công (lưu mô hình, duyệt hồ sơ). Tự mô phỏng vật lý và <b>tự tắt</b> khi hạt tan hết, không cần code hẹn giờ.",
        keywords: ["confetti", "celebration", "particle", "ăn mừng", "success"],
        sections: [
            {
                title: "Gắn vào một Grid phủ lên nội dung",
                desc: "Đặt <code>ConfettiBurst</code> trong cùng <code>Grid</code> với nội dung, đặt <code>IsBursting = true</code> để bắn. <code>Origin</code> mặc định ở giữa-dưới; để trống thì bắn từ tâm.",
                xaml: `<Grid Height="170" ClipToBounds="True">
    <Button Content="Duyệt hồ sơ" Click="OnApproveClick" />
    <ui:ConfettiBurst x:Name="Celebration" ParticleCount="110" />
</Grid>`,
                csharp: `private void OnApproveClick(object sender, RoutedEventArgs e)
{
    Approve();
    Celebration.IsBursting = true;   // tự tắt khi hạt tan hết
}

// Tuỳ chỉnh
Celebration.Gravity = 1100;                                   // px/s²
Celebration.Duration = new Duration(TimeSpan.FromSeconds(2));
Celebration.Palette = new[] { Colors.Orange, Colors.SeaGreen };`,
                callout: {
                    type: "tip",
                    body: "Giữ <code>ParticleCount</code> ≤ 150 và <code>ClipToBounds=&quot;True&quot;</code> trên Grid chứa nó — hạt vẽ bằng <code>DrawRectangle</code> nên rất nhẹ, nhưng vẫn nên giới hạn."
                }
            }
        ],
        related: ["growl", "dialog", "spotlightcard"]
    },

    sheensweep: {
        title: "SheenSweep — vệt sáng quét bề mặt",
        badge: "Hiệu ứng & Trang trí",
        desc: "Một dải sáng chéo chạy qua bề mặt theo chu kỳ, có khoảng nghỉ giữa hai lần quét — hiệu ứng “sheen” quen thuộc trên nút CTA và thẻ sản phẩm. Đặt trong cùng <code>Grid</code> với nội dung, phía trên cùng.",
        keywords: ["sheen", "shine", "gloss", "quét sáng", "cta"],
        sections: [
            {
                title: "Quét sáng qua nút",
                desc: "<code>Speed</code> là số lần quét mỗi giây, <code>PauseSeconds</code> là thời gian nghỉ giữa hai lần. <code>BandWidth</code> tính theo tỉ lệ chiều rộng, <code>Angle</code> là độ nghiêng của dải sáng.",
                xaml: `<Grid Width="320" Height="52">
    <Border CornerRadius="10" Background="#101828" />
    <TextBlock Text="Xuất báo cáo" Foreground="White"
               HorizontalAlignment="Center" VerticalAlignment="Center" />
    <ui:SheenSweep Speed="0.35" PauseSeconds="0.8" PeakOpacity="0.45" />
</Grid>`,
                csharp: `var sheen = new SheenSweep
{
    Speed = 0.35,          // số lần quét mỗi giây
    PauseSeconds = 0.8,    // nghỉ giữa hai lần quét
    BandWidth = 0.22,      // bề rộng dải sáng theo % chiều rộng
    Angle = 24,
    PeakOpacity = 0.45
};

// Tạm dừng khi cần
sheen.IsActive = false;`,
                callout: {
                    type: "warn",
                    body: "Chỉ nên dùng cho 1–2 CTA chính. Nếu gắn cho mọi item trong danh sách, mắt sẽ bị “nhiễu” và tốn frame vô ích."
                }
            }
        ],
        related: ["button", "spotlightcard", "borderbeam"]
    },

    pulsering: {
        title: "PulseRing — vòng lan toả báo hiệu live",
        badge: "Hiệu ứng & Trang trí",
        desc: "Các vòng tròn lan ra rồi mờ dần quanh một điểm, dùng cho trạng thái đang chạy, đang đồng bộ hoặc đánh dấu vị trí trên bản vẽ.",
        keywords: ["pulse", "ring", "live", "radar", "lan toả"],
        sections: [
            {
                title: "Chấm live cạnh nhãn trạng thái",
                desc: "<code>RingCount</code> là số vòng cùng lúc, <code>Duration</code> là thời gian một vòng đi từ tâm ra biên, <code>MaxRadius</code> giới hạn bán kính. Đặt <code>CenterDot=\"False\"</code> nếu chỉ muốn các vòng.",
                xaml: `<ui:PulseRing MaxRadius="46" Width="110" Height="110"
              RingCount="3" Duration="2.2" DotRadius="6" />`,
                csharp: `var pulse = new PulseRing
{
    MaxRadius = 46,
    RingCount = 3,
    Duration = 2.2,        // giây cho một vòng lan
    RingThickness = 2,
    RingBrush = Brushes.SeaGreen
};

// Tắt khi mất kết nối
pulse.IsActive = false;`,
                callout: {
                    type: "tip",
                    body: "Mỗi vòng là một <code>DrawEllipse</code>, rất nhẹ — nhưng trên lưới dày (hàng trăm điểm) hãy giảm <code>RingCount</code> xuống 1–2."
                }
            }
        ],
        related: ["badge", "loadings", "heatmapcalendar"]
    },

    wavedivider: {
        title: "WaveDivider — dải sóng phân cách",
        badge: "Hiệu ứng & Trang trí",
        desc: "Hai lớp sóng sin trôi với tốc độ khác nhau, dùng làm đường phân cách section hoặc chân của khối header. Vẽ bằng <code>StreamGeometry</code> nên không cần ảnh.",
        keywords: ["wave", "divider", "sóng", "section", "header"],
        sections: [
            {
                title: "Sóng ở chân khối header",
                desc: "<code>Amplitude</code> là biên độ (px), <code>Wavelength</code> là độ dài bước sóng, <code>Speed</code> là tốc độ trôi (px/s). Đặt <code>ShowBackWave=\"False\"</code> nếu chỉ cần một lớp.",
                xaml: `<Grid Height="120" ClipToBounds="True">
    <Border Background="{DynamicResource CONCOST.Brush.SurfaceSecondary}" />
    <ui:WaveDivider Height="70" VerticalAlignment="Bottom"
                    Amplitude="10" Speed="18"
                    FillBrush="{DynamicResource CONCOST.Brush.Primary}" />
</Grid>`,
                csharp: `var wave = new WaveDivider
{
    Height = 70,
    Amplitude = 10,
    Wavelength = 180,
    Speed = 18,            // px / giây
    ShowBackWave = true
};

// Tĩnh, không chạy
wave.IsAnimated = false;`
            }
        ],
        related: ["divider", "aurorabackground", "card"]
    },

    gradienttext: {
        title: "GradientText — chữ gradient chuyển động",
        badge: "Hiệu ứng & Trang trí",
        desc: "Chữ được tô bằng gradient chạy liên tục — dùng cho hero headline, badge phiên bản hoặc số liệu nhấn mạnh. Vẽ trực tiếp bằng <code>FormattedText</code> với brush biến đổi theo thời gian.",
        keywords: ["gradient text", "headline", "chữ gradient", "hero"],
        sections: [
            {
                title: "Headline gradient",
                desc: "<code>StartColor</code>/<code>EndColor</code> là hai đầu gradient, <code>Speed</code> là số chu kỳ mỗi giây, <code>Angle</code> là hướng gradient. Đặt <code>IsAnimated=\"False\"</code> để chữ đứng yên.",
                xaml: `<ui:GradientText Text="CONCOST UI" FontSize="34"
                 StartColor="#FF7900" EndColor="#0284C7" Speed="0.35" />`,
                csharp: `var headline = new GradientText
{
    Text = "CONCOST UI",
    FontSize = 34,
    StartColor = Color.FromRgb(0xFF, 0x79, 0x00),
    EndColor = Color.FromRgb(0x02, 0x84, 0xC7),
    Speed = 0.35
};

// Chữ tĩnh
headline.IsAnimated = false;`,
                callout: {
                    type: "tip",
                    body: "Chọn hai màu gần nhau về sắc độ (cam → đỏ, xanh → tím). Hai màu đối lập (cam → xanh dương) sẽ cho đoạn giữa bị xám, làm chữ khó đọc."
                }
            }
        ],
        related: ["textblock", "aurorabackground", "spotlightcard"]
    },

    cursorglow: {
        title: "CursorGlow — quầng sáng bám con trỏ",
        badge: "Hiệu ứng & Trang trí",
        desc: "Quầng sáng mềm trượt theo con trỏ với độ trễ nhẹ, dùng cho hero tối hoặc panel chào mừng. Không chặn chuột (<code>IsHitTestVisible=false</code>) nên đặt trên cùng vẫn bấm được nút bên dưới.",
        keywords: ["cursor", "glow", "spotlight", "con trỏ", "hero"],
        sections: [
            {
                title: "Đặt phủ lên nền tối",
                desc: "<code>GlowSize</code> là đường kính quầng sáng, <code>FollowSpeed</code> điều khiển độ trễ (0 = bám ngay, 0.2 = mượt). Khi chuột ra ngoài, quầng sáng mờ dần chứ không biến mất đột ngột.",
                xaml: `<Grid Height="170" ClipToBounds="True" Background="#101828">
    <TextBlock Text="Di chuột trong khung này" Foreground="White"
               HorizontalAlignment="Center" VerticalAlignment="Center" />
    <ui:CursorGlow GlowSize="240" GlowOpacity="0.42" />
</Grid>`,
                csharp: `var glow = new CursorGlow
{
    GlowSize = 240,
    GlowOpacity = 0.42,
    FollowSpeed = 0.18
};

// Tắt khi cửa sổ không active
glow.IsAnimated = false;`,
                callout: {
                    type: "tip",
                    body: "Chỉ có tác dụng trên nền tối hoặc ảnh. Trên nền sáng, quầng sáng gần như không thấy — hãy dùng <code>SpotlightCard</code> thay thế."
                }
            }
        ],
        related: ["spotlightcard", "aurorabackground", "gridpattern"]
    },

    gridpattern: {
        title: "GridPattern — nền lưới chấm / kẻ / chữ thập",
        badge: "Hiệu ứng & Trang trí",
        desc: "Ba kiểu nền lưới vẽ bằng <code>DrawingBrush</code> tile: chấm, đường kẻ, hoặc dấu chữ thập — phù hợp nền bản vẽ, blueprint, dashboard kỹ thuật. Vì là tile brush nên chi phí chỉ là **một** lệnh fill, không phụ thuộc số ô hiển thị.",
        keywords: ["grid", "pattern", "dots", "blueprint", "lưới"],
        sections: [
            {
                title: "Ba kiểu lưới",
                desc: "<code>Pattern</code> nhận <code>Dots</code>, <code>Lines</code>, <code>Cross</code>. <code>CellSize</code> là bước lưới, <code>PatternOpacity</code> giữ lưới ở mức nền. <code>OffsetX/OffsetY</code> dịch pha để làm parallax.",
                xaml: `<Grid Height="120" Background="{DynamicResource CONCOST.Brush.Surface}">
    <ui:GridPattern Pattern="Dots" CellSize="18" PatternOpacity="0.55" />
</Grid>`,
                csharp: `var pattern = new GridPattern
{
    Pattern = GridPatternType.Cross,
    CellSize = 18,
    StrokeThickness = 1,
    PatternOpacity = 0.55
};

// Parallax theo vị trí cuộn
pattern.OffsetX = scrollOffset * 0.15;
pattern.OffsetY = scrollOffset * 0.08;`,
                callout: {
                    type: "tip",
                    body: "Màu lưới lấy từ <code>CONCOST.Brush.Border</code> nếu không set <code>Stroke</code> — nên lưới tự đổi theo theme sáng/tối."
                }
            }
        ],
        related: ["gridpattern", "aurorabackground", "card"]
    },

    shakeelement: {
        title: "ShakeElement — rung khi dữ liệu không hợp lệ",
        badge: "Hiệu ứng & Trang trí",
        desc: "Attached behaviour rung ngang một phần tử khi validate lỗi — phản hồi quen thuộc, 360ms, biên độ giảm dần. Dùng được trên mọi <code>FrameworkElement</code> và giữ nguyên <code>RenderTransform</code> sẵn có.",
        keywords: ["shake", "invalid", "validation", "rung", "error"],
        sections: [
            {
                title: "Kích hoạt từ code",
                desc: "Gọi <code>ShakeElement.Shake(element)</code>, hoặc set attached property <code>ShakeElement.IsShaking=&quot;True&quot;</code> trong XAML. Tự bỏ qua nếu người dùng đã tắt animation hệ thống.",
                xaml: `<Border x:Name="LengthField"
        attached:ShakeElement.Amplitude="8"
        attached:ShakeElement.Duration="0:0:0.36">
    <!-- ô nhập -->
</Border>`,
                csharp: `if (!IsValid(value))
{
    ShakeElement.Shake(LengthField);
    Growl.Toast("Chiều dài dầm phải > 0", GrowlType.Warning);
}`,
                callout: {
                    type: "warn",
                    body: "Đừng rung cả form. Rung đúng ô nhập để người dùng biết cần sửa chỗ nào — rung cả form gây mất phương hướng."
                }
            }
        ],
        related: ["textbox", "growl", "numericupdown"]
    },

    brutaltheme: {
        title: "ThemePreset.Brutal — neo-brutalism",
        badge: "Bắt đầu",
        desc: "Preset thứ bảy của <code>ThemeManager</code>: nền kem <code>#FFFDF5</code>, viền mực <code>#111111</code>, accent vàng điện <code>#FFE14D</code> — tinh thần neo-brutalism nhưng vẫn đọc được trong app nghiệp vụ.",
        keywords: ["theme", "brutal", "neo-brutalism", "preset"],
        sections: [
            {
                title: "Áp và khôi phục",
                desc: "Preset ghi đè 7 brush nền/viền/chữ. Lần đầu áp dụng sẽ <b>snapshot</b> giá trị cũ, nên chuyển về <code>Default</code> là khôi phục nguyên trạng — không cần khởi động lại.",
                xaml: `<!-- Không cần XAML: preset ghi vào Application.Resources -->`,
                csharp: `ThemeManager.Current.Preset = ThemePreset.Brutal;

// Quay lại mặc định — brush được khôi phục từ snapshot
ThemeManager.Current.Preset = ThemePreset.Default;`,
                callout: {
                    type: "warn",
                    title: "Đây là preset màu, không phải bộ style",
                    body: "Nó không đổi độ dày viền hay bo góc. Muốn “brutal” đúng nghĩa (viền 2–3px, bo 0, shadow cứng) thì thêm một style variant riêng trong <code>Generic.xaml</code>."
                }
            }
        ],
        related: ["theming", "card", "button"]
    },

    reducedmotion: {
        title: "Reduced motion — tôn trọng cài đặt hệ thống",
        badge: "Bắt đầu",
        desc: "Mọi hiệu ứng của thư viện đọc <code>MotionSettings.AnimationsEnabled</code>, mặc định lấy từ <b>Windows “Show animations”</b> (<code>SystemParameters.ClientAreaAnimation</code>). Người dùng tắt animation ở Windows thì hiệu ứng vẽ khung tĩnh thay vì đứng hình — đúng WCAG 2.2.2.",
        keywords: ["reduced motion", "accessibility", "animation", "wcag"],
        sections: [
            {
                title: "Không cần cấu hình gì",
                desc: "Mặc định đã bật. Ứng dụng có thể tắt thêm bằng <code>MotionSettings.ForceDisabled</code> — hữu ích khi bạn có toggle “giảm chuyển động” trong phần cài đặt.",
                xaml: `<!-- Không cần khai báo: hiệu ứng tự kiểm tra -->`,
                csharp: `// Tắt toàn bộ hiệu ứng trong app
MotionSettings.ForceDisabled = true;

// Đọc trạng thái hiện tại để hiện trong Settings
bool motion = MotionSettings.AnimationsEnabled;

// Lắng nghe thay đổi (Windows đổi setting hoặc app toggle)
MotionSettings.Changed += (s, e) => UpdateMotionLabel();`,
                callout: {
                    type: "info",
                    body: "Khi animation bị tắt, <code>AnimatedEffectBase</code> gọi <code>OnMotionStopped()</code> để hiệu ứng chọn khung tĩnh hợp lý (ví dụ CursorGlow đặt quầng sáng ở giữa) thay vì dừng ở frame ngẫu nhiên."
                }
            }
        ],
        related: ["spotlightcard", "aurorabackground", "loadings"]
    },

    tiltcard: {
        title: "TiltCard — thẻ nghiêng theo con trỏ",
        badge: "Hiệu ứng & Trang trí",
        desc: "Thẻ nghiêng về phía con trỏ và nhấc nhẹ khi hover. Độ nghiêng làm bằng <b>skew + scale (2.5D)</b> chứ không dùng <code>Viewport3D</code>, nên chữ vẫn sắc nét và không tốn thêm lớp 3D.",
        keywords: ["tilt", "3d", "hover", "card", "nghiêng"],
        variants: [
            { key: "CONCOST.TiltCard.Base", desc: "Nền surface, viền 1px, nghiêng tối đa 6°." },
            { key: "CONCOST.TiltCard.Elevated", desc: "Không viền, có shadow, nghiêng 8° và nhấc 1.04." }
        ],
        sections: [
            {
                title: "Thẻ thông tin cấu kiện",
                desc: "<code>MaxTilt</code> là độ nghiêng tối đa ở mép, <code>Lift</code> là tỉ lệ phóng khi hover. Rời chuột thẻ tự về phẳng với easing.",
                xaml: `<ui:TiltCard Width="280" MaxTilt="6" Lift="1.02"
             Style="{DynamicResource CONCOST.TiltCard.Base}">
    <StackPanel>
        <TextBlock Text="Dầm B1 — 300×600" FontWeight="SemiBold" />
        <TextBlock Text="Di chuột để thấy độ nghiêng" Opacity="0.6" />
    </StackPanel>
</ui:TiltCard>`,
                csharp: `var card = new TiltCard
{
    MaxTilt = 6,
    Lift = 1.02
};

// Cần cả hiệu ứng sáng theo chuột? Bọc SpotlightCard bên trong.`,
                callout: {
                    type: "tip",
                    body: "Giữ <code>MaxTilt</code> ≤ 8°. Nghiêng quá tay làm chữ khó đọc và tạo cảm giác “đồ chơi” trong app nghiệp vụ."
                }
            }
        ],
        related: ["spotlightcard", "card", "magnetichover"]
    },

    magnetichover: {
        title: "MagneticHover — nút hút theo con trỏ",
        badge: "Hiệu ứng & Trang trí",
        desc: "Attached behaviour kéo phần tử về phía con trỏ vài pixel rồi trả về vị trí cũ khi rời chuột. Dùng <code>TranslateTransform</code> nên <b>không gây layout</b> mỗi frame.",
        keywords: ["magnetic", "hover", "button", "hút", "micro-interaction"],
        sections: [
            {
                title: "Bật cho một nút",
                desc: "<code>Strength</code> là tỉ lệ bám theo con trỏ (0.1–0.4), <code>MaxOffset</code> chặn trôi quá xa. Tự bỏ qua khi người dùng tắt animation hệ thống.",
                xaml: `<Button Content="Kéo tôi về phía con trỏ"
        attached:MagneticHover.IsEnabled="True"
        attached:MagneticHover.Strength="0.3"
        attached:MagneticHover.MaxOffset="12" />`,
                csharp: `MagneticHover.SetIsEnabled(button, true);
MagneticHover.SetStrength(button, 0.3);
MagneticHover.SetMaxOffset(button, 12);`,
                callout: {
                    type: "warn",
                    body: "Đừng bật cho mọi nút trong toolbar — mắt sẽ bị “nhấp nháy”. Chỉ dùng cho 1 CTA chính mỗi màn hình."
                }
            }
        ],
        related: ["button", "tiltcard", "sheensweep"]
    },

    checkmarkdraw: {
        title: "CheckMarkDraw — dấu tích tự vẽ",
        badge: "Hiệu ứng & Trang trí",
        desc: "Dấu ✓ vẽ dần từng nét trong 320ms khi <code>IsChecked</code> chuyển sang true, và xoá ngược khi false. Dùng cho luồng duyệt, lưu, hoàn tất.",
        keywords: ["check", "tick", "success", "confirm", "duyệt"],
        sections: [
            {
                title: "Gắn vào trạng thái duyệt",
                desc: "<code>ShowCircle</code> vẽ nền tròn mờ phía sau. <code>Duration</code> đặt <code>0</code> để hiện ngay (ví dụ khi load lại danh sách đã duyệt).",
                xaml: `<ui:CheckMarkDraw Width="52" Height="52"
                  IsChecked="{Binding IsApproved}"
                  StrokeThickness="3" />`,
                csharp: `ApprovalMark.IsChecked = true;

// Hiện ngay, không vẽ
ApprovalMark.Duration = new Duration(TimeSpan.Zero);`,
                callout: {
                    type: "tip",
                    body: "Màu mặc định lấy <code>CONCOST.Brush.Success</code> nếu có, không thì <code>CONCOST.Brush.Primary</code> — đặt <code>Stroke</code> nếu muốn màu riêng."
                }
            }
        ],
        related: ["growl", "dialog", "progressbar"]
    },

    scrollprogressbar: {
        title: "ScrollProgressBar — thanh tiến độ đọc",
        badge: "Hiệu ứng & Trang trí",
        desc: "Thanh mảnh hiển thị đã cuộn được bao nhiêu phần của một <code>ScrollViewer</code>. Không set <code>Target</code> thì tự tìm ScrollViewer cha gần nhất.",
        keywords: ["scroll", "progress", "reading", "thanh tiến độ"],
        sections: [
            {
                title: "Đặt trên cùng của trang",
                desc: "Gán <code>Target</code> tường minh khi thanh nằm ngoài ScrollViewer; đặt bên trong thì không cần. <code>ShowTrack=&quot;False&quot;</code> để chỉ còn phần đã đọc.",
                xaml: `<ui:ScrollProgressBar Target="{Binding ElementName=PageScroll}"
                      Thickness="3" />`,
                csharp: `bar.Target = PageScroll;
bar.Thickness = 3;
bar.ShowTrack = false;`,
                callout: {
                    type: "info",
                    body: "Khi nội dung ngắn hơn khung (không cuộn được), thanh tự hiển thị đầy 100% thay vì 0%."
                }
            }
        ],
        related: ["scrollviewer", "divider", "loadings"]
    },

    scrollreveal: {
        title: "ScrollReveal — hiện dần khi vào khung nhìn",
        badge: "Hiệu ứng & Trang trí",
        desc: "Attached behaviour: phần tử bắt đầu ở trạng thái mờ + lệch xuống, và khi cuộn tới ngưỡng thì fade + trượt lên. Tự tìm <code>ScrollViewer</code> cha, hiện **một lần** rồi ngắt listener.",
        keywords: ["scroll reveal", "fade in", "slide", "viewport", "cuộn"],
        sections: [
            {
                title: "Bật cho từng khối, delay tăng dần",
                desc: "<code>Offset</code> là quãng trượt (px), <code>Delay</code> tạo hiệu ứng stagger, <code>Threshold</code> là tỉ lệ chiều cao khung nhìn để kích hoạt (0.92 = gần đáy).",
                xaml: `<ScrollViewer Height="200">
    <StackPanel>
        <Border attached:ScrollReveal.IsEnabled="True"
                attached:ScrollReveal.Offset="24"
                attached:ScrollReveal.Delay="0:0:0.12">
            <!-- nội dung -->
        </Border>
    </StackPanel>
</ScrollViewer>`,
                csharp: `ScrollReveal.SetIsEnabled(card, true);
ScrollReveal.SetOffset(card, 24);
ScrollReveal.SetDelay(card, new Duration(TimeSpan.FromMilliseconds(120)));`,
                callout: {
                    type: "warn",
                    body: "Đừng bật cho hàng trăm item trong danh sách ảo hoá — mỗi lần cuộn nó phải dò cây visual. Chỉ dùng cho vài khối nội dung tĩnh."
                }
            }
        ],
        related: ["scrollprogressbar", "parallaxpanel", "card"]
    },

    parallaxpanel: {
        title: "ParallaxPanel — lớp nền trôi chậm hơn",
        badge: "Hiệu ứng & Trang trí",
        desc: "Lớp nội dung tự dịch theo vị trí cuộn với hệ số <code>Speed</code> — đặt phía sau nội dung chính để tạo cảm giác chiều sâu. Không chặn chuột.",
        keywords: ["parallax", "depth", "lớp nền", "chiều sâu"],
        sections: [
            {
                title: "Nền trôi chậm, nội dung cuộn bình thường",
                desc: "<code>Speed &lt; 1</code> để trôi chậm hơn, <code>&gt; 1</code> để trôi nhanh hơn. <code>MaxOffset</code> chặn trôi quá xa. Đặt <code>Axis=&quot;Horizontal&quot;</code> nếu cuộn ngang.",
                xaml: `<Grid Height="320">
    <ui:ParallaxPanel Speed="0.45" Height="120" VerticalAlignment="Top">
        <Border Background="{DynamicResource CONCOST.Brush.SurfaceSecondary}" />
    </ui:ParallaxPanel>
    <Border Height="90" Background="{DynamicResource CONCOST.Brush.Primary}" />
</Grid>`,
                csharp: `var layer = new ParallaxPanel
{
    Speed = 0.45,
    MaxOffset = 120
};

// Hoặc gán tường minh nếu panel nằm ngoài ScrollViewer
layer.Target = PageScroll;`
            }
        ],
        related: ["scrollreveal", "aurorabackground", "gridpattern"]
    },

    typewritertext: {
        title: "TypewriterText — gõ từng ký tự",
        badge: "Hiệu ứng & Trang trí",
        desc: "Chữ hiện dần như đang gõ, có caret nhấp nháy. Tốc độ tính theo ký tự/giây; tắt animation hệ thống thì hiện full text ngay.",
        keywords: ["typewriter", "typing", "caret", "gõ chữ"],
        sections: [
            {
                title: "Gõ một câu mô tả",
                desc: "<code>CharsPerSecond</code> là tốc độ, <code>StartDelay</code> là thời gian chờ trước khi gõ, <code>IsLooping</code> để gõ lại sau <code>RestartDelay</code> giây.",
                xaml: `<ui:TypewriterText Text="Bóc tách khối lượng tự động từ mô hình BIM."
                   CharsPerSecond="26" ShowCaret="True" />`,
                csharp: `var typer = new TypewriterText
{
    Text = "Bóc tách khối lượng...",
    CharsPerSecond = 26,
    StartDelay = 0.25,
    ShowCaret = true,
    IsLooping = false
};`,
                callout: {
                    type: "tip",
                    body: "Giữ tốc độ 20–30 ký tự/giây. Nhanh hơn thì người dùng không kịp đọc, chậm hơn thì sốt ruột."
                }
            }
        ],
        related: ["textreveal", "gradienttext", "textblock"]
    },

    textreveal: {
        title: "TextReveal — từng từ hiện dần",
        badge: "Hiệu ứng & Trang trí",
        desc: "Headline hiện theo từng từ: mỗi từ fade + trượt lên, lệch nhau <code>WordDelay</code> giây. Dùng cho tiêu đề lớn, không dùng cho đoạn văn dài.",
        keywords: ["text reveal", "stagger", "headline", "từng từ"],
        sections: [
            {
                title: "Tiêu đề hiện dần",
                desc: "<code>WordDelay</code> là giãn cách giữa các từ, <code>WordDuration</code> là thời gian mỗi từ, <code>OffsetY</code> là quãng trượt.",
                xaml: `<ui:TextReveal Text="Kết cấu chính xác, thi công an toàn"
                FontSize="26" WordDelay="0.08" />`,
                csharp: `var reveal = new TextReveal
{
    Text = "Kết cấu chính xác, thi công an toàn",
    WordDelay = 0.08,
    WordDuration = 0.45,
    OffsetY = 14
};`,
                callout: {
                    type: "warn",
                    body: "Mỗi từ là một lần <code>FormattedText</code>. Với đoạn văn dài (hơn ~15 từ) hãy dùng <code>TextBlock</code> thường để tránh tốn CPU mỗi frame. Muốn chạy lại từ đầu thì gọi <code>Restart()</code> — đổi <code>IsAnimated</code> không đủ vì thời gian đã tích luỹ."
                }
            }
        ],
        related: ["typewritertext", "textblock", "scrollreveal"]
    },

    ribbon: {
        title: "Ribbon — thanh lệnh nhiều tab",
        badge: "Layout & Shell",
        desc: "Ribbon kiểu Office / AutoCAD: dải tab ở trên, mỗi tab chứa các group, mỗi group xếp lệnh trên lưới 3 hàng. Khi hẹp, ribbon tự hạ kích thước lệnh rồi gộp group thành một nút — không bao giờ cắt mất lệnh.",
        keywords: ["ribbon", "tab", "group", "toolbar", "office", "autocad", "thanh lệnh"],
        sections: [
            {
                title: "Khai báo ribbon",
                desc: "Control nằm trong namespace <code>CONCOST.UI.Controls.Ribbon</code>. <code>ItemsSource</code> là danh sách <code>RibbonTabItem</code>; <code>Content</code> của mỗi tab là danh sách <code>RibbonGroup</code>.",
                xaml: `<Window xmlns:ribbon="clr-namespace:CONCOST.UI.Controls.Ribbon;assembly=CONCOST.UI">

    <ribbon:Ribbon>
        <ribbon:RibbonTabItem Header="Home">
            <ribbon:RibbonGroup Header="Clipboard">
                <ribbon:RibbonButton Header="Save" Size="Large" />
                <ribbon:RibbonSeparator />
                <ribbon:RibbonButton Header="Copy" Size="Small" />
                <ribbon:RibbonButton Header="Paste" Size="Small" />
            </ribbon:RibbonGroup>

            <ribbon:RibbonGroup Header="Paragraph">
                <ribbon:RibbonButton Header="Align" Size="Medium" />
                <ribbon:RibbonButton Header="Grid" Size="Small" />
            </ribbon:RibbonGroup>
        </ribbon:RibbonTabItem>

        <ribbon:RibbonTabItem Header="Insert">
            <ribbon:RibbonGroup Header="Tables">
                <ribbon:RibbonButton Header="Table" Size="Large" />
            </ribbon:RibbonGroup>
        </ribbon:RibbonTabItem>
    </ribbon:Ribbon>
</Window>`,
                csharp: `var ribbon = new Ribbon();

var clipboard = new RibbonGroup { Header = "Clipboard" };
clipboard.Items.Add(new RibbonButton { Header = "Save", Size = RibbonItemSize.Large });
clipboard.Items.Add(new RibbonSeparator());
clipboard.Items.Add(new RibbonButton { Header = "Copy", Size = RibbonItemSize.Small });

var home = new RibbonTabItem { Header = "Home" };
home.Content = new List<RibbonGroup> { clipboard };

ribbon.ItemsSource = new List<RibbonTabItem> { home };`,
                callout: {
                    type: "tip",
                    body: "Icon là <code>ImageSource</code> nên <b>không</b> tự đổi màu theo theme. Nếu app cho đổi theme lúc chạy, hãy dùng chung một <code>SolidColorBrush</code> <b>không freeze</b> cho mọi icon rồi đổi <code>Color</code> khi theme đổi — đúng cách Showcase đang làm, nếu không icon sẽ tối om trên nền tối."
                }
            },
            {
                title: "Ba kích thước lệnh",
                desc: "<code>Size</code> quyết định lệnh chiếm bao nhiêu hàng: <code>Large</code> = cả 3 hàng (icon 28 trên, nhãn dưới), <code>Medium</code> = 2 hàng, <code>Small</code> = 1 hàng (icon 16 bên trái). Một cột chứa được 1 Medium + 1 Small, hoặc 3 Small.",
                xaml: `<ribbon:RibbonGroup Header="Mixed">
    <ribbon:RibbonButton Header="Large" Size="Large" />
    <ribbon:RibbonButton Header="Medium" Size="Medium" />
    <ribbon:RibbonButton Header="Small" Size="Small" />
</ribbon:RibbonGroup>`,
                callout: {
                    type: "tip",
                    body: "Để cột không bị hở, hãy xếp theo nhịp <code>Large</code> · <code>Medium + Small</code> · <code>3 × Small</code>. Nếu hai <code>Medium</code> đứng cạnh nhau, nút thứ hai sẽ mở cột mới và để trống hàng cuối."
                }
            },
            {
                title: "Tự thu gọn khi hẹp",
                desc: "Ribbon đo tổng bề rộng của tab đang chọn rồi leo thang: hạ toàn bộ <code>Large</code> xuống Medium → hạ tiếp xuống Small → gộp từng group thành một nút. Group bị gộp vẫn giữ đủ lệnh trong flyout của nó. Hẹp đến mức dải tab cũng không vừa thì các tab bị cắt bớt và nút <b>»</b> hiện ra để liệt kê toàn bộ tab — không tab nào bị mất.",
                csharp: `// Group quan trọng: không bao giờ bị gộp
var review = new RibbonGroup { Header = "Review", CollapsePriority = 0 };

// Group phụ: bị gộp trước tiên (số càng nhỏ càng gộp sớm)
var utilities = new RibbonGroup { Header = "Utilities", CollapsePriority = 10 };

// Thu gọn cả ribbon xuống chỉ còn dải tab
ribbon.IsMinimized = true;`,
                callout: {
                    type: "warn",
                    body: "<code>CollapsePriority = 0</code> nghĩa là <b>ghim</b>: group đó không bao giờ bị gộp, kể cả khi ribbon rất hẹp. Dùng cho các lệnh không được phép biến mất."
                }
            },
            {
                title: "Flyout của group",
                desc: "Mũi tên cạnh tên group mở flyout chứa toàn bộ lệnh của group đó — giống panel flyout của AutoCAD. Flyout rộng tối thiểu bằng group và đóng khi bấm ra ngoài hoặc chọn một lệnh.",
                csharp: `// Mở/đóng flyout bằng code
group.IsDropDownOpen = true;

// Nội dung flyout là menu dựng từ item của group
RibbonMenu flyout = group.CollapsedMenu;`,
                callout: {
                    type: "warn",
                    body: "Flyout hiện là <b>menu lệnh</b> (icon + tên), chưa phải bản panel mở rộng như AutoCAD — một control WPF chỉ có một cha trong visual tree nên không thể bê nguyên control đang hiển thị vào popup."
                }
            },
            {
                title: "Quick Access — thanh lệnh luôn hiện",
                desc: "Dải lệnh luôn hiện: mặc định nằm <b>cùng hàng với tab</b>, sát mép phải (`QuickAccessPlacement.HeaderRight`); đổi sang `AboveRibbon` hoặc `BelowRibbon` nếu muốn nó thành dải riêng. Lệnh ghim vào đây luôn bấm được dù đang ở tab nào. Mũi tên bên phải mở panel tùy biến: mọi lệnh trong ribbon, nhóm theo tab, có icon và dấu tích ở lệnh đã ghim. Panel giới hạn chiều cao và cuộn được, bấm nhiều lệnh liên tiếp mà không bị đóng.",
                csharp: `// Ghim / bỏ ghim
ribbon.PinToQuickAccess(saveButton);
ribbon.UnpinFromQuickAccess(saveButton);

// Hiện cả nhãn chữ, không chỉ icon
ribbon.QuickAccess.ShowLabels = true;

// Mở menu tùy biến bằng code
ribbon.QuickAccess.IsCustomizeOpen = true;

// Đổi vị trí QAT: inline (mặc định), trên dải tab, hoặc dưới vùng nội dung
ribbon.QuickAccessPlacement = RibbonQuickAccessPlacement.AboveRibbon;

// Tự dựng danh sách ghim của riêng bạn
var bar = new RibbonQuickAccessToolBar { PinnableItems = ribbon.AllItems };
bar.Pin(ribbon.AllItems[0]);`,
                callout: {
                    type: "tip",
                    body: "Một control WPF chỉ có một cha trong visual tree, nên QAT không chứa chính <code>RibbonItem</code> gốc mà chứa bản sao nhẹ (<code>RibbonQuickAccessButton</code>) dùng chung <code>Command</code>, icon và tooltip. Nhờ vậy ghim lệnh không nhân đôi logic."
                }
            },
            {
                title: "File menu — ApplicationMenu",
                desc: "<code>RibbonApplicationMenu</code> là khối nổi ở góc trái trên, tách khỏi dải tab. Bấm vào mở panel hai cột: lệnh chính bên trái, tài liệu gần đây bên phải, và một nhóm lệnh chân trang (Options, Exit). Khối này mặc định <b>trung tính</b> (nền xám, chữ đậm) để không tranh chấp với màu nhấn — màu nhấn đã dành cho tab đang chọn; muốn khối màu như Office 2010 thì đặt <code>Background</code> / <code>Foreground</code> cho control.",
                xaml: `<ribbon:Ribbon.ApplicationMenu>
    <ribbon:RibbonApplicationMenu Header="File" RecentHeader="Mở gần đây">
        <ribbon:RibbonMenuItem Header="New" InputGestureText="Ctrl+N" />
        <ribbon:RibbonMenuItem Header="Open" InputGestureText="Ctrl+O" />
        <ribbon:RibbonMenuSeparator />
        <ribbon:RibbonMenuItem Header="Print" InputGestureText="Ctrl+P" />
    </ribbon:RibbonApplicationMenu>
</ribbon:Ribbon.ApplicationMenu>`,
                csharp: `var file = new RibbonApplicationMenu { Header = "File", RecentHeader = "Mở gần đây" };
file.Items.Add(new RibbonMenuItem { Header = "New", InputGestureText = "Ctrl+N" });
file.FooterItems = new object[] { new RibbonMenuItem { Header = "Exit" } };
file.RecentItems = new object[] { "Tower-A.dwg", "Site-plan.dwg" };

ribbon.ApplicationMenu = file;`,
                callout: {
                    type: "warn",
                    body: "Đây là <b>menu panel</b> kiểu Office 2010, không phải backstage toàn màn hình kiểu 2013+. Muốn backstage thì dựng một <code>Grid</code> riêng và hoán đổi nội dung cửa sổ."
                }
            },
            {
                title: "Tab ngữ cảnh",
                desc: "<code>RibbonContextualTabGroup</code> gói các tab chỉ tồn tại trong một chế độ (Sketch, Table, Modify). Đặt thẳng vào <code>ItemsSource</code>; ribbon tự bung thành các tab con và vẽ dải màu phía trên, gộp các tab liền nhau cùng nhóm vào một dải.",
                xaml: `<ribbon:Ribbon>
    <ribbon:RibbonTabItem Header="Home" />

    <ribbon:RibbonContextualTabGroup Header="Công cụ vẽ" Accent="#D93B3B">
        <ribbon:RibbonTabItem Header="Sketch" />
    </ribbon:RibbonContextualTabGroup>
</ribbon:Ribbon>`,
                csharp: `var sketch = new RibbonContextualTabGroup
{
    Header = "Công cụ vẽ",
    Accent = Brushes.Crimson
};
sketch.Items.Add(new RibbonTabItem { Header = "Sketch" });

ribbon.ItemsSource = new object[] { homeTab, sketch };

// Bật/tắt cả nhóm theo chế độ đang làm việc
sketch.Visibility = Visibility.Visible;   // hoặc Collapsed`,
                callout: {
                    type: "tip",
                    body: "Dùng <code>Visibility</code> chứ không phải thêm/xóa tab: tab bị ẩn vẫn giữ nguyên trạng thái, và <code>SelectedIndex</code> tự nhảy về tab hợp lệ gần nhất. Đặt <code>IsAvailable</code> trên từng tab nếu cần ẩn lẻ."
                }
            },
            {
                title: "Screen tip",
                desc: "Mỗi lệnh tự có screen tip dựng từ <code>Header</code>, <code>ScreenTip</code> và <code>GestureText</code> — không cần viết thêm gì. Cần hình ảnh hay chân trang thì gán <code>RibbonToolTip</code> trực tiếp.",
                xaml: `<ribbon:RibbonButton Header="Save"
                     ScreenTip="Ghi bản vẽ hiện tại"
                     GestureText="Ctrl+S" />

<ribbon:RibbonButton Header="Print">
    <ribbon:RibbonButton.ToolTip>
        <ribbon:RibbonToolTip Title="Print"
                              Description="Gửi bản vẽ tới máy in hoặc xuất PDF."
                              GestureText="Ctrl+P"
                              Footer="Nhấn F1 để xem trợ giúp" />
    </ribbon:RibbonButton.ToolTip>
</ribbon:RibbonButton>`,
                csharp: `// Tự sinh từ Header + ScreenTip + GestureText
save.GestureText = "Ctrl+S";

// Hoặc tự dựng
var tip = new RibbonToolTip
{
    Title = "Print",
    Description = "Gửi bản vẽ tới máy in hoặc xuất PDF.",
    GestureText = "Ctrl+P",
    Footer = "Nhấn F1 để xem trợ giúp",
    Preview = screenshot
};
print.ToolTip = tip;`,
                callout: {
                    type: "tip",
                    body: "Tooltip vẫn hiện khi lệnh bị vô hiệu hóa (<code>ToolTipService.ShowOnDisabled</code>), vì lý do lệnh bị mờ thường chính là thứ người dùng cần biết. Gán <code>ToolTip</code> của riêng bạn thì ribbon không ghi đè nữa."
                }
            },
            {
                title: "Ô nhập liệu trên lưới",
                desc: "<code>RibbonComboBox</code>, <code>RibbonTextBox</code> và <code>RibbonCheckBox</code> sống ngay trên lưới 3 hàng. Cả ba thừa hưởng <code>RibbonItem</code> nên vẫn có <code>Command</code>, <code>ScreenTip</code>, <code>Size</code> và <b><code>ColumnSpan</code></b> — số cột mà control chiếm, mặc định 1.",
                xaml: `<ribbon:RibbonGroup Header="Text">
    <ribbon:RibbonComboBox Header="Font" ColumnSpan="2"
                           ItemsSource="{Binding Fonts}" SelectedItem="{Binding Font}" />
    <ribbon:RibbonComboBox Header="Size" ColumnSpan="2"
                           ItemsSource="{Binding Sizes}" SelectedItem="{Binding Size}" />

    <ribbon:RibbonSeparator />

    <ribbon:RibbonCheckBox Header="Snap" IsChecked="{Binding Snap}" />
    <ribbon:RibbonTextBox Header="Step" ColumnSpan="2" Text="{Binding Step}" />
</ribbon:RibbonGroup>`,
                csharp: `var font = new RibbonComboBox
{
    Header = "Font",          // nhãn bên trái, bỏ trống nếu không cần
    ColumnSpan = 2,           // rộng 2 cột = 140 px
    ItemsSource = fonts,
    DisplayMemberPath = "Name"
};

font.SelectedItem = "Calibri";    // hoặc font.SelectedIndex = 0

var step = new RibbonTextBox { Header = "Step", Text = "10", ColumnSpan = 2 };
var snap = new RibbonCheckBox { Header = "Snap", IsChecked = true };`,
                callout: {
                    type: "tip",
                    body: "Một cột chỉ rộng 70 px, nên <code>RibbonComboBox</code> và <code>RibbonTextBox</code> gần như luôn cần <code>ColumnSpan=\"2\"</code> — nếu không nhãn sẽ ăn hết chỗ của ô nhập. Hai control cùng <code>ColumnSpan</code> đứng liền nhau sẽ xếp <b>chồng theo hàng</b> trong cùng một dải cột, nhờ vậy nhóm không bị phình ngang."
                }
            },
            {
                title: "Gallery",
                desc: "<code>RibbonGallery</code> là lưới ảnh có chọn, dùng cho bảng màu hay thư viện kiểu. Trên ribbon lưới chỉ vẽ <b>ô màu</b>; nút mũi tên mở bản đầy đủ trong popup, nơi mỗi ô có thêm tên dưới hình. Hai chế độ xem dùng chung một <code>SelectedItem</code>, nên chọn ở đâu cũng như nhau.",
                xaml: `<ribbon:RibbonGroup Header="Styles">
    <ribbon:RibbonGallery ColumnSpan="3"
                          ItemWidth="34" ItemHeight="32"
                          ItemsSource="{Binding Swatches}"
                          SelectedItem="{Binding Material}" />
</ribbon:RibbonGroup>`,
                csharp: `var gallery = new RibbonGallery
{
    ColumnSpan = 3,
    ItemWidth = 34,           // ô trên ribbon: chỉ đủ cho ô màu
    ItemHeight = 32,
    ItemsSource = new[]
    {
        new RibbonGalleryItem("Steel", steelIcon),
        new RibbonGalleryItem("Sand", sandIcon)
    }
};

gallery.SelectedItem = gallery.ItemsSource.First();
gallery.SelectionChanged += (_, _) => Apply(gallery.SelectedItem);

// Popup rộng rãi hơn nên hiện được tên dưới mỗi ô
gallery.ExpandedItemTemplate = (DataTemplate)FindResource("MyNamedTile");

// Ẩn mũi tên mở rộng khi lưới đã đủ chỗ
gallery.ShowExpandButton = false;`,
                callout: {
                    type: "warn",
                    body: "<code>RibbonGalleryItem</code> là object thuần (Header / Icon / ToolTip / Tag), không phải control — nhờ vậy cùng một item hiện được ở cả lưới trên ribbon lẫn popup. Lưới trên ribbon mặc định chỉ vẽ hình (<code>ItemTemplate</code>), popup vẽ hình + tên (<code>ExpandedItemTemplate</code>): ô 32 px không đủ chỗ cho chữ nên nhãn sẽ bị cắt hoặc tràn sang hàng dưới. Gallery tự bật <code>IsFixedSize</code> nên <b>không</b> bị ép xuống một hàng khi ribbon thu gọn."
                }
            },
            {
                title: "Key tip (Alt)",
                desc: "Nhấn <b>Alt</b> khi ribbon đang mở: mỗi tab, mỗi lệnh và mỗi nút mở hộp thoại của group hiện một badge chữ cái. Gõ chữ cái là lệnh chạy; gõ chữ cái của tab khác là đổi tab rồi hiện badge của tab đó; <b>Esc</b> để thoát. Không cần khai báo gì thêm — bỏ trống <code>KeyTip</code> thì ribbon tự lấy chữ cái đầu của <code>Header</code> và tự tránh trùng.",
                xaml: `<ribbon:RibbonTabItem Header="Home" KeyTip="H">
    <ribbon:RibbonGroup Header="Font" KeyTip="F"
                        DialogLauncherCommand="{Binding OpenFontDialog}">
        <ribbon:RibbonToggleButton Header="Bold" KeyTip="B" />
        <ribbon:RibbonToggleButton Header="Italic" />      <!-- tự thành "I" -->
    </ribbon:RibbonGroup>
</ribbon:RibbonTabItem>`,
                csharp: `// Alt do ribbon tự bắt ở window chứa nó, không cần khai báo InputBinding
ribbon.ShowKeyTips();      // mở bằng tay, ví dụ từ một nút trên toolbar
ribbon.HideKeyTips();

bool showing = ribbon.IsShowingKeyTips;

// Tắt hẳn phím Alt nếu ứng dụng đã dùng Alt cho việc khác
KeyTipService.SetIsEnabled(ribbon, false);

// Nơi tự xử lý bàn phím (remote, gamepad) thì đẩy phím vào thẳng
bool handled = ribbon.ProcessKeyTipKey(Key.B);`,
                callout: {
                    type: "tip",
                    body: "Badge được vẽ trong chính ribbon (<code>PART_KeyTipLayer</code>), không dùng <code>AdornerLayer</code>, nên nó vẫn đúng chỗ khi ribbon nằm trong popup hay trong một panel cuộn. Lệnh không nhìn thấy (group đã gộp, tab đang ẩn) thì không có badge — và <b>không</b> bấm được bằng key tip. Bộ badge hiện cả lệnh của tab đang mở, nên lệnh đang thấy trước mắt chỉ cách một chữ cái."
                }
            },
            {
                title: "Màu, tìm kiếm và lưu trạng thái",
                desc: "<code>RibbonColorPicker</code> là ô màu lớn có swatch và popup chứa <code>ColorPicker</code> đầy đủ. <code>RibbonSearchBox</code> bọc <code>SearchBox</code> thành một ô trên lưới. Còn <code>RibbonStateStore</code> lo phần người dùng mong đợi nhất: mở lại app thì tab, trạng thái thu gọn, lệnh đã ghim và group đã gấp vẫn y như cũ.",
                xaml: `<ribbon:RibbonGroup Header="Paint">
    <ribbon:RibbonColorPicker Header="Colour" KeyTip="C"
                              SelectedColor="#DC2626" ShowAlpha="True" />
</ribbon:RibbonGroup>

<ribbon:RibbonGroup Header="Find">
    <ribbon:RibbonSearchBox Header="Find" ColumnSpan="3"
                            Placeholder="Tìm lệnh, màu…"
                            SearchCommand="{Binding SearchCommand}" />
</ribbon:RibbonGroup>`,
                csharp: `var colour = new RibbonColorPicker { Header = "Colour", ShowAlpha = true };
colour.SelectedColorChanged += (_, _) => Apply(colour.SelectedColor);

var search = new RibbonSearchBox
{
    ColumnSpan = 3,
    Placeholder = "Tìm lệnh, màu…",
    IsInstantSearch = true,       // chạy khi gõ, không cần Enter
    SearchCommand = SearchCommand
};

// Lưu / khôi phục trạng thái người dùng
RibbonStateStore.Save(ribbon.CaptureState(), "ribbon.json");
ribbon.ApplyState(RibbonStateStore.Load("ribbon.json"));

// Hoặc tự quản lý chuỗi JSON
string json = RibbonStateStore.ToJson(ribbon.CaptureState());
RibbonState? state = RibbonStateStore.FromJson(json);`,
                callout: {
                    type: "warn",
                    body: "Trạng thái được nhận diện bằng <code>Id</code>: tab dùng <code>Id</code> rồi mới đến <code>Header</code>, lệnh dùng <code>Id</code>, group dùng <code>tab:Header</code>. Thiếu <code>Id</code> thì lệnh <b>không</b> được ghim lại sau khi khởi động. Id lạ trong file cũ bị bỏ qua, nên bản lưu cũ không làm hỏng ribbon mới."
                }
            },
            {
                title: "MenuBar phía trên ribbon",
                desc: "<code>RibbonMenuBar</code> là dải menu ngang đặt trên ribbon (hoặc dùng một mình trong app đơn giản). Mỗi <code>RibbonMenuBarItem</code> mở dropdown chứa <code>RibbonMenuItem</code> và <code>RibbonMenuSeparator</code>, tận dụng toàn bộ hệ thống menu đã có. Click mở dropdown, hover chuyển đổi khi menu đang mở, Left/Right di chuyển, Escape đóng. Thanh luôn giãn hết bề rộng ribbon; đường kẻ dưới nó tắt bằng <code>ShowDivider=\"False\"</code> khi muốn thanh hoà vào nền.",
                xaml: `<ribbon:RibbonMenuBar>
    <ribbon:RibbonMenuBarItem Header="File">
        <ribbon:RibbonMenuItem Header="New" InputGestureText="Ctrl+N" />
        <ribbon:RibbonMenuItem Header="Open…" InputGestureText="Ctrl+O" />
        <ribbon:RibbonMenuSeparator />
        <ribbon:RibbonMenuItem Header="Exit" />
    </ribbon:RibbonMenuBarItem>
    <ribbon:RibbonMenuBarItem Header="Edit">
        <ribbon:RibbonMenuItem Header="Undo" InputGestureText="Ctrl+Z" />
        <ribbon:RibbonMenuItem Header="Redo" InputGestureText="Ctrl+Y" />
    </ribbon:RibbonMenuBarItem>
</ribbon:RibbonMenuBar>`,
                csharp: `var file = new RibbonMenuBarItem { Header = "File" };
file.Items.Add(new RibbonMenuItem { Header = "New" });

var bar = new RibbonMenuBar();
bar.Items.Add(file);
bar.Items.Add(new RibbonMenuBarItem { Header = "Edit" });`
            },
            {
                title: "Thuộc tính mở rộng cho host",
                desc: "Sáu công tắc để gắn ribbon vào một shell cụ thể: chọn tab theo <code>Id</code>, tắt rung cuối của thang thu gọn, khoá thanh Quick Access, nhường phím <code>Alt</code> cho host, bỏ viền/nhãn group, và đặt nội dung cho trạng thái rỗng.",
                xaml: `<ribbon:Ribbon ShowBorder="False"
               IsGroupCollapseEnabled="False"
               IsQuickAccessCustomizable="False"
               IsKeyTipEnabled="False"
               SelectedTabId="{Binding ActiveTabId}"
               EmptyContent="No workspace open." />

<!-- Nhãn group là attached property, đặt trên ribbon là áp cho mọi group -->
<ribbon:Ribbon ShowGroupLabels="False" />`,
                csharp: `// Chọn tab theo Id: đổi thứ tự tab cũng không trỏ sai tab
ribbon.SelectedTabId = "insert";

// Tắt rung cuối: group chỉ bị hạ cỡ, không bao giờ gộp thành một nút
ribbon.IsGroupCollapseEnabled = false;

// Khoá thanh Quick Access: lệnh đã ghim vẫn bấm được, chỉ mất nút tùy biến
ribbon.IsQuickAccessCustomizable = false;

// Nhường Alt cho Revit / AutoCAD
ribbon.IsKeyTipEnabled = false;

// Dock vào panel riêng
ribbon.ShowBorder = false;
Ribbon.SetShowGroupLabels(ribbon, false);

// Trạng thái rỗng
ribbon.EmptyContent = "No workspace open.";   // IsEmpty = true khi không còn tab nào`,
                callout: {
                    type: "tip",
                    body: "<code>SelectedTabId</code> và <code>SelectedIndex</code> cùng hai chiều: chọn tab thì <code>Id</code> được đẩy ra, đặt <code>Id</code> thì tab được chọn. <code>IsGroupCollapseEnabled = false</code> còn tự mở lại những group vừa bị gộp, nên bật/tắt lúc chạy vẫn đúng."
                }
            }
        ],
        related: ["ribboncommands", "splitbutton", "concostwindow"]
    },

    ribboncommands: {
        title: "Ribbon commands — nút, split, menu",
        badge: "Layout & Shell",
        desc: "Bộ lệnh đặt trong ribbon: nút thường, toggle, radio, split button hai vùng, dropdown và menu lồng nhau. Tất cả đều theo <code>ICommand</code>: <code>IsEnabled</code> tự bám <code>CanExecute</code>.",
        keywords: ["ribbon button", "split button", "dropdown", "menu", "command", "radio"],
        sections: [
            {
                title: "Nút, toggle và radio",
                desc: "<code>RibbonToggleButton</code> giữ trạng thái bật/tắt. Đặt cùng <code>GroupName</code> thì chúng loại trừ nhau như radio — dùng <code>RibbonRadioButton</code> để không thể bỏ chọn bằng cách bấm lại.",
                xaml: `<ribbon:RibbonGroup Header="Units">
    <ribbon:RibbonRadioButton Header="mm" Size="Small" GroupName="unit" IsChecked="True" />
    <ribbon:RibbonRadioButton Header="cm" Size="Small" GroupName="unit" />
    <ribbon:RibbonRadioButton Header="m"  Size="Small" GroupName="unit" />
</ribbon:RibbonGroup>`,
                csharp: `var bold = new RibbonToggleButton { Header = "Bold", Size = RibbonItemSize.Small };
bold.Command = EditingCommands.ToggleBold;   // IsEnabled bám CanExecute

// Radio: bấm lại không bỏ chọn
var mm = new RibbonRadioButton { Header = "mm", GroupName = "unit", IsChecked = true };`
            },
            {
                title: "Split button — hai vùng bấm",
                desc: "Thân nút chạy <code>Command</code>, mũi tên mở menu. Hai vùng hit-test tách rời: nút lớn thì vùng mũi tên là dải đáy (nơi tam giác nằm), nút nhỏ thì là dải phải.",
                xaml: `<ribbon:RibbonSplitButton Header="Export" Size="Large">
    <ribbon:RibbonMenuItem Header="PDF" />
    <ribbon:RibbonMenuSeparator />
    <ribbon:RibbonMenuItem Header="CSV" />
</ribbon:RibbonSplitButton>`,
                csharp: `var export = new RibbonSplitButton { Header = "Export", Size = RibbonItemSize.Large };
export.Command = ExportCommand;                       // vùng trái
export.Items.Add(new RibbonMenuItem { Header = "PDF" });  // vùng mũi tên
export.ArrowWidth = 20;                               // bề rộng vùng mũi tên`
            },
            {
                title: "Menu lồng nhau",
                desc: "<code>RibbonMenu</code> là danh sách dọc; <code>RibbonMenuItem</code> chạy lệnh hoặc mở submenu nếu có item con. <code>InputGestureText</code> chỉ hiển thị phím tắt, không tự đăng ký.",
                xaml: `<ribbon:RibbonDropDownButton Header="Insert" Size="Large">
    <ribbon:RibbonMenuItem Header="Table" />
    <ribbon:RibbonMenuItem Header="Chart">
        <ribbon:RibbonMenuItem Header="Bar" />
        <ribbon:RibbonMenuItem Header="Line" />
    </ribbon:RibbonMenuItem>
    <ribbon:RibbonMenuSeparator />
    <ribbon:RibbonMenuItem Header="Link" InputGestureText="Ctrl+K" />
</ribbon:RibbonDropDownButton>`,
                callout: {
                    type: "tip",
                    body: "Submenu mở khi rê chuột hoặc bấm phím <code>→</code>, đóng bằng <code>Esc</code>. Chọn một lệnh sẽ tự đóng cả popup cha."
                }
            }
        ],
        related: ["ribbon", "splitbutton", "badge"]
    },
    datagridpanels: {
        title: "Panel phụ trợ cho DataGrid — lọc, tìm, chọn cột, tổng hợp",
        badge: "Basic Controls",
        desc: "Bốn control độc lập gắn vào một <code>DataGrid</code> có sẵn để biến bảng chỉ-đọc thành công cụ dữ liệu: dải lọc theo từng cột, ô tìm kiếm toàn bảng, menu chọn cột hiển thị và dòng tổng hợp ở chân bảng. Cả bốn đều <b>không cần viết lại DataGrid</b> — chỉ trỏ <code>TargetGrid</code> vào bảng là xong.",
        keywords: ["datagrid", "filter", "search", "column chooser", "summary", "lọc", "tìm kiếm", "tổng hợp"],
        sections: [
            {
                title: "Dải lọc theo cột",
                desc: "Mỗi cột có một ô nhập; gõ gì lọc ngay theo kiểu <code>Contains</code> không phân biệt hoa thường trên chuỗi hiển thị của ô đó. Ô nhập <b>trùng khít chiều rộng cột</b> nên luôn nằm đúng dưới cột nó lọc, kể cả khi kéo giãn cột hay cuộn ngang. Đường dẫn binding nhiều cấp như <code>Material.Grade</code> cũng đọc được.",
                xaml: `<StackPanel>
    <controls:DataGridFilterPanel TargetGrid="{Binding ElementName=beamGrid}" />

    <DataGrid x:Name="beamGrid"
              ItemsSource="{Binding Beams}"
              AutoGenerateColumns="False"
              CanUserAddRows="False">
        <DataGrid.Columns>
            <DataGridTextColumn Header="Element" Binding="{Binding Name}" Width="2*" />
            <DataGridTextColumn Header="Grade" Binding="{Binding Material.Grade}" Width="*" />
        </DataGrid.Columns>
    </DataGrid>
</StackPanel>`,
                csharp: `var panel = new DataGridFilterPanel { TargetGrid = beamGrid };

// Lọc sẵn một cột trước khi người dùng gõ
panel.Filters.Single(f => f.Header == "Grade").FilterText = "C30";

// Xoá toàn bộ (hoặc gắn vào nút ClearFiltersCommand)
panel.ClearFilters();

// Biết khi nào bảng đã lọc lại
panel.FiltersApplied += (_, __) => status.Text = $"{beamGrid.Items.Count} dòng";`,
                callout: {
                    type: "tip",
                    body: "Dải lọc và ô tìm kiếm <b>cộng dồn</b> chứ không ghi đè nhau: một dòng phải thoả cả hai. Xoá ô tìm kiếm sẽ để nguyên bộ lọc theo cột."
                }
            },
            {
                title: "Tìm kiếm toàn bảng",
                desc: "Một ô duy nhất quét mọi cột đang hiển thị. Cột bị ẩn bằng <code>Visibility</code> sẽ được bỏ qua, nên bảng đã giấu cột nội bộ thì tìm kiếm cũng không lộ nội dung của cột đó.",
                xaml: `<StackPanel>
    <controls:DataGridSearchPanel TargetGrid="{Binding ElementName=beamGrid}"
                                  PlaceholderText="Tìm mã cấu kiện, mác bê tông…" />
    <DataGrid x:Name="beamGrid" ItemsSource="{Binding Beams}" AutoGenerateColumns="False" />
</StackPanel>`,
                csharp: `var search = new DataGridSearchPanel { TargetGrid = beamGrid };

search.SearchText = "C30";      // lọc ngay, hai chiều
search.ClearSearch();           // trả lại toàn bộ dòng

// IsSearching dùng để bật/tắt nhãn "đang tìm" trên toolbar
if (search.IsSearching) hint.Visibility = Visibility.Visible;`,
                callout: {
                    type: "tip",
                    body: "Ô tìm kiếm dùng sẵn nút ✕ của <code>TextBox</code> (<code>InfoElement.ShowClearButton</code>), không cần thêm nút Clear riêng."
                }
            },
            {
                title: "Chọn cột hiển thị",
                desc: "Nút thả xuống liệt kê mọi cột của bảng; bỏ tick là cột ẩn ngay. Danh sách tự dựng lại khi bảng thêm/bớt cột và mỗi lần mở menu, nên nếu app ẩn cột từ menu riêng thì trạng thái tick vẫn đúng.",
                xaml: `<StackPanel>
    <controls:DataGridColumnChooser TargetGrid="{Binding ElementName=beamGrid}"
                                    HeaderText="Cột hiển thị"
                                    HorizontalAlignment="Left"
                                    Margin="0,0,0,8" />
    <DataGrid x:Name="beamGrid" ItemsSource="{Binding Beams}" AutoGenerateColumns="False" />
</StackPanel>`,
                csharp: `var chooser = new DataGridColumnChooser
{
    TargetGrid = beamGrid,
    HeaderText = "Cột hiển thị"
};

// Ẩn cột thứ hai, rồi trả lại tất cả
chooser.Columns[1].IsVisible = false;
chooser.ShowAll();

// Mở/đóng bằng code (menu tự đóng khi bấm ra ngoài)
chooser.IsOpen = true;`,
                callout: {
                    type: "tip",
                    body: "Cột được khớp theo <b>tham chiếu</b> chứ không theo tiêu đề, nên hai cột trùng tên vẫn ẩn đúng cột. Muốn khoá tính năng này khi in ấn thì ẩn luôn nút bằng <code>Visibility</code>."
                }
            },
            {
                title: "Dòng tổng hợp",
                desc: "Một dòng ngay dưới bảng, mỗi cột một ô: cột số hiện <code>Sum</code>, cột chữ hiện số dòng. Dòng này <b>bám theo hình học của bảng</b> — độ rộng cột, thứ tự kéo cột, bề rộng header dòng và cả vị trí cuộn ngang — nên con số luôn nằm dưới đúng cột.",
                xaml: `<StackPanel>
    <DataGrid x:Name="beamGrid" ItemsSource="{Binding Beams}" AutoGenerateColumns="False" />
    <controls:DataGridSummary TargetGrid="{Binding ElementName=beamGrid}" DisplayFormat="N0" />
</StackPanel>`,
                csharp: `var summary = new DataGridSummary
{
    TargetGrid = beamGrid,
    Aggregate = DataGridSummaryAggregate.Auto,   // số → Sum, chữ → Count
    DisplayFormat = "N0"
};

// Đổi cách tính cho riêng một cột
DataGridSummary.SetAggregateOnColumn(lengthColumn, DataGridSummaryAggregate.Average);

// Tính lại khi dữ liệu đổi ngoài luồng binding
summary.RefreshSummaries();`,
                callout: {
                    type: "tip",
                    body: "Tổng hợp chạy trên <b>view đã lọc</b>: lọc còn 5 dòng thì tổng cũng chỉ của 5 dòng đó. Số được đọc theo culture hiện tại trước, rồi mới thử culture bất biến, nên dữ liệu <code>\"1,5\"</code> kiểu Việt Nam vẫn cộng đúng."
                }
            }
        ],
        related: ["datagrid", "datapager", "stackedheaderdatagrid"]
    },

    commandpalette: {
        title: "CommandPalette — thanh lệnh Ctrl+K",
        badge: "Mới",
        desc: "Hộp tìm kiếm phủ lên màn hình, lọc danh sách lệnh khi gõ và chạy bằng <code>Enter</code>. Tìm <b>không phân biệt dấu tiếng Việt</b>, điều hướng bằng mũi tên, và tự vẽ lớp phủ tối phía sau nên không phải quản popup hay chuyển focus.",
        keywords: ["command palette", "ctrl k", "tìm lệnh", "không dấu", "quick actions", "spotlight"],
        variants: [
            { key: "CONCOST.CommandPalette.Base", desc: "Scrim tối <code>#8C0F172A</code>, thẻ rộng tối đa 600px, dòng cao 44px, footer gợi ý phím." }
        ],
        sections: [
            {
                title: "Mở palette bằng Ctrl+K",
                desc: "Đặt palette vào cùng <code>Grid</code> với nội dung trang và bind <code>IsOpen</code>. Phím tắt do app đăng ký — control không chiếm phím toàn cục khi chưa nằm trong cây visual.",
                xaml: `<Grid>
    <ContentControl Content="{Binding CurrentPage}" />

    <ui:CommandPalette x:Name="Palette"
                       IsOpen="{Binding IsPaletteOpen, Mode=TwoWay}"
                       ItemsSource="{Binding Commands}"
                       Placeholder="Tìm lệnh, trang hoặc cấu hình…"
                       EmptyText="Không có lệnh nào khớp" />

    <Grid.InputBindings>
        <KeyBinding Key="K" Modifiers="Control" Command="{Binding TogglePaletteCommand}" />
    </Grid.InputBindings>
</Grid>`,
                csharp: `// Mỗi dòng là dữ liệu thuần: tiêu đề, mô tả, lệnh, icon, phím tắt, nhóm
Commands = new List<CommandPaletteItem>
{
    new("Xuất PDF", "Xuất bản vẽ hiện tại", ExportPdfCommand, IconKind.File, "Ctrl+P", "Xuất bản"),
    new("Tổng quan dự án", "Bảng KPI và tiến độ", NavigateCommand, IconKind.ChartLine, "Ctrl+1", "Điều hướng"),
    new("Cấu hình thép", "Đường kính, mác thép, lớp bảo vệ", SettingsCommand, IconKind.Settings, null, "Hệ thống")
};

// Biết lệnh nào vừa chạy để ghi log / đóng palette
Palette.ItemInvoked += (_, e) => Status = "Đã chạy: " + e.Item.Title;`,
                callout: {
                    type: "tip",
                    body: "Muốn giữ palette mở để chạy liên tiếp nhiều lệnh thì đặt <code>CloseOnInvoke=\"False\"</code>; mặc định palette tự đóng sau khi chạy."
                }
            },
            {
                title: "Tìm không dấu, khớp từng từ khoá",
                desc: "Chuỗi truy vấn được chuẩn hoá <code>FormD</code> rồi bỏ dấu, riêng <code>đ</code> đổi thành <code>d</code> vì <code>FormD</code> không tách được chữ này. Mỗi từ khoá khớp độc lập trên tiêu đề, mô tả, nhóm, phím tắt và <code>Keywords</code>.",
                xaml: `<ui:CommandPalette ItemsSource="{Binding Commands}" IsOpen="True" />`,
                csharp: `// Gõ "duong kinh" ra "Đường kính", "hoa don" ra "Hoá đơn"
var tokens = CommandPaletteText.Tokenize("duong kinh");   // ["duong", "kinh"]
bool hit = CommandPaletteText.Matches(item, tokens);

// Từ đồng nghĩa: thêm vào Keywords để tìm được bằng từ khác
new CommandPaletteItem("Bảng khối lượng", "Bóc tách theo cấu kiện",
    QuantityCommand, IconKind.Table, "Ctrl+2", "Dữ liệu")
{
    Keywords = "bom tong hop volume takeoff"
};`,
                callout: {
                    type: "note",
                    body: "Chiều cao danh sách tự cắt theo <b>số dòng nguyên</b> (44px/dòng, <code>MaxListHeight</code> mặc định 276 = 6 dòng) nên không bao giờ hiện nửa lệnh ở mép dưới. Highlight luôn bám danh sách sau lọc, không trỏ vào dòng vừa bị lọc mất."
                }
            }
        ],
        related: ["searchbox", "contextmenu", "hotkeymanager"]
    },

    radialgauge: {
        title: "RadialGauge — đồng hồ KPI tròn",
        badge: "Mới",
        desc: "Cung nền + cung giá trị + vạch chia + số ở giữa, vẽ trong <code>OnRender</code> bằng <code>StreamGeometry</code> với đầu nét tròn nên sắc ở mọi kích thước và mọi DPI. Giá trị clamp vào <code>Minimum</code>–<code>Maximum</code> rồi chạy êm bằng cubic ease.",
        keywords: ["gauge", "kpi", "đồng hồ", "vòng tròn", "dashboard", "progress ring"],
        sections: [
            {
                title: "Đồng hồ phần trăm",
                desc: "Mặc định cung mở ở đáy (bắt đầu 135°, quét 270°). <code>ValueFormat</code> theo định dạng số chuẩn, <code>Suffix</code> và <code>Label</code> là chữ đi kèm.",
                xaml: `<ui:RadialGauge Width="180" Height="180"
               Minimum="0" Maximum="100" Value="{Binding Progress}"
               ValueFormat="N0" Suffix="%"
               Label="Tiến độ bóc tách"
               ShowTicks="True" TickCount="10"
               TrackThickness="14" />`,
                csharp: `var gauge = new RadialGauge
{
    Minimum = 0,
    Maximum = 100,
    Value = 72,
    StartAngle = 135,
    SweepAngle = 270,
    ValueFormat = "N0",
    Suffix = "%",
    Label = "Hoàn thành",
    ShowTicks = true,
    TickCount = 10,
    IsAnimated = true,
    AnimationDuration = TimeSpan.FromMilliseconds(520)
};`,
                callout: {
                    type: "tip",
                    body: "Màu cung lấy từ <code>ValueBrush</code>, nền cung từ <code>TrackBrush</code> — để trống thì lấy brush theme, nên đổi Light/Dark là đồng hồ tự đổi theo. Vạch chia dùng <code>TextTertiary</code>: nếu dùng màu track thì gần như không nhìn thấy."
                }
            },
            {
                title: "Đồng hồ áp lực đất, thang ngược",
                desc: "Đặt <code>Minimum</code> lớn hơn <code>Maximum</code> để có thang ngược (ví dụ độ sâu), hoặc đổi <code>StartAngle</code>/<code>SweepAngle</code> để cung mở ở nửa trên cho bảng điều khiển thấp.",
                xaml: `<ui:RadialGauge Width="150" Height="150"
               Minimum="0" Maximum="250" Value="{Binding Pressure}"
               StartAngle="180" SweepAngle="180"
               ValueFormat="F1" Suffix=" kPa"
               ShowValue="True" />`,
                csharp: `// Thang ngược: 0 ở đỉnh, giá trị càng lớn cung càng ngắn
gauge.Minimum = 10;
gauge.Maximum = 0;

// Giá trị ngoài khoảng được clamp, không ném lỗi
gauge.Value = 999;      // vẽ như Maximum

// Tắt animation cho lưới nhiều đồng hồ cập nhật liên tục
gauge.IsAnimated = false;`,
                callout: {
                    type: "note",
                    body: "Mỗi đồng hồ chỉ vẽ vài primitive, nhưng <b>đừng</b> bật animation cho hàng trăm đồng hồ cập nhật mỗi giây — mỗi lần đổi giá trị là một <code>DoubleAnimation</code>."
                }
            }
        ],
        related: ["statisticcard", "progress", "sparkline"]
    },

    particlefield: {
        title: "ParticleField — nền hạt trôi",
        badge: "Mới",
        desc: "Hạt trôi nhẹ làm nền cho hero, splash hoặc empty state. Hạt wrap quanh biên nên nền không bao giờ trống, <code>Seed</code> cho kết quả tái lập, và field tự dựng lại khi host đổi kích thước. Bật <code>ConnectDistance</code> để có hiệu ứng chòm sao.",
        keywords: ["particle", "hạt", "nền động", "constellation", "chòm sao", "splash", "ambient"],
        sections: [
            {
                title: "Nền hạt cho thẻ hero",
                desc: "Đặt làm phần tử đầu tiên trong <code>Grid</code> để nó nằm dưới nội dung. <code>IsHitTestVisible</code> đã tắt sẵn nên không chặn click.",
                xaml: `<Grid>
    <ui:ParticleField ParticleCount="70"
                      Speed="14"
                      Direction="270"
                      ParticleOpacity="0.6"
                      ParticleBrush="{DynamicResource CONCOST.Brush.Primary}" />

    <StackPanel Margin="32" VerticalAlignment="Center">
        <TextBlock Text="Bóc tách khối lượng" Style="{DynamicResource CONCOST.TextBlock.H1}" />
        <TextBlock Text="Nhập dữ liệu, xuất báo cáo." Style="{DynamicResource CONCOST.TextBlock.Body}" />
    </StackPanel>
</Grid>`,
                csharp: `var field = new ParticleField
{
    ParticleCount = 70,
    MinRadius = 1.0,
    MaxRadius = 2.6,
    Speed = 14,          // px/giây
    Direction = 270,     // 270 = rơi xuống, 90 = bay lên
    Spread = 0.28,       // độ lệch hướng, 0 = mọi hạt song song
    ParticleOpacity = 0.6,
    Seed = 42            // cùng seed = cùng bố cục
};`,
                callout: {
                    type: "note",
                    body: "Field dựng lại hạt khi kích thước thật khác kích thước lúc dựng — nếu không, hạt chỉ phủ một phần ba trên cùng của khung."
                }
            },
            {
                title: "Chòm sao: nối hạt gần nhau",
                desc: "Đặt <code>ConnectDistance &gt; 0</code> để vẽ đường nối giữa các hạt gần nhau. Khoảng cách tính bằng pixel; càng lớn càng nhiều đường và càng tốn CPU.",
                xaml: `<Border Height="220" CornerRadius="16" ClipToBounds="True"
        Background="{DynamicResource CONCOST.Brush.SurfaceAlt}">
    <ui:ParticleField ParticleCount="48"
                      ConnectDistance="90"
                      Speed="8"
                      ParticleOpacity="0.9"
                      ParticleBrush="{DynamicResource CONCOST.Brush.TextTertiary}" />
</Border>`,
                csharp: `// Số hạt đang hoạt động — dùng cho test hoặc chẩn đoán hiệu năng
int alive = field.ActiveParticleCount;

// Giảm tải khi cửa sổ mất focus
field.Speed = 0;`,
                callout: {
                    type: "tip",
                    body: "Ngân sách: khoảng 60 hạt không nối là thoải mái; bật <code>ConnectDistance</code> thì chi phí là <b>bậc hai</b> theo số hạt — 48 hạt là mức an toàn cho nền toàn màn hình."
                }
            }
        ],
        related: ["aurorabackground", "snowcanvas", "gridpattern"]
    },

    pagetransitionhost: {
        title: "PageTransitionHost — chuyển trang có hiệu ứng",
        badge: "Mới",
        desc: "Đổi <code>Content</code> là trang mới chạy hiệu ứng vào: fade, trượt từ bốn phía, hoặc scale. Animation nằm ở host nên dùng được với mọi loại nội dung, không cần storyboard riêng cho từng trang.",
        keywords: ["transition", "chuyển trang", "slide", "fade", "scale", "điều hướng"],
        variants: [
            { key: "CONCOST.PageTransitionHost.Base", desc: "Template tối giản: chỉ <code>ContentPresenter</code> bọc trong <code>TransformGroup</code> (scale + translate)." }
        ],
        sections: [
            {
                title: "Trượt ngang khi đổi màn hình",
                desc: "<code>Transition</code> nhận <code>Fade</code>, <code>SlideLeft</code>, <code>SlideRight</code>, <code>SlideUp</code>, <code>SlideDown</code> hoặc <code>Scale</code>. <code>Offset</code> là quãng trượt, <code>Duration</code> là thời gian chạy.",
                xaml: `<ui:PageTransitionHost Content="{Binding CurrentPage}"
                       Transition="SlideLeft"
                       Offset="32"
                       Duration="0:0:0.26" />`,
                csharp: `var host = new PageTransitionHost
{
    Transition = PageTransition.SlideLeft,
    Offset = 32,
    Duration = TimeSpan.FromMilliseconds(260),
    Content = new OverviewView()
};

// Đổi nội dung là tự chạy hiệu ứng vào
host.Content = new QuantityView();

// Chạy lại hiệu ứng cho nội dung hiện tại
host.Play();`,
                callout: {
                    type: "tip",
                    body: "Hiệu ứng <b>tự bỏ qua</b> khi control chưa load, khi <code>IsTransitionEnabled=\"False\"</code>, hoặc khi Windows tắt animation — nội dung vẫn đổi ngay, không bao giờ bị kẹt ở trạng thái trong suốt."
                }
            },
            {
                title: "Chọn kiểu chuyển cảnh theo hướng điều hướng",
                desc: "Thường người dùng đi tới thì trượt sang trái, quay lại thì trượt sang phải. Bind <code>Transition</code> vào hướng điều hướng để có cảm giác đúng chiều.",
                xaml: `<ui:PageTransitionHost Content="{Binding CurrentPage}"
                       Transition="{Binding NavigationTransition}"
                       IsTransitionEnabled="{Binding IsEffectsEnabled}" />`,
                csharp: `// Đi tới: nội dung mới vào từ phải. Quay lại: vào từ trái.
NavigationTransition = isForward ? PageTransition.SlideLeft : PageTransition.SlideRight;

// Tắt hiệu ứng trên máy yếu mà không phải gỡ control khỏi XAML
host.IsTransitionEnabled = false;

// Đang chạy hay không (để khoá nút Next trong lúc chuyển)
if (host.IsTransitioning) NextButton.IsEnabled = false;`,
                callout: {
                    type: "note",
                    body: "Host chỉ animate <b>nội dung mới đi vào</b>, không animate nội dung cũ đi ra — như vậy mỗi lần chuyển trang chỉ có một animation, và không cần giữ hai view cùng lúc."
                }
            }
        ],
        related: ["tabs", "cardcarousel", "navigationview"]
    },

    treedatagrid: {
        title: "TreeDataGrid — bảng có phân cấp",
        badge: "Mới",
        desc: "<code>DataGrid</code> hiển thị cây: bind <code>RootItems</code> và grid tự dẹt cây thành danh sách dòng, mỗi dòng là <b>chính item</b> chứ không phải wrapper — nên mọi cột vẫn bind thẳng vào thuộc tính của item.",
        keywords: ["tree grid", "phân cấp", "hierarchy", "datagrid", "expand", "cây"],
        sections: [
            {
                title: "Cây dữ liệu cơ bản",
                desc: "Cột đầu tiên do grid tự sinh: nó vẽ thụt lề và nút mở/đóng. Chỉ cần <code>HierarchyPath</code> là tên thuộc tính hiển thị — giống <code>DisplayMemberPath</code>. Cần định dạng phức tạp hơn thì dùng <code>HierarchyContentTemplate</code>.",
                xaml: `<ui:TreeDataGrid RootItems="{Binding Assemblies}"
                   ChildrenPath="Children"
                   HierarchyPath="Name"
                   IndentSize="18"
                   HierarchyHeader="Cấu kiện"
                   HierarchyColumnWidth="280">
    <DataGridTextColumn Header="Khối lượng" Binding="{Binding Volume, StringFormat=N2}" />
    <DataGridTextColumn Header="Mác" Binding="{Binding Grade}" />
</ui:TreeDataGrid>`,
                csharp: `var grid = new TreeDataGrid
{
    RootItems = assemblies,
    ChildrenPath = "Children",
    HierarchyPath = "Name",   // không đặt thì cell hiện ToString() của item
    IndentSize = 18,
    HierarchyColumnWidth = 280
};

// Nội dung phức tạp hơn một dòng chữ: icon, badge…
grid.HierarchyContentTemplate = (DataTemplate)FindResource("NodeCellTemplate");

// Mở rộng / thu gọn theo lập trình
grid.ExpandAll();
grid.ExpandTo(node);      // mở cả đường dẫn tới node rồi cuộn tới
grid.ToggleNode(node);
bool open = grid.IsNodeExpanded(node);`,
                callout: {
                    type: "note",
                    body: "Đặt cả <code>HierarchyPath</code> lẫn <code>HierarchyContentTemplate</code> thì template thắng. Trạng thái mở/đóng giữ theo <b>tham chiếu object</b>, nên hai node trùng giá trị vẫn là hai nhánh khác nhau và trạng thái sống sót qua mỗi lần dựng lại danh sách."
                }
            },
            {
                title: "Cột phát sinh và số dòng hiển thị",
                desc: "<code>ShowHierarchyColumn=\"False\"</code> để dùng grid như bảng phẳng; <code>VisibleItemCount</code> cho biết đang có bao nhiêu dòng nhìn thấy (chỉ nhánh đang mở), tiện cho dòng trạng thái.",
                xaml: `<TextBlock Text="{Binding ElementName=Tree, Path=VisibleItemCount, StringFormat='{}{0} dòng đang hiển thị'}" />`,
                csharp: `// Bảng phẳng: bỏ cột cây, giữ nguyên dữ liệu
grid.ShowHierarchyColumn = false;

// Đếm dòng đang thấy để hiện ở status bar
StatusText = grid.VisibleItemCount + " dòng";`,
                callout: {
                    type: "tip",
                    body: "Style <code>CONCOST.TreeDataGrid.Base</code> dựa trên <code>CONCOST.DataGrid.Base</code>, nên template, row style, grid lines và virtualization đều dùng chung — không có bản sao template nào để lệch nhau về sau."
                }
            }
        ],
        related: ["datagrid", "treeview", "multiselectdatagrid"]
    },

    multiselectdatagrid: {
        title: "MultiSelectDataGrid — chọn nhiều dòng bằng checkbox",
        badge: "Mới",
        desc: "DataGrid có cột checkbox và checkbox ba trạng thái ở header. Bấm vào ô checkbox là <b>đảo một dòng</b> chứ không xoá lựa chọn cũ, nên chọn hai mươi dòng không cần giữ Ctrl.",
        keywords: ["multi select", "checkbox", "chọn nhiều", "select all", "datagrid"],
        sections: [
            {
                title: "Chọn nhiều, lấy danh sách ra",
                desc: "<code>SelectedItemsList</code> là danh sách có phát thông báo (khác <code>SelectedItems</code> của DataGrid), nên bind command hoặc bật/tắt nút được ngay.",
                xaml: `<ui:MultiSelectDataGrid ItemsSource="{Binding Beams}"
                          ShowCheckBoxColumn="True"
                          CheckBoxColumnWidth="44" />`,
                csharp: `var grid = new MultiSelectDataGrid { ItemsSource = beams };

grid.SelectEverything();     // chọn hết
grid.InvertSelection();      // đảo lựa chọn
grid.ToggleItem(beam);       // đảo một dòng
grid.ClearEverything();

// Danh sách có phát thông báo để bind
DeleteCommand = new RelayCommand(
    () => Delete(grid.SelectedItemsList.Cast<Beam>().ToList()),
    () => grid.SelectedCount > 0);`,
                callout: {
                    type: "note",
                    body: "<code>IsAllSelected</code> là ba trạng thái: <code>true</code> khi chọn hết, <code>false</code> khi không chọn gì, <code>null</code> khi chọn một phần — header hiện ô gạch ngang đúng như bạn mong đợi."
                }
            },
            {
                title: "Click thường vẫn là click thường",
                desc: "Chỉ ô checkbox mới đảo lựa chọn. Bấm vào chỗ khác trong dòng thì DataGrid chạy như cũ: click thường chọn một dòng, Ctrl và Shift mở rộng.",
                xaml: `<ui:MultiSelectDataGrid ItemsSource="{Binding Beams}" ShowCheckBoxColumn="False" />`,
                csharp: `// Tắt cột checkbox nhưng giữ nguyên API chọn
grid.ShowCheckBoxColumn = false;
bool selected = grid.IsItemSelected(beam);`,
                callout: {
                    type: "tip",
                    body: "Checkbox trong ô có <code>IsHitTestVisible=\"False\"</code> — click rơi xuống dòng, đó chính là cơ chế khiến click thường đảo lựa chọn thay vì thay thế nó."
                }
            }
        ],
        related: ["datagrid", "treedatagrid", "checkbox"]
    },

    treemap: {
        title: "TreeMap — bản đồ diện tích",
        badge: "Mới",
        desc: "Diện tích mỗi ô tỉ lệ với giá trị, các ô được xếp <b>squarified</b> để gần vuông thay vì thành dải dài. Dùng cho cơ cấu khối lượng, phân bổ chi phí, mức tiêu thụ theo hạng mục.",
        keywords: ["treemap", "bản đồ cây", "diện tích", "squarified", "dashboard"],
        sections: [
            {
                title: "Cơ cấu khối lượng theo cấu kiện",
                desc: "<code>ValuePath</code> là trọng số, <code>LabelPath</code> là chữ hiển thị. Giá trị 0 hoặc âm bị bỏ — ô diện tích 0 chỉ tạo ra vệt không nhìn thấy.",
                xaml: `<ui:TreeMap Height="260"
           ItemsSource="{Binding Volumes}"
           ValuePath="Value"
           LabelPath="Label"
           ShowValues="True"
           ValueFormat="N1"
           Gap="4"
           CornerRadius="6">
    <ui:TreeMap.Palette>
        <SolidColorBrush Color="#FF7900" />
        <SolidColorBrush Color="#0284C7" />
        <SolidColorBrush Color="#16A34A" />
        <SolidColorBrush Color="#9333EA" />
    </ui:TreeMap.Palette>
</ui:TreeMap>`,
                csharp: `var map = new TreeMap
{
    ItemsSource = volumes,
    ValuePath = "Value",
    LabelPath = "Label",
    ShowValues = true,
    Gap = 4
};

// Ô nào vừa được bấm
map.ItemInvoked += (_, e) => Select(e.Entry.Item);

// Tự vẽ tooltip theo đúng hình chữ nhật đã vẽ
var entry = map.HitTest(mousePoint);`,
                callout: {
                    type: "note",
                    body: "<code>Entries</code> trả về đúng các hình chữ nhật đã vẽ (kèm <code>Bounds</code> và <code>Share</code>), nên hit-test, tooltip và nhãn của bạn luôn khớp với hình — không phải tính lại layout lần thứ hai."
                }
            },
            {
                title: "Một màu, hoặc không nhãn",
                desc: "Không đặt <code>Palette</code> thì mọi ô dùng <code>Fill</code> (mặc định là màu accent của theme). Tắt nhãn khi ô quá nhỏ để khỏi rối.",
                xaml: `<ui:TreeMap ItemsSource="{Binding Cost}" ShowLabels="False" Fill="{DynamicResource CONCOST.Brush.PrimaryTint}" />`,
                csharp: `// Cùng một màu cho mọi ô
map.Fill = (Brush)FindResource("CONCOST.Brush.Primary");

// Tắt chữ khi chỉ cần hình khối
map.ShowLabels = false;`,
                callout: {
                    type: "tip",
                    body: "Bố cục tính lại khi đổi kích thước. Với dữ liệu rất nhỏ, đặt <code>Gap=\"0\"</code> để không mất diện tích vào khoảng hở."
                }
            }
        ],
        related: ["charts", "statisticcard", "sparkline"]
    },

    dockingmanager: {
        title: "DockingManager — layout kiểu Visual Studio",
        badge: "Mới",
        desc: "Panel quản lý các <code>ToolWindow</code> ở bốn cạnh và vùng giữa. Cùng một cạnh thì các panel trở thành <b>tab của một nhóm</b>, kích thước co giãn theo tỉ lệ khi đổi cửa sổ, kéo header để dock sang cạnh khác với 5 vùng thả, và lưu/khôi phục được layout.",
        keywords: ["docking", "tool window", "auto hide", "splitter", "layout", "tab", "visual studio"],
        sections: [
            {
                title: "Nhóm pane và tab",
                desc: "Mọi panel khai <code>Dock=\"Left\"</code> thuộc <b>một nhóm</b> ở cạnh trái. Từ hai panel trở lên, nhóm hiện thành tab ở đáy — không phải mỗi panel chiếm một dải riêng. Tab đang chọn là <code>IsActive</code>, và header riêng của panel tự ẩn đi vì tab đã ghi tên nó.",
                xaml: `<ui:DockingManager>
    <ui:ToolWindow Header="Cấu kiện" Icon="Layers" Dock="Left" DockSize="210" />
    <ui:ToolWindow Header="Lớp" Icon="LayerColor" Dock="Left" DockSize="210" />

    <ui:ToolWindow Header="Thuộc tính" Dock="Right" DockSize="230" />
    <ui:ToolWindow Header="Nhật ký" Dock="Bottom" DockSize="120" />
    <ui:ToolWindow Header="Bản vẽ" Dock="Center" />
</ui:DockingManager>`,
                csharp: `// Hai panel cùng cạnh = hai tab của một nhóm
manager.Children.Add(new ToolWindow { Header = "Cấu kiện", Dock = DockSide.Left, DockSize = 210 });
manager.Children.Add(new ToolWindow { Header = "Lớp", Dock = DockSide.Left, DockSize = 210 });

// Chọn tab bằng code, đi đúng đường của cú click
layers.IsActive = true;

// Nhóm hiện tại của một cạnh
IReadOnlyList<ToolWindow> left = manager.Group(DockSide.Left);`,
                callout: {
                    type: "tip",
                    body: "Kích thước trong một nhóm được đồng bộ: kéo splitter hay đổi <code>DockSize</code> của panel này thì các tab còn lại nhận cùng kích thước, nên chuyển tab không làm cạnh bị giật."
                }
            },
            {
                title: "Kích thước theo tỉ lệ",
                desc: "Mặc định panel giữ <b>tỉ lệ</b> của nó khi cửa sổ đổi kích thước — như cột <code>*</code> trong <code>Grid</code>. Đặt <code>IsProportional=\"False\"</code> nếu muốn ghim cứng pixel.",
                xaml: `<ui:DockingManager IsProportional="True" SplitterThickness="6" />`,
                csharp: `// 210px trên cửa sổ 1174px → thu còn 814px thì panel còn ~146px
manager.IsProportional = false;   // tắt: giữ nguyên 210px`,
                callout: {
                    type: "note",
                    body: "Kéo splitter cập nhật <code>DockSize</code> của cả nhóm; lần đổi kích thước cửa sổ sau đó sẽ lấy tỉ lệ mới làm chuẩn."
                }
            },
            {
                title: "Kéo để dock lại — 5 vùng thả",
                desc: "Giữ và kéo header: vùng đang trỏ tới được tô sáng bằng khung nét đứt. Vùng thả tính theo <b>từng panel</b> (25% mỗi cạnh, còn lại là giữa), nên kéo vào nửa trái của vùng giữa là dock sang trái, không phải theo dải biên của cả cửa sổ.",
                xaml: `<ui:ToolWindow Header="Nhật ký" Dock="Bottom"
               CanAutoHide="True" CanClose="True" />`,
                csharp: `// Dock theo code (menu View)
manager.Dock(pane, DockSide.Right);

// Tự ẩn / flyout
pane.ToggleAutoHide();
manager.ToggleFlyout(pane);
manager.CloseFlyouts();

// Sự kiện để lưu layout hoặc ghi log
manager.PaneDocked += (_, w) => SaveLayout();
manager.PaneClosed += (_, w) => Log(w.Header);`,
                callout: {
                    type: "tip",
                    body: "Khung nét đứt vẽ đúng vùng panel sẽ chiếm sau khi thả, không phải nửa panel đang trỏ — gợi ý không bao giờ nói dối."
                }
            },
            {
                title: "Nhóm lồng nhau — layout là một cây",
                desc: "<code>DockPaneGroup</code> chia vùng của nó cho các con theo <code>Proportion</code>, và các con có thể là nhóm khác. Nhờ vậy biểu diễn được “cạnh trái chia hai hàng, hàng trên lại có tab” — thứ mà bốn slot cố định không làm được.",
                xaml: `<ui:DockPaneGroup Dock="Left" DockSize="250" Orientation="Vertical">
    <!-- Hàng trên: hai tab -->
    <ui:DockPaneGroup Mode="Tabs" ui:DockPaneGroup.Proportion="2">
        <ui:ToolWindow Header="Cấu kiện" Icon="Layers" />
        <ui:ToolWindow Header="Lớp" Icon="LayerColor" />
    </ui:DockPaneGroup>

    <!-- Hàng dưới: một panel -->
    <ui:ToolWindow Header="Tìm kiếm" Icon="Search" ui:DockPaneGroup.Proportion="1" />
</ui:DockPaneGroup>`,
                csharp: `var tabs = new DockPaneGroup { Mode = DockGroupMode.Tabs };
tabs.Children.Add(tree);
tabs.Children.Add(layers);
DockPaneGroup.SetProportion(tabs, 2);

var split = new DockPaneGroup
{
    Dock = DockSide.Left,
    DockSize = 250,
    Orientation = Orientation.Vertical,   // các con xếp dọc
    Mode = DockGroupMode.Split
};
split.Children.Add(tabs);
split.Children.Add(search);

manager.Children.Add(split);`,
                callout: {
                    type: "tip",
                    body: "<code>Mode=\"Tabs\"</code> biến nhóm thành tab (chỉ con đang <code>IsActive</code> hiện); <code>Mode=\"Split\"</code> chia chỗ. Mỗi nhóm tự vẽ và tự kéo splitter của nó, nên lồng bao nhiêu tầng cũng được."
                }
            },
            {
                title: "Cửa sổ nổi",
                desc: "Kéo header ra **ngoài** vùng docking là panel tách thành cửa sổ riêng. Gọi <code>Float(pane)</code> nếu muốn làm bằng code; đóng cửa sổ đó thì panel tự dock lại đúng cạnh cũ, đúng kích thước cũ.",
                xaml: `<!-- Không cần XAML -->`,
                csharp: `Window floating = manager.Float(props);

// Đóng cửa sổ → tự dock lại cạnh cũ
floating.Closed += (_, _) => SaveLayout();`,
                callout: {
                    type: "note",
                    body: "Panel đang nổi bị tắt <code>CanAutoHide</code> (vì không còn cạnh nào để ghim vào), và được bật lại khi dock về."
                }
            },
            {
                title: "Kéo tab",
                desc: "Giữ và kéo một tab: thả lại trên chính dải tab thì nó **đổi thứ tự**, thả sang cạnh khác thì panel chuyển sang nhóm đó, thả ra ngoài cửa sổ thì tách thành cửa sổ nổi. Ba việc đó cũng gọi được bằng code.",
                xaml: `<!-- Không cần XAML: tab tự kéo được -->`,
                csharp: `// Đổi thứ tự tab (0 = đầu)
manager.MovePane(pane, 0);
group.MoveChild(pane, 1);      // trong một nhóm lồng

// Thả ở một toạ độ bất kỳ — dock hoặc tách
manager.DockAt(pane, pointInManager);

// Tự vẽ gợi ý thả khi làm drag riêng
manager.ShowDropHint(pane, point);
manager.EndDropPreview();`,
                callout: {
                    type: "tip",
                    body: "<code>DockAt</code> nhận toạ độ trong hệ của manager: điểm nằm ngoài biên thì tách cửa sổ, nằm trong thì dock theo vùng thả. Nhờ vậy một dải tab tự dựng cũng dùng lại được toàn bộ gesture."
                }
            },
            {
                title: "Lưu và khôi phục layout",
                desc: "Mỗi panel được lưu cạnh, kích thước, trạng thái tự ẩn và tab đang chọn. Khôi phục khớp theo <code>Header</code>, nên panel lạ trong file cũ bị bỏ qua thay vì làm hỏng layout.",
                xaml: `<!-- Không cần XAML: gọi từ code khi đóng/mở ứng dụng -->`,
                csharp: `string snapshot = manager.CaptureLayout();
File.WriteAllText("layout.txt", snapshot);

// Lần chạy sau
manager.ApplyLayout(File.ReadAllText("layout.txt"));

// Trả về false nếu không khớp panel nào
bool ok = manager.ApplyLayout(snapshot);`,
                callout: {
                    type: "note",
                    body: "Định dạng là văn bản thuần có dòng tiêu đề phiên bản (<code>CONCOST-DOCK-1</code>), nên đọc được bằng mắt và tiến hoá được mà không cần thư viện JSON."
                }
            }
        ],
        related: ["tabs", "documenttabcontrol", "expander"]
    },

    searchabletreepicker: {
        title: "SearchableTreePicker — cây có tìm kiếm",
        badge: "Mới",
        desc: "Ô tìm kiếm đặt trước một cây: gõ để thu hẹp cây xuống các mục khớp <b>và những nhánh dẫn tới chúng</b>, rồi chọn một hoặc nhiều mục.",
        keywords: ["tree picker", "tìm kiếm cây", "chọn nhiều", "search", "treeview"],
        sections: [
            {
                title: "Lọc mà vẫn thấy đường dẫn",
                desc: "Bộ lọc giữ lại nhánh cha của mỗi kết quả. Bỏ hết node không khớp sẽ chỉ còn một danh sách lá trơ trọi, không biết nó nằm ở đâu — giữ đường dẫn là thứ làm kết quả đọc được.",
                xaml: `<ui:SearchableTreePicker ItemsSource="{Binding Nodes}"
                         ChildrenPath="Children"
                         DisplayPath="Name"
                         SelectionMode="Multiple"
                         Placeholder="Tìm cấu kiện..." />`,
                csharp: `var picker = new SearchableTreePicker
{
    ItemsSource = nodes,
    ChildrenPath = "Children",
    DisplayPath = "Name",        // đường dẫn có chấm cũng được: "Info.Label"
    SelectionMode = PickerSelectionMode.Multiple
};

picker.SelectionChanged += (_, _) => Show(picker.SelectedItems);`,
                callout: {
                    type: "tip",
                    body: "So khớp đi qua <code>CommandPaletteText.Normalize</code>: gõ <code>dam d1</code> tìm ra “Dầm D1”, <code>tang 1</code> tìm ra “Tầng 1”. Với thư viện tiếng Việt thì đây là khác biệt giữa dùng được và không."
                }
            },
            {
                title: "Chọn một hay nhiều",
                desc: "<code>SelectionMode=\"Multiple\"</code> hiện checkbox từng dòng. <code>SelectAllVisible()</code> chỉ lấy những gì đang hiện — nghĩa là sau khi lọc, “chọn tất cả” đúng nghĩa là chọn hết kết quả.",
                xaml: `<ui:SearchableTreePicker SelectionMode="Single" />`,
                csharp: `picker.SearchText = "cột";
picker.SelectAllVisible();      // chỉ các mục còn hiện sau khi lọc
picker.ClearSelection();

int n = picker.SelectedCount;
IReadOnlyList<object> picked = picker.SelectedItems;`,
                callout: {
                    type: "note",
                    body: "Đổi từ Multiple sang Single thì danh sách chọn được rút còn mục cuối, không để lại trạng thái không hợp lệ."
                }
            }
        ],
        related: ["treedatagrid", "commandpalette", "searchbox"]
    },

    meshgradient: {
        title: "Mesh gradient & đèn rọi",
        badge: "Mới",
        desc: "Hai hiệu ứng nền: <code>AnimatedMeshGradient</code> cho bốn khối màu “thở” quanh neo cố định, và <code>CursorSpotlight</code> làm tối mọi thứ trừ một vòng quanh con trỏ.",
        keywords: ["mesh gradient", "gradient động", "spotlight", "đèn rọi", "nền"],
        sections: [
            {
                title: "Mesh, không phải aurora",
                desc: "Khác <code>AuroraBackground</code>: aurora trôi các khối ngang mặt phẳng nên đọc như thời tiết; mesh giữ khối quanh neo và chỉ dao động nhẹ, nên trông như một ảnh tĩnh biết cử động. Bán kính tính theo <b>cạnh dài</b> — tính theo cạnh ngắn thì panel rộng sẽ hở nền giữa các khối.",
                xaml: `<controls:AnimatedMeshGradient Height="240"
    ColorA="#7F77DD" ColorB="#1D9E75"
    ColorC="#EF9F27" ColorD="#D4537E"
    Speed="1" BlobOpacity="0.55" Spread="0.75" />`,
                csharp: "mesh.Restart();          // quay lại frame đầu\n" +
                        "mesh.IsAnimated = false; // đứng yên, vẫn vẽ",
                callout: {
                    type: "note",
                    body: "Vẽ bằng radial gradient trong suốt ở viền, không dùng <code>BlurEffect</code> — rẻ hơn nhiều khi phủ cả cửa sổ."
                }
            },
            {
                title: "Đèn rọi không cướp click",
                desc: "<code>CursorSpotlight</code> không nhận hit-test, nhưng vẫn theo được con trỏ vì <code>Mouse.GetPosition</code> đọc vị trí hiện tại chứ không cần nhận sự kiện. Đặt <code>FollowCursor=\"False\"</code> rồi tự đặt <code>SpotlightCenter</code> nếu muốn điều khiển theo kịch bản tour.",
                xaml: `<Grid>
    <StackPanel>...</StackPanel>
    <controls:CursorSpotlight Radius="130" SpotDim="0.62" />
</Grid>`,
                csharp: "spotlight.FollowCursor = false;\n" +
                        "spotlight.SpotlightCenter = new Point(120, 80);\n\n" +
                        "spotlight.SpotColor = Color.FromArgb(0x22, 0xFF, 0xFF, 0xFF);",
                callout: {
                    type: "tip",
                    body: "Vẽ bằng <b>một</b> hình chữ nhật phủ toàn khung với gradient tuyệt đối và <code>SpreadMethod=Pad</code>. Chỉ vẽ hình ellipse sẽ ra vòng tối ở giữa — đúng ngược lại điều cần."
                }
            }
        ],
        related: ["aurorabackground", "spotlightcard", "cursorglow"]
    },

    glasscard: {
        title: "Kính mờ & viền gradient",
        badge: "Mới",
        desc: "Hai bề mặt: <code>FrostedGlass</code> làm mờ đúng vùng nằm sau nó, và <code>AnimatedGradientBorder</code> cho viền gradient xoay quanh thẻ.",
        keywords: ["frosted", "kính mờ", "glass", "gradient border", "viền động"],
        sections: [
            {
                title: "Kính mờ không cần backdrop filter",
                desc: "WPF không có backdrop filter, nên blur phải tự dựng: trỏ <code>Source</code> vào phần tử phía sau, control vẽ <code>VisualBrush</code> của nó qua <code>BlurEffect</code>. Viewbox của brush được đặt theo <b>toạ độ tấm kính trong Source</b>, nên blur khớp đúng vùng nằm sau thay vì kéo giãn cả nền vào tấm kính.",
                xaml: `<Grid>
    <controls:AnimatedMeshGradient x:Name="Backdrop" />
    <controls:FrostedGlass Source="{Binding ElementName=Backdrop}"
        BlurRadius="22" Corner="16"
        Tint="#66FFFFFF" />
</Grid>`,
                csharp: "glass.Source = backdrop;\n" +
                        "glass.BlurRadius = 24;\n" +
                        "glass.Tint = new SolidColorBrush(Color.FromArgb(0x99, 0xFF, 0xFF, 0xFF));",
                callout: {
                    type: "warning",
                    body: "Bản sao đó được vẽ lại mỗi khi Source đổi, nên đây là control cho <b>vài tấm kính trên một hero</b> — không phải nền cho danh sách cuộn dài."
                }
            },
            {
                title: "Viền xoay bằng animation, không bằng OnRender",
                desc: "<code>AnimatedGradientBorder</code> xoay một <code>RotateTransform</code> gắn vào brush bằng <code>DoubleAnimation</code>. WPF chạy nó ở render thread, nên một chục thẻ như vậy không tốn gì trên UI thread và vẫn mượt khi cửa sổ bận.",
                xaml: `<controls:AnimatedGradientBorder SecondsPerTurn="8"
    RingThickness="2" Corner="14">
    <StackPanel>...</StackPanel>
</controls:AnimatedGradientBorder>`,
                csharp: "card.IsAnimated = false;   // dừng ở frame đầu, thẻ vẫn hiện",
                callout: {
                    type: "note",
                    body: "Khi <code>MotionSettings.AnimationsEnabled</code> tắt, animation bị xoá và viền giữ frame đầu — người dùng giảm chuyển động thấy đúng chiếc thẻ đó, không phải một khung trống."
                }
            }
        ],
        related: ["card", "meshgradient", "borderbeam"]
    },

    motion: {
        title: "Stagger & Shared element",
        badge: "Mới",
        desc: "Hai kiểu chuyển động: danh sách hiện dần từng mục, và một phần tử “lớn ra” từ chỗ nó từng nằm.",
        keywords: ["stagger", "shared element", "animation", "chuyển cảnh", "flip"],
        sections: [
            {
                title: "Danh sách hiện dần",
                desc: "<code>StaggerAnimation</code> là attached behaviour: độ trễ suy ra từ vị trí mục nên dùng được với mọi panel và mọi item template. Container được đánh dấu theo <b>tham chiếu</b> — panel ảo hoá dùng lại container, nếu không đánh dấu thì cuộn qua lại mục nào cũng chạy lại hiệu ứng.",
                xaml: `<ListBox controls:StaggerAnimation.IsEnabled="True"
         controls:StaggerAnimation.Delay="55"
         controls:StaggerAnimation.OffsetY="16"
         controls:StaggerAnimation.MaxItems="24" />`,
                csharp: "StaggerAnimation.SetDelay(list, 70);\n" +
                        "StaggerAnimation.SetMaxItems(list, 12);   // chặn trần độ trễ",
                callout: {
                    type: "tip",
                    body: "<code>MaxItems</code> chặn độ trễ tăng mãi: danh sách 200 dòng sẽ không mất mười giây mới hiện xong."
                }
            },
            {
                title: "Shared element",
                desc: "<code>SharedElementTransition</code> không animate layout — phần tử đã nằm đúng chỗ cuối, chỉ bị scale/dịch về hình chữ nhật cũ ở frame đầu rồi về identity. Nhờ vậy không có layout thrash và phần tử vẫn tương tác được trong suốt quá trình.",
                xaml: `<!-- Không cần XAML: gọi khi đổi view -->`,
                csharp: "SharedElementTransition.RunFrom(target, source);\n\n" +
                        "// hoặc tự tính rect trong hệ của cha\n" +
                        "var parent = (UIElement)VisualTreeHelper.GetParent(target);\n" +
                        "var origin = source.TranslatePoint(new Point(0, 0), parent);\n" +
                        "SharedElementTransition.Run(target, new Rect(origin, source.RenderSize));",
                callout: {
                    type: "note",
                    body: "Cả hai rect phải ở cùng hệ toạ độ — thường là cha của phần tử. Kết thúc thì <code>RenderTransform</code> được trả về như cũ để không phá hit-test."
                }
            }
        ],
        related: ["pagetransitionhost", "scrollreveal", "glasscard"]
    },

    dragdrop: {
        title: "Drop zone, viền gợi ý & ghost",
        badge: "Mới",
        desc: "Ba mảnh của một thao tác kéo thả: <code>FileDropZone</code> nhận tệp, <code>DropTargetHighlight</code> vẽ viền gợi ý quanh bất kỳ phần tử nào, và <code>DragGhost</code> mang nhãn theo con trỏ.",
        keywords: ["drag drop", "file drop", "kéo thả", "ghost", "drop target"],
        sections: [
            {
                title: "Vùng thả tệp",
                desc: "<code>Filter</code> được kiểm tra <b>ngay khi tệp còn đang bay</b>, nên vùng chỉ sáng lên với tệp nó sẽ nhận. Nhận mọi drag rồi từ chối lúc thả là kiểu hành vi làm người dùng thấy hỏng.",
                xaml: `<ui:FileDropZone Filter="*.dwg;*.dxf"
                 Caption="Kéo tệp bản vẽ vào đây"
                 Hint="hoặc bấm để chọn tệp" />`,
                csharp: `zone.FilesDropped += (_, e) => Load(e.Files);

// Bấm vào vùng thì host tự mở hộp chọn tệp
zone.BrowseRequested += (_, _) => OpenDialog();
zone.AddFiles(picked);          // cùng bộ lọc, cùng sự kiện

bool ok = zone.Matches(@"C://ban ve//cot.dwg");`,
                callout: {
                    type: "tip",
                    body: "Thư viện không tự mở hộp chọn tệp: hiện picker nào, và host có muốn hay không, không phải quyết định của control."
                }
            },
            {
                title: "Viền gợi ý cho mọi phần tử",
                desc: "<code>DropTargetHighlight</code> là attached property, vẽ viền nét đứt bằng <b>adorner</b> nên nằm trên nội dung và không đổi layout — không thêm border, không xô padding. Nó chỉ báo “đang có drag ở trên”; việc nhận hay không vẫn do phần tử quyết định qua <code>DragDropEffects</code>.",
                xaml: `<Border controls:DropTargetHighlight.IsEnabled="True"
        controls:DropTargetHighlight.CornerRadius="10" />`,
                csharp: `DropTargetHighlight.SetIsEnabled(card, true);
DropTargetHighlight.SetBrush(card, Brushes.OrangeRed);`,
                callout: {
                    type: "note",
                    body: "Tách hai việc đó ra là thứ cho phép tô sáng cả một thẻ trong khi chỉ một dòng bên trong nhận dữ liệu."
                }
            },
            {
                title: "Ghost theo con trỏ",
                desc: "<code>DragGhost</code> là một cửa sổ trong suốt, luôn trên cùng, không nhận chuột, đặt lệch khỏi con trỏ 14px. Cả tiến trình chỉ có một ghost — người dùng không kéo hai thứ một lúc.",
                xaml: `<!-- Không cần XAML -->`,
                csharp: `DragGhost.Show("Dầm D1 (300×600)", IconKind.Layers);
DragGhost.Move(this, e.GetPosition(this));   // tự quy đổi DPI
DragGhost.Hide();`,
                callout: {
                    type: "tip",
                    body: "Truyền vào <code>Move</code> chính visual mà con trỏ đang thuộc về; phép chia theo DPI được làm bên trong nên ghost rơi đúng chỗ con trỏ."
                }
            }
        ],
        related: ["filedropzone", "dragdrophelper", "searchabletreepicker"]
    },

    documenttabcontrol: {
        title: "DocumentTabControl — tab tài liệu",
        badge: "Mới",
        desc: "TabControl cho tài liệu: mỗi tab có icon, chấm đánh dấu chưa lưu và nút đóng. Đóng tab là một <b>yêu cầu</b> — host có thể chặn để hỏi lưu trước.",
        keywords: ["document tab", "tab", "đóng tab", "dirty", "unsaved", "editor"],
        sections: [
            {
                title: "Tab có nút đóng và chấm chưa lưu",
                desc: "<code>IsDirty</code> chỉ hiện chấm — control không bao giờ tự xoá nó, vì việc lưu là của host. <code>CanClose=\"False\"</code> ẩn nút đóng của riêng tab đó.",
                xaml: `<ui:DocumentTabControl ItemsSource="{Binding Documents}" ShowCloseButtons="True">
    <ui:DocumentTabControl.ItemTemplate>
        <DataTemplate>
            <ui:DocumentTabItem Header="{Binding Title}"
                                Icon="File"
                                IsDirty="{Binding IsDirty}"
                                CloseCommand="{Binding DataContext.CloseDocumentCommand,
                                               RelativeSource={RelativeSource AncestorType=ui:DocumentTabControl}}"
                                Content="{Binding}" />
        </DataTemplate>
    </ui:DocumentTabControl.ItemTemplate>
</ui:DocumentTabControl>`,
                csharp: `// Đóng theo lập trình, đi đúng đường của nút đóng
tabs.CloseTab(document);
tabs.CloseOtherTabs(document);
tabs.CloseAllTabs();

// Tab không có CloseCommand thì bị gỡ khỏi Items;
// có CloseCommand thì command chạy và control không tự gỡ.`,
                callout: {
                    type: "note",
                    body: "<code>Close()</code> phát routed event <code>CloseRequested</code> (bubble) — <code>DocumentTabControl</code> bắt bằng class handler. Nhờ vậy nút đóng, menu ngữ cảnh và phím tắt đều đi qua cùng một chỗ."
                }
            },
            {
                title: "Tắt nút đóng toàn bộ",
                desc: "Khi cửa sổ chỉ hiển thị một tài liệu, tắt hết nút đóng cho gọn — hoặc tắt theo từng tab.",
                xaml: `<ui:DocumentTabControl ShowCloseButtons="False" />
<ui:DocumentTabItem Header="Bản vẽ A" CanClose="False" />`,
                csharp: `tabs.ShowCloseButtons = false;

// Một tab cụ thể không được đóng
var locked = new DocumentTabItem { Header = "Mẫu", CanClose = false };
tabs.Items.Add(locked);`,
                callout: {
                    type: "tip",
                    body: "Style <code>CONCOST.DocumentTabControl.Base</code> kế thừa <code>CONCOST.TabControl.Base</code> nhưng thay <code>ItemContainerStyle</code> bằng style document — nếu không, tab sẽ rơi về template TabItem thường và mất nút đóng."
                }
            }
        ],
        related: ["tabs", "dockingmanager", "tabcontrol"]
    },

    statusbar: {
        title: "StatusBar — thanh trạng thái",
        badge: "Mới",
        desc: "Dải thông tin dưới đáy cửa sổ. Mỗi ô là một <code>StatusBarItem</code>; ô khai <code>Alignment=\"Right\"</code> bám mép phải, phần giữa co giãn theo cửa sổ.",
        keywords: ["status bar", "trạng thái", "bottom", "footer", "shell"],
        sections: [
            {
                title: "Hai phía, một khoảng trống ở giữa",
                desc: "<code>StatusBarPanel</code> xếp ô trái từ mép trái và ô phải từ mép phải, nên không cần <code>Grid</code> với cột <code>*</code> để đẩy hai nhóm ra hai đầu.",
                xaml: `<ui:StatusBar>
    <ui:StatusBarItem Text="Sẵn sàng" Icon="Check" />
    <ui:StatusBarItem Text="12 cấu kiện" IsSeparator="True" />
    <ui:StatusBarItem Text="Đã lưu" Icon="Save" />

    <ui:StatusBarItem Text="100%" Alignment="Right" />
    <ui:StatusBarItem Text="mm" Alignment="Right" IsSeparator="True" />
</ui:StatusBar>`,
                csharp: `var bar = new StatusBar { Spacing = 12 };

bar.Items.Add(new StatusBarItem { Text = "Sẵn sàng", Icon = IconKind.Check });
bar.Items.Add(new StatusBarItem { Text = "100%", Alignment = StatusBarAlignment.Right });`,
                callout: {
                    type: "tip",
                    body: "Thứ tự khai báo quyết định thứ tự trong từng nhóm, không phải vị trí trên màn hình — một ô <code>Right</code> chen giữa các ô <code>Left</code> vẫn nằm bên phải."
                }
            },
            {
                title: "Ô bấm được",
                desc: "Gắn <code>Command</code> cho ô để biến nó thành nút — ví dụ mở bảng log hoặc đổi đơn vị. Ô không có command thì con trỏ để mặc định, không giả vờ bấm được.",
                xaml: `<ui:StatusBarItem Text="3 cảnh báo" Icon="Alert"
                   Command="{Binding ShowWarningsCommand}" />`,
                csharp: `var warnings = new StatusBarItem
{
    Text = "3 cảnh báo",
    Icon = IconKind.Alert,
    Command = new RelayCommand(() => ShowWarnings())
};`,
                callout: {
                    type: "note",
                    body: "<code>Content</code> được ưu tiên hơn <code>Text</code>: đặt một control vào ô (progress bar, combo đơn vị) thì bỏ <code>Text</code> đi, đừng đặt cả hai."
                }
            }
        ],
        related: ["ribbon", "commandbar", "concostwindow"]
    },

    commandbar: {
        title: "CommandBar — thanh lệnh có menu “thêm”",
        badge: "Mới",
        desc: "Thanh công cụ một dòng: nút có icon + nhãn, và khi hết chỗ thì phần thừa rơi vào menu xổ xuống thay vì bị cắt cụt.",
        keywords: ["command bar", "toolbar", "overflow", "menu", "shell"],
        sections: [
            {
                title: "Khai báo lệnh bằng dữ liệu",
                desc: "Mỗi mục là một <code>CommandBarItem</code>. Control không tự quyết định cái gì vừa — <code>OverflowPanel</code> đo trong lượt layout rồi báo lại, nên <code>OverflowItems</code> luôn khớp với thứ đang bị giấu.",
                xaml: `<ui:CommandBar ItemsSource="{Binding Commands}" OverflowToolTip="Thêm lệnh" />`,
                csharp: `bar.ItemsSource = new[]
{
    new CommandBarItem("Xuất PDF", ExportPdfCommand, IconKind.File),
    new CommandBarItem("Xuất Excel", ExportExcelCommand, IconKind.Save),
    new CommandBarItem("In", PrintCommand, IconKind.Print),
    new CommandBarItem { IsSeparator = true },
    new CommandBarItem("Trợ giúp", HelpCommand, IconKind.Help, "Mở tài liệu")
};`,
                callout: {
                    type: "tip",
                    body: "<code>ShowLabel=\"False\"</code> giữ icon và bỏ nhãn — dùng cho nhóm lệnh phụ để thanh gọn lại trước khi phải overflow."
                }
            },
            {
                title: "Menu thừa và trạng thái",
                desc: "<code>HasOverflow</code> và <code>OverflowItems</code> là read-only, tự cập nhật sau mỗi lượt layout. <code>IsOverflowOpen</code> hai chiều, bind được để đóng menu khi chuyển tab.",
                xaml: `<ui:CommandBar ItemsSource="{Binding Commands}"
               IsOverflowOpen="{Binding IsMoreOpen, Mode=TwoWay}" />`,
                csharp: `if (bar.HasOverflow) status.Text = $"{bar.OverflowItems.Cast<object>().Count()} lệnh bị ẩn";

// Chạy lệnh bằng code, đi đúng đường của nút bấm
bar.Invoke(bar.ItemsSource.First());`,
                callout: {
                    type: "note",
                    body: "Khi không còn mục nào bị ẩn, control tự đặt <code>IsOverflowOpen = false</code> — menu không bao giờ mở rỗng."
                }
            }
        ],
        related: ["ribbon", "quickaccesstoolbar", "statusbar"]
    },

    quickaccesstoolbar: {
        title: "QuickAccessToolbar — dải icon trên cùng",
        badge: "Mới",
        desc: "Bản rời của thanh truy cập nhanh trong ribbon: chỉ icon, cao 28px, đặt được ở bất kỳ cửa sổ nào không cần cả ribbon.",
        keywords: ["quick access", "qat", "toolbar", "icon", "shell"],
        sections: [
            {
                title: "Icon, tooltip, hết",
                desc: "Mỗi mục là một <code>QuickAccessItem</code> — icon, tooltip, command. Không nhãn, không nhóm: đây là chỗ cho những lệnh dùng hàng ngày.",
                xaml: `<ui:QuickAccessToolbar ItemsSource="{Binding QuickCommands}" IsCustomizable="False" />`,
                csharp: `qat.ItemsSource = new[]
{
    new QuickAccessItem(IconKind.Save, "Lưu (Ctrl+S)", SaveCommand),
    new QuickAccessItem(IconKind.Undo, "Hoàn tác", UndoCommand),
    new QuickAccessItem { IsSeparator = true },
    new QuickAccessItem(IconKind.Print, "In", PrintCommand)
};`,
                callout: {
                    type: "tip",
                    body: "<code>IsCustomizable=\"False\"</code> ẩn nút mũi tên khi không có gì bị tràn — hợp với cửa sổ con chỉ có ba bốn lệnh cố định."
                }
            },
            {
                title: "Tràn xuống menu",
                desc: "Cùng cơ chế với <code>CommandBar</code>: hết chỗ thì mục thừa rơi vào popup, không bị cắt.",
                xaml: `<ui:QuickAccessToolbar ItemsSource="{Binding QuickCommands}"
                        IsOverflowOpen="{Binding IsQatMoreOpen, Mode=TwoWay}" />`,
                csharp: `// Lưu bố cục người dùng tự sắp
var visible = qat.ItemsSource.Cast<QuickAccessItem>()
                  .Take(qat.ItemsSource.Cast<QuickAccessItem>().Count() - qat.OverflowItems.Cast<object>().Count());`,
                callout: {
                    type: "note",
                    body: "Đây là control độc lập, không phải con của <code>Ribbon</code>. Ribbon vẫn dùng <code>RibbonQuickAccessToolBar</code> nội bộ của nó — hai thứ cùng ý tưởng, khác phạm vi."
                }
            }
        ],
        related: ["commandbar", "ribbon", "ribboncommands"]
    },

};
