using Godot;
using System;

public partial class ShopMenu : Control
{
    [Export] public Label FinishLabel;
    [Signal] public delegate void NextRaceEventHandler();
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (FinishLabel != null) {
            FinishLabel.Text = "Congrats, you finished: " + GameManager.Instance.PlayerRacePosition;
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void _on_next_race()
    {
        GetTree().ChangeSceneToFile("res://scenes/game.tscn");
    }
}
