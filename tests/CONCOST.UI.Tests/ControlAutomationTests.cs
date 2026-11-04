using System;
using System.Windows;
using System.Windows.Automation.Peers;
using CONCOST.UI.Controls;
using CONCOST.UI.Controls.Canvas;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// Self-painted controls used to expose no automation peer at all, so a screen reader saw a blank
/// rectangle. Each of these now returns a peer with a name.
/// </summary>
public class ControlAutomationTests
{
    [Fact]
    public void Self_painted_controls_expose_a_named_peer()
    {
        WpfApplicationHost.Run(() =>
        {
            var cases = new (string Name, Func<FrameworkElement> Make)[]
            {
                ("ModelViewer3D", () => new ModelViewer3D()),
                ("ImageViewer", () => new ImageViewer()),
                ("Magnifier", () => new Magnifier()),
                ("SignaturePad", () => new SignaturePad()),
                ("RulerControl", () => new RulerControl()),
                ("DockingManager", () => new DockingManager()),
            };

            foreach (var (name, make) in cases)
            {
                FrameworkElement element = make();
                AutomationPeer? peer = UIElementAutomationPeer.CreatePeerForElement(element);

                Assert.True(peer is not null, $"{name} returned no automation peer.");
                Assert.False(string.IsNullOrWhiteSpace(peer!.GetName()), $"{name} has no accessible name.");
            }
        });
    }
}
