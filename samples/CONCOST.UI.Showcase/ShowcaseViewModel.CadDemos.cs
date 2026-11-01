using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using CONCOST.UI.AttachedProperties;
using CONCOST.UI.Controls;
using CONCOST.UI.Controls.Ribbon;
using CONCOST.UI.Core.UndoRedo;
using CONCOST.UI.Icons;
using CONCOST.UI.Services;

namespace CONCOST.UI.Showcase;

/// <summary>
/// Demos for the Revit-inspired shell: the viewport chain, the browsers, the command line, the
/// status bar and the window chrome.
/// </summary>
public partial class ShowcaseViewModel
{
    // =========================================================================
    // DEMO: view cube, navigation bar, view control bar, viewport
    // =========================================================================

    private object CreateViewCubeDemo()
    {
        // ---------------------------------------------------------------------
        // 1. Camera Controller & 3D Structural Mesh Model
        // ---------------------------------------------------------------------
        var camera = new CameraController();
        camera.SetOrientation(new Vector3D(0.8, -1.0, 0.65), isFinal: false);

        var viewer = new ModelViewer3D
        {
            Height = 460,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            ShowAxes = true,
            Zoom = 0.9,
            EdgeBrush = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)), // Sky Cyan BIM
            Cursor = System.Windows.Input.Cursors.SizeAll
        };

        // Xây dựng mô hình khung thép không gian đa tầng (Multi-story Steel Structure)
        var vertices = new List<Point3D>
        {
            // Tầng 0 (Đáy móng)
            new(-5, -5, -5), new(5, -5, -5), new(5, 5, -5), new(-5, 5, -5),
            // Tầng 1 (Sàn 1)
            new(-5, -5, -1), new(5, -5, -1), new(5, 5, -1), new(-5, 5, -1),
            // Tầng 2 (Sàn 2)
            new(-5, -5, 3), new(5, -5, 3), new(5, 5, 3), new(-5, 5, 3),
            // Tầng 3 (Sàn mái)
            new(-5, -5, 7), new(5, -5, 7), new(5, 5, 7), new(-5, 5, 7),
            // Đỉnh chóp kết cấu (Apex)
            new(0, 0, 10.5)
        };

        var edges = new List<MeshEdge>
        {
            // 4 Cột chính xuyên suốt
            new(0, 4), new(4, 8), new(8, 12),
            new(1, 5), new(5, 9), new(9, 13),
            new(2, 6), new(6, 10), new(10, 14),
            new(3, 7), new(7, 11), new(11, 15),

            // Dầm sàn tầng 0
            new(0, 1), new(1, 2), new(2, 3), new(3, 0),
            // Dầm sàn tầng 1
            new(4, 5), new(5, 6), new(6, 7), new(7, 4),
            // Dầm sàn tầng 2
            new(8, 9), new(9, 10), new(10, 11), new(11, 8),
            // Dầm sàn tầng 3 (Mái)
            new(12, 13), new(13, 14), new(14, 15), new(15, 12),

            // Giằng gió chéo (Cross Bracing) ở các mặt
            new(0, 5), new(1, 4), // Mặt trước tầng 0-1
            new(4, 9), new(5, 8), // Mặt trước tầng 1-2
            new(8, 13), new(9, 12), // Mặt trước tầng 2-3
            new(2, 7), new(3, 6), // Mặt sau tầng 0-1
            new(6, 11), new(7, 10), // Mặt sau tầng 1-2
            new(10, 15), new(11, 14), // Mặt sau tầng 2-3

            // Giàn chóp mái (Roof Truss to Apex)
            new(12, 16), new(13, 16), new(14, 16), new(15, 16)
        };

        viewer.SetMesh(vertices, edges);

        // ---------------------------------------------------------------------
        // 2. ViewCube Control Configuration
        // ---------------------------------------------------------------------
        var cube = new ViewCube
        {
            Host = camera,
            Size = ViewCubeSize.Large,
            Position = ViewCubePosition.TopRight,
            DockMargin = 16,
            ShowCompass = true,
            SnapToClosestView = true,
            KeepSceneUpright = true,
            AnimatedTransitions = true,
            AnimationDuration = 450,
            InactiveOpacity = 70
        };

        // ---------------------------------------------------------------------
        // 3. Hai chiều đồng bộ ViewCube và ModelViewer3D
        // ---------------------------------------------------------------------
        var viewTelemetry = new TextBlock
        {
            Text = $"Yaw: {camera.Yaw:0.#}° | Pitch: {camera.Pitch:0.#}° | Roll: {camera.Roll:0.#}°",
            Foreground = new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8)),
            FontSize = 11,
            FontFamily = new FontFamily("Consolas, Courier New, monospace"),
            VerticalAlignment = VerticalAlignment.Center
        };

        var viewNameBadge = new TextBlock
        {
            Text = "Phối cảnh ISO (Top-Front-Right)",
            Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)),
            FontSize = 12,
            FontWeight = FontWeights.SemiBold
        };

        var interactionHint = new TextBlock
        {
            Text = "Sẵn sàng điều hướng. Bấm các mặt hoặc kéo chuột để xoay.",
            Foreground = new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8)),
            FontSize = 11,
            VerticalAlignment = VerticalAlignment.Center
        };

        bool updatingFromCamera = false;
        camera.Changed += (_, e) =>
        {
            if (updatingFromCamera) return;
            updatingFromCamera = true;
            viewer.Yaw = camera.Yaw;
            viewer.Pitch = camera.Pitch;
            updatingFromCamera = false;

            viewTelemetry.Text = $"Yaw: {camera.Yaw:0.#}° | Pitch: {camera.Pitch:0.#}° | Roll: {camera.Roll:0.#}°";
        };

        System.ComponentModel.DependencyPropertyDescriptor.FromProperty(ModelViewer3D.YawProperty, typeof(ModelViewer3D))
            .AddValueChanged(viewer, (_, _) =>
            {
                if (updatingFromCamera) return;
                updatingFromCamera = true;
                camera.Yaw = viewer.Yaw;
                camera.Pitch = viewer.Pitch;
                updatingFromCamera = false;
            });

        cube.ViewRequested += (_, e) =>
        {
            viewNameBadge.Text = $"Góc nhìn: {e.Hit.Label} ({e.Hit.Part})";
            interactionHint.Text = $"Đã yêu cầu chuyển góc: {e.Hit.Label}";
            ToastService.Info($"Chuyển góc nhìn: {e.Hit.Label}", "ViewCube 3D");
        };

        cube.RegionHovered += (_, e) =>
        {
            if (e.Hit.IsEmpty)
                interactionHint.Text = "Rê chuột lên ViewCube để chọn 26 vùng, mũi tên 90° hoặc la bàn";
            else
                interactionHint.Text = $"Đang trỏ: {e.Hit.Label} [{e.Hit.Part}]";
        };

        cube.ViewTransitionCompleted += (_, _) =>
        {
            viewTelemetry.Text = $"Yaw: {camera.Yaw:0.#}° | Pitch: {camera.Pitch:0.#}° | Roll: {camera.Roll:0.#}°";
            interactionHint.Text = "Chuyển hướng hoàn tất";
        };

        // ---------------------------------------------------------------------
        // 4. Viewport CAD Container (Dark Industrial Gradient Stage)
        // ---------------------------------------------------------------------
        var stage = new Border
        {
            Height = 460,
            CornerRadius = new CornerRadius(12),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x33, 0x41, 0x55)),
            Background = new LinearGradientBrush(
                Color.FromRgb(0x13, 0x1A, 0x24),
                Color.FromRgb(0x0A, 0x0E, 0x14), 45),
            ClipToBounds = true
        };

        var viewportGrid = new Grid();
        viewportGrid.Children.Add(viewer);
        viewportGrid.Children.Add(cube);

        // Header HUD Overlay
        var headerHud = new Border
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(14),
            Background = new SolidColorBrush(Color.FromArgb(0xDC, 0x0F, 0x17, 0x2A)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x33, 0x41, 0x55)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12, 8, 12, 8)
        };

        var headerHudStack = new StackPanel();
        var hudTitleRow = new StackPanel { Orientation = Orientation.Horizontal };
        hudTitleRow.Children.Add(new TextBlock
        {
            Text = "🏢 CAD/BIM VIEWPORT 3D",
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8)),
            Margin = new Thickness(0, 0, 8, 0)
        });
        hudTitleRow.Children.Add(new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(0x0E, 0x74, 0x90)),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(5, 1, 5, 1),
            Child = new TextBlock { Text = "LIVE 3D", FontSize = 9, FontWeight = FontWeights.Bold, Foreground = Brushes.White }
        });
        headerHudStack.Children.Add(hudTitleRow);
        headerHudStack.Children.Add(viewNameBadge);

        viewportGrid.Children.Add(headerHud);

        // Footer Telemetry Bar Overlay
        var footerBar = new Border
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Bottom,
            Background = new SolidColorBrush(Color.FromArgb(0xE0, 0x09, 0x0D, 0x16)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x1E, 0x29, 0x3B)),
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(14, 6, 14, 6)
        };

        var footerGrid = new Grid();
        footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        Grid.SetColumn(interactionHint, 0);
        Grid.SetColumn(viewTelemetry, 1);
        footerGrid.Children.Add(interactionHint);
        footerGrid.Children.Add(viewTelemetry);

        footerBar.Child = footerGrid;
        viewportGrid.Children.Add(footerBar);

        stage.Child = viewportGrid;

        // ---------------------------------------------------------------------
        // 5. Interactive Control Panel (Bảng tùy biến đầy đủ thuộc tính)
        // ---------------------------------------------------------------------
        // Row 1: Phím bấm góc nhìn nhanh (Quick Camera View Buttons)
        var quickViewsRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 14, 0, 10) };
        var quickLabel = CadHint("Góc nhìn nhanh:");
        quickLabel.VerticalAlignment = VerticalAlignment.Center;
        quickLabel.Margin = new Thickness(0, 0, 10, 0);
        quickViewsRow.Children.Add(quickLabel);

        Button MakeViewBtn(string title, Action action)
        {
            var btn = SecondaryButton(title);
            btn.Margin = new Thickness(0, 0, 6, 0);
            btn.Click += (_, _) => action();
            return btn;
        }

        quickViewsRow.Children.Add(MakeViewBtn("🏠 Home (ISO)", () => cube.ResetToHome()));
        quickViewsRow.Children.Add(MakeViewBtn("Mặt Trên (Top)", () => cube.GoToFace("TOP")));
        quickViewsRow.Children.Add(MakeViewBtn("Mặt Trước (Front)", () => cube.GoToFace("FRONT")));
        quickViewsRow.Children.Add(MakeViewBtn("Mặt Phải (Right)", () => cube.GoToFace("RIGHT")));
        quickViewsRow.Children.Add(MakeViewBtn("Mặt Sau (Back)", () => cube.GoToFace("BACK")));
        quickViewsRow.Children.Add(MakeViewBtn("Mặt Trái (Left)", () => cube.GoToFace("LEFT")));
        quickViewsRow.Children.Add(MakeViewBtn("🎯 Khung hình (Fit)", () => { viewer.ResetView(); camera.FitToView(); }));

        // Row 2: Vị trí & Kích thước & Theme & Ngôn ngữ
        var configRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 10) };

        // Position Combo
        var posLabel = CadHint("Vị trí:");
        posLabel.Margin = new Thickness(0, 0, 6, 0);
        posLabel.VerticalAlignment = VerticalAlignment.Center;
        var posCombo = new ComboBox
        {
            Width = 120,
            ItemsSource = new[] { "Góc Trên-Phải", "Góc Trên-Trái", "Góc Dưới-Phải", "Góc Dưới-Trái" },
            SelectedIndex = 0,
            Margin = new Thickness(0, 0, 16, 0)
        };
        posCombo.SelectionChanged += (_, _) =>
        {
            cube.Position = posCombo.SelectedIndex switch
            {
                1 => ViewCubePosition.TopLeft,
                2 => ViewCubePosition.BottomRight,
                3 => ViewCubePosition.BottomLeft,
                _ => ViewCubePosition.TopRight
            };
        };

        // Size Combo
        var sizeLabel = CadHint("Kích thước:");
        sizeLabel.Margin = new Thickness(0, 0, 6, 0);
        sizeLabel.VerticalAlignment = VerticalAlignment.Center;
        var sizeCombo = new ComboBox
        {
            Width = 110,
            ItemsSource = new[] { "Lớn (128px)", "Chuẩn (100px)", "Vừa (80px)", "Nhỏ (64px)" },
            SelectedIndex = 0,
            Margin = new Thickness(0, 0, 16, 0)
        };
        sizeCombo.SelectionChanged += (_, _) =>
        {
            cube.Size = sizeCombo.SelectedIndex switch
            {
                1 => ViewCubeSize.Normal,
                2 => ViewCubeSize.Small,
                3 => ViewCubeSize.Tiny,
                _ => ViewCubeSize.Large
            };
        };

        // Theme Preset Combo
        var themeLabel = CadHint("Chủ đề màu:");
        themeLabel.Margin = new Thickness(0, 0, 6, 0);
        themeLabel.VerticalAlignment = VerticalAlignment.Center;
        var themeCombo = new ComboBox
        {
            Width = 135,
            ItemsSource = new[] { "Revit Chuẩn", "Dark Slate", "Blueprint Cyan", "Amber Gold" },
            SelectedIndex = 0,
            Margin = new Thickness(0, 0, 16, 0)
        };
        themeCombo.SelectionChanged += (_, _) =>
        {
            switch (themeCombo.SelectedIndex)
            {
                case 1: // Dark Slate
                    cube.FaceBrush = new SolidColorBrush(Color.FromRgb(0x1E, 0x29, 0x3B));
                    cube.EdgeBrush = new SolidColorBrush(Color.FromRgb(0x47, 0x55, 0x69));
                    cube.BorderBrush = new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B));
                    cube.TextBrush = new SolidColorBrush(Color.FromRgb(0xF8, 0xFA, 0xFC));
                    cube.HighlightBrush = new SolidColorBrush(Color.FromRgb(0x02, 0x84, 0xC7));
                    cube.CompassBrush = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8));
                    cube.CompassTextBrush = new SolidColorBrush(Color.FromRgb(0xBA, 0xE6, 0xFD));
                    break;
                case 2: // Blueprint Cyan
                    cube.FaceBrush = new SolidColorBrush(Color.FromRgb(0x0C, 0x4A, 0x6E));
                    cube.EdgeBrush = new SolidColorBrush(Color.FromRgb(0x02, 0x84, 0xC7));
                    cube.BorderBrush = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8));
                    cube.TextBrush = Brushes.White;
                    cube.HighlightBrush = new SolidColorBrush(Color.FromRgb(0xF9, 0x73, 0x16)); // Cam CAD
                    cube.CompassBrush = new SolidColorBrush(Color.FromRgb(0x7D, 0xD3, 0xFC));
                    cube.CompassTextBrush = Brushes.White;
                    break;
                case 3: // Amber Gold
                    cube.FaceBrush = new SolidColorBrush(Color.FromRgb(0x45, 0x1A, 0x03));
                    cube.EdgeBrush = new SolidColorBrush(Color.FromRgb(0x92, 0x40, 0x0E));
                    cube.BorderBrush = new SolidColorBrush(Color.FromRgb(0xD9, 0x77, 0x06));
                    cube.TextBrush = new SolidColorBrush(Color.FromRgb(0xFE, 0xF3, 0xC7));
                    cube.HighlightBrush = new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B));
                    cube.CompassBrush = new SolidColorBrush(Color.FromRgb(0xFB, 0xBF, 0x24));
                    cube.CompassTextBrush = new SolidColorBrush(Color.FromRgb(0xFE, 0xF3, 0xC7));
                    break;
                default: // Revit Classic
                    cube.FaceBrush = null;
                    cube.EdgeBrush = null;
                    cube.BorderBrush = null;
                    cube.TextBrush = null;
                    cube.HighlightBrush = null;
                    cube.CompassBrush = null;
                    cube.CompassTextBrush = null;
                    break;
            }
        };

        // Custom Labels (Ngôn ngữ mặt cube)
        var langLabel = CadHint("Nhãn khối:");
        langLabel.Margin = new Thickness(0, 0, 6, 0);
        langLabel.VerticalAlignment = VerticalAlignment.Center;
        var langCombo = new ComboBox
        {
            Width = 110,
            ItemsSource = new[] { "Tiếng Anh (TOP)", "Tiếng Việt (TRÊN)" },
            SelectedIndex = 0
        };
        langCombo.SelectionChanged += (_, _) =>
        {
            if (langCombo.SelectedIndex == 1)
            {
                cube.TopLabel = "TRÊN";
                cube.BottomLabel = "DƯỚI";
                cube.FrontLabel = "TRƯỚC";
                cube.BackLabel = "SAU";
                cube.LeftLabel = "TRÁI";
                cube.RightLabel = "PHẢI";
            }
            else
            {
                cube.TopLabel = null;
                cube.BottomLabel = null;
                cube.FrontLabel = null;
                cube.BackLabel = null;
                cube.LeftLabel = null;
                cube.RightLabel = null;
            }
        };

        // ---------------------------------------------------------------------
        // 5. Khay dieu khien tuong tac 2 cot chuan Desktop CAD/BIM
        // ---------------------------------------------------------------------
        var workbench = new Grid { Margin = new Thickness(0, 16, 0, 4) };
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(430) });
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Cot 1: Hinh hoc & Thiet lap (Geometry & Appearance)
        var col1 = new StackPanel();
        var head1 = new TextBlock
        {
            Text = "HINH HOC VA GIAO DIEN (GEOMETRY & APPEARANCE)",
            FontSize = 11.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 12)
        };
        head1.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        col1.Children.Add(head1);

        var rowPosSize = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
        rowPosSize.Children.Add(posLabel);
        rowPosSize.Children.Add(posCombo);
        rowPosSize.Children.Add(sizeLabel);
        rowPosSize.Children.Add(sizeCombo);
        col1.Children.Add(rowPosSize);

        var rowThemeLang = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };
        rowThemeLang.Children.Add(themeLabel);
        rowThemeLang.Children.Add(themeCombo);
        rowThemeLang.Children.Add(langLabel);
        rowThemeLang.Children.Add(langCombo);
        col1.Children.Add(rowThemeLang);

        col1.Children.Add(AppearanceSlider("Do nhay keo xoay", 0.2, 2.0, cube.DragSensitivity, 0.05, "0.00", v => cube.DragSensitivity = v));
        col1.Children.Add(AppearanceSlider("Do mo khi roi chuot", 10, 100, cube.InactiveOpacity, 5, "0'%'", v => cube.InactiveOpacity = v));
        col1.Children.Add(AppearanceSlider("Thoi gian chuyen goc", 100, 1500, cube.AnimationDuration, 50, "0' ms'", v => cube.AnimationDuration = v));

        Grid.SetColumn(col1, 0);
        workbench.Children.Add(col1);

        // Cot 2: Tinh nang & Huong dan Icon chi tiet (Features & Status Callout)
        var col2 = new StackPanel();
        var head2 = new TextBlock
        {
            Text = "TINH NANG VA HUONG DAN TUONG TAC (FEATURES & HINTS)",
            FontSize = 11.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 12)
        };
        head2.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        col2.Children.Add(head2);

        var switches = new WrapPanel { Margin = new Thickness(0, 0, 0, 12) };
        switches.Children.Add(AppearanceToggle("Luon hien icon", cube.AlwaysShowTools, v => cube.AlwaysShowTools = v));
        switches.Children.Add(AppearanceToggle("Hien la ban", cube.ShowCompass, v => cube.ShowCompass = v));
        switches.Children.Add(AppearanceToggle("Hut goc gan nhat", cube.SnapToClosestView, v => cube.SnapToClosestView = v));
        switches.Children.Add(AppearanceToggle("Giu dung truc", cube.KeepSceneUpright, v => cube.KeepSceneUpright = v));
        switches.Children.Add(AppearanceToggle("Chuyen dong muot", cube.AnimatedTransitions, v => cube.AnimatedTransitions = v));
        col2.Children.Add(switches);

        var iconGuideCallout = CreateCalloutCard(
            "HUONG DAN DIEU KHIEN VIEWCUBE 3D:\n" +
            "• [Home] Goc tren-trai: Quay ve goc nhin Isometric chuan.\n" +
            "• [Roll] Goc tren-phai: Quay 90 do quanh truc nhin.\n" +
            "• [Mui ten] 4 huong: Buoc nhay 90 do sang mat ke tiep.\n" +
            "• [La ban] N, E, S, W: Keo xoay phuong vi mat dat.\n" +
            "• [Menu] Goc duoi-phai: Chuyen che do Ortho / Perspective.\n" +
            "• Keo chuot truc tiep len khoi: Xoay 3D tu do muot ma.");
        col2.Children.Add(iconGuideCallout);

        Grid.SetColumn(col2, 2);
        workbench.Children.Add(col2);

        var content = new StackPanel();
        content.Children.Add(stage);
        content.Children.Add(quickViewsRow);
        content.Children.Add(workbench);

                var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Khối điều hướng Revit ViewCube 3D",
            content,
            xaml: @"<Grid>
    <controls:ModelViewer3D x:Name=""Viewport"" ShowAxes=""True"" />
    <ui:ViewCube Host=""{Binding Camera}""
                 Size=""Large""
                 Position=""TopRight""
                 ShowCompass=""True""
                 InactiveOpacity=""70""
                 FaceBrush=""#1E293B""
                 HighlightBrush=""#0284C7""
                 TopLabel=""TRÊN"" FrontLabel=""TRƯỚC"" />
</Grid>",
            csharp: @"// ViewCube được gắn trực tiếp vào ICameraHost của Viewport 3D:
var camera = new CameraController();
cube.Host = camera;
cube.Size = ViewCubeSize.Large;
cube.Position = ViewCubePosition.TopRight;
cube.ShowCompass = true;

// Hỗ trợ đổi nhãn mặt sang Tiếng Việt:
cube.TopLabel = ""TRÊN"";
cube.FrontLabel = ""TRƯỚC"";

