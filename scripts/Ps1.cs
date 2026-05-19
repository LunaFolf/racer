using Godot;
using System;

[Tool]
public partial class Ps1 : ColorRect
{
	public override void _Ready()
	{
		if (Engine.IsEditorHint()) Visible = false;
		else Visible = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
