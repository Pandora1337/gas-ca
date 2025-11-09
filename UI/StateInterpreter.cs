using Godot;
using Godot.Collections;
using System;
using Pandora1337.Atmosphere.State;

public partial class StateInterpreter : Control
{
	[Export] public GasState state;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GenerateProperties();
	}

	// Sample Property data:
	// { "name": "Diffusion",   "class_name": &"", "type": 0, "hint": 0, "hint_string": "", "usage": 128 }
	// { "name": "isContinuous","class_name": &"", "type": 1, "hint": 0, "hint_string": "", "usage": 4102 }
	// { "name": "filterMode",  "class_name": &"", "type": 2, "hint": 2, "hint_string": "NORM,HIDE,WALL,MOLE,TEMP,PRES,WIND,EPIL", "usage": 4102 }
	// { "name": "maxMoles",    "class_name": &"", "type": 3, "hint": 0, "hint_string": "", "usage": 4102 }
	//
	// { "name": "gases",       "class_name": &"", "type": 28, "hint": 23, "hint_string": "24/17:GasObject", "usage": 4102 }
	// type: (int)Variant.Type.Array  hint: PropertyHint.TypeString hint_string: Variant.Type.Object / PropertyHint.ResourceType
	public void GenerateProperties()
	{
		if (state == null)
			return;
		
        bool isEditor = OS.HasFeature("editor");
		VBoxContainer root = GetNode<VBoxContainer>("VBoxContainer");

        Array<Dictionary> properties = state.GetPropertyList();
        if (!isEditor)
            properties.Reverse();

        bool foundScriptName = false;
        foreach (Dictionary dict in properties)
		{
			// GD.Print(dict);
			
			// (int)PropertyUsageFlags.Subgroup == 256
			// Category
			if ((int)dict["usage"] == (int)PropertyUsageFlags.Category && (String)dict["hint_string"] == "")
			{
				// Script Class names are also serialised as a category in the Editor, so
				// to avoid double naming I skip the first "category"
				if (isEditor && !foundScriptName)
				{
					foundScriptName = true;
					continue;
				}

				SpawnCategory(root, (String)dict["name"]);
			}
			
			// Bitmask
			if ((int)dict["usage"] != 4102)
				continue;

			// Bool
			if ((int)dict["type"] == (int)Variant.Type.Bool)
				SpawnCheckbox(root, dict);

			// Int / Enum 
			if ((int)dict["type"] == (int)Variant.Type.Int)
				SpawnOptionBox(root, dict);

			// Float
			if ((int)dict["type"] == (int)Variant.Type.Float)
				SpawnSpinBox(root, dict);
        }
	}

	void SpawnCategory(Control root, String text)
	{
		Label newLabel = new()
		{
			FocusMode = FocusModeEnum.None,
			CustomMinimumSize = new Vector2(0, 30),
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Bottom,
			ClipText = true,
			Text = text,
		};
		root.AddChild(newLabel);
	}

	void SpawnCheckbox(Control root, Dictionary data)
	{
		StringName variable = (StringName)data["name"];
		CheckBox entry = new CheckBox
		{
			FocusMode = FocusModeEnum.None,
			CustomMinimumSize = new Vector2(150, 30),
			ClipText = true,
			Text = HumanifyString((string)data["name"]),
			ButtonPressed = (bool)state.Get(variable),
		};

		state.Changed += () => entry.ButtonPressed = (bool)state.Get(variable);
		entry.Toggled += (val) =>
		{
			state.Set(variable, val);
			state.EmitChanged();
		};

		root.AddChild(entry);
	}

	void SpawnOptionBox(Control root, Dictionary data)
	{
		HBoxContainer cont = new();
		OptionButton opt = new()
		{
			FocusMode = FocusModeEnum.None,
		};

		string items = (string)data["hint_string"];
		foreach (string item in items.Split(","))
		{
			opt.AddItem(item);
		}

		StringName variable = (StringName)data["name"];
		opt.Select((int)state.Get(variable));

		state.Changed += () => opt.Selected = (int)state.Get(variable);
		opt.ItemSelected += (val) =>
		{
			state.Set(variable, val);
			state.EmitChanged();
		};

		cont.AddChild(GetLabel(HumanifyString((String)data["name"])));
		cont.AddChild(opt);
		root.AddChild(cont);
	}

	private void SpawnSpinBox(Control root, Dictionary data)
	{
		HBoxContainer cont = new();

		// TODO make this a LineEdit
		StringName variable = (StringName)data["name"];
		SpinBox spin = new SpinBox()
		{
			FocusMode = FocusModeEnum.None,
			Step = 0.0001f,
			MinValue = 0f,
			AllowGreater = true,
			Alignment = HorizontalAlignment.Right,
			Value = (float)state.Get(variable),
		};

		LineEdit line = new()
		{
			FocusMode = FocusModeEnum.Click,
			ContextMenuEnabled = false,
			EmojiMenuEnabled = false,
		};

		state.Changed += () => spin.Value = (float)state.Get(variable);
		spin.ValueChanged += (val) =>
		{
			// if (!val.IsValidFloat())
			// 	return;
			state.Set(variable, val);
			state.EmitChanged();
		};
		// line.TextChanged += (val) =>
		// {
		// 	GD.Print(line.Text);
		// 	GD.Print(line.Text[-1]);
		// 	GD.Print(line.Text[-1].ToString().IsValidFloat());
		// 	if (!line.Text[-1].ToString().IsValidFloat())
		// 		line.DeleteCharAtCaret();
		// };

		cont.AddChild(GetLabel(HumanifyString((string)data["name"])));
		cont.AddChild(spin);
		// cont.AddChild(line);
		root.AddChild(cont);
	}

	Label GetLabel(String text)
	{
		return new()
		{
			FocusMode = FocusModeEnum.None,
			CustomMinimumSize = new Vector2(164, 30),
			VerticalAlignment = VerticalAlignment.Center,
			ClipText = true,
			Text = text,
		};
	}

	String HumanifyString(String input)
	{
		String str = "";
		str += char.ToUpper(input[0]);
		for (int i = 1; i < input.Length; i++)
		{
			if (char.IsUpper(input[i]) && char.IsLower(input[i - 1]))
				str += ' ';

			str += input[i];
		}

		return str;
	}
}
