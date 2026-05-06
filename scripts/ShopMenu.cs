using Godot;
using System;
using System.Collections.Generic;

public partial class ShopMenu : Control
{
    [Export] public Label ScoreLabel;
    [Export] public Label PointsLabel;
    [Export] public GpuParticles2D PointsParticles;
    [Export] public BoxContainer ButtonList;
    [Export] public Button NextRaceButton;
    [Signal] public delegate void NextRaceEventHandler();

    private int removalCounter;
    private bool _countdown = false;

    private List<PlayerUpgrade> availableUpgrades = new ();

    public override void _Ready()
    {
        GameManager.Instance.SetGameState(GameManager.State.Shop);

        NextRaceButton.Visible = false;

        foreach (var child in ButtonList.GetChildren())
        {
            child.Free();
        }

        if (ScoreLabel != null) {
            ScoreLabel.Text = "Data Transfered: " + GameManager.Instance.PlayerScore + "mb";
        }
        if (PointsLabel != null) {
            PointsLabel.Text = GameManager.Instance.PlayerPoints.ToString();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_countdown) return;

        if (GameManager.Instance.PlayerScore <= 0)
        {
            if (GameManager.Instance.PlayerScore < 0)
            {
                GameManager.Instance.PlayerScore = 0;
                ScoreLabel.Text = "Data Transfered: " + GameManager.Instance.PlayerScore + "mb";
            }
            _countdown = false;
            GenerateShop();
            return;
        }

        int deliminator = GameManager.Instance.RaceCount * 16;

        GameManager.Instance.PlayerScore -= deliminator;
        removalCounter += deliminator;

        ScoreLabel.Text = "Data Transfered: " + GameManager.Instance.PlayerScore + "mb";

        if (removalCounter >= 1024)
        {
            PointsParticles.Restart();
            PointsParticles.Emitting = true;
            removalCounter = 0;
            GameManager.Instance.PlayerPoints++;
            PointsLabel.Text = GameManager.Instance.PlayerPoints.ToString();
        }
    }

    public void StartCountdown()
    {
        _countdown = true;
    }

    private int availableTypeCount(PlayerUpgrade.Type type)
    {
        int count = 0;
        foreach (PlayerUpgrade upgrade in availableUpgrades)
        {
            if (upgrade.type == type) count++;
        }

        return count;
    }

    private void GenerateShop()
    {
        for (int i = 0; i < 3; i++)
        {
            PlayerUpgrade.Type upgradeType;

            do
            {
                upgradeType = (PlayerUpgrade.Type)GD.RandRange(0, 2);
            } while (availableTypeCount(upgradeType) >= 2);

            var multiplier = -1f;

            while (multiplier <= 0f)
            {
                multiplier = GD.RandRange(1,25) / 100f;
                GD.Print(multiplier);
            }

            availableUpgrades.Add(new PlayerUpgrade(upgradeType, multiplier));
        }

        foreach (PlayerUpgrade upgrade in availableUpgrades)
        {
            var button = new UpgradeButton(upgrade, this);
            ButtonList.AddChild(button);
        }

        ButtonList.GetChild<Button>(0).GrabFocus();

        NextRaceButton.Visible = true;
    }

    public void UpdateButtons()
    {
        foreach (UpgradeButton button in ButtonList.GetChildren())
        {
            button.Refresh();
        }
    }

    public bool AddPlayerUpgrade(PlayerUpgrade upgrade, int cost)
    {
        if (GameManager.Instance.PlayerPoints < cost) return false;
        GameManager.Instance.PlayerPoints -= cost;
        PointsLabel.Text = GameManager.Instance.PlayerPoints.ToString();
        PointsParticles.Restart();
        PointsParticles.Emitting = true;

        GD.Print(upgrade.type, upgrade.name, upgrade.multiplier);
        GD.Print(GameManager.Instance);

        GameManager.Instance.PlayerUpgrades.Add(upgrade);
        GD.Print(GameManager.Instance.PlayerUpgrades.Count);

        CallDeferred("UpdateButtons");

        return true;
    }

    public void _on_next_race()
    {
        GameManager.Instance.RaceCount++;
        GameManager.Instance.SwitchToRaceScene();
    }
}
