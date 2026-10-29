using System.Windows;
using System.Windows.Controls;

namespace CONCOST.UI.Controls.Engineering;

public class LengthBox : UnitBox
{
    static LengthBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(LengthBox), new FrameworkPropertyMetadata(typeof(UnitBox)));
    }

    public LengthBox()
    {
        Unit = "mm";
        DecimalPlaces = 0;
    }
}

public class AreaBox : UnitBox
{
    static AreaBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(AreaBox), new FrameworkPropertyMetadata(typeof(UnitBox)));
    }

    public AreaBox()
    {
        Unit = "m²";
        DecimalPlaces = 2;
    }
}

public class VolumeBox : UnitBox
{
    static VolumeBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(VolumeBox), new FrameworkPropertyMetadata(typeof(UnitBox)));
    }

    public VolumeBox()
    {
        Unit = "m³";
        DecimalPlaces = 2;
    }
}

public class ForceBox : UnitBox
{
    static ForceBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ForceBox), new FrameworkPropertyMetadata(typeof(UnitBox)));
    }

    public ForceBox()
    {
        Unit = "kN";
        DecimalPlaces = 1;
    }
}

public class MomentBox : UnitBox
{
    static MomentBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(MomentBox), new FrameworkPropertyMetadata(typeof(UnitBox)));
    }

    public MomentBox()
    {
        Unit = "kN.m";
        DecimalPlaces = 2;
    }
}

public class PressureBox : UnitBox
{
    static PressureBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(PressureBox), new FrameworkPropertyMetadata(typeof(UnitBox)));
    }

    public PressureBox()
    {
        Unit = "MPa";
        DecimalPlaces = 2;
    }
}
