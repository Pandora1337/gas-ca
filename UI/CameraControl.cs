using Godot;
using Godot.Collections;
using System;

public partial class CameraControl : Camera2D
{
    [Export]
    float speed = 30;

    [Export] float ZoomMax = 4;
    Vector2 zoom_max
    {
        get => new(ZoomMax, ZoomMax);
    }

    [Export] float ZoomStep = 0.1f;
    Vector2 zoom_step
    {
        get => new(ZoomStep, ZoomStep);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        Vector2 direction = Input.GetVector("go_left", "go_right", "go_up", "go_down");
        Position += direction * speed;
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        
        bool isDragging = Input.IsActionPressed("cam_drag");
        if (isDragging && @event is InputEventMouseMotion mvent)
            Position -= mvent.Relative / Zoom;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (Input.IsActionPressed("shift") || Input.IsActionPressed("ctrl"))
            return;

        Vector2 zoom_step = new(0.1f, 0.1f);
        if (@event.IsActionPressed("scroll_up"))
        {
            if (Zoom + zoom_step > zoom_max)
                return;

            Zoom += zoom_step;
        }

        if (@event.IsActionPressed("scroll_down"))
        {
            if (Zoom - zoom_step <= Vector2.Zero)
                return;

            Zoom -= zoom_step;
        }
    }
}
