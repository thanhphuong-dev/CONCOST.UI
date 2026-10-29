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
    }

};
