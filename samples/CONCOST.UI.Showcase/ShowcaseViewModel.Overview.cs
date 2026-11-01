using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CONCOST.UI.AttachedProperties;
using CONCOST.UI.Icons;
using CONCOST.UI.Services;

namespace CONCOST.UI.Showcase;

public partial class ShowcaseViewModel
{
    private object CreateOverviewDemo()
    {
        var root = new StackPanel();
        string L(string key, string fallback) => LocalizationManager.Current.GetString(key, fallback);
        string ComponentName(string name) =>
            LocalizationManager.Current.Culture.TwoLetterISOLanguageName.Equals("vi", StringComparison.OrdinalIgnoreCase)
                ? name
                : L($"Showcase.Component.{name}", name);

        TextBlock Text(string value, double size, string colorKey, FontWeight? weight = null)
        {
            var text = new TextBlock
            {
                Text = value,
                FontSize = size,
                FontWeight = weight ?? FontWeights.Normal,
                TextWrapping = TextWrapping.Wrap
            };
            text.SetResourceReference(TextBlock.ForegroundProperty, colorKey);
            return text;
        }

        Border Surface(UIElement child, double padding = 20)
        {
            var border = new Border
            {
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(padding),
                Margin = new Thickness(0, 0, 0, 18),
                Child = child
            };
            border.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
            border.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
            return border;
        }

        // Customer-facing introduction and the primary path into a live demo.
        var hero = new Grid();
        hero.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        hero.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var intro = new StackPanel { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Left, MaxWidth = 880 };
        var eyebrow = Text(L("Showcase.Overview.Eyebrow", "CONCOST.UI  /  LIVE COMPONENT SHOWCASE"), 11, "CONCOST.Brush.Primary", FontWeights.SemiBold);
        eyebrow.Margin = new Thickness(0, 0, 0, 12);
        var headline = Text(L("Showcase.Overview.Headline", "Engineering interfaces,\nready for real projects."), 30, "CONCOST.Brush.TextPrimary", FontWeights.SemiBold);
        headline.LineHeight = 38;
        headline.Margin = new Thickness(0, 0, 0, 10);
        var summary = Text(L("Showcase.Overview.Summary", "Explore polished WPF controls built for engineering, CAD, BIM, and construction workflows. Every example is interactive and designed to show how the component behaves in an application."), 15, "CONCOST.Brush.TextSecondary");
        summary.LineHeight = 23;
        summary.Margin = new Thickness(0, 0, 0, 18);
        intro.Children.Add(eyebrow);
        intro.Children.Add(headline);
        intro.Children.Add(summary);

        var actions = new StackPanel { Orientation = Orientation.Horizontal };
        var openSpreadsheet = new Button
        {
            Content = L("Showcase.Overview.OpenSpreadsheet", "Explore Spreadsheet"),
            Padding = new Thickness(16, 9, 16, 9),
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            Cursor = System.Windows.Input.Cursors.Hand,
            Margin = new Thickness(0, 0, 10, 0)
        };
        openSpreadsheet.SetResourceReference(Button.StyleProperty, "CONCOST.Button.Primary");
        BorderElement.SetCornerRadius(openSpreadsheet, new CornerRadius(7));
        IconElement.SetKind(openSpreadsheet, IconKind.Table);
        openSpreadsheet.Click += (_, _) => OpenControl("Spreadsheet");

        var browseCustom = new Button
        {
            Content = L("Showcase.Overview.BrowseCustom", "Browse custom controls"),
            Padding = new Thickness(14, 9, 14, 9),
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            Cursor = System.Windows.Input.Cursors.Hand
        };
        browseCustom.SetResourceReference(Button.StyleProperty, "CONCOST.Button.Secondary");
        BorderElement.SetCornerRadius(browseCustom, new CornerRadius(7));
        browseCustom.Click += (_, _) => BrowseCategory("Custom Controls");
        actions.Children.Add(openSpreadsheet);
        actions.Children.Add(browseCustom);
        intro.Children.Add(actions);
        Grid.SetColumn(intro, 0);
        hero.Children.Add(intro);

