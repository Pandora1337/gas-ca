using Godot;
using Godot.Collections;
using Pandora1337.Atmosphere.State;

namespace Pandora1337.Atmosphere;

public partial class GasVisualiser : Node
{
    public enum GasVisualiserMode
    {
        NORM, // Normal gas render
        HIDE, // Hide all gas render
        WALL, // Shows all walls
        MOLE, // False-color based on mol concentration
        TEMP, // False-color based on temperature
        PRES, // False-color based on pressure
        WIND, // False-color based on wind force
        EPIL, // Gives epilepsy
    }

    [Export] float updateTime = 0.0001f;

    [Export] PackedScene gasTile;
    [Export] GasStateVis state;

    GasManager gm;
    public Dictionary<Vector2I, Node2D> gasTiles = [];

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        gm = GetParent<GasManager>();
    }

    float timePassed = 0f;
    public override void _Process(double delta)
    {
        timePassed += (float)delta;
        if (timePassed < updateTime)
            return;

        timePassed = 0;

        GasVisualiserMode mode = state.filterMode;
        if (mode == GasVisualiserMode.HIDE)
        {
            RemoveAllTiles();
            return;
        }

        if (mode == GasVisualiserMode.WALL)
        {
            foreach (Vector2I pos in gm.GetWalls())
            {
                if (gasTiles.ContainsKey(pos))
                    continue;

                CreateTile(pos);
                gasTiles[pos].Modulate = Colors.Red;
            }
            return;
        }

        foreach ((Vector2I aPos, GasMix a) in gm.gasDict)
        {
            if (!gasTiles.ContainsKey(aPos))
                CreateTile(aPos);

            UpdateTile(gasTiles[aPos], a);
        }
    }

    void UpdateTile(Node2D tile, GasMix a)
    {
        switch (state.filterMode)
        {
            case GasVisualiserMode.MOLE:
                tile.Modulate = GetCellColor(a.Moles, state.maxMoles);
                return;

            case GasVisualiserMode.TEMP:
                tile.Modulate = GetCellColor(a.Temperature, state.maxTempK);
                return;

            case GasVisualiserMode.PRES:
                tile.Modulate = GetCellColor(a.Pressure, state.maxPressure);
                return;

            case GasVisualiserMode.WIND:
                tile.Modulate = GetCellColor(a.Wind.Length(), state.maxWind);

                var line = tile.GetNode<Line2D>("Line2D");
                if (Mathf.Abs(a.Wind.X) < 0.0001 && Mathf.Abs(a.Wind.Y) < 0.0001)
                    line.SetPointPosition(
                        1, Godot.Vector2.Zero
                    );
                else
                    line.SetPointPosition(
                        1, a.Wind.Length() <= 28f ? a.Wind : a.Wind.Normalized() * 28f
                    );
                return;

            case GasVisualiserMode.EPIL:
                tile.Modulate = GetEpilColor(a);
                return;

            default:
                // Natural gas color.
                tile.Modulate = a.GetGasColor();
                return;
        }
    }

    public void CreateTile(Vector2I pos)
    {
        Node2D newCell = gasTile.Instantiate() as Node2D;

        newCell.Name = $"Gas {pos}";
        newCell.Position = gm._tilemap.MapToLocal(pos);

        this.AddChild(newCell);
        gasTiles.Add(pos, newCell);
    }

    public void RemoveTile(Vector2I pos)
    {
        if (!gasTiles.TryGetValue(pos, out Node2D tile))
            return;

        tile.QueueFree();
        gasTiles.Remove(pos);
    }

    public void RemoveAllTiles()
    {
        foreach (var i in gasTiles)
            RemoveTile(i.Key);
    }

    static Color GetCellColor(float value, float maxvalue)
    {
        float min = Mathf.Min(value, maxvalue);

        // Yes this is a magic number
        // Blue is 240 out of 359 on the HUE slider
        // so 240 / 359 = 0.668523677
        float mult = Mathf.Remap(min, 0, maxvalue, 0.668523677f, 0);
        return Color.FromHsv(mult, 1, 1, 1);
    }

    Color GetEpilColor(GasMix gas)
    {
        float mult = Mathf.Remap(gas.Pressure, 0, state.maxPressure, 0, 240);
        Color col = Color.FromHsv(mult, 1, 1, 1);
        return col;
    }
}
