using Godot;
using System;
using Pandora1337.Atmosphere.Debug;

namespace Pandora1337.Atmosphere.State;

[GlobalClass]
public partial class GasStateInt : GasState
{
    public int gasIndex;
    // Data type 28
    [Export] public GasObject[] gases;
    [Export] public float temp = 270;
    [Export] public float moles = 10;
    [Export] public GasInteractor.SpawnType spawnMode = GasInteractor.SpawnType.GAS;

    public GasMix GetGasMix()
    {
        GasData data = new()
        {
            gas = gases[gasIndex],
            moles = moles,
        };

        return new GasMix()
        {
            Temperature = temp,
            Gases = [data],
        };
    }
}
