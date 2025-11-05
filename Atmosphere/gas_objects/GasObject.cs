using Godot;

namespace Pandora1337.Atmosphere;

[GlobalClass]
public partial class GasObject : Resource
{
    [Export] public string nameGas = "New Gas";
    [Export] public string nameChem;

    //This is the mass, in grams, of 1 mole of the gas
    // [Export] public float molarMass = 1;

    //Color stuff
    [ExportCategory("Color")]
    [Export] public bool customColor;
    [Export] public Color color = Colors.White;
    [Export] public float minMolesToSee = 0.4f;
}
