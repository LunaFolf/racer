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


    public void SetPositionVisible(bool state)
	{
		Position.Visible = state;
	}

	public void SetPositionText(string positions)
	{ 
		Position.Text = positions;
	}

	public void SetCountdownText(string countdown)
	{
		Countdown.Text = countdown;
	}
}
