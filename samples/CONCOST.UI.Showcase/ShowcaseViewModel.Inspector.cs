using System.Collections.ObjectModel;
using System.Linq;
using CONCOST.UI.Services;

namespace CONCOST.UI.Showcase;

/// <summary>
/// Bottom dock of the showcase: the variants the library ships for the selected control and the
/// extended (own + attached) properties it exposes.
/// </summary>
public partial class ShowcaseViewModel
{
    private readonly ObservableCollection<ShowcaseVariantInfo> _inspectorVariants = new();
    private readonly ObservableCollection<ShowcasePropertyInfo> _inspectorProperties = new();
    private string _inspectorTypeLabel = string.Empty;

    public ObservableCollection<ShowcaseVariantInfo> InspectorVariants => _inspectorVariants;
    public ObservableCollection<ShowcasePropertyInfo> InspectorProperties => _inspectorProperties;

    public string InspectorTypeLabel
    {
        get => _inspectorTypeLabel;
        private set
        {
            if (SetProperty(ref _inspectorTypeLabel, value))
                OnPropertyChanged(nameof(HasInspectorType));
        }
    }

    public bool HasInspectorType => _inspectorTypeLabel.Length > 0;

    /// <summary>False for guide pages and effect entries, which are not bound to a control.</summary>
    public bool HasInspectorData { get; private set; }

    public bool HasInspectorVariants => _inspectorVariants.Count > 0;
    public bool HasInspectorProperties => _inspectorProperties.Count > 0;

    private void RefreshInspector(ShowcaseItem? item)
    {
        var description = item is null ? null : ShowcaseInspector.Describe(item);

        _inspectorVariants.Clear();
        _inspectorProperties.Clear();

        if (description is not null)
        {
            foreach (var variant in description.Variants)
                _inspectorVariants.Add(variant);

            foreach (var property in description.Properties)
                _inspectorProperties.Add(property);
        }

        InspectorTypeLabel = description?.TypeLabel ?? string.Empty;
        HasInspectorData = description?.ControlType is not null;

        OnPropertyChanged(nameof(HasInspectorData));
        OnPropertyChanged(nameof(HasInspectorVariants));
        OnPropertyChanged(nameof(HasInspectorProperties));
    }
}
