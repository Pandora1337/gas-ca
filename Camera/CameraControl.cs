using Godot;
using Godot.Collections;
using System;

public partial class CameraControl : Camera2D
{
    [Export] CameraState state;

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        Vector2 direction = Input.GetVector("go_left", "go_right", "go_up", "go_down");
        Position += direction * state.speed;
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

        if (@event.IsActionPressed("scroll_up"))
        {
            if (Zoom + state.zoom_step > state.zoom_max)
                return;

            Zoom += state.zoom_step;
        }

        if (@event.IsActionPressed("scroll_down"))
        {
            if (Zoom - state.zoom_step <= Vector2.Zero)
                return;

            Zoom -= state.zoom_step;
        }
    }
}
