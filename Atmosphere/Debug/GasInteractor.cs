using System;
using System.Linq;
using Godot;
using Pandora1337.Atmosphere.State;

namespace Pandora1337.Atmosphere.Debug;

public partial class GasInteractor : Node
{
    public enum SpawnType { WALLS, GAS, BODY }

    [Export] PackedScene body;
    [Export] GasStateInt state;
    [Export] GasStateSim stateSim;

    GasManager gasManager;

    public override void _Ready()
    {
        base._Ready();
        gasManager = GetParent<GasManager>();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("spawn_tile"))
            AddCell();

        if (@event.IsActionPressed("delete_tile"))
            RemoveCell();

        if (Input.IsActionPressed("shift"))
        {
            if (@event.IsActionPressed("scroll_up"))
                SwitchSpawnModes(-1);
            if (@event.IsActionPressed("scroll_down"))
                SwitchSpawnModes(1);
        }

        if (Input.IsActionPressed("ctrl"))
        {
            if (@event.IsActionPressed("scroll_up"))
                SwitchSpawnGas(-1);
            if (@event.IsActionPressed("scroll_down"))
                SwitchSpawnGas(1);
        }

        // Step forward
        if (@event.IsActionPressed("sim_step"))
        {
            if (Input.IsActionPressed("shift"))
            {
                gasManager.Step();
            }
            else
            {
                stateSim.isContinuous = !stateSim.isContinuous;
                stateSim.EmitChanged();
                GD.Print("Simulation " + (stateSim.isContinuous ? "Resumed" : "Paused"));
            }
        }
    }

    private void AddCell()
    {
        Vector2 posMouse = GetViewport().GetCamera2D().GetGlobalMousePosition();
        Vector2I posTile = gasManager.GetGridCoord(posMouse);
        switch (state.spawnMode)
        {
            case SpawnType.BODY:
                Node2D newBody = body.Instantiate() as Node2D;
                newBody.Position = posMouse;
                AddChild(newBody);
                return;

            case SpawnType.WALLS:
                gasManager._tilemap.SetCellsTerrainConnect([posTile], 0, 0);
                gasManager.AddWallCell(posTile);
                return;

            default:
                gasManager.AddGasMixToCell(posTile, state.GetGasMix());
                return;
        }
    }

    private void RemoveCell()
    {
        Vector2 posMouse = GetViewport().GetCamera2D().GetGlobalMousePosition();
        Vector2I posTile = gasManager.GetGridCoord(posMouse);
        gasManager._tilemap.EraseCell(posTile);
        gasManager._tilemap.SetCellsTerrainConnect([posTile], 0, -1);
        gasManager.RemoveWallCell(posTile);
        gasManager.RemoveGasCell(posTile);

        // TODO remove this abomination
        gasManager.GetNode<GasVisualiser>("GasVisualiser").RemoveTile(posTile);
    }

    private void SwitchSpawnModes(int incr)
    {
        // int maxEnum = (int)Enum.GetValues(typeof(SpawnType)).Cast<SpawnType>().Max();
        int maxEnum = (Enum.GetValues(typeof(SpawnType)) as int[]).Max();
        state.spawnMode = (SpawnType)IncrementEnum((int)state.spawnMode, incr, maxEnum);
        state.EmitChanged();
        GD.Print($"Spawn mode: {state.spawnMode}");
    }

    private void SwitchSpawnGas(int incr)
    {
        int gasIndex = state.gasIndex;
        state.gasIndex = IncrementEnum(gasIndex, incr, state.gases.Length - 1);
        state.EmitChanged();
        GD.Print($"Spawn gas: {state.gases[gasIndex].nameChem}");
    }

    private int IncrementEnum(int currentVal, int increment, int maxVal)
    {
        currentVal += increment;
        if (currentVal > maxVal)
            currentVal = 0;

        if (currentVal < 0)
            currentVal = maxVal;

        return currentVal;
    }
}