// Đổi góc nhìn trực tiếp bằng code:
cube.ResetToHome();
cube.GoToFace(""TOP"");"));

        return root;
    }

    private object CreateNavigationBarDemo()
    {
        // 1. Khoi tao Camera va Model 3D cho Viewport CAD
        var camera = new CameraController();
        camera.SetOrientation(new Vector3D(0.85, -1.0, 0.65), isFinal: false);

        var viewer = new ModelViewer3D
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            ShowAxes = true,
            Zoom = 0.95,
            EdgeBrush = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)),
            Cursor = System.Windows.Input.Cursors.SizeAll
        };

        var vertices = new List<Point3D>
        {
            new(-5, -5, -4), new(5, -5, -4), new(5, 5, -4), new(-5, 5, -4),
            new(-5, -5, 0),  new(5, -5, 0),  new(5, 5, 0),  new(-5, 5, 0),
            new(-5, -5, 4),  new(5, -5, 4),  new(5, 5, 4),  new(-5, 5, 4),
            new(0, 0, 7.5)
        };
        var edges = new List<MeshEdge>
        {
            new(0, 1), new(1, 2), new(2, 3), new(3, 0),
            new(4, 5), new(5, 6), new(6, 7), new(7, 4),
            new(8, 9), new(9, 10), new(10, 11), new(11, 8),
            new(0, 4), new(1, 5), new(2, 6), new(3, 7),
            new(4, 8), new(5, 9), new(6, 10), new(7, 11),
            new(8, 12), new(9, 12), new(10, 12), new(11, 12)
        };
        viewer.SetMesh(vertices, edges);

        // 2. Khoi tao thanh dieu huong NavigationBar
        var bar = new NavigationBar
        {
            Variant = NavigationBarVariant.Floating,
            Orientation = Orientation.Horizontal,
            ButtonSize = 28,
            IconSize = 14,
            CornerRadius = new CornerRadius(6),
            Elevation = 2,
            ShowWalk = true,
            ShowPerspective = true,
            Ucs = UcsMode.Top,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(14)
        };

        // HUD Text & Pill trong Viewport
        var hudText = new TextBlock
        {
            Text = "Lệnh: Sẵn sàng | Công cụ: None | UCS: Top",
            FontSize = 11.5,
            FontWeight = FontWeights.Medium,
            Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)),
            VerticalAlignment = VerticalAlignment.Center
        };

        var hudPill = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(220, 15, 23, 42)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(120, 56, 189, 248)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12, 5, 12, 5),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(14),
            Child = hudText
        };

        // Callout thong bao trang thai
        var statusCallout = CreateCalloutCard("Sẵn sàng: Bấm các nút trên thanh NavigationBar để điều khiển góc nhìn và công cụ 3D.");

        void UpdateCallout(string msg)
        {
            if (statusCallout.Child is Grid g)
            {
                var tb = g.Children.OfType<TextBlock>().FirstOrDefault(t => Grid.GetColumn(t) == 1);
                if (tb != null) tb.Text = msg;
            }
        }

        bar.NavigationRequested += (_, e) =>
        {
            switch (e.Command)
            {
                case NavigationCommand.ZoomIn:
                    viewer.Zoom = Math.Min(3.0, viewer.Zoom * 1.2);
                    break;
                case NavigationCommand.ZoomOut:
                    viewer.Zoom = Math.Max(0.2, viewer.Zoom / 1.2);
                    break;
                case NavigationCommand.ZoomExtents:
                    viewer.Zoom = 0.95;
                    camera.SetOrientation(new Vector3D(0.85, -1.0, 0.65), isFinal: true);
                    break;
                case NavigationCommand.ZoomSelection:
                    viewer.Zoom = 1.35;
                    break;
                case NavigationCommand.Perspective:
                    break;
            }
            hudText.Text = $"Lệnh: {e.Command} | Công cụ: {bar.ActiveTool} | UCS: {bar.Ucs}";
            UpdateCallout($"Đã thực thi lệnh điều hướng: {e.Command}");
            ToastService.Info($"Điều hướng: {e.Command}", "NavigationBar");
        };

        bar.ActiveToolChanged += (_, _) =>
        {
            hudText.Text = $"Công cụ: {bar.ActiveTool} | UCS: {bar.Ucs}";
            UpdateCallout($"Công cụ tương tác đang bật: {bar.ActiveTool} (Bấm lại nút trên thanh để tắt)");
            viewer.Cursor = bar.ActiveTool switch
            {
                NavigationTool.Pan   => System.Windows.Input.Cursors.Hand,
                NavigationTool.Orbit => System.Windows.Input.Cursors.SizeAll,
                NavigationTool.Walk  => System.Windows.Input.Cursors.Cross,
                _                    => System.Windows.Input.Cursors.Arrow
            };
        };

        bar.UcsChanged += (_, _) =>
        {
            hudText.Text = $"Mặt phẳng UCS: {bar.Ucs} | Công cụ: {bar.ActiveTool}";
            UpdateCallout($"Đã chuyển đổi mặt phẳng làm việc sang: {bar.Ucs}");
            switch (bar.Ucs)
            {
                case UcsMode.Top:   camera.SetOrientation(new Vector3D(0, 0, 1), isFinal: true); break;
                case UcsMode.Front: camera.SetOrientation(new Vector3D(0, -1, 0), isFinal: true); break;
                case UcsMode.Right: camera.SetOrientation(new Vector3D(1, 0, 0), isFinal: true); break;
                case UcsMode.World: camera.SetOrientation(new Vector3D(0.85, -1.0, 0.65), isFinal: true); break;
            }
        };

        // Khung hien thi Viewport 3D
        var viewportHost = new Grid();
        viewportHost.Children.Add(viewer);
        viewportHost.Children.Add(hudPill);
        viewportHost.Children.Add(bar);

        var stage = new Border
        {
            Height = 260,
            CornerRadius = new CornerRadius(8),
            Background = new LinearGradientBrush(
                Color.FromRgb(0x13, 0x1B, 0x2A), Color.FromRgb(0x1F, 0x2D, 0x42), 45),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x33, 0x41, 0x55)),
            BorderThickness = new Thickness(1),
            ClipToBounds = true,
            Child = viewportHost
        };

        // 3. Khay cau hinh tuong tac 2 cot chuan Desktop CAD/BIM
        var workbench = new Grid { Margin = new Thickness(0, 14, 0, 4) };
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(430) });
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Cot 1: Cau hinh kieu dang va kich thuoc
        var col1 = new StackPanel();
        var head1 = new TextBlock
        {
            Text = "KIỂU DÁNG & HÌNH HỌC (GEOMETRY & STYLE)",
            FontSize = 11.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 10)
        };
        head1.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        col1.Children.Add(head1);

        var variantRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
        var varLbl = new TextBlock { Text = "Kiểu:", FontSize = 12, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0), Width = 38 };
        varLbl.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        var variantCombo = new ComboBox
        {
            Width = 160,
            ItemsSource = new[] { "Floating (Nổi)", "Seamless (Phẳng)", "Glass (Kính mờ)" },
            SelectedIndex = 0,
            Margin = new Thickness(0, 0, 14, 0)
        };
        variantCombo.SelectionChanged += (_, _) =>
        {
            bar.Variant = (NavigationBarVariant)Math.Max(0, variantCombo.SelectedIndex);
            UpdateCallout($"Đã đổi kiểu hiển thị NavigationBar sang: {bar.Variant}");
        };

        var oriLbl = new TextBlock { Text = "Hướng:", FontSize = 12, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0), Width = 45 };
        oriLbl.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        var oriCombo = new ComboBox
        {
            Width = 100,
            ItemsSource = new[] { "Ngang", "Dọc" },
            SelectedIndex = 0
        };
        oriCombo.SelectionChanged += (_, _) =>
        {
            bar.Orientation = oriCombo.SelectedIndex == 1 ? Orientation.Vertical : Orientation.Horizontal;
        };

        variantRow.Children.Add(varLbl);
        variantRow.Children.Add(variantCombo);
        variantRow.Children.Add(oriLbl);
        variantRow.Children.Add(oriCombo);
        col1.Children.Add(variantRow);

        col1.Children.Add(AppearanceSlider("Cỡ nút (ButtonSize)", 20, 40, bar.ButtonSize, 2, "0", v => bar.ButtonSize = v));
        col1.Children.Add(AppearanceSlider("Cỡ icon (IconSize)", 10, 22, bar.IconSize, 1, "0", v => bar.IconSize = v));
        col1.Children.Add(AppearanceSlider("Bo góc (CornerRadius)", 0, 20, bar.CornerRadius.TopLeft, 1, "0", v => bar.CornerRadius = new CornerRadius(v)));
        col1.Children.Add(AppearanceSlider("Độ nổi (Elevation)", 0, 4, bar.Elevation, 1, "0", v => bar.Elevation = (int)v));

        Grid.SetColumn(col1, 0);
        workbench.Children.Add(col1);

        // Cot 2: Nhom cong cu, hanh dong & callout
        var col2 = new StackPanel();
        var head2 = new TextBlock
        {
            Text = "NHÓM TÍNH NĂNG & THAO TÁC (FEATURES & ACTIONS)",
            FontSize = 11.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 10)
        };
        head2.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        col2.Children.Add(head2);

        var switchesWrap = new WrapPanel { Margin = new Thickness(0, 0, 0, 10) };
        switchesWrap.Children.Add(AppearanceToggle("Nhãn UCS", bar.ShowUcsLabel, v => bar.ShowUcsLabel = v));
        switchesWrap.Children.Add(AppearanceToggle("Cụm zoom", bar.ShowZoom, v => bar.ShowZoom = v));
        switchesWrap.Children.Add(AppearanceToggle("Phối cảnh", bar.ShowPerspective, v => bar.ShowPerspective = v));
        switchesWrap.Children.Add(AppearanceToggle("Walk", bar.ShowWalk, v => bar.ShowWalk = v));
        switchesWrap.Children.Add(AppearanceToggle("Đường phân cách", bar.ShowDividers, v => bar.ShowDividers = v));
        col2.Children.Add(switchesWrap);

        // Quick action buttons
        var quickActions = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
        var resetBtn = SecondaryButton("🏠 Reset View");
        resetBtn.Margin = new Thickness(0, 0, 6, 0);
        resetBtn.Click += (_, _) => { viewer.ResetView(); camera.FitToView(); UpdateCallout("Đã đặt lại góc nhìn mặc định."); };

        var extentsBtn = SecondaryButton("🔍 Zoom Extents");
        extentsBtn.Margin = new Thickness(0, 0, 6, 0);
        extentsBtn.Click += (_, _) => { viewer.Zoom = 0.95; UpdateCallout("Đã thu phóng toàn bộ mô hình (Zoom Extents)."); };

        var isoBtn = SecondaryButton("📐 Góc nhìn ISO");
        isoBtn.Click += (_, _) => { camera.SetOrientation(new Vector3D(0.85, -1.0, 0.65), isFinal: true); UpdateCallout("Đã chuyển sang góc phối cảnh Isometric."); };

        quickActions.Children.Add(resetBtn);
        quickActions.Children.Add(extentsBtn);
        quickActions.Children.Add(isoBtn);
        col2.Children.Add(quickActions);

        col2.Children.Add(statusCallout);

        Grid.SetColumn(col2, 2);
        workbench.Children.Add(col2);

        var stageHost = new StackPanel();
        stageHost.Children.Add(stage);
        stageHost.Children.Add(workbench);

        string xamlCode = @"<ui:NavigationBar Variant=""Floating""
                  Orientation=""Horizontal""
                  ButtonSize=""28""
                  IconSize=""14""
                  CornerRadius=""6""
                  Elevation=""1""
                  Ucs=""World""
                  ActiveTool=""None""
                  ShowUcsLabel=""True""
                  ShowZoom=""True""
                  ShowWalk=""True""
                  ShowPerspective=""True""
                  ShowDividers=""True""
                  NavigationRequested=""OnNavigate""
                  ActiveToolChanged=""OnActiveToolChanged""
                  UcsChanged=""OnUcsChanged"" />";

        string csCode = @"// 1. Cấu hình kiểu dáng (Variant) và kích thước
bar.Variant = NavigationBarVariant.Floating; // Floating (nổi có bóng), Seamless (phẳng trong toolbar), Glass (kính mờ 3D)
bar.Orientation = Orientation.Horizontal;    // Horizontal (ngang) hoặc Vertical (dọc)
bar.ButtonSize = 28;                         // Kích thước nút bấm (px)
bar.IconSize = 14;                           // Kích thước icon
bar.CornerRadius = new CornerRadius(6);      // Bo góc thanh điều hướng
bar.Elevation = 1;                           // Độ nổi đổ bóng (0-4)

// 2. Ẩn / hiện các cụm nút tính năng
bar.ShowUcsLabel = true;                     // Nhãn hệ tọa độ (World, Top, Front, Right)
bar.ShowZoom = true;                         // Cụm zoom (In, Out, Extents, Selection)
bar.ShowWalk = true;                         // Công cụ đi bộ (Walk)
bar.ShowPerspective = true;                  // Nút chuyển đổi phối cảnh (Perspective)
bar.ShowDividers = true;                     // Vạch ngăn cách giữa các nhóm

// 3. Xử lý các lệnh điều hướng một lần (NavigationCommand)
bar.NavigationRequested += (sender, e) =>
{
    switch (e.Command)
    {
        case NavigationCommand.ZoomIn:        camera.Zoom(0.8); break;
        case NavigationCommand.ZoomOut:       camera.Zoom(1.25); break;
        case NavigationCommand.ZoomExtents:   camera.FitToExtents(); break;
        case NavigationCommand.ZoomSelection: camera.FitToSelection(); break;
        case NavigationCommand.Perspective:   camera.TogglePerspective(); break;
    }
};

// 4. Xử lý công cụ tương tác giữ trạng thái (ActiveTool: Pan, Orbit, Walk)
bar.ActiveToolChanged += (sender, e) =>
{
    viewport.CurrentTool = bar.ActiveTool switch
    {
        NavigationTool.Pan   => ViewportTool.Pan,
        NavigationTool.Orbit => ViewportTool.Orbit,
        NavigationTool.Walk  => ViewportTool.Walk,
        _                    => ViewportTool.Select
    };
};";

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Thanh điều hướng Viewport 3D (NavigationBar)",
            stageHost,
            xaml: xamlCode,
            csharp: csCode));

        return root;
    }

    private object CreateViewControlBarDemo()
    {
        // 1. Khoi tao Camera va Model 3D cho Viewport CAD
        var camera = new CameraController();
        camera.SetOrientation(new Vector3D(0.9, -1.05, 0.7), isFinal: false);

        var viewer = new ModelViewer3D
        {
            Height = 310,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            ShowAxes = true,
            Zoom = 0.95,
            EdgeBrush = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)),
            Cursor = System.Windows.Input.Cursors.SizeAll
        };

        // Mesh da tang
        var verticesFine = new List<Point3D>
        {
            new(-5, -5, -4), new(5, -5, -4), new(5, 5, -4), new(-5, 5, -4),
            new(-5, -5, 0),  new(5, -5, 0),  new(5, 5, 0),  new(-5, 5, 0),
            new(-5, -5, 4),  new(5, -5, 4),  new(5, 5, 4),  new(-5, 5, 4),
            new(0, 0, 7.5)
        };
        var edgesFine = new List<MeshEdge>
        {
            new(0, 1), new(1, 2), new(2, 3), new(3, 0),
            new(4, 5), new(5, 6), new(6, 7), new(7, 4),
            new(8, 9), new(9, 10), new(10, 11), new(11, 8),
            new(0, 4), new(1, 5), new(2, 6), new(3, 7),
            new(4, 8), new(5, 9), new(6, 10), new(7, 11),
            new(8, 12), new(9, 12), new(10, 12), new(11, 12)
        };

        var verticesCoarse = new List<Point3D>
        {
            new(-5, -5, -4), new(5, -5, -4), new(5, 5, -4), new(-5, 5, -4),
            new(-5, -5, 4),  new(5, -5, 4),  new(5, 5, 4),  new(-5, 5, 4)
        };
        var edgesCoarse = new List<MeshEdge>
        {
            new(0, 1), new(1, 2), new(2, 3), new(3, 0),
            new(4, 5), new(5, 6), new(6, 7), new(7, 4),
            new(0, 4), new(1, 5), new(2, 6), new(3, 7)
        };

        viewer.SetMesh(verticesFine, edgesFine);

        // 2. ViewControlBar docked tai day cua Viewport (Revit style)
        var bar = new ViewControlBar
        {
            Variant = ViewControlBarVariant.Glass,
            ControlHeight = 28,
            IconSize = 14,
            CornerRadius = new CornerRadius(6),
            Elevation = 2,
            ShowShadows = false,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(14)
        };

        var scales = new ObservableCollection<string> { "1:50", "1:100", "1:200", "1:500", "1:1000" };
        bar.Scales = scales;
        bar.ViewScale = "1:100";

        // HUD Viewport Info Pill
        var hudText = new TextBlock
        {
            Text = $"Ti le: {bar.ViewScale} | Chi tiet: {bar.DetailLevel} | Kieu: {bar.VisualStyle}",
            FontSize = 11.5,
            FontWeight = FontWeights.Medium,
            VerticalAlignment = VerticalAlignment.Center
        };
        hudText.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");

        var hudPill = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(210, 15, 23, 42)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(120, 56, 189, 248)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12, 6, 12, 6),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(14),
            Child = hudText
        };

        var statusCallout = CreateCalloutCard("San sang: Thay doi cac thong so tren ViewControlBar de cap nhat khung nhin CAD trong thoi gian thuc.");

        void UpdateCallout(string msg)
        {
            if (statusCallout.Child is Grid g)
            {
                var tb = g.Children.OfType<TextBlock>().FirstOrDefault(t => Grid.GetColumn(t) == 1);
                if (tb != null) tb.Text = msg;
            }
        }

        bar.SettingsChanged += (_, name) =>
        {
            hudText.Text = $"Ti le: {bar.ViewScale} | Chi tiet: {bar.DetailLevel} | Kieu: {bar.VisualStyle}";
            UpdateCallout($"Da cap nhat thiet lap khung nhin: [{name}]. Mo hinh CAD dong bo thanh cong.");

            // Sync visual style with 3D viewer
            switch (bar.VisualStyle)
            {
                case VisualStyleMode.Wireframe:
                    viewer.EdgeBrush = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8));
                    break;
                case VisualStyleMode.HiddenLine:
                    viewer.EdgeBrush = new SolidColorBrush(Color.FromRgb(0xFA, 0xFA, 0xFA));
                    break;
                case VisualStyleMode.Shaded:
                    viewer.EdgeBrush = new SolidColorBrush(Color.FromRgb(0xFB, 0x92, 0x3C));
                    break;
                case VisualStyleMode.Realistic:
                    viewer.EdgeBrush = new SolidColorBrush(Color.FromRgb(0x4A, 0xDE, 0x80));
                    break;
            }

            // Sync detail level with mesh
            if (bar.DetailLevel == ViewDetailLevel.Coarse)
                viewer.SetMesh(verticesCoarse, edgesCoarse);
            else
                viewer.SetMesh(verticesFine, edgesFine);

            ToastService.Info($"Thiet lap: {name}", "ViewControlBar");
        };

        bar.HideIsolateRequested += (_, _) =>
        {
            UpdateCallout("Da kich hoat che do An/Co lap doi tuong tam thoi (Temporary Hide/Isolate).");
            ToastService.Info("An / Co lap tam thoi", "ViewControlBar");
        };

        // Viewport Host Grid
        var viewportHost = new Grid();
        viewportHost.Children.Add(viewer);
        viewportHost.Children.Add(hudPill);
        viewportHost.Children.Add(bar);

        var stage = new Border
        {
            Height = 310,
            CornerRadius = new CornerRadius(8),
            Background = new LinearGradientBrush(
                Color.FromRgb(0x13, 0x1B, 0x2A), Color.FromRgb(0x1F, 0x2D, 0x42), 45),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x33, 0x41, 0x55)),
            BorderThickness = new Thickness(1),
            ClipToBounds = true,
            Child = viewportHost
        };

        // 3. Khay cau hinh tuong tac 2 cot chuan muc
        var workbench = new Grid { Margin = new Thickness(0, 18, 0, 4) };
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(430) });
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Cot 1: Hinh hoc & Them ti le
        var col1 = new StackPanel();
        var head1 = new TextBlock
        {
            Text = "KIEU DANG & HINH HOC (GEOMETRY & STYLE)",
            FontSize = 11.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 12)
        };
        head1.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        col1.Children.Add(head1);

        var variantRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };
        var varLbl = new TextBlock { Text = "Kieu:", FontSize = 12, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0), Width = 38 };
        varLbl.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        var variantCombo = new ComboBox
        {
            Width = 145,
            ItemsSource = new[] { "Floating (Noi)", "Seamless (Phang)", "Glass (Kinh mo)" },
            SelectedIndex = 2,
            Margin = new Thickness(0, 0, 16, 0)
        };
        variantCombo.SelectionChanged += (_, _) =>
        {
            bar.Variant = (ViewControlBarVariant)Math.Max(0, variantCombo.SelectedIndex);
            UpdateCallout($"Doi kieu hien thi sang: {bar.Variant}");
        };
        variantRow.Children.Add(varLbl);
        variantRow.Children.Add(variantCombo);
        col1.Children.Add(variantRow);

        col1.Children.Add(AppearanceSlider("Cao dieu khien", 20, 40, bar.ControlHeight, 2, "0", v => bar.ControlHeight = v));
        col1.Children.Add(AppearanceSlider("Co icon", 10, 22, bar.IconSize, 1, "0", v => bar.IconSize = v));
        col1.Children.Add(AppearanceSlider("Bo goc", 0, 20, bar.CornerRadius.TopLeft, 1, "0", v => bar.CornerRadius = new CornerRadius(v)));
        col1.Children.Add(AppearanceSlider("Do noi", 0, 4, bar.Elevation, 1, "0", v => bar.Elevation = (int)v));

        // Scale Custom Add Row
        var scaleRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
        var scaleLbl = new TextBlock { Text = "Them ti le:", FontSize = 12, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0), Width = 65 };
        scaleLbl.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        var scaleBox = new TextBox { Text = "1:75", Width = 75, Height = 28, Margin = new Thickness(0, 0, 8, 0), VerticalContentAlignment = VerticalAlignment.Center };
        var addScaleBtn = SecondaryButton("Them vao list");
        addScaleBtn.Height = 28;
        addScaleBtn.Padding = new Thickness(10, 2, 10, 2);
        addScaleBtn.Click += (_, _) =>
        {
            string txt = scaleBox.Text?.Trim() ?? string.Empty;
            if (txt.Length > 0 && !scales.Contains(txt))
            {
                scales.Add(txt);
                bar.ViewScale = txt;
                UpdateCallout($"Da them ti le moi: {txt} va chon truc tiep.");
            }
        };
        scaleRow.Children.Add(scaleLbl);
        scaleRow.Children.Add(scaleBox);
        scaleRow.Children.Add(addScaleBtn);
        col1.Children.Add(scaleRow);

        Grid.SetColumn(col1, 0);
        workbench.Children.Add(col1);

        // Cot 2: Cac nhom cong tac tinh nang
        var col2 = new StackPanel();
        var head2 = new TextBlock
        {
            Text = "NHOM TINH NANG & TRANG THAI (FEATURES & STATUS)",
            FontSize = 11.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 12)
        };
        head2.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        col2.Children.Add(head2);

        var switchesWrap = new WrapPanel { Margin = new Thickness(0, 0, 0, 12) };
        switchesWrap.Children.Add(AppearanceToggle("Ti le (Scale)", bar.ShowScale, v => bar.ShowScale = v));
        switchesWrap.Children.Add(AppearanceToggle("Muc chi tiet", bar.ShowDetailLevel, v => bar.ShowDetailLevel = v));
        switchesWrap.Children.Add(AppearanceToggle("Kieu hien thi", bar.ShowVisualStyle, v => bar.ShowVisualStyle = v));
        switchesWrap.Children.Add(AppearanceToggle("Cum cong tac", bar.ShowToggles, v => bar.ShowToggles = v));
        switchesWrap.Children.Add(AppearanceToggle("An / Isolate", bar.ShowHideIsolate, v => bar.ShowHideIsolate = v));
        switchesWrap.Children.Add(AppearanceToggle("Duong phan cach", bar.ShowDividers, v => bar.ShowDividers = v));
        switchesWrap.Children.Add(AppearanceToggle("Nhan 'An'", false, v => bar.HideIsolateCaption = v ? "An" : "Hide"));
        col2.Children.Add(switchesWrap);

        col2.Children.Add(statusCallout);

        Grid.SetColumn(col2, 2);
        workbench.Children.Add(col2);

        var stageHost = new StackPanel();
        stageHost.Children.Add(stage);
        stageHost.Children.Add(workbench);

        string xamlCode = @"<ui:ViewControlBar Variant=""Floating""
                   ControlHeight=""28""
                   IconSize=""14""
                   CornerRadius=""6""
                   Elevation=""1""
                   ViewScale=""1:100""
                   DetailLevel=""Medium""
                   VisualStyle=""HiddenLine""
                   ShowScale=""True""
                   ShowDetailLevel=""True""
                   ShowVisualStyle=""True""
                   ShowToggles=""True""
                   ShowHideIsolate=""True""
                   HideIsolateCaption=""Hide""
                   SettingsChanged=""OnViewSettingChanged""
                   HideIsolateRequested=""OnHideIsolate"" />";

        string csCode = @"// 1. Cau hinh thanh dieu khien khung nhin CAD
