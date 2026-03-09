using Godot;
using System;

public partial class MainMenu : Control
{

	[Signal] public delegate void StartGameEventHandler();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_start_game()
	{
		GetTree().ChangeSceneToFile("res://scenes/game.tscn");
	}
}
