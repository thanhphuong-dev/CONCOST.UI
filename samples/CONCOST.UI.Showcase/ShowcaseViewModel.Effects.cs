using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CONCOST.UI.Controls;
using CONCOST.UI.AttachedProperties;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Showcase;

public partial class ShowcaseViewModel
{
    // =========================================================================
    // DEMO: Ripple Effect Control & Attached Property
    // =========================================================================
    private object CreateRippleDemo()
    {
        var mainSp = new StackPanel { Margin = new Thickness(0) };

        void AddCardSection(string title, UIElement content, string? xamlSnippet = null, string? csSnippet = null)
        {
            mainSp.Children.Add(CreateStandardDemoCard(title, content, xamlSnippet, csSnippet));
        }

        // ---------------------------------------------------------------------
        // SECTION 1: Standard Material Ripple Wrapper Containers
        // ---------------------------------------------------------------------
        Grid CreateBasicRippleSection()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var r1 = new Ripple { CornerRadius = new CornerRadius(6) };
            var btn1 = new Button { Content = "Primary Wave Action", Height = 42 };
            btn1.SetResourceReference(Button.StyleProperty, "CONCOST.Button.Primary");
            BorderElement.SetCornerRadius(btn1, new CornerRadius(6));
            r1.Content = btn1;
            Grid.SetColumn(r1, 0);
            grid.Children.Add(r1);

            var r2 = new Ripple { CornerRadius = new CornerRadius(6) };
            var btn2 = new Button { Content = "Secondary Soft Wave", Height = 42 };
            btn2.SetResourceReference(Button.StyleProperty, "CONCOST.Button.Secondary");
            BorderElement.SetCornerRadius(btn2, new CornerRadius(6));
            r2.Content = btn2;
            Grid.SetColumn(r2, 2);
            grid.Children.Add(r2);

            var r3 = new Ripple { CornerRadius = new CornerRadius(6) };
            var btn3 = new Button { Content = "Critical Execution Wave", Height = 42 };
            btn3.SetResourceReference(Button.StyleProperty, "CONCOST.Button.Danger");
            BorderElement.SetCornerRadius(btn3, new CornerRadius(6));
            r3.Content = btn3;
            Grid.SetColumn(r3, 4);
            grid.Children.Add(r3);

            return grid;
        }

        string sec1Xaml =
@"<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width=""*"" />
        <ColumnDefinition Width=""14"" />
        <ColumnDefinition Width=""*"" />
        <ColumnDefinition Width=""14"" />
        <ColumnDefinition Width=""*"" />
    </Grid.ColumnDefinitions>
    <controls:Ripple Grid.Column=""0"" CornerRadius=""6"">
        <Button Style=""{DynamicResource CONCOST.Button.Primary}"" Content=""Primary Wave Action"" Height=""42"" />
    </controls:Ripple>
    <controls:Ripple Grid.Column=""2"" CornerRadius=""6"">
        <Button Style=""{DynamicResource CONCOST.Button.Secondary}"" Content=""Secondary Soft Wave"" Height=""42"" />
    </controls:Ripple>
    <controls:Ripple Grid.Column=""4"" CornerRadius=""6"">
        <Button Style=""{DynamicResource CONCOST.Button.Danger}"" Content=""Critical Execution Wave"" Height=""42"" />
    </controls:Ripple>
</Grid>";

        AddCardSection("1. Khung bao Ripple chuẩn cho vật liệu", CreateBasicRippleSection(), sec1Xaml);

        // ---------------------------------------------------------------------
        // SECTION 2: Custom Ripple Brushes & Ripple Modes (Target vs Center)
        // ---------------------------------------------------------------------
        Grid CreateModeRippleSection()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Card 1: Target Origin (Click Point)
            var card1 = new Border
            {
                Height = 85,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(14)
            };
            card1.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
            card1.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
            var sp1 = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            var t1 = new TextBlock { Text = "Target Mode (Click Point)", FontWeight = FontWeights.Normal, FontSize = 13 };
            t1.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
            var sub1 = new TextBlock { Text = "Wave expands from exact pointer coordinate", FontSize = 11, Margin = new Thickness(0, 4, 0, 0) };
            sub1.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
            sp1.Children.Add(t1); sp1.Children.Add(sub1);
            card1.Child = sp1;