bar.Variant = ViewControlBarVariant.Glass; // Floating, Seamless, Glass
bar.ControlHeight = 28;
bar.IconSize = 14;
bar.CornerRadius = new CornerRadius(6);
bar.Elevation = 1;

// 2. Danh sach ti le ban ve
var scales = new ObservableCollection<string> { ""1:50"", ""1:100"", ""1:200"", ""1:500"" };
bar.Scales = scales;
bar.ViewScale = ""1:100"";

// 3. Bat/tat cac cum nut chuc nang
bar.ShowScale = true;
bar.ShowDetailLevel = true;
bar.ShowVisualStyle = true;
bar.ShowToggles = true;
bar.ShowHideIsolate = true;
bar.ShowDividers = true;

// 4. Lang nghe su kien thay doi thiet lap (SettingsChanged)
bar.SettingsChanged += (sender, settingName) =>
{
    switch (settingName)
    {
        case nameof(ViewControlBar.ViewScale):
            viewport.ApplyScale(bar.ViewScale);
            break;
        case nameof(ViewControlBar.DetailLevel):
            viewport.SetDetailLevel(bar.DetailLevel);
            break;
        case nameof(ViewControlBar.VisualStyle):
            viewport.SetVisualStyle(bar.VisualStyle);
            break;
        case nameof(ViewControlBar.ShowShadows):
            viewport.ToggleShadows(bar.ShowShadows);
            break;
    }
};

// 5. Xu ly su kien An / Co lap doi tuong
bar.HideIsolateRequested += (sender, e) =>
{
    viewport.ToggleTemporaryHideIsolate();
};";

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Thanh thuoc tinh khung nhin CAD (ViewControlBar)",
            stageHost,
            xaml: xamlCode,
            csharp: csCode));

        return root;
    }

    private object CreateViewportControlDemo()
    {
        // ---------------------------------------------------------------------
        // 1. Khung nhìn CAD kỹ thuật cao (High-precision CAD Drafting Viewport)
        // ---------------------------------------------------------------------
        var viewport = new ViewportControl
        {
            Height = 340,
            GridStep = 500,
            GridMajorEvery = 5,
            ShowGrid = true,
            ShowCrosshair = true,
            ShowAperture = true,
            SnapToGrid = true,
            ShowAxes = true,
            ShowOriginMarker = true,
            AxisThickness = 1.2,
            SelectionRectangle = new Rect(60, 40, 220, 150),
            SelectionFillOpacity = 0.08
        };

        viewport.SetResourceReference(ViewportControl.AxisBrushProperty, "CONCOST.Brush.TextSecondary");
        viewport.SetResourceReference(ViewportControl.SelectionBrushProperty, "CONCOST.Brush.Primary");

        // 1.1 Thêm chi tiết dầm, cột và kích thước kỹ thuật vào Overlay
        var overlay = viewport.Overlay;

        // Trục lưới kỹ thuật A, B & 1, 2
        void AddGridAxis(double x1, double y1, double x2, double y2, string bubbleLabel, double bx, double by)
        {
            var axisLine = new System.Windows.Shapes.Line
            {
                X1 = x1, Y1 = y1, X2 = x2, Y2 = y2,
                Stroke = new SolidColorBrush(Color.FromArgb(0x40, 0x38, 0xBD, 0xF8)),
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 6, 4, 2, 4 },
                IsHitTestVisible = false
            };
            overlay.Children.Add(axisLine);

            var bubble = new Border
            {
                Width = 22, Height = 22,
                CornerRadius = new CornerRadius(11),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)),
                BorderThickness = new Thickness(1.2),
                Background = new SolidColorBrush(Color.FromArgb(0xEE, 0x0F, 0x17, 0x2A)),
                IsHitTestVisible = false
            };
            var bubbleTxt = new TextBlock
            {
                Text = bubbleLabel, FontSize = 11, FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            bubble.Child = bubbleTxt;
            Canvas.SetLeft(bubble, bx - 11);
            Canvas.SetTop(bubble, by - 11);
            overlay.Children.Add(bubble);
        }

        AddGridAxis(60, 30, 60, 300, "A", 60, 20);
        AddGridAxis(280, 30, 280, 300, "B", 280, 20);
        AddGridAxis(40, 80, 500, 80, "1", 28, 80);
        AddGridAxis(40, 230, 500, 230, "2", 28, 230);

        // Cột bê tông cốt thép (Columns) tại giao điểm trục
        void AddCadColumn(double cx, double cy)
        {
            var col = new System.Windows.Shapes.Rectangle
            {
                Width = 20, Height = 20,
                Fill = new SolidColorBrush(Color.FromRgb(0x02, 0x84, 0xC7)),
                Stroke = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)),
                StrokeThickness = 1.5,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(col, cx - 10);
            Canvas.SetTop(col, cy - 10);
            overlay.Children.Add(col);
        }
        AddCadColumn(60, 80);
        AddCadColumn(280, 80);
        AddCadColumn(60, 230);
        AddCadColumn(280, 230);

        // Dầm kết cấu nối giữa các cột
        var beamTop = new System.Windows.Shapes.Line { X1 = 60, Y1 = 80, X2 = 280, Y2 = 80, Stroke = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)), StrokeThickness = 3, IsHitTestVisible = false };
        var beamLeft = new System.Windows.Shapes.Line { X1 = 60, Y1 = 80, X2 = 60, Y2 = 230, Stroke = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)), StrokeThickness = 3, IsHitTestVisible = false };
        var beamRight = new System.Windows.Shapes.Line { X1 = 280, Y1 = 80, X2 = 280, Y2 = 230, Stroke = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)), StrokeThickness = 3, IsHitTestVisible = false };
        var beamBottom = new System.Windows.Shapes.Line { X1 = 60, Y1 = 230, X2 = 280, Y2 = 230, Stroke = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)), StrokeThickness = 3, IsHitTestVisible = false };
        overlay.Children.Add(beamTop);
        overlay.Children.Add(beamLeft);
        overlay.Children.Add(beamRight);
        overlay.Children.Add(beamBottom);

        // Kích thước dầm chính (Dimension Label)
        var dimLabel = new TextBlock
        {
            Text = "6,000 mm (Dầm D1 300×600)",
            FontSize = 10.5,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(0xFB, 0x92, 0x3C)),
            IsHitTestVisible = false
        };
        Canvas.SetLeft(dimLabel, 110);
        Canvas.SetTop(dimLabel, 60);
        overlay.Children.Add(dimLabel);

        // Kích thước dầm phụ nhịp trục
        var dimSpan = new TextBlock
        {
            Text = "4,500 mm",
            FontSize = 10.5,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(0xFB, 0x92, 0x3C)),
            IsHitTestVisible = false
        };
        Canvas.SetLeft(dimSpan, 290);
        Canvas.SetTop(dimSpan, 150);
        overlay.Children.Add(dimSpan);

        // HUD Viewport Info Pill góc trên bên phải
        var hudPill = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(220, 15, 23, 42)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(120, 56, 189, 248)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10, 4, 10, 4),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(12),
            IsHitTestVisible = false
        };
        var hudText = new TextBlock
        {
            Text = "[Tỷ lệ: 1:100] [Lưới: 500 mm] [Snap: Grid / Endpoint] [WCS]",
            FontSize = 10.5,
            FontWeight = FontWeights.Medium,
            Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8))
        };
        hudPill.Child = hudText;

        // Container stage
        var stageGrid = new Grid();
        stageGrid.Children.Add(viewport);
        stageGrid.Children.Add(hudPill);

        var stage = new Border
        {
            CornerRadius = new CornerRadius(8),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x33, 0x41, 0x55)),
            BorderThickness = new Thickness(1),
            Background = new SolidColorBrush(Color.FromRgb(0x0F, 0x17, 0x2A)),
            ClipToBounds = true,
            Child = stageGrid
        };

        // Telemetry Callout Card
        var statusCallout = CreateCalloutCard("Tọa độ con trỏ: X = 0 mm, Y = 0 mm | Bắt điểm: Sẵn sàng | Di chuyển chuột để xem phản hồi.");

        void UpdateCallout(string msg)
        {
            if (statusCallout.Child is Grid g)
            {
                var tb = g.Children.OfType<TextBlock>().FirstOrDefault(t => Grid.GetColumn(t) == 1);
                if (tb != null) tb.Text = msg;
            }
        }

        viewport.CursorMoved += (_, e) =>
        {
            string snapStatus = e.IsSnapped ? "BÁM ĐIỂM (SNAPPED)" : "Tự do";
            UpdateCallout($"Tọa độ: X = {e.World.X:0.#} mm, Y = {e.World.Y:0.#} mm | Trạng thái: {snapStatus}");
        };

        viewport.WorldClicked += (_, e) =>
        {
            ToastService.Info($"Click tại: X = {e.World.X:0.#}, Y = {e.World.Y:0.#} mm", "Viewport");
        };

        // ---------------------------------------------------------------------
        // 2. Bàn điều khiển tham số 2 cột chuẩn CAD Workstation
        // ---------------------------------------------------------------------
        var workbench = new Grid { Margin = new Thickness(0, 14, 0, 4) };
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(430) });
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Cột 1: Hiển thị lưới & Trục tọa độ
        var col1 = new StackPanel();
        var head1 = new TextBlock
        {
            Text = "LƯỚI & TRỤC TỌA ĐỘ (GRID & DATUM)",
            FontSize = 11.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 10)
        };
        head1.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        col1.Children.Add(head1);

        var gridToggles = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        gridToggles.Children.Add(AppearanceToggle("Hiện lưới (ShowGrid)", viewport.ShowGrid, v => viewport.ShowGrid = v));
        gridToggles.Children.Add(AppearanceToggle("Trục tọa độ (ShowAxes)", viewport.ShowAxes, v => viewport.ShowAxes = v));
        gridToggles.Children.Add(AppearanceToggle("Gốc tọa độ (Origin)", viewport.ShowOriginMarker, v => viewport.ShowOriginMarker = v));
        gridToggles.Children.Add(AppearanceToggle("Tâm ngắm (Crosshair)", viewport.ShowCrosshair, v => viewport.ShowCrosshair = v));
        gridToggles.Children.Add(AppearanceToggle("Ô ngắm (Aperture)", viewport.ShowAperture, v => viewport.ShowAperture = v));
        col1.Children.Add(gridToggles);

        col1.Children.Add(AppearanceSlider("Bước lưới (GridStep)", 100, 2000, viewport.GridStep, 100, "{0:0} mm", v => { viewport.GridStep = v; hudText.Text = $"[Tỷ lệ: 1:100] [Lưới: {v:0} mm] [Snap: Grid / Endpoint] [WCS]"; }));
        col1.Children.Add(AppearanceSlider("Cỡ marker bắt điểm (SnapMarkerSize)", 4, 16, viewport.SnapMarkerSize, 1, "{0:0} px", v => viewport.SnapMarkerSize = v));
        col1.Children.Add(AppearanceSlider("Độ mờ vùng chọn (SelectionOpacity)", 0, 0.4, viewport.SelectionFillOpacity, 0.05, "{0:0.00}", v => viewport.SelectionFillOpacity = v));

        Grid.SetColumn(col1, 0);
        workbench.Children.Add(col1);

        // Cột 2: Tính năng bắt điểm & Thao tác CAD nhanh
        var col2 = new StackPanel();
        var head2 = new TextBlock
        {
            Text = "BẮT ĐIỂM & THAO TÁC VIEWPORT (SNAPPING & ACTIONS)",
            FontSize = 11.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 10)
        };
        head2.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        col2.Children.Add(head2);

        var snapToggles = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        snapToggles.Children.Add(AppearanceToggle("Bám lưới (SnapToGrid)", viewport.SnapToGrid, v => viewport.SnapToGrid = v));
        snapToggles.Children.Add(AppearanceToggle("Hiện nhãn bám (ShowSnapLabel)", viewport.ShowSnapLabel, v => viewport.ShowSnapLabel = v));
        col2.Children.Add(snapToggles);

        // Nút thao tác nhanh
        var actRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
        var btnZoomExtents = SecondaryButton("Zoom Extents");
        btnZoomExtents.Click += (_, _) =>
        {
            viewport.ZoomToFit(new Rect3D(0, 0, 0, 12000, 8000, 0));
            UpdateCallout("Đã thu phóng toàn bộ bản vẽ (Zoom Extents: 12,000 × 8,000 mm).");
        };

        var btnToggleGrid = SecondaryButton("Bật/Tắt Lưới");
        btnToggleGrid.Click += (_, _) =>
        {
            viewport.ToggleGrid();
            UpdateCallout($"Đã chuyển đổi trạng thái lưới: {(viewport.ShowGrid ? "BẬT" : "TẮT")}");
        };

        var btnAddBeam = SecondaryButton("+ Thêm Dầm CAD");
        btnAddBeam.Click += (_, _) =>
        {
            var extraBeam = new System.Windows.Shapes.Line
            {
                X1 = 60, Y1 = 155, X2 = 280, Y2 = 155,
                Stroke = new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E)),
                StrokeThickness = 2.5,
                StrokeDashArray = new DoubleCollection { 4, 2 }
            };
            overlay.Children.Add(extraBeam);
            UpdateCallout("Đã thêm Dầm phụ D2 (300×400) cao độ +3.600m vào bản vẽ.");
        };

        actRow.Children.Add(btnZoomExtents);
        actRow.Children.Add(btnToggleGrid);
        actRow.Children.Add(btnAddBeam);
        col2.Children.Add(actRow);

        col2.Children.Add(statusCallout);

        Grid.SetColumn(col2, 2);
        workbench.Children.Add(col2);

        var stageHost = new StackPanel();
        stageHost.Children.Add(stage);
        stageHost.Children.Add(workbench);

        string xamlCode = @"<ui:ViewportControl Height=""340""
                    GridStep=""500""
                    GridMajorEvery=""5""
                    ShowGrid=""True""
                    ShowCrosshair=""True""
                    ShowAperture=""True""
                    SnapToGrid=""True""
                    ShowAxes=""True""
                    ShowOriginMarker=""True""
                    CursorMoved=""OnCursorMoved""
                    WorldClicked=""OnWorldClicked"" />";

        string csCode = @"// 1. Cấu hình lưới tọa độ & điểm bám CAD
viewport.GridStep = 500;             // Bước lưới chính (500 mm)
viewport.GridMajorEvery = 5;         // Cứ mỗi 5 ô lưới có một đường nét đậm
viewport.SnapToGrid = true;          // Bắt điểm vào lưới
viewport.ShowCrosshair = true;       // Tâm ngắm kỹ thuật
viewport.ShowAperture = true;        // Ô ngắm vuông xung quanh tâm

// 2. Thu phóng vừa khung hình bản vẽ
viewport.ZoomToFit(new Rect3D(0, 0, 0, 12000, 8000, 0));

