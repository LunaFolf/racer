using Godot;
using System;

public partial class UpgradeButton : Button
{
	private PlayerUpgrade upgrade;
	private ShopMenu shopMenu;

	public UpgradeButton(PlayerUpgrade upgrade, ShopMenu shopMenu)
	{
		this.upgrade = upgrade;
		this.shopMenu = shopMenu;
	}

	public override void _Ready()
	{
        Text = upgrade.name + "\n+" + upgrade.multiplier * 100f + "% " + upgrade.type.ToString().Capitalize() + "\n<> " + (2 * GameManager.Instance.RaceCount);
		Pressed += OnPressed;
    }

    public void OnPressed()
    {
        shopMenu.AddPlayerUpgrade(upgrade);
    }
}
