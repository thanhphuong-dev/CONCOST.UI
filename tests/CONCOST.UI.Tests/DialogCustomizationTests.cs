using System.Linq;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The static confirm/input helpers used to hard-code "Cancel", "Confirm" and "OK". Both now take
/// the labels, so a host can localise them without a custom dialog.
/// </summary>
public class DialogCustomizationTests
{
    [Fact]
    public void Confirm_and_input_helpers_accept_button_labels()
    {
        var confirm = typeof(Dialog).GetMethod("ShowConfirmAsync")!;
        Assert.Contains(confirm.GetParameters(), p => p.Name == "confirmText" && p.HasDefaultValue);
        Assert.Contains(confirm.GetParameters(), p => p.Name == "cancelText" && p.HasDefaultValue);

        var input = typeof(Dialog).GetMethod("ShowInputAsync")!;
        Assert.Contains(input.GetParameters(), p => p.Name == "okText" && p.HasDefaultValue);
        Assert.Contains(input.GetParameters(), p => p.Name == "cancelText" && p.HasDefaultValue);
    }
}
