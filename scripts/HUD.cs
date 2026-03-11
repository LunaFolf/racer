using Godot;
using System;

public partial class HUD : CanvasLayer
{
	[Export] public Label Position;
	[Export] public Minimap Minimap;
	// Called when the node enters the scene tree for the first time.

	public void SetPosition(string positions)
	{
		Position.Text = positions;
	}
}