// 3. Tương tác con trỏ và bám điểm thời gian thực
viewport.CursorMoved += (s, e) =>
{
    Point3D worldPos = e.World;      // Tọa độ thực trong không gian CAD (mm)
    bool isSnapped = e.IsSnapped;    // Đang bám điểm hay tự do
};";

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Khung nhìn kỹ thuật CAD (ViewportControl)",
            stageHost,
            xaml: xamlCode,
            csharp: csCode));

        return root;
    }

    private object CreateCommandLineDemo()
    {
        var commandLine = new CommandLine
        {
            Variant = CommandLineVariant.Docked,
            HistoryHeight = 96,
            ExpandedHistoryHeight = 240
        };

        commandLine.AddCommands(new[]
        {
            new CommandDefinition("LINE", "Vẽ thanh liên tục (Polyline)") { Prompt = "Specify first point" },
            new CommandDefinition("CIRCLE", "Vẽ đường tròn kết cấu").WithAliases("C"),
            new CommandDefinition("ZOOM", "Phóng to / thu nhỏ khung nhìn").WithAliases("Z"),
            new CommandDefinition("SAVE", "Lưu bản vẽ kết cấu CAD").WithAliases("QSAVE"),
            new CommandDefinition("ERASE", "Xóa đối tượng đã chọn").WithAliases("E", "DEL")
        });

        // Bản vẽ tương tác thời gian thực
        var sheet = new Canvas { ClipToBounds = true };

        // Lưới kỹ thuật nền tối CAD
        for (int x = 20; x < 900; x += 40)
        {
            sheet.Children.Add(new System.Windows.Shapes.Line
            {
                X1 = x, Y1 = 0, X2 = x, Y2 = 250,
                Stroke = new SolidColorBrush(Color.FromArgb(0x20, 0x38, 0xBD, 0xF8)),
                StrokeThickness = 1,
                IsHitTestVisible = false
            });
        }
        for (int y = 20; y < 250; y += 40)
        {
            sheet.Children.Add(new System.Windows.Shapes.Line
            {
                X1 = 0, Y1 = y, X2 = 900, Y2 = y,
                Stroke = new SolidColorBrush(Color.FromArgb(0x20, 0x38, 0xBD, 0xF8)),
                StrokeThickness = 1,
                IsHitTestVisible = false
            });
        }

        // Trục kỹ thuật và cột công trình có sẵn
        var committedLine1 = new System.Windows.Shapes.Line { X1 = 60, Y1 = 180, X2 = 300, Y2 = 180, Stroke = new SolidColorBrush(Color.FromRgb(0x02, 0x84, 0xC7)), StrokeThickness = 2.5, IsHitTestVisible = false };
        var committedLine2 = new System.Windows.Shapes.Line { X1 = 60, Y1 = 60, X2 = 300, Y2 = 60, Stroke = new SolidColorBrush(Color.FromRgb(0x02, 0x84, 0xC7)), StrokeThickness = 2.5, IsHitTestVisible = false };
        var committedCol1 = new System.Windows.Shapes.Rectangle { Width = 14, Height = 14, Fill = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)), IsHitTestVisible = false };
        Canvas.SetLeft(committedCol1, 60 - 7); Canvas.SetTop(committedCol1, 180 - 7);
        var committedCol2 = new System.Windows.Shapes.Rectangle { Width = 14, Height = 14, Fill = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)), IsHitTestVisible = false };
        Canvas.SetLeft(committedCol2, 300 - 7); Canvas.SetTop(committedCol2, 180 - 7);

        sheet.Children.Add(committedLine1);
        sheet.Children.Add(committedLine2);
        sheet.Children.Add(committedCol1);
        sheet.Children.Add(committedCol2);

        var sketch = new Polyline { StrokeThickness = 2.5 };
        sketch.SetResourceReference(Shape.StrokeProperty, "CONCOST.Brush.Primary");
        sheet.Children.Add(sketch);

        var points = new List<Point>();

        const double Scale = 0.05;
        const double OriginX = 60;
        const double OriginY = 180;

        Point ToCanvas(Point3D point) => new(OriginX + point.X * Scale, OriginY - point.Y * Scale);

        void Redraw()
        {
            sketch.Points.Clear();
            foreach (var point in points) sketch.Points.Add(point);
        }

        void AskForPoint() => commandLine.BeginPrompt("Specify next point", new[] { "Undo", "Close" });

        commandLine.CommandInvoked += (_, e) =>
        {
            switch (e.Definition.Name)
            {
                case "LINE":
                    points.Clear();
                    points.Add(new Point(60, 180));
                    points.Add(new Point(300, 180));
                    Redraw();
                    commandLine.WriteLine("Vẽ đoạn thẳng liên tục: Nhập tọa độ hoặc click điểm tiếp theo.");
                    AskForPoint();
                    break;

                case "CIRCLE":
                    var circle = new System.Windows.Shapes.Ellipse
                    {
                        Width = 40, Height = 40,
                        Stroke = new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B)),
                        StrokeThickness = 2,
                        Fill = new SolidColorBrush(Color.FromArgb(0x30, 0xF5, 0x9E, 0x0B))
                    };
                    Canvas.SetLeft(circle, 180 - 20);
                    Canvas.SetTop(circle, 120 - 20);
                    sheet.Children.Add(circle);
                    commandLine.WriteLine("Đã vẽ đường tròn CIRCLE bán kính R = 400 mm tại tọa độ (2400, 1200).");
                    break;

                case "ZOOM":
                    commandLine.WriteLine("Khung nhìn: ZOOM [All/Extents/Window] — Đã vừa khớp toàn bộ bản vẽ.");
                    break;

                case "SAVE":
                    commandLine.WriteLine("Đang lưu bản vẽ: Mat-bang-ket-cau-tang-1.dwg...");
                    commandLine.WriteLine("Lưu thành công: 0 cảnh báo, 0 lỗi, kích thước 4.2 MB.");
                    ToastService.Success("Đã lưu bản vẽ kết cấu DWG thành công!", "AutoCAD CLI");
                    break;

                case "ERASE":
                    points.Clear();
                    Redraw();
                    commandLine.WriteLine("Đã xóa các đối tượng sketch đang vẽ.");
                    break;
            }
        };

        commandLine.PromptCompleted += (_, e) =>
        {
            if (e.Prompt == "Specify first point")
            {
                if (!CadInputParser.TryParseCoordinate(e.Text, out Point3D start))
                {
                    commandLine.WriteLine("Yêu cầu tọa độ dạng: x,y (ví dụ: 1000,500)", CommandMessageKind.Error);
                    commandLine.BeginPrompt("Specify first point");
                    return;
                }
                points.Clear();
                points.Add(ToCanvas(start));
                Redraw();
                AskForPoint();
                return;
            }

            if (e.Prompt != "Specify next point") return;

            if (e.Option == "Undo")
            {
                if (points.Count > 0) points.RemoveAt(points.Count - 1);
                Redraw();
                commandLine.WriteLine("*Undo*, đã quay lại đỉnh trước đó.", CommandMessageKind.Hint);
                AskForPoint();
                return;
            }

            if (e.Option == "Close")
            {
                if (points.Count > 2)
                {
                    points.Add(points[0]);
                    Redraw();
                }
                commandLine.WriteLine($"Đã khép góc đa tuyến: {points.Count - 1} đoạn.");
                return;
            }

            if (e.Text.Length == 0)
            {
                commandLine.WriteLine($"Đã hoàn tất lệnh vẽ: {Math.Max(0, points.Count - 1)} đoạn.");
                return;
            }

            if (!CadInputParser.TryParseCoordinate(e.Text, out Point3D next))
            {
                commandLine.WriteLine($"\"{e.Text}\" không phải là tọa độ hợp lệ. Hãy gõ x,y", CommandMessageKind.Error);
                AskForPoint();
                return;
            }

            points.Add(ToCanvas(next));
            Redraw();
            AskForPoint();
        };

        commandLine.UnknownCommand += (_, raw) =>
            commandLine.WriteLine($"Lệnh không nhận dạng được: \"{raw}\". Bấm phím Tab để xem danh sách lệnh.", CommandMessageKind.Error);

        var stage = new Border
        {
            Height = 310,
            CornerRadius = new CornerRadius(8),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x33, 0x41, 0x55)),
            BorderThickness = new Thickness(1),
            ClipToBounds = true,
            Background = new SolidColorBrush(Color.FromRgb(0x0F, 0x17, 0x2A))
        };

        var hudBadge = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(220, 15, 23, 42)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(120, 56, 189, 248)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(8, 4, 8, 4),
            Margin = new Thickness(14, 12, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };
        hudBadge.Child = new TextBlock
        {
            Text = "[BẢN VẼ: MẶT BẰNG KẾT CẤU TẦNG 1] [TỶ LỆ: 1:100] [DÒNG LỆNH: SẴN SÀNG]",
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8))
        };

        var surface = new Grid();
        surface.RowDefinitions.Add(new RowDefinition());
        surface.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var paper = new Grid();
        paper.Children.Add(sheet);
        paper.Children.Add(hudBadge);

        Grid.SetRow(paper, 0);
        Grid.SetRow(commandLine, 1);

        surface.Children.Add(paper);
        surface.Children.Add(commandLine);

        stage.Child = surface;

        // 3. Khay cấu hình tương tác 2 cột chuẩn Desktop CAD
        var workbench = new Grid { Margin = new Thickness(0, 14, 0, 4) };
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(430) });
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Cột 1: Kiểu dáng và kích thước
        var col1 = new StackPanel();
        var head1 = new TextBlock
        {
            Text = "KIỂU DÁNG VÀ LỊCH SỬ (GEOMETRY & HISTORY)",
            FontSize = 11.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 10)
        };
        head1.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        col1.Children.Add(head1);

        var variantRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
        var varLbl = new TextBlock { Text = "Kiểu:", FontSize = 12, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0), Width = 38 };
        varLbl.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        var variantCombo = new ComboBox
        {
            Width = 160,
            ItemsSource = new[] { "Docked (Đáy màn hình)", "Inline (Phẳng)", "Floating (Nổi)" },
            SelectedIndex = 0,
            Margin = new Thickness(0, 0, 16, 0)
        };
        variantCombo.SelectionChanged += (_, _) =>
            commandLine.Variant = (CommandLineVariant)Math.Max(0, variantCombo.SelectedIndex);
        variantRow.Children.Add(varLbl);
        variantRow.Children.Add(variantCombo);
        col1.Children.Add(variantRow);

        col1.Children.Add(AppearanceSlider("Chiều cao lịch sử", 60, 200, commandLine.HistoryHeight, 10, "{0:0} px", v => commandLine.HistoryHeight = v));
        col1.Children.Add(AppearanceSlider("Chiều cao mở rộng", 150, 400, commandLine.ExpandedHistoryHeight, 20, "{0:0} px", v => commandLine.ExpandedHistoryHeight = v));

        Grid.SetColumn(col1, 0);
        workbench.Children.Add(col1);

        // Cột 2: Phím tắt & Thao tác lệnh nhanh
        var col2 = new StackPanel();
        var head2 = new TextBlock
        {
            Text = "LỆNH MẪU & TƯƠNG TÁC NHANH (QUICK COMMANDS)",
            FontSize = 11.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 10)
        };
        head2.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        col2.Children.Add(head2);

        var switches = new WrapPanel { Margin = new Thickness(0, 0, 0, 10) };
        switches.Children.Add(AppearanceToggle("Hiện lịch sử", commandLine.ShowHistory, v => commandLine.ShowHistory = v));
        switches.Children.Add(AppearanceToggle("Mở rộng lịch sử (F2)", commandLine.IsHistoryExpanded, v => commandLine.IsHistoryExpanded = v));
        col2.Children.Add(switches);

        // Nút thực thi lệnh nhanh
        var actRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
        var btnLine = SecondaryButton("Lệnh LINE");
        btnLine.Click += (_, _) => commandLine.Execute("LINE");

        var btnCircle = SecondaryButton("Lệnh CIRCLE");
        btnCircle.Click += (_, _) => commandLine.Execute("CIRCLE");

        var btnZoom = SecondaryButton("Lệnh ZOOM");
        btnZoom.Click += (_, _) => commandLine.Execute("ZOOM");

        var btnSave = SecondaryButton("Lệnh QSAVE");
        btnSave.Click += (_, _) => commandLine.Execute("SAVE");

        var btnErase = SecondaryButton("Lệnh ERASE");
        btnErase.Click += (_, _) => commandLine.Execute("ERASE");

        actRow.Children.Add(btnLine);
        actRow.Children.Add(btnCircle);
        actRow.Children.Add(btnZoom);
        actRow.Children.Add(btnSave);
        actRow.Children.Add(btnErase);
        col2.Children.Add(actRow);

        var cmdCallout = CreateCalloutCard(
            "HƯỚNG DẪN DÒNG LỆNH CAD (COMMAND LINE):\n" +
            "• Gõ lệnh hoặc bấm nút nhanh ở trên: LINE, CIRCLE, ZOOM, SAVE, ERASE.\n" +
            "• Phím Tab: Tự động hoàn thành tên lệnh (Auto-completion).\n" +
            "• Phím Mũi tên Lên / Xuống: Xem lại lịch sử các lệnh đã gõ.\n" +
            "• Phím F2: Phóng to / Thu gọn hộp thoại lịch sử dòng lệnh.");
        col2.Children.Add(cmdCallout);

        Grid.SetColumn(col2, 2);
        workbench.Children.Add(col2);

        var stageHost = new StackPanel();
        stageHost.Children.Add(stage);
        stageHost.Children.Add(workbench);

        string xamlCode = @"<ui:CommandLine Variant=""Docked""
                HistoryHeight=""96""
                ExpandedHistoryHeight=""240""
                ShowHistory=""True""
                CommandInvoked=""OnCommandInvoked""
                PromptCompleted=""OnPromptCompleted"" />";

        string csCode = @"// 1. Đăng ký tập lệnh CAD và phím tắt Alias
commandLine.AddCommands(new[]
{
    new CommandDefinition(""LINE"", ""Vẽ đoạn thẳng liên tục"") { Prompt = ""Specify first point"" },
    new CommandDefinition(""CIRCLE"", ""Vẽ đường tròn"").WithAliases(""C""),
    new CommandDefinition(""ZOOM"", ""Phóng to / thu nhỏ"").WithAliases(""Z""),
    new CommandDefinition(""SAVE"", ""Lưu bản vẽ"").WithAliases(""QSAVE"")
});

// 2. Thực thi lệnh và nhận sự kiện
commandLine.CommandInvoked += (s, e) =>
{
    switch (e.Definition.Name)
    {
        case ""LINE"": commandLine.BeginPrompt(""Specify first point""); break;
        case ""SAVE"": commandLine.WriteLine(""Đã lưu bản vẽ thành công.""); break;
    }
};";

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Dòng lệnh CAD kỹ thuật (CommandLine)",
            stageHost,
            xaml: xamlCode,
            csharp: csCode));

        return root;
    }

    private object CreateDynamicInputDemo()
    {
        const double MmToPx = 0.05;

        // ---------------------------------------------------------------------
        // 1. Khung vẽ kỹ thuật CAD hoàn chỉnh (Realistic CAD Drafting Viewport)
        // ---------------------------------------------------------------------
        var stage = new Border
        {
            Height = 280,
            CornerRadius = new CornerRadius(8),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x33, 0x41, 0x55)),
            BorderThickness = new Thickness(1),
            Background = new SolidColorBrush(Color.FromRgb(0x0F, 0x17, 0x2A)),
            ClipToBounds = true
        };

        var sheet = new Canvas { Height = 280, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch, ClipToBounds = true };

        // 1.1 Lưới kỹ thuật CAD (Blueprint Grid Lines)
        for (int x = 20; x < 900; x += 40)
        {
            sheet.Children.Add(new System.Windows.Shapes.Line
            {
                X1 = x, Y1 = 0, X2 = x, Y2 = 280,
                Stroke = new SolidColorBrush(Color.FromArgb(0x25, 0x38, 0xBD, 0xF8)),
                StrokeThickness = 1,
                IsHitTestVisible = false
            });
        }
        for (int y = 20; y < 280; y += 40)
        {
            sheet.Children.Add(new System.Windows.Shapes.Line
            {
                X1 = 0, Y1 = y, X2 = 900, Y2 = y,
                Stroke = new SolidColorBrush(Color.FromArgb(0x25, 0x38, 0xBD, 0xF8)),
                StrokeThickness = 1,
                IsHitTestVisible = false
            });
        }

        // 1.2 Các đoạn dầm / tường đã vẽ xong trước đó (Pre-committed Structural Walls)
        var wall1 = new System.Windows.Shapes.Line { X1 = 60, Y1 = 220, X2 = 60, Y2 = 80, Stroke = new SolidColorBrush(Color.FromRgb(0x02, 0x84, 0xC7)), StrokeThickness = 2.5, IsHitTestVisible = false };
        var wall2 = new System.Windows.Shapes.Line { X1 = 60, Y1 = 80, X2 = 300, Y2 = 80, Stroke = new SolidColorBrush(Color.FromRgb(0x02, 0x84, 0xC7)), StrokeThickness = 2.5, IsHitTestVisible = false };
        sheet.Children.Add(wall1);
        sheet.Children.Add(wall2);

        // Kích thước đoạn đã vẽ
        var dimCommitted = new TextBlock { Text = "6,000 mm", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8)), IsHitTestVisible = false };
        Canvas.SetLeft(dimCommitted, 155);
        Canvas.SetTop(dimCommitted, 62);
        sheet.Children.Add(dimCommitted);

        // Điểm góc dầm
        void AddVertexDot(double vx, double vy)
        {
            var dot = new System.Windows.Shapes.Rectangle { Width = 6, Height = 6, Fill = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)), IsHitTestVisible = false };
            Canvas.SetLeft(dot, vx - 3);
            Canvas.SetTop(dot, vy - 3);
            sheet.Children.Add(dot);
        }
        AddVertexDot(60, 220);
        AddVertexDot(60, 80);

        // Ký hiệu bắt điểm Snap Marker (Green Square) tại điểm gốc
        var snapMarker = new Border { Width = 10, Height = 10, BorderBrush = new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E)), BorderThickness = new Thickness(1.5), Background = Brushes.Transparent, IsHitTestVisible = false };
        Canvas.SetLeft(snapMarker, 300 - 5);
        Canvas.SetTop(snapMarker, 80 - 5);
        sheet.Children.Add(snapMarker);

        // 1.3 Đoạn đang vẽ trực tiếp (Active Polyline & Rubberband Line)
        var drawn = new Polyline { StrokeThickness = 2.5, Stroke = new SolidColorBrush(Color.FromRgb(0x02, 0x84, 0xC7)), StrokeLineJoin = PenLineJoin.Round, IsHitTestVisible = false };
        var rubber = new System.Windows.Shapes.Line { StrokeThickness = 2, Stroke = new SolidColorBrush(Color.FromRgb(0xFF, 0x79, 0x00)), IsHitTestVisible = false };
        var polarRay = new System.Windows.Shapes.Line { StrokeThickness = 1, Stroke = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)), StrokeDashArray = new DoubleCollection { 4, 3 }, IsHitTestVisible = false };

        sheet.Children.Add(drawn);
        sheet.Children.Add(polarRay);
        sheet.Children.Add(rubber);

        // Nhãn kích thước động dọc theo đường kéo chuột
        var measure = new TextBlock { Text = "3,200 mm", FontSize = 10.5, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(0xFB, 0x92, 0x3C)), IsHitTestVisible = false };
        sheet.Children.Add(measure);

        // Góc nghiêng (Angle Badge)
        var angleBadge = new Border { Background = new SolidColorBrush(Color.FromArgb(0x40, 0xF5, 0x9E, 0x0B)), CornerRadius = new CornerRadius(3), Padding = new Thickness(4, 1, 4, 1), IsHitTestVisible = false };
        var angleText = new TextBlock { Text = "∠ 22.8°", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B)) };
        angleBadge.Child = angleText;
        sheet.Children.Add(angleBadge);

        // Con trỏ chuột Crosshair dấu cộng kỹ thuật
        var crosshairH = new System.Windows.Shapes.Line { Stroke = new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E)), StrokeThickness = 1, IsHitTestVisible = false };
        var crosshairV = new System.Windows.Shapes.Line { Stroke = new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E)), StrokeThickness = 1, IsHitTestVisible = false };
        var pickbox = new System.Windows.Shapes.Rectangle { Width = 6, Height = 6, Stroke = new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E)), StrokeThickness = 1, Fill = Brushes.Transparent, IsHitTestVisible = false };
        sheet.Children.Add(crosshairH);
        sheet.Children.Add(crosshairV);
        sheet.Children.Add(pickbox);

        // 1.4 Hộp DynamicInput thời gian thực
        var input = new DynamicInput
        {
            Kind = DynamicInputKind.Distance,
            Prompt = "Chiều dài",
            Text = "3200",
            FieldWidth = 130,
            Elevation = 1
        };
        sheet.Children.Add(input);

        // 1.5 HUD Overlay góc trên trái & UCS góc dưới trái
        var hudBadge = new Border { Background = new SolidColorBrush(Color.FromArgb(0x50, 0x00, 0x00, 0x00)), CornerRadius = new CornerRadius(4), Padding = new Thickness(8, 4, 8, 4), IsHitTestVisible = false };
        hudBadge.Child = new TextBlock { Text = "[Lệnh: PLINE (Vẽ đa tuyến)] [Chế độ: Dynamic Input (F12)] [Tỉ lệ: 1:100]", FontSize = 10, Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)) };
        Canvas.SetLeft(hudBadge, 12);
        Canvas.SetTop(hudBadge, 10);
        sheet.Children.Add(hudBadge);

        var ucsStack = new StackPanel { Orientation = Orientation.Horizontal, IsHitTestVisible = false };
        ucsStack.Children.Add(new TextBlock { Text = "Y ↑ ", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E)) });
        ucsStack.Children.Add(new TextBlock { Text = "X →", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)) });
        Canvas.SetLeft(ucsStack, 14);
        Canvas.SetTop(ucsStack, 255);
        sheet.Children.Add(ucsStack);

        stage.Child = sheet;

        // Điểm gốc và điểm chuột ban đầu
        Point origin = new(300, 80);
        Point cursor = new(460, 142);

        void UpdateVisuals()
        {
            rubber.X1 = origin.X;
            rubber.Y1 = origin.Y;
            rubber.X2 = cursor.X;
            rubber.Y2 = cursor.Y;

            // Polar ray kéo dài thêm 70px
            double dx = cursor.X - origin.X;
            double dy = cursor.Y - origin.Y;
            double dist = Math.Sqrt((dx * dx) + (dy * dy));
            if (dist > 1)
            {
                polarRay.X1 = cursor.X;
                polarRay.Y1 = cursor.Y;
                polarRay.X2 = cursor.X + (dx / dist) * 70;
                polarRay.Y2 = cursor.Y + (dy / dist) * 70;
                polarRay.Visibility = Visibility.Visible;
            }
            else
            {
                polarRay.Visibility = Visibility.Collapsed;
            }

            // Crosshair
            crosshairH.X1 = cursor.X - 14;
            crosshairH.Y1 = cursor.Y;
            crosshairH.X2 = cursor.X + 14;
            crosshairH.Y2 = cursor.Y;

            crosshairV.X1 = cursor.X;
            crosshairV.Y1 = cursor.Y - 14;
            crosshairV.X2 = cursor.X;
            crosshairV.Y2 = cursor.Y + 14;

            Canvas.SetLeft(pickbox, cursor.X - 3);
            Canvas.SetTop(pickbox, cursor.Y - 3);

            // Dimension text & Angle badge
            double midX = (origin.X + cursor.X) / 2;
            double midY = (origin.Y + cursor.Y) / 2;
            Canvas.SetLeft(measure, midX + 8);
            Canvas.SetTop(measure, midY - 18);

            double angleRad = Math.Atan2(dy, dx);
            double angleDeg = angleRad * (180.0 / Math.PI);
            if (angleDeg < 0) angleDeg += 360;
            angleText.Text = $"∠ {angleDeg:0.0}°";
            Canvas.SetLeft(angleBadge, origin.X + 16);
            Canvas.SetTop(angleBadge, origin.Y + 8);

            Canvas.SetLeft(snapMarker, origin.X - 5);
            Canvas.SetTop(snapMarker, origin.Y - 5);
        }

        UpdateVisuals();
        input.Open(cursor, "Chiều dài", "3200");

        // ---------------------------------------------------------------------
        // 2. Bảng điều khiển tham số 2 cột cân xứng (2-Column Balanced Deck)
        // ---------------------------------------------------------------------
        var controlGrid = new Grid { Margin = new Thickness(0, 14, 0, 10) };
        controlGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        controlGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Cột 0: Kiểu dáng & Hình học
        var colStyle = new StackPanel { Margin = new Thickness(0, 0, 14, 0) };
        colStyle.Children.Add(CadGroupHeader("KIỂU DÁNG & HÌNH HỌC (HUD APPEARANCE)"));

        var comboRow = new Grid { Margin = new Thickness(0, 4, 0, 8) };
        comboRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        comboRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var varStack = new StackPanel { Margin = new Thickness(0, 0, 6, 0) };
        varStack.Children.Add(CadHint("Kiểu hiển thị:"));
        var variantCombo = new ComboBox
        {
            Height = 28,
            ItemsSource = new[] { "Floating (Nổi chuẩn CAD)", "Compact (Gọn 1 dòng)", "Glass (Kính mờ kỹ thuật)" },
            SelectedIndex = 0
        };
        varStack.Children.Add(variantCombo);
        Grid.SetColumn(varStack, 0);
        comboRow.Children.Add(varStack);

        var placeStack = new StackPanel { Margin = new Thickness(6, 0, 0, 0) };
        placeStack.Children.Add(CadHint("Vị trí neo:"));
        var placeCombo = new ComboBox
        {
            Height = 28,
            ItemsSource = new[] { "Tự động (Auto-flip)", "Bên phải (Right)", "Bên trái (Left)", "Phía trên (Above)" },
            SelectedIndex = 0
        };
        placeStack.Children.Add(placeCombo);
        Grid.SetColumn(placeStack, 1);
        comboRow.Children.Add(placeStack);
        colStyle.Children.Add(comboRow);

        colStyle.Children.Add(AppearanceSlider("Rộng ô (FieldWidth)", 90, 220, input.FieldWidth, 10, "{0:0} px", v => input.FieldWidth = v));
        colStyle.Children.Add(AppearanceSlider("Bo góc (CornerRadius)", 0, 16, input.CornerRadius.TopLeft, 1, "{0:0} px", v => input.CornerRadius = new CornerRadius(v)));
        colStyle.Children.Add(AppearanceSlider("Độ nổi bóng (Elevation)", 0, 4, input.Elevation, 1, "{0:0}", v => input.Elevation = (int)v));

        Grid.SetColumn(colStyle, 0);
        controlGrid.Children.Add(colStyle);

        // Cột 1: Tính năng nhập liệu & Chế độ bắt điểm
        var colFeatures = new StackPanel { Margin = new Thickness(14, 0, 0, 0) };
        colFeatures.Children.Add(CadGroupHeader("TÍNH NĂNG NHẬP LIỆU & BẮT ĐIỂM (CAD DRAFTING MODES)"));

        var togglesWrap = new WrapPanel { Margin = new Thickness(0, 4, 0, 10) };
        togglesWrap.Children.Add(AppearanceToggle("Đơn vị (mm)", input.ShowUnit, v => input.ShowUnit = v));
        togglesWrap.Children.Add(AppearanceToggle("Đường gióng con trỏ", input.ShowLeader, v => input.ShowLeader = v));
        togglesWrap.Children.Add(AppearanceToggle("Chọn hết khi mở", input.SelectAllOnOpen, v => input.SelectAllOnOpen = v));
        colFeatures.Children.Add(togglesWrap);

        // Thẻ hiển thị trạng thái thời gian thực
        var statusCard = CreateCalloutCard("Đang vẽ đoạn số 3: Chiều dài L = 3,200 mm | Góc nghiêng α = 22.8° | Gõ chiều dài rồi Enter để hoàn tất.");
        colFeatures.Children.Add(statusCard);

        // Hàng nút thao tác nhanh
        var actRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
        var btnEnter = SecondaryButton("✓ Vẽ đoạn (Enter)");
        var btnSample3200 = SecondaryButton("3,200 mm");
        var btnSampleAngle = SecondaryButton("Góc 45°");
        var btnReset = SecondaryButton("↺ Đặt lại bản vẽ");

        actRow.Children.Add(btnEnter);
        actRow.Children.Add(btnSample3200);
        actRow.Children.Add(btnSampleAngle);
        actRow.Children.Add(btnReset);
        colFeatures.Children.Add(actRow);

        Grid.SetColumn(colFeatures, 1);
        controlGrid.Children.Add(colFeatures);

        // Event hooks
        variantCombo.SelectionChanged += (_, _) =>
        {
            input.Variant = (DynamicInputVariant)Math.Max(0, variantCombo.SelectedIndex);
            statusCard.Child = new TextBlock { Text = $"Đã đổi kiểu DynamicInput sang: {input.Variant}", Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)), FontSize = 11 };
        };

        placeCombo.SelectionChanged += (_, _) =>
        {
            input.Placement = placeCombo.SelectedIndex switch
            {
                1 => DynamicInputPlacement.Right,
                2 => DynamicInputPlacement.Left,
                3 => DynamicInputPlacement.Above,
                _ => DynamicInputPlacement.Auto
            };
            statusCard.Child = new TextBlock { Text = $"Vị trí hiển thị: {input.Placement} (Tự động lật khi chạm mép khung)", Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)), FontSize = 11 };
        };

        // Di chuyển chuột trên Sheet
        sheet.MouseMove += (_, e) =>
        {
            if (input.IsMouseOver) return;
            cursor = e.GetPosition(sheet);

            double dx = cursor.X - origin.X;
            double dy = cursor.Y - origin.Y;
            double lenMm = Math.Sqrt((dx * dx) + (dy * dy)) / MmToPx;
            double angleRad = Math.Atan2(dy, dx);
            double angleDeg = angleRad * (180.0 / Math.PI);
            if (angleDeg < 0) angleDeg += 360;

            measure.Text = $"{lenMm:0} mm";
            UpdateVisuals();

            if (input.IsOpen)
            {
                input.MoveTo(cursor);
                input.Text = $"{lenMm:0}";
            }

            statusCard.Child = new TextBlock { Text = $"Con trỏ: X={cursor.X * 10:0} mm, Y={cursor.Y * 10:0} mm | Chiều dài: {lenMm:0} mm | Góc: {angleDeg:0.0}°", Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)), FontSize = 11 };
        };

        void CommitPoint(Point newPoint)
        {
            drawn.Points.Add(origin);
            drawn.Points.Add(newPoint);
            AddVertexDot(newPoint.X, newPoint.Y);
            origin = newPoint;
            cursor = new Point(newPoint.X + 80, newPoint.Y - 40);
            UpdateVisuals();
            input.Open(cursor, "Chiều dài", "3200");
            ToastService.Success($"Đã vẽ thành công đoạn dầm: {measure.Text}", "DynamicInput");
        }

        btnEnter.Click += (_, _) => CommitPoint(cursor);
        btnSample3200.Click += (_, _) =>
        {
            cursor = new Point(origin.X + 3200 * MmToPx, origin.Y);
            measure.Text = "3,200 mm";
            UpdateVisuals();
            input.MoveTo(cursor);
            input.Text = "3200";
        };
        btnSampleAngle.Click += (_, _) =>
        {
            double rad45 = 45 * Math.PI / 180.0;
            double d = 3200 * MmToPx;
            cursor = new Point(origin.X + d * Math.Cos(rad45), origin.Y + d * Math.Sin(rad45));
            measure.Text = "3,200 mm";
            UpdateVisuals();
            input.MoveTo(cursor);
            input.Text = "3200";
        };
        btnReset.Click += (_, _) =>
        {
            drawn.Points.Clear();
            origin = new Point(300, 80);
            cursor = new Point(460, 142);
            UpdateVisuals();
            input.Open(cursor, "Chiều dài", "3200");
            ToastService.Info("Đã đặt lại bản vẽ về điểm ban đầu.", "DynamicInput");
        };

        input.ValueCommitted += (_, e) =>
        {
            if (e.Number > 0)
            {
                double dx = cursor.X - origin.X;
                double dy = cursor.Y - origin.Y;
                double dist = Math.Sqrt((dx * dx) + (dy * dy));
                if (dist > 1)
                {
                    double targetPx = e.Number * MmToPx;
                    var endPt = new Point(origin.X + (dx / dist) * targetPx, origin.Y + (dy / dist) * targetPx);
                    CommitPoint(endPt);
                }
            }
        };

        var content = new StackPanel();
        content.Children.Add(stage);
        content.Children.Add(controlGrid);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Hộp nhập liệu con trỏ kỹ thuật CAD (DynamicInput)",
            content,
            xaml: @"<ui:ViewportControl x:Name=""Viewport"">
    <ui:ViewportControl.Overlay>
        <ui:DynamicInput x:Name=""LiveInput""
                         Kind=""Distance""
                         Variant=""Floating""
                         Placement=""Auto""
                         UnitSuffix=""(mm)""
                         FieldWidth=""130""
                         Elevation=""1""
                         ValueCommitted=""OnValueCommitted"" />
    </ui:ViewportControl.Overlay>
