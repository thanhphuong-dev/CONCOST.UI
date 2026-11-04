using System;
using System.Windows.Data;
using System.Windows.Markup;
using CONCOST.UI.Services;

namespace CONCOST.UI.Showcase;

/// <summary>
/// XAML sugar for <see cref="LocalizationManager"/> lookups: <c>{loc:Loc Showcase.Header.DarkMode}</c>.
/// Returns a binding on the manager's string indexer rather than a literal, so the text follows the
/// language selector without rebuilding the shell.
/// </summary>
[MarkupExtensionReturnType(typeof(string))]
internal sealed class LocExtension : MarkupExtension
{
    public LocExtension()
    {
    }

    public LocExtension(string key) => Key = key;

    /// <summary>Resource key, for example <c>Showcase.Nav.Overview</c>.</summary>
    public string? Key { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var binding = new Binding($"[{Key}]")
        {
            Source = LocalizationManager.Current,
            Mode = BindingMode.OneWay
        };
        return binding.ProvideValue(serviceProvider);
    }
}
