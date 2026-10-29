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
    }
};
