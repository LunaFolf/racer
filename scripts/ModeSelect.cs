using Godot;
using System;

public partial class ModeSelect : Control
{

	[Export] private Button TutorialButton;
	[Export] private Button RaceButton;

	public override void _Ready()
	{

		GD.Print("Main Menu ready!");

		TutorialButton.GrabFocus();
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
	public void _on_tutorial_mode()
	{
		GameManager.Instance.UiAccept();
		GetTree().ChangeSceneToFile("res://scenes/tutorial.tscn");
	}
}
