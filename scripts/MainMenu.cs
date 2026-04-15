using Godot;
using System;

public partial class MainMenu : Control
{

	[Export] private Button StartButton;
	[Export] private Button QuitButton;

	public override void _Ready()
	{
		GD.Print("Main Menu ready!");

		StartButton.GrabFocus();
	}

	public void _on_start_game()
	{
		GD.Print("Start button pressed");
        GameManager.Instance.SwitchToRaceScene();
    }

	public void _on_quit_game()
	{
		GD.Print("Quit Game");
        GetTree().Root.PropagateNotification((int)NotificationWMCloseRequest);
		GetTree().Quit();
    }
}
