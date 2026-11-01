using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The chrome of the drawing surface: the datum axes, the grid brushes, the pick box, the snap
/// label. Each layer is verified by painting the control and reading the pixels back, because the
/// whole point of the property pack is which colour lands where.
/// </summary>
/// <remarks>
/// A detached element paints once, during its first layout pass; an invalidate after that is never
/// walked because there is no live tree behind it. Every test therefore configures the viewport
/// before it is measured, and a test that needs two pictures builds two viewports.
/// </remarks>
public class ViewportChromeTests
{
    private const int Width = 200;
    private const int Height = 200;

    /// <summary>Where the default plan camera puts the model origin on a 200x200 surface.</summary>
    private const double Centre = Width / 2.0;

    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    /// <summary>
    /// A square surface with a plain white background, so any painted pixel stands out. <paramref
    /// name="configure"/> runs before the first layout pass, which is the only moment a detached
    /// control picks a property change up.
    /// </summary>
    private static ViewportControl Surface(Action<ViewportControl>? configure = null)
    {
        var viewport = new ViewportControl
        {
            Width = Width,
            Height = Height,
            Background = Brushes.White,
            ShowGrid = false,
            ShowAxes = false,
            ShowCrosshair = false,
            ShowOriginMarker = false,
            SnapToGrid = false
        };

        configure?.Invoke(viewport);

        viewport.Measure(new Size(Width, Height));
        viewport.Arrange(new Rect(0, 0, Width, Height));
        viewport.UpdateLayout();

        return viewport;
    }

    private static byte[] Render(ViewportControl viewport)
    {
        var bitmap = new RenderTargetBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(viewport);

        var pixels = new byte[Width * Height * 4];
        bitmap.CopyPixels(pixels, Width * 4, 0);

        return pixels;
    }

    private static Color PixelAt(byte[] pixels, int x, int y)
    {
        int index = (y * Width + x) * 4;
        return Color.FromArgb(pixels[index + 3], pixels[index + 2], pixels[index + 1], pixels[index]);
    }

