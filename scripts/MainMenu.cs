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
		GameManager.Instance.UiAccept();
		GetTree().ChangeSceneToFile("res://scenes/mode.tscn");
    }

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("quit")) _on_quit_game();
		if (@event.IsActionPressed("ui_back")) QuitButton.GrabFocus();
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
