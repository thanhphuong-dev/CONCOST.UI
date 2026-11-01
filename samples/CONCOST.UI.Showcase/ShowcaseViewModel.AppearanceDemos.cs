using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CONCOST.UI.AttachedProperties;
using CONCOST.UI.Controls;
using CONCOST.UI.Core.Helpers;
using CONCOST.UI.Icons;
using CONCOST.UI.Services;
using CONCOST.UI.Themes;

namespace CONCOST.UI.Showcase;

public partial class ShowcaseViewModel
{
    // =========================================================================
    // DEMO: the appearance pack — elevation, hover, press, focus ring, surfaces
    // =========================================================================

    private object CreateAppearanceLabDemo()
    {
        string T(string vietnamese, string english) =>
            LocalizationManager.Current.Culture.TwoLetterISOLanguageName.Equals("vi", StringComparison.OrdinalIgnoreCase)
                ? vietnamese
                : english;

        var root = new StackPanel();

        root.Children.Add(CreateAppearancePlayground(T));
        root.Children.Add(CreateAppearanceAnatomy(T));
        root.Children.Add(CreateSurfaceRoles(T));
        root.Children.Add(CreateAccentSwatches(T));

        return root;
    }

    // ------------------------------------------------------------------ playground

    private Border CreateAppearancePlayground(Func<string, string, string> t)
    {
        // ---- the three subjects every slider drives -------------------------
        var card = new Card
        {
            Header = t("Cấu kiện D1 — Dầm chính", "Member D1 — Main beam"),
            Footer = t("Bê tông M300 · 2.41 m³", "Concrete M300 · 2.41 m³"),
            Icon = IconKind.Layers,
            Width = 246,
            Elevation = 2,
            IsInteractive = true,
            AccentBar = CardAccent.Left,
            Margin = new Thickness(0, 0, 20, 0)
        };

        var cardBody = new StackPanel();
        cardBody.Children.Add(DemoCaption(t("Thể tích bê tông", "Concrete volume")));

        var value = new TextBlock { Text = "2.41 m³", FontSize = 26, FontWeight = FontWeights.SemiBold };
        value.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        cardBody.Children.Add(value);

        var note = new TextBlock
        {
            Text = t("Cấp bê tông theo TCVN 5574, đổ tại công trường.", "Grade per TCVN 5574, cast on site."),
            FontSize = 11.5,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 0)
        };
        note.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        cardBody.Children.Add(note);
        card.Content = cardBody;

