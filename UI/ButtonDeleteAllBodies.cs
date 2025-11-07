using Godot;
using System;
using Pandora1337.Atmosphere.State;

public partial class ButtonDeleteAllBodies : Button
{
    [Export] GasStateForce state;
    
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        Pressed += () => state.EmitSignal("DeleteBodies");
    }
}
