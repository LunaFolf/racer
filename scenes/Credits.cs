using Godot;
using System;

public partial class Credits : Control
{
	[Export] private Button BackButton;
	public override void _Ready()
	{
		BackButton.GrabFocus();
	}

	public void _on_back_pressed()
	{
		GameManager.Instance.UiAccept();
		GetTree().ChangeSceneToFile("res://scenes/menu.tscn");
	}
}
