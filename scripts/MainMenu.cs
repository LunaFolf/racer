using Godot;
using System;

public partial class MainMenu : Control
{

	[Export] private Button StartButton;
	[Export] private Button OptionsButton;

	public override void _Ready()
	{

		GD.Print("Main Menu ready!");

		StartButton.GrabFocus();
	}

	public void _on_start_game()
	{
		GameManager.Instance.UiAccept();
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
		GameManager.Instance.UiAccept();
		GD.Print("Quit Game");
        GetTree().Root.PropagateNotification((int)NotificationWMCloseRequest);
		GetTree().Quit();
    }

	public void _on_credits_pressed()
	{
		GameManager.Instance.UiAccept();
		GetTree().ChangeSceneToFile("res://scenes/credits.tscn");
	}
	public void _on_options_pressed()
	{
		GameManager.Instance.UiAccept();
		GetTree().ChangeSceneToFile("res://scenes/options.tscn");
	}
}
