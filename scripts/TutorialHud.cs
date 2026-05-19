using Godot;
using System;

public partial class TutorialHud : HUD
{
	[Export] public Label TutorialText;
	[Export] public ProgressBar ProgressBar;
	public override void _Ready()
	{
	}

	public void SetTutorialText(string text)
	{
		TutorialText.Text = text;
	}

	public void SetTutorialProgress(float progress)
	{
		ProgressBar.Value = progress;
	}
}
