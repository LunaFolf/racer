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
    [Export] public Button QuitButton;
    [Export] public Label SpeedLabel;
    [Export] public Label TractionLabel;
    [Export] public Label TurningLabel;

    [Export] public AudioStreamPlayer DataDownSFX;
    [Export] public AudioStreamPlayer BitsUpSFX;
    [Export] public AudioStreamPlayer CompleteRaceSFX;
    [Signal] public delegate void NextRaceEventHandler();

    [Export] private Timer _countdownTimer;
    private int removalCounter;
    private bool _countdown = false;

    private List<PlayerUpgrade> availableUpgrades = new ();

    public override void _Ready()
    {
        GameManager.Instance.SetGameState(GameManager.State.Shop);

        GameManager.Instance.MusicPlayer.VolumeDb = -32;

        NextRaceButton.Visible = false;
        QuitButton.Visible = false;

        CompleteRaceSFX.Play(0.21f);

        foreach (var child in ButtonList.GetChildren())
        {
            child.Free();
        }

        if (ScoreLabel != null) {
            ScoreLabel.Text = "Data Transferred: " + GameManager.Instance.PlayerScore + "mb";
        }
        if (PointsLabel != null) {
            PointsLabel.Text = GameManager.Instance.PlayerPoints.ToString();
        }

        UpdateStats();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_countdown) return;

        if (GameManager.Instance.PlayerScore <= 0)
        {
            if (GameManager.Instance.PlayerScore < 0)
            {
                GameManager.Instance.PlayerScore = 0;
                ScoreLabel.Text = "Data Transferred: " + GameManager.Instance.PlayerScore + "mb";
            }
            DataDownSFX.Stop();
            _countdown = false;
            GenerateShop();
            return;
        }

        int deliminator = GameManager.Instance.RaceCount * 16;

        GameManager.Instance.PlayerScore -= deliminator;
        removalCounter += deliminator;

        DataDownSFX.Play(0.18f);

        ScoreLabel.Text = "Data Transferred: " + GameManager.Instance.PlayerScore + "mb";

        if (removalCounter >= 1024)
        {
            BitsUpSFX.Play();
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

    public override void _UnhandledInput(InputEvent @event)
    {
        if (GameManager.Instance.PlayerScore <= 0) return;

        if (!@event.IsActionPressed("ui_accept")) return;

        _countdownTimer.Stop();
        _countdown = false;

        DataDownSFX.Stop();

        var remainingPoints = GameManager.Instance.PlayerScore + removalCounter;
        if (remainingPoints > 0)
        {
            var newPoints = remainingPoints / 1024;
            GameManager.Instance.PlayerPoints += newPoints;
            PointsLabel.Text = GameManager.Instance.PlayerPoints.ToString();
            PointsParticles.Restart();
            PointsParticles.Emitting = true;

            GameManager.Instance.PlayerScore = 0;
            ScoreLabel.Text = "Data Transferred: " + GameManager.Instance.PlayerScore + "mb";
        }

        GenerateShop();
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
                multiplier = GD.RandRange(1,10) / 100f;
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
        QuitButton.Visible = true;
    }

    public void UpdateButtons()
    {
        foreach (UpgradeButton button in ButtonList.GetChildren())
        {
            button.Refresh();
        }
    }

    private void UpdateStats()
    {
        SpeedLabel.Text = "+" + (int)(GameManager.Instance.PlayerUpgradesMults.Speed * 100f) + "%";
        TractionLabel.Text = "+" + (int)(GameManager.Instance.PlayerUpgradesMults.Traction * 100f) + "%";
        TurningLabel.Text = "+" + (int)(GameManager.Instance.PlayerUpgradesMults.Turning * 100f) + "%";
    }

    public bool AddPlayerUpgrade(PlayerUpgrade upgrade, int cost)
    {
        GD.Print("PP: " + GameManager.Instance.PlayerPoints + ", cost: " + cost + "");
        if (GameManager.Instance.PlayerPoints < cost)
        {
            GameManager.Instance.UiDeny();
            return false;
        }
        GameManager.Instance.PlayerPoints -= cost;
        PointsLabel.Text = GameManager.Instance.PlayerPoints.ToString();
        PointsParticles.Restart();
        PointsParticles.Emitting = true;

        GD.Print(upgrade.type, upgrade.name, upgrade.multiplier);
        GD.Print(GameManager.Instance);

        // GameManager.Instance.PlayerUpgrades.Add(upgrade);
        GameManager.Instance.AddPlayerUpgrade(upgrade);
        GD.Print(GameManager.Instance.PlayerUpgrades.Count);

        CallDeferred("UpdateButtons");

        UpdateStats();
        GameManager.Instance.UiAccept();

        return true;
    }

    public void _on_next_race()
    {
        GameManager.Instance.UiAccept();
        GameManager.Instance.RaceCount++;
        GameManager.Instance.SwitchToRaceScene();
    }
    public void _on_quit_game()
    {
        GameManager.Instance.UiAccept();
        GameManager.Instance.RaceCount = 1;
        GetTree().ChangeSceneToFile("res://scenes/menu.tscn");
    }
}