            var r1 = new Ripple { Mode = RippleMode.Target, CornerRadius = new CornerRadius(8), Duration = TimeSpan.FromMilliseconds(420), MaxConcurrentRipples = 4, Content = card1, Cursor = Cursors.Hand };
            Grid.SetColumn(r1, 0); grid.Children.Add(r1);

            // Card 2: Center Mode (Geometric Center)
            var card2 = new Border
            {
                Height = 85,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(14)
            };
            card2.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
            card2.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
            var sp2 = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            var t2 = new TextBlock { Text = "Center Mode (Radiating)", FontWeight = FontWeights.Normal, FontSize = 13 };
            t2.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
            var sub2 = new TextBlock { Text = "Wave expands outwards from card center", FontSize = 11, Margin = new Thickness(0, 4, 0, 0) };
            sub2.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
            sp2.Children.Add(t2); sp2.Children.Add(sub2);
            card2.Child = sp2;

            var r2 = new Ripple { Mode = RippleMode.Center, CornerRadius = new CornerRadius(8), Duration = TimeSpan.FromMilliseconds(420), MaxConcurrentRipples = 4, Content = card2, Cursor = Cursors.Hand };
            Grid.SetColumn(r2, 2); grid.Children.Add(r2);

            // Card 3: Custom Brand Orange Wave
            var card3 = new Border
            {
                Height = 85,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12, 4, 12, 4),
                Padding = new Thickness(14)
            };
            card3.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
            card3.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
            var sp3 = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            var t3 = new TextBlock { Text = "Accent Orange Wave", FontWeight = FontWeights.Normal, FontSize = 13 };
            t3.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.Primary");
            var sub3 = new TextBlock { Text = "Brand color, mixed corner clipping, and bounded wave overlap", FontSize = 11, Margin = new Thickness(0, 4, 0, 0) };
            sub3.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
            sp3.Children.Add(t3); sp3.Children.Add(sub3);
            card3.Child = sp3;

            var r3 = new Ripple
            {
                CornerRadius = new CornerRadius(12, 4, 12, 4),
                RippleBrush = new SolidColorBrush(Color.FromRgb(255, 121, 0)),
                MaxOpacity = 0.45,
                Duration = TimeSpan.FromMilliseconds(560),
                Content = card3,
                Cursor = Cursors.Hand
            };
            Grid.SetColumn(r3, 4); grid.Children.Add(r3);

            return grid;
        }

        string sec2Xaml =
@"<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width=""*"" />
        <ColumnDefinition Width=""14"" />
        <ColumnDefinition Width=""*"" />
        <ColumnDefinition Width=""14"" />
        <ColumnDefinition Width=""*"" />
    </Grid.ColumnDefinitions>
    <controls:Ripple Grid.Column=""0"" Mode=""Target"" CornerRadius=""8"" Duration=""0:00:00.420"" MaxConcurrentRipples=""4"">
        <Border Style=""{DynamicResource CONCOST.Border.Card}"" Height=""85"" Padding=""14"">
            <TextBlock Text=""Target Mode (Click Point)"" FontWeight=""Normal"" />
        </Border>
    </controls:Ripple>
    <controls:Ripple Grid.Column=""2"" Mode=""Center"" CornerRadius=""8"" Duration=""0:00:00.420"" MaxConcurrentRipples=""4"">
        <Border Style=""{DynamicResource CONCOST.Border.Card}"" Height=""85"" Padding=""14"">
            <TextBlock Text=""Center Mode (Radiating)"" FontWeight=""Normal"" />
        </Border>
    </controls:Ripple>
    <controls:Ripple Grid.Column=""4"" RippleBrush=""#FF7900"" MaxOpacity=""0.45"" Duration=""0:00:00.560"" MaxConcurrentRipples=""4"" CornerRadius=""12,4,12,4""><!-- Mixed per-corner clipping -->
        <Border Style=""{DynamicResource CONCOST.Border.Card}"" CornerRadius=""12,4,12,4"" Height=""85"" Padding=""14"">
            <TextBlock Text=""Accent Orange Wave"" Foreground=""{DynamicResource CONCOST.Brush.Primary}"" />
        </Border>
    </controls:Ripple>
</Grid>";

        var sec2Cs = @"var ripple = new Ripple
{
    Mode = RippleMode.Target,
    CornerRadius = new CornerRadius(12, 4, 12, 4),
    RippleBrush = new SolidColorBrush(Color.FromRgb(255, 121, 0)),
    MaxOpacity = 0.45,
    Duration = TimeSpan.FromMilliseconds(560),
    MaxConcurrentRipples = 4
};