</ui:ViewportControl>",
            csharp: @"// Mở hộp nhập liệu bám tại toạ độ con trỏ chuột
LiveInput.Open(cursorPoint, ""Chiều dài"", ""3200"");
LiveInput.MoveTo(newPoint);

LiveInput.ValueCommitted += (_, e) =>
{
    // Nhận độ dài số thực và vẽ đoạn thẳng trực tiếp
    double lengthMm = e.Number;
    CommitSegment(lengthMm);
};"));

        return root;
    }

    private static TextBlock CadGroupHeader(string text)
    {
        var tb = new TextBlock
        {
            Text = text,
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 6)
        };
        tb.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.Primary");
        return tb;
    }

    private static Border VariantCard(string caption, Brush? backdrop, UIElement content)
    {
        var frame = new Border
        {
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(12),
            Margin = new Thickness(0, 0, 0, 10)
        };

        frame.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
        frame.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

        if (backdrop is not null) frame.Background = backdrop;

        var label = CadHint(caption);
        label.Margin = new Thickness(0, 0, 0, 8);

        var host = new StackPanel();
        host.Children.Add(label);
        host.Children.Add(content);

        frame.Child = host;

        return frame;
    }

    private object CreateCadStatusBarDemo()
    {
        // 1. Khoi tao CadStatusBar
        var bar = new CadStatusBar
        {
            Variant = CadStatusBarVariant.Floating,
            ControlHeight = 26,
            IconSize = 13,
            CornerRadius = new CornerRadius(6),
            Elevation = 1,
            SelectionCount = 3,
            Coordinates = new Point3D(1250, 800, 0),
            WorkPlane = UcsMode.Top,
            GridEnabled = true,
            ObjectSnapEnabled = true,
            OrthoEnabled = true,
            ShowToggleLabels = true,
            ShowTaskQueue = true,
            ShowNotifications = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(12)
        };

        var statusCallout = CreateCalloutCard("San sang: Bam cac cong tac tren thanh CAD StatusBar de xem phan hoi trang thai thoi gian thuc.");

        void UpdateCallout(string msg)
        {
            if (statusCallout.Child is Grid g)
            {
                var tb = g.Children.OfType<TextBlock>().FirstOrDefault(t => Grid.GetColumn(t) == 1);
                if (tb != null) tb.Text = msg;
            }
        }

        bar.ToggleChanged += (_, e) => UpdateCallout($"Cong tac: {e.Toggle} = {(e.IsEnabled ? "BAT (ON)" : "TAT (OFF)")}");
        bar.WorkPlaneChanged += (_, _) => UpdateCallout($"Mat phang lam viec UCS: {bar.WorkPlane}");
        bar.NotificationSelected += (_, note) =>
        {
            UpdateCallout($"Thong bao duoc chon: {note.Title} — {note.Message}");
            ToastService.Info(note.Message, note.Title);
        };

        int selection = 3;
        var selectBtn = SecondaryButton("Chon them (+1)");
        selectBtn.Click += (_, _) =>
        {
            selection++;
            bar.SelectionCount = selection;
            UpdateCallout($"Da chon: {CadStatusBar.SelectionSummary(selection)}");
        };

        var resetSelectBtn = SecondaryButton("Bo chon (0)");
        resetSelectBtn.Click += (_, _) =>
        {
            selection = 0;
            bar.SelectionCount = selection;
            UpdateCallout("Da bo chon tat ca doi tuong.");
        };

        var addTaskBtn = SecondaryButton("Chay tac vu nen");
        addTaskBtn.Click += (_, _) =>
        {
            var task = new CadTask("Dang boc khoi luong du an", 0);
            bar.AddTask(task);
            UpdateCallout("Bat dau tac vu nen: Dang boc khoi luong...");

            var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(120) };
            timer.Tick += (_, _) =>
            {
                task.Percent += 8;
                bar.RefreshTasks();
                if (task.Percent < 100) return;

                timer.Stop();
                bar.RemoveTask(task);
                bar.Notify(new CadNotification("Boc khoi luong hoan tat", "3 214 cau kien ket cau", CadNotificationSeverity.Success));
                UpdateCallout("Tac vu nen da xong: 3 214 cau kien duoc boc tach thanh cong!");
            };
            timer.Start();
        };

        var notifyBtn = SecondaryButton("Phat thong bao");
        notifyBtn.Click += (_, _) =>
        {
            bar.Notify(new CadNotification("Ban ve da luu thanh cong", "Model_Level18.rvt", CadNotificationSeverity.Info));
            UpdateCallout("Da phat thong bao he thong moi.");
        };

        // 2. CAD Viewport Stage mo phong chan thuc
        var stageCanvas = new Grid();

        var bgGrid = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromRgb(0x13, 0x1B, 0x2A), Color.FromRgb(0x1F, 0x2D, 0x42), 45)
        };
        stageCanvas.Children.Add(bgGrid);

        var cadWatermark = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        cadWatermark.Children.Add(new TextBlock
        {
            Text = "CAD DRAWING VIEWPORT (2D/3D)",
            FontSize = 13,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(160, 148, 163, 184)),
            HorizontalAlignment = HorizontalAlignment.Center
        });
        cadWatermark.Children.Add(new TextBlock
        {
            Text = "Re chuot tren ban ve cap nhat toa do theo thoi gian thuc",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(120, 148, 163, 184)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 4, 0, 0)
        });
        stageCanvas.Children.Add(cadWatermark);

        stageCanvas.MouseMove += (_, e) =>
        {
            var pt = e.GetPosition(stageCanvas);
            bar.UpdateCoordinates(new Point3D(pt.X * 10, (stageCanvas.ActualHeight - pt.Y) * 10, 0));
        };

        stageCanvas.Children.Add(bar);

        var stage = new Border
        {
            Height = 240,
            CornerRadius = new CornerRadius(8),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x33, 0x41, 0x55)),
            BorderThickness = new Thickness(1),
            ClipToBounds = true,
            Child = stageCanvas
        };

        // 3. Khay cau hinh tuong tac 2 cot chuan Desktop
        var workbench = new Grid { Margin = new Thickness(0, 18, 0, 4) };
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(430) });
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Cot 1: Kieu dang va hinh hoc
        var col1 = new StackPanel();
        var head1 = new TextBlock
        {
            Text = "KIEU DANG VA HINH HOC (GEOMETRY & STYLE)",
            FontSize = 11.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 12)
        };
        head1.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        col1.Children.Add(head1);

        var variantRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };
        var varLbl = new TextBlock { Text = "Kieu:", FontSize = 12, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0), Width = 38 };
        varLbl.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        var variantCombo = new ComboBox
        {
            Width = 145,
            ItemsSource = new[] { "Floating (Noi)", "Seamless (Phang)", "Glass (Kinh mo)" },
            SelectedIndex = 0,
            Margin = new Thickness(0, 0, 16, 0)
        };
        variantCombo.SelectionChanged += (_, _) =>
        {
            bar.Variant = (CadStatusBarVariant)Math.Max(0, variantCombo.SelectedIndex);
            UpdateCallout($"Doi kieu hien thi CadStatusBar sang: {bar.Variant}");
        };
        variantRow.Children.Add(varLbl);
        variantRow.Children.Add(variantCombo);
        col1.Children.Add(variantRow);

        col1.Children.Add(AppearanceSlider("Cao nut", 20, 36, bar.ControlHeight, 2, "0", v => bar.ControlHeight = v));
        col1.Children.Add(AppearanceSlider("Co icon", 10, 20, bar.IconSize, 1, "0", v => bar.IconSize = v));
        col1.Children.Add(AppearanceSlider("Bo goc", 0, 16, bar.CornerRadius.TopLeft, 1, "0", v => bar.CornerRadius = new CornerRadius(v)));
        col1.Children.Add(AppearanceSlider("Do noi", 0, 4, bar.Elevation, 1, "0", v => bar.Elevation = (int)v));

        Grid.SetColumn(col1, 0);
        workbench.Children.Add(col1);

        // Cot 2: Tinh nang, tac vu va hanh dong
        var col2 = new StackPanel();
        var head2 = new TextBlock
        {
            Text = "TINH NANG VA TRANG THAI HOAT DONG (FEATURES & STATUS)",
            FontSize = 11.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 12)
        };
        head2.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        col2.Children.Add(head2);

        var switches = new WrapPanel { Margin = new Thickness(0, 0, 0, 10) };
        switches.Children.Add(AppearanceToggle("Tom tat chon", bar.ShowSelection, v => bar.ShowSelection = v));
        switches.Children.Add(AppearanceToggle("Mat phang UCS", bar.ShowWorkPlane, v => bar.ShowWorkPlane = v));
        switches.Children.Add(AppearanceToggle("Cum cong tac", bar.ShowToggles, v => bar.ShowToggles = v));
        switches.Children.Add(AppearanceToggle("Nhan SNAP/ORTHO", bar.ShowToggleLabels, v => bar.ShowToggleLabels = v));
        switches.Children.Add(AppearanceToggle("Tac vu nen", bar.ShowTaskQueue, v => bar.ShowTaskQueue = v));
        switches.Children.Add(AppearanceToggle("Thong bao", bar.ShowNotifications, v => bar.ShowNotifications = v));
        switches.Children.Add(AppearanceToggle("Toa do", bar.ShowCoordinates, v => bar.ShowCoordinates = v));
        switches.Children.Add(AppearanceToggle("Duong phan cach", bar.ShowDividers, v => bar.ShowDividers = v));
        col2.Children.Add(switches);

        var actionsRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };
        selectBtn.Margin = new Thickness(0, 0, 6, 0);
        resetSelectBtn.Margin = new Thickness(0, 0, 6, 0);
        addTaskBtn.Margin = new Thickness(0, 0, 6, 0);
        actionsRow.Children.Add(selectBtn);
        actionsRow.Children.Add(resetSelectBtn);
        actionsRow.Children.Add(addTaskBtn);
        actionsRow.Children.Add(notifyBtn);
        col2.Children.Add(actionsRow);

        col2.Children.Add(statusCallout);

        Grid.SetColumn(col2, 2);
        workbench.Children.Add(col2);

        var stageHost = new StackPanel();
        stageHost.Children.Add(stage);
        stageHost.Children.Add(workbench);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Thanh trang thai CAD (CadStatusBar)",
            stageHost,
            xaml: @"<ui:CadStatusBar Variant=""Floating""
                 ControlHeight=""26"" IconSize=""13""
                 CornerRadius=""6"" Elevation=""1""
                 SelectionCount=""{Binding Count}""
                 Coordinates=""{Binding Cursor}""
                 WorkPlane=""Top""
                 ShowToggleLabels=""True""
                 ShowTaskQueue=""True""
                 ShowNotifications=""True""
                 ToggleChanged=""OnToggleChanged""
                 WorkPlaneChanged=""OnWorkPlaneChanged""
                 NotificationSelected=""OnNotificationSelected"" />",
            csharp: @"// 1. Cap nhat so luong doi tuong dang chon:
bar.SelectionCount = 5;

// 2. Cap nhat toa do theo con tro chuot trong khung ve:
bar.UpdateCoordinates(new Point3D(pt.X, pt.Y, 0));

// 3. Them tac vu nen (Background task voi % tien do):
var task = new CadTask(""Dang boc tach khoi luong"", 45);
bar.AddTask(task);

