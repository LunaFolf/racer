using Godot;
using System;

public partial class Credits : Control
{
	[Export] private Button BackButton;
	public override void _Ready()
	{
		BackButton.GrabFocus();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_back")) _on_back_pressed();
	}

	public void _on_back_pressed()
	{
		GameManager.Instance.UiAccept();
		GetTree().ChangeSceneToFile("res://scenes/menu.tscn");
	}
}
