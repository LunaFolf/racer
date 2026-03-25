using Godot;
using System;

public partial class MainMenu : Control
{

	[Signal] public delegate void StartGameEventHandler();

	public override void _Ready()
	{
		GD.Print("Main Menu ready!");
	}

	public void _on_start_game()
	{
		GD.Print("Start button pressed");
        GameManager.Instance.SwitchToRaceScene();
    }
}