// Optional: trigger the effect from code.
ripple.TriggerRipple(new Point(24, 12));";
        AddCardSection("2. Tuỳ biến brush và chế độ Ripple (Target và Center)", CreateModeRippleSection(), sec2Xaml, sec2Cs);

        // ---------------------------------------------------------------------
        // SECTION 3: Attached Property RippleElement (Buttons & Cards)
        // ---------------------------------------------------------------------
        Grid CreateAttachedRippleSection()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var b1 = new Button { Content = "Attached Button Ripple", Height = 42 };
            b1.SetResourceReference(Button.StyleProperty, "CONCOST.Button.Primary");
            BorderElement.SetCornerRadius(b1, new CornerRadius(6));
            RippleElement.SetIsEnabled(b1, true);
            RippleElement.SetCornerRadius(b1, new CornerRadius(6));
            Grid.SetColumn(b1, 0); grid.Children.Add(b1);

            var b2 = new Button { Content = "Secondary Attached Ripple", Height = 42 };
            b2.SetResourceReference(Button.StyleProperty, "CONCOST.Button.Secondary");
            BorderElement.SetCornerRadius(b2, new CornerRadius(6));
            RippleElement.SetIsEnabled(b2, true);
            RippleElement.SetCornerRadius(b2, new CornerRadius(6));
            RippleElement.SetMode(b2, RippleMode.Center);
            RippleElement.SetDuration(b2, TimeSpan.FromMilliseconds(500));
            RippleElement.SetMaxOpacity(b2, 0.28);
            RippleElement.SetMaxConcurrentRipples(b2, 3);
            Grid.SetColumn(b2, 2); grid.Children.Add(b2);

            var card = new Border
            {
                Height = 42,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(14, 0, 14, 0),
                Cursor = Cursors.Hand
            };
            card.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
            card.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
            var cardSp = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            var cardIcon = new Icon { Kind = IconKind.Layers, Width = 16, Height = 16, Margin = new Thickness(0, 0, 8, 0) };
            cardIcon.SetResourceReference(Icon.ForegroundProperty, "CONCOST.Brush.Primary");
            var cardText = new TextBlock { Text = "Interactive Tile Ripple", FontWeight = FontWeights.Normal, FontSize = 12.5 };
            cardText.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
            cardSp.Children.Add(cardIcon); cardSp.Children.Add(cardText);
            card.Child = cardSp;

            RippleElement.SetIsEnabled(card, true);
            RippleElement.SetCornerRadius(card, new CornerRadius(6));
            Grid.SetColumn(card, 4); grid.Children.Add(card);

            return grid;
        }

        string sec3Xaml =
@"<Grid xmlns:attached=""clr-namespace:CONCOST.UI.Controls;assembly=CONCOST.UI"">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width=""*"" />
        <ColumnDefinition Width=""14"" />
        <ColumnDefinition Width=""*"" />
        <ColumnDefinition Width=""14"" />
        <ColumnDefinition Width=""*"" />
    </Grid.ColumnDefinitions>
    <Button Grid.Column=""0"" Style=""{DynamicResource CONCOST.Button.Primary}""
            attached:RippleElement.IsEnabled=""True""
            attached:RippleElement.CornerRadius=""6""
            Content=""Attached Button Ripple"" Height=""42"" />
    <Button Grid.Column=""2"" Style=""{DynamicResource CONCOST.Button.Secondary}""
            attached:RippleElement.IsEnabled=""True""
            attached:RippleElement.CornerRadius=""6""
            attached:RippleElement.Mode=""Center""
            attached:RippleElement.Duration=""0:00:00.500""
            attached:RippleElement.MaxOpacity=""0.28""
            attached:RippleElement.MaxConcurrentRipples=""3""
            Content=""Secondary Attached Ripple"" Height=""42"" />
    <Border Grid.Column=""4"" Background=""{DynamicResource CONCOST.Brush.SurfaceSecondary}""
            BorderBrush=""{DynamicResource CONCOST.Brush.Border}"" BorderThickness=""1""
            CornerRadius=""6"" Height=""42"" Padding=""14,0""
            attached:RippleElement.IsEnabled=""True""
            attached:RippleElement.CornerRadius=""6"">
        <TextBlock Text=""Interactive Tile Ripple"" VerticalAlignment=""Center"" />
    </Border>
