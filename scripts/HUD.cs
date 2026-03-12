using Godot;
using System;

public partial class HUD : CanvasLayer
{
	[Export] public Label Position;
	[Export] public Minimap Minimap;
	[Export] public Label Countdown;

	public override void _Ready()
	{

	}

	public void SetPosition(string positions)
	{
		Position.Text = positions;
	}

	public void SetCountdown(string countdown)
	{
		Countdown.Text = countdown;
	}
}