// 4. Phat thong bao he thong (Toast/Notification):
bar.Notify(new CadNotification(""Ban ve da luu"", ""Level18.rvt"", CadNotificationSeverity.Success));"));

        return root;
    }

    // =========================================================================
    // DEMO: project browser, document workspace, properties palette
    // =========================================================================

    private static void FillProjectTree(ProjectBrowser browser)
    {
        var project = browser.AddRoot("Landmark Tower 2026 (45 Tầng)", ProjectNodeKind.Project);

        var views = project.Add("Mặt Bằng & Khung Nhìn (Views)", ProjectNodeKind.Folder);
        views.Add("Mặt Bằng Tầng 1 (+0.000)", ProjectNodeKind.View).IsModified = true;
        views.Add("Mặt Bằng Tầng 2 (+3.600)", ProjectNodeKind.View);
        views.Add("Mặt Bằng Tầng 18 (+64.800)", ProjectNodeKind.View);
        views.Add("Mặt Đứng Trục 1-12 (South)", ProjectNodeKind.View);
        views.Add("Mặt Cắt Điển Hình A-A", ProjectNodeKind.View);
        views.Add("Khung Không Gian 3D Central", ProjectNodeKind.View);

        var sheets = project.Add("Tập Bản Vẽ Phát Hành (Sheets)", ProjectNodeKind.Sheet);
        sheets.Add("KT-01: Mặt Bằng Tổng Thể 1:500", ProjectNodeKind.Sheet);
        sheets.Add("KC-18: Mặt Bằng Kết Cấu Tầng 18 1:100", ProjectNodeKind.Sheet);
        sheets.Add("MEP-06: Sơ Đồ Cấp Thoát Nước", ProjectNodeKind.Sheet);

        var schedules = project.Add("Bảng Thống Kê & Dự Toán (Schedules)", ProjectNodeKind.Schedule);
        schedules.Add("Tiên Lượng Bê Tông Cốt Thép Tầng 18", ProjectNodeKind.Schedule);
        schedules.Add("Bảng Dự Toán Chi Phí Gói Thầu Kết Cấu", ProjectNodeKind.Schedule);
        schedules.Add("Thống Kê Khối Lượng Thép Hình SS400", ProjectNodeKind.Schedule);

        var families = project.Add("Thư Viện Cấu Kiện BIM (Families)", ProjectNodeKind.Family);
        families.Add("Cột Bê Tông Cốt Thép C600x600", ProjectNodeKind.Family);
        families.Add("Dầm Bê Tông B30 D300x600", ProjectNodeKind.FamilySymbol);
        families.Add("Cửa Đi 2 Cánh Kính Thủy Lực D-0912", ProjectNodeKind.FamilySymbol);

        var levels = project.Add("Cao Độ & Lưới Trục (Levels)", ProjectNodeKind.Level);
        levels.Add("Cao Độ Tầng Hầm B1 (-3.600)", ProjectNodeKind.Level);
        levels.Add("Cao Độ Tầng Trệt L1 (+0.000)", ProjectNodeKind.Level);
        levels.Add("Cao Độ Tầng Điển Hình L18 (+64.800)", ProjectNodeKind.Level);

        browser.Refresh();
    }

    private object CreateProjectBrowserDemo()
    {
        // A browser is a palette, not a page. It is staged at the width it really lives at — 300 px
        // inside a dock — because at page width a tree of six rows is a corner of empty white.
        var browser = new ProjectBrowser
        {
            Width = 300,
            Variant = ProjectBrowserVariant.Card,
            SearchPlaceholder = "Tìm view, sheet, family…",
            AllKindsCaption = "Mọi loại",
            SortCaptions = new[] { "Tên A→Z", "Tên Z→A", "Theo loại" },
            EmptyText = "Không có mục nào khớp bộ lọc.",
            CountCaption = "{0} mục"
        };

        FillProjectTree(browser);

        var readout = CadHint("Mở một view bằng cách bấm đúp; chuột phải để Rename / Duplicate / Delete.");

        browser.OpenRequested += (_, e) =>
        {
            readout.Text = $"Mở {e.Node.Name}";
            ToastService.Info($"Mở {e.Node.Name}", "Project Browser");
        };

        browser.DeleteRequested += (_, e) => readout.Text = $"Xoá {e.Node.Name} (host quyết định)";

        browser.SelectionChanged += (_, _) => readout.Text = $"Đang chọn: {browser.SelectedNode?.Name ?? "(không)"}";

        var stage = new Border { Padding = new Thickness(14), CornerRadius = new CornerRadius(10), VerticalAlignment = VerticalAlignment.Top };
        stage.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
        stage.Child = browser;

        // ---------------------------------------------------------------- appearance panel
        var panel = new StackPanel { Width = 330, Margin = new Thickness(26, 0, 0, 0), VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left };

        var variant = new ComboBox
        {
            Height = 27,
            MinHeight = 0,
            Margin = new Thickness(0, 0, 0, 10),
            ItemsSource = new[] { "Card — browser tự vẽ viền", "Seamless — palette vẽ viền", "Floating — nổi trên bản vẽ" },
            SelectedIndex = 0
        };

        variant.SelectionChanged += (_, _) =>
        {
            browser.Variant = variant.SelectedIndex switch
            {
                1 => ProjectBrowserVariant.Seamless,
                2 => ProjectBrowserVariant.Floating,
                _ => ProjectBrowserVariant.Card
            };

            browser.Elevation = browser.Variant == ProjectBrowserVariant.Floating ? 3 : 0;
        };

        panel.Children.Add(variant);
        panel.Children.Add(AppearanceToggle("Thanh công cụ", true, value => browser.ShowToolbar = value));
        panel.Children.Add(AppearanceToggle("Ô tìm kiếm", true, value => browser.ShowSearch = value));
        panel.Children.Add(AppearanceToggle("Lọc theo loại", true, value => browser.ShowKindFilter = value));
        panel.Children.Add(AppearanceToggle("Sắp xếp", true, value => browser.ShowSort = value));
        panel.Children.Add(AppearanceToggle("Nhóm theo loại", true, value => browser.ShowGroup = value));
        panel.Children.Add(AppearanceToggle("Dòng đếm", true, value => browser.ShowCount = value));
        panel.Children.Add(AppearanceSlider("Cao nút", 18, 34, 24, 1, "{0:0} px", value => browser.ControlHeight = value));
        panel.Children.Add(AppearanceSlider("Cỡ icon", 10, 18, 13, 1, "{0:0} px", value => browser.IconSize = value));
        panel.Children.Add(AppearanceSlider("Bo góc", 0, 14, 8, 1, "{0:0} px", value => browser.CornerRadius = new CornerRadius(value)));
        panel.Children.Add(AppearanceSlider("Độ nổi", 0, 4, 0, 1, "{0:0}", value => browser.Elevation = (int)Math.Round(value)));

        var filterEmpty = new Button
        {
            Content = "Bộ lọc không khớp",
            Height = 28,
            MinHeight = 0,
            Padding = new Thickness(12, 0, 12, 0),
            Margin = new Thickness(0, 4, 8, 0)
        };
        filterEmpty.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        filterEmpty.Click += (_, _) => browser.SetSearchText("không có gì");

        var filterClear = new Button
        {
            Content = "Xoá lọc",
            Height = 28,
            MinHeight = 0,
            Padding = new Thickness(12, 0, 12, 0),
            Margin = new Thickness(0, 4, 0, 0)
        };
        filterClear.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        filterClear.Click += (_, _) => browser.SetSearchText(string.Empty);

        var filterRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 12, 0, 0) };
        filterRow.Children.Add(filterEmpty);
        filterRow.Children.Add(filterClear);

        panel.Children.Add(filterRow);
        panel.Children.Add(readout);

        // ---------------------------------------------------------------- what the rebuild changed
        // A palette staged at 300 px leaves half a page card empty, so the right third carries the
        // four rules the page is arguing for instead of white.
        var rules = new StackPanel { Width = 300 };

        var rulesTitle = new TextBlock { Text = "Bốn quy tắc của trang này", FontSize = 12, FontWeight = FontWeights.SemiBold };
        rulesTitle.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        rules.Children.Add(rulesTitle);

        rules.Children.Add(RuleRow("Một tấm duy nhất", "Variant quyết định ai vẽ viền — browser hay palette chứa nó."));
        rules.Children.Add(RuleRow("Nút không có khung", "Bốn chip 26 px thành icon ghost, vạch phân cách rời đi cùng nhóm."));
        rules.Children.Add(RuleRow("Sàn 24 px, không phải 34", "ComboBox.Base đặt MinHeight 34 — ApplyControlHeight đưa nó về 0 trước khi gán cao."));
        rules.Children.Add(RuleRow("Chữ dịch được", "Placeholder, caption, empty text và dòng đếm đều là property."));

        var rulesCard = new Border
        {
            Child = rules,
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(16, 14, 16, 16),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top
        };
        rulesCard.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");

        var layout = new Grid();
        layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        Grid.SetColumn(stage, 0);
        layout.Children.Add(stage);

        Grid.SetColumn(panel, 1);
        layout.Children.Add(panel);

        Grid.SetColumn(rulesCard, 2);
        layout.Children.Add(rulesCard);

        // ---------------------------------------------------------------- the three dresses
        var gallery = new StackPanel { Orientation = Orientation.Horizontal };

        var card = new ProjectBrowser { Width = 268, Height = 300, Variant = ProjectBrowserVariant.Card, ShowCount = false };
        FillProjectTree(card);

        // Seamless inside a palette that draws the edge: the browser contributes no plate of its own.
        var seamless = new ProjectBrowser { Height = 300, Variant = ProjectBrowserVariant.Seamless, ShowCount = false };
        FillProjectTree(seamless);

        var palette = new Border { CornerRadius = new CornerRadius(8), BorderThickness = new Thickness(1), Padding = new Thickness(6), Child = seamless };
        palette.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
        palette.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
        palette.Width = 268;

        var floating = new ProjectBrowser
        {
            Width = 268,
            Height = 300,
            Variant = ProjectBrowserVariant.Floating,
            Elevation = 3,
            CornerRadius = new CornerRadius(14),
            ShowCount = false
        };
        FillProjectTree(floating);

        var gradient = new LinearGradientBrush(Color.FromRgb(24, 28, 36), Color.FromRgb(52, 62, 78), 45);

        gallery.Children.Add(VariantCard("Card — một viền, browser tự vẽ", null, card));
        gallery.Children.Add(VariantCard("Seamless — palette vẽ viền", null, palette));
        gallery.Children.Add(VariantCard("Floating — nổi trên bản vẽ", gradient, floating));

        var root = new StackPanel();

        root.Children.Add(DemoSection(
            "Cửa sổ dự án",
            layout,
            xaml: "<ui:ProjectBrowser Variant=\"Card\" ControlHeight=\"24\" IconSize=\"13\"\n" +
                  "                    SearchPlaceholder=\"Search the project\"\n" +
                  "                    AllKindsCaption=\"All kinds\"\n" +
                  "                    CountCaption=\"{0} items\"\n" +
                  "                    EmptyText=\"Nothing matches this search.\"\n" +
                  "                    OpenRequested=\"OnOpenView\" />",
            csharp: "var project = browser.AddRoot(\"Building B\", ProjectNodeKind.Project);\n" +
                    "project.Add(\"Level 1\", ProjectNodeKind.View);\n\n" +
                    "browser.Refresh();\n" +
                    "browser.ExpandAll();\n\n" +
                    "// Lọc giữ nguyên đường dẫn tới node khớp\n" +
                    "browser.SetSearchText(\"lev\");\n\n" +
                    "// Nhóm theo loại, sắp xếp theo tên hoặc loại\n" +
                    "browser.GroupMode = ProjectGroupMode.ByKind;\n" +
                    "browser.SortMode = ProjectSortMode.NameAscending;\n\n" +
                    "// Mọi nhãn đều là thuộc tính, nên palette dịch được\n" +
                    "browser.AllKindsCaption = \"Mọi loại\";\n" +
                    "browser.CountCaption = \"{0} mục\";"));

        root.Children.Add(DemoSection(
            "Ba kiểu dáng, ba bối cảnh",
            gallery,
            xaml: "<ui:ProjectBrowser Variant=\"Seamless\" />",
            csharp: "browser.Variant = ProjectBrowserVariant.Card;      // tự vẽ một viền\n" +
                    "browser.Variant = ProjectBrowserVariant.Seamless;  // palette đã có viền\n" +
                    "browser.Variant = ProjectBrowserVariant.Floating;  // nổi, có bóng\n\n" +
                    "browser.ShowCount  = false;   // dòng đếm rút theo viền của nó\n" +
                    "browser.ShowSearch = false;   // cả header rút nếu không còn gì"));

        return root;
    }

    private object CreateDocumentWorkspaceDemo()
    {
        var workspace = new DocumentWorkspace
        {
            Height = 350,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        // Tab 1: Mặt Bằng Kết Cấu Tầng 18 (2D CAD Floor Plan Grid)
        var planCanvas = new Canvas { Background = new SolidColorBrush(Color.FromRgb(0x0F, 0x17, 0x2A)), ClipToBounds = true };
        var gridBrush = new SolidColorBrush(Color.FromArgb(0x40, 0x38, 0xBD, 0xF8));
        var axisBrush = new SolidColorBrush(Color.FromArgb(0x90, 0x38, 0xBD, 0xF8));
        for (int r = 0; r < 3; r++)
        {
            double y = 50 + r * 60;
            planCanvas.Children.Add(new System.Windows.Shapes.Line { X1 = 30, Y1 = y, X2 = 480, Y2 = y, Stroke = gridBrush, StrokeDashArray = new DoubleCollection { 4, 2 } });
            var bubble = new Border { Width = 20, Height = 20, CornerRadius = new CornerRadius(10), BorderBrush = axisBrush, BorderThickness = new Thickness(1), Background = new SolidColorBrush(Color.FromRgb(0x1E, 0x29, 0x3B)) };
            bubble.Child = new TextBlock { Text = ((char)('A' + r)).ToString(), FontSize = 10, FontWeight = FontWeights.Bold, Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            Canvas.SetLeft(bubble, 10);
            Canvas.SetTop(bubble, y - 10);
            planCanvas.Children.Add(bubble);
        }
        for (int c = 0; c < 5; c++)
        {
            double x = 70 + c * 90;
            planCanvas.Children.Add(new System.Windows.Shapes.Line { X1 = x, Y1 = 30, X2 = x, Y2 = 210, Stroke = gridBrush, StrokeDashArray = new DoubleCollection { 4, 2 } });
            var bubble = new Border { Width = 20, Height = 20, CornerRadius = new CornerRadius(10), BorderBrush = axisBrush, BorderThickness = new Thickness(1), Background = new SolidColorBrush(Color.FromRgb(0x1E, 0x29, 0x3B)) };
            bubble.Child = new TextBlock { Text = (c + 1).ToString(), FontSize = 10, FontWeight = FontWeights.Bold, Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            Canvas.SetLeft(bubble, x - 10);
            Canvas.SetTop(bubble, 8);
            planCanvas.Children.Add(bubble);

            for (int r = 0; r < 3; r++)
            {
                double y = 50 + r * 60;
                var col = new Border { Width = 14, Height = 14, BorderBrush = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)), BorderThickness = new Thickness(1.5), Background = new SolidColorBrush(Color.FromArgb(0x40, 0x02, 0x84, 0xC7)) };
                Canvas.SetLeft(col, x - 7);
                Canvas.SetTop(col, y - 7);
                planCanvas.Children.Add(col);
            }
        }
        var planTag = new Border { Background = new SolidColorBrush(Color.FromArgb(0xA0, 0x00, 0x00, 0x00)), CornerRadius = new CornerRadius(4), Padding = new Thickness(8, 4, 8, 4) };
        planTag.Child = new TextBlock { Text = "📐 Mặt Bằng Kết Cấu Tầng 18 | Tỉ lệ 1:100", FontSize = 11, Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)) };
        Canvas.SetLeft(planTag, 20);
        Canvas.SetTop(planTag, 215);
        planCanvas.Children.Add(planTag);
        workspace.AddDocument("Mặt Bằng Tầng 18", planCanvas, IconKind.FileCAD);

        // Tab 2: Khung Không Gian 3D (3D Structural Wireframe View)
        var viewer3D = new ModelViewer3D
        {
            ShowAxes = true,
            Zoom = 1.0,
            EdgeBrush = new SolidColorBrush(Color.FromRgb(0x02, 0x84, 0xC7)),
            Cursor = Cursors.SizeAll
        };
        var verts = new List<Point3D>
        {
            new(-4, -4, -3), new(4, -4, -3), new(4, 4, -3), new(-4, 4, -3),
            new(-4, -4, 0),  new(4, -4, 0),  new(4, 4, 0),  new(-4, 4, 0),
            new(-4, -4, 3),  new(4, -4, 3),  new(4, 4, 3),  new(-4, 4, 3),
            new(0, 0, 5.5)
        };
        var edges = new List<MeshEdge>
        {
            new(0, 1), new(1, 2), new(2, 3), new(3, 0),
            new(4, 5), new(5, 6), new(6, 7), new(7, 4),
            new(8, 9), new(9, 10), new(10, 11), new(11, 8),
            new(0, 4), new(1, 5), new(2, 6), new(3, 7),
            new(4, 8), new(5, 9), new(6, 10), new(7, 11),
            new(8, 12), new(9, 12), new(10, 12), new(11, 12)
        };
        viewer3D.SetMesh(verts, edges);
        workspace.AddDocument("Khung Không Gian 3D", viewer3D, IconKind.Viewport3D);

        // Tab 3: Bảng Tiên Lượng Khối Lượng & Chi Phí (Cost Schedule)
        var scheduleBorder = new Border { Background = new SolidColorBrush(Color.FromRgb(0x0F, 0x17, 0x2A)), Padding = new Thickness(14) };
        var scheduleStack = new StackPanel();
        var scheduleHeader = new TextBlock { Text = "📊 BẢNG TIÊN LƯỢNG KHỐI LƯỢNG & DỰ TOÁN KẾT CẤU (TCVN 2026)", FontSize = 12, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)), Margin = new Thickness(0, 0, 0, 10) };
        scheduleStack.Children.Add(scheduleHeader);

        var tableGrid = new Grid();
        tableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        tableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
        tableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        tableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
        tableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
        tableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });

        string[] headers = { "Mã CK", "Tiết diện", "Dài (m)", "Bê tông (m³)", "Thép (kg)", "Thành tiền (đ)" };
        tableGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        for (int h = 0; h < headers.Length; h++)
        {
            var th = new TextBlock { Text = headers[h], FontSize = 11, FontWeight = FontWeights.SemiBold, Foreground = new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8)), Margin = new Thickness(4, 4, 4, 8) };
            Grid.SetRow(th, 0);
            Grid.SetColumn(th, h);
            tableGrid.Children.Add(th);
        }

        string[][] rowData = {
            new[] { "D1-T18", "300 × 600", "7.5", "1.35 m³", "185.0 kg", "8,500,000 đ" },
            new[] { "D2-T18", "250 × 500", "6.0", "0.75 m³", "95.5 kg", "4,350,000 đ" },
            new[] { "C1-T18", "600 × 600", "3.6", "1.30 m³", "240.0 kg", "11,200,000 đ" },
            new[] { "C2-T18", "500 × 500", "3.6", "0.90 m³", "165.0 kg", "7,800,000 đ" }
        };
        for (int r = 0; r < rowData.Length; r++)
        {
            tableGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (int c = 0; c < rowData[r].Length; c++)
            {
                var td = new TextBlock { Text = rowData[r][c], FontSize = 11, Foreground = c == 0 ? new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)) : (c == 5 ? new SolidColorBrush(Color.FromRgb(0x34, 0xD3, 0x99)) : Brushes.White), Margin = new Thickness(4, 4, 4, 4) };
                Grid.SetRow(td, r + 1);
                Grid.SetColumn(td, c);
                tableGrid.Children.Add(td);
            }
        }
        scheduleStack.Children.Add(tableGrid);

        var totalBadge = new Border { Background = new SolidColorBrush(Color.FromArgb(0x30, 0x34, 0xD3, 0x99)), CornerRadius = new CornerRadius(4), Padding = new Thickness(10, 6, 10, 6), Margin = new Thickness(0, 12, 0, 0), HorizontalAlignment = HorizontalAlignment.Left };
        totalBadge.Child = new TextBlock { Text = "💰 TỔNG GIÁ TRỊ: 31,850,000 VNĐ  |  Bê tông: 4.30 m³  |  Thép: 685.5 kg", FontSize = 11, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(0x34, 0xD3, 0x99)) };
        scheduleStack.Children.Add(totalBadge);

        scheduleBorder.Child = scheduleStack;
        workspace.AddDocument("Dự Toán Chi Phí A101", scheduleBorder, IconKind.FileText);

        var readout = CadHint($"{workspace.Documents.Count} cửa sổ làm việc đồng thời — chế độ hiển thị {workspace.TileMode}");

        workspace.DocumentClosed += (_, pane) => readout.Text = $"Đã đóng cửa sổ: {pane.Title}";

        workspace.DocumentClosing += (_, e) =>
        {
            if (!e.Pane.IsDirty) return;
            e.Cancel = true;
            ToastService.Warning($"{e.Pane.Title} còn thay đổi chưa lưu.", "Document Workspace");
        };

        var tiles = new ComboBox
        {
            Width = 160,
            Height = 28,
            ItemsSource = new[] { "Single (Đơn lẻ)", "Vertical (Chia dọc)", "Horizontal (Chia ngang)", "Grid (Lưới 2x2)" },
            SelectedIndex = 0
        };

        tiles.SelectionChanged += (_, _) =>
        {
            workspace.TileMode = (DocumentTileMode)tiles.SelectedIndex;
            readout.Text = $"Chế độ chia cửa sổ: {workspace.TileMode} — {workspace.PaneCount} khung nhìn";
        };

        var closeAll = SecondaryButton("Đóng tất cả");
        closeAll.Click += (_, _) => workspace.CloseAll();

        var dirty = SecondaryButton("Đánh dấu chưa lưu (*)");
        dirty.Click += (_, _) =>
        {
            foreach (var pane in workspace.Documents) pane.IsDirty = true;
            readout.Text = "Tất cả cửa sổ đang ở trạng thái chưa lưu (*) — Thử đóng tab để kiểm tra chặn đóng an toàn.";
        };

        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
        buttons.Children.Add(tiles);
        buttons.Children.Add(closeAll);
        buttons.Children.Add(dirty);

        var content = new StackPanel();
        content.Children.Add(workspace);
        content.Children.Add(buttons);
        content.Children.Add(readout);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Vùng cửa sổ tài liệu CAD/BIM đa khung (DocumentWorkspace)",
            content,
            xaml: @"<ui:DocumentWorkspace x:Name=""Workspace"" Height=""350""
                      DocumentClosing=""OnDocumentClosing"" />",
            csharp: @"// Thêm các tài liệu đa dạng: Mặt bằng 2D, Khung 3D, Dự toán
workspace.AddDocument(""Mặt Bằng Tầng 18"", planCanvas, IconKind.FileCAD);
workspace.AddDocument(""Khung 3D"", viewer3D, IconKind.Viewport3D);
workspace.AddDocument(""Dự Toán A101"", scheduleGrid, IconKind.FileText);

// Thay đổi bố cục hiển thị cửa sổ (Tile Mode)
workspace.TileMode = DocumentTileMode.Vertical;"));

        return root;
    }

    private object CreatePropertiesPaletteDemo()
    {
        var undo = new UndoRedoManager();
        var properties = new PropertyManager(new TransactionManager(undo));
        var selection = new SelectionManager();

        var beam = new CadDemoBeam { Length = 6000, Height = 400, Material = "Bê tông B25" };

        properties.Register<CadDemoBeam>(
            new PropertyDescriptor("Length", target => ((CadDemoBeam)target).Length, (target, value) => ((CadDemoBeam)target).Length = Convert.ToDouble(value))
            {
                DisplayName = "Chiều dài (L)",
                Category = "Kích thước hình học",
                Editor = PropertyEditorKind.Numeric,
                Unit = "mm",
                DefaultValue = 6000.0,
                Validator = (_, value) => value is double length && length > 0 ? null : "Chiều dài phải dương."
            },
            new PropertyDescriptor("Height", target => ((CadDemoBeam)target).Height, (target, value) => ((CadDemoBeam)target).Height = Convert.ToDouble(value))
            {
                DisplayName = "Chiều cao tiết diện (H)",
                Category = "Kích thước hình học",
                Editor = PropertyEditorKind.Numeric,
                Unit = "mm",
                DefaultValue = 400.0
            },
            new PropertyDescriptor("Material", target => ((CadDemoBeam)target).Material, (target, value) => ((CadDemoBeam)target).Material = value as string ?? string.Empty)
            {
                DisplayName = "Cấp độ bền bê tông",
                Category = "Vật liệu xây dựng",
                Editor = PropertyEditorKind.Choice,
                Choices = new object[] { "Bê tông B25", "Bê tông B30", "Bê tông B35", "Thép hình SS400" },
                DefaultValue = "Bê tông B25"
            },
            new PropertyDescriptor("IsLocked", target => ((CadDemoBeam)target).IsLocked)
            {
                DisplayName = "Khoá vị trí cấu kiện",
                Category = "Trạng thái kỹ thuật",
                Editor = PropertyEditorKind.Boolean
            });

        selection.Select(beam);

        // Workbench 2 cột: Trái là CAD Beam Viewport, Phải là PropertiesPalette
        var workbench = new Grid { Height = 340, Margin = new Thickness(0, 0, 0, 10) };
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(330) });

        // CAD Drawing Viewport (Trái)
        var drawingBorder = new Border
        {
            Background = new LinearGradientBrush(Color.FromRgb(0x0F, 0x17, 0x2A), Color.FromRgb(0x09, 0x0E, 0x1A), 45),
            CornerRadius = new CornerRadius(8),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x33, 0x41, 0x55)),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 10, 0),
            ClipToBounds = true
        };

        var drawingGrid = new Grid();
        var beamCanvas = new Canvas { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Width = 380, Height = 180 };

        var beamRect = new System.Windows.Shapes.Rectangle
        {
            Width = 260,
            Height = 60,
            Stroke = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)),
            StrokeThickness = 2,
            Fill = new SolidColorBrush(Color.FromArgb(0x30, 0x02, 0x84, 0xC7))
        };
        Canvas.SetLeft(beamRect, 60);
        Canvas.SetTop(beamRect, 60);
        beamCanvas.Children.Add(beamRect);

        // Rebar lines (Thép chủ trên và dưới)
        var topRebar = new System.Windows.Shapes.Line { X1 = 65, Y1 = 68, X2 = 315, Y2 = 68, Stroke = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)), StrokeThickness = 2 };
        var botRebar = new System.Windows.Shapes.Line { X1 = 65, Y1 = 112, X2 = 315, Y2 = 112, Stroke = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)), StrokeThickness = 2 };
        beamCanvas.Children.Add(topRebar);
        beamCanvas.Children.Add(botRebar);

        // Kích thước (Dimension Lines)
        var dimLen = new TextBlock { Text = $"L = {beam.Length:0} mm", FontSize = 11, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B)) };
        Canvas.SetLeft(dimLen, 160);
        Canvas.SetTop(dimLen, 38);
        beamCanvas.Children.Add(dimLen);

        var dimH = new TextBlock { Text = $"H={beam.Height:0}", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B)) };
        Canvas.SetLeft(dimH, 12);
        Canvas.SetTop(dimH, 82);
        beamCanvas.Children.Add(dimH);

        drawingGrid.Children.Add(beamCanvas);

        // Header badge on drawing
        var headerBadge = new Border { Background = new SolidColorBrush(Color.FromArgb(0x40, 0x00, 0x00, 0x00)), CornerRadius = new CornerRadius(4), Padding = new Thickness(8, 4, 8, 4), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(10) };
        var headerText = new TextBlock { Text = $"ĐANG CHỌN: DẦM CHÍNH D1 — {beam.Material}", FontSize = 11, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)) };
        headerBadge.Child = headerText;
        drawingGrid.Children.Add(headerBadge);

        drawingBorder.Child = drawingGrid;
        Grid.SetColumn(drawingBorder, 0);
        workbench.Children.Add(drawingBorder);

        // Palette (Phải)
        var palette = new PropertiesPalette(properties, selection) { Height = 340 };
        palette.Build(beam);
        Grid.SetColumn(palette, 1);
        workbench.Children.Add(palette);

        // Khi sửa thuộc tính -> Cập nhật kích thước đồ họa tức thì
        var readout = CadHint($"Thuộc tính cấu kiện: Chiều dài {beam.Length} mm, Chiều cao {beam.Height} mm, Vật liệu: {beam.Material}");
        palette.PropertyCommitted += (_, e) =>
        {
            readout.Text = $"Đã cập nhật {e.Property.DisplayName}: {e.OldValue} → {e.NewValue}";
            dimLen.Text = $"L = {beam.Length:0} mm";
            dimH.Text = $"H={beam.Height:0}";
            headerText.Text = $"ĐANG CHỌN: DẦM CHÍNH D1 — {beam.Material}";
            
            // Dynamic visual scale
            double newW = Math.Max(120, Math.Min(300, beam.Length * 0.045));
            double newH = Math.Max(30, Math.Min(90, beam.Height * 0.15));
            beamRect.Width = newW;
            beamRect.Height = newH;
            topRebar.X2 = 60 + newW - 5;
            botRebar.X2 = 60 + newW - 5;
            botRebar.Y1 = 60 + newH - 8;
            botRebar.Y2 = 60 + newH - 8;
        };

        var undoButton = SecondaryButton("Hoàn tác (Undo)");
        undoButton.Click += (_, _) =>
        {
            undo.Undo();
            palette.Refresh();
            readout.Text = $"Đã hoàn tác — Chiều dài dầm hiện tại: {beam.Length:0} mm";
            dimLen.Text = $"L = {beam.Length:0} mm";
            dimH.Text = $"H={beam.Height:0}";
            double newW = Math.Max(120, Math.Min(300, beam.Length * 0.045));
            double newH = Math.Max(30, Math.Min(90, beam.Height * 0.15));
            beamRect.Width = newW;
            beamRect.Height = newH;
        };

        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 8, 0, 0) };
        buttons.Children.Add(undoButton);

        var content = new StackPanel();
        content.Children.Add(workbench);
        content.Children.Add(buttons);
        content.Children.Add(readout);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Bảng thuộc tính kỹ thuật CAD & BIM (PropertiesPalette)",
            content,
            xaml: @"<ui:PropertiesPalette x:Name=""Palette"" Height=""340""
                       PropertyCommitted=""OnPropertyCommitted"" />",
            csharp: @"// Đăng ký thuộc tính kỹ thuật phân loại theo Category