</Grid>";

        var sec3Cs = @"RippleElement.SetIsEnabled(button, true);
RippleElement.SetCornerRadius(button, new CornerRadius(6));
RippleElement.SetMode(button, RippleMode.Center);
RippleElement.SetDuration(button, TimeSpan.FromMilliseconds(500));
RippleElement.SetMaxOpacity(button, 0.28);
RippleElement.SetMaxConcurrentRipples(button, 3);";
        AddCardSection("3. Attached property RippleElement (nút và thẻ)", CreateAttachedRippleSection(), sec3Xaml, sec3Cs);

        return mainSp;
    }

    // =========================================================================
    // DEMO: Skeleton / Shimmer Loading Placeholder Effect
    // =========================================================================
    private object CreateSkeletonDemo()
    {
        var mainSp = new StackPanel { Margin = new Thickness(0) };

        void AddCardSection(string title, UIElement content, string? xamlSnippet = null, string? csSnippet = null)
        {
            mainSp.Children.Add(CreateStandardDemoCard(title, content, xamlSnippet, csSnippet));
        }

        // ---------------------------------------------------------------------
        // SECTION 1: Basic Skeleton Shapes (TextLine, Circle Avatar, Rectangles)
        // ---------------------------------------------------------------------
        Grid CreateBasicShapesSection()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Column 1: Avatar Profile Placeholder
            var col1 = new Border
            {
                Padding = new Thickness(14),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8)
            };
            col1.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
            col1.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
            var sp1 = new StackPanel();
            var row1 = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };
            var avatar = new Skeleton { SkeletonShape = SkeletonShape.Circle, Width = 42, Height = 42, Margin = new Thickness(0, 0, 12, 0) };
            var textStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            textStack.Children.Add(new Skeleton { SkeletonShape = SkeletonShape.TextLine, Width = 110, Height = 14, Margin = new Thickness(0, 0, 0, 6) });
            textStack.Children.Add(new Skeleton { SkeletonShape = SkeletonShape.TextLine, Width = 70, Height = 11 });
            row1.Children.Add(avatar); row1.Children.Add(textStack);
            sp1.Children.Add(row1);
            sp1.Children.Add(new Skeleton { SkeletonShape = SkeletonShape.TextLine, Height = 12, Margin = new Thickness(0, 0, 0, 6) });
            sp1.Children.Add(new Skeleton { SkeletonShape = SkeletonShape.TextLine, Width = 160, Height = 12, HorizontalAlignment = HorizontalAlignment.Left });
            col1.Child = sp1;
            Grid.SetColumn(col1, 0); grid.Children.Add(col1);

            // Column 2: Document / Drawing Card Placeholder
            var col2 = new Border
            {
                Padding = new Thickness(14),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8)
            };
            col2.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
            col2.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
            var sp2 = new StackPanel();
            sp2.Children.Add(new Skeleton { SkeletonShape = SkeletonShape.Rectangle, Height = 52, CornerRadius = new CornerRadius(6), Margin = new Thickness(0, 0, 0, 10) });
            sp2.Children.Add(new Skeleton { SkeletonShape = SkeletonShape.TextLine, Width = 130, Height = 13, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 0, 0, 6) });
            sp2.Children.Add(new Skeleton { SkeletonShape = SkeletonShape.TextLine, Width = 85, Height = 11, HorizontalAlignment = HorizontalAlignment.Left });
            col2.Child = sp2;
            Grid.SetColumn(col2, 2); grid.Children.Add(col2);

            // Column 3: Structural Member Specification Lines
            var col3 = new Border
            {
                Padding = new Thickness(14),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8)
            };
            col3.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
            col3.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
            var sp3 = new StackPanel();
            sp3.Children.Add(new Skeleton { SkeletonShape = SkeletonShape.TextLine, Height = 14, Margin = new Thickness(0, 0, 0, 8) });
            sp3.Children.Add(new Skeleton { SkeletonShape = SkeletonShape.TextLine, Height = 14, Margin = new Thickness(0, 0, 0, 8) });
            sp3.Children.Add(new Skeleton { SkeletonShape = SkeletonShape.TextLine, Height = 14, Margin = new Thickness(0, 0, 0, 8) });
            sp3.Children.Add(new Skeleton { SkeletonShape = SkeletonShape.TextLine, Width = 100, Height = 14, HorizontalAlignment = HorizontalAlignment.Left });
            col3.Child = sp3;
            Grid.SetColumn(col3, 4); grid.Children.Add(col3);

            return grid;
        }

        string sec1Xaml =
