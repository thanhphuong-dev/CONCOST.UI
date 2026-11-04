using System.ComponentModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;

namespace CONCOST.UI.AttachedProperties
{
    /// <summary>
    /// Gives an icon-only control an accessible name taken from its tooltip.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An icon button has no text, so a screen reader announces it as an unnamed "button". The
    /// tooltip is usually the only human-readable string the control has, but WPF does not use it as
    /// the automation name — <see cref="AutomationProperties.Name"/> is the only thing UI Automation
    /// reads. Setting the property from every call site is the kind of thing that gets done once and
    /// then forgotten on the next button, so the icon button styles switch this on instead and every
    /// button that uses them is named.
    /// </para>
    /// <para>
    /// The name is only filled in when the control has none: a host that set its own
    /// <see cref="AutomationProperties.Name"/> keeps it. The tooltip is read when the element loads
    /// and again whenever it changes, because a tooltip that comes from a binding is not there yet
    /// when the property is first applied.
    /// </para>
    /// </remarks>
    public static class AccessibleName
    {
        // FrameworkElement.ToolTip is only a wrapper: the value actually lives in
        // ToolTipService.ToolTip, and a descriptor registered against the wrapper never fires when
        // the tooltip arrives from a binding or a setter.
        // A headered control is usually built first and given its caption second (an object
        // initialiser, a binding), so the name has to be re-read when the header arrives.
        private static readonly DependencyPropertyDescriptor HeaderDescriptor =
            DependencyPropertyDescriptor.FromProperty(HeaderedContentControl.HeaderProperty, typeof(HeaderedContentControl));

        private static readonly DependencyPropertyDescriptor ToolTipDescriptor =
            DependencyPropertyDescriptor.FromProperty(ToolTipService.ToolTipProperty, typeof(FrameworkElement));

        /// <summary>When <c>true</c>, the tooltip becomes the control's accessible name.</summary>
        public static readonly DependencyProperty FromToolTipProperty = DependencyProperty.RegisterAttached(
            "FromToolTip", typeof(bool), typeof(AccessibleName),
            new PropertyMetadata(false, OnFromToolTipChanged));

        public static void SetFromToolTip(DependencyObject element, bool value) => element.SetValue(FromToolTipProperty, value);

        public static bool GetFromToolTip(DependencyObject element) => (bool)element.GetValue(FromToolTipProperty);

        private static void OnFromToolTipChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not FrameworkElement element) return;

            if ((bool)e.NewValue)
            {
                element.Loaded += OnLoaded;
                ToolTipDescriptor.AddValueChanged(element, OnToolTipChanged);
                HeaderDescriptor.AddValueChanged(element, OnToolTipChanged);

                // Applied immediately as well as on load: a tooltip set straight from the template or
                // the markup is already there, and a control that is never loaded (measured
                // offscreen) still ends up with its name.
                Apply(element);
            }
            else
            {
                element.Loaded -= OnLoaded;
                ToolTipDescriptor.RemoveValueChanged(element, OnToolTipChanged);
                HeaderDescriptor.RemoveValueChanged(element, OnToolTipChanged);
            }
        }

        private static void OnLoaded(object sender, RoutedEventArgs e) => Apply((FrameworkElement)sender);

        private static void OnToolTipChanged(object? sender, EventArgs e)
        {
            if (sender is FrameworkElement element) Apply(element);
        }

        private static void Apply(FrameworkElement element)
        {
            // A host's own name wins: this only fills a gap.
            if (!string.IsNullOrWhiteSpace(AutomationProperties.GetName(element))) return;

            // A button with a caption already has a name — UI Automation derives it from the
            // content, and a tooltip would only make it worse ("Save (Ctrl+S)" for a button that
            // reads "Save").
            if (element is System.Windows.Controls.ContentControl { Content: string caption } &&
                !string.IsNullOrWhiteSpace(caption))
            {
                return;
            }

            // A ribbon command carries a rich RibbonToolTip object rather than a string, so the
            // header is the name it actually shows on screen.
            string? text = ToolTipService.GetToolTip(element) as string;

            if (string.IsNullOrWhiteSpace(text) && element is HeaderedContentControl { Header: string header })
                text = header;

            if (!string.IsNullOrWhiteSpace(text)) AutomationProperties.SetName(element, text);
        }
    }
}