properties.Register<CadDemoBeam>(
    new PropertyDescriptor(""Length"", b => ((CadDemoBeam)b).Length, (b, v) => ((CadDemoBeam)b).Length = Convert.ToDouble(v))
    { DisplayName = ""Chiều dài"", Category = ""Kích thước"", Unit = ""mm"" });

// Thay đổi thuộc tính tự động đồng bộ đồ họa mô hình và hoàn tác Undo
palette.Build(beam);"));

        return root;
    }

    // =========================================================================
    // DEMO: title bar, info bar, backstage, font picker
    // =========================================================================

    private object CreateApplicationTitleBarDemo()
    {
        // ---------------------------------------------------------------------
        // 1. TitleBar Instance & Configuration
        // ---------------------------------------------------------------------
        var titleBar = new ApplicationTitleBar
        {
            ApplicationName = "CONCOST BIM Suite 2026",
            DocumentTitle = "Landmark Tower — Mặt Bằng Kết Cấu Tầng 18",
            Subtitle = "RVT v2026.2",
            IsDocumentModified = false,
            ShowQuickAccess = true,
            ShowWindowButtons = true,
            ShowAppIcon = true,
            ShowSearchBox = true,
            ShowUserAccount = true,
            UserAccountName = "KTS. Lê Hoàng Minh",
            TitleAlignment = HorizontalAlignment.Center,
            TitleForeground = Brushes.White,
            AppIconBrush = new SolidColorBrush(Color.FromRgb(0xFB, 0x92, 0x3C)),
            Height = 38
        };

        // Dải nút thao tác nhanh mặc định
        titleBar.QuickAccess.Add(new TitleBarAction(IconKind.Undo, "Hoàn tác (Ctrl+Z)", null));
        titleBar.QuickAccess.Add(new TitleBarAction(IconKind.Redo, "Làm lại (Ctrl+Y)", null));
        titleBar.QuickAccess.Add(new TitleBarAction(IconKind.Save, "Lưu dự án (Ctrl+S)", null));
        titleBar.QuickAccess.Add(new TitleBarAction(IconKind.Folder, "Mở bản vẽ (Ctrl+O)", null));
        titleBar.QuickAccess.Add(new TitleBarAction(IconKind.Print, "In ấn xuất PDF (Ctrl+P)", null));
        titleBar.RefreshQuickAccess();

        // ---------------------------------------------------------------------
        // 2. Mô phỏng Cửa sổ Ứng dụng CAD hoàn chỉnh (Full-Width CAD Window Mockup)
        // ---------------------------------------------------------------------
        var windowMockup = new Border
        {
            Height = 280,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            CornerRadius = new CornerRadius(10),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x47, 0x55, 0x69)),
            Background = new SolidColorBrush(Color.FromRgb(0x0F, 0x17, 0x2A)),
            ClipToBounds = true
        };

        var windowGrid = new Grid();
        windowGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 0: TitleBar
        windowGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 1: Ribbon Tab Strip
        windowGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // 2: Viewport / Workspace
        windowGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 3: Status Strip

        // Add TitleBar to Row 0
        Grid.SetRow(titleBar, 0);
        windowGrid.Children.Add(titleBar);

        // Row 1: Mockup Ribbon Tabs
        var ribbonStrip = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(0x1E, 0x29, 0x3B)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x33, 0x41, 0x55)),
            BorderThickness = new Thickness(0, 1, 0, 1),
            Padding = new Thickness(10, 4, 10, 4)
        };

        var ribbonTabs = new StackPanel { Orientation = Orientation.Horizontal };
        string[] tabs = { "Tệp (File)", "Kiến Trúc", "Kết Cấu", "Hệ Thống MEP", "Khối Lượng & Chi Phí", "Phân Tích Tải Trọng", "Cài Đặt" };
        for (int i = 0; i < tabs.Length; i++)
        {
            var tabBtn = new Border
            {
                Padding = new Thickness(10, 4, 10, 4),
                Margin = new Thickness(2, 0, 2, 0),
                CornerRadius = new CornerRadius(4),
                Background = i == 2 ? new SolidColorBrush(Color.FromRgb(0x02, 0x84, 0xC7)) : Brushes.Transparent,
                Cursor = System.Windows.Input.Cursors.Hand
            };
            tabBtn.Child = new TextBlock
            {
                Text = tabs[i],
                FontSize = 11,
                FontWeight = i == 2 ? FontWeights.SemiBold : FontWeights.Normal,
                Foreground = i == 2 ? Brushes.White : new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8))
            };
            ribbonTabs.Children.Add(tabBtn);
        }
        ribbonStrip.Child = ribbonTabs;
        Grid.SetRow(ribbonStrip, 1);
        windowGrid.Children.Add(ribbonStrip);

        // Row 2: CAD Workspace Canvas Mockup (Realistic Structural BIM Viewport)
        var workspace = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromRgb(0x0B, 0x13, 0x26),
                Color.FromRgb(0x06, 0x09, 0x13), 90),
            ClipToBounds = true
        };

        var viewportGrid = new Grid();

        // 2.1 Bản vẽ lưới kết cấu kỹ thuật (Schematic Structural Grid Blueprint)
        var blueprintCanvas = new Canvas
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Width = 440,
            Height = 150
        };

        var gridBrush = new SolidColorBrush(Color.FromArgb(0x40, 0x38, 0xBD, 0xF8));
        var axisBrush = new SolidColorBrush(Color.FromArgb(0x80, 0x38, 0xBD, 0xF8));
        var colBrush = new SolidColorBrush(Color.FromRgb(0x02, 0x84, 0xC7));
        var colFill = new SolidColorBrush(Color.FromArgb(0x40, 0x02, 0x84, 0xC7));
        var dimBrush = new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B));

        // Đường lưới trục ngang
        for (int row = 0; row < 3; row++)
        {
            double y = 30 + row * 45;
            var line = new System.Windows.Shapes.Line { X1 = 40, Y1 = y, X2 = 400, Y2 = y, Stroke = gridBrush, StrokeDashArray = new DoubleCollection { 4, 2 } };
            blueprintCanvas.Children.Add(line);

            // Bóng trục bên trái (A, B, C)
            var bubble = new Border { Width = 18, Height = 18, CornerRadius = new CornerRadius(9), BorderBrush = axisBrush, BorderThickness = new Thickness(1), Background = new SolidColorBrush(Color.FromRgb(0x0F, 0x17, 0x2A)) };
            bubble.Child = new TextBlock { Text = ((char)('A' + row)).ToString(), FontSize = 9, FontWeight = FontWeights.Bold, Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            Canvas.SetLeft(bubble, 16);
            Canvas.SetTop(bubble, y - 9);
            blueprintCanvas.Children.Add(bubble);
        }

        // Đường lưới trục dọc & cột kết cấu
        for (int col = 0; col < 5; col++)
        {
            double x = 70 + col * 75;
            var line = new System.Windows.Shapes.Line { X1 = x, Y1 = 20, X2 = x, Y2 = 130, Stroke = gridBrush, StrokeDashArray = new DoubleCollection { 4, 2 } };
            blueprintCanvas.Children.Add(line);

            // Bóng trục bên trên (1, 2, 3, 4, 5)
            var bubble = new Border { Width = 18, Height = 18, CornerRadius = new CornerRadius(9), BorderBrush = axisBrush, BorderThickness = new Thickness(1), Background = new SolidColorBrush(Color.FromRgb(0x0F, 0x17, 0x2A)) };
            bubble.Child = new TextBlock { Text = (col + 1).ToString(), FontSize = 9, FontWeight = FontWeights.Bold, Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            Canvas.SetLeft(bubble, x - 9);
            Canvas.SetTop(bubble, 2);
            blueprintCanvas.Children.Add(bubble);

            // Cột giao điểm kết cấu (Columns)
            for (int row = 0; row < 3; row++)
            {
                double y = 30 + row * 45;
                var columnBox = new Border { Width = 10, Height = 10, BorderBrush = colBrush, BorderThickness = new Thickness(1), Background = colFill };
                Canvas.SetLeft(columnBox, x - 5);
                Canvas.SetTop(columnBox, y - 5);
                blueprintCanvas.Children.Add(columnBox);
            }
        }

        viewportGrid.Children.Add(blueprintCanvas);

        // 2.2 Viewport HUD Badges (Góc trên trái)
        var hudLeft = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(12, 10, 0, 0) };
        var hudBadge1 = new Border { Background = new SolidColorBrush(Color.FromArgb(0x30, 0x00, 0x00, 0x00)), CornerRadius = new CornerRadius(4), Padding = new Thickness(6, 2, 6, 2), Margin = new Thickness(0, 0, 6, 0) };
        hudBadge1.Child = new TextBlock { Text = "[Mặt Bằng Tầng 18]", FontSize = 10, Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)) };
        var hudBadge2 = new Border { Background = new SolidColorBrush(Color.FromArgb(0x30, 0x00, 0x00, 0x00)), CornerRadius = new CornerRadius(4), Padding = new Thickness(6, 2, 6, 2) };
        hudBadge2.Child = new TextBlock { Text = "[Tỉ Lệ 1:100 | Wireframe BIM]", FontSize = 10, Foreground = new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8)) };
        hudLeft.Children.Add(hudBadge1);
        hudLeft.Children.Add(hudBadge2);
        viewportGrid.Children.Add(hudLeft);

        // 2.3 Status Badge (Góc dưới phải)
        var docStatus = new TextBlock
        {
            Text = "Trạng thái: Đã đồng bộ với Central Server",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromRgb(0x34, 0xD3, 0x99)),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 14, 8)
        };
        viewportGrid.Children.Add(docStatus);

        // 2.4 UCS Coordinate System Icon (Góc dưới trái)
        var ucsStack = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Bottom, Margin = new Thickness(14, 0, 0, 8) };
        ucsStack.Children.Add(new TextBlock { Text = "Y ↑ ", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E)) });
        ucsStack.Children.Add(new TextBlock { Text = "X →", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)) });
        viewportGrid.Children.Add(ucsStack);

        workspace.Child = viewportGrid;

        Grid.SetRow(workspace, 2);
        windowGrid.Children.Add(workspace);

        // Row 3: Mini Status Bar
        var statusBar = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(0x0F, 0x17, 0x2A)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x1E, 0x29, 0x3B)),
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(12, 4, 12, 4)
        };
        var statusStack = new StackPanel { Orientation = Orientation.Horizontal };
        statusStack.Children.Add(new TextBlock { Text = "Sẵn sàng | Tỉ lệ: 1:100 | Hệ toạ độ: VN-2000 | Tầng 18 (Cao độ +64.800m)", FontSize = 10, Foreground = new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B)) });
        statusBar.Child = statusStack;
        Grid.SetRow(statusBar, 3);
        windowGrid.Children.Add(statusBar);

        windowMockup.Child = windowGrid;

        // ---------------------------------------------------------------------
        // 3. Tương tác với Window Buttons sống động
        // ---------------------------------------------------------------------
        bool isMaximizedState = false;
        titleBar.MaximizeRequested += (_, _) =>
        {
            isMaximizedState = !isMaximizedState;
            titleBar.IsMaximized = isMaximizedState;
            windowMockup.Height = isMaximizedState ? 380 : 280;
            ToastService.Info(isMaximizedState ? "Cửa sổ: Phóng to tối đa (Maximized)" : "Cửa sổ: Phục hồi kích thước chuẩn (Restored)", "ApplicationTitleBar");
        };

        titleBar.MinimizeRequested += (_, _) =>
        {
            ToastService.Info("Cửa sổ: Thu nhỏ xuống Taskbar", "ApplicationTitleBar");
        };

        titleBar.CloseRequested += (_, _) =>
        {
            if (titleBar.IsDocumentModified)
                ToastService.Warning("Cảnh báo: Bản vẽ có thay đổi chưa lưu! Vui lòng lưu trước khi đóng.", "ApplicationTitleBar");
            else
                ToastService.Info("Đóng cửa sổ làm việc.", "ApplicationTitleBar");
        };

        // ---------------------------------------------------------------------
        // 4. Bảng điều khiển tương tác phong phú (Interactive Control Panel)
        // ---------------------------------------------------------------------
        var readout = CadHint($"Tiêu đề hiện tại: {ApplicationTitleBar.ComposeTitle(titleBar.DocumentTitle, titleBar.IsDocumentModified, titleBar.ApplicationName)}");

        // Row TextBoxes
        var textRow = new Grid { Margin = new Thickness(0, 16, 0, 10) };
        textRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        textRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        textRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Document Title TextBox
        var docBoxStack = new StackPanel { Margin = new Thickness(0, 0, 8, 0) };
        docBoxStack.Children.Add(CadHint("Tên tài liệu / Bản vẽ (DocumentTitle):"));
        var docTextBox = new TextBox { Text = titleBar.DocumentTitle, Height = 28, Margin = new Thickness(0, 4, 0, 0) };
        docTextBox.TextChanged += (_, _) =>
        {
            titleBar.DocumentTitle = docTextBox.Text;
            docStatus.Text = $"{docTextBox.Text} — Trạng thái: {(titleBar.IsDocumentModified ? "Có thay đổi chưa lưu *" : "Đã đồng bộ")}";
            readout.Text = $"Tiêu đề hiện tại: {ApplicationTitleBar.ComposeTitle(titleBar.DocumentTitle, titleBar.IsDocumentModified, titleBar.ApplicationName)}";
        };
        docBoxStack.Children.Add(docTextBox);
        Grid.SetColumn(docBoxStack, 0);
        textRow.Children.Add(docBoxStack);

        // Application Name TextBox
        var appBoxStack = new StackPanel { Margin = new Thickness(4, 0, 4, 0) };
        appBoxStack.Children.Add(CadHint("Tên phần mềm (ApplicationName):"));
        var appTextBox = new TextBox { Text = titleBar.ApplicationName, Height = 28, Margin = new Thickness(0, 4, 0, 0) };
        appTextBox.TextChanged += (_, _) =>
        {
            titleBar.ApplicationName = appTextBox.Text;
            readout.Text = $"Tiêu đề hiện tại: {ApplicationTitleBar.ComposeTitle(titleBar.DocumentTitle, titleBar.IsDocumentModified, titleBar.ApplicationName)}";
        };
        appBoxStack.Children.Add(appTextBox);
        Grid.SetColumn(appBoxStack, 1);
        textRow.Children.Add(appBoxStack);

        // User Account TextBox
        var userBoxStack = new StackPanel { Margin = new Thickness(8, 0, 0, 0) };
        userBoxStack.Children.Add(CadHint("Tài khoản người dùng (UserAccountName):"));
        var userTextBox = new TextBox { Text = titleBar.UserAccountName, Height = 28, Margin = new Thickness(0, 4, 0, 0) };
        userTextBox.TextChanged += (_, _) => titleBar.UserAccountName = userTextBox.Text;
        userBoxStack.Children.Add(userTextBox);
        Grid.SetColumn(userBoxStack, 2);
        textRow.Children.Add(userBoxStack);

        // Toggles
        var switches = new WrapPanel { Margin = new Thickness(0, 4, 0, 10) };
        switches.Children.Add(AppearanceToggle("Chưa lưu bản vẽ (Modified *)", titleBar.IsDocumentModified, v =>
        {
            titleBar.IsDocumentModified = v;
            docStatus.Text = $"{titleBar.DocumentTitle} — Trạng thái: {(v ? "⚠️ Có thay đổi chưa lưu (*)" : "Đã đồng bộ")}";
            readout.Text = $"Tiêu đề hiện tại: {ApplicationTitleBar.ComposeTitle(titleBar.DocumentTitle, v, titleBar.ApplicationName)}";
        }));
        switches.Children.Add(AppearanceToggle("Hiện nút cửa sổ (WindowButtons)", titleBar.ShowWindowButtons, v => titleBar.ShowWindowButtons = v));
        switches.Children.Add(AppearanceToggle("Hiện Quick Access Strip", titleBar.ShowQuickAccess, v => titleBar.ShowQuickAccess = v));
        switches.Children.Add(AppearanceToggle("Tìm kiếm lệnh CAD (SearchBox)", titleBar.ShowSearchBox, v => titleBar.ShowSearchBox = v));
        switches.Children.Add(AppearanceToggle("Hiện tài khoản kỹ sư (UserAccount)", titleBar.ShowUserAccount, v => titleBar.ShowUserAccount = v));
        switches.Children.Add(AppearanceToggle("Hiện Logo App (AppIcon)", titleBar.ShowAppIcon, v => titleBar.ShowAppIcon = v));

        // Combo Row
        var comboRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 10) };

        var alignLabel = CadHint("Căn lề tiêu đề:");
        alignLabel.VerticalAlignment = VerticalAlignment.Center;
        alignLabel.Margin = new Thickness(0, 0, 8, 0);
        var alignCombo = new ComboBox
        {
            Width = 120,
            ItemsSource = new[] { "Chính giữa (Center)", "Căn trái (Left)" },
            SelectedIndex = 0,
            Margin = new Thickness(0, 0, 18, 0)
        };
        alignCombo.SelectionChanged += (_, _) =>
            titleBar.TitleAlignment = alignCombo.SelectedIndex == 1 ? HorizontalAlignment.Left : HorizontalAlignment.Center;

        var themeLabel = CadHint("Màu sắc TitleBar:");
        themeLabel.VerticalAlignment = VerticalAlignment.Center;
        themeLabel.Margin = new Thickness(0, 0, 8, 0);
        var themeCombo = new ComboBox
        {
            Width = 140,
            ItemsSource = new[] { "Mặc định (Trong suốt)", "Dark Carbon", "Navy CAD", "Titanium Glass" },
            SelectedIndex = 0,
            Margin = new Thickness(0, 0, 18, 0)
        };
        themeCombo.SelectionChanged += (_, _) =>
        {
            titleBar.Background = themeCombo.SelectedIndex switch
            {
                1 => new SolidColorBrush(Color.FromRgb(0x18, 0x1E, 0x24)),
                2 => new SolidColorBrush(Color.FromRgb(0x0C, 0x2A, 0x4A)),
                3 => new SolidColorBrush(Color.FromArgb(0x55, 0x33, 0x41, 0x55)),
                _ => Brushes.Transparent
            };
        };

        comboRow.Children.Add(alignLabel);
        comboRow.Children.Add(alignCombo);
        comboRow.Children.Add(themeLabel);
        comboRow.Children.Add(themeCombo);

        var content = new StackPanel();
        content.Children.Add(windowMockup);
        content.Children.Add(textRow);
        content.Children.Add(switches);
        content.Children.Add(comboRow);
        content.Children.Add(readout);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Thanh tiêu đề ứng dụng (ApplicationTitleBar)",
            content,
            xaml: @"<ui:ApplicationTitleBar ApplicationName=""CONCOST BIM Suite 2026""
                       DocumentTitle=""Landmark Tower — Mặt Bằng Kết Cấu""
                       IsDocumentModified=""{Binding IsDirty}""
                       ShowQuickAccess=""True""
                       ShowWindowButtons=""True""
                       ShowSearchBox=""True""
                       ShowUserAccount=""True""
                       UserAccountName=""KTS. Lê Hoàng Minh""
                       TitleAlignment=""Center""
                       CloseRequested=""OnCloseRequested""
                       MinimizeRequested=""OnMinimizeRequested""
                       MaximizeRequested=""OnMaximizeRequested"" />",
            csharp: @"// Quản lý tiêu đề tự động kèm dấu * khi có thay đổi chưa lưu:
