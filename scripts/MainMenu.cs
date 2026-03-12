using Godot;
using System;

public partial class MainMenu : Control
{

	[Signal] public delegate void StartGameEventHandler();

	public void _on_start_game()
	{
        GameManager.Instance.SwitchToRaceScene();
    }
}
