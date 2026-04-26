using Godot;
using System;
using System.Collections.Generic;

public partial class ShopMenu : Control
{
    [Export] public Label FinishLabel;
    [Export] public BoxContainer ButtonList;
    [Signal] public delegate void NextRaceEventHandler();

    private List<PlayerUpgrade> availableUpgrades = new ();

    public override void _Ready()
    {
        GameManager.Instance.SetGameState(GameManager.State.Shop);
        if (FinishLabel != null) {
            FinishLabel.Text = "Congrats, you finished: " + GameManager.Instance.PlayerRacePosition;
        }

        for (int i = 0; i < 3; i++)
        {
            var upgradeType = (PlayerUpgrade.Type)GD.RandRange(0, 2);
            var multiplier = -1f;

            while (multiplier <= 0f)
            {
                multiplier = (float)Math.Round((GD.Randf() * 25) / 100, 2);
                GD.Print(multiplier);
            }

            availableUpgrades.Add(new PlayerUpgrade(upgradeType, multiplier));
        }

        foreach (PlayerUpgrade upgrade in availableUpgrades) {
            var button = new UpgradeButton(upgrade, this);
            ButtonList.AddChild(button);
        }

        ButtonList.GetChild<Button>(0).GrabFocus();
    }

    public void AddPlayerUpgrade(PlayerUpgrade upgrade)
    {
        GD.Print(upgrade.type, upgrade.name, upgrade.multiplier);
        GD.Print(GameManager.Instance);

        GameManager.Instance.PlayerUpgrades.Add(upgrade);
        GD.Print(GameManager.Instance.PlayerUpgrades.Count);
    }

    public void _on_next_race()
    {
        GameManager.Instance.SwitchToRaceScene();
    }
}
