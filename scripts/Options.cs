using Godot;
using System;

public partial class Options : Control
{
	[Export] private LineEdit SeedInput;
	[Export] private HSlider MasterSlider;
	[Export] private HSlider MusicSlider;
	[Export] private HSlider SFXSlider;
	[Export] private OptionButton WindowButton;

	public override void _Ready()
	{
		MasterSlider.SetValueNoSignal( AudioServer.GetBusVolumeLinear(0));
		MusicSlider.SetValueNoSignal( AudioServer.GetBusVolumeLinear(1));
		SFXSlider.SetValueNoSignal( AudioServer.GetBusVolumeLinear(2));

		SeedInput.SetText(GameManager.Instance.GameSeed);

		WindowButton.Selected = (int)DisplayServer.WindowGetMode();

		SeedInput.GrabFocus();
	}

	public void _on_window_mode_changed(int newMode)
	{
		switch (newMode)
		{
			case 0: // Window
				DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
				break;
			case 1: // Borderless Fullscreen
				DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
				break;
			case 2: // Exclusive Fullscreen
				DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
				break;
		}
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

	public void _on_seed_change(string newText)
	{
		GameManager.Instance.UiAccept();
		GameManager.Instance.GameSeed = newText;
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
