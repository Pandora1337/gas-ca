using Godot;
using System;
using Pandora1337.Atmosphere.State;

public partial class SimStat : RichTextLabel
{
    [Export] GasStateSim state;

    public override void _Ready()
    {
        state.Changed += SetText;
        SetText();
    }

    void SetText()
    {
        String str = "Simulation: ";
        if (state.isContinuous)
        {
            str += "[color=#00FF00]RUNNING[/color]";
        }
        else
        {
            str += "[color=#FF0000]PAUSED[/color]";
        }
        Text = str;
    }
}