        var button = new Button
        {
            Content = t("Áp dụng", "Apply"),
            Padding = new Thickness(20, 10, 20, 10),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 20, 0)
        };
        button.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Primary");

        var chip = new Border
        {
            Padding = new Thickness(16, 10, 16, 10),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            VerticalAlignment = VerticalAlignment.Center
        };
        SurfaceElement.SetRole(chip, SurfaceRole.Elevated);
        SurfaceElement.SetBorder(chip, SurfaceBorder.Soft);

        var chipLabel = new TextBlock { Text = t("Bản vẽ KT-01", "Drawing A-101"), FontSize = 12 };
        chipLabel.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        chip.Child = chipLabel;

        var subjects = new List<FrameworkElement> { card, button, chip };

        var stageRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        stageRow.Children.Add(card);
        stageRow.Children.Add(button);
        stageRow.Children.Add(chip);

        var stage = new Border
        {
            Padding = new Thickness(28, 34, 28, 34),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(14),
            VerticalAlignment = VerticalAlignment.Top
        };
        SurfaceElement.SetRole(stage, SurfaceRole.Sunken);
        SurfaceElement.SetBorder(stage, SurfaceBorder.Soft);
        stage.Child = stageRow;

        // ---- live state -----------------------------------------------------
        int elevation = 2;
        int hoverElevation = 1;
        bool interactive = true;

        void ApplyElevation()
        {
            foreach (FrameworkElement subject in subjects)
            {
                if (subject is Card subjectCard)
                {
                    // Card fans the value out through the same pack, so the demo never drifts from
                    // what the control really does.
                    subjectCard.Elevation = elevation;
                }
                else
                {
                    VisualElement.SetElevation(subject, elevation);
                }
            }
        }

        void ApplyHoverElevation()
        {
            foreach (FrameworkElement subject in subjects)
            {
                if (subject is Card && interactive) continue;
                VisualElement.SetHoverElevation(subject, hoverElevation);
            }
        }

        var controls = new StackPanel { Width = 330, VerticalAlignment = VerticalAlignment.Top };

        controls.Children.Add(AppearanceSlider(t("Độ nổi (Elevation)", "Elevation"), 0, 4, elevation, 1, "0", v =>
        {
            elevation = (int)Math.Round(v);
            ApplyElevation();
        }));

        controls.Children.Add(AppearanceSlider(t("Độ nổi khi rê chuột", "HoverElevation"), 0, 3, hoverElevation, 1, "0", v =>
        {
            hoverElevation = (int)Math.Round(v);
            ApplyHoverElevation();
        }));

        controls.Children.Add(AppearanceToggle(t("Card tự nổi khi rê chuột (IsInteractive)", "Card lifts on hover (IsInteractive)"), interactive, v =>
        {
            interactive = v;
            card.IsInteractive = v;
            ApplyElevation();
            ApplyHoverElevation();
        }));

        controls.Children.Add(AppearanceSlider(t("Phóng to khi rê (HoverScale)", "Hover scale"), 0.94, 1.12, 1.03, 0.01, "0.00", v =>
        {
            foreach (FrameworkElement subject in subjects) VisualElement.SetHoverScale(subject, v);
        }));

        controls.Children.Add(AppearanceSlider(t("Thu nhỏ khi nhấn (PressScale)", "Press scale"), 0.85, 1.0, 0.97, 0.01, "0.00", v =>
        {
            foreach (FrameworkElement subject in subjects) VisualElement.SetPressScale(subject, v);
        }));

        controls.Children.Add(AppearanceSlider(t("Thời lượng chuyển động (ms)", "Motion duration (ms)"), 0, 600, 160, 20, "0", v =>
        {
            foreach (FrameworkElement subject in subjects) VisualElement.SetMotionDuration(subject, v);
        }));

        controls.Children.Add(AppearanceSlider(t("Gốc phóng to (ScaleOrigin)", "Scale origin"), 0, 1, 0.5, 0.1, "0.0", v =>
        {
            foreach (FrameworkElement subject in subjects) VisualElement.SetScaleOrigin(subject, v);
        }));

        controls.Children.Add(AppearanceSlider(t("Mờ khi vô hiệu (DisabledOpacity)", "Disabled opacity"), 0.2, 1, 0.45, 0.05, "0.00", v =>
        {
            foreach (FrameworkElement subject in subjects) VisualElement.SetDisabledOpacity(subject, v);
            if (!button.IsEnabled) VisualElement.SetDisabledOpacity(button, v);
        }));

        controls.Children.Add(AppearanceToggle(t("Quầng sáng accent (Glow)", "Accent glow (Glow)"), false, v =>
        {
            foreach (FrameworkElement subject in subjects) VisualElement.SetGlow(subject, v);
        }));

        controls.Children.Add(AppearanceToggle(t("Vòng focus bàn phím (FocusRing)", "Keyboard focus ring (FocusRing)"), false, v =>
        {
            foreach (FrameworkElement subject in subjects) VisualElement.SetFocusRing(subject, v);
        }));

        controls.Children.Add(AppearanceToggle(t("Vô hiệu hoá nút giữa", "Disable the middle button"), false, v =>
        {
            button.IsEnabled = !v;
        }));

        var hint = new TextBlock
        {
            Text = t(
                "Rê chuột và nhấn thử lên thẻ, nút và chip để thấy chuyển động; chuyển theme tối/sáng bên dưới để thấy bóng đổi màu theo theme.",
                "Hover and press the card, button and chip to feel the motion; switch the theme below to watch the shadows re-tint."),
            FontSize = 11.5,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 12, 0, 0)
        };
        hint.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextTertiary");
        controls.Children.Add(hint);

        var layout = new Grid();
        layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        Grid.SetColumn(stage, 0);
        Grid.SetColumn(controls, 1);
        layout.Children.Add(stage);
        layout.Children.Add(controls);

        return DemoSection(
            t("Sân chơi thuộc tính mở rộng", "Extended property playground"),
            layout,
            xaml:
                "<!-- Any element: no template of its own is required. -->\n" +
                "<Button Style=\"{StaticResource CONCOST.Button.Primary}\"\n" +
                "        attached:VisualElement.Elevation=\"2\"\n" +
                "        attached:VisualElement.HoverElevation=\"1\"\n" +
                "        attached:VisualElement.HoverScale=\"1.03\"\n" +
                "        attached:VisualElement.PressScale=\"0.97\"\n" +
                "        attached:VisualElement.MotionDuration=\"160\"\n" +
                "        attached:VisualElement.Glow=\"True\"\n" +
                "        attached:VisualElement.FocusRing=\"True\"\n" +
                "        Content=\"Áp dụng\" />",
            csharp:
                "// The same behaviour from code, on any UIElement.\n" +
                "VisualElement.SetElevation(button, 2);\n" +
                "VisualElement.SetHoverElevation(button, 1);\n" +
                "VisualElement.SetHoverScale(button, 1.03);\n" +
                "VisualElement.SetPressScale(button, 0.97);\n" +
                "VisualElement.SetMotionDuration(button, 160);\n" +
                "VisualElement.SetScaleOrigin(button, 0.5);\n" +
                "VisualElement.SetDisabledOpacity(button, 0.45);\n" +
                "VisualElement.SetGlow(button, true);\n" +
                "VisualElement.SetFocusRing(button, true);\n\n" +
                "// A tinted shadow instead of the theme grey:\n" +
                "VisualElement.SetShadowColor(button, Color.FromArgb(0x40, 0x02, 0x84, 0xC7));");
    }

    // ------------------------------------------------------------------ card anatomy

    private Border CreateAppearanceAnatomy(Func<string, string, string> t)
    {
        var cards = new List<Card>();

        var row = new WrapPanel();

        foreach (CardAccent placement in new[] { CardAccent.Left, CardAccent.Top, CardAccent.Right, CardAccent.Bottom })
        {
            var sample = new Card
            {
                Header = placement.ToString(),
                Content = new TextBlock
                {
                    Text = t("Dầm phụ D2", "Secondary beam D2"),
                    FontSize = 12
                },
                Icon = IconKind.Layers,
                AccentBar = placement,
                Elevation = 1,
                Width = 172,
                Margin = new Thickness(0, 0, 14, 14)
            };

            ((TextBlock)sample.Content).SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

            cards.Add(sample);
            row.Children.Add(sample);
        }

        var controls = new StackPanel { Margin = new Thickness(0, 6, 0, 0) };

        controls.Children.Add(AppearanceSlider(t("Bo góc (CornerRadius)", "Corner radius"), 0, 28, 10, 1, "0", v =>
        {
            foreach (Card sample in cards) sample.CornerRadius = new CornerRadius(v);
        }));

        controls.Children.Add(AppearanceSlider(t("Độ dày dải accent", "Accent thickness"), 1, 10, 3, 1, "0", v =>
        {
            foreach (Card sample in cards) sample.AccentThickness = v;
        }));

        controls.Children.Add(AppearanceToggle(t("Nổi khi rê chuột (IsInteractive)", "Lift on hover (IsInteractive)"), false, v =>
        {
            foreach (Card sample in cards) sample.IsInteractive = v;
        }));

        return DemoSection(
            t("Card — dải accent và hình học", "Card — accent rule and geometry"),
            new StackPanel { Children = { row, controls } },
            xaml:
                "<Card Style=\"{StaticResource CONCOST.Card.Base}\"\n" +
                "      Header=\"Dầm phụ D2\"\n" +
                "      Icon=\"Layers\"\n" +
                "      controls:Card.AccentBar=\"Left\"\n" +
                "      controls:Card.AccentThickness=\"3\"\n" +
                "      controls:Card.CornerRadius=\"10\"\n" +
                "      controls:Card.Elevation=\"1\"\n" +
                "      controls:Card.IsInteractive=\"True\" />",
            csharp:
                "var card = new Card\n" +
                "{\n" +
                "    Header = \"Dầm phụ D2\",\n" +
                "    Icon = IconKind.Layers,\n" +
                "    AccentBar = CardAccent.Left,\n" +
                "    AccentBrush = (Brush)FindResource(\"CONCOST.Brush.Success\"),\n" +
                "    AccentThickness = 3,\n" +
                "    CornerRadius = new CornerRadius(10),\n" +
                "    Elevation = 1,\n" +
                "    IsInteractive = true,\n" +
                "    HeaderBackground = (Brush)FindResource(\"CONCOST.Brush.SurfaceSecondary\")\n" +
                "};");
    }

    // ------------------------------------------------------------------ surface roles

    private Border CreateSurfaceRoles(Func<string, string, string> t)
    {
        var grid = new WrapPanel();

        void Chip(string caption, SurfaceRole role, SurfaceBorder border)
        {
            var chip = new Border
            {
                Padding = new Thickness(14, 9, 14, 9),
                Margin = new Thickness(0, 0, 10, 10),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(9)
            };

            SurfaceElement.SetRole(chip, role);
            SurfaceElement.SetBorder(chip, border);

            var label = new TextBlock { Text = caption, FontSize = 11.5 };
            label.SetResourceReference(TextBlock.ForegroundProperty,
                role == SurfaceRole.Accent ? "CONCOST.Brush.TextInverse" : "CONCOST.Brush.TextPrimary");

            chip.Child = label;
            grid.Children.Add(chip);
        }

        Chip("Surface", SurfaceRole.Base, SurfaceBorder.Soft);
        Chip("SurfaceSecondary", SurfaceRole.Secondary, SurfaceBorder.Soft);
        Chip("SurfaceElevated", SurfaceRole.Elevated, SurfaceBorder.Strong);
        Chip("SurfaceSunken", SurfaceRole.Sunken, SurfaceBorder.Soft);
        Chip("Primary", SurfaceRole.Accent, SurfaceBorder.Accent);
        Chip("PrimarySoft", SurfaceRole.AccentSoft, SurfaceBorder.Accent);
        Chip("Danger", SurfaceRole.Danger, SurfaceBorder.None);

        var caption = new TextBlock
        {
            Text = t(
                "SurfaceElement đổi vai trò chứ không đổi mã màu: đổi accent hoặc theme ở dưới, mọi chip tự cập nhật qua DynamicResource.",
                "SurfaceElement takes a role, not a hex: change the accent or the theme below and every chip follows through DynamicResource."),
            FontSize = 11.5,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 0)
        };
        caption.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextTertiary");

        return DemoSection(
            t("Vai trò bề mặt có theme", "Themed surface roles"),
            new StackPanel { Children = { grid, caption } },
            xaml:
                "<Border attached:SurfaceElement.Role=\"Elevated\"\n" +
                "        attached:SurfaceElement.Border=\"Strong\"\n" +
                "        attached:SurfaceElement.Radius=\"12\"\n" +
                "        BorderThickness=\"1\" Padding=\"16\">\n" +
                "    <TextBlock Text=\"SurfaceElevated\" />\n" +
                "</Border>",
            csharp:
                "var panel = new Border { BorderThickness = new Thickness(1), Padding = new Thickness(16) };\n" +
                "SurfaceElement.SetRole(panel, SurfaceRole.Elevated);\n" +
                "SurfaceElement.SetBorder(panel, SurfaceBorder.Strong);\n" +
                "SurfaceElement.SetRadius(panel, new CornerRadius(12));");
    }

    // ------------------------------------------------------------------ accent and theme

    private Border CreateAccentSwatches(Func<string, string, string> t)
    {
        var root = new StackPanel();

        root.Children.Add(DemoCaption(t("Màu nhấn (accent) — thay đổi tức thì mọi control và bóng đổ", "Accent colour — every control and shadow follows instantly")));

        var swatches = new WrapPanel { Margin = new Thickness(0, 2, 0, 12) };

        void Swatch(string caption, string hex)
        {
            var button = new Button
            {
                Content = caption,
                Padding = new Thickness(14, 7, 14, 7),
                Margin = new Thickness(0, 0, 8, 8),
                FontSize = 11.5,
                Cursor = Cursors.Hand
            };

            button.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
            BorderElement.SetCornerRadius(button, new CornerRadius(8));

            Color color = ColorHelper.FromHex(hex);

            var dot = new Border
            {
                Width = 10,
                Height = 10,
                CornerRadius = new CornerRadius(5),
                Background = new SolidColorBrush(color),
                Margin = new Thickness(0, 0, 7, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            var content = new StackPanel { Orientation = Orientation.Horizontal };
            content.Children.Add(dot);
            content.Children.Add(new TextBlock { Text = caption, FontSize = 11.5, VerticalAlignment = VerticalAlignment.Center });
            button.Content = content;

            button.Click += (_, _) => ThemeManager.Current.AccentColor = color;

            swatches.Children.Add(button);
        }

        Swatch("Default", "#FF7900");
        Swatch("Ocean", "#0284C7");
        Swatch("Forest", "#16A34A");
        Swatch("Sunset", "#EA580C");
        Swatch("Violet", "#8B5CF6");
        Swatch("Slate", "#475569");

        root.Children.Add(swatches);

        var themeRow = new StackPanel { Orientation = Orientation.Horizontal };

        void ThemeButton(string caption, AppTheme theme)
        {
            var button = new Button
            {
                Content = caption,
                Padding = new Thickness(14, 7, 14, 7),
                Margin = new Thickness(0, 0, 8, 0),
                FontSize = 11.5,
                Cursor = Cursors.Hand
            };

            button.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
            BorderElement.SetCornerRadius(button, new CornerRadius(8));
            button.Click += (_, _) => ThemeManager.Current.Theme = theme;

            themeRow.Children.Add(button);
        }

        ThemeButton(t("Sáng", "Light"), AppTheme.Light);
        ThemeButton(t("Tối", "Dark"), AppTheme.Dark);
        ThemeButton(t("Theo Windows", "Follow Windows"), AppTheme.System);

        root.Children.Add(themeRow);

        return DemoSection(
            t("Accent và theme", "Accent and theme"),
            root,
            csharp:
                "// The accent is regenerated into a full palette: hover, pressed, soft tint, border.\n" +
                "ThemeManager.Current.AccentColor = ColorHelper.FromHex(\"#0284C7\");\n" +
                "ThemeManager.Current.Theme = AppTheme.Dark;\n\n" +
                "// Attached shadows listen for those changes and rebuild themselves, so a card\n" +
                "// keeps a soft light shadow on paper and a deep one on ink.");
    }

    // ------------------------------------------------------------------ helpers

    private static Grid AppearanceSlider(string label, double minimum, double maximum, double value, double tick, string format, Action<double> onChanged)
    {
        var row = new Grid { Margin = new Thickness(0, 0, 0, 9), HorizontalAlignment = HorizontalAlignment.Left };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(145) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(56) });

        var caption = new TextBlock
        {
            Text = label,
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        };
        caption.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var slider = new Slider
        {
            Minimum = minimum,
            Maximum = maximum,
            Value = value,
            TickFrequency = tick,
            SmallChange = tick,
            LargeChange = tick,
            IsSnapToTickEnabled = true,
            VerticalAlignment = VerticalAlignment.Center,
            Width = 190,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        var readout = new TextBlock
        {
            Text = value.ToString(format),
            FontSize = 11,
            FontWeight = FontWeights.SemiBold,
            FontFamily = new FontFamily("Consolas, Courier New, monospace"),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        readout.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");

        var badge = new Border
        {
            CornerRadius = new CornerRadius(4),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(6, 2, 6, 2),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 46,
            Child = readout
        };
        badge.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
        badge.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

        slider.ValueChanged += (_, e) =>
        {
            readout.Text = e.NewValue.ToString(format);
            onChanged(e.NewValue);
        };

        Grid.SetColumn(caption, 0);
        Grid.SetColumn(slider, 1);
        Grid.SetColumn(badge, 2);

        row.Children.Add(caption);
        row.Children.Add(slider);
        row.Children.Add(badge);

        return row;
    }

    private static CheckBox AppearanceToggle(string label, bool value, Action<bool> onChanged)
    {
        var box = new CheckBox
        {
            Content = label,
            IsChecked = value,
            FontSize = 12,
            Margin = new Thickness(0, 0, 18, 10),
            VerticalContentAlignment = VerticalAlignment.Center
        };
        box.SetResourceReference(Control.ForegroundProperty, "CONCOST.Brush.TextPrimary");

        box.Checked += (_, _) => onChanged(true);
        box.Unchecked += (_, _) => onChanged(false);

        return box;
    }
}
