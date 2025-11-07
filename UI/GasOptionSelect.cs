using Godot;
using Pandora1337.Atmosphere.State;
using System;
using Pandora1337.Atmosphere;

public partial class GasOptionSelect : OptionButton
{
	[Export] GasStateInt state;

	public override void _Ready()
	{
		base._Ready();

		state.Changed += () => Select(state.gasIndex);
		ItemSelected += (val) =>
		{
			state.gasIndex = (int)val;
			state.EmitChanged();
		};

		foreach (GasObject i in state.gases)
		{
			AddItem(i.nameChem);
		}
	}
}