titleBar.IsDocumentModified = true;
// Kết quả hiển thị: ""Landmark Tower — Mặt Bằng Kết Cấu* — CONCOST BIM Suite 2026""

// Bắt các sự kiện điều khiển cửa sổ:
titleBar.CloseRequested += (_, _) => CheckUnsavedChanges();
titleBar.MaximizeRequested += (_, _) => ToggleMaximize();"));

        return root;
    }

    private object CreateApplicationInfoBarDemo()
    {
        var infoBar = new ApplicationInfoBar { IsClosable = true };

        // Default: Clash detection warning
        infoBar.ActionText = "Định vị vị trí va chạm";
        infoBar.Show("Phát hiện va chạm mô hình (Clash Detection)", 
            "Dầm kết cấu D1-T18 giao cắt ống cấp nước MEP đường kính D150 tại trục 3-C.", 
            CadNotificationSeverity.Warning);

        var readout = CadHint("Hệ thống thông báo kỹ thuật công trình (BIM Alert Bar) hỗ trợ 4 mức độ: Info, Success, Warning, Error.");

        infoBar.ActionInvoked += (_, _) =>
        {
            readout.Text = "Hành động: Đã điều hướng Camera Viewport đến toạ độ va chạm tại trục 3-C.";
            ToastService.Info("Đang tiêu điểm vị trí va chạm: Dầm D1 x Ống MEP D150", "BIM Clash");
        };

        infoBar.Closed += (_, _) =>
        {
            readout.Text = "Đã đóng thanh thông báo.";
        };

        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };

        var btnWarning = SecondaryButton("⚠️ Va chạm MEP (Warning)");
        btnWarning.Click += (_, _) =>
        {
            infoBar.ActionText = "Định vị va chạm";
            infoBar.Show("Phát hiện va chạm mô hình", "Dầm D1-T18 giao cắt ống MEP D150 tại trục 3-C.", CadNotificationSeverity.Warning);
        };
        buttons.Children.Add(btnWarning);

        var btnError = SecondaryButton("❌ Vượt tải trọng (Error)");
        btnError.Click += (_, _) =>
        {
            infoBar.ActionText = "Tự động tăng tiết diện";
            infoBar.Show("Vi phạm tiêu chuẩn TCVN 5574:2018", "Hàm lượng cốt thép dầm D2 vượt quá 2.5% (μ = 2.85%).", CadNotificationSeverity.Error);
        };
        buttons.Children.Add(btnError);

        var btnSuccess = SecondaryButton("✅ Đồng bộ Central (Success)");
        btnSuccess.Click += (_, _) =>
        {
            infoBar.ActionText = "Xem lịch sử";
            infoBar.Show("Đồng bộ thành công", "Mô hình Landmark Tower đã được đồng bộ an toàn với Central BIM 360.", CadNotificationSeverity.Success);
        };
        buttons.Children.Add(btnSuccess);

        var btnInfo = SecondaryButton("ℹ️ Cập nhật TCVN (Info)");
        btnInfo.Click += (_, _) =>
        {
            infoBar.ActionText = "Xem chi tiết";
            infoBar.Show("Bản cập nhật tiêu chuẩn", "Đã cập nhật bộ dữ liệu định mức xây dựng và đơn giá Quý IV/2026.", CadNotificationSeverity.Info);
        };
        buttons.Children.Add(btnInfo);

        var content = new StackPanel();
        content.Children.Add(infoBar);
        content.Children.Add(buttons);
        content.Children.Add(readout);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Thanh thông báo kỹ thuật công trình (ApplicationInfoBar)",
            content,
            xaml: @"<ui:ApplicationInfoBar IsClosable=""True""
                       ActionText=""Định vị vị trí va chạm""
                       ActionInvoked=""OnClashLocate"" />",
            csharp: @"// Bốn cấp độ thông báo BIM: Info, Success, Warning, Error
infoBar.ActionText = ""Định vị va chạm"";
infoBar.Show(""Cảnh báo va chạm"", ""Dầm D1 giao cắt ống MEP"", CadNotificationSeverity.Warning);"));

        return root;
    }

    private object CreateBackstageViewDemo()
    {
        var backstage = new RibbonBackstageView { Height = 360, ApplicationName = "CONCOST BIM Suite" };

        // 1. Tab Thông Tin Dự Án (Project Info Dashboard)
        var infoPanel = new StackPanel { Margin = new Thickness(24) };
        infoPanel.Children.Add(new TextBlock { Text = "THÔNG TIN DỰ ÁN & HỒ SƠ THIẾT KẾ", FontSize = 16, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)), Margin = new Thickness(0, 0, 0, 14) });

        var metaGrid = new Grid();
        metaGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
        metaGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        string[][] metaData = {
            new[] { "Tên công trình:", "LANDMARK TOWER 2026 — KHU PHỨC HỢP THƯƠNG MẠI" },
            new[] { "Chủ đầu tư:", "TẬP ĐOÀN ĐẦU TƯ ĐỊA ỐC QUỐC TẾ" },
            new[] { "Địa điểm:", "Khu đô thị mới Thủ Thiêm, TP. Hồ Chí Minh" },
            new[] { "Quy mô:", "45 tầng nổi + 3 tầng hầm (Tổng diện tích sàn 125,000 m²)" },
            new[] { "Kỹ sư chủ trì:", "KTS. Lê Hoàng Minh — Chứng chỉ hành nghề hạng I" },
            new[] { "Tiêu chuẩn áp dụng:", "TCVN 5574:2018, QCVN 06:2022/BXD, Eurocode 2" },
            new[] { "Phiên bản tệp BIM:", "Autodesk Revit 2026 RVT / IFC 4.3" }
        };

        for (int i = 0; i < metaData.Length; i++)
        {
            metaGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var lbl = new TextBlock { Text = metaData[i][0], FontSize = 11, FontWeight = FontWeights.SemiBold, Foreground = new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8)), Margin = new Thickness(0, 4, 0, 4) };
            var val = new TextBlock { Text = metaData[i][1], FontSize = 11, Foreground = Brushes.White, Margin = new Thickness(0, 4, 0, 4) };
            Grid.SetRow(lbl, i);
            Grid.SetColumn(lbl, 0);
            Grid.SetRow(val, i);
            Grid.SetColumn(val, 1);
            metaGrid.Children.Add(lbl);
            metaGrid.Children.Add(val);
        }
        infoPanel.Children.Add(metaGrid);

        backstage.Items.Add(new BackstageItem("Thông tin dự án", IconKind.Info) { Content = infoPanel });

        // 2. Tab Bản Vẽ Gần Đây (Recent Models)
        var recentPanel = new StackPanel { Margin = new Thickness(24) };
        recentPanel.Children.Add(new TextBlock { Text = "BẢN VẼ & MÔ HÌNH MỞ GẦN ĐÂY", FontSize = 16, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)), Margin = new Thickness(0, 0, 0, 14) });

        string[][] recents = {
            new[] { "Landmark_Tower_Tang18_Structure.rvt", "Mặt bằng kết cấu tầng 18 — 28.5 MB — Đã sửa 15 phút trước" },
            new[] { "Tower_Foundation_PileCaps.dwg", "Móng cọc đài dầm hầm B3 — 14.2 MB — Đã sửa hôm qua" },
            new[] { "MEP_Coordination_Floor18.ifc", "Mô hình phối hợp MEP tầng 18 — 45.1 MB — 3 ngày trước" },
            new[] { "Cost_Estimate_Master_2026.xlsx", "Dự toán tổng hợp gói thầu kết cấu — 5.8 MB — 5 ngày trước" }
        };

        foreach (var file in recents)
        {
            var itemBorder = new Border { Background = new SolidColorBrush(Color.FromArgb(0x20, 0xFF, 0xFF, 0xFF)), CornerRadius = new CornerRadius(6), Padding = new Thickness(12, 8, 12, 8), Margin = new Thickness(0, 0, 0, 8), Cursor = Cursors.Hand };
            var itemStack = new StackPanel();
            itemStack.Children.Add(new TextBlock { Text = file[0], FontSize = 12, FontWeight = FontWeights.SemiBold, Foreground = Brushes.White });
            itemStack.Children.Add(new TextBlock { Text = file[1], FontSize = 10, Foreground = new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8)), Margin = new Thickness(0, 2, 0, 0) });
            itemBorder.Child = itemStack;
            recentPanel.Children.Add(itemBorder);
        }
        backstage.Items.Add(new BackstageItem("Mở bản vẽ", IconKind.Folder) { Content = recentPanel });

        // 3. Tab Xuất Dữ Liệu (Export Formats)
        var exportPanel = new StackPanel { Margin = new Thickness(24) };
        exportPanel.Children.Add(new TextBlock { Text = "XUẤT DỮ LIỆU & BÁO CÁO THIẾT KẾ", FontSize = 16, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)), Margin = new Thickness(0, 0, 0, 14) });

        string[][] exportOpts = {
            new[] { "Xuất mô hình chuẩn mở openBIM IFC 4.3", "Tương thích cao với Tekla, Archicad, Solibri" },
            new[] { "Xuất bản vẽ CAD 2D/3D DWG/DXF", "Bao gồm layer chuẩn TCVN, hatch và dimension" },
            new[] { "In ấn tập hồ sơ kỹ thuật PDF đa trang", "Khổ giấy A1, A0 độ phân giải vector cao" },
            new[] { "Xuất bảng tiên lượng dự toán Microsoft Excel", "Tính toán khối lượng tự động theo định mức" }
        };

        foreach (var opt in exportOpts)
        {
            var optBorder = new Border { Background = new SolidColorBrush(Color.FromArgb(0x20, 0xFF, 0xFF, 0xFF)), CornerRadius = new CornerRadius(6), Padding = new Thickness(12, 8, 12, 8), Margin = new Thickness(0, 0, 0, 8), Cursor = Cursors.Hand };
            var optStack = new StackPanel();
            optStack.Children.Add(new TextBlock { Text = opt[0], FontSize = 12, FontWeight = FontWeights.SemiBold, Foreground = new SolidColorBrush(Color.FromRgb(0x34, 0xD3, 0x99)) });
            optStack.Children.Add(new TextBlock { Text = opt[1], FontSize = 10, Foreground = new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8)), Margin = new Thickness(0, 2, 0, 0) });
            optBorder.Child = optStack;
            exportPanel.Children.Add(optBorder);
        }
        backstage.Items.Add(new BackstageItem("Xuất dữ liệu", IconKind.Export) { Content = exportPanel });

        backstage.Open();
        backstage.Select("Thông tin dự án");

        var readout = CadHint("Giao diện Backstage quản lý toàn diện dự án; phím Esc hoặc nút Mở/Đóng để chuyển trạng thái.");

        backstage.SelectedItemChanged += (_, _) => readout.Text = $"Đang xem trang: {backstage.SelectedItem?.Header}";
        backstage.IsOpenChanged += (_, _) => readout.Text = backstage.IsOpen ? "Đang mở Backstage Quản lý Dự án" : "Đã quay trở lại không gian bản vẽ CAD";

        var openBtn = SecondaryButton("Mở Backstage");
        openBtn.Click += (_, _) => backstage.Open();

        var closeBtn = SecondaryButton("Đóng Backstage");
        closeBtn.Click += (_, _) => backstage.Close();

        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
        buttons.Children.Add(openBtn);
        buttons.Children.Add(closeBtn);

        var content = new StackPanel();
        content.Children.Add(backstage);
        content.Children.Add(buttons);
        content.Children.Add(readout);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Trung tâm quản lý dự án & tập tin (RibbonBackstageView)",
            content,
            xaml: @"<ui:RibbonBackstageView x:Name=""Backstage""
                      ApplicationName=""CONCOST BIM Suite""
                      IsOpen=""False"" />",
            csharp: @"// Thêm các trang quản trị: Thông tin, Bản vẽ gần đây, Xuất file
backstage.Items.Add(new BackstageItem(""Thông tin dự án"", IconKind.Info) { Content = infoPanel });
backstage.Items.Add(new BackstageItem(""Mở bản vẽ"", IconKind.Folder) { Content = recentPanel });
backstage.Open();"));

        return root;
    }

    private object CreateFontPickerDemo()
    {
        // ---------------------------------------------------------------------
        // 1. Khung tên bản vẽ kỹ thuật CAD/BIM (Technical Title Block Preview)
        // ---------------------------------------------------------------------
        var titleBlock = new Border
        {
            Height = 220,
            CornerRadius = new CornerRadius(6),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)),
            BorderThickness = new Thickness(2),
            Background = new SolidColorBrush(Color.FromRgb(0x0F, 0x17, 0x2A)),
            Padding = new Thickness(16)
        };

        var titleBlockGrid = new Grid();
        titleBlockGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        titleBlockGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        titleBlockGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Tên dự án & Công trình
        var projName = new TextBlock
        {
            Text = "TÒA NHÀ TRUNG TÂM THƯƠNG MẠI & VĂN PHÒNG LANDMARK 2026",
            FontSize = 13,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)),
            Margin = new Thickness(0, 0, 0, 6)
        };
        Grid.SetRow(projName, 0);
        titleBlockGrid.Children.Add(projName);

        // Nội dung mẫu bản vẽ và ghi chú kỹ thuật (Chịu ảnh hưởng trực tiếp của FontPicker)
        var previewBody = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        var sheetTitle = new TextBlock
        {
            Text = "MẶT BẰNG KẾT CẤU TẦNG ĐIỂN HÌNH (L3 - L20) — TỶ LỆ 1:100",
            FontSize = 15,
            FontWeight = FontWeights.SemiBold,
            Foreground = Brushes.White,
            Margin = new Thickness(0, 0, 0, 8)
        };
        var noteAnnotation = new TextBlock
        {
            Text = "Ghi chú: Dầm D1 (300×600) — Bê tông B25 (C20/25), 4Ø25 thép dọc chịu lực, cốt đai Ø10a150 TCVN 5574:2018.",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8)),
            TextWrapping = TextWrapping.Wrap
        };
        previewBody.Children.Add(sheetTitle);
        previewBody.Children.Add(noteAnnotation);
        Grid.SetRow(previewBody, 1);
        titleBlockGrid.Children.Add(previewBody);

        // Bảng khung tên kiểm duyệt
        var footerGrid = new Grid { Margin = new Thickness(0, 8, 0, 0) };
        footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var authorTxt = new TextBlock { Text = "Người vẽ: KTS. Lê Hoàng Minh", FontSize = 10.5, Foreground = new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B)) };
        var checkerTxt = new TextBlock { Text = "Chủ trì: KS. Trần Quốc Bảo", FontSize = 10.5, Foreground = new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B)) };
        var dateTxt = new TextBlock { Text = "Ngày: 01/11/2026 | Bản vẽ số: KC-08", FontSize = 10.5, Foreground = new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B)), HorizontalAlignment = HorizontalAlignment.Right };

        Grid.SetColumn(authorTxt, 0);
        Grid.SetColumn(checkerTxt, 1);
        Grid.SetColumn(dateTxt, 2);
        footerGrid.Children.Add(authorTxt);
        footerGrid.Children.Add(checkerTxt);
        footerGrid.Children.Add(dateTxt);

        Grid.SetRow(footerGrid, 2);
        titleBlockGrid.Children.Add(footerGrid);

        titleBlock.Child = titleBlockGrid;

        // ---------------------------------------------------------------------
        // 2. Bộ chọn Font & Khay cấu hình 2 cột
        // ---------------------------------------------------------------------
        var picker = new FontPicker
        {
            SelectedFontFamily = new FontFamily("Segoe UI"),
            SelectedFontSize = 14,
            ShowPreview = false
        };

        void UpdatePreviewFont()
        {
            sheetTitle.FontFamily = picker.SelectedFontFamily;
            sheetTitle.FontSize = Math.Max(12, picker.SelectedFontSize);
            sheetTitle.FontWeight = picker.EffectiveWeight;
            sheetTitle.FontStyle = picker.EffectiveStyle;

            noteAnnotation.FontFamily = picker.SelectedFontFamily;
            noteAnnotation.FontSize = Math.Max(10, picker.SelectedFontSize - 2);
            noteAnnotation.FontWeight = picker.EffectiveWeight;
            noteAnnotation.FontStyle = picker.EffectiveStyle;
        }

        picker.SelectionChanged += (_, _) => UpdatePreviewFont();

        var workbench = new Grid { Margin = new Thickness(0, 14, 0, 4) };
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(450) });
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Cột 1: FontPicker Control
        var col1 = new StackPanel();
        var head1 = new TextBlock
        {
            Text = "BỘ CHỌN FONT CHỮ BẢN VẼ (TYPOGRAPHY SELECTOR)",
            FontSize = 11.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 10)
        };
        head1.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        col1.Children.Add(head1);
        col1.Children.Add(picker);

        Grid.SetColumn(col1, 0);
        workbench.Children.Add(col1);

        // Cột 2: Mẫu font CAD tiêu chuẩn & Thông tin
        var col2 = new StackPanel();
        var head2 = new TextBlock
        {
            Text = "FONT KỸ THUẬT TIÊU CHUẨN (CAD/BIM PRESETS)",
            FontSize = 11.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 10)
        };
        head2.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        col2.Children.Add(head2);

        var presetsRow = new WrapPanel { Margin = new Thickness(0, 0, 0, 10) };
        var btnSegoe = SecondaryButton("Segoe UI (BIM hiện đại)");
        btnSegoe.Click += (_, _) => { picker.SelectedFontFamily = new FontFamily("Segoe UI"); picker.SelectedFontSize = 14; UpdatePreviewFont(); };

        var btnArial = SecondaryButton("Arial (TCVN chuẩn)");
        btnArial.Click += (_, _) => { picker.SelectedFontFamily = new FontFamily("Arial"); picker.SelectedFontSize = 13; UpdatePreviewFont(); };

        var btnConsolas = SecondaryButton("Consolas (Kỹ thuật Monospace)");
        btnConsolas.Click += (_, _) => { picker.SelectedFontFamily = new FontFamily("Consolas"); picker.SelectedFontSize = 13; UpdatePreviewFont(); };

        presetsRow.Children.Add(btnSegoe);
        presetsRow.Children.Add(btnArial);
        presetsRow.Children.Add(btnConsolas);
        col2.Children.Add(presetsRow);

        var infoCard = CreateCalloutCard(
            "QUY TẮC FONT CHỮ BẢN VẼ KỸ THUẬT CAD/BIM:\n" +
            "• Tiêu đề bản vẽ: 14 - 18 pt, Chữ in hoa, SemiBold/Bold.\n" +
            "• Ghi chú kết cấu & thép: 10 - 12 pt, Regular, dễ đọc khi in ở tỷ lệ 1:100.\n" +
            "• Ký hiệu trục tọa độ: 10 - 12 pt, Bold, nằm trọn trong bong bóng trục Ø8mm.\n" +
            "• Thay đổi font và kích cỡ ở bên trái để quan sát khung tên mẫu ở trên co giãn trực quan.");
        col2.Children.Add(infoCard);

        Grid.SetColumn(col2, 2);
        workbench.Children.Add(col2);

        var stageHost = new StackPanel();
        stageHost.Children.Add(titleBlock);
        stageHost.Children.Add(workbench);

        string xamlCode = @"<ui:FontPicker SelectedFontFamily=""Segoe UI""
               SelectedFontSize=""14""
               ShowPreview=""True""
               SelectionChanged=""OnFontChanged"" />";

        string csCode = @"// Lắng nghe sự kiện thay đổi font để cập nhật kiểu chữ bản vẽ
picker.SelectionChanged += (s, e) =>
{
    textBlock.FontFamily = picker.SelectedFontFamily;
    textBlock.FontSize = picker.SelectedFontSize;
    textBlock.FontWeight = picker.EffectiveWeight;
    textBlock.FontStyle = picker.EffectiveStyle;
};";

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Bộ chọn font bản vẽ kỹ thuật (FontPicker)",
            stageHost,
            xaml: xamlCode,
            csharp: csCode));

        return root;
    }

    // =========================================================================
    // Demo helpers
    // =========================================================================

    private static TextBlock CadHint(string text)
    {
        var hint = new TextBlock
        {
            Text = text,
            FontSize = 12,
            LineHeight = 18,
            Margin = new Thickness(0, 10, 0, 0),
            TextWrapping = TextWrapping.Wrap
        };

        hint.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        return hint;
    }

    /// <summary>One rule of the page: a short title over a line of why.</summary>
    private static StackPanel RuleRow(string title, string detail)
    {
        var row = new StackPanel { Margin = new Thickness(0, 12, 0, 0) };

        var head = new TextBlock { Text = title, FontSize = 12 };
        head.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");

        var body = new TextBlock { Text = detail, FontSize = 11.5, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 2, 0, 0) };
        body.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        row.Children.Add(head);
        row.Children.Add(body);

        return row;
    }

    private static Button SecondaryButton(string caption)
    {
        var button = new Button { Content = caption, Padding = new Thickness(12, 6, 12, 6), Margin = new Thickness(0, 0, 6, 0) };

        button.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");

        return button;
    }

    private static Border Framed(UIElement child)
    {
        var frame = new Border
        {
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(4),
            HorizontalAlignment = HorizontalAlignment.Left,
            Child = child
        };

        frame.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
        frame.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

        return frame;
    }

    private static object PageText(string text)
    {
        var block = new TextBlock
        {
            Text = text,
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(28)
        };

        block.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");

        return block;
    }

    private static object InfoPage()
    {
        var panel = new StackPanel { Margin = new Thickness(28) };

        var title = new TextBlock { Text = "CONCOST", FontSize = 22, FontWeight = FontWeights.SemiBold };
        title.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");

        var version = new TextBlock { Text = "Phiên bản 1.3 — CAD shell", FontSize = 12, Margin = new Thickness(0, 4, 0, 12) };
        version.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var note = new TextBlock
        {
            Text = "Trang backstage là nội dung thuần: host đặt file list, tuỳ chọn hay bản xem trước in ở đây.",
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap
        };
        note.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        panel.Children.Add(title);
        panel.Children.Add(version);
        panel.Children.Add(note);

        return panel;
    }

    /// <summary>A stand-in model object for the properties palette demo.</summary>
    private sealed class CadDemoBeam
    {
        public double Length { get; set; } = 6000;

        public double Height { get; set; } = 400;

        public string Material { get; set; } = "Bê tông B25";

        public bool IsLocked { get; }
    }
}
