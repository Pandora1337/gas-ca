using Godot;
using System.Collections.Generic;

namespace Pandora1337.Atmosphere.Debug;

public partial class GasTooltip : Control
{
    GasManager gasManager;
    Camera2D cam;
    Label label;

    public override void _Ready()
    {
        base._Ready();

        cam = GetViewport().GetCamera2D();
        gasManager = GetParent<GasManager>();
        label = GetNode<Label>("Label");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        Vector2 posMouse = cam.GetGlobalMousePosition();
        Vector2I gridPos = gasManager.GetGridCoord(posMouse);
        if (!gasManager.gasDict.TryGetValue(gridPos, out GasMix gasMix))
        {
            Hide();
            return;
        }

        Show();
        UpdateTooltip(gasMix, gridPos);

        float zoomRecip = 1f / cam.Zoom.X;
        Position = posMouse + new Vector2(30, 0) * zoomRecip;
        Scale = new(zoomRecip, zoomRecip);
    }

    public void UpdateTooltip(GasMix gasMix, Vector2 pos)
    {
        float totalMoles = 0;
        List<string> gasesArr = [];
        foreach (GasData gas in gasMix.Gases)
        {
            gasesArr.Add($"- {gas.gas.nameChem}: {gas.moles} moles");
            totalMoles += gas.moles;
        }

        gasesArr.Sort();
        string gases = string.Join("\n", gasesArr);

        label.Text = @$"Position: {pos}
			Pressure: {gasMix.Pressure}
			Temperature: {gasMix.Temperature}K ({Gas.KToCelsius(gasMix.Temperature)}C)
			Wind: {gasMix.Wind}
			Wind Power: {gasMix.Wind.Length()}
			Total Moles: {totalMoles}
			Gases: 
			{gases}";
    }
}
