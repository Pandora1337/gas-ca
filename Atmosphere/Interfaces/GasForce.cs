using Godot;
using Pandora1337.Atmosphere.State;

namespace Pandora1337.Atmosphere.Interfaces;

public partial class GasForce : RigidBody2D
{
    [Export] GasStateForce state;

    bool useForceFormula;
    bool freezeOnSimulationPause;

    GasManager gm;

    public override void _Ready()
    {
        // gm = GetNode<GasManager>("../GasManager");
        gm = GasManager.instance;

        // This method is special, as it allows the engine
        // to automatically disconnect signals from a freed node
        state.Connect(GasStateForce.SignalName.Changed, Callable.From(UpdateProperties));
        state.Connect(GasStateForce.SignalName.DeleteBodies, Callable.From(QueueFree));
        UpdateProperties();
    }

    public override void _PhysicsProcess(double delta)
    {
        Freeze = false;
        if (freezeOnSimulationPause && !gm.state.isContinuous)
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

    void UpdateProperties()
    {
        Mass = state.mass;
        useForceFormula = state.useForceFormula;
        freezeOnSimulationPause = state.freezeOnSimulationPause;
    }
}