    /// <summary>True when any pixel in the 3x3 neighbourhood of (x, y) satisfies the test.</summary>
    private static bool AnyNear(byte[] pixels, double x, double y, Func<Color, bool> predicate)
    {
        int cx = (int)Math.Round(x);
        int cy = (int)Math.Round(y);

        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                int px = cx + dx;
                int py = cy + dy;

                if (px < 0 || py < 0 || px >= Width || py >= Height) continue;
                if (predicate(PixelAt(pixels, px, py))) return true;
            }
        }

        return false;
    }

    /// <summary>How many pixels of one row differ from the white backdrop.</summary>
    private static int InkedInRow(byte[] pixels, int y)
    {
        int count = 0;

        for (int x = 0; x < Width; x++)
        {
            if (!IsWhite(PixelAt(pixels, x, y))) count++;
        }

        return count;
    }

    private static int InkedInBox(byte[] pixels, int left, int top, int right, int bottom)
    {
        int count = 0;

        for (int y = top; y < bottom; y++)
        {
            for (int x = left; x < right; x++)
            {
                if (!IsWhite(PixelAt(pixels, x, y))) count++;
            }
        }

        return count;
    }

    private static int CountRed(byte[] pixels)
    {
        int count = 0;

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (IsRed(PixelAt(pixels, x, y))) count++;
            }
        }

        return count;
    }

    // A one-pixel stroke at an integer coordinate is anti-aliased across two columns at 50%, so the
    // threshold sits well below full saturation and well above the grey of the theme's grid tokens.
    private const int Tint = 45;

    private static bool IsRed(Color color) => color.R > color.G + Tint && color.R > color.B + Tint;

    private static bool IsBlue(Color color) => color.B > color.R + Tint && color.B > color.G + Tint;

    private static bool IsGreen(Color color) => color.G > color.R + Tint && color.G > color.B + Tint;

    private static bool IsWhite(Color color) => color.R > 250 && color.G > 250 && color.B > 250;

    // ---------------------------------------------------------------- defaults

    [Fact]
    public void Chrome_defaults_are_the_quiet_ones()
    {
        RunOnSTA(() =>
        {
            var viewport = new ViewportControl();

            // The datum is on by default, but it is drawn from the theme's datum token and dashed,
            // which is what keeps it from reading as a warning line across the middle of a plan.
            Assert.True(viewport.ShowAxes);
            Assert.True(viewport.ShowOriginMarker);
            Assert.Null(viewport.AxisBrush);
            Assert.Null(viewport.AxisDashStyle);
            Assert.Equal(1, viewport.AxisThickness);

            Assert.Null(viewport.GridBrush);
            Assert.Null(viewport.GridMajorBrush);
            Assert.Equal(5, viewport.GridMajorEvery);
            Assert.Equal(8, viewport.MinGridPixelSpacing);

            Assert.Null(viewport.CrosshairBrush);
            Assert.True(viewport.ShowAperture);
            Assert.Null(viewport.SnapMarkerBrush);
            Assert.Equal(5, viewport.SnapMarkerSize);
            Assert.True(viewport.ShowSnapLabel);

            Assert.Null(viewport.SelectionBrush);
            Assert.Equal(0.12, viewport.SelectionFillOpacity);
            Assert.Null(viewport.DimensionBrush);
            Assert.Equal(1.1, viewport.FitMargin);
        });
    }

    [Fact]
    public void A_brush_override_is_handed_back_verbatim()
    {
        RunOnSTA(() =>
        {
            var viewport = new ViewportControl();

            viewport.AxisBrush = Brushes.Red;
            viewport.GridBrush = Brushes.Blue;
            viewport.GridMajorBrush = Brushes.Green;
            viewport.CrosshairBrush = Brushes.Orange;
            viewport.SnapMarkerBrush = Brushes.Purple;
            viewport.SelectionBrush = Brushes.Teal;
            viewport.DimensionBrush = Brushes.Navy;

            Assert.Same(Brushes.Red, viewport.AxisBrush);
            Assert.Same(Brushes.Blue, viewport.GridBrush);
            Assert.Same(Brushes.Green, viewport.GridMajorBrush);
            Assert.Same(Brushes.Orange, viewport.CrosshairBrush);
            Assert.Same(Brushes.Purple, viewport.SnapMarkerBrush);
            Assert.Same(Brushes.Teal, viewport.SelectionBrush);
            Assert.Same(Brushes.Navy, viewport.DimensionBrush);
        });
    }

    // ---------------------------------------------------------------- datum

    [Fact]
    public void The_datum_crosses_the_projected_origin()
    {
        RunOnSTA(() =>
        {
            var viewport = Surface(v =>
            {
                v.ShowAxes = true;
                v.AxisBrush = Brushes.Red;
                v.AxisDashStyle = DashStyles.Solid;
            });

            var origin = viewport.ToScreen(new Point3D(0, 0, 0));
            Assert.Equal(Centre, origin.X, 3);
            Assert.Equal(Centre, origin.Y, 3);

            var pixels = Render(viewport);

            // Both arms of the cross run through the origin, far from it in each direction.
            Assert.True(AnyNear(pixels, origin.X, 20, IsRed), "the vertical axis is missing");
            Assert.True(AnyNear(pixels, 20, origin.Y, IsRed), "the horizontal axis is missing");

            // …and nowhere near the opposite edge of the origin, so it is a cross and not a border.
            Assert.False(AnyNear(pixels, origin.X + 60, 20, IsRed));
        });
    }

    [Fact]
    public void Switching_the_datum_off_leaves_the_grid_alone()
    {
        RunOnSTA(() =>
        {
            var viewport = Surface(v =>
            {
                v.ShowGrid = true;
                v.GridStep = 2000;
                v.GridBrush = Brushes.Blue;
                v.GridMajorBrush = Brushes.Blue;
                v.ShowAxes = false;
                v.AxisBrush = Brushes.Red;
            });

            var origin = viewport.ToScreen(new Point3D(0, 0, 0));
            var pixels = Render(viewport);

            Assert.True(AnyNear(pixels, 20, 25, IsBlue), "the grid should still be painted");
            Assert.False(AnyNear(pixels, origin.X, 40, IsRed), "the datum should be gone");
        });
    }

    [Fact]
    public void The_origin_marker_survives_on_its_own()
    {
        RunOnSTA(() =>
        {
            // The ring is not a child of the axes: a host that wants a datum on a blank sheet turns
            // the lines off and keeps the marker.
            var ringed = Surface(v =>
            {
                v.ShowAxes = false;
                v.ShowOriginMarker = true;
                v.AxisBrush = Brushes.Red;
            });

            var origin = ringed.ToScreen(new Point3D(0, 0, 0));

            // The ring is drawn 3.5 px out from the origin, so the centre itself stays blank — that
            // is what makes it a marker and not a dot.
            var withMarker = Render(ringed);
            Assert.True(AnyNear(withMarker, origin.X + 4, origin.Y, IsRed) ||
                        AnyNear(withMarker, origin.X, origin.Y + 4, IsRed));

            var bare = Surface(v =>
            {
                v.ShowAxes = false;
                v.ShowOriginMarker = false;
                v.AxisBrush = Brushes.Red;
            });

            var withoutMarker = Render(bare);
            Assert.False(AnyNear(withoutMarker, origin.X + 4, origin.Y, IsRed));
            Assert.False(AnyNear(withoutMarker, origin.X, origin.Y + 4, IsRed));
        });
    }

    [Fact]
    public void Everything_off_is_a_blank_surface()
    {
        RunOnSTA(() =>
        {
            var pixels = Render(Surface());

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Assert.True(IsWhite(PixelAt(pixels, x, y)),
                        $"nothing should be painted, but ({x}, {y}) is {PixelAt(pixels, x, y)}");
                }
            }
        });
    }

    // ---------------------------------------------------------------- grid

    [Fact]
    public void The_major_brush_lands_on_every_nth_line()
    {
        RunOnSTA(() =>
        {
            const double step = 1000;

            var viewport = Surface(v =>
            {
                v.ShowGrid = true;
                v.GridStep = step;
                v.GridMajorEvery = 5;
                v.GridBrush = Brushes.Blue;
                v.GridMajorBrush = Brushes.Green;
            });

            var origin = viewport.ToScreen(new Point3D(0, 0, 0));
            double scale = viewport.UnitsToPixels(1);

            var pixels = Render(viewport);

            // 1000 units to the left of the origin is a minor line, 5000 is a major one. The row is
            // sampled between two horizontal lines so only the verticals can answer.
            Assert.True(AnyNear(pixels, origin.X - step * scale, 25, IsBlue));
            Assert.True(AnyNear(pixels, origin.X - step * 5 * scale, 25, IsGreen));
            Assert.False(AnyNear(pixels, origin.X - step * 5 * scale, 25, IsBlue));
        });
    }

    [Fact]
    public void A_denser_step_than_the_pixel_floor_is_doubled_until_it_reads()
    {
        RunOnSTA(() =>
        {
            // 10 world units at this zoom is a tenth of a pixel, so the step has to double several
            // times before any line is legible. A floor of 2 px lets far more lines through than a
            // floor of 8, which is the whole point of the ladder.
            var fine = Surface(v =>
            {
                v.ShowGrid = true;
                v.GridStep = 10;
                v.MinGridPixelSpacing = 2;
            });

            var coarse = Surface(v =>
            {
                v.ShowGrid = true;
                v.GridStep = 10;
                v.MinGridPixelSpacing = 8;
            });

            Assert.True(10 * fine.UnitsToPixels(1) < 2);

            int fineLines = InkedInRow(Render(fine), 105);
            int coarseLines = InkedInRow(Render(coarse), 105);

            Assert.True(coarseLines > 0, "the ladder should stop before the grid disappears");
            Assert.True(coarseLines < fineLines, $"the floor should thin the grid ({fineLines} -> {coarseLines})");
        });
    }

    // ---------------------------------------------------------------- pick box

    [Fact]
    public void A_zero_fill_opacity_draws_the_rubber_band_as_an_outline()
    {
        RunOnSTA(() =>
        {
            var viewport = Surface(v =>
            {
                v.SelectionBrush = Brushes.Red;
                v.SelectionFillOpacity = 0;
                v.SelectionRectangle = new Rect(60, 60, 80, 80);
            });

            var pixels = Render(viewport);

            // Outline present — read along the top edge, because the pen is dashed and a single
            // sample can land in a gap.
            Assert.True(InkedInRow(pixels, 60) > 0, "the outline should be painted");

            // …interior untouched, which is the whole point of a pick box over geometry.
            Assert.True(IsWhite(PixelAt(pixels, 100, 100)));
        });
    }

    [Fact]
    public void The_rubber_band_fills_when_the_opacity_asks_for_it()
    {
        RunOnSTA(() =>
        {
            var viewport = Surface(v =>
            {
                v.SelectionBrush = Brushes.Red;
                v.SelectionFillOpacity = 0.6;
                v.SelectionRectangle = new Rect(60, 60, 80, 80);
            });

            var inside = PixelAt(Render(viewport), 100, 100);

            Assert.False(IsWhite(inside));
            Assert.True(inside.R > inside.B, $"the fill should be red, but is {inside}");
        });
    }

    // ---------------------------------------------------------------- snap label

    [Fact]
    public void The_snap_label_can_be_switched_off_without_losing_the_marker()
    {
        RunOnSTA(() =>
        {
            var labelled = Surface(v =>
            {
                v.SnapKind = SnapKind.Grid;
                v.Snap = new Point(Centre, Centre);
                v.SnapMarkerBrush = Brushes.Red;
            });

            // The glyph sits on the snap point, the caption nine pixels to its right.
            var withCaption = Render(labelled);
            Assert.True(AnyNear(withCaption, Centre, Centre, IsRed));
            Assert.True(InkedInBox(withCaption, 112, 104, 160, 120) > 0, "the caption should be there");

            var bare = Surface(v =>
            {
                v.SnapKind = SnapKind.Grid;
                v.Snap = new Point(Centre, Centre);
                v.SnapMarkerBrush = Brushes.Red;
                v.ShowSnapLabel = false;
            });

            var withoutCaption = Render(bare);
            Assert.True(AnyNear(withoutCaption, Centre, Centre, IsRed));
            Assert.Equal(0, InkedInBox(withoutCaption, 112, 104, 160, 120));
        });
    }

    [Fact]
    public void The_snap_marker_grows_with_its_size()
    {
        RunOnSTA(() =>
        {
            var small = Surface(v =>
            {
                v.SnapKind = SnapKind.Endpoint;
                v.Snap = new Point(Centre, Centre);
                v.SnapMarkerBrush = Brushes.Red;
                v.ShowSnapLabel = false;
                v.SnapMarkerSize = 3;
            });

            var large = Surface(v =>
            {
                v.SnapKind = SnapKind.Endpoint;
                v.Snap = new Point(Centre, Centre);
                v.SnapMarkerBrush = Brushes.Red;
                v.ShowSnapLabel = false;
                v.SnapMarkerSize = 10;
            });

            int smallInk = CountRed(Render(small));
            int largeInk = CountRed(Render(large));

            Assert.True(smallInk > 0, "the marker should be painted at all");
            Assert.True(largeInk > smallInk, $"a bigger marker should ink more pixels ({smallInk} -> {largeInk})");
        });
    }

    // ---------------------------------------------------------------- dimension

    [Fact]
    public void The_temporary_dimension_takes_its_own_brush()
    {
        RunOnSTA(() =>
        {
            var viewport = Surface(v =>
            {
                v.DimensionBrush = Brushes.Red;
                v.TemporaryDimension = new TemporaryDimension
                {
                    Start = new Point3D(0, 0, 0),
                    End = new Point3D(4000, 0, 0),
                    Text = "4000"
                };
            });

            var pixels = Render(viewport);

            // The witness line is pushed 28 px off a run that projects along the origin's row, so the
            // dimension line itself sits at y = centre + 28 and spans 4000 world units to the right.
            Assert.True(AnyNear(pixels, Centre + 5, Centre + 28, IsRed), "the dimension line is missing");
            Assert.True(AnyNear(pixels, Centre, Centre + 14, IsRed), "the witness line is missing");
        });
    }

    // ---------------------------------------------------------------- fit margin

    [Fact]
    public void Fit_margin_leaves_more_air_around_the_bounds()
    {
        RunOnSTA(() =>
        {
            var bounds = new Rect3D(0, 0, 0, 4000, 4000, 0);

            var tight = Surface(v => v.FitMargin = 1);
            tight.ZoomToFit(bounds);

            var airy = Surface(v => v.FitMargin = 2);
            airy.ZoomToFit(bounds);

            Assert.True(airy.Camera.OrthographicHeight > tight.Camera.OrthographicHeight);

            // A margin under one would crop the drawing, so it is clamped to flush.
            var cropped = Surface(v => v.FitMargin = 0.5);
            cropped.ZoomToFit(bounds);

            Assert.Equal(tight.Camera.OrthographicHeight, cropped.Camera.OrthographicHeight, 6);
        });
    }
}
