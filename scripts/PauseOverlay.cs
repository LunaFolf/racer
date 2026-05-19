using Godot;
using System;

[Tool]
public partial class PauseOverlay : CanvasLayer
{
	[Export] public Button ResumeButton;
	[Export] private HSlider MasterSlider;
	[Export] private HSlider MusicSlider;
	[Export] private HSlider SFXSlider;
	public override void _Ready()
	{
		Visible = false;
		MasterSlider.SetValueNoSignal( AudioServer.GetBusVolumeLinear(0));
		MusicSlider.SetValueNoSignal( AudioServer.GetBusVolumeLinear(1));
		SFXSlider.SetValueNoSignal( AudioServer.GetBusVolumeLinear(2));
	}

	public new void SetVisible(bool state)
	{
		Visible = state;

		GameManager.Instance.SetGameState(Visible ? GameManager.State.Paused : GameManager.State.Racing);

		if (!Visible) return;

		ResumeButton.GrabFocus();
		Input.StopJoyVibration(0);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (GameManager.Instance.GameState != GameManager.State.Racing) return;

		if (@event.IsActionPressed("pause")) ToggleVisible();
	}

	public void ToggleVisible()
	{
		SetVisible(!Visible);
	}

	public void _on_resume_pressed()
	{
		GameManager.Instance.UiAccept();
		SetVisible(false);
	}

	public void _on_quit_pressed()
	{
		GameManager.Instance.UiAccept();
		GetTree().ChangeSceneToFile("res://scenes/menu.tscn");
	}

	public void _on_master_volume_change(float newVolume)
	{
		AudioServer.SetBusVolumeLinear(0, newVolume);
	}
	public void _on_music_volume_change(float newVolume)
	{
		AudioServer.SetBusVolumeLinear(1, newVolume);
	}
	public void _on_sfx_volume_change(float newVolume)
	{
		GameManager.Instance.UiAccept();
		AudioServer.SetBusVolumeLinear(2, newVolume);
	}
}
