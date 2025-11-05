using System;
using Godot;

namespace Pandora1337.Atmosphere;

[GlobalClass]
public partial class GasData : Resource
{
    [Export] public GasObject gas;
    [Export] public float moles;

    public bool Contains(GasObject compareGas)
    {
        return gas == compareGas;
    }
}