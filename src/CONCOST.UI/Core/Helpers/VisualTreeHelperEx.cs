using System.Windows;
using System.Windows.Media;

namespace CONCOST.UI.Core.Helpers;

public static class VisualTreeHelperEx
{
    public static T? FindParent<T>(DependencyObject? child) where T : DependencyObject
    {
        while (child != null)
        {
            if (child is T parent)
                return parent;

            child = VisualTreeHelper.GetParent(child);
        }
        return null;
    }

    public static T? FindChild<T>(DependencyObject? parent) where T : DependencyObject
    {
        if (parent == null) return null;

        int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T typedChild)
                return typedChild;

            var descendant = FindChild<T>(child);
            if (descendant != null)
                return descendant;
        }
        return null;
    }
}
