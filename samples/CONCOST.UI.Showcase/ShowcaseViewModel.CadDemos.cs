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
            Cursor = System.Windows.Input.Cursors.SizeAll
        };
        viewer.SetResourceReference(ModelViewer3D.EdgeBrushProperty, "CONCOST.Brush.Primary");

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
            ShowFocusRing = false,
            ShowTooltips = true,
            SnapToClosestView = true,
            KeepSceneUpright = true,
            AnimatedTransitions = true,
            AnimationDuration = 450,
            InactiveOpacity = 100
        };

        // Hai góc đã lưu mẫu: menu chuột phải và menu định hướng liệt kê chúng.
        cube.SaveCurrentView(Loc.T("Trục đo mặc định", "Default isometric"));
        cube.SaveCurrentView(Loc.T("Nhìn từ trên", "Top view"));

        // ---------------------------------------------------------------------
        // 3. Hai chiều đồng bộ ViewCube và ModelViewer3D
        // ---------------------------------------------------------------------
        var viewTelemetry = new TextBlock
        {
            Text = $"Yaw: {camera.Yaw:0.#}° | Pitch: {camera.Pitch:0.#}° | Roll: {camera.Roll:0.#}°",
            FontSize = 11,
            FontFamily = new FontFamily("Consolas, Courier New, monospace"),
            VerticalAlignment = VerticalAlignment.Center
        };
        viewTelemetry.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var viewNameBadge = new TextBlock
        {
            Text = Loc.T("Phối cảnh trục đo", "ISO view (Top-Front-Right)"),
            FontSize = 12,
            FontWeight = FontWeights.SemiBold
        };
        viewNameBadge.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.Primary");

        var interactionHint = new TextBlock
        {
            Text = Loc.T("Sẵn sàng điều hướng. Bấm các mặt hoặc kéo chuột để xoay.", "Ready to navigate. Click a face or drag to orbit."),
            FontSize = 11,
            VerticalAlignment = VerticalAlignment.Center
        };
        interactionHint.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

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
            viewNameBadge.Text = Loc.T($"Góc nhìn: {e.Hit.Label} ({e.Hit.Part})", $"View: {e.Hit.Label} ({e.Hit.Part})");
            interactionHint.Text = Loc.T($"Đã yêu cầu chuyển góc: {e.Hit.Label}", $"View change requested: {e.Hit.Label}");
            ToastService.Info(Loc.T($"Chuyển góc nhìn: {e.Hit.Label}", $"Switching view: {e.Hit.Label}"), "ViewCube 3D");
        };

        cube.SaveViewRequested += (_, _) =>
        {
            string name = Loc.F("Góc đã lưu {0}", "Saved view {0}", cube.SavedViews.Count + 1);
            cube.SaveCurrentView(name);
            ToastService.Success(Loc.F("Đã lưu \"{0}\" — mở menu định hướng để quay lại.", "Saved \"{0}\" — open the orientation menu to return.", name), "ViewCube 3D");
        };

        cube.SavedViewActivated += (_, view) =>
        {
            interactionHint.Text = Loc.F("Đang xem góc đã lưu: {0}", "Viewing saved view: {0}", view.Name);
        };

        cube.RegionHovered += (_, e) =>
        {
            if (e.Hit.IsEmpty)
                interactionHint.Text = Loc.T("Rê chuột lên khối điều hướng để chọn 26 vùng, mũi tên 90° hoặc la bàn", "Hover the ViewCube to pick one of 26 zones, the 90° arrows or the compass");
            else
                interactionHint.Text = Loc.T($"Đang trỏ: {e.Hit.Label}", $"Hovering: {e.Hit.Label} [{e.Hit.Part}]");
        };

        cube.ViewTransitionCompleted += (_, _) =>
        {
            viewTelemetry.Text = $"Yaw: {camera.Yaw:0.#}° | Pitch: {camera.Pitch:0.#}° | Roll: {camera.Roll:0.#}°";
            viewNameBadge.Text = Loc.F("Định hướng: {0}", "Orientation: {0}", cube.OrientationName);
            interactionHint.Text = Loc.T("Chuyển hướng hoàn tất", "View change complete");
        };

        // ---------------------------------------------------------------------
        // 4. Viewport CAD Container (Dark Industrial Gradient Stage)
        // ---------------------------------------------------------------------
        var stage = new Border
        {
            Height = 460,
            CornerRadius = new CornerRadius(12),
            BorderThickness = new Thickness(1),
            ClipToBounds = true
        };
        stage.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.CanvasBackground");
        stage.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

        var viewportGrid = new Grid();
        viewportGrid.Children.Add(viewer);
        viewportGrid.Children.Add(cube);

        // Header HUD Overlay
        // The plate follows the theme. It used to be a hard-coded navy block, which read as a
        // leftover from a dark canvas on a light page.
        var headerHud = new Border
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(14),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12, 8, 12, 8)
        };
        headerHud.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
        headerHud.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

        var headerHudStack = new StackPanel();
        var hudTitleRow = new StackPanel { Orientation = Orientation.Horizontal };
        var hudTitle = new TextBlock
        {
            Text = Loc.T("🏢 KHUNG NHÌN 3D CAD/BIM", "🏢 CAD/BIM VIEWPORT 3D"),
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 8, 0)
        };
        hudTitle.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        hudTitleRow.Children.Add(hudTitle);

        var liveBadge = new Border
        {
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(5, 1, 5, 1),
            Child = new TextBlock { Text = Loc.T("TRỰC TIẾP 3D", "LIVE 3D"), FontSize = 9, FontWeight = FontWeights.Bold, Foreground = Brushes.White }
        };
        liveBadge.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Primary");
        hudTitleRow.Children.Add(liveBadge);

        headerHudStack.Children.Add(hudTitleRow);
        headerHudStack.Children.Add(viewNameBadge);

        // The stack was built but never attached, so the plate painted as an empty dark pill.
        headerHud.Child = headerHudStack;

        viewportGrid.Children.Add(headerHud);

        // Footer Telemetry Bar Overlay
        // Same reason as the header: a light canvas wants a light status strip.
        var footerBar = new Border
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Bottom,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(14, 6, 14, 6)
        };
        footerBar.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
        footerBar.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

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
        var quickViewsRow = new WrapPanel { Margin = new Thickness(0, 14, 0, 10) };
        var quickLabel = CadHint(Loc.T("Góc nhìn nhanh:", "Quick views:"));
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

        quickViewsRow.Children.Add(MakeViewBtn(Loc.T("🏠 Góc gốc", "🏠 Home (ISO)"), () => cube.ResetToHome()));
        quickViewsRow.Children.Add(MakeViewBtn(Loc.T("Mặt trên", "Top"), () => cube.GoToFace("TOP")));
        quickViewsRow.Children.Add(MakeViewBtn(Loc.T("Mặt trước", "Front"), () => cube.GoToFace("FRONT")));
        quickViewsRow.Children.Add(MakeViewBtn(Loc.T("Mặt phải", "Right"), () => cube.GoToFace("RIGHT")));
        quickViewsRow.Children.Add(MakeViewBtn(Loc.T("Mặt sau", "Back"), () => cube.GoToFace("BACK")));
        quickViewsRow.Children.Add(MakeViewBtn(Loc.T("Mặt trái", "Left"), () => cube.GoToFace("LEFT")));
        quickViewsRow.Children.Add(MakeViewBtn(Loc.T("🎯 Vừa khung hình", "🎯 Fit"), () => { viewer.ResetView(); camera.FitToView(); }));

        // Góc đã lưu: menu định hướng của cube cũng liệt kê các góc này.
        quickViewsRow.Children.Add(MakeViewBtn(Loc.T("💾 Lưu góc hiện tại", "💾 Save current view"), () =>
        {
            string name = Loc.F("Góc đã lưu {0}", "Saved view {0}", cube.SavedViews.Count + 1);
            cube.SaveCurrentView(name);
            ToastService.Success(Loc.F("Đã lưu \"{0}\".", "Saved \"{0}\".", name), "ViewCube 3D");
        }));

        var savedViewsLabel = CadHint(Loc.T("Góc đã lưu:", "Saved views:"));
        savedViewsLabel.VerticalAlignment = VerticalAlignment.Center;
        savedViewsLabel.Margin = new Thickness(6, 0, 10, 0);
        quickViewsRow.Children.Add(savedViewsLabel);

        var savedViewsHost = new WrapPanel();
        quickViewsRow.Children.Add(savedViewsHost);

        void RefreshSavedViewButtons()
        {
            savedViewsHost.Children.Clear();

            foreach (var view in cube.SavedViews)
            {
                var captured = view;
                var chip = SecondaryButton(captured.Name);
                chip.Margin = new Thickness(0, 0, 6, 0);
                chip.Click += (_, _) => cube.GoToSavedView(captured.Name);
                savedViewsHost.Children.Add(chip);
            }

            if (savedViewsHost.Children.Count == 0)
            {
                var empty = CadHint(Loc.T("(chưa có)", "(none)"));
                empty.VerticalAlignment = VerticalAlignment.Center;
                savedViewsHost.Children.Add(empty);
            }
        }

        RefreshSavedViewButtons();
        cube.SavedViewActivated += (_, _) => RefreshSavedViewButtons();

        // Row 2: Vị trí & Kích thước & Theme & Ngôn ngữ
        var configRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 10) };

        // Position Combo
        var posLabel = CadHint(Loc.T("Vị trí:", "Position:"));
        posLabel.Margin = new Thickness(0, 0, 6, 0);
        posLabel.VerticalAlignment = VerticalAlignment.Center;
        var posCombo = new ComboBox
        {
            Width = 120,
            ItemsSource = new[] { Loc.T("Góc Trên-Phải", "Top-right"), Loc.T("Góc Trên-Trái", "Top-left"), Loc.T("Góc Dưới-Phải", "Bottom-right"), Loc.T("Góc Dưới-Trái", "Bottom-left") },
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
        var sizeLabel = CadHint(Loc.T("Kích thước:", "Size:"));
        sizeLabel.Margin = new Thickness(0, 0, 6, 0);
        sizeLabel.VerticalAlignment = VerticalAlignment.Center;
        var sizeCombo = new ComboBox
        {
            Width = 110,
            ItemsSource = new[] { Loc.T("Lớn (128px)", "Large (128px)"), Loc.T("Chuẩn (100px)", "Normal (100px)"), Loc.T("Vừa (80px)", "Medium (80px)"), Loc.T("Nhỏ (64px)", "Small (64px)") },
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
        var themeLabel = CadHint(Loc.T("Chủ đề màu:", "Theme:"));
        themeLabel.Margin = new Thickness(0, 0, 6, 0);
        themeLabel.VerticalAlignment = VerticalAlignment.Center;
        var themeCombo = new ComboBox
        {
            Width = 135,
            ItemsSource = new[] { Loc.T("Revit chuẩn", "Revit classic"), Loc.T("Xám đá đậm", "Dark Slate"), Loc.T("Xanh bản vẽ", "Blueprint Cyan"), Loc.T("Vàng hổ phách", "Amber Gold") },
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
        var langLabel = CadHint(Loc.T("Nhãn khối:", "Labels:"));
        langLabel.Margin = new Thickness(0, 0, 6, 0);
        langLabel.VerticalAlignment = VerticalAlignment.Center;
        var langCombo = new ComboBox
        {
            Width = 110,
            ItemsSource = new[] { Loc.T("Tiếng Anh", "English (TOP)"), Loc.T("Tiếng Việt", "Vietnamese (TRÊN)") },
            SelectedIndex = Loc.IsVietnamese ? 1 : 0
        };
        if (Loc.IsVietnamese)
        {
            cube.TopLabel = "TRÊN";
            cube.BottomLabel = "DƯỚI";
            cube.FrontLabel = "TRƯỚC";
            cube.BackLabel = "SAU";
            cube.LeftLabel = "TRÁI";
            cube.RightLabel = "PHẢI";
        }
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
            Text = Loc.T("HÌNH HỌC VÀ GIAO DIỆN", "GEOMETRY & APPEARANCE"),
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

        col1.Children.Add(AppearanceSlider(Loc.T("Độ nhạy kéo xoay", "Drag sensitivity"), 0.2, 2.0, cube.DragSensitivity, 0.05, "0.00", v => cube.DragSensitivity = v));
        col1.Children.Add(AppearanceSlider(Loc.T("Độ mờ khi rời chuột", "Idle opacity"), 10, 100, cube.InactiveOpacity, 5, "0'%'", v => cube.InactiveOpacity = v));
        col1.Children.Add(AppearanceSlider(Loc.T("Thời gian chuyển góc", "Transition time"), 100, 1500, cube.AnimationDuration, 50, "0' ms'", v => cube.AnimationDuration = v));

        Grid.SetColumn(col1, 0);
        workbench.Children.Add(col1);

        // Cot 2: Tinh nang & Huong dan Icon chi tiet (Features & Status Callout)
        var col2 = new StackPanel();
        var head2 = new TextBlock
        {
            Text = Loc.T("TÍNH NĂNG VÀ HƯỚNG DẪN TƯƠNG TÁC", "FEATURES & HINTS"),
            FontSize = 11.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 12)
        };
        head2.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        col2.Children.Add(head2);

        var switches = new WrapPanel { Margin = new Thickness(0, 0, 0, 12) };
        switches.Children.Add(AppearanceToggle(Loc.T("Luôn hiện biểu tượng", "Always show tools"), cube.AlwaysShowTools, v => cube.AlwaysShowTools = v));
        switches.Children.Add(AppearanceToggle(Loc.T("Hiện la bàn", "Show compass"), cube.ShowCompass, v => cube.ShowCompass = v));
        switches.Children.Add(AppearanceToggle(Loc.T("Hút góc gần nhất", "Snap to closest view"), cube.SnapToClosestView, v => cube.SnapToClosestView = v));
        switches.Children.Add(AppearanceToggle(Loc.T("Giữ đúng trục", "Keep scene upright"), cube.KeepSceneUpright, v => cube.KeepSceneUpright = v));
        switches.Children.Add(AppearanceToggle(Loc.T("Chuyển động mượt", "Animated transitions"), cube.AnimatedTransitions, v => cube.AnimatedTransitions = v));
        col2.Children.Add(switches);

        var iconGuideCallout = CreateCalloutCard(
            Loc.T("HƯỚNG DẪN ĐIỀU KHIỂN KHỐI 3D:\n", "VIEWCUBE 3D CONTROL GUIDE:\n") +
            Loc.T("• [Góc gốc] Góc trên-trái: Quay về góc nhìn trục đo chuẩn.\n", "• [Home] Top-left: return to the standard isometric view.\n") +
            Loc.T("• [Xoay nghiêng] Góc trên-phải: Quay 90° quanh trục nhìn.\n", "• [Roll] Top-right: roll 90° around the view axis.\n") +
            Loc.T("• [Mũi tên] 4 hướng: Bước nhảy 90° sang mặt kế tiếp.\n", "• [Arrows] four directions: step 90° to the next face.\n") +
            Loc.T("• [La bàn] B, Đ, N, T: Kéo xoay phương vị mặt đất.\n", "• [Compass] N, E, S, W: drag to turn the ground heading.\n") +
            Loc.T("• [Trình đơn] Góc dưới-phải: Chuyển chế độ Trực giao / Phối cảnh.\n", "• [Menu] Bottom-right: switch Ortho / Perspective.\n") +
            Loc.T("• Kéo chuột trực tiếp lên khối: Xoay 3D tự do mượt mà.", "• Drag directly on the cube: smooth free 3D orbit."));
        col2.Children.Add(iconGuideCallout);

        Grid.SetColumn(col2, 2);
        workbench.Children.Add(col2);

        var content = new StackPanel();
        content.Children.Add(stage);
        content.Children.Add(quickViewsRow);
        content.Children.Add(workbench);

                var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Khối điều hướng Revit ViewCube 3D", "Revit ViewCube 3D"),
            content,
            xaml: Loc.Code(@"<Grid>
    <controls:ModelViewer3D x:Name=""Viewport"" ShowAxes=""True"" />
    <ui:ViewCube Host=""{Binding Camera}""
                 Size=""Large""
                 Position=""TopRight""
                 ShowCompass=""True""
                 ShowFocusRing=""False""
                 InactiveOpacity=""70""
                 FaceBrush=""#1E293B""
                 HighlightBrush=""#0284C7""
                 TopLabel=""TRÊN"" FrontLabel=""TRƯỚC"" />
</Grid>"),
            csharp: Loc.Code(@"// ViewCube được gắn trực tiếp vào ICameraHost của Viewport 3D:
var camera = new CameraController();
cube.Host = camera;
cube.Size = ViewCubeSize.Large;
cube.Position = ViewCubePosition.TopRight;
cube.ShowCompass = true;
cube.ShowFocusRing = false;

// Hỗ trợ đổi nhãn mặt sang Tiếng Việt:
cube.TopLabel = ""TRÊN"";
cube.FrontLabel = ""TRƯỚC"";

// Đổi góc nhìn trực tiếp bằng code:
cube.ResetToHome();
cube.GoToFace(""TOP"");")));

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
            Zoom = 0.82,
            EdgeBrush = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)),
            Cursor = System.Windows.Input.Cursors.SizeAll
        };

        var vertices = new List<Point3D>
        {
            new(-12, -4, -3), new(12, -4, -3), new(12, 4, -3), new(-12, 4, -3),
            new(-12, -4, 0),  new(12, -4, 0),  new(12, 4, 0),  new(-12, 4, 0),
            new(-12, -4, 3),  new(12, -4, 3),  new(12, 4, 3),  new(-12, 4, 3),
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
            Text = Loc.T("Lệnh: Sẵn sàng | Công cụ: Không | Hệ tọa độ: Mặt trên", "Command: ready | Tool: None | UCS: Top"),
            FontSize = 11.5,
            FontWeight = FontWeights.Medium,
            Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)),
            VerticalAlignment = VerticalAlignment.Center
        };

        var hudPill = new Border
        {
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12, 5, 12, 5),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(14),
            Child = hudText
        };
        hudPill.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
        hudPill.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
        hudText.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");

        // Callout thong bao trang thai
        var statusCallout = CreateCalloutCard(Loc.T("Sẵn sàng: Bấm các nút trên thanh điều hướng để điều khiển góc nhìn và công cụ 3D.", "Ready: use the NavigationBar buttons to control the view and 3D tools."));

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
            hudText.Text = Loc.T($"Lệnh: {e.Command} | Công cụ: {bar.ActiveTool} | UCS: {bar.Ucs}", $"Command: {e.Command} | Tool: {bar.ActiveTool} | UCS: {bar.Ucs}");
            UpdateCallout(Loc.T($"Đã thực thi lệnh điều hướng: {e.Command}", $"Navigation command executed: {e.Command}"));
            ToastService.Info(Loc.T($"Điều hướng: {e.Command}", $"Navigation: {e.Command}"), "NavigationBar");
        };

        bar.ActiveToolChanged += (_, _) =>
        {
            hudText.Text = Loc.T($"Công cụ: {bar.ActiveTool} | UCS: {bar.Ucs}", $"Tool: {bar.ActiveTool} | UCS: {bar.Ucs}");
            UpdateCallout(Loc.T($"Công cụ tương tác đang bật: {bar.ActiveTool} (Bấm lại nút trên thanh để tắt)", $"Active tool: {bar.ActiveTool} (click the button again to turn it off)"));
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
            hudText.Text = Loc.T($"Mặt phẳng UCS: {bar.Ucs} | Công cụ: {bar.ActiveTool}", $"UCS plane: {bar.Ucs} | Tool: {bar.ActiveTool}");
            UpdateCallout(Loc.T($"Đã chuyển đổi mặt phẳng làm việc sang: {bar.Ucs}", $"Work plane changed to: {bar.Ucs}"));
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
            BorderThickness = new Thickness(1),
            ClipToBounds = true,
            Child = viewportHost
        };
        stage.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.CanvasBackground");
        stage.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

        // 3. Khay cau hinh tuong tac 2 cot chuan Desktop CAD/BIM
        var workbench = new Grid { Margin = new Thickness(0, 14, 0, 4) };
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(430) });
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Cot 1: Cau hinh kieu dang va kich thuoc
        var col1 = new StackPanel();
        var head1 = new TextBlock
        {
            Text = Loc.T("KIỂU DÁNG & HÌNH HỌC", "GEOMETRY & STYLE"),
            FontSize = 11.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 10)
        };
        head1.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        col1.Children.Add(head1);

        var variantRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
        var varLbl = new TextBlock { Text = Loc.T("Kiểu:", "Variant:"), FontSize = 12, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0), Width = 38 };
        varLbl.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        var variantCombo = new ComboBox
        {
            Width = 160,
            ItemsSource = new[] { Loc.T("Nổi", "Floating"), Loc.T("Phẳng", "Seamless"), Loc.T("Kính mờ", "Glass") },
            SelectedIndex = 0,
            Margin = new Thickness(0, 0, 14, 0)
        };
        variantCombo.SelectionChanged += (_, _) =>
        {
            bar.Variant = (NavigationBarVariant)Math.Max(0, variantCombo.SelectedIndex);
            UpdateCallout(Loc.T($"Đã đổi kiểu hiển thị thanh điều hướng sang: {bar.Variant}", $"NavigationBar variant changed to: {bar.Variant}"));
        };

        var oriLbl = new TextBlock { Text = Loc.T("Hướng:", "Direction:"), FontSize = 12, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0), Width = 45 };
        oriLbl.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        var oriCombo = new ComboBox
        {
            Width = 100,
            ItemsSource = new[] { Loc.T("Ngang", "Horizontal"), Loc.T("Dọc", "Vertical") },
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

        col1.Children.Add(AppearanceSlider(Loc.T("Cỡ nút", "Button size (ButtonSize)"), 20, 40, bar.ButtonSize, 2, "0", v => bar.ButtonSize = v));
        col1.Children.Add(AppearanceSlider(Loc.T("Cỡ biểu tượng", "Icon size (IconSize)"), 10, 22, bar.IconSize, 1, "0", v => bar.IconSize = v));
        col1.Children.Add(AppearanceSlider(Loc.T("Bo góc", "Corner radius"), 0, 20, bar.CornerRadius.TopLeft, 1, "0", v => bar.CornerRadius = new CornerRadius(v)));
        col1.Children.Add(AppearanceSlider(Loc.T("Độ nổi", "Elevation"), 0, 4, bar.Elevation, 1, "0", v => bar.Elevation = (int)v));

        Grid.SetColumn(col1, 0);
        workbench.Children.Add(col1);

        // Cot 2: Nhom cong cu, hanh dong & callout
        var col2 = new StackPanel();
        var head2 = new TextBlock
        {
            Text = Loc.T("NHÓM TÍNH NĂNG & THAO TÁC", "FEATURES & ACTIONS"),
            FontSize = 11.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 10)
        };
        head2.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        col2.Children.Add(head2);

        var switchesWrap = new WrapPanel { Margin = new Thickness(0, 0, 0, 10) };
        switchesWrap.Children.Add(AppearanceToggle(Loc.T("Nhãn hệ tọa độ", "UCS label"), bar.ShowUcsLabel, v => bar.ShowUcsLabel = v));
        switchesWrap.Children.Add(AppearanceToggle(Loc.T("Cụm thu phóng", "Zoom cluster"), bar.ShowZoom, v => bar.ShowZoom = v));
        switchesWrap.Children.Add(AppearanceToggle(Loc.T("Phối cảnh", "Perspective"), bar.ShowPerspective, v => bar.ShowPerspective = v));
        switchesWrap.Children.Add(AppearanceToggle(Loc.T("Đi bộ", "Walk"), bar.ShowWalk, v => bar.ShowWalk = v));
        switchesWrap.Children.Add(AppearanceToggle(Loc.T("Đường phân cách", "Divider"), bar.ShowDividers, v => bar.ShowDividers = v));
        col2.Children.Add(switchesWrap);

        // Quick action buttons
        var quickActions = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
        var resetBtn = SecondaryButton(Loc.T("🏠 Đặt lại góc nhìn", "🏠 Reset View"));
        resetBtn.Margin = new Thickness(0, 0, 6, 0);
        resetBtn.Click += (_, _) => { viewer.ResetView(); camera.FitToView(); UpdateCallout(Loc.T("Đã đặt lại góc nhìn mặc định.", "Default view restored.")); };

        var extentsBtn = SecondaryButton(Loc.T("🔍 Thu phóng toàn khung", "🔍 Zoom Extents"));
        extentsBtn.Margin = new Thickness(0, 0, 6, 0);
        extentsBtn.Click += (_, _) => { viewer.Zoom = 0.95; UpdateCallout(Loc.T("Đã thu phóng toàn bộ mô hình.", "Zoomed to the whole model (Zoom Extents).")); };

        var isoBtn = SecondaryButton(Loc.T("📐 Góc nhìn trục đo", "📐 ISO view"));
        isoBtn.Click += (_, _) => { camera.SetOrientation(new Vector3D(0.85, -1.0, 0.65), isFinal: true); UpdateCallout(Loc.T("Đã chuyển sang góc phối cảnh trục đo.", "Switched to the isometric perspective view.")); };

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

        string csCode = Loc.Code(@"// 1. Cấu hình kiểu dáng (Variant) và kích thước
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
};");

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Thanh điều hướng khung nhìn 3D", "3D viewport navigation bar (NavigationBar)"),
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
            new(-12, -4, -3), new(12, -4, -3), new(12, 4, -3), new(-12, 4, -3),
            new(-12, -4, 3),  new(12, -4, 3),  new(12, 4, 3),  new(-12, 4, 3)
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
            HideIsolateCaption = Loc.T("Ẩn", "Hide"),
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
            Text = Loc.F("Tỉ lệ: {0} | Chi tiết: {1} | Kiểu: {2}",
                "Scale: {0} | Detail: {1} | Style: {2}", bar.ViewScale, bar.DetailLevel, bar.VisualStyle),
            FontSize = 11.5,
            FontWeight = FontWeights.Medium,
            VerticalAlignment = VerticalAlignment.Center
        };
        hudText.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");

        // The pill follows the theme: a fixed navy plate put dark theme text on a dark fill.
        var hudPill = new Border
        {
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12, 6, 12, 6),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(14),
            Child = hudText
        };
        hudPill.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
        hudPill.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

        var statusCallout = CreateCalloutCard(Loc.T(
            "Sẵn sàng: Thay đổi các thông số trên thanh thuộc tính để cập nhật khung nhìn CAD trong thời gian thực.",
            "Ready: change the settings on the ViewControlBar to update the CAD view live."));

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
            hudText.Text = Loc.F("Tỉ lệ: {0} | Chi tiết: {1} | Kiểu: {2}",
                "Scale: {0} | Detail: {1} | Style: {2}", bar.ViewScale, bar.DetailLevel, bar.VisualStyle);
            UpdateCallout(Loc.F("Đã cập nhật thiết lập khung nhìn: [{0}]. Mô hình CAD đồng bộ thành công.",
                "View setting updated: [{0}]. The CAD model is in sync.", name));

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

            ToastService.Info(Loc.F("Thiết lập: {0}", "Setting: {0}", name), "ViewControlBar");
        };

        bar.HideIsolateRequested += (_, _) =>
        {
            UpdateCallout(Loc.T("Đã kích hoạt chế độ Ẩn/Cô lập đối tượng tạm thời.",
                "Temporary Hide/Isolate mode is on."));
            ToastService.Info(Loc.T("Ẩn / Cô lập tạm thời", "Temporary hide / isolate"), "ViewControlBar");
        };

        // Viewport Host Grid
        var viewportHost = new Grid();
        viewportHost.Children.Add(viewer);
        viewportHost.Children.Add(hudPill);
        viewportHost.Children.Add(bar);

        var stage = new Border
        {
            Height = 280,
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            ClipToBounds = true,
            Child = viewportHost
        };
        stage.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.CanvasBackground");
        stage.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

        // 3. Khay cau hinh tuong tac 2 cot chuan muc
        var workbench = new Grid { Margin = new Thickness(0, 18, 0, 4) };
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(430) });
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Cot 1: Hinh hoc & Them ti le
        var col1 = new StackPanel();
        var head1 = new TextBlock
        {
            Text = Loc.T("KIỂU DÁNG & HÌNH HỌC", "GEOMETRY & STYLE"),
            FontSize = 11.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 12)
        };
        head1.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        col1.Children.Add(head1);

        var variantRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };
        var varLbl = new TextBlock { Text = Loc.T("Kiểu:", "Style:"), FontSize = 12, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0), Width = 38 };
        varLbl.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        var variantCombo = new ComboBox
        {
            Width = 145,
            ItemsSource = new[]
            {
                Loc.T("Nổi", "Floating"),
                Loc.T("Phẳng", "Seamless"),
                Loc.T("Kính mờ", "Glass")
            },
            SelectedIndex = 2,
            Margin = new Thickness(0, 0, 16, 0)
        };
        variantCombo.SelectionChanged += (_, _) =>
        {
            bar.Variant = (ViewControlBarVariant)Math.Max(0, variantCombo.SelectedIndex);
            UpdateCallout(Loc.F("Đổi kiểu hiển thị sang: {0}", "Switched the style to {0}", bar.Variant));
        };
        variantRow.Children.Add(varLbl);
        variantRow.Children.Add(variantCombo);
        col1.Children.Add(variantRow);

        col1.Children.Add(AppearanceSlider(Loc.T("Cao điều khiển", "Control height"), 20, 40, bar.ControlHeight, 2, "0", v => bar.ControlHeight = v));
        col1.Children.Add(AppearanceSlider(Loc.T("Cỡ biểu tượng", "Icon size"), 10, 22, bar.IconSize, 1, "0", v => bar.IconSize = v));
        col1.Children.Add(AppearanceSlider(Loc.T("Bo góc", "Corner radius"), 0, 20, bar.CornerRadius.TopLeft, 1, "0", v => bar.CornerRadius = new CornerRadius(v)));
        col1.Children.Add(AppearanceSlider(Loc.T("Độ nổi", "Elevation"), 0, 4, bar.Elevation, 1, "0", v => bar.Elevation = (int)v));

        // Scale Custom Add Row
        var scaleRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
        var scaleLbl = new TextBlock { Text = Loc.T("Thêm tỉ lệ:", "Add scale:"), FontSize = 12, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0), Width = 65 };
        scaleLbl.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        var scaleBox = new TextBox { Text = "1:75", Width = 75, Height = 28, Margin = new Thickness(0, 0, 8, 0), VerticalContentAlignment = VerticalAlignment.Center };
        var addScaleBtn = SecondaryButton(Loc.T("Thêm vào danh sách", "Add to list"));
        addScaleBtn.Height = 28;
        addScaleBtn.Padding = new Thickness(10, 2, 10, 2);
        addScaleBtn.Click += (_, _) =>
        {
            string txt = scaleBox.Text?.Trim() ?? string.Empty;
            if (txt.Length > 0 && !scales.Contains(txt))
            {
                scales.Add(txt);
                bar.ViewScale = txt;
                UpdateCallout(Loc.F("Đã thêm tỉ lệ mới: {0} và chọn trực tiếp.",
                    "Added the new scale {0} and selected it.", txt));
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
            Text = Loc.T("NHÓM TÍNH NĂNG & TRẠNG THÁI", "FEATURES & STATUS"),
            FontSize = 11.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 12)
        };
        head2.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        col2.Children.Add(head2);

        var switchesWrap = new WrapPanel { Margin = new Thickness(0, 0, 0, 12) };
        switchesWrap.Children.Add(AppearanceToggle(Loc.T("Tỉ lệ", "Scale"), bar.ShowScale, v => bar.ShowScale = v));
        switchesWrap.Children.Add(AppearanceToggle(Loc.T("Mức chi tiết", "Detail level"), bar.ShowDetailLevel, v => bar.ShowDetailLevel = v));
        switchesWrap.Children.Add(AppearanceToggle(Loc.T("Kiểu hiển thị", "Visual style"), bar.ShowVisualStyle, v => bar.ShowVisualStyle = v));
        switchesWrap.Children.Add(AppearanceToggle(Loc.T("Cụm công tắc", "Toggle cluster"), bar.ShowToggles, v => bar.ShowToggles = v));
        switchesWrap.Children.Add(AppearanceToggle(Loc.T("Ẩn / Cô lập", "Hide / isolate"), bar.ShowHideIsolate, v => bar.ShowHideIsolate = v));
        switchesWrap.Children.Add(AppearanceToggle(Loc.T("Đường phân cách", "Dividers"), bar.ShowDividers, v => bar.ShowDividers = v));
        switchesWrap.Children.Add(AppearanceToggle(Loc.T("Nhãn 'Ẩn'", "Hide caption"), false,
                v => bar.HideIsolateCaption = v ? Loc.T("Ẩn", "Hide") : Loc.T("Ẩn/Cô lập", "Hide")));
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
            Loc.T("Thanh thuộc tính khung nhìn CAD", "CAD view property bar (ViewControlBar)"),
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

        // Đưa gốc toạ độ vào giữa khung ngay từ đầu. Trục X/Y được vẽ tại gốc thế giới, nên nếu
        // camera mặc định để gốc ra ngoài khung thì mọi thuộc tính màu/nét trục đều trông như
        // không có tác dụng — người dùng đổi màu mà không thấy gì đổi.
        viewport.Camera.LookAt(new Point3D(0, 0, 0));
        viewport.Camera.OrthographicHeight = 1600;

        // Điểm bám mẫu: nếu không có, dấu bám chỉ hiện khi con trỏ tình cờ nằm trong bán kính
        // SnapDistance quanh một giao lưới. Người dùng bật/tắt “Hiện dấu bám” / “Hiện nhãn bám”
        // lúc đó sẽ không thấy gì đổi, và tưởng công tắc hỏng. Đặt sẵn một điểm trên giao lưới
        // để hai công tắc có thứ để bật tắt ngay khi trang vừa mở.
        viewport.Loaded += (_, _) =>
        {
            var sample = viewport.ToScreen(new Point3D(1000, 1000, 0));
            if (double.IsNaN(sample.X) || double.IsNaN(sample.Y)) return;

            viewport.Snap = sample;
            viewport.SnapKind = SnapKind.Grid;
        };

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
            Text = Loc.T("6,000 mm (Dầm D1 300×600)", "6,000 mm (beam D1 300×600)"),
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
        // The viewport paints the themed surface, so the pill has to follow it too.
        var hudPill = new Border
        {
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10, 4, 10, 4),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(12),
            IsHitTestVisible = false
        };
        hudPill.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
        hudPill.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

        var hudText = new TextBlock
        {
            Text = Loc.T("[Tỷ lệ: 1:100] [Lưới: 500 mm] [Bắt điểm: Lưới / Điểm cuối] [Hệ tọa độ thế giới]", "[Scale: 1:100] [Grid: 500 mm] [Snap: Grid / Endpoint] [WCS]"),
            FontSize = 10.5,
            FontWeight = FontWeights.Medium
        };
        hudText.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
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
        var statusCallout = CreateCalloutCard(Loc.T("Tọa độ con trỏ: X = 0 mm, Y = 0 mm | Bắt điểm: Sẵn sàng | Di chuyển chuột để xem phản hồi.", "Cursor: X = 0 mm, Y = 0 mm | Snap: ready | Move the mouse to see feedback."));

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
            string snapStatus = e.IsSnapped ? Loc.T("ĐÃ BẮT ĐIỂM", "SNAPPED") : Loc.T("Tự do", "Free");
            UpdateCallout(Loc.T($"Tọa độ: X = {e.World.X:0.#} mm, Y = {e.World.Y:0.#} mm | Trạng thái: {snapStatus}", $"Coordinate: X = {e.World.X:0.#} mm, Y = {e.World.Y:0.#} mm | State: {snapStatus}"));
        };

        viewport.WorldClicked += (_, e) =>
        {
            ToastService.Info(Loc.T($"Nhấp tại: X = {e.World.X:0.#}, Y = {e.World.Y:0.#} mm", $"Clicked at: X = {e.World.X:0.#}, Y = {e.World.Y:0.#} mm"), "Viewport");
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
            Text = Loc.T("LƯỚI & TRỤC TỌA ĐỘ", "GRID & DATUM"),
            FontSize = 11.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 10)
        };
        head1.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        col1.Children.Add(head1);

        // Hai panel định kiểu được khai báo trước để công tắc Hiện lưới / Trục tọa độ bật tắt
        // được chúng: một thanh trượt đổi màu đường trục trong khi trục đang tắt là một control
        // trông như hỏng, vì chẳng có gì trên khung nhìn để nó tác động vào.
        StackPanel? axisStylePanel = null;
        StackPanel? gridStylePanel = null;
        StackPanel? crosshairStylePanel = null;

        var gridToggles = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        gridToggles.Children.Add(AppearanceToggle(Loc.T("Hiện lưới", "Show grid (ShowGrid)"), viewport.ShowGrid, v =>
        {
            viewport.ShowGrid = v;
            if (gridStylePanel is not null) gridStylePanel.IsEnabled = v;
        }));
        gridToggles.Children.Add(AppearanceToggle(Loc.T("Trục tọa độ", "Axes (ShowAxes)"), viewport.ShowAxes, v =>
        {
            viewport.ShowAxes = v;
            if (axisStylePanel is not null) axisStylePanel.IsEnabled = v;
        }));
        gridToggles.Children.Add(AppearanceToggle(Loc.T("Gốc tọa độ", "Origin marker (Origin)"), viewport.ShowOriginMarker, v => viewport.ShowOriginMarker = v));
        gridToggles.Children.Add(AppearanceToggle(Loc.T("Tâm ngắm", "Crosshair (ShowCrosshair)"), viewport.ShowCrosshair, v =>
        {
            viewport.ShowCrosshair = v;
            if (crosshairStylePanel is not null) crosshairStylePanel.IsEnabled = v;
        }));
        gridToggles.Children.Add(AppearanceToggle(Loc.T("Ô ngắm", "Aperture box (Aperture)"), viewport.ShowAperture, v => viewport.ShowAperture = v));
        col1.Children.Add(gridToggles);

        axisStylePanel = new StackPanel();

        // ── Màu & nét của trục toạ độ ──────────────────────────────────────────
        // Ba thuộc tính mở rộng của ViewportControl mà bản demo trước đây không hề lộ ra:
        // AxisBrush (màu), AxisDashStyle (nét đứt / nét liền) và AxisThickness (độ dày).
        var axisHead = new TextBlock
        {
            Text = Loc.T("MÀU & NÉT TRỤC", "AXIS STYLE"),
            FontSize = 10.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 4, 0, 6)
        };
        axisHead.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        axisStylePanel.Children.Add(axisHead);

        // Ô màu: một Border nhỏ, bấm để gán. Border thay cho Button vì theme tô lại nền của Button
        // khi hover, và một ô màu đổi màu khi trỏ vào thì không còn là ô màu nữa.
        var axisSwatches = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };

        void AddSwatch(Panel host, Color color, string name, Action apply)
        {
            var swatch = new Border
            {
                Width = 22,
                Height = 22,
                Margin = new Thickness(0, 0, 6, 0),
                CornerRadius = new CornerRadius(4),
                Background = new SolidColorBrush(color),
                BorderBrush = new SolidColorBrush(Color.FromArgb(0x66, 0x88, 0x88, 0x88)),
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand,
                ToolTip = name
            };
            swatch.MouseLeftButtonUp += (_, _) => apply();
            host.Children.Add(swatch);
        }

        AddSwatch(axisSwatches, Color.FromRgb(0x8A, 0x8A, 0x8A), Loc.T("Xám — theo giao diện", "Grey — follow theme"), () =>
            viewport.SetResourceReference(ViewportControl.AxisBrushProperty, "CONCOST.Brush.TextSecondary"));
        AddSwatch(axisSwatches, Color.FromRgb(0xEF, 0x44, 0x44), Loc.T("Đỏ", "Red"), () =>
            viewport.AxisBrush = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)));
        AddSwatch(axisSwatches, Color.FromRgb(0x25, 0x63, 0xEB), Loc.T("Xanh dương", "Blue"), () =>
            viewport.AxisBrush = new SolidColorBrush(Color.FromRgb(0x25, 0x63, 0xEB)));
        AddSwatch(axisSwatches, Color.FromRgb(0x16, 0xA3, 0x4A), Loc.T("Xanh lá", "Green"), () =>
            viewport.AxisBrush = new SolidColorBrush(Color.FromRgb(0x16, 0xA3, 0x4A)));
        AddSwatch(axisSwatches, Color.FromRgb(0x93, 0x33, 0xEA), Loc.T("Tím", "Purple"), () =>
            viewport.AxisBrush = new SolidColorBrush(Color.FromRgb(0x93, 0x33, 0xEA)));
        AddSwatch(axisSwatches, Color.FromRgb(0xFB, 0x92, 0x3C), Loc.T("Cam", "Amber"), () =>
            viewport.AxisBrush = new SolidColorBrush(Color.FromRgb(0xFB, 0x92, 0x3C)));

        axisStylePanel.Children.Add(axisSwatches);

        var dashRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 6) };
        var dashLabel = new TextBlock
        {
            Text = Loc.T("Nét trục", "Axis dash"),
            Width = 92,
            FontSize = 11.5,
            VerticalAlignment = VerticalAlignment.Center
        };
        dashLabel.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var dashCombo = new ComboBox { Width = 150, Height = 28, MinHeight = 28, FontSize = 11.5 };
        foreach (var caption in new[] { Loc.T("Nét đứt", "Dash"), Loc.T("Nét liền", "Solid"), Loc.T("Chấm", "Dot"), Loc.T("Gạch-chấm", "Dash-dot") })
            dashCombo.Items.Add(new ComboBoxItem { Content = caption });

        dashCombo.SelectedIndex = 0;
        dashCombo.SelectionChanged += (_, _) =>
        {
            viewport.AxisDashStyle = dashCombo.SelectedIndex switch
            {
                1 => DashStyles.Solid,
                2 => DashStyles.Dot,
                3 => DashStyles.DashDot,
                _ => DashStyles.Dash
            };
        };

        dashRow.Children.Add(dashLabel);
        dashRow.Children.Add(dashCombo);
        axisStylePanel.Children.Add(dashRow);

        axisStylePanel.Children.Add(AppearanceSlider(Loc.T("Độ dày trục", "Axis thickness (AxisThickness)"), 0.5, 4, viewport.AxisThickness, 0.1, "{0:0.0}", v => viewport.AxisThickness = v));

        axisStylePanel.IsEnabled = viewport.ShowAxes;
        axisStylePanel.ToolTip = Loc.T("Bật “Trục tọa độ” để thấy thay đổi trên khung nhìn.", "Turn on “Axes (ShowAxes)” to see the change in the viewport.");
        col1.Children.Add(axisStylePanel);

        crosshairStylePanel = new StackPanel { Margin = new Thickness(0, 8, 0, 0) };
        var crosshairHead = new TextBlock
        {
            Text = Loc.T("MÀU & NÉT TÂM NGẮM", "CROSSHAIR STYLE"),
            FontSize = 10.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 6)
        };
        crosshairHead.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        crosshairStylePanel.Children.Add(crosshairHead);

        var crosshairSwatches = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        AddSwatch(crosshairSwatches, Color.FromRgb(0x8A, 0x8A, 0x8A), Loc.T("Xám — theo giao diện", "Grey — follow theme"), () =>
            viewport.ClearValue(ViewportControl.CrosshairBrushProperty));
        AddSwatch(crosshairSwatches, Color.FromRgb(0x25, 0x63, 0xEB), Loc.T("Xanh dương", "Blue"), () =>
            viewport.CrosshairBrush = new SolidColorBrush(Color.FromRgb(0x25, 0x63, 0xEB)));
        AddSwatch(crosshairSwatches, Color.FromRgb(0x16, 0xA3, 0x4A), Loc.T("Xanh lá", "Green"), () =>
            viewport.CrosshairBrush = new SolidColorBrush(Color.FromRgb(0x16, 0xA3, 0x4A)));
        crosshairStylePanel.Children.Add(crosshairSwatches);
        crosshairStylePanel.Children.Add(AppearanceSlider(Loc.T("Độ dày", "Thickness (CrosshairThickness)"), 0.25, 3, viewport.CrosshairThickness, 0.25, "{0:0.00} px", v => viewport.CrosshairThickness = v));
        crosshairStylePanel.Children.Add(AppearanceSlider(Loc.T("Chiều dài", "Extent (CrosshairExtent)"), 0.2, 1, viewport.CrosshairExtent, 0.1, "{0:0%}", v => viewport.CrosshairExtent = v));

        var crosshairDashRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 6) };
        var crosshairDashLabel = new TextBlock
        {
            Text = Loc.T("Kiểu nét", "Dash style"),
            Width = 92,
            FontSize = 11.5,
            VerticalAlignment = VerticalAlignment.Center
        };
        crosshairDashLabel.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        var crosshairDashCombo = new ComboBox { Width = 150, Height = 28, MinHeight = 28, FontSize = 11.5 };
        foreach (string caption in new[] { Loc.T("Chấm", "Dot"), Loc.T("Gạch", "Dash"), Loc.T("Liền", "Solid"), Loc.T("Gạch-chấm", "Dash-dot") })
            crosshairDashCombo.Items.Add(new ComboBoxItem { Content = caption });
        crosshairDashCombo.SelectedIndex = 0;
        crosshairDashCombo.SelectionChanged += (_, _) => viewport.CrosshairDashStyle = crosshairDashCombo.SelectedIndex switch
        {
            1 => DashStyles.Dash,
            2 => DashStyles.Solid,
            3 => DashStyles.DashDot,
            _ => DashStyles.Dot
        };
        crosshairDashRow.Children.Add(crosshairDashLabel);
        crosshairDashRow.Children.Add(crosshairDashCombo);
        crosshairStylePanel.Children.Add(crosshairDashRow);
        crosshairStylePanel.IsEnabled = viewport.ShowCrosshair;
        col1.Children.Add(crosshairStylePanel);

        // Màu lưới: đặt cả nét mảnh lẫn nét đậm, nét đậm lấy cùng màu ở độ đậm cao hơn.
        var gridHead = new TextBlock
        {
            Text = Loc.T("MÀU LƯỚI", "GRID COLOUR"),
            FontSize = 10.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 6, 0, 6)
        };
        gridHead.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        gridStylePanel = new StackPanel();
        gridStylePanel.Children.Add(gridHead);

        var gridSwatches = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };

        void SetGrid(Color color, byte minorAlpha, byte majorAlpha)
        {
            viewport.GridBrush = new SolidColorBrush(Color.FromArgb(minorAlpha, color.R, color.G, color.B));
            viewport.GridMajorBrush = new SolidColorBrush(Color.FromArgb(majorAlpha, color.R, color.G, color.B));
        }

        AddSwatch(gridSwatches, Color.FromRgb(0x8A, 0x8A, 0x8A), Loc.T("Xám — theo giao diện", "Grey — follow theme"), () =>
        {
            viewport.ClearValue(ViewportControl.GridBrushProperty);
            viewport.ClearValue(ViewportControl.GridMajorBrushProperty);
        });
        AddSwatch(gridSwatches, Color.FromRgb(0x25, 0x63, 0xEB), Loc.T("Xanh dương", "Blue"), () => SetGrid(Color.FromRgb(0x25, 0x63, 0xEB), 0x30, 0x70));
        AddSwatch(gridSwatches, Color.FromRgb(0x16, 0xA3, 0x4A), Loc.T("Xanh lá", "Green"), () => SetGrid(Color.FromRgb(0x16, 0xA3, 0x4A), 0x30, 0x70));
        AddSwatch(gridSwatches, Color.FromRgb(0x93, 0x33, 0xEA), Loc.T("Tím", "Purple"), () => SetGrid(Color.FromRgb(0x93, 0x33, 0xEA), 0x30, 0x70));
        AddSwatch(gridSwatches, Color.FromRgb(0xEF, 0x44, 0x44), Loc.T("Đỏ", "Red"), () => SetGrid(Color.FromRgb(0xEF, 0x44, 0x44), 0x28, 0x60));

        gridStylePanel.Children.Add(gridSwatches);
        gridStylePanel.Children.Add(AppearanceSlider(Loc.T("Bước lưới", "Grid step (GridStep)"), 100, 2000, viewport.GridStep, 100, "{0:0} mm", v => { viewport.GridStep = v; hudText.Text = Loc.F("[Tỷ lệ: 1:100] [Lưới: {0:0} mm] [Bắt điểm: Lưới / Điểm cuối] [Hệ tọa độ thế giới]", "[Scale: 1:100] [Grid: {0:0} mm] [Snap: Grid / Endpoint] [WCS]", v); }));
        gridStylePanel.IsEnabled = viewport.ShowGrid;
        gridStylePanel.ToolTip = Loc.T("Bật “Hiện lưới” để thấy thay đổi trên khung nhìn.", "Turn on “Show grid (ShowGrid)” to see the change in the viewport.");
        col1.Children.Add(gridStylePanel);

        col1.Children.Add(AppearanceSlider(Loc.T("Cỡ dấu bắt điểm", "Snap marker size (SnapMarkerSize)"), 4, 16, viewport.SnapMarkerSize, 1, "{0:0} px", v => viewport.SnapMarkerSize = v));
        col1.Children.Add(AppearanceSlider(Loc.T("Độ mờ vùng chọn", "Selection opacity (SelectionOpacity)"), 0, 0.4, viewport.SelectionFillOpacity, 0.05, "{0:0.00}", v => viewport.SelectionFillOpacity = v));

        Grid.SetColumn(col1, 0);
        workbench.Children.Add(col1);

        // Cột 2: Tính năng bắt điểm & Thao tác CAD nhanh
        var col2 = new StackPanel();
        var head2 = new TextBlock
        {
            Text = Loc.T("BẮT ĐIỂM & THAO TÁC KHUNG NHÌN", "SNAPPING & ACTIONS"),
            FontSize = 11.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 10)
        };
        head2.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        col2.Children.Add(head2);

        var snapToggles = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        snapToggles.Children.Add(AppearanceToggle(Loc.T("Bám lưới", "Snap to grid (SnapToGrid)"), viewport.SnapToGrid, v => viewport.SnapToGrid = v));
        snapToggles.Children.Add(AppearanceToggle(Loc.T("Hiện dấu bám", "Show snap marker (ShowSnapMarker)"), viewport.ShowSnapMarker, v => viewport.ShowSnapMarker = v));
        snapToggles.Children.Add(AppearanceToggle(Loc.T("Hiện nhãn bám", "Show snap label (ShowSnapLabel)"), viewport.ShowSnapLabel, v => viewport.ShowSnapLabel = v));
        col2.Children.Add(snapToggles);

        // Ô vuông cam trong khung nhìn chính là dấu bắt điểm (snap marker) — tô bằng
        // SnapMarkerBrush, mặc định lấy màu accent của theme. Đổi màu tại đây.
        var snapHead = new TextBlock
        {
            Text = Loc.T("MÀU DẤU BẮT ĐIỂM", "SNAP MARKER COLOUR"),
            FontSize = 10.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 2, 0, 6)
        };
        snapHead.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        col2.Children.Add(snapHead);

        var snapSwatches = new WrapPanel { Margin = new Thickness(0, 0, 0, 10) };

        void AddSnapSwatch(Color color, string name, Action apply)
        {
            var swatch = new Border
            {
                Width = 22,
                Height = 22,
                Margin = new Thickness(0, 0, 6, 0),
                CornerRadius = new CornerRadius(4),
                Background = new SolidColorBrush(color),
                BorderBrush = new SolidColorBrush(Color.FromArgb(0x66, 0x88, 0x88, 0x88)),
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand,
                ToolTip = name
            };
            swatch.MouseLeftButtonUp += (_, _) => apply();
            snapSwatches.Children.Add(swatch);
        }

        AddSnapSwatch(Color.FromRgb(0xFB, 0x92, 0x3C), Loc.T("Cam — màu chủ đạo (mặc định)", "Amber — accent (default)"), () =>
            viewport.ClearValue(ViewportControl.SnapMarkerBrushProperty));
        AddSnapSwatch(Color.FromRgb(0x22, 0xC5, 0x5E), Loc.T("Xanh lá", "Green"), () =>
            viewport.SnapMarkerBrush = new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E)));
        AddSnapSwatch(Color.FromRgb(0xEF, 0x44, 0x44), Loc.T("Đỏ", "Red"), () =>
            viewport.SnapMarkerBrush = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)));
        AddSnapSwatch(Color.FromRgb(0x25, 0x63, 0xEB), Loc.T("Xanh dương", "Blue"), () =>
            viewport.SnapMarkerBrush = new SolidColorBrush(Color.FromRgb(0x25, 0x63, 0xEB)));
        AddSnapSwatch(Color.FromRgb(0x93, 0x33, 0xEA), Loc.T("Tím", "Purple"), () =>
            viewport.SnapMarkerBrush = new SolidColorBrush(Color.FromRgb(0x93, 0x33, 0xEA)));

        col2.Children.Add(snapSwatches);

        // Nút thao tác nhanh
        var actRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
        var btnZoomExtents = SecondaryButton(Loc.T("Thu phóng toàn khung", "Zoom Extents"));
        btnZoomExtents.Click += (_, _) =>
        {
            viewport.ZoomToFit(new Rect3D(0, 0, 0, 12000, 8000, 0));
            UpdateCallout(Loc.T("Đã thu phóng toàn bộ bản vẽ (12,000 × 8,000 mm).", "Zoomed to the whole drawing (Zoom Extents: 12,000 × 8,000 mm)."));
        };

        var btnToggleGrid = SecondaryButton(Loc.T("Bật/Tắt Lưới", "Toggle grid"));
        btnToggleGrid.Click += (_, _) =>
        {
            viewport.ToggleGrid();
            UpdateCallout(Loc.F("Đã chuyển đổi trạng thái lưới: {0}", "Grid toggled: {0}",
                viewport.ShowGrid ? Loc.T("BẬT", "ON") : Loc.T("TẮT", "OFF")));
        };

        var btnOrigin = SecondaryButton(Loc.T("Về gốc toạ độ", "Go to origin"));
        btnOrigin.Click += (_, _) =>
        {
            viewport.Camera.LookAt(new Point3D(0, 0, 0));
            viewport.Camera.OrthographicHeight = 1600;
            UpdateCallout(Loc.T("Đã đưa khung nhìn về gốc toạ độ — trục X/Y và vòng tròn gốc nằm giữa khung.", "Viewport returned to the origin — the X/Y axes and origin ring sit in the middle."));
        };

        var btnAddBeam = SecondaryButton(Loc.T("+ Thêm Dầm CAD", "+ Add CAD beam"));
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
            UpdateCallout(Loc.T("Đã thêm Dầm phụ D2 (300×400) cao độ +3.600m vào bản vẽ.", "Added secondary beam D2 (300×400) at +3.600 m."));
        };

        actRow.Children.Add(btnZoomExtents);
        actRow.Children.Add(btnToggleGrid);
        actRow.Children.Add(btnOrigin);
        actRow.Children.Add(btnAddBeam);
        col2.Children.Add(actRow);

        col2.Children.Add(statusCallout);

        Grid.SetColumn(col2, 2);
        workbench.Children.Add(col2);

        var stageHost = new StackPanel();
        stageHost.Children.Add(stage);
        stageHost.Children.Add(workbench);

        string xamlCode = Loc.Code(@"<ui:ViewportControl Height=""340""
                    GridStep=""500""
                    GridMajorEvery=""5""
                    ShowGrid=""True""
                    ShowCrosshair=""True""
                    CrosshairBrush=""#64748B""
                    CrosshairThickness=""1.2""
                    CrosshairDashStyle=""Dash""
                    CrosshairExtent=""0.75""
                    ShowAperture=""True""
                    SnapToGrid=""True""
                    ShowAxes=""True""
                    ShowOriginMarker=""True""
                    CursorMoved=""OnCursorMoved""
                    WorldClicked=""OnWorldClicked"" />

<!-- Màu và nét của trục toạ độ: ba thuộc tính mở rộng, không cần template riêng -->
<ui:ViewportControl ShowAxes=""True""
                    AxisBrush=""#EF4444""
                    AxisThickness=""2""
                    AxisDashStyle=""Solid"" />

<!-- Màu lưới: nét mảnh và nét đậm (mỗi GridMajorEvery ô) đặt riêng -->
<ui:ViewportControl ShowGrid=""True""
                    GridBrush=""#302563EB""
                    GridMajorBrush=""#702563EB""
                    GridMajorEvery=""5"" />");

        string csCode = Loc.Code(@"// 1. Cấu hình lưới tọa độ & điểm bám CAD
viewport.GridStep = 500;             // Bước lưới chính (500 mm)
viewport.GridMajorEvery = 5;         // Cứ mỗi 5 ô lưới có một đường nét đậm
viewport.SnapToGrid = true;          // Bắt điểm vào lưới
viewport.ShowCrosshair = true;       // Tâm ngắm kỹ thuật
viewport.ShowAperture = true;        // Ô ngắm vuông xung quanh tâm
viewport.CrosshairBrush = new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B));
viewport.CrosshairThickness = 1.2;   // Độ dày nét, 0.25–8 px
viewport.CrosshairDashStyle = DashStyles.Dash;
viewport.CrosshairExtent = 0.75;     // Tỷ lệ chiều dài nét trên mỗi trục (0–1)

