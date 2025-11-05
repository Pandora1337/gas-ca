using Godot;
using Godot.Collections;

namespace Pandora1337.Atmosphere;

[GlobalClass]
partial class GasMixObject : Resource
{
    [Export]
    public float temp = 20f;

    [Export] public Array<GasData> gasDatas;
    public GasMix GasMix
    {
        get
        {
            GasMix newGas = new();
            foreach (GasData gas in gasDatas)
            {
                newGas.ChangeMoles(gas.gas, gas.moles);
            }

            newGas.SetTemperature(temp);
            return newGas;
        }
    }
}