        // Compact product preview that makes the library feel tangible before a demo is opened.
        var preview = new Border
        {
            Width = 284,
            MinHeight = 210,
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(18),
            Margin = new Thickness(30, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            BorderThickness = new Thickness(1)
        };
        preview.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
        preview.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
        var previewContent = new StackPanel();
        var previewHeader = new Grid { Margin = new Thickness(0, 0, 0, 16) };
        previewHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        previewHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var previewTitle = Text(L("Showcase.Overview.StructuralSummary", "STRUCTURAL SUMMARY"), 10, "CONCOST.Brush.TextSecondary", FontWeights.SemiBold);
        Grid.SetColumn(previewTitle, 0);
        previewHeader.Children.Add(previewTitle);
        var status = new Border { CornerRadius = new CornerRadius(9), Padding = new Thickness(8, 4, 8, 4), BorderThickness = new Thickness(1) };
        status.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
        status.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
        var statusText = Text(L("Showcase.Overview.Live", "LIVE"), 9, "CONCOST.Brush.Primary", FontWeights.SemiBold);
        status.Child = statusText;
        Grid.SetColumn(status, 1);
        previewHeader.Children.Add(status);
        previewContent.Children.Add(previewHeader);

        var projectName = Text(L("Showcase.Overview.ProjectName", "Tower A · Structural model"), 13, "CONCOST.Brush.TextPrimary", FontWeights.SemiBold);
        projectName.Margin = new Thickness(0, 0, 0, 3);
        previewContent.Children.Add(projectName);
        var projectCaption = Text(L("Showcase.Overview.ProjectCaption", "Coordination snapshot"), 11, "CONCOST.Brush.TextSecondary");
        projectCaption.Margin = new Thickness(0, 0, 0, 15);
        previewContent.Children.Add(projectCaption);

        var previewMetrics = new Grid();
        previewMetrics.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        previewMetrics.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        previewMetrics.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        previewMetrics.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        AddPreviewMetric(previewMetrics, 0, 0, "18", L("Showcase.Overview.Floors", "FLOORS"));
        AddPreviewMetric(previewMetrics, 0, 1, "72", L("Showcase.Overview.Columns", "COLUMNS"));
        AddPreviewMetric(previewMetrics, 1, 0, "C35/45", L("Showcase.Overview.Concrete", "CONCRETE"));
        AddPreviewMetric(previewMetrics, 1, 1, "4,500 m²", L("Showcase.Overview.Footprint", "FOOTPRINT"));
        previewContent.Children.Add(previewMetrics);
        preview.Child = previewContent;
        Grid.SetColumn(preview, 1);
        hero.Children.Add(preview);
        root.Children.Add(Surface(hero, 26));

        // Evidence of scope and theme support, calculated from the actual catalog.
        var stats = new UniformGrid { Columns = 4, Margin = new Thickness(-4, 0, -4, 18) };
        var basicCount = AllItems.Count(x => x.Category == "Basic Controls");
        var customCount = AllItems.Count(x => x.Category == "Custom Controls");
        var guideCount = AllItems.Count(x => x.Category == "Getting Started");

        // Every page except the guide pages is a live demo, so the count covers all six buckets
        // instead of only the two that happen to have a card below.
        AddStat(stats, $"{AllItems.Count - guideCount}", L("Showcase.Overview.Interactive", "INTERACTIVE COMPONENTS"), IconKind.Layers, "CONCOST.Brush.Info");
        AddStat(stats, $"{basicCount}", L("Showcase.Overview.Foundation", "FOUNDATION CONTROLS"), IconKind.Check, "CONCOST.Brush.Success");
        AddStat(stats, $"{customCount}", L("Showcase.Overview.Custom", "CUSTOM CONTROLS"), IconKind.TuneAdjust, "CONCOST.Brush.Warning");
        AddStat(stats, "Light + Dark", L("Showcase.Overview.RuntimeThemes", "RUNTIME THEMES"), IconKind.DarkMode, "CONCOST.Brush.Danger");
        root.Children.Add(stats);

        // A short curated path lets a client see representative controls immediately.
        var featured = new StackPanel();
        featured.Children.Add(CreateSectionHeading(L("Showcase.Overview.Featured", "Featured demonstrations"), L("Showcase.Overview.FeaturedSubtitle", "Open a live example to experience each component.")));
        var featuredWrap = new UniformGrid { Columns = 3 };
        AddFeaturedCard(featuredWrap, "Spreadsheet", L("Showcase.Overview.CategoryData", "DATA CONTROLS"), L("Showcase.Overview.FeatureSpreadsheet", "Formula editing, multi-sheet workbooks, and engineering schedules."), IconKind.Table, "CONCOST.Brush.Info");
        AddFeaturedCard(featuredWrap, "StackedHeaderDataGrid", L("Showcase.Overview.CategoryData", "DATA CONTROLS"), L("Showcase.Overview.FeatureStackedGrid", "Grouped headers for dense engineering datasets."), IconKind.ViewList, "CONCOST.Brush.Success");
        AddFeaturedCard(featuredWrap, "StatisticCard (KPI)", L("Showcase.Overview.CategoryDashboard", "DASHBOARD"), L("Showcase.Overview.FeatureStatistic", "At-a-glance metrics with status and trend indicators."), IconKind.ChartBar, "CONCOST.Brush.Warning");
        AddFeaturedCard(featuredWrap, "PropertyGrid", L("Showcase.Overview.CategoryInput", "INPUT CONTROLS"), L("Showcase.Overview.FeatureProperty", "An editable property inspector for CAD and BIM objects."), IconKind.TuneAdjust, "CONCOST.Brush.Danger");
        AddFeaturedCard(featuredWrap, "CheckComboBox", L("Showcase.Overview.CategoryInput", "INPUT CONTROLS"), L("Showcase.Overview.FeatureCheckCombo", "Searchable multi-select with clear selection feedback."), IconKind.CheckCircle, "CONCOST.Brush.Primary");
        AddFeaturedCard(featuredWrap, "Drawer", L("Showcase.Overview.CategoryNavigation", "NAVIGATION"), L("Showcase.Overview.FeatureDrawer", "An animated side panel for secondary workflows."), IconKind.Menu, "CONCOST.Brush.TextSecondary");
        featured.Children.Add(featuredWrap);
        root.Children.Add(Surface(featured, 22));

        // Clickable categories reduce the time needed to find the right demo in a client meeting.
        var categories = new StackPanel();
        categories.Children.Add(CreateSectionHeading(L("Showcase.Overview.Browse", "Browse the library"), L("Showcase.Overview.BrowseSubtitle", "Filter the catalog by type or open the getting started guide.")));
        var categoryWrap = new UniformGrid { Columns = 3 };
        AddCategoryCard(categoryWrap, "Basic Controls", L("Showcase.Overview.CategoryBasicDescription", "Familiar WPF essentials with a consistent CONCOST finish."), IconKind.Check, basicCount, "CONCOST.Brush.Info");
        AddCategoryCard(categoryWrap, "Custom Controls", L("Showcase.Overview.CategoryCustomDescription", "Purpose-built components for data-heavy engineering applications."), IconKind.TuneAdjust, customCount, "CONCOST.Brush.Success");
        AddCategoryCard(categoryWrap, "Getting Started", L("Showcase.Overview.CategoryGettingStartedDescription", "An overview and guide to integrating the library."), IconKind.File, AllItems.Count(x => x.Category == "Getting Started"), "CONCOST.Brush.Warning");
        categories.Children.Add(categoryWrap);
        root.Children.Add(Surface(categories, 22));

        var hint = new Border
        {
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(16, 12, 16, 12),
            Margin = new Thickness(0, 0, 0, 12),
            BorderThickness = new Thickness(1)
        };
        hint.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
        hint.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
        var hintContent = new StackPanel();
        hintContent.Children.Add(Text(L("Showcase.Overview.PresentationTipTitle", "EXPLORE THE LIBRARY"), 10, "CONCOST.Brush.Primary", FontWeights.SemiBold));
        var hintDescription = Text(L("Showcase.Overview.PresentationTip", "Choose a control from the catalog to open its interactive demo. Select View Code to inspect the XAML and C# examples."), 13, "CONCOST.Brush.TextSecondary");
        hintDescription.LineHeight = 20;
        hintDescription.Margin = new Thickness(0, 4, 0, 0);
        hintContent.Children.Add(hintDescription);
        hint.Child = hintContent;
        root.Children.Add(hint);
        return root;

        void AddPreviewMetric(Grid grid, int row, int column, string value, string label)
        {
            var metric = new StackPanel { Margin = new Thickness(column == 0 ? 0 : 10, row == 0 ? 0 : 14, 0, 0) };
            var valueText = Text(value, 14, "CONCOST.Brush.TextPrimary", FontWeights.SemiBold);
            valueText.Margin = new Thickness(0, 0, 0, 3);
            metric.Children.Add(valueText);
            metric.Children.Add(Text(label, 10, "CONCOST.Brush.TextSecondary", FontWeights.SemiBold));
            Grid.SetRow(metric, row);
            Grid.SetColumn(metric, column);
            grid.Children.Add(metric);
        }

        void AddStat(Panel panel, string value, string label, IconKind iconKind, string accentKey)
        {
            var body = new Grid();
            body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(38) });
            body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var iconBox = new Border { Width = 34, Height = 34, CornerRadius = new CornerRadius(8), VerticalAlignment = VerticalAlignment.Center };
            var icon = new Icon { Kind = iconKind, Width = 16, Height = 16, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            SetAccentIcon(iconBox, icon, accentKey);
            iconBox.Child = icon;
            body.Children.Add(iconBox);
            var copy = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 4, 0) };
            var valueText = Text(value, 18, "CONCOST.Brush.TextPrimary", FontWeights.SemiBold);
            valueText.Margin = new Thickness(0, 0, 0, 3);
            copy.Children.Add(valueText);
            var labelText = Text(label, 10, "CONCOST.Brush.TextSecondary", FontWeights.SemiBold);
            labelText.LineHeight = 13;
            copy.Children.Add(labelText);
            Grid.SetColumn(copy, 1);
            body.Children.Add(copy);
            var card = Surface(body, 14);
            card.Margin = new Thickness(4);
            panel.Children.Add(card);
        }

        StackPanel CreateSectionHeading(string title, string subtitle)
        {
            var heading = new StackPanel { Margin = new Thickness(0, 0, 0, 14) };
            heading.Children.Add(Text(title, 18, "CONCOST.Brush.TextPrimary", FontWeights.SemiBold));
            var description = Text(subtitle, 12, "CONCOST.Brush.TextSecondary");
            description.Margin = new Thickness(0, 3, 0, 0);
            heading.Children.Add(description);
            return heading;
        }

        void AddFeaturedCard(Panel panel, string name, string category, string description, IconKind iconKind, string accentKey)
        {
            var cardContent = new StackPanel();
            var top = new Grid { Margin = new Thickness(0, 0, 0, 12) };
            top.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(54) });
            top.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var iconPlate = new Border { Width = 36, Height = 36, CornerRadius = new CornerRadius(9) };
            var icon = new Icon { Kind = iconKind, Width = 17, Height = 17, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            SetAccentIcon(iconPlate, icon, accentKey);
            iconPlate.Child = icon;
            top.Children.Add(iconPlate);
            var categoryText = Text(category.ToUpperInvariant(), 9, "CONCOST.Brush.TextSecondary", FontWeights.SemiBold);
            categoryText.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(categoryText, 1);
            top.Children.Add(categoryText);
            cardContent.Children.Add(top);
            cardContent.Children.Add(Text(ComponentName(name), 14, "CONCOST.Brush.TextPrimary", FontWeights.SemiBold));
            var desc = Text(description, 12, "CONCOST.Brush.TextSecondary");
            desc.LineHeight = 18;
            desc.Margin = new Thickness(0, 5, 0, 0);
            cardContent.Children.Add(desc);

            var button = new Button
            {
                Content = cardContent,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinHeight = 140,
                Padding = new Thickness(15),
                Margin = new Thickness(4),
                Cursor = System.Windows.Input.Cursors.Hand,
                ToolTip = $"Open the {name} demonstration"
            };
            button.SetResourceReference(Button.StyleProperty, "Showcase.OverviewCardButton");
            button.Click += (_, _) => OpenControl(name);
            panel.Children.Add(button);
        }

        void AddCategoryCard(Panel panel, string name, string description, IconKind iconKind, int count, string accentKey)
        {
            var content = new Grid();
            content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var iconPlate = new Border { Width = 36, Height = 36, CornerRadius = new CornerRadius(9) };
            var icon = new Icon { Kind = iconKind, Width = 17, Height = 17, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            SetAccentIcon(iconPlate, icon, accentKey);
            iconPlate.Child = icon;
            content.Children.Add(iconPlate);
            var details = new StackPanel { Margin = new Thickness(10, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            var categoryLabel = name switch
            {
                "Basic Controls" => L("Showcase.Category.Basic", name),
                "Custom Controls" => L("Showcase.Category.Custom", name),
                _ => L("Showcase.Category.GettingStarted", name)
            };
            details.Children.Add(Text($"{categoryLabel}  ·  {count}", 13, "CONCOST.Brush.TextPrimary", FontWeights.SemiBold));
            var desc = Text(description, 12, "CONCOST.Brush.TextSecondary");
            desc.LineHeight = 18;
            desc.Margin = new Thickness(0, 4, 0, 0);
            details.Children.Add(desc);
            Grid.SetColumn(details, 1);
            content.Children.Add(details);
            var button = new Button
            {
                Content = content,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinHeight = 88,
                Padding = new Thickness(14),
                Margin = new Thickness(4),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            button.SetResourceReference(Button.StyleProperty, "Showcase.OverviewCardButton");
            button.Click += (_, _) =>
            {
                if (name == "Getting Started")
                    OpenControl("Documentation");
                else
                    BrowseCategory(name);
            };
            panel.Children.Add(button);
        }

        void SetAccentIcon(Border plate, Icon icon, string accentKey)
        {
            plate.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
            plate.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
            plate.BorderThickness = new Thickness(1);
            icon.SetResourceReference(Icon.ForegroundProperty, accentKey);
        }
    }
}
