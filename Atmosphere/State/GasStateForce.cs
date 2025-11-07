using Godot;
using System;

namespace Pandora1337.Atmosphere.State;

[GlobalClass]
public partial class GasStateForce : GasState
{
    [Signal]
    // Custom signal name HAS to end with 'EventHandler'
    public delegate void DeleteBodiesEventHandler();

    [Export] public bool useForceFormula = false;
    [Export] public bool freezeOnSimulationPause = true;
    [Export] public float mass = 0.1f;
}