@"<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width=""*"" />
        <ColumnDefinition Width=""14"" />
        <ColumnDefinition Width=""*"" />
        <ColumnDefinition Width=""14"" />
        <ColumnDefinition Width=""*"" />
    </Grid.ColumnDefinitions>
    <!-- Profile Avatar Skeleton -->
    <StackPanel Grid.Column=""0"">
        <StackPanel Orientation=""Horizontal"">
            <controls:Skeleton SkeletonShape=""Circle"" Width=""42"" Height=""42"" Margin=""0,0,12,0"" />
            <StackPanel VerticalAlignment=""Center"">
                <controls:Skeleton SkeletonShape=""TextLine"" Width=""110"" Height=""14"" Margin=""0,0,0,6"" />
                <controls:Skeleton SkeletonShape=""TextLine"" Width=""70"" Height=""11"" />
            </StackPanel>
        </StackPanel>
    </StackPanel>
    <!-- Card Preview Skeleton -->
    <StackPanel Grid.Column=""2"">
        <controls:Skeleton SkeletonShape=""Rectangle"" Height=""52"" CornerRadius=""6"" Margin=""0,0,0,10"" />
        <controls:Skeleton SkeletonShape=""TextLine"" Width=""130"" Height=""13"" Margin=""0,0,0,6"" />
    </StackPanel>
    <!-- Multi-line Spec Skeleton -->
    <StackPanel Grid.Column=""4"">
        <controls:Skeleton SkeletonShape=""TextLine"" Height=""14"" Margin=""0,0,0,8"" />
        <controls:Skeleton SkeletonShape=""TextLine"" Height=""14"" Margin=""0,0,0,8"" />
        <controls:Skeleton SkeletonShape=""TextLine"" Width=""100"" Height=""14"" />
    </StackPanel>
</Grid>";

        AddCardSection("1. Hình skeleton cơ bản (TextLine, Circle Avatar, hình chữ nhật)", CreateBasicShapesSection(), sec1Xaml);

        // ---------------------------------------------------------------------
        // SECTION 2: Full Structural KPI Metric Card Placeholders
        // ---------------------------------------------------------------------
        Grid CreateKpiSkeletonSection()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var sk1 = new Skeleton { SkeletonShape = SkeletonShape.Card, Height = 108 };
            Grid.SetColumn(sk1, 0); grid.Children.Add(sk1);

            var sk2 = new Skeleton { SkeletonShape = SkeletonShape.Card, Height = 108 };
            Grid.SetColumn(sk2, 2); grid.Children.Add(sk2);

            var sk3 = new Skeleton { SkeletonShape = SkeletonShape.Card, Height = 108 };
            Grid.SetColumn(sk3, 4); grid.Children.Add(sk3);

            return grid;
        }

        string sec2Xaml =
@"<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width=""*"" />
        <ColumnDefinition Width=""14"" />
        <ColumnDefinition Width=""*"" />
        <ColumnDefinition Width=""14"" />
        <ColumnDefinition Width=""*"" />
    </Grid.ColumnDefinitions>
    <controls:Skeleton Grid.Column=""0"" SkeletonShape=""Card"" Height=""108"" />
    <controls:Skeleton Grid.Column=""2"" SkeletonShape=""Card"" Height=""108"" />
    <controls:Skeleton Grid.Column=""4"" SkeletonShape=""Card"" Height=""108"" />
