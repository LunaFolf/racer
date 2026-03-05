using Godot;
using System;

public partial class HUD : CanvasLayer
{
	[Export] public Label Ranking;
	[Export] public Label Positions;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void SetRanking(string ranking)
	{
		Ranking.Text = ranking;
	}

	public void SetPositions(string positions)
	{
		Positions.Text = positions;
	}
}
