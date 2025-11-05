using Godot;

namespace Pandora1337.Atmosphere.Interfaces;

public partial class GasForce : RigidBody2D
{
    [Export] bool useForceFormula = false;
    [Export] bool freezeOnSimulationPause = false;
    GasManager gm;

    public override void _Ready()
    {
        // gm = GetNode<GasManager>("../GasManager");
        gm = GasManager.instance;
    }

    public override void _PhysicsProcess(double delta)
    {
        Freeze = false;
        if (freezeOnSimulationPause && !gm.isContinuous)
        {
            Freeze = true;
            return;
        }

        Vector2I pos = gm.GetGridCoord(GlobalPosition);
        if (gm.TryGetGas(pos, out GasMix value))
        {
            Vector2 force;
            if (useForceFormula)
            {
                // F = pAu^2 / RT
                force = value.Pressure * value.Wind.LengthSquared() * value.Wind.Normalized() / (value.Temperature * Gas.R);
                GD.Print("Force " + force);
                GD.Print("Wind " + value.Wind);
            }
            else
            {
                force = value.Wind;
            }

            ApplyForce(force);
        }
    }
}
