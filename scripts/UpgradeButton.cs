using Godot;
using System;

public partial class UpgradeButton : Button
{
	private PlayerUpgrade upgrade;
	private ShopMenu shopMenu;
	public int baseCost = 2;
	public int timesBought = 0;

	public int Cost
	{
		get
		{
			return (baseCost * GameManager.Instance.RaceCount) * (timesBought + 1);
		}
	}

	public UpgradeButton(PlayerUpgrade upgrade, ShopMenu shopMenu)
	{
		this.upgrade = upgrade;
		this.shopMenu = shopMenu;
	}

	public override void _Ready()
	{
		UpdateText();
        Pressed += OnPressed;
		SizeFlagsHorizontal = SizeFlags.ExpandFill;
    }

	private void UpdateText()
	{
        Text = upgrade.name + "\n+" + upgrade.multiplier * 100f + "% " + upgrade.type.ToString().Capitalize() + "\n" + Cost + " Bits";
    }

    public void OnPressed()
    {
		timesBought++;
        if (!shopMenu.AddPlayerUpgrade(upgrade, Cost)) timesBought--;
    }

    public void Refresh()
    {
        UpdateText();

        if (GameManager.Instance.PlayerPoints < Cost)
        {
            Color color = Modulate;
            color.A8 = 165;
            Modulate = color;
        }
    }
}
