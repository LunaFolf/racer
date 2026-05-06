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
        if (GameManager.Instance.GameSeed.Length > 0)
        {
            GD.Print("Using Seed: " + GameManager.Instance.GameSeed);
            GD.Seed(GameManager.Instance.GameSeed.Hash());
        }
        GameManager.Instance.SwitchToRaceScene();
    }

	public void _on_quit_game()
	{
		GD.Print("Quit Game");
        GetTree().Root.PropagateNotification((int)NotificationWMCloseRequest);
		GetTree().Quit();
    }

	public void _on_seed_change(string newText)
	{
		GameManager.Instance.GameSeed = newText;
	}
}
