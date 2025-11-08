using Godot;
using System;
using Pandora1337.Atmosphere.State;
using Pandora1337.Atmosphere.Debug;
using System.Linq;

public partial class SpawnMode : RichTextLabel
{
    [Export] GasStateInt state;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    { 
        state.Changed += SetText;
        SetText();
    }

    void SetText()
    {
        String str = "Spawn mode: ";

        int maxVal = (Enum.GetValues(typeof(GasInteractor.SpawnType)) as int[]).Max();
        float mult = Mathf.Remap((int)state.spawnMode, 0, maxVal, 0, 0.668523677f);
        Color rgb = Color.FromHsv(mult, 1, 1, 1);
        str += $"[color=#{rgb.ToHtml()}]";
        str += state.spawnMode.ToString();
        str += "[/color]";

        if (state.spawnMode == GasInteractor.SpawnType.GAS)
        {
            str += "\nSpawn gas: ";
            str += $"[color=#{state.GetGasMix().GetGasColor().ToHtml()}]";
            str += state.gases[state.gasIndex].nameChem;
            str += "[/color]";
        }

        Text = str;
    }
}