// 2. Màu và nét của trục toạ độ
viewport.AxisBrush = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
viewport.AxisThickness = 2;          // 0.5 – 4 px
viewport.AxisDashStyle = DashStyles.Solid;   // Dash | Solid | Dot | DashDot

// 3. Màu lưới: nét mảnh và nét đậm riêng, alpha quyết định độ đậm
viewport.GridBrush = new SolidColorBrush(Color.FromArgb(0x30, 0x25, 0x63, 0xEB));
viewport.GridMajorBrush = new SolidColorBrush(Color.FromArgb(0x70, 0x25, 0x63, 0xEB));

// 4. Thu phóng vừa khung hình bản vẽ
viewport.ZoomToFit(new Rect3D(0, 0, 0, 12000, 8000, 0));

// 5. Tương tác con trỏ và bám điểm thời gian thực
viewport.CursorMoved += (s, e) =>
{
    Point3D worldPos = e.World;      // Tọa độ thực trong không gian CAD (mm)
    bool isSnapped = e.IsSnapped;    // Đang bám điểm hay tự do
};");

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Khung nhìn kỹ thuật CAD", "Technical CAD viewport (ViewportControl)"),
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
            new CommandDefinition("LINE", Loc.T("Vẽ thanh liên tục", "Draw a continuous polyline")) { Prompt = "Specify first point" },
            new CommandDefinition("CIRCLE", Loc.T("Vẽ đường tròn kết cấu", "Draw a structural circle")).WithAliases("C"),
            new CommandDefinition("ZOOM", Loc.T("Phóng to / thu nhỏ khung nhìn", "Zoom the viewport in / out")).WithAliases("Z"),
            new CommandDefinition("SAVE", Loc.T("Lưu bản vẽ kết cấu CAD", "Save the CAD structural drawing")).WithAliases("QSAVE"),
            new CommandDefinition("ERASE", Loc.T("Xóa đối tượng đã chọn", "Erase the selected objects")).WithAliases("E", "DEL")
        });

        // A command line that opens empty reads as a broken text field. Seed the banner a real
        // CAD shell prints before the first prompt.
        commandLine.WriteLine(Loc.T("CONCOST CAD — Dòng lệnh kỹ thuật", "CONCOST CAD — technical command line (AutoCAD-style)"), CommandMessageKind.Hint);
        commandLine.WriteLine(Loc.T("Bản vẽ: Mat-bang-ket-cau-tang-1.dwg · Tỷ lệ 1:100 · Đơn vị: mm", "Drawing: Mat-bang-ket-cau-tang-1.dwg · scale 1:100 · units: mm"));
        commandLine.WriteLine(Loc.T("Nhập lệnh hoặc bấm LỆNH MẪU bên dưới · Tab: gợi ý · F2: mở rộng lịch sử", "Type a command or press a QUICK COMMAND below · Tab: suggestions · F2: expand history"));

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
                    commandLine.WriteLine(Loc.T("Vẽ đoạn thẳng liên tục: Nhập tọa độ hoặc click điểm tiếp theo.", "Continuous polyline: type a coordinate or click the next point."));
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
                    commandLine.WriteLine(Loc.T("Đã vẽ đường tròn CIRCLE bán kính R = 400 mm tại tọa độ (2400, 1200).", "CIRCLE drawn with R = 400 mm at (2400, 1200)."));
                    break;

                case "ZOOM":
                    commandLine.WriteLine(Loc.T("Khung nhìn: ZOOM [All/Extents/Window] — Đã vừa khớp toàn bộ bản vẽ.", "Viewport: ZOOM [All/Extents/Window] — the whole drawing now fits."));
                    break;

                case "SAVE":
                    commandLine.WriteLine(Loc.T("Đang lưu bản vẽ: Mat-bang-ket-cau-tang-1.dwg...", "Saving drawing: Mat-bang-ket-cau-tang-1.dwg..."));
                    commandLine.WriteLine(Loc.T("Lưu thành công: 0 cảnh báo, 0 lỗi, kích thước 4.2 MB.", "Saved: 0 warnings, 0 errors, 4.2 MB."));
                    ToastService.Success(Loc.T("Đã lưu bản vẽ kết cấu DWG thành công!", "Structural DWG saved."), "AutoCAD CLI");
                    break;

                case "ERASE":
                    points.Clear();
                    Redraw();
                    commandLine.WriteLine(Loc.T("Đã xóa các đối tượng sketch đang vẽ.", "Sketch objects erased."));
                    break;
            }
        };

        commandLine.PromptCompleted += (_, e) =>
        {
            if (e.Prompt == "Specify first point")
            {
                if (!CadInputParser.TryParseCoordinate(e.Text, out Point3D start))
                {
                    commandLine.WriteLine(Loc.T("Yêu cầu tọa độ dạng: x,y (ví dụ: 1000,500)", "Enter a coordinate as x,y (for example 1000,500)"), CommandMessageKind.Error);
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
                commandLine.WriteLine(Loc.T("*Undo*, đã quay lại đỉnh trước đó.", "*Undo*, back to the previous vertex."), CommandMessageKind.Hint);
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
                commandLine.WriteLine(Loc.T($"Đã khép góc đa tuyến: {points.Count - 1} đoạn.", $"Polyline closed: {points.Count - 1} segment(s)."));
                return;
            }

            if (e.Text.Length == 0)
            {
                commandLine.WriteLine(Loc.T($"Đã hoàn tất lệnh vẽ: {Math.Max(0, points.Count - 1)} đoạn.", $"Drawing finished: {Math.Max(0, points.Count - 1)} segment(s)."));
                return;
            }

            if (!CadInputParser.TryParseCoordinate(e.Text, out Point3D next))
            {
                commandLine.WriteLine(Loc.T($"\"{e.Text}\" không phải là tọa độ hợp lệ. Hãy gõ x,y", $"\"{e.Text}\" is not a valid coordinate. Type x,y"), CommandMessageKind.Error);
                AskForPoint();
                return;
            }

            points.Add(ToCanvas(next));
            Redraw();
            AskForPoint();
        };

        commandLine.UnknownCommand += (_, raw) =>
            commandLine.WriteLine(Loc.T($"Lệnh không nhận dạng được: \"{raw}\". Bấm phím Tab để xem danh sách lệnh.", $"Unknown command: \"{raw}\". Press Tab for the command list."), CommandMessageKind.Error);

        var stage = new Border
        {
            Height = 280,
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
            Text = Loc.T("[BẢN VẼ: MẶT BẰNG KẾT CẤU TẦNG 1] [TỶ LỆ: 1:100] [DÒNG LỆNH: SẴN SÀNG]", "[DRAWING: LEVEL 1 STRUCTURAL PLAN] [SCALE: 1:100] [COMMAND LINE: READY]"),
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8))
        };

        var paper = new Grid();
        paper.Children.Add(sheet);
        paper.Children.Add(hudBadge);

        // The command line is docked under the drawing, not inside it: a light strip painted over
        // the dark canvas reads as a hole in the viewport, and the history would eat into the
        // drawing's height.
        stage.Child = paper;

        // 3. Khay cấu hình tương tác 2 cột chuẩn Desktop CAD
        var workbench = new Grid { Margin = new Thickness(0, 14, 0, 4) };
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(430) });
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Cột 1: Kiểu dáng và kích thước
        var col1 = new StackPanel();
        var head1 = new TextBlock
        {
            Text = Loc.T("KIỂU DÁNG VÀ LỊCH SỬ", "GEOMETRY & HISTORY"),
            FontSize = 11.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 10)
        };
        head1.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        col1.Children.Add(head1);

        var variantRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
        var varLbl = new TextBlock { Text = Loc.T("Kiểu:", "Variant:"), FontSize = 12, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0), Width = 38 };
        varLbl.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        var variantCombo = new ComboBox
        {
            Width = 200,
            ItemsSource = new[] { Loc.T("Gắn đáy màn hình", "Docked (bottom of screen)"), Loc.T("Phẳng nội dòng", "Inline (flat)"), Loc.T("Nổi", "Floating") },
            SelectedIndex = 0,
            Margin = new Thickness(0, 0, 16, 0)
        };
        variantCombo.SelectionChanged += (_, _) =>
            commandLine.Variant = (CommandLineVariant)Math.Max(0, variantCombo.SelectedIndex);
        variantRow.Children.Add(varLbl);
        variantRow.Children.Add(variantCombo);
        col1.Children.Add(variantRow);

        col1.Children.Add(AppearanceSlider(Loc.T("Chiều cao lịch sử", "History height"), 60, 200, commandLine.HistoryHeight, 10, "{0:0} px", v => commandLine.HistoryHeight = v));
        col1.Children.Add(AppearanceSlider(Loc.T("Chiều cao mở rộng", "Expanded height"), 150, 400, commandLine.ExpandedHistoryHeight, 20, "{0:0} px", v => commandLine.ExpandedHistoryHeight = v));

        Grid.SetColumn(col1, 0);
        workbench.Children.Add(col1);

        // Cột 2: Phím tắt & Thao tác lệnh nhanh
        var col2 = new StackPanel();
        var head2 = new TextBlock
        {
            Text = Loc.T("LỆNH MẪU & TƯƠNG TÁC NHANH", "QUICK COMMANDS"),
            FontSize = 11.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 10)
        };
        head2.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        col2.Children.Add(head2);

        var switches = new WrapPanel { Margin = new Thickness(0, 0, 0, 10) };
        switches.Children.Add(AppearanceToggle(Loc.T("Hiện lịch sử", "Show history"), commandLine.ShowHistory, v => commandLine.ShowHistory = v));
        switches.Children.Add(AppearanceToggle(Loc.T("Mở rộng lịch sử (F2)", "Expand history (F2)"), commandLine.IsHistoryExpanded, v => commandLine.IsHistoryExpanded = v));
        col2.Children.Add(switches);

        // Nút thực thi lệnh nhanh
        var actRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
        var btnLine = SecondaryButton(Loc.T("Lệnh LINE", "LINE"));
        btnLine.Click += (_, _) => commandLine.Execute("LINE");

        var btnCircle = SecondaryButton(Loc.T("Lệnh CIRCLE", "CIRCLE"));
        btnCircle.Click += (_, _) => commandLine.Execute("CIRCLE");

        var btnZoom = SecondaryButton(Loc.T("Lệnh ZOOM", "ZOOM"));
        btnZoom.Click += (_, _) => commandLine.Execute("ZOOM");

        var btnSave = SecondaryButton(Loc.T("Lệnh QSAVE", "QSAVE"));
        btnSave.Click += (_, _) => commandLine.Execute("SAVE");

        var btnErase = SecondaryButton(Loc.T("Lệnh ERASE", "ERASE"));
        btnErase.Click += (_, _) => commandLine.Execute("ERASE");

        actRow.Children.Add(btnLine);
        actRow.Children.Add(btnCircle);
        actRow.Children.Add(btnZoom);
        actRow.Children.Add(btnSave);
        actRow.Children.Add(btnErase);
        col2.Children.Add(actRow);

        var cmdCallout = CreateCalloutCard(
            Loc.T("HƯỚNG DẪN DÒNG LỆNH CAD:\n", "CAD COMMAND LINE GUIDE:\n") +
            Loc.T("• Gõ lệnh hoặc bấm nút nhanh ở trên: LINE, CIRCLE, ZOOM, SAVE, ERASE.\n", "• Type a command or press a quick button above: LINE, CIRCLE, ZOOM, SAVE, ERASE.\n") +
            Loc.T("• Phím Tab: Tự động hoàn thành tên lệnh.\n", "• Tab: command auto-completion.\n") +
            Loc.T("• Phím Mũi tên Lên / Xuống: Xem lại lịch sử các lệnh đã gõ.\n", "• Up / Down arrows: recall the commands you typed.\n") +
            Loc.T("• Phím F2: Phóng to / Thu gọn hộp thoại lịch sử dòng lệnh.", "• F2: expand / collapse the command history."));
        col2.Children.Add(cmdCallout);

        Grid.SetColumn(col2, 2);
        workbench.Children.Add(col2);

        var stageHost = new StackPanel();
        stageHost.Children.Add(stage);
        stageHost.Children.Add(commandLine);
        stageHost.Children.Add(workbench);

        string xamlCode = @"<ui:CommandLine Variant=""Docked""
                HistoryHeight=""96""
                ExpandedHistoryHeight=""240""
                ShowHistory=""True""
                CommandInvoked=""OnCommandInvoked""
                PromptCompleted=""OnPromptCompleted"" />";

        string csCode = Loc.Code(@"// 1. Đăng ký tập lệnh CAD và phím tắt Alias
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
};");

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Dòng lệnh CAD kỹ thuật", "CAD command line (CommandLine)"),
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

        var sheet = new Canvas { ClipToBounds = true };

        // 1.1 Lưới kỹ thuật CAD (Blueprint Grid Lines)
        // A tiled brush, not one line per column: the old loop stopped at a hard-coded 900 px, so
        // on a wide card the right-hand side of the sheet had no grid at all.
        var gridPen = new Pen(new SolidColorBrush(Color.FromArgb(0x25, 0x38, 0xBD, 0xF8)), 1);
        gridPen.Freeze();

        var gridTile = new GeometryGroup();
        gridTile.Children.Add(new LineGeometry(new Point(0, 0), new Point(40, 0)));
        gridTile.Children.Add(new LineGeometry(new Point(0, 0), new Point(0, 40)));
        gridTile.Freeze();

        var gridBrush = new DrawingBrush(new GeometryDrawing(null, gridPen, gridTile))
        {
            TileMode = TileMode.Tile,
            Viewport = new Rect(0, 0, 40, 40),
            ViewportUnits = BrushMappingMode.Absolute,
            Viewbox = new Rect(0, 0, 40, 40),
            ViewboxUnits = BrushMappingMode.Absolute,
            Stretch = Stretch.None
        };
        gridBrush.Freeze();

        var stageGrid = new Grid();
        stageGrid.Children.Add(new System.Windows.Shapes.Rectangle { Fill = gridBrush, IsHitTestVisible = false });
        stageGrid.Children.Add(sheet);

        // 1.2 Các đoạn dầm / tường đã vẽ xong trước đó (Pre-committed Structural Walls)
        var wall1 = new System.Windows.Shapes.Line { X1 = 120, Y1 = 215, X2 = 120, Y2 = 55, Stroke = new SolidColorBrush(Color.FromRgb(0x02, 0x84, 0xC7)), StrokeThickness = 2.5, IsHitTestVisible = false };
        var wall2 = new System.Windows.Shapes.Line { X1 = 120, Y1 = 55, X2 = 560, Y2 = 55, Stroke = new SolidColorBrush(Color.FromRgb(0x02, 0x84, 0xC7)), StrokeThickness = 2.5, IsHitTestVisible = false };
        sheet.Children.Add(wall1);
        sheet.Children.Add(wall2);

        // Kích thước đoạn đã vẽ
        var dimCommitted = new TextBlock { Text = "6,000 mm", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8)), IsHitTestVisible = false };
        Canvas.SetLeft(dimCommitted, 340);
        Canvas.SetTop(dimCommitted, 37);
        sheet.Children.Add(dimCommitted);

        // Điểm góc dầm
        void AddVertexDot(double vx, double vy)
        {
            var dot = new System.Windows.Shapes.Rectangle { Width = 6, Height = 6, Fill = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)), IsHitTestVisible = false };
            Canvas.SetLeft(dot, vx - 3);
            Canvas.SetTop(dot, vy - 3);
            sheet.Children.Add(dot);
        }
        AddVertexDot(120, 215);
        AddVertexDot(120, 55);

        // Ký hiệu bắt điểm Snap Marker (Green Square) tại điểm gốc
        var snapMarker = new Border { Width = 10, Height = 10, BorderBrush = new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E)), BorderThickness = new Thickness(1.5), Background = Brushes.Transparent, IsHitTestVisible = false };
        Canvas.SetLeft(snapMarker, 560 - 5);
        Canvas.SetTop(snapMarker, 55 - 5);
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
        // Opaque plate: at 25 % alpha the ray showed straight through the chip and the value
        // read as a smudge on the stroke.
        var angleBadge = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(0xF2, 0x0F, 0x17, 0x2A)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x99, 0xF5, 0x9E, 0x0B)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(3),
            Padding = new Thickness(5, 1, 5, 1),
            IsHitTestVisible = false
        };
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
            Prompt = Loc.T("Chiều dài", "Length"),
            Text = "3200",
            FieldWidth = 130,
            Elevation = 1
        };
        sheet.Children.Add(input);

        // 1.5 HUD Overlay góc trên trái & UCS góc dưới trái
        var hudBadge = new Border { Background = new SolidColorBrush(Color.FromArgb(0x50, 0x00, 0x00, 0x00)), CornerRadius = new CornerRadius(4), Padding = new Thickness(8, 4, 8, 4), IsHitTestVisible = false };
        hudBadge.Child = new TextBlock { Text = Loc.T("[Lệnh: Vẽ đa tuyến] [Chế độ: Nhập liệu động (F12)] [Tỉ lệ: 1:100]", "[Command: PLINE (polyline)] [Mode: Dynamic Input (F12)] [Scale: 1:100]"), FontSize = 10, Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)) };
        Canvas.SetLeft(hudBadge, 12);
        Canvas.SetTop(hudBadge, 10);
        sheet.Children.Add(hudBadge);

        var ucsStack = new StackPanel { Orientation = Orientation.Horizontal, IsHitTestVisible = false };
        ucsStack.Children.Add(new TextBlock { Text = "Y ↑ ", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E)) });
        ucsStack.Children.Add(new TextBlock { Text = "X →", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)) });
        Canvas.SetLeft(ucsStack, 14);
        Canvas.SetTop(ucsStack, 252);
        sheet.Children.Add(ucsStack);

        stage.Child = stageGrid;

        // Điểm gốc và điểm chuột ban đầu
        Point origin = new(560, 55);
        Point cursor = new(707, 116.8);

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
            Canvas.SetLeft(angleBadge, origin.X + 12);
            Canvas.SetTop(angleBadge, origin.Y + 10);

            Canvas.SetLeft(snapMarker, origin.X - 5);
            Canvas.SetTop(snapMarker, origin.Y - 5);
        }

        UpdateVisuals();
        input.Open(cursor, Loc.T("Chiều dài", "Length"), "3200");

        // ---------------------------------------------------------------------
        // 2. Bảng điều khiển tham số 2 cột cân xứng (2-Column Balanced Deck)
        // ---------------------------------------------------------------------
        var controlGrid = new Grid { Margin = new Thickness(0, 14, 0, 10) };
        controlGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        controlGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Cột 0: Kiểu dáng & Hình học
        var colStyle = new StackPanel { Margin = new Thickness(0, 0, 14, 0) };
        colStyle.Children.Add(CadGroupHeader(Loc.T("KIỂU DÁNG & HÌNH HỌC", "APPEARANCE & GEOMETRY (HUD)")));

        var comboRow = new Grid { Margin = new Thickness(0, 4, 0, 8) };
        comboRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        comboRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var varStack = new StackPanel { Margin = new Thickness(0, 0, 6, 0) };
        varStack.Children.Add(CadHint(Loc.T("Kiểu hiển thị:", "Variant:")));
        var variantCombo = new ComboBox
        {
            Height = 28,
            ItemsSource = new[] { Loc.T("Nổi chuẩn CAD", "Floating (CAD standard)"), Loc.T("Gọn 1 dòng", "Compact (single line)"), Loc.T("Kính mờ kỹ thuật", "Glass (technical frosted)") },
            SelectedIndex = 0
        };
        varStack.Children.Add(variantCombo);
        Grid.SetColumn(varStack, 0);
        comboRow.Children.Add(varStack);

        var placeStack = new StackPanel { Margin = new Thickness(6, 0, 0, 0) };
        placeStack.Children.Add(CadHint(Loc.T("Vị trí neo:", "Anchor:")));
        var placeCombo = new ComboBox
        {
            Height = 28,
            ItemsSource = new[] { Loc.T("Tự động lật", "Automatic (auto-flip)"), Loc.T("Bên phải", "Right"), Loc.T("Bên trái", "Left"), Loc.T("Phía trên", "Above") },
            SelectedIndex = 0
        };
        placeStack.Children.Add(placeCombo);
        Grid.SetColumn(placeStack, 1);
        comboRow.Children.Add(placeStack);
        colStyle.Children.Add(comboRow);

        colStyle.Children.Add(AppearanceSlider(Loc.T("Rộng ô", "Field width"), 90, 220, input.FieldWidth, 10, "{0:0} px", v => input.FieldWidth = v));
        colStyle.Children.Add(AppearanceSlider(Loc.T("Bo góc", "Corner radius"), 0, 16, input.CornerRadius.TopLeft, 1, "{0:0} px", v => input.CornerRadius = new CornerRadius(v)));
        colStyle.Children.Add(AppearanceSlider(Loc.T("Độ nổi bóng", "Elevation"), 0, 4, input.Elevation, 1, "{0:0}", v => input.Elevation = (int)v));

        Grid.SetColumn(colStyle, 0);
        controlGrid.Children.Add(colStyle);

        // Cột 1: Tính năng nhập liệu & Chế độ bắt điểm
        var colFeatures = new StackPanel { Margin = new Thickness(14, 0, 0, 0) };
        colFeatures.Children.Add(CadGroupHeader(Loc.T("TÍNH NĂNG NHẬP LIỆU & BẮT ĐIỂM", "INPUT & SNAP (CAD DRAFTING MODES)")));

        var togglesWrap = new WrapPanel { Margin = new Thickness(0, 4, 0, 10) };
        togglesWrap.Children.Add(AppearanceToggle(Loc.T("Đơn vị (mm)", "Unit (mm)"), input.ShowUnit, v => input.ShowUnit = v));
        togglesWrap.Children.Add(AppearanceToggle(Loc.T("Đường gióng con trỏ", "Cursor leader"), input.ShowLeader, v => input.ShowLeader = v));
        togglesWrap.Children.Add(AppearanceToggle(Loc.T("Chọn hết khi mở", "Select all on open"), input.SelectAllOnOpen, v => input.SelectAllOnOpen = v));
        colFeatures.Children.Add(togglesWrap);

        // Thẻ hiển thị trạng thái thời gian thực
        var statusCard = CreateCalloutCard(Loc.T("Đang vẽ đoạn số 3: Chiều dài L = 3,200 mm | Góc nghiêng α = 22.8° | Gõ chiều dài rồi nhấn phím xác nhận để hoàn tất.", "Drawing segment 3: length L = 3,200 mm | angle α = 22.8° | type a length then press Enter."));
        colFeatures.Children.Add(statusCard);

        void SetStatus(string message)
        {
            // Keep the plate and its icon: swapping Child for a bare TextBlock dropped the
            // two-column layout and the theme foreground on the first mouse move.
            if (statusCard.Child is not Grid grid) return;

            foreach (var child in grid.Children)
            {
                if (child is TextBlock tb)
                {
                    tb.Text = message;
                    return;
                }
            }
        }

        // Hàng nút thao tác nhanh
        var actRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
        var btnEnter = SecondaryButton(Loc.T("✓ Vẽ đoạn", "✓ Draw segment (Enter)"));
        var btnSample3200 = SecondaryButton("3,200 mm");
        var btnSampleAngle = SecondaryButton(Loc.T("Góc 45°", "Angle 45°"));
        var btnReset = SecondaryButton(Loc.T("↺ Đặt lại bản vẽ", "↺ Reset drawing"));

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
            SetStatus(Loc.T($"Đã đổi kiểu DynamicInput sang: {input.Variant}", $"DynamicInput variant changed to: {input.Variant}"));
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
            SetStatus(Loc.T($"Vị trí hiển thị: {input.Placement} (Tự động lật khi chạm mép khung)", $"Placement: {input.Placement} (auto-flips at the viewport edge)"));
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

            SetStatus(Loc.T($"Con trỏ: X={cursor.X * 10:0} mm, Y={cursor.Y * 10:0} mm | Chiều dài: {lenMm:0} mm | Góc: {angleDeg:0.0}°", $"Cursor: X={cursor.X * 10:0} mm, Y={cursor.Y * 10:0} mm | Length: {lenMm:0} mm | Angle: {angleDeg:0.0}°"));
        };

        void CommitPoint(Point newPoint)
        {
            drawn.Points.Add(origin);
            drawn.Points.Add(newPoint);
            AddVertexDot(newPoint.X, newPoint.Y);
            origin = newPoint;
            cursor = new Point(newPoint.X + 80, newPoint.Y - 40);
            UpdateVisuals();
            input.Open(cursor, Loc.T("Chiều dài", "Length"), "3200");
            ToastService.Success(Loc.T($"Đã vẽ thành công đoạn dầm: {measure.Text}", $"Beam segment drawn: {measure.Text}"), "DynamicInput");
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
            origin = new Point(560, 55);
            cursor = new Point(707, 116.8);
            UpdateVisuals();
            input.Open(cursor, Loc.T("Chiều dài", "Length"), "3200");
            ToastService.Info(Loc.T("Đã đặt lại bản vẽ về điểm ban đầu.", "Drawing reset to the starting point."), "DynamicInput");
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
            Loc.T("Hộp nhập liệu con trỏ kỹ thuật CAD", "CAD dynamic input (DynamicInput)"),
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
            csharp: Loc.Code(@"// Mở hộp nhập liệu bám tại toạ độ con trỏ chuột
LiveInput.Open(cursorPoint, ""Chiều dài"", ""3200"");
LiveInput.MoveTo(newPoint);

LiveInput.ValueCommitted += (_, e) =>
{
    // Nhận độ dài số thực và vẽ đoạn thẳng trực tiếp
    double lengthMm = e.Number;
    CommitSegment(lengthMm);
};")));

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

        // A card with a backdrop is a dark stage, and the caption has to survive on it.
        if (backdrop is not null)
            label.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextInverse");

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

        var statusCallout = CreateCalloutCard(Loc.T(
            "Sẵn sàng: Bấm các công tắc trên thanh trạng thái CAD để xem phản hồi thời gian thực.",
            "Ready: press the toggles on the CAD StatusBar to see live state feedback."));

        void UpdateCallout(string msg)
        {
            if (statusCallout.Child is Grid g)
            {
                var tb = g.Children.OfType<TextBlock>().FirstOrDefault(t => Grid.GetColumn(t) == 1);
                if (tb != null) tb.Text = msg;
            }
        }

        bar.ToggleChanged += (_, e) => UpdateCallout(Loc.F("Công tắc: {0} = {1}", "Toggle: {0} = {1}",
            e.Toggle, e.IsEnabled ? Loc.T("BẬT", "ON") : Loc.T("TẮT", "OFF")));
        bar.WorkPlaneChanged += (_, _) =>
            UpdateCallout(Loc.F("Mặt phẳng làm việc UCS: {0}", "UCS work plane: {0}", bar.WorkPlane));
        bar.NotificationSelected += (_, note) =>
        {
            UpdateCallout(Loc.F("Thông báo được chọn: {0} — {1}", "Notification selected: {0} — {1}",
                note.Title, note.Message));
            ToastService.Info(note.Message, note.Title);
        };

        int selection = 3;
        var selectBtn = SecondaryButton(Loc.T("Chọn thêm (+1)", "Select one more (+1)"));
        selectBtn.Click += (_, _) =>
        {
            selection++;
            bar.SelectionCount = selection;
            UpdateCallout(Loc.F("Đã chọn: {0}", "Selected: {0}", CadStatusBar.SelectionSummary(selection)));
        };

        var resetSelectBtn = SecondaryButton(Loc.T("Bỏ chọn (0)", "Clear selection (0)"));
        resetSelectBtn.Click += (_, _) =>
        {
            selection = 0;
            bar.SelectionCount = selection;
            UpdateCallout(Loc.T("Đã bỏ chọn tất cả đối tượng.", "Cleared the selection."));
        };

        var addTaskBtn = SecondaryButton(Loc.T("Chạy tác vụ nền", "Run background task"));
        addTaskBtn.Click += (_, _) =>
        {
            var task = new CadTask(Loc.T("Đang bóc khối lượng dự án", "Taking off project quantities"), 0);
            bar.AddTask(task);
            UpdateCallout(Loc.T("Bắt đầu tác vụ nền: Đang bóc khối lượng...",
                "Background task started: taking off quantities..."));

            var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(120) };
            timer.Tick += (_, _) =>
            {
                task.Percent += 8;
                bar.RefreshTasks();
                if (task.Percent < 100) return;

                timer.Stop();
                bar.RemoveTask(task);
                bar.Notify(new CadNotification(
                    Loc.T("Bóc khối lượng hoàn tất", "Quantity take-off finished"),
                    Loc.T("3 214 cấu kiện kết cấu", "3,214 structural members"),
                    CadNotificationSeverity.Success));
                UpdateCallout(Loc.T("Tác vụ nền đã xong: 3 214 cấu kiện được bóc tách thành công!",
                    "Background task finished: 3,214 members taken off."));
            };
            timer.Start();
        };

        var notifyBtn = SecondaryButton(Loc.T("Phát thông báo", "Raise a notification"));
        notifyBtn.Click += (_, _) =>
        {
            bar.Notify(new CadNotification(
                Loc.T("Bản vẽ đã lưu thành công", "Drawing saved successfully"),
                "Model_Level18.rvt", CadNotificationSeverity.Info));
            UpdateCallout(Loc.T("Đã phát thông báo hệ thống mới.", "Raised a new system notification."));
        };

        // 2. CAD Viewport Stage mo phong chan thuc
        var stageCanvas = new Grid();

        var bgGrid = new Border();
        bgGrid.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.CanvasBackground");
        stageCanvas.Children.Add(bgGrid);

        var cadWatermark = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        cadWatermark.Children.Add(new TextBlock
        {
            Text = Loc.T("KHUNG VẼ BẢN VẼ CAD (2D/3D)", "CAD DRAWING VIEWPORT (2D/3D)"),
            FontSize = 13,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(160, 148, 163, 184)),
            HorizontalAlignment = HorizontalAlignment.Center
        });
        cadWatermark.Children.Add(new TextBlock
        {
            Text = Loc.T("Rê chuột trên bản vẽ cập nhật toạ độ theo thời gian thực",
                "Move the pointer over the drawing to update coordinates live"),
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
            Text = Loc.T("KIỂU DÁNG VÀ HÌNH HỌC", "GEOMETRY & STYLE"),
            FontSize = 11.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 12)
        };
        head1.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        col1.Children.Add(head1);

        var variantRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };
        var varLbl = new TextBlock { Text = Loc.T("Kiểu:", "Style:"), FontSize = 12, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0), Width = 38 };
        varLbl.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        var variantCombo = new ComboBox
        {
            Width = 145,
            ItemsSource = new[]
            {
                Loc.T("Nổi", "Floating"),
                Loc.T("Phẳng", "Seamless"),
                Loc.T("Kính mờ", "Glass")
            },
            SelectedIndex = 0,
            Margin = new Thickness(0, 0, 16, 0)
        };
        variantCombo.SelectionChanged += (_, _) =>
        {
            bar.Variant = (CadStatusBarVariant)Math.Max(0, variantCombo.SelectedIndex);
            UpdateCallout(Loc.F("Đổi kiểu hiển thị thanh trạng thái sang: {0}", "Switched the CadStatusBar style to {0}", bar.Variant));
        };
        variantRow.Children.Add(varLbl);
        variantRow.Children.Add(variantCombo);
        col1.Children.Add(variantRow);

        col1.Children.Add(AppearanceSlider(Loc.T("Cao nút", "Button height"), 20, 36, bar.ControlHeight, 2, "0", v => bar.ControlHeight = v));
        col1.Children.Add(AppearanceSlider(Loc.T("Cỡ biểu tượng", "Icon size"), 10, 20, bar.IconSize, 1, "0", v => bar.IconSize = v));
        col1.Children.Add(AppearanceSlider(Loc.T("Bo góc", "Corner radius"), 0, 16, bar.CornerRadius.TopLeft, 1, "0", v => bar.CornerRadius = new CornerRadius(v)));
        col1.Children.Add(AppearanceSlider(Loc.T("Độ nổi", "Elevation"), 0, 4, bar.Elevation, 1, "0", v => bar.Elevation = (int)v));

        Grid.SetColumn(col1, 0);
        workbench.Children.Add(col1);

        // Cot 2: Tinh nang, tac vu va hanh dong
        var col2 = new StackPanel();
        var head2 = new TextBlock
        {
            Text = Loc.T("TÍNH NĂNG VÀ TRẠNG THÁI HOẠT ĐỘNG", "FEATURES & STATUS"),
            FontSize = 11.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 12)
        };
        head2.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        col2.Children.Add(head2);

        var switches = new WrapPanel { Margin = new Thickness(0, 0, 0, 10) };
        switches.Children.Add(AppearanceToggle(Loc.T("Tóm tắt chọn", "Selection summary"), bar.ShowSelection, v => bar.ShowSelection = v));
        switches.Children.Add(AppearanceToggle(Loc.T("Mặt phẳng tọa độ", "UCS work plane"), bar.ShowWorkPlane, v => bar.ShowWorkPlane = v));
        switches.Children.Add(AppearanceToggle(Loc.T("Cụm công tắc", "Toggle cluster"), bar.ShowToggles, v => bar.ShowToggles = v));
        switches.Children.Add(AppearanceToggle(Loc.T("Nhãn công tắc", "SNAP/ORTHO labels"), bar.ShowToggleLabels, v => bar.ShowToggleLabels = v));
        switches.Children.Add(AppearanceToggle(Loc.T("Tác vụ nền", "Background tasks"), bar.ShowTaskQueue, v => bar.ShowTaskQueue = v));
        switches.Children.Add(AppearanceToggle(Loc.T("Thông báo", "Notifications"), bar.ShowNotifications, v => bar.ShowNotifications = v));
        switches.Children.Add(AppearanceToggle(Loc.T("Toạ độ", "Coordinates"), bar.ShowCoordinates, v => bar.ShowCoordinates = v));
        switches.Children.Add(AppearanceToggle(Loc.T("Đường phân cách", "Dividers"), bar.ShowDividers, v => bar.ShowDividers = v));
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
            Loc.T("Thanh trạng thái CAD", "CAD status bar (CadStatusBar)"),
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
        var project = browser.AddRoot(
            Loc.T("Tháp Landmark 2026 (45 tầng)", "Landmark Tower 2026 (45 floors)"), ProjectNodeKind.Project);

        var views = project.Add(Loc.T("Mặt Bằng & Khung Nhìn", "Plans & views"), ProjectNodeKind.Folder);
        views.Add(Loc.T("Mặt Bằng Tầng 1 (+0.000)", "Level 1 Plan (+0.000)"), ProjectNodeKind.View).IsModified = true;
        views.Add(Loc.T("Mặt Bằng Tầng 2 (+3.600)", "Level 2 Plan (+3.600)"), ProjectNodeKind.View);
        views.Add(Loc.T("Mặt Bằng Tầng 18 (+64.800)", "Level 18 Plan (+64.800)"), ProjectNodeKind.View);
        views.Add(Loc.T("Mặt Đứng Trục 1-12 (Hướng Nam)", "Grid 1-12 Elevation (South)"), ProjectNodeKind.View);
        views.Add(Loc.T("Mặt Cắt Điển Hình A-A", "Typical Section A-A"), ProjectNodeKind.View);
        views.Add(Loc.T("Khung Không Gian 3D Trung Tâm", "Central 3D Space Frame"), ProjectNodeKind.View);

        var sheets = project.Add(Loc.T("Tập Bản Vẽ Phát Hành", "Issued sheets"), ProjectNodeKind.Sheet);
        sheets.Add(Loc.T("KT-01: Mặt Bằng Tổng Thể 1:500", "A-01: General Arrangement Plan 1:500"), ProjectNodeKind.Sheet);
        sheets.Add(Loc.T("KC-18: Mặt Bằng Kết Cấu Tầng 18 1:100", "S-18: Level 18 Structural Plan 1:100"), ProjectNodeKind.Sheet);
        sheets.Add(Loc.T("MEP-06: Sơ Đồ Cấp Thoát Nước", "MEP-06: Water Supply & Drainage Diagram"), ProjectNodeKind.Sheet);

        var schedules = project.Add(Loc.T("Bảng Thống Kê & Dự Toán", "Schedules & quantities"), ProjectNodeKind.Schedule);
        schedules.Add(Loc.T("Tiên Lượng Bê Tông Cốt Thép Tầng 18", "Level 18 Concrete & Rebar Quantities"), ProjectNodeKind.Schedule);
        schedules.Add(Loc.T("Bảng Dự Toán Chi Phí Gói Thầu Kết Cấu", "Structural Package Cost Estimate"), ProjectNodeKind.Schedule);
        schedules.Add(Loc.T("Thống Kê Khối Lượng Thép Hình SS400", "SS400 Structural Steel Take-off"), ProjectNodeKind.Schedule);

        var families = project.Add(Loc.T("Thư Viện Cấu Kiện BIM", "BIM families"), ProjectNodeKind.Family);
        families.Add(Loc.T("Cột Bê Tông Cốt Thép C600x600", "RC Column C600x600"), ProjectNodeKind.Family);
        families.Add(Loc.T("Dầm Bê Tông B30 D300x600", "RC Beam B30 D300x600"), ProjectNodeKind.FamilySymbol);
        families.Add(Loc.T("Cửa Đi 2 Cánh Kính Thủy Lực D-0912", "Double-Leaf Hydraulic Glass Door D-0912"), ProjectNodeKind.FamilySymbol);

        var levels = project.Add(Loc.T("Cao Độ & Lưới Trục", "Levels & grid"), ProjectNodeKind.Level);
        levels.Add(Loc.T("Cao Độ Tầng Hầm B1 (-3.600)", "Basement B1 Level (-3.600)"), ProjectNodeKind.Level);
        levels.Add(Loc.T("Cao Độ Tầng Trệt L1 (+0.000)", "Ground Floor L1 Level (+0.000)"), ProjectNodeKind.Level);
        levels.Add(Loc.T("Cao Độ Tầng Điển Hình L18 (+64.800)", "Typical Floor L18 Level (+64.800)"), ProjectNodeKind.Level);

        browser.Refresh();
    }

    private static TextBlock PanelCaption(string text)
    {
        var caption = new TextBlock
        {
            Text = text.ToUpperInvariant(),
            FontSize = 10.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 6)
        };
        caption.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        return caption;
    }

    private object CreateProjectBrowserDemo()
    {
        // A browser is a palette, not a page. It is staged at the width it really lives at inside a
        // dock, and the stage takes whatever room the panel leaves, so the example is the largest
        // object on its own page instead of the smallest.
        var browser = new ProjectBrowser
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Height = 420,
            Variant = ProjectBrowserVariant.Card,
            SearchPlaceholder = Loc.T("Tìm khung nhìn, bản vẽ, cấu kiện…", "Find a view, sheet, family…"),
            AllKindsCaption = Loc.T("Mọi loại", "All types"),
            SortCaptions = new[] { Loc.T("Tên A→Z", "Name A→Z"), Loc.T("Tên Z→A", "Name Z→A"), Loc.T("Theo loại", "By type") },
            EmptyText = Loc.T("Không có mục nào khớp bộ lọc.", "No items match the filter."),
            CountCaption = Loc.T("{0} mục", "{0} items")
        };

        FillProjectTree(browser);
        browser.ExpandAll();

        var readout = CadHint(Loc.T(
            "Mở một khung nhìn bằng cách nhấp đúp; nhấp chuột phải để Đổi tên / Nhân bản / Xóa.",
            "Double-click to open a view; right-click to Rename / Duplicate / Delete."));

        browser.OpenRequested += (_, e) =>
        {
            readout.Text = Loc.F("Mở {0}", "Open {0}", e.Node.Name);
            ToastService.Info(Loc.F("Mở {0}", "Open {0}", e.Node.Name), "Project Browser");
        };

        browser.DeleteRequested += (_, e) => readout.Text = Loc.F("Xoá {0} (ứng dụng chủ quyết định)", "Delete {0} (the host decides)", e.Node.Name);

        browser.SelectionChanged += (_, _) => readout.Text = Loc.F("Đang chọn: {0}", "Selected: {0}", browser.SelectedNode?.Name ?? Loc.T("(không)", "(none)"));

        // No stage behind it: the palette draws its own border, and a grey plate under that border is
        // a second frame around a control that already has one. It sits on the card's own surface.
        browser.VerticalAlignment = VerticalAlignment.Top;

        // ---------------------------------------------------------------- appearance panel
        // No fixed width: the slider rows are 401 px wide (label + track + value badge), and a 330 px
        // column clipped every badge off the right edge.
        var panel = new StackPanel { VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Stretch };

        // Two pickers, two captions: unlabelled, they read as one list of styles, and the page looked
        // like it had three styles and no Revit tree.
        panel.Children.Add(PanelCaption(Loc.T("Kiểu nền", "Plate")));

        var variant = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Height = 27,
            MinHeight = 0,
            Margin = new Thickness(0, 0, 0, 10),
            ItemsSource = new[] { Loc.T("Thẻ — tự vẽ viền", "Card — the browser draws its own border"), Loc.T("Phẳng — bảng vẽ viền", "Seamless — the palette draws the border"), Loc.T("Nổi — nổi trên bản vẽ", "Floating — lifts above the drawing") },
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

        panel.Children.Add(PanelCaption(Loc.T("Kiểu cây", "Tree style")));

        // The row treatment is a separate axis from the plate: a classic tree still has to decide
        // whether the browser or the palette draws the border around it.
        var treeStyle = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Height = 27,
            MinHeight = 0,
            Margin = new Thickness(0, 0, 0, 10),
            ItemsSource = new[]
            {
                Loc.T("Cây hiện đại — bo tròn, mũi tên gập", "Modern tree — rounded pill, chevron"),
                Loc.T("Cây cổ điển — kiểu Revit, ô +/−", "Classic tree — the Revit boxed +/−")
            },
            SelectedIndex = 0
        };

        treeStyle.SelectionChanged += (_, _) =>
            browser.TreeStyle = treeStyle.SelectedIndex == 1
                ? ProjectBrowserTreeStyle.Classic
                : ProjectBrowserTreeStyle.Modern;

        panel.Children.Add(treeStyle);

        panel.Children.Add(PanelCaption(Loc.T("Hiển thị", "Shown")));

        // Two per row, not one: five accent-filled boxes stacked down the page were the loudest
        // thing on it, louder than the palette they were supposed to describe.
        // Three columns rather than a wrap panel: six switches wrapped 5 + 1 and left a lone box on
        // its own row.
        var chromeToggles = new System.Windows.Controls.Primitives.UniformGrid
        {
            Columns = 3,
            Margin = new Thickness(0, 0, 0, 4)
        };
        chromeToggles.Children.Add(AppearanceToggle(Loc.T("Thanh công cụ", "Toolbar"), true, value => browser.ShowToolbar = value));
        chromeToggles.Children.Add(AppearanceToggle(Loc.T("Ô tìm kiếm", "Search box"), true, value => browser.ShowSearch = value));
        chromeToggles.Children.Add(AppearanceToggle(Loc.T("Lọc theo loại", "Filter by type"), true, value => browser.ShowKindFilter = value));
        chromeToggles.Children.Add(AppearanceToggle(Loc.T("Sắp xếp", "Sort"), true, value => browser.ShowSort = value));
        chromeToggles.Children.Add(AppearanceToggle(Loc.T("Nhóm theo loại", "Group by type"), true, value => browser.ShowGroup = value));
        chromeToggles.Children.Add(AppearanceToggle(Loc.T("Dòng đếm", "Count row"), true, value => browser.ShowCount = value));
        panel.Children.Add(chromeToggles);
        var sliders = new StackPanel();
        sliders.Children.Add(AppearanceSlider(Loc.T("Cao nút", "Button height"), 18, 34, 24, 1, "{0:0} px", value => browser.ControlHeight = value));
        sliders.Children.Add(AppearanceSlider(Loc.T("Cỡ biểu tượng", "Icon size"), 10, 18, 13, 1, "{0:0} px", value => browser.IconSize = value));
        sliders.Children.Add(AppearanceSlider(Loc.T("Bo góc", "Corner radius"), 0, 14, 8, 1, "{0:0} px", value => browser.CornerRadius = new CornerRadius(value)));
        sliders.Children.Add(AppearanceSlider(Loc.T("Độ nổi", "Elevation"), 0, 4, 0, 1, "{0:0}", value => browser.Elevation = (int)Math.Round(value)));

        var filterEmpty = new Button
        {
            Content = Loc.T("Bộ lọc không khớp", "No matching filter"),
            Height = 28,
            MinHeight = 0,
            Padding = new Thickness(12, 0, 12, 0),
            Margin = new Thickness(0, 4, 8, 0)
        };
        filterEmpty.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        filterEmpty.Click += (_, _) => browser.SetSearchText(Loc.T("không có gì", "no match"));

        var filterClear = new Button
        {
            Content = Loc.T("Xoá lọc", "Clear filter"),
            Height = 28,
            MinHeight = 0,
            Padding = new Thickness(12, 0, 12, 0),
            Margin = new Thickness(0, 4, 0, 0)
        };
        filterClear.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        filterClear.Click += (_, _) => browser.SetSearchText(string.Empty);

        var filterRow = new StackPanel { Orientation = Orientation.Horizontal };
        filterRow.Children.Add(filterEmpty);
        filterRow.Children.Add(filterClear);

        var actions = new StackPanel { Margin = new Thickness(0, 14, 0, 0) };
        actions.Children.Add(filterRow);
        actions.Children.Add(readout);

        // The control takes the card's width, the actions sit under it, and the switches follow in two
        // columns. The card's content host measures with an infinite width, so a capped, centred grid
        // collapses to its content instead of holding a container width — star columns that stretch
        // are the only shape that fills at every window size.
        var work = new Grid { Margin = new Thickness(0, 22, 0, 0) };
        work.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        work.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
        work.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        Grid.SetColumn(panel, 0);
        work.Children.Add(panel);

        Grid.SetColumn(sliders, 2);
        work.Children.Add(sliders);

        var layout = new StackPanel();
        layout.Children.Add(browser);
        layout.Children.Add(actions);
        layout.Children.Add(work);

        var root = new StackPanel();

        // One palette, and every switch that changes it on the right: the page is a workbench, not a
        // gallery. A second copy on the page would show the same thing the combo already does.
        root.Children.Add(DemoSection(
            Loc.T("Cửa sổ dự án", "Project window"),
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
                    Loc.T("// Lọc giữ nguyên đường dẫn tới node khớp\n", "// The filter keeps the path to every matching node\n") +
                    "browser.SetSearchText(\"lev\");\n\n" +
                    Loc.T("// Nhóm theo loại, sắp xếp theo tên hoặc loại\n", "// Group by kind, sort by name or kind\n") +
                    "browser.GroupMode = ProjectGroupMode.ByKind;\n" +
                    "browser.SortMode = ProjectSortMode.NameAscending;\n\n" +
                    Loc.T("// Mọi nhãn đều là thuộc tính, nên palette dịch được\n", "// Every label is a property, so the palette is translatable\n") +
                    Loc.T("browser.AllKindsCaption = \"Mọi loại\";\n", "browser.AllKindsCaption = \"All kinds\";\n") +
                    Loc.T("browser.CountCaption = \"{0} mục\";", "browser.CountCaption = \"{0} items\";")));

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
        planTag.Child = new TextBlock { Text = Loc.T("📐 Mặt Bằng Kết Cấu Tầng 18 | Tỉ lệ 1:100", "📐 Level 18 Structural Plan | Scale 1:100"), FontSize = 11, Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)) };
        Canvas.SetLeft(planTag, 20);
        Canvas.SetTop(planTag, 215);
        planCanvas.Children.Add(planTag);
        workspace.AddDocument(Loc.T("Mặt Bằng Tầng 18", "Level 18 Floor Plan"), planCanvas, IconKind.FileCAD);

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
        workspace.AddDocument(Loc.T("Khung Không Gian 3D", "3D Structural Frame"), viewer3D, IconKind.Viewport3D);

        // Tab 3: Bảng Tiên Lượng Khối Lượng & Chi Phí (Cost Schedule)
        var scheduleBorder = new Border { Background = new SolidColorBrush(Color.FromRgb(0x0F, 0x17, 0x2A)), Padding = new Thickness(14) };
        var scheduleStack = new StackPanel();
        var scheduleHeader = new TextBlock { Text = Loc.T("📊 BẢNG TIÊN LƯỢNG KHỐI LƯỢNG & DỰ TOÁN KẾT CẤU (TCVN 2026)", "📊 QUANTITY & COST TAKE-OFF SCHEDULE (TCVN 2026)"), FontSize = 12, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)), Margin = new Thickness(0, 0, 0, 10) };
        scheduleStack.Children.Add(scheduleHeader);

        var tableGrid = new Grid();
        tableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        tableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
        tableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        tableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
        tableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
        tableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });

        string[] headers = { Loc.T("Mã CK", "Mark"), Loc.T("Tiết diện", "Section"), Loc.T("Dài (m)", "Length (m)"), Loc.T("Bê tông (m³)", "Concrete (m³)"), Loc.T("Thép (kg)", "Steel (kg)"), Loc.T("Thành tiền (đ)", "Amount (VND)") };
        tableGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        for (int h = 0; h < headers.Length; h++)
        {
            var th = new TextBlock { Text = headers[h], FontSize = 11, FontWeight = FontWeights.SemiBold, Foreground = new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8)), Margin = new Thickness(4, 4, 4, 8) };
            Grid.SetRow(th, 0);
            Grid.SetColumn(th, h);
            tableGrid.Children.Add(th);
        }

        string[][] rowData = {
            new[] { "D1-T18", "300 × 600", "7.5", "1.35 m³", "185.0 kg", Loc.T("8,500,000 đ", "8,500,000 VND") },
            new[] { "D2-T18", "250 × 500", "6.0", "0.75 m³", "95.5 kg", Loc.T("4,350,000 đ", "4,350,000 VND") },
            new[] { "C1-T18", "600 × 600", "3.6", "1.30 m³", "240.0 kg", Loc.T("11,200,000 đ", "11,200,000 VND") },
            new[] { "C2-T18", "500 × 500", "3.6", "0.90 m³", "165.0 kg", Loc.T("7,800,000 đ", "7,800,000 VND") }
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
        totalBadge.Child = new TextBlock { Text = Loc.T("💰 TỔNG GIÁ TRỊ: 31,850,000 VNĐ  |  Bê tông: 4.30 m³  |  Thép: 685.5 kg", "💰 TOTAL: 31,850,000 VND  |  Concrete: 4.30 m³  |  Steel: 685.5 kg"), FontSize = 11, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(0x34, 0xD3, 0x99)) };
        scheduleStack.Children.Add(totalBadge);

        scheduleBorder.Child = scheduleStack;
        workspace.AddDocument(Loc.T("Dự Toán Chi Phí A101", "A101 Cost Estimate"), scheduleBorder, IconKind.FileText);

        var readout = CadHint(Loc.F("{0} cửa sổ làm việc đồng thời — chế độ hiển thị {1}",
            "{0} documents open at once — tile mode {1}", workspace.Documents.Count, workspace.TileMode));

        workspace.DocumentClosed += (_, pane) => readout.Text = Loc.F("Đã đóng cửa sổ: {0}", "Closed the window: {0}", pane.Title);

        workspace.DocumentClosing += (_, e) =>
        {
            if (!e.Pane.IsDirty) return;
            e.Cancel = true;
            ToastService.Warning(Loc.F("{0} còn thay đổi chưa lưu.", "{0} still has unsaved changes.", e.Pane.Title), "Document Workspace");
        };

        var tiles = new ComboBox
        {
            Width = 160,
            Height = 28,
            ItemsSource = new[] { Loc.T("Đơn lẻ", "Single"), Loc.T("Chia dọc", "Vertical split"), Loc.T("Chia ngang", "Horizontal split"), Loc.T("Lưới 2x2", "2x2 grid") },
            SelectedIndex = 0
        };

        tiles.SelectionChanged += (_, _) =>
        {
            workspace.TileMode = (DocumentTileMode)tiles.SelectedIndex;
            readout.Text = Loc.F("Chế độ chia cửa sổ: {0} — {1} khung nhìn", "Split mode: {0} — {1} panes", workspace.TileMode, workspace.PaneCount);
        };

        var closeAll = SecondaryButton(Loc.T("Đóng tất cả", "Close all"));
        closeAll.Click += (_, _) => workspace.CloseAll();

        var dirty = SecondaryButton(Loc.T("Đánh dấu chưa lưu (*)", "Mark unsaved (*)"));
        dirty.Click += (_, _) =>
        {
            foreach (var pane in workspace.Documents) pane.IsDirty = true;
            readout.Text = Loc.T("Tất cả cửa sổ đang ở trạng thái chưa lưu (*) — Thử đóng thẻ để kiểm tra chặn đóng an toàn.", "Every window has unsaved changes (*) — try closing a tab to see the safe-close guard.");
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
            Loc.T("Vùng cửa sổ tài liệu CAD/BIM đa khung", "Multi-frame CAD/BIM document workspace (DocumentWorkspace)"),
            content,
            xaml: @"<ui:DocumentWorkspace x:Name=""Workspace"" Height=""350""
                      DocumentClosing=""OnDocumentClosing"" />",
            csharp: Loc.Code(@"// Thêm các tài liệu đa dạng: Mặt bằng 2D, Khung 3D, Dự toán
workspace.AddDocument(""Mặt Bằng Tầng 18"", planCanvas, IconKind.FileCAD);
workspace.AddDocument(""Khung 3D"", viewer3D, IconKind.Viewport3D);
workspace.AddDocument(""Dự Toán A101"", scheduleGrid, IconKind.FileText);

// Thay đổi bố cục hiển thị cửa sổ (Tile Mode)
workspace.TileMode = DocumentTileMode.Vertical;")));

        return root;
    }

    private object CreatePropertiesPaletteDemo()
    {
        var undo = new UndoRedoManager();
        var properties = new PropertyManager(new TransactionManager(undo));
        var selection = new SelectionManager();

        var beam = new CadDemoBeam();
        var column = new CadDemoColumn();
        var wall = new CadDemoWall();
        object? activeTarget = beam;

        // 1. Đăng ký thuộc tính Dầm bê tông cốt thép (RC Beam)
        properties.Register<CadDemoBeam>(
            new PropertyDescriptor("Length", target => ((CadDemoBeam)target).Length, (target, value) => ((CadDemoBeam)target).Length = Convert.ToDouble(value))
            {
                DisplayName = Loc.T("Chiều dài (L)", "Length (L)"),
                Category = Loc.T("Kích thước hình học", "Geometry"),
                Editor = PropertyEditorKind.Numeric,
                Unit = "mm",
                DefaultValue = 6000.0,
                Validator = (_, value) => value is double length && length > 0 ? null : Loc.T("Chiều dài dầm phải dương (> 0).", "Beam length must be positive (> 0).")
            },
            new PropertyDescriptor("Height", target => ((CadDemoBeam)target).Height, (target, value) => ((CadDemoBeam)target).Height = Convert.ToDouble(value))
            {
                DisplayName = Loc.T("Chiều cao tiết diện (H)", "Section depth (H)"),
                Category = Loc.T("Kích thước hình học", "Geometry"),
                Editor = PropertyEditorKind.Numeric,
                Unit = "mm",
                DefaultValue = 400.0,
                Validator = (_, value) => value is double h && h > 0 ? null : Loc.T("Chiều cao phải dương.", "Depth must be positive.")
            },
            new PropertyDescriptor("Width", target => ((CadDemoBeam)target).Width, (target, value) => ((CadDemoBeam)target).Width = Convert.ToDouble(value))
            {
                DisplayName = Loc.T("Bề rộng dầm (B)", "Section width (B)"),
                Category = Loc.T("Kích thước hình học", "Geometry"),
                Editor = PropertyEditorKind.Numeric,
                Unit = "mm",
                DefaultValue = 250.0,
                Validator = (_, value) => value is double b && b > 0 ? null : Loc.T("Bề rộng phải dương.", "Width must be positive.")
            },
            new PropertyDescriptor("Material", target => ((CadDemoBeam)target).Material, (target, value) => ((CadDemoBeam)target).Material = value as string ?? string.Empty)
            {
                DisplayName = Loc.T("Cấp độ bền bê tông", "Concrete grade"),
                Category = Loc.T("Vật liệu & Tiêu chuẩn", "Materials & Specs"),
                Editor = PropertyEditorKind.Choice,
                Choices = new object[] { Loc.T("Bê tông B25", "Concrete B25"), Loc.T("Bê tông B30", "Concrete B30"), Loc.T("Bê tông B35", "Concrete B35"), Loc.T("Thép hình SS400", "SS400 structural steel") },
                DefaultValue = Loc.T("Bê tông B25", "Concrete B25")
            },
            new PropertyDescriptor("RebarGrade", target => ((CadDemoBeam)target).RebarGrade, (target, value) => ((CadDemoBeam)target).RebarGrade = value as string ?? string.Empty)
            {
                DisplayName = Loc.T("Cấp độ bền cốt thép", "Rebar grade"),
                Category = Loc.T("Vật liệu & Tiêu chuẩn", "Materials & Specs"),
                Editor = PropertyEditorKind.Choice,
                Choices = new object[] { "CB400-V", "CB300-V", "CB500-V" },
                DefaultValue = "CB400-V"
            },
            new PropertyDescriptor("Story", target => ((CadDemoBeam)target).Story)
            {
                DisplayName = Loc.T("Tầng định vị", "Story level"),
                Category = Loc.T("Vị trí & Ràng buộc", "Identity & Constraints")
            },
            new PropertyDescriptor("IsLocked", target => ((CadDemoBeam)target).IsLocked, (target, value) => ((CadDemoBeam)target).IsLocked = Convert.ToBoolean(value))
            {
                DisplayName = Loc.T("Khoá vị trí cấu kiện", "Lock member position"),
                Category = Loc.T("Vị trí & Ràng buộc", "Identity & Constraints"),
                Editor = PropertyEditorKind.Boolean
            });

        // 2. Đăng ký thuộc tính Cột thép tổ hợp (Steel Column)
        properties.Register<CadDemoColumn>(
            new PropertyDescriptor("Height", target => ((CadDemoColumn)target).Height, (target, value) => ((CadDemoColumn)target).Height = Convert.ToDouble(value))
            {
                DisplayName = Loc.T("Chiều cao tầng (H)", "Story height (H)"),
                Category = Loc.T("Kích thước hình học", "Geometry"),
                Editor = PropertyEditorKind.Numeric,
                Unit = "mm",
                DefaultValue = 3600.0,
                Validator = (_, value) => value is double h && h > 0 ? null : Loc.T("Chiều cao cột phải dương (> 0).", "Column height must be positive (> 0).")
            },
            new PropertyDescriptor("Width", target => ((CadDemoColumn)target).Width, (target, value) => ((CadDemoColumn)target).Width = Convert.ToDouble(value))
            {
                DisplayName = Loc.T("Bề rộng cánh (B)", "Flange width (B)"),
                Category = Loc.T("Kích thước hình học", "Geometry"),
                Editor = PropertyEditorKind.Numeric,
                Unit = "mm",
                DefaultValue = 400.0
            },
            new PropertyDescriptor("Depth", target => ((CadDemoColumn)target).Depth, (target, value) => ((CadDemoColumn)target).Depth = Convert.ToDouble(value))
            {
                DisplayName = Loc.T("Chiều cao bụng (Hw)", "Web depth (Hw)"),
                Category = Loc.T("Kích thước hình học", "Geometry"),
                Editor = PropertyEditorKind.Numeric,
                Unit = "mm",
                DefaultValue = 400.0
            },
            new PropertyDescriptor("Material", target => ((CadDemoColumn)target).Material, (target, value) => ((CadDemoColumn)target).Material = value as string ?? string.Empty)
            {
                DisplayName = Loc.T("Mác thép kết cấu", "Structural steel grade"),
                Category = Loc.T("Vật liệu & Tiêu chuẩn", "Materials & Specs"),
                Editor = PropertyEditorKind.Choice,
                Choices = new object[] { "SS400", "Q345B", "A572-Gr50", "SM490" },
                DefaultValue = "SS400"
            },
            new PropertyDescriptor("Story", target => ((CadDemoColumn)target).Story)
            {
                DisplayName = Loc.T("Tầng định vị", "Story level"),
                Category = Loc.T("Vị trí & Ràng buộc", "Identity & Constraints")
            },
            new PropertyDescriptor("IsLocked", target => ((CadDemoColumn)target).IsLocked, (target, value) => ((CadDemoColumn)target).IsLocked = Convert.ToBoolean(value))
            {
                DisplayName = Loc.T("Khoá vị trí cấu kiện", "Lock member position"),
                Category = Loc.T("Vị trí & Ràng buộc", "Identity & Constraints"),
                Editor = PropertyEditorKind.Boolean
            });

        // 3. Đăng ký thuộc tính Tường vây (Diaphragm Wall)
        properties.Register<CadDemoWall>(
            new PropertyDescriptor("Length", target => ((CadDemoWall)target).Length, (target, value) => ((CadDemoWall)target).Length = Convert.ToDouble(value))
            {
                DisplayName = Loc.T("Chiều dài tường (L)", "Wall length (L)"),
                Category = Loc.T("Kích thước hình học", "Geometry"),
                Editor = PropertyEditorKind.Numeric,
                Unit = "mm",
                DefaultValue = 5000.0,
                Validator = (_, value) => value is double l && l > 0 ? null : Loc.T("Chiều dài tường phải dương (> 0).", "Wall length must be positive (> 0).")
            },
            new PropertyDescriptor("Height", target => ((CadDemoWall)target).Height, (target, value) => ((CadDemoWall)target).Height = Convert.ToDouble(value))
            {
                DisplayName = Loc.T("Chiều cao tường (H)", "Wall height (H)"),
                Category = Loc.T("Kích thước hình học", "Geometry"),
                Editor = PropertyEditorKind.Numeric,
                Unit = "mm",
                DefaultValue = 3200.0
            },
            new PropertyDescriptor("Thickness", target => ((CadDemoWall)target).Thickness, (target, value) => ((CadDemoWall)target).Thickness = Convert.ToDouble(value))
            {
                DisplayName = Loc.T("Chiều dày tường (t)", "Wall thickness (t)"),
                Category = Loc.T("Kích thước hình học", "Geometry"),
                Editor = PropertyEditorKind.Numeric,
                Unit = "mm",
                DefaultValue = 300.0
            },
            new PropertyDescriptor("Material", target => ((CadDemoWall)target).Material, (target, value) => ((CadDemoWall)target).Material = value as string ?? string.Empty)
            {
                DisplayName = Loc.T("Cấp độ bền bê tông", "Concrete grade"),
                Category = Loc.T("Vật liệu & Tiêu chuẩn", "Materials & Specs"),
                Editor = PropertyEditorKind.Choice,
                Choices = new object[] { Loc.T("Bê tông B30", "Concrete B30"), Loc.T("Bê tông B35", "Concrete B35"), Loc.T("Bê tông B40", "Concrete B40") },
                DefaultValue = Loc.T("Bê tông B30", "Concrete B30")
            },
            new PropertyDescriptor("Story", target => ((CadDemoWall)target).Story)
            {
                DisplayName = Loc.T("Tầng định vị", "Story level"),
                Category = Loc.T("Vị trí & Ràng buộc", "Identity & Constraints")
            },
            new PropertyDescriptor("IsLocked", target => ((CadDemoWall)target).IsLocked, (target, value) => ((CadDemoWall)target).IsLocked = Convert.ToBoolean(value))
            {
                DisplayName = Loc.T("Khoá vị trí cấu kiện", "Lock member position"),
                Category = Loc.T("Vị trí & Ràng buộc", "Identity & Constraints"),
                Editor = PropertyEditorKind.Boolean
            });

        selection.Select(beam);

        // =========================================================================
        // PRIMARY SHOWCASE CONTROL: PropertiesPalette
        // =========================================================================
        var palette = new PropertiesPalette(properties, selection)
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ShowSearch = true,
            RevitLayout = false,
            EmptyTitle = Loc.T("Chưa chọn cấu kiện", "Nothing selected"),
            EmptySubtitle = Loc.T("Chọn một đối tượng trên mô hình để chỉnh sửa tham số.", "Select an element to edit its parameters."),
            EmptyHint = Loc.T("Chọn đối tượng từ danh sách bên dưới hoặc nhấp vào mô hình để nạp thuộc tính.", "Pick an element from the target selector below to inspect parameters.")
        };

        var paletteDock = new Border
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Height = 460,
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            ClipToBounds = true
        };
        paletteDock.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
        paletteDock.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
        paletteDock.Child = palette;

        // Strip status / transaction log directly below palette
        var activityStrip = new Border
        {
            Margin = new Thickness(0, 10, 0, 0),
            Padding = new Thickness(12, 8, 12, 8),
            CornerRadius = new CornerRadius(6),
            BorderThickness = new Thickness(1)
        };
        activityStrip.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
        activityStrip.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.BorderSubtle");

        var activityText = new TextBlock
        {
            Text = Loc.T("✓ Sẵn sàng: Đã nạp cấu kiện dầm và 7 tham số kỹ thuật qua PropertyManager.", "✓ Ready: RC beam element and 7 engineering parameters loaded via PropertyManager."),
            FontSize = 11.5,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        activityText.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        activityStrip.Child = activityText;

        // Palette event handlers
        palette.PropertyCommitted += (_, e) =>
        {
            activityText.Text = Loc.F("✓ Đã cập nhật thuộc tính [{0}] qua TransactionManager",
                "✓ Committed property [{0}] via TransactionManager", e.Property);
        };

        palette.TypeChangeRequested += (_, newType) =>
        {
            if (activeTarget is CadDemoBeam b)
            {
                b.TypeName = newType;
                if (newType.Contains("300×500")) { b.Width = 300; b.Height = 500; }
                else if (newType.Contains("350×600")) { b.Width = 350; b.Height = 600; }
                else if (newType.Contains("400×700")) { b.Width = 400; b.Height = 700; }
                else { b.Width = 250; b.Height = 400; }
                palette.Build(b);
            }
            else if (activeTarget is CadDemoColumn c)
            {
                c.TypeName = newType;
                if (newType.Contains("300×300")) { c.Width = 300; c.Depth = 300; }
                else if (newType.Contains("350×350")) { c.Width = 350; c.Depth = 350; }
                else if (newType.Contains("450×450")) { c.Width = 450; c.Depth = 450; }
                else { c.Width = 400; c.Depth = 400; }
                palette.Build(c);
            }
            activityText.Text = Loc.F("✓ Đã đổi Type đối tượng sang [{0}]", "✓ Switched instance type to [{0}]", newType);
        };

        palette.EditTypeRequested += (_, _) =>
        {
            ToastService.Info(Loc.T("Mở hộp thoại chỉnh sửa Type Properties...", "Opening Type Properties dialog..."), "PropertiesPalette");
            activityText.Text = Loc.T("✓ Nhấn 'Sửa kiểu' (Edit Type): Đã phát sự kiện EditTypeRequested", "✓ Edit Type pressed: raised EditTypeRequested event");
        };

        // =========================================================================
        // BOTTOM WORKBENCH: Extended Properties, Styles & Interactive Controls
        // =========================================================================
        var configGrid = new Grid { Margin = new Thickness(0, 16, 0, 0) };
        configGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        configGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(32) });
        configGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Column 0: Bố cục & Đối tượng
        var col0 = new StackPanel();
        col0.Children.Add(PanelCaption(Loc.T("KIỂU DÁNG VÀ BỐ CỤC (PALETTE STYLES)", "LAYOUT & PALETTE STYLES")));

        var layoutStyleCombo = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Height = 28,
            Margin = new Thickness(0, 0, 0, 10),
            ItemsSource = new[]
            {
                Loc.T("Hiện đại - Gọn gàng (CONCOST Compact Layout)", "CONCOST Modern Compact Layout"),
                Loc.T("Autodesk Revit Phẳng (Revit Properties Style)", "Autodesk Revit Properties Style")
            },
            SelectedIndex = 0
        };

        col0.Children.Add(layoutStyleCombo);

        col0.Children.Add(PanelCaption(Loc.T("ĐỐI TƯỢNG CAD / BIM (ACTIVE TARGET)", "ACTIVE CAD / BIM TARGET")));

        var targetObjectCombo = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Height = 28,
            Margin = new Thickness(0, 0, 0, 10),
            ItemsSource = new[]
            {
                Loc.T("Dầm bê tông cốt thép (RC Beam D1 - 6000×400×250)", "RC Beam D1 (6000×400×250)"),
                Loc.T("Cột thép hình chữ H (Steel Column H400×400)", "Steel Column H400×400"),
                Loc.T("Tường vây hầm BTCT (Diaphragm Wall W1 - 5000×3200×300)", "Diaphragm Wall W1 (5000×3200×300)"),
                Loc.T("(Chưa chọn cấu kiện — Trạng thái rỗng / Empty State)", "(Nothing Selected — Empty State)")
            },
            SelectedIndex = 0
        };

        targetObjectCombo.SelectionChanged += (_, _) =>
        {
            switch (targetObjectCombo.SelectedIndex)
            {
                case 0:
                    activeTarget = beam;
                    selection.Select(beam);
                    palette.Build(beam);
                    break;
                case 1:
                    activeTarget = column;
                    selection.Select(column);
                    palette.Build(column);
                    break;
                case 2:
                    activeTarget = wall;
                    selection.Select(wall);
                    palette.Build(wall);
                    break;
                default:
                    activeTarget = null;
                    selection.Clear();
                    palette.Build(null);
                    break;
            }
            activityText.Text = Loc.F("✓ Đã chọn đối tượng: {0}", "✓ Selected object: {0}", targetObjectCombo.SelectedItem);
        };

        col0.Children.Add(targetObjectCombo);

        col0.Children.Add(AppearanceSlider(Loc.T("Chiều cao bảng", "Palette height"), 380, 600, 460, 20, "{0:0} px", v =>
        {
            paletteDock.Height = v;
        }));

        col0.Children.Add(AppearanceSlider(Loc.T("Bo góc bảng", "Corner radius"), 0, 16, 8, 2, "{0:0} px", v =>
        {
            paletteDock.CornerRadius = new CornerRadius(v);
        }));

        Grid.SetColumn(col0, 0);
        configGrid.Children.Add(col0);

        // Column 2: Công tắc tính năng & Thao tác nhanh
        var col2 = new StackPanel();
        col2.Children.Add(PanelCaption(Loc.T("CÔNG TẮC THUỘC TÍNH MỞ RỘNG (EXTENDED PROPERTIES)", "EXTENDED PROPERTIES & TOGGLES")));

        var switchesGrid = new System.Windows.Controls.Primitives.UniformGrid
        {
            Columns = 2,
            Margin = new Thickness(0, 0, 0, 8)
        };

        switchesGrid.Children.Add(AppearanceToggle(Loc.T("Ô tìm kiếm (ShowSearch)", "Search box (ShowSearch)"), true, v => palette.ShowSearch = v));

        var revitToggle = AppearanceToggle(Loc.T("Bố cục Revit (RevitLayout)", "Revit layout (RevitLayout)"), false, v =>
        {
            palette.RevitLayout = v;
            layoutStyleCombo.SelectedIndex = v ? 1 : 0;
            activityText.Text = Loc.F("✓ Chế độ RevitLayout: {0}", "✓ RevitLayout mode: {0}", v ? "BẬT (ON)" : "TẮT (OFF)");
        });
        switchesGrid.Children.Add(revitToggle);

        layoutStyleCombo.SelectionChanged += (_, _) =>
        {
            bool isRevit = layoutStyleCombo.SelectedIndex == 1;
            palette.RevitLayout = isRevit;
            if (revitToggle is CheckBox cb && cb.IsChecked != isRevit) cb.IsChecked = isRevit;
            activityText.Text = Loc.F("✓ Bố cục palette: {0}", "✓ Palette layout: {0}", isRevit ? "Autodesk Revit" : "CONCOST Compact");
        };

        switchesGrid.Children.Add(AppearanceToggle(Loc.T("Khóa vị trí cấu kiện", "Lock member position"), false, v =>
        {
            if (activeTarget is CadDemoBeam bm) { bm.IsLocked = v; properties.TrySetValue(bm, "IsLocked", v, out _); }
            else if (activeTarget is CadDemoColumn cl) { cl.IsLocked = v; properties.TrySetValue(cl, "IsLocked", v, out _); }
            else if (activeTarget is CadDemoWall wl) { wl.IsLocked = v; properties.TrySetValue(wl, "IsLocked", v, out _); }
            activityText.Text = Loc.F("✓ Trạng thái khóa vị trí: {0}", "✓ Lock position status: {0}", v ? Loc.T("Đã khóa", "Locked") : Loc.T("Mở khóa", "Unlocked"));
        }));

        switchesGrid.Children.Add(AppearanceToggle(Loc.T("Chế độ chỉ đọc (ReadOnly)", "Read-only mode"), false, v => palette.IsEnabled = !v));

        col2.Children.Add(switchesGrid);

        col2.Children.Add(PanelCaption(Loc.T("THAO TÁC TƯƠNG TÁC NHANH (QUICK ACTIONS)", "QUICK ACTIONS")));

        var actionsRow1 = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 6) };
        var undoBtn = SecondaryButton(Loc.T("Hoàn tác (Undo)", "Undo"));
        IconElement.SetKind(undoBtn, IconKind.Undo);
        undoBtn.Margin = new Thickness(0, 0, 8, 0);
        undoBtn.Click += (_, _) =>
        {
            if (undo.CanUndo)
            {
                undo.Undo();
                if (activeTarget != null) palette.Build(activeTarget);
                activityText.Text = Loc.T("✓ Đã hoàn tác (Undo) bước chỉnh sửa gần nhất.", "✓ Undid the last property change.");
            }
            else
            {
                activityText.Text = Loc.T("Không có thao tác nào để hoàn tác.", "No actions in undo stack.");
            }
        };

        var resetBtn = SecondaryButton(Loc.T("Mặc định (Reset All)", "Reset All"));
        IconElement.SetKind(resetBtn, IconKind.Refresh);
        resetBtn.Margin = new Thickness(0, 0, 8, 0);
        resetBtn.Click += (_, _) =>
        {
            if (activeTarget is CadDemoBeam bm)
            {
                bm.Length = 6000; bm.Height = 400; bm.Width = 250;
                bm.Material = Loc.T("Bê tông B25", "Concrete B25");
                bm.RebarGrade = "CB400-V"; bm.IsLocked = false;
                palette.Build(bm);
            }
            else if (activeTarget is CadDemoColumn cl)
            {
                cl.Height = 3600; cl.Width = 400; cl.Depth = 400;
                cl.Material = "SS400"; cl.IsLocked = false;
                palette.Build(cl);
            }
            else if (activeTarget is CadDemoWall wl)
            {
                wl.Length = 5000; wl.Height = 3200; wl.Thickness = 300;
                wl.Material = Loc.T("Bê tông B30", "Concrete B30");
                wl.IsLocked = false;
                palette.Build(wl);
            }
            activityText.Text = Loc.T("✓ Đã khôi phục toàn bộ thuộc tính về giá trị mặc định.", "✓ Reset all properties to default values.");
        };

        actionsRow1.Children.Add(undoBtn);
        actionsRow1.Children.Add(resetBtn);
        col2.Children.Add(actionsRow1);

        var actionsRow2 = new StackPanel { Orientation = Orientation.Horizontal };
        var errorBtn = SecondaryButton(Loc.T("Giả lập lỗi (-500)", "Test Error (-500)"));
        IconElement.SetKind(errorBtn, IconKind.Warning);
        errorBtn.Margin = new Thickness(0, 0, 8, 0);
        errorBtn.Click += (_, _) =>
        {
            if (activeTarget != null)
            {
                properties.TrySetValueFromText(activeTarget, "Length", "-500", out string? why);
                activityText.Text = Loc.F("⚠️ Cảnh báo xác thực: {0}", "⚠️ Validation warning: {0}", why ?? Loc.T("Chiều dài không hợp lệ", "Invalid length"));
            }
        };

        var emptyBtn = SecondaryButton(Loc.T("Bỏ chọn (Empty)", "Deselect (Empty)"));
        IconElement.SetKind(emptyBtn, IconKind.Close);
        emptyBtn.Click += (_, _) => targetObjectCombo.SelectedIndex = 3;

        actionsRow2.Children.Add(errorBtn);
        actionsRow2.Children.Add(emptyBtn);
        col2.Children.Add(actionsRow2);

        Grid.SetColumn(col2, 2);
        configGrid.Children.Add(col2);

        // Combined layout
        var wholeLayout = new StackPanel();
        wholeLayout.Children.Add(paletteDock);
        wholeLayout.Children.Add(activityStrip);
        wholeLayout.Children.Add(configGrid);

        var callout = CreateCalloutCard(Loc.T(
            "💡 HƯỚNG DẪN: PropertiesPalette là bảng điều khiển thuộc tính chính cho CAD/BIM. Bạn có thể đổi kiểu 'CONCOST Compact' ↔ 'Autodesk Revit Style', chọn Dầm/Cột/Tường/Trạng thái rỗng, bật/tắt ô tìm kiếm hoặc giả lập kiểm tra lỗi trực quan.",
            "💡 GUIDE: PropertiesPalette is the primary CAD/BIM property inspector. Toggle between 'CONCOST Compact' ↔ 'Autodesk Revit Style', select Beam/Column/Wall/Empty state, toggle search box, or simulate inline validation errors."));
        callout.Margin = new Thickness(0, 16, 0, 0);
        wholeLayout.Children.Add(callout);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Bảng thuộc tính đối tượng kỹ thuật CAD & BIM", "CAD & BIM Technical Properties Palette (PropertiesPalette)"),
            wholeLayout,
            xaml: @"<!-- 1. Bảng thuộc tính PropertiesPalette hiển thị theo chuẩn CONCOST Compact hoặc Revit -->
