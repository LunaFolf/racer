using Godot;
using System;

public partial class HUD : CanvasLayer
{
	[Export] public Label Position;
	[Export] public Minimap Minimap;
	[Export] public Label Countdown;
	[Export] public Label Score;
	[Export] public Control MainHUDGroup;
	[Export] public ShaderMaterial Bloom;

	[Export] public Label UpgradesList;
	[Export] public Label SpeedUpgrade;
	[Export] public Label TractionUpgrade;
	[Export] public Label TurningUpgrade;

	public override void _Ready()
	{
		var upgrades = GameManager.Instance.PlayerUpgrades;

		if (upgrades.Count < 1)
		{
			UpgradesList.Visible = false;
			return;
		}

		UpgradesList.Visible = true;

		float speed = 0, traction = 0, turning = 0;

		foreach (var upgrade in upgrades) { 
			switch(upgrade.type)
			{
				case PlayerUpgrade.Type.TURNING:
					turning += upgrade.multiplier;
					break;
				case PlayerUpgrade.Type.SPEED:
					speed += upgrade.multiplier;
					break;
				case PlayerUpgrade.Type.TRACTION:
					traction += upgrade.multiplier;
					break;
			}
		}

		SpeedUpgrade.Text = "+" + (int)(speed * 100) + "%";
		TurningUpgrade.Text = "+" + (int)(turning * 100) + "%";
		TractionUpgrade.Text = "+" + (int)(traction * 100) + "%";

		SpeedUpgrade.GetParent<Label>().Visible = speed > 0f;
        TurningUpgrade.GetParent<Label>().Visible = turning > 0f;
        TractionUpgrade.GetParent<Label>().Visible = traction > 0f;
    }


	public void SetPositionVisible(bool state)
	{
		Position.Visible = state;
	}

	public void SetScoreVisible(bool state)
	{
        Score.GetParent<Label>().Visible = state;
	}

	public void SetPositionText(string positions)
	{ 
		Position.Text = positions;
	}

	public void SetCountdownText(string countdown)
	{
		Countdown.Text = countdown;
	}

	public void SetScoreText(string score)
	{
        Score.Text = score;
	}

	public void SetScoreText(int score)
	{
		Score.Text = score.ToString() + "mb";
	}

	public void SetScoreText(float score) => SetScoreText((int)score);
}
