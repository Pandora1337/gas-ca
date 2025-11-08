using Godot;
using System;

public partial class FpsCounter : RichTextLabel
{
    public override void _Process(double delta)
    {
        String str = "FPS: ";
        int fps = (int)Engine.GetFramesPerSecond();
        if (fps >= 60)
        {
            str += "[color=#00FF00]";
        }
        else
        {
            float mult = Mathf.Remap(fps, 0, 60, 0, 0.334261838f);
            Color rgb = Color.FromHsv(mult, 1, 1, 1);
            str += $"[color=#{rgb.ToHtml()}]";
        }
        str += fps.ToString() + "[/color]";
        Text = str;
    }
}
