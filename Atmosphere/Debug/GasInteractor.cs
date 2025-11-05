using System;
using System.Linq;
using Godot;
using Godot.Collections;

namespace Pandora1337.Atmosphere.Debug;

public partial class GasInteractor : Node
{
    [Export] private GasMixObject[] gases;
    // [Export] GasObject gas;
    [Export] PackedScene body;

    // interaction settings
    enum SpawnType { WALLS, GAS, BODY }
    SpawnType spawnMode = SpawnType.GAS;

    GasManager gasManager;
    private int gasIndex;

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

        if (@event.IsActionPressed("switch_spawn_modes"))
        {
            if (Input.IsActionPressed("shift"))
                SwitchSpawnModes();
                
            if (Input.IsActionPressed("ctrl"))
                SwitchSpawnGas();
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
                gasManager.isContinuous = !gasManager.isContinuous;
                GD.Print("Simulation " + (gasManager.isContinuous ? "Resumed" : "Paused"));
            }
        }
    }

    private void AddCell()
    {
        Vector2 posMouse = GetViewport().GetCamera2D().GetGlobalMousePosition();
        Vector2I posTile = gasManager.GetGridCoord(posMouse);
        switch (spawnMode)
        {
            case SpawnType.BODY:
                Node2D newBody = body.Instantiate() as Node2D;
                newBody.Position = posMouse;
                AddChild(newBody);
                return;

            case (SpawnType.WALLS):
                gasManager._tilemap.SetCellsTerrainConnect([posTile], 0, 0);
                gasManager.AddWallCell(posTile);
                return;

            default:
                gasManager.AddGasMixToCell(posTile, gases[gasIndex].GasMix);
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

    private void SwitchSpawnModes()
    {
        spawnMode += 1;

        int maxEnum = (int)Enum.GetValues(typeof(SpawnType)).Cast<SpawnType>().Max();
        if ((int)spawnMode > maxEnum)
            spawnMode = 0;

        GD.Print($"Spawn mode: {spawnMode}");
    }

    private void SwitchSpawnGas()
    {
        gasIndex += 1;
        if (gasIndex >= gases.Length)
            gasIndex = 0;

        GD.Print($"Spawn gas: {gases[gasIndex].GasMix.Gases[0].gas.nameChem}");
    }
}