</Grid>";

        AddCardSection("2. Placeholder cho thẻ KPI kết cấu đầy đủ", CreateKpiSkeletonSection(), sec2Xaml);

        // ---------------------------------------------------------------------
        // SECTION 3: Interactive Data Fetching Switch (Smooth Loaded Transition)
        // ---------------------------------------------------------------------
        StackPanel CreateInteractiveLoadingSection()
        {
            var container = new StackPanel();

            var toggleBar = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 16) };
            var toggleLabel = new TextBlock
            {
                Text = "IsLoading State:",
                FontWeight = FontWeights.Normal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };
            toggleLabel.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");

            var toggleSwitch = new ToggleSwitch { IsChecked = true, VerticalAlignment = VerticalAlignment.Center };
            var statusLabel = new TextBlock
            {
                Text = "Simulating Shimmer Loading (Fetching BIM Database...)",
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 0, 0)
            };
            statusLabel.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

            toggleBar.Children.Add(toggleLabel);
            toggleBar.Children.Add(toggleSwitch);
            toggleBar.Children.Add(statusLabel);
            container.Children.Add(toggleBar);

            // Interactive Skeleton Wrap around real card content
            var skeletonWrapper = new Skeleton
            {
                IsLoading = true,
                CornerRadius = new CornerRadius(10),
                Height = 112
            };

            var realCard = new Border
            {
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(18, 14, 18, 14)
            };
            realCard.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
            realCard.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

            var cardGrid = new Grid();
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var infoSp = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            var titleTxt = new TextBlock { Text = "Reinforced Concrete Foundation Pad F1-04", FontWeight = FontWeights.Normal, FontSize = 14 };
            titleTxt.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
            var specTxt = new TextBlock { Text = "Dimensions: 2400 x 2400 x 800 mm  •  Concrete Grade: C35/45  •  Status: Approved", FontSize = 12, Margin = new Thickness(0, 6, 0, 0) };
            specTxt.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
            var tagTxt = new TextBlock { Text = "Certified under Eurocode 2 (EN 1992-1-1)", FontSize = 11, FontWeight = FontWeights.Normal, Margin = new Thickness(0, 6, 0, 0) };
            tagTxt.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.Success");
            infoSp.Children.Add(titleTxt); infoSp.Children.Add(specTxt); infoSp.Children.Add(tagTxt);
            Grid.SetColumn(infoSp, 0); cardGrid.Children.Add(infoSp);

            var iconBorder = new Border
            {
                Width = 44,
                Height = 44,
                CornerRadius = new CornerRadius(8),
                VerticalAlignment = VerticalAlignment.Center
            };
            iconBorder.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
            iconBorder.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
            iconBorder.BorderThickness = new Thickness(1);
            var icon = new Icon { Kind = IconKind.CheckCircle, Width = 22, Height = 22, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            icon.SetResourceReference(Icon.ForegroundProperty, "CONCOST.Brush.Success");
            iconBorder.Child = icon;
            Grid.SetColumn(iconBorder, 1); cardGrid.Children.Add(iconBorder);

            realCard.Child = cardGrid;
            skeletonWrapper.Content = realCard;

            toggleSwitch.Checked += (s, e) =>
            {
                skeletonWrapper.IsLoading = true;
                statusLabel.Text = "Simulating Shimmer Loading (Fetching BIM Database...)";
                statusLabel.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
            };
            toggleSwitch.Unchecked += (s, e) =>
            {
                skeletonWrapper.IsLoading = false;
                statusLabel.Text = "Data Loaded Successfully (Faded into live view)";
                statusLabel.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.Success");
            };

            container.Children.Add(skeletonWrapper);
            return container;
        }

        string sec3Xaml =
@"<controls:Skeleton IsLoading=""{Binding IsFetchingData}"" CornerRadius=""10"">
    <!-- Real content is smoothly faded in when IsLoading transitions to False -->
    <Border Style=""{DynamicResource CONCOST.Border.Card}"" Padding=""18,14"">
        <Grid>
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width=""*"" />
                <ColumnDefinition Width=""Auto"" />
            </Grid.ColumnDefinitions>
            <StackPanel Grid.Column=""0"">
                <TextBlock Text=""Reinforced Concrete Foundation Pad F1-04"" FontWeight=""Normal"" />
                <TextBlock Text=""Dimensions: 2400 x 2400 x 800 mm • Status: Approved"" />
            </StackPanel>
            <Border Grid.Column=""1"" Width=""44"" Height=""44"" CornerRadius=""8"">
                <icons:Icon Kind=""CheckCircle"" Width=""22"" Height=""22"" Foreground=""{DynamicResource CONCOST.Brush.Success}"" />
            </Border>
        </Grid>
    </Border>
</controls:Skeleton>";

        AddCardSection("3. Chuyển đổi trạng thái tải dữ liệu (chuyển mượt sang loaded)", CreateInteractiveLoadingSection(), sec3Xaml);

        return mainSp;
    }
}
