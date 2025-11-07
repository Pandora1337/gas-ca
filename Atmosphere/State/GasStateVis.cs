using Godot;
using Godot.Collections;

namespace Pandora1337.Atmosphere.State;

[GlobalClass]
public partial class GasStateVis : GasState
{
    [Export] public GasVisualiser.GasVisualiserMode filterMode = GasVisualiser.GasVisualiserMode.WIND;

    [ExportCategory("Max Values")]
    [Export] public float maxMoles = 50f;
    [Export] public float maxTempK = 400f;
    [Export] public float maxPressure = 5f;
    [Export] public float maxWind = 5f;
}
