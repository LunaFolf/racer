using Godot;
using System;

public partial class ClickableURL : Label
{
	[Export] public string Url = "";
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (Url.Length == 0) return;

		Text = Url;
	}

	public void _on_Label_pressed()
	{
		OS.ShellOpen(Url);
	}
}
