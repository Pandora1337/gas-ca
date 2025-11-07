using Godot;
using System;
using Pandora1337.Atmosphere.State;

// I made this inherit from GasState because im lazy and didnt want to make more boilerplate

[GlobalClass]
public partial class CameraState : GasState
{
    [Export]
    public float speed = 30;

    [Export]
    public float ZoomMax = 5;
    public Vector2 zoom_max
    {
        get => new(ZoomMax, ZoomMax);
    }

    [Export]
    public float ZoomStep = 0.1f;
    public Vector2 zoom_step
    {
        get => new(ZoomStep, ZoomStep);
    }
}