<ui:PropertiesPalette x:Name=""ObjectPalette""
                       Height=""480""
                       ShowSearch=""True""
                       RevitLayout=""False""
                       EmptyTitle=""Chưa chọn cấu kiện""
                       EmptySubtitle=""Chọn một đối tượng trên mô hình để chỉnh sửa tham số.""
                       PropertyCommitted=""OnPropertyCommitted""
                       TypeChangeRequested=""OnTypeChangeRequested""
                       EditTypeRequested=""OnEditTypeRequested"" />",
            csharp: Loc.Code(@"// Đăng ký mô hình ITypeSelectable và PropertyDescriptor với PropertyManager
var undo = new UndoRedoManager();
var properties = new PropertyManager(new TransactionManager(undo));
var selection = new SelectionManager();

properties.Register<CadDemoBeam>(
    new PropertyDescriptor(""Length"", b => ((CadDemoBeam)b).Length, (b, v) => ((CadDemoBeam)b).Length = Convert.ToDouble(v))
    {
        DisplayName = ""Chiều dài (L)"",
        Category = ""Kích thước hình học"",
        Editor = PropertyEditorKind.Numeric,
        Unit = ""mm"",
        DefaultValue = 6000.0,
        Validator = (_, v) => v is double l && l > 0 ? null : ""Chiều dài phải dương (> 0).""
    },
    new PropertyDescriptor(""Material"", b => ((CadDemoBeam)b).Material, (b, v) => ((CadDemoBeam)b).Material = v as string)
    {
        DisplayName = ""Cấp độ bền bê tông"",
        Category = ""Vật liệu & Tiêu chuẩn"",
        Editor = PropertyEditorKind.Choice,
        Choices = new object[] { ""Bê tông B25"", ""Bê tông B30"", ""Bê tông B35"" }
    });

// Nạp đối tượng đang chọn vào Palette
selection.Select(beam);
ObjectPalette.Build(beam);")));

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
            ApplicationName = "CONCOST BIM 2026",
            DocumentTitle = Loc.T("Tháp Landmark — Mặt Bằng Kết Cấu Tầng 18", "Landmark Tower — Level 18 Structural Plan"),
            Subtitle = "RVT v2026.2",
            IsDocumentModified = false,
            ShowQuickAccess = true,
            ShowWindowButtons = true,
            ShowAppIcon = true,
            ShowSearchBox = true,
            ShowUserAccount = true,
            UserAccountName = Loc.T("KTS. Lê Hoàng Minh", "Arch. Le Hoang Minh"),
            TitleAlignment = HorizontalAlignment.Center,
            TitleForeground = Brushes.White,
            AppIconBrush = new SolidColorBrush(Color.FromRgb(0xFB, 0x92, 0x3C)),
            Height = 38
        };

        // Dải nút thao tác nhanh mặc định
        titleBar.QuickAccess.Add(new TitleBarAction(IconKind.Undo, Loc.T("Hoàn tác (Ctrl+Z)", "Undo (Ctrl+Z)"), null));
        titleBar.QuickAccess.Add(new TitleBarAction(IconKind.Redo, Loc.T("Làm lại (Ctrl+Y)", "Redo (Ctrl+Y)"), null));
        titleBar.QuickAccess.Add(new TitleBarAction(IconKind.Save, Loc.T("Lưu dự án (Ctrl+S)", "Save project (Ctrl+S)"), null));
        titleBar.QuickAccess.Add(new TitleBarAction(IconKind.Folder, Loc.T("Mở bản vẽ (Ctrl+O)", "Open drawing (Ctrl+O)"), null));
        titleBar.QuickAccess.Add(new TitleBarAction(IconKind.Print, Loc.T("In ấn xuất PDF (Ctrl+P)", "Print to PDF (Ctrl+P)"), null));
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
        string[] tabs = { Loc.T("Tệp", "File"), Loc.T("Kiến Trúc", "Architecture"), Loc.T("Kết Cấu", "Structure"), Loc.T("Hệ Thống MEP", "MEP Systems"), Loc.T("Khối Lượng & Chi Phí", "Quantities & Cost"), Loc.T("Phân Tích Tải Trọng", "Load Analysis"), Loc.T("Cài Đặt", "Settings") };
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
        hudBadge1.Child = new TextBlock { Text = Loc.T("[Mặt Bằng Tầng 18]", "[Level 18 Plan]"), FontSize = 10, Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)) };
        var hudBadge2 = new Border { Background = new SolidColorBrush(Color.FromArgb(0x30, 0x00, 0x00, 0x00)), CornerRadius = new CornerRadius(4), Padding = new Thickness(6, 2, 6, 2) };
        hudBadge2.Child = new TextBlock { Text = Loc.T("[Tỉ Lệ 1:100 | Khung dây BIM]", "[Scale 1:100 | BIM Wireframe]"), FontSize = 10, Foreground = new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8)) };
        hudLeft.Children.Add(hudBadge1);
        hudLeft.Children.Add(hudBadge2);
        viewportGrid.Children.Add(hudLeft);

        // 2.3 Status Badge (Góc dưới phải)
        var docStatus = new TextBlock
        {
            Text = Loc.T("Trạng thái: Đã đồng bộ với máy chủ trung tâm", "Status: synced with the Central Server"),
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
        statusStack.Children.Add(new TextBlock { Text = Loc.T("Sẵn sàng | Tỉ lệ: 1:100 | Hệ toạ độ: VN-2000 | Tầng 18 (Cao độ +64.800m)", "Ready | Scale: 1:100 | Coordinate system: VN-2000 | Level 18 (elevation +64.800 m)"), FontSize = 10, Foreground = new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B)) });
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
            ToastService.Info(isMaximizedState ? Loc.T("Cửa sổ: Phóng to tối đa", "Window: maximised") : Loc.T("Cửa sổ: Phục hồi kích thước chuẩn", "Window: restored to normal size"), "ApplicationTitleBar");
        };

        titleBar.MinimizeRequested += (_, _) =>
        {
            ToastService.Info(Loc.T("Cửa sổ: Thu nhỏ xuống thanh tác vụ", "Window: minimised to the taskbar"), "ApplicationTitleBar");
        };

        titleBar.CloseRequested += (_, _) =>
        {
            if (titleBar.IsDocumentModified)
                ToastService.Warning(Loc.T("Cảnh báo: Bản vẽ có thay đổi chưa lưu! Vui lòng lưu trước khi đóng.", "Warning: the drawing has unsaved changes! Save before closing."), "ApplicationTitleBar");
            else
                ToastService.Info(Loc.T("Đóng cửa sổ làm việc.", "Close the workspace window."), "ApplicationTitleBar");
        };

        // ---------------------------------------------------------------------
        // 4. Bảng điều khiển tương tác phong phú (Interactive Control Panel)
        // ---------------------------------------------------------------------
        var readout = CadHint(Loc.F("Tiêu đề hiện tại: {0}", "Current title: {0}",
            ApplicationTitleBar.ComposeTitle(titleBar.DocumentTitle, titleBar.IsDocumentModified, titleBar.ApplicationName)));

        // Row TextBoxes
        var textRow = new Grid { Margin = new Thickness(0, 16, 0, 10) };
        textRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        textRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        textRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Document Title TextBox
        var docBoxStack = new StackPanel { Margin = new Thickness(0, 0, 8, 0) };
        docBoxStack.Children.Add(CadHint(Loc.T("Tên tài liệu / Bản vẽ:", "Document / drawing name (DocumentTitle):")));
        var docTextBox = new TextBox { Text = titleBar.DocumentTitle, Height = 28, Margin = new Thickness(0, 4, 0, 0) };
        docTextBox.TextChanged += (_, _) =>
        {
            titleBar.DocumentTitle = docTextBox.Text;
            docStatus.Text = Loc.F("{0} — Trạng thái: {1}", "{0} — Status: {1}", docTextBox.Text,
        titleBar.IsDocumentModified ? Loc.T("Có thay đổi chưa lưu *", "Unsaved changes *") : Loc.T("Đã đồng bộ", "Synced"));
            readout.Text = Loc.F("Tiêu đề hiện tại: {0}", "Current title: {0}", ApplicationTitleBar.ComposeTitle(titleBar.DocumentTitle, titleBar.IsDocumentModified, titleBar.ApplicationName));
        };
        docBoxStack.Children.Add(docTextBox);
        Grid.SetColumn(docBoxStack, 0);
        textRow.Children.Add(docBoxStack);

        // Application Name TextBox
        var appBoxStack = new StackPanel { Margin = new Thickness(4, 0, 4, 0) };
        appBoxStack.Children.Add(CadHint(Loc.T("Tên phần mềm:", "Application name (ApplicationName):")));
        var appTextBox = new TextBox { Text = titleBar.ApplicationName, Height = 28, Margin = new Thickness(0, 4, 0, 0) };
        appTextBox.TextChanged += (_, _) =>
        {
            titleBar.ApplicationName = appTextBox.Text;
            readout.Text = Loc.F("Tiêu đề hiện tại: {0}", "Current title: {0}", ApplicationTitleBar.ComposeTitle(titleBar.DocumentTitle, titleBar.IsDocumentModified, titleBar.ApplicationName));
        };
        appBoxStack.Children.Add(appTextBox);
        Grid.SetColumn(appBoxStack, 1);
        textRow.Children.Add(appBoxStack);

        // User Account TextBox
        var userBoxStack = new StackPanel { Margin = new Thickness(8, 0, 0, 0) };
        userBoxStack.Children.Add(CadHint(Loc.T("Tài khoản người dùng:", "User account (UserAccountName):")));
        var userTextBox = new TextBox { Text = titleBar.UserAccountName, Height = 28, Margin = new Thickness(0, 4, 0, 0) };
        userTextBox.TextChanged += (_, _) => titleBar.UserAccountName = userTextBox.Text;
        userBoxStack.Children.Add(userTextBox);
        Grid.SetColumn(userBoxStack, 2);
        textRow.Children.Add(userBoxStack);

        // Toggles
        var switches = new WrapPanel { Margin = new Thickness(0, 4, 0, 10) };
        switches.Children.Add(AppearanceToggle(Loc.T("Chưa lưu bản vẽ (*)", "Unsaved drawing (Modified *)"), titleBar.IsDocumentModified, v =>
        {
            titleBar.IsDocumentModified = v;
            docStatus.Text = Loc.F("{0} — Trạng thái: {1}", "{0} — Status: {1}", titleBar.DocumentTitle,
        v ? Loc.T("⚠️ Có thay đổi chưa lưu (*)", "⚠️ Unsaved changes (*)") : Loc.T("Đã đồng bộ", "Synced"));
            readout.Text = Loc.F("Tiêu đề hiện tại: {0}", "Current title: {0}", ApplicationTitleBar.ComposeTitle(titleBar.DocumentTitle, v, titleBar.ApplicationName));
        }));
        switches.Children.Add(AppearanceToggle(Loc.T("Hiện nút cửa sổ", "Window buttons (WindowButtons)"), titleBar.ShowWindowButtons, v => titleBar.ShowWindowButtons = v));
        switches.Children.Add(AppearanceToggle(Loc.T("Hiện thanh truy cập nhanh", "Quick Access Strip"), titleBar.ShowQuickAccess, v => titleBar.ShowQuickAccess = v));
        switches.Children.Add(AppearanceToggle(Loc.T("Tìm kiếm lệnh CAD", "CAD command search (SearchBox)"), titleBar.ShowSearchBox, v => titleBar.ShowSearchBox = v));
        switches.Children.Add(AppearanceToggle(Loc.T("Hiện tài khoản kỹ sư", "Engineer account (UserAccount)"), titleBar.ShowUserAccount, v => titleBar.ShowUserAccount = v));
        switches.Children.Add(AppearanceToggle(Loc.T("Hiện biểu tượng ứng dụng", "App logo (AppIcon)"), titleBar.ShowAppIcon, v => titleBar.ShowAppIcon = v));

        // Combo Row
        var comboRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 10) };

        var alignLabel = CadHint(Loc.T("Căn lề tiêu đề:", "Title alignment:"));
        alignLabel.VerticalAlignment = VerticalAlignment.Center;
        alignLabel.Margin = new Thickness(0, 0, 8, 0);
        var alignCombo = new ComboBox
        {
            Width = 170,
            ItemsSource = new[] { Loc.T("Chính giữa", "Centre"), Loc.T("Căn trái", "Left") },
            SelectedIndex = 0,
            Margin = new Thickness(0, 0, 18, 0)
        };
        alignCombo.SelectionChanged += (_, _) =>
            titleBar.TitleAlignment = alignCombo.SelectedIndex == 1 ? HorizontalAlignment.Left : HorizontalAlignment.Center;

        var themeLabel = CadHint(Loc.T("Màu thanh tiêu đề:", "Title bar colour:"));
        themeLabel.VerticalAlignment = VerticalAlignment.Center;
        themeLabel.Margin = new Thickness(0, 0, 8, 0);
        var themeCombo = new ComboBox
        {
            Width = 190,
            ItemsSource = new[] { Loc.T("Mặc định (Trong suốt)", "Default (transparent)"), Loc.T("Đen cacbon", "Dark Carbon"), Loc.T("Xanh hải quân CAD", "Navy CAD"), Loc.T("Kính titan", "Titanium Glass") },
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
            Loc.T("Thanh tiêu đề ứng dụng", "Application title bar (ApplicationTitleBar)"),
            content,
            xaml: Loc.Code(@"<ui:ApplicationTitleBar ApplicationName=""CONCOST BIM Suite 2026""
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
                       MaximizeRequested=""OnMaximizeRequested"" />"),
            csharp: Loc.Code(@"// Quản lý tiêu đề tự động kèm dấu * khi có thay đổi chưa lưu:
titleBar.IsDocumentModified = true;
// Kết quả hiển thị: ""Landmark Tower — Mặt Bằng Kết Cấu* — CONCOST BIM Suite 2026""

// Bắt các sự kiện điều khiển cửa sổ:
titleBar.CloseRequested += (_, _) => CheckUnsavedChanges();
titleBar.MaximizeRequested += (_, _) => ToggleMaximize();")));

        return root;
    }

    private object CreateApplicationInfoBarDemo()
    {
        var infoBar = new ApplicationInfoBar { IsClosable = true };

        // Default: Clash detection warning
        infoBar.ActionText = Loc.T("Định vị vị trí va chạm", "Locate clash");
        infoBar.Show(Loc.T("Phát hiện va chạm mô hình", "Model clash detection"),
            Loc.T("Dầm kết cấu D1-T18 giao cắt ống cấp nước MEP đường kính D150 tại trục 3-C.", "Structural beam D1-L18 clashes with the D150 MEP water supply pipe at grid 3-C."),
            CadNotificationSeverity.Warning);

        var readout = CadHint(Loc.T(
            "Hệ thống thông báo kỹ thuật công trình hỗ trợ 4 mức độ: Thông tin, Thành công, Cảnh báo, Lỗi.",
            "The BIM alert bar supports four severities: Info, Success, Warning, Error."));

        infoBar.ActionInvoked += (_, _) =>
        {
            readout.Text = Loc.T("Hành động: Đã điều hướng góc nhìn đến toạ độ va chạm tại trục 3-C.", "Action: the viewport camera moved to the clash at grid 3-C.");
            ToastService.Info(Loc.T("Đang tiêu điểm vị trí va chạm: Dầm D1 x Ống MEP D150", "Clash focus: beam D1 × MEP pipe D150"), "BIM Clash");
        };

        infoBar.Closed += (_, _) =>
        {
            readout.Text = Loc.T("Đã đóng thanh thông báo.", "Notification bar dismissed.");
        };

        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };

        var btnWarning = SecondaryButton(Loc.T("⚠️ Va chạm MEP", "⚠️ MEP clash (Warning)"));
        btnWarning.Click += (_, _) =>
        {
            infoBar.ActionText = Loc.T("Định vị va chạm", "Locate clash");
            infoBar.Show(Loc.T("Phát hiện va chạm mô hình", "Model clash detected"), Loc.T("Dầm D1-T18 giao cắt ống MEP D150 tại trục 3-C.", "Beam D1-L18 clashes with MEP pipe D150 at grid 3-C."), CadNotificationSeverity.Warning);
        };
        buttons.Children.Add(btnWarning);

        var btnError = SecondaryButton(Loc.T("❌ Vượt tải trọng", "❌ Overload (Error)"));
        btnError.Click += (_, _) =>
        {
            infoBar.ActionText = Loc.T("Tự động tăng tiết diện", "Auto-enlarge section");
            infoBar.Show(Loc.T("Vi phạm tiêu chuẩn TCVN 5574:2018", "TCVN 5574:2018 code violation"), Loc.T("Hàm lượng cốt thép dầm D2 vượt quá 2.5% (μ = 2.85%).", "Rebar ratio in beam D2 exceeds 2.5% (μ = 2.85%)."), CadNotificationSeverity.Error);
        };
        buttons.Children.Add(btnError);

        var btnSuccess = SecondaryButton(Loc.T("✅ Đồng bộ máy chủ", "✅ Central sync (Success)"));
        btnSuccess.Click += (_, _) =>
        {
            infoBar.ActionText = Loc.T("Xem lịch sử", "View history");
            infoBar.Show(Loc.T("Đồng bộ thành công", "Sync complete"), Loc.T("Mô hình Tháp Landmark đã được đồng bộ an toàn với máy chủ trung tâm.", "The Landmark Tower model was safely synced with Central BIM 360."), CadNotificationSeverity.Success);
        };
        buttons.Children.Add(btnSuccess);

        var btnInfo = SecondaryButton(Loc.T("ℹ️ Cập nhật TCVN", "ℹ️ TCVN update (Info)"));
        btnInfo.Click += (_, _) =>
        {
            infoBar.ActionText = Loc.T("Xem chi tiết", "View details");
            infoBar.Show(Loc.T("Bản cập nhật tiêu chuẩn", "Standards update"), Loc.T("Đã cập nhật bộ dữ liệu định mức xây dựng và đơn giá Quý IV/2026.", "Construction norms and Q4/2026 unit rates have been updated."), CadNotificationSeverity.Info);
        };
        buttons.Children.Add(btnInfo);

        var content = new StackPanel();
        content.Children.Add(infoBar);
        content.Children.Add(buttons);
        content.Children.Add(readout);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Thanh thông báo kỹ thuật công trình", "Project information bar (ApplicationInfoBar)"),
            content,
            xaml: Loc.Code(@"<ui:ApplicationInfoBar IsClosable=""True""
                       ActionText=""Định vị vị trí va chạm""
                       ActionInvoked=""OnClashLocate"" />"),
            csharp: Loc.Code(@"// Bốn cấp độ thông báo BIM: Info, Success, Warning, Error
infoBar.ActionText = ""Định vị va chạm"";
infoBar.Show(""Cảnh báo va chạm"", ""Dầm D1 giao cắt ống MEP"", CadNotificationSeverity.Warning);")));

        return root;
    }

    private object CreateBackstageViewDemo()
    {
        var backstage = new RibbonBackstageView { Height = 360, ApplicationName = "CONCOST BIM" };

        // 1. Tab Thông Tin Dự Án (Project Info Dashboard)
        var infoPanel = new StackPanel { Margin = new Thickness(24) };
        infoPanel.Children.Add(new TextBlock { Text = Loc.T("THÔNG TIN DỰ ÁN & HỒ SƠ THIẾT KẾ", "PROJECT INFORMATION & DESIGN DOSSIER"), FontSize = 16, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)), Margin = new Thickness(0, 0, 0, 14) });

        var metaGrid = new Grid();
        metaGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
        metaGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        string[][] metaData = {
            new[] { Loc.T("Tên công trình:", "Project name:"), Loc.T("THÁP LANDMARK 2026 — KHU PHỨC HỢP THƯƠNG MẠI", "LANDMARK TOWER 2026 — MIXED-USE COMMERCIAL COMPLEX") },
            new[] { Loc.T("Chủ đầu tư:", "Client:"), Loc.T("TẬP ĐOÀN ĐẦU TƯ ĐỊA ỐC QUỐC TẾ", "INTERNATIONAL REAL ESTATE INVESTMENT GROUP") },
            new[] { Loc.T("Địa điểm:", "Location:"), Loc.T("Khu đô thị mới Thủ Thiêm, TP. Hồ Chí Minh", "Thu Thiem New Urban Area, Ho Chi Minh City") },
            new[] { Loc.T("Quy mô:", "Scope:"), Loc.T("45 tầng nổi + 3 tầng hầm (Tổng diện tích sàn 125,000 m²)", "45 storeys + 3 basements (125,000 m² gross floor area)") },
            new[] { Loc.T("Kỹ sư chủ trì:", "Lead engineer:"), Loc.T("KTS. Lê Hoàng Minh — Chứng chỉ hành nghề hạng I", "Arch. Le Hoang Minh — Class I practising certificate") },
            new[] { Loc.T("Tiêu chuẩn áp dụng:", "Applied standards:"), "TCVN 5574:2018, QCVN 06:2022/BXD" },
            new[] { Loc.T("Phiên bản tệp BIM:", "BIM file version:"), "Revit 2026 RVT / IFC 4.3" }
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

        backstage.Items.Add(new BackstageItem(Loc.T("Thông tin dự án", "Project information"), IconKind.Info) { Content = infoPanel });

        // 2. Tab Bản Vẽ Gần Đây (Recent Models)
        var recentPanel = new StackPanel { Margin = new Thickness(24) };
        recentPanel.Children.Add(new TextBlock { Text = Loc.T("BẢN VẼ & MÔ HÌNH MỞ GẦN ĐÂY", "RECENT DRAWINGS & MODELS"), FontSize = 16, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)), Margin = new Thickness(0, 0, 0, 14) });

        string[][] recents = {
            new[] { "Landmark_Tower_Tang18_Structure.rvt", Loc.T("Mặt bằng kết cấu tầng 18 — 28.5 MB — Đã sửa 15 phút trước", "Level 18 structural plan — 28.5 MB — edited 15 minutes ago") },
            new[] { "Tower_Foundation_PileCaps.dwg", Loc.T("Móng cọc đài dầm hầm B3 — 14.2 MB — Đã sửa hôm qua", "Pile caps & basement B3 beams — 14.2 MB — edited yesterday") },
            new[] { "MEP_Coordination_Floor18.ifc", Loc.T("Mô hình phối hợp MEP tầng 18 — 45.1 MB — 3 ngày trước", "Level 18 MEP coordination model — 45.1 MB — 3 days ago") },
            new[] { "Cost_Estimate_Master_2026.xlsx", Loc.T("Dự toán tổng hợp gói thầu kết cấu — 5.8 MB — 5 ngày trước", "Structural package cost summary — 5.8 MB — 5 days ago") }
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
        backstage.Items.Add(new BackstageItem(Loc.T("Mở bản vẽ", "Open drawing"), IconKind.Folder) { Content = recentPanel });

        // 3. Tab Xuất Dữ Liệu (Export Formats)
        var exportPanel = new StackPanel { Margin = new Thickness(24) };
        exportPanel.Children.Add(new TextBlock { Text = Loc.T("XUẤT DỮ LIỆU & BÁO CÁO THIẾT KẾ", "EXPORT DATA & DESIGN REPORTS"), FontSize = 16, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)), Margin = new Thickness(0, 0, 0, 14) });

        string[][] exportOpts = {
            new[] { Loc.T("Xuất mô hình chuẩn mở IFC 4.3", "Export an openBIM IFC 4.3 model"), Loc.T("Tương thích cao với Tekla, Archicad, Solibri", "High compatibility with Tekla, Archicad and Solibri") },
            new[] { Loc.T("Xuất bản vẽ CAD 2D/3D DWG/DXF", "Export 2D/3D CAD drawings DWG/DXF"), Loc.T("Bao gồm lớp chuẩn TCVN, mặt cắt và kích thước", "Includes TCVN standard layers, hatches and dimensions") },
            new[] { Loc.T("In ấn tập hồ sơ kỹ thuật PDF đa trang", "Print a multi-page PDF technical dossier"), Loc.T("Khổ giấy A1, A0 độ phân giải vector cao", "A1/A0 sheets at high-resolution vector quality") },
            new[] { Loc.T("Xuất bảng tiên lượng dự toán Excel", "Export the cost take-off to Microsoft Excel"), Loc.T("Tính toán khối lượng tự động theo định mức", "Automatic quantity calculation from norms") }
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
        backstage.Items.Add(new BackstageItem(Loc.T("Xuất dữ liệu", "Export data"), IconKind.Export) { Content = exportPanel });

        backstage.Open();
        backstage.Select(Loc.T("Thông tin dự án", "Project information"));

        var readout = CadHint(Loc.T(
            "Giao diện hậu trường quản lý toàn diện dự án; phím Esc hoặc nút Mở/Đóng để chuyển trạng thái.",
            "The Backstage view manages the whole project; Esc or the Open/Close button toggles it."));

        backstage.SelectedItemChanged += (_, _) => readout.Text = Loc.F("Đang xem trang: {0}", "Viewing page: {0}", backstage.SelectedItem?.Header);
        backstage.IsOpenChanged += (_, _) => readout.Text = backstage.IsOpen ? Loc.T("Đang mở trung tâm quản lý dự án", "Opening the project-management Backstage") : Loc.T("Đã quay trở lại không gian bản vẽ CAD", "Back to the CAD drawing space");

        var openBtn = SecondaryButton(Loc.T("Mở trung tâm dự án", "Open Backstage"));
        openBtn.Click += (_, _) => backstage.Open();

        var closeBtn = SecondaryButton(Loc.T("Đóng trung tâm dự án", "Close Backstage"));
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
            Loc.T("Trung tâm quản lý dự án & tập tin", "Project & file backstage (RibbonBackstageView)"),
            content,
            xaml: @"<ui:RibbonBackstageView x:Name=""Backstage""
                      ApplicationName=""CONCOST BIM Suite""
                      IsOpen=""False"" />",
            csharp: Loc.Code(@"// Thêm các trang quản trị: Thông tin, Bản vẽ gần đây, Xuất file
backstage.Items.Add(new BackstageItem(""Thông tin dự án"", IconKind.Info) { Content = infoPanel });
backstage.Items.Add(new BackstageItem(""Mở bản vẽ"", IconKind.Folder) { Content = recentPanel });
backstage.Open();")));

        return root;
    }

    private object CreateFontPickerDemo()
    {
        // ---------------------------------------------------------------------
        // 1. Khung tên bản vẽ kỹ thuật CAD/BIM (Technical Title Block Preview)
        // ---------------------------------------------------------------------
        // A drawing sheet is paper: white in both themes, with dark ink.
        var titleBlock = new Border
        {
            Height = 220,
            CornerRadius = new CornerRadius(6),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xCB, 0xD5, 0xE1)),
            BorderThickness = new Thickness(1),
            Background = Brushes.White,
            Padding = new Thickness(16)
        };

        var titleBlockGrid = new Grid();
        titleBlockGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        titleBlockGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        titleBlockGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Tên dự án & Công trình
        var projName = new TextBlock
        {
            Text = Loc.T("TÒA NHÀ TRUNG TÂM THƯƠNG MẠI & VĂN PHÒNG LANDMARK 2026", "LANDMARK 2026 COMMERCIAL & OFFICE TOWER"),
            FontSize = 13,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(0x02, 0x84, 0xC7)),
            Margin = new Thickness(0, 0, 0, 6)
        };
        Grid.SetRow(projName, 0);
        titleBlockGrid.Children.Add(projName);

        // Nội dung mẫu bản vẽ và ghi chú kỹ thuật (Chịu ảnh hưởng trực tiếp của FontPicker)
        var previewBody = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        var sheetTitle = new TextBlock
        {
            Text = Loc.T("MẶT BẰNG KẾT CẤU TẦNG ĐIỂN HÌNH (L3 - L20) — TỶ LỆ 1:100", "TYPICAL FLOOR STRUCTURAL PLAN (L3 - L20) — SCALE 1:100"),
            FontSize = 15,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(0x0F, 0x17, 0x2A)),
            Margin = new Thickness(0, 0, 0, 8)
        };
        var noteAnnotation = new TextBlock
        {
            Text = Loc.T("Ghi chú: Dầm D1 (300×600) — Bê tông B25 (C20/25), 4Ø25 thép dọc chịu lực, cốt đai Ø10a150 TCVN 5574:2018.", "Note: beam D1 (300×600) — concrete B25 (C20/25), 4Ø25 longitudinal rebar, Ø10a150 stirrups per TCVN 5574:2018."),
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromRgb(0x47, 0x55, 0x69)),
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

        var authorTxt = new TextBlock { Text = Loc.T("Người vẽ: KTS. Lê Hoàng Minh", "Drawn by: Arch. Le Hoang Minh"), FontSize = 10.5, Foreground = new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B)) };
        var checkerTxt = new TextBlock { Text = Loc.T("Chủ trì: KS. Trần Quốc Bảo", "Lead: Eng. Tran Quoc Bao"), FontSize = 10.5, Foreground = new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B)) };
        var dateTxt = new TextBlock { Text = Loc.T("Ngày: 01/11/2026 | Bản vẽ số: KC-08", "Date: 01/11/2026 | Drawing no.: KC-08"), FontSize = 10.5, Foreground = new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B)), HorizontalAlignment = HorizontalAlignment.Right };

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
            Text = Loc.T("BỘ CHỌN PHÔNG CHỮ BẢN VẼ", "DRAWING TYPOGRAPHY SELECTOR"),
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
            Text = Loc.T("PHÔNG KỸ THUẬT TIÊU CHUẨN", "STANDARD TECHNICAL FONTS (CAD/BIM PRESETS)"),
            FontSize = 11.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 10)
        };
        head2.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        col2.Children.Add(head2);

        var presetsRow = new WrapPanel { Margin = new Thickness(0, 0, 0, 10) };
        var btnSegoe = SecondaryButton(Loc.T("Segoe UI (BIM hiện đại)", "Segoe UI (modern BIM)"));
        btnSegoe.Click += (_, _) => { picker.SelectedFontFamily = new FontFamily("Segoe UI"); picker.SelectedFontSize = 14; UpdatePreviewFont(); };

        var btnArial = SecondaryButton(Loc.T("Arial (TCVN chuẩn)", "Arial (TCVN standard)"));
        btnArial.Click += (_, _) => { picker.SelectedFontFamily = new FontFamily("Arial"); picker.SelectedFontSize = 13; UpdatePreviewFont(); };

        var btnConsolas = SecondaryButton(Loc.T("Consolas (Đơn cách kỹ thuật)", "Consolas (technical monospace)"));
        btnConsolas.Click += (_, _) => { picker.SelectedFontFamily = new FontFamily("Consolas"); picker.SelectedFontSize = 13; UpdatePreviewFont(); };

        presetsRow.Children.Add(btnSegoe);
        presetsRow.Children.Add(btnArial);
        presetsRow.Children.Add(btnConsolas);
        col2.Children.Add(presetsRow);

        var infoCard = CreateCalloutCard(
            Loc.T("QUY TẮC PHÔNG CHỮ BẢN VẼ KỸ THUẬT CAD/BIM:\n", "CAD/BIM DRAWING TYPOGRAPHY RULES:\n") +
            Loc.T("• Tiêu đề bản vẽ: 14 - 18 pt, Chữ in hoa, Bán đậm / Đậm.\n", "• Drawing titles: 14 - 18 pt, upper case, SemiBold/Bold.\n") +
            Loc.T("• Ghi chú kết cấu & thép: 10 - 12 pt, Thường, dễ đọc khi in ở tỷ lệ 1:100.\n", "• Structural & rebar notes: 10 - 12 pt, Regular, legible when printed at 1:100.\n") +
            Loc.T("• Ký hiệu trục tọa độ: 10 - 12 pt, Đậm, nằm trọn trong bong bóng trục Ø8mm.\n", "• Grid bubbles: 10 - 12 pt, Bold, fully inside the Ø8 mm bubble.\n") +
            Loc.T("• Thay đổi phông và kích cỡ ở bên trái để quan sát khung tên mẫu ở trên co giãn trực quan.", "• Change the font and size on the left to watch the sample title block above scale."));
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

        string csCode = Loc.Code(@"// Lắng nghe sự kiện thay đổi font để cập nhật kiểu chữ bản vẽ
picker.SelectionChanged += (s, e) =>
{
    textBlock.FontFamily = picker.SelectedFontFamily;
    textBlock.FontSize = picker.SelectedFontSize;
    textBlock.FontWeight = picker.EffectiveWeight;
    textBlock.FontStyle = picker.EffectiveStyle;
};");

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Bộ chọn phông bản vẽ kỹ thuật", "Drawing font picker (FontPicker)"),
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

        var version = new TextBlock { Text = Loc.T("Phiên bản 1.3 — CAD shell", "Version 1.3 — CAD shell"), FontSize = 12, Margin = new Thickness(0, 4, 0, 12) };
        version.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var note = new TextBlock
        {
            Text = Loc.T("Trang backstage là nội dung thuần: host đặt file list, tuỳ chọn hay bản xem trước in ở đây.", "The Backstage page is plain content: the host puts its file list, options or print preview here."),
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap
        };
        note.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        panel.Children.Add(title);
        panel.Children.Add(version);
        panel.Children.Add(note);

        return panel;
    }

    /// <summary>Stand-in beam model implementing ITypeSelectable.</summary>
    private sealed class CadDemoBeam : ITypeSelectable
    {
        public string Mark { get; set; } = "D1";

        public string TypeName { get; set; } = "Dầm BTCT 250×400";

        public IReadOnlyList<string> AvailableTypes { get; } = new[]
        {
            "Dầm BTCT 250×400",
            "Dầm BTCT 300×500",
            "Dầm BTCT 350×600",
            "Dầm BTCT 400×700"
        };

        public double Length { get; set; } = 6000;

        public double Height { get; set; } = 400;

        public double Width { get; set; } = 250;

        public string Material { get; set; } = Loc.T("Bê tông B25", "Concrete B25");

        public string RebarGrade { get; set; } = "CB400-V";

        public string Story { get; set; } = "Level 2 (+3.600m)";

        public bool IsLocked { get; set; }
    }

    /// <summary>Stand-in steel column model implementing ITypeSelectable.</summary>
    private sealed class CadDemoColumn : ITypeSelectable
    {
        public string Mark { get; set; } = "C1";

        public string TypeName { get; set; } = "Cột thép H400×400";

        public IReadOnlyList<string> AvailableTypes { get; } = new[]
        {
            "Cột thép H300×300",
            "Cột thép H350×350",
            "Cột thép H400×400",
            "Cột thép H450×450"
        };

        public double Height { get; set; } = 3600;

        public double Width { get; set; } = 400;

        public double Depth { get; set; } = 400;

        public string Material { get; set; } = "SS400";

        public string Story { get; set; } = "Level 1-2";

        public bool IsLocked { get; set; }
    }

    /// <summary>Stand-in diaphragm wall model implementing ITypeSelectable.</summary>
    private sealed class CadDemoWall : ITypeSelectable
    {
        public string Mark { get; set; } = "W1";

        public string TypeName { get; set; } = "Tường vây BTCT D600";

        public IReadOnlyList<string> AvailableTypes { get; } = new[]
        {
            "Tường vây BTCT D600",
            "Tường vây BTCT D800",
            "Tường vây BTCT D1000"
        };

        public double Length { get; set; } = 5000;

        public double Height { get; set; } = 3200;

        public double Thickness { get; set; } = 300;

        public string Material { get; set; } = Loc.T("Bê tông B30", "Concrete B30");

        public string Story { get; set; } = "Basement B1";

        public bool IsLocked { get; set; }
    }
}
