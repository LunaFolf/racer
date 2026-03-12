using Godot;
using System;

public partial class ShopMenu : Control
{
    [Export] public Label FinishLabel;
    [Signal] public delegate void NextRaceEventHandler();
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GameManager.Instance.SetGameState(GameManager.State.Shop);
        if (FinishLabel != null) {
            FinishLabel.Text = "Congrats, you finished: " + GameManager.Instance.PlayerRacePosition;
        }
    }

    public void _on_next_race()
    {
        GameManager.Instance.SwitchToRaceScene();
    }
}
