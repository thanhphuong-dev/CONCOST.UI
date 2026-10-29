// Cookbook page — complete, runnable examples that combine several controls.
const DOCS_EXAMPLES = {
    examples: {
        title: "Ví dụ sử dụng",
        badge: "Cookbook",
        desc: "Bốn tình huống thực tế ghép nhiều control lại với nhau: form nhập thông số dầm, màn hình bảng khối lượng, panel cấu hình trong plugin Revit và dashboard KPI có trạng thái loading. Mỗi ví dụ đều có XAML và C# chạy được.",
        keywords: ["example", "sample", "cookbook", "ví dụ", "mẫu", "demo"],
        sections: [
            {
                title: "1. Form nhập thông số dầm",
                desc: "Ghép <code>UnitBox</code>, <code>NumericUpDown</code>, <code>RebarInput</code>, <code>CheckBox</code> và <code>SplitButton</code> thành một form gọn, có đơn vị và validate sẵn.",
                callout: {
                    type: "tip",
                    body: "<code>UnitBox</code> và <code>RebarInput</code> không có thuộc tính <code>Header</code> — dùng <code>TextBlock</code> với style <code>CONCOST.TextBlock.Caption</code> làm nhãn, hoặc <code>attached:InfoElement.Title</code> nếu bạn tự viết template."
                },
                xaml: `<ui:Card Width="380" Padding="20">
    <StackPanel>
        <TextBlock Text="Beam B1 — section &amp; rebar"
                   Style="{DynamicResource CONCOST.TextBlock.H4}" Margin="0,0,0,14" />

        <ui:UnitBox Header="Width (b)" Value="{Binding WidthMm, Mode=TwoWay}"
                    Unit="mm" DecimalPlaces="0" Margin="0,0,0,10" />

        <ui:UnitBox Header="Depth (h)" Value="{Binding DepthMm, Mode=TwoWay}"
                    Unit="mm" DecimalPlaces="0" Margin="0,0,0,10" />

        <TextBlock Text="Span" Style="{DynamicResource CONCOST.TextBlock.Caption}" />
        <ui:NumericUpDown Value="{Binding SpanM, Mode=TwoWay}"
                          Minimum="1" Maximum="24" Increment="0.5"
                          DecimalPlaces="2" Suffix=" m" Margin="0,2,0,10" />

        <TextBlock Text="Main rebar" Style="{DynamicResource CONCOST.TextBlock.Caption}" />
        <eng:RebarInput Text="{Binding RebarText, Mode=TwoWay}" Margin="0,2,0,12" />

        <CheckBox Content="Include self weight"
                  IsChecked="{Binding IncludeSelfWeight, Mode=TwoWay}"
                  Style="{DynamicResource CONCOST.CheckBox.Base}" Margin="0,0,0,16" />

        <ui:SplitButton Content="Calculate" Icon="Formula"
                        Style="{DynamicResource CONCOST.SplitButton.Primary}"
                        Click="OnCalculate" HorizontalAlignment="Stretch">
            <ui:SplitButton.DropDownContent>
                <StackPanel MinWidth="200">
                    <Button Content="ACI 318" Style="{DynamicResource CONCOST.Button.Icon}"
                            HorizontalContentAlignment="Left" Click="OnCalculateAci" />
                    <Button Content="Eurocode 2" Style="{DynamicResource CONCOST.Button.Icon}"
                            HorizontalContentAlignment="Left" Click="OnCalculateEc2" />
                </StackPanel>
            </ui:SplitButton.DropDownContent>
        </ui:SplitButton>
    </StackPanel>
</ui:Card>`,
                csharp: `// BeamViewModel.cs — chỉ cần INotifyPropertyChanged, không phụ thuộc MVVM framework
public class BeamViewModel : INotifyPropertyChanged
{
    private double _widthMm = 300, _depthMm = 600, _spanM = 6.0;
    private string _rebarText = "4D20";
    private bool _includeSelfWeight = true;

    public double WidthMm { get => _widthMm; set => Set(ref _widthMm, value); }
    public double DepthMm { get => _depthMm; set => Set(ref _depthMm, value); }
    public double SpanM { get => _spanM; set => Set(ref _spanM, value); }
    public string RebarText { get => _rebarText; set => Set(ref _rebarText, value); }
    public bool IncludeSelfWeight { get => _includeSelfWeight; set => Set(ref _includeSelfWeight, value); }

    // Kết quả tính
    public double SteelAreaMm2 { get; private set; }
    public double SelfWeightKnM { get; private set; }

    public void Calculate(string designCode)
    {
        // As lấy trực tiếp từ RebarInput (4D20 → 1256.6 mm²)
        SteelAreaMm2 = RebarParser.Parse(RebarText).AreaMm2;

        // Trọng lượng bản thân: b × h × 25 kN/m³
        SelfWeightKnM = IncludeSelfWeight
            ? WidthMm / 1000.0 * DepthMm / 1000.0 * 25.0
            : 0.0;

        Debug.WriteLine($"[{designCode}] As = {SteelAreaMm2:N1} mm², g = {SelfWeightKnM:N2} kN/m");
    }

    private void Set<T>(ref T field, T value, [CallerMemberName] string name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public event PropertyChangedEventHandler PropertyChanged;
}`
            },
            {
                title: "2. Màn hình bảng khối lượng có tìm kiếm & phân trang",
                desc: "Bố cục chuẩn cho bảng bóc tách: thanh công cụ ở trên, phân trang ở dưới, bảng header đa tầng ở giữa với footer tổng hợp tự động.",
                xaml: `<DockPanel Margin="16">
    <!-- Thanh công cụ -->
    <StackPanel DockPanel.Dock="Top" Orientation="Horizontal" Margin="0,0,0,12">
        <ui:SearchBox Width="280"
                      Placeholder="Tìm cấu kiện, mác bê tông..."
                      IsInstantSearch="True"
                      Style="{DynamicResource CONCOST.SearchBox.Base}" />

        <ui:SplitButton Content="Export" Icon="ExportFile" Margin="8,0,0,0"
                        Style="{DynamicResource CONCOST.SplitButton.Outline}">
            <ui:SplitButton.DropDownContent>
                <StackPanel MinWidth="180">
                    <Button Content="Excel (.xlsx)" Style="{DynamicResource CONCOST.Button.Icon}"
                            HorizontalContentAlignment="Left" />
                    <Button Content="CSV" Style="{DynamicResource CONCOST.Button.Icon}"
                            HorizontalContentAlignment="Left" />
                </StackPanel>
            </ui:SplitButton.DropDownContent>
        </ui:SplitButton>
    </StackPanel>

    <!-- Phân trang -->
    <ui:DataPager DockPanel.Dock="Bottom" Margin="0,12,0,0"
                  PageIndex="{Binding PageIndex, Mode=TwoWay}"
                  PageSize="{Binding PageSize, Mode=TwoWay}"
                  TotalItems="{Binding TotalRows}" />

    <!-- Bảng header đa tầng -->
    <ui:StackedHeaderDataGrid ItemsSource="{Binding PagedRows}"
                              AutoGenerateColumns="False"
                              ShowFooterSummary="True"
                              Style="{DynamicResource CONCOST.DataGrid.Base}">
        <ui:StackedHeaderDataGrid.ColumnHeaderTree>
            <ui:ColumnLeaf HeaderText="Member" BindingPath="Member" Width="150" CanUserSort="True" />

            <ui:ColumnGroup HeaderText="Section (mm)">
                <ui:ColumnLeaf HeaderText="b" BindingPath="Width" Width="70"
                               AggregateType="Average" AggregateFormat="{}{0:N0}" />
                <ui:ColumnLeaf HeaderText="h" BindingPath="Depth" Width="70"
                               AggregateType="Average" AggregateFormat="{}{0:N0}" />
            </ui:ColumnGroup>

            <ui:ColumnGroup HeaderText="Quantities">
                <ui:ColumnLeaf HeaderText="Concrete (m³)" BindingPath="ConcreteVolume" Width="120"
                               CellType="Numeric" AggregateType="Sum" AggregateFormat="{}{0:N2}" />
                <ui:ColumnLeaf HeaderText="Steel (kg)" BindingPath="SteelWeight" Width="110"
                               CellType="Numeric" AggregateType="Sum" AggregateFormat="{}{0:N1}" />
            </ui:ColumnGroup>
        </ui:StackedHeaderDataGrid.ColumnHeaderTree>
    </ui:StackedHeaderDataGrid>
</DockPanel>`,
                csharp: `// QuantityViewModel.cs — lọc, sắp xếp rồi mới cắt trang
public class QuantityViewModel : INotifyPropertyChanged
{
    private readonly List<BoqRow> _allRows = new();

    public ObservableCollection<BoqRow> PagedRows { get; } = new();
    public int TotalRows => _filtered.Count;
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 25;

    private List<BoqRow> _filtered = new();

    public void ApplyFilter(string keyword)
    {
        _filtered = string.IsNullOrWhiteSpace(keyword)
            ? _allRows.ToList()
            : _allRows.Where(r => r.Member.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();

        PageIndex = 1;
        RefreshPage();
    }

    public void RefreshPage()
    {
        PagedRows.Clear();
        foreach (var row in _filtered.Skip((PageIndex - 1) * PageSize).Take(PageSize))
            PagedRows.Add(row);

        OnPropertyChanged(nameof(TotalRows));
    }
}`
            },
            {
                title: "3. Panel cấu hình trong plugin Revit / AutoCAD",
                desc: "UserControl không có <code>App.xaml</code>: nạp resource vào chính nó, áp theme cục bộ, và dùng <code>Expander</code> + <code>PropertyGrid</code> để gọn panel hẹp.",
                callout: {
                    type: "warn",
                    title: "Đừng ghi vào Application.Current.Resources",
                    body: "Đó là tài nguyên của Revit/AutoCAD. Gán vào <code>UserControl.Resources</code> (hoặc gọi <code>ThemeManager.Current.ApplyTheme(this, …)</code>) để không làm hỏng giao diện phần mềm chủ."
                },
                xaml: `<UserControl x:Class="MyPlugin.SettingsPanel"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:ui="clr-namespace:CONCOST.UI.Controls;assembly=CONCOST.UI"
             Background="{DynamicResource CONCOST.Brush.Background}">

    <UserControl.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="pack://application:,,,/CONCOST.UI;component/Themes/ThemeResources.xaml" />
                <ResourceDictionary Source="pack://application:,,,/CONCOST.UI;component/Themes/Generic.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </UserControl.Resources>

    <DockPanel Margin="12">
        <StackPanel DockPanel.Dock="Top" Orientation="Horizontal" Margin="0,0,0,10">
            <TextBlock Text="Element properties" Style="{DynamicResource CONCOST.TextBlock.H4}" />
            <ui:ToggleSwitch IsChecked="{Binding IsDark, Mode=TwoWay}" Margin="12,0,0,0" />
        </StackPanel>

        <ui:Expander Header="Selected element" IsExpanded="True"
                     Style="{DynamicResource CONCOST.Expander.Card}" Margin="0,0,0,10">
            <ui:PropertyGrid SelectedObject="{Binding SelectedElement}" Margin="12" />
        </ui:Expander>

        <ui:Expander Header="Advanced" Style="{DynamicResource CONCOST.Expander.Card}">
            <StackPanel Margin="12">
                <ui:UnitBox Header="Tolerance" Value="{Binding ToleranceMm, Mode=TwoWay}"
                            Unit="mm" DecimalPlaces="1" />
            </StackPanel>
        </ui:Expander>
    </DockPanel>
</UserControl>`,
                csharp: `// SettingsPanel.xaml.cs — áp theme ngay khi tạo view, không cần Application
public partial class SettingsPanel : UserControl
{
    private readonly SettingsViewModel _vm = new();

    public SettingsPanel()
    {
        InitializeComponent();
        DataContext = _vm;

        // Scope theme vào chính panel này
        ThemeManager.Current.ApplyTheme(this, AppTheme.Light);

        _vm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(SettingsViewModel.IsDark))
                ThemeManager.Current.ApplyTheme(this, _vm.IsDark ? AppTheme.Dark : AppTheme.Light);
        };
    }
}

// Command entry point (Revit)
[Transaction(TransactionMode.Manual)]
public class ShowSettingsCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData data, ref string message, ElementSet elements)
    {
        var window = new CONCOSTWindow
        {
            Title = "CONCOST Settings",
            Width = 520,
            Height = 640,
            Content = new SettingsPanel()
        };

        new WindowInteropHelper(window) { Owner = data.Application.MainWindowHandle };
        window.ShowDialog();
        return Result.Succeeded;
    }
}`
            },
            {
                title: "4. Dashboard KPI có trạng thái loading",
                desc: "Kết hợp <code>StatisticCard</code>, <code>ChartLine</code> và <code>Skeleton</code>: hiện placeholder khi đang tải, đổi sang dữ liệu thật khi xong.",
                xaml: `<Grid Margin="16">
    <!-- Trạng thái tải -->
    <StackPanel Visibility="{Binding IsLoading, Converter={StaticResource BooleanToVisibility}}">
        <UniformGrid Columns="3" Margin="0,0,0,12">
            <ui:Skeleton Height="96" Style="{DynamicResource CONCOST.Skeleton.Card}" Margin="0,0,12,0" />
            <ui:Skeleton Height="96" Style="{DynamicResource CONCOST.Skeleton.Card}" Margin="0,0,12,0" />
            <ui:Skeleton Height="96" Style="{DynamicResource CONCOST.Skeleton.Card}" />
        </UniformGrid>
        <ui:Skeleton Height="240" Style="{DynamicResource CONCOST.Skeleton.Card}" />
    </StackPanel>

    <!-- Dữ liệu thật -->
    <StackPanel Visibility="{Binding IsLoading, Converter={StaticResource BooleanToVisibilityInverted}}">
        <UniformGrid Columns="3" Margin="0,0,0,12">
            <ui:StatisticCard Title="Total concrete" Value="{Binding ConcreteM3, StringFormat=N1}"
                              Unit="m³" Trend="+4.2%" TrendDirection="Up" Icon="MaterialConcrete"
                              Style="{DynamicResource CONCOST.StatisticCard.Base}" Margin="0,0,12,0" />
            <ui:StatisticCard Title="Steel weight" Value="{Binding SteelKg, StringFormat=N0}"
                              Unit="kg" Trend="-1.8%" TrendDirection="Down" Icon="Rebar"
                              Margin="0,0,12,0" />
            <ui:StatisticCard Title="Members" Value="{Binding MemberCount}"
                              Unit="pcs" Progress="{Binding ReviewProgress}" Icon="BIMModel" />
        </UniformGrid>

        <charts:ChartLine Title="Concrete by level" Unit="m³" ShowArea="True" Smooth="True"
                          Height="240" Datas="{Binding ConcreteByLevel}" />
    </StackPanel>
</Grid>`,
                csharp: `// DashboardViewModel.cs — bật skeleton, tải dữ liệu, tắt skeleton
public async Task LoadAsync()
{
    IsLoading = true;
    try
    {
        var summary = await _api.GetSummaryAsync();

        ConcreteM3 = summary.ConcreteM3;
        SteelKg = summary.SteelKg;
        MemberCount = summary.MemberCount;
        ReviewProgress = summary.ReviewedPercent;

        ConcreteByLevel = summary.ByLevel
            .Select(x => new KeyValuePair<string, double>(x.Level, x.Volume))
            .ToList();
    }
    catch (Exception ex)
    {
        Growl.Toast($"Không tải được số liệu: {ex.Message}", GrowlType.Error);
    }
    finally
    {
        IsLoading = false;
    }
}`
            }
        ],
        related: ["intro", "spreadsheet", "splitbutton", "class-library"]
    }
};
