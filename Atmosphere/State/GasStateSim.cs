using Godot;
using System;

namespace Pandora1337.Atmosphere.State;

[GlobalClass]
public partial class GasStateSim : GasState
{
    [Export] public bool isRunning = true;
    public bool isContinuous
    {
        get => isRunning;
        set => isRunning = value;
    }

    /// <summary>
    /// Wind will only flow to lower pressure cells
    /// </summary>
    [Export] public bool windFlowToGasCellsOnly = false;

    [Export] public GasManager.NeighborNumber neighborNumber;

    [ExportCategory("Diffusion")]
    [Export] public bool useSimpleDiffusion = false;
    [Export] public float diffusionRate = 0.1f;

    /// <summary> values below will delete gas cell</summary>
    [Export] public float minConcentration = 0.001f;

    [ExportCategory("Coefs")]
    [Export] public float thermalConductivity = 0.02f;
    [Export] public float windForceCoef = 1f;
}
