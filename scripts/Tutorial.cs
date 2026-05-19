using Godot;
using System;

public partial class Tutorial : Node2D
{
    [Export] public HUD Hud;
    [Export] PackedScene PlayerPackedScene;
    [Export] PackedScene MainCameraScene;
    private Player _player;
    private TutorialHud TutHud => Hud as TutorialHud;

    public enum Stage
    {
        Accelerate,
        Turn,
        Reverse,
        Complete
    }

    public Stage CurrentStage = Stage.Accelerate;
    private double _stageProgress = 0;
    public override void _Ready()
    {
        GD.Print("Get Player");
        _player = PlayerPackedScene.Instantiate<Player>();
        GD.Print("Get Camera");
        _player.Camera = MainCameraScene.Instantiate<MainCamera>();
        _player.Hud = Hud;
        AddChild( _player );
        _player.AddChild(_player.Camera);

        GameManager.Instance.SetGameState( GameManager.State.Tutorial);
    }

    public override void _Process(double delta)
    {
        if (CurrentStage == Stage.Complete)
        {
            if (Input.IsActionJustPressed("ui_accept"))
            {
                GameManager.Instance.SetGameState(GameManager.State.Starting);
                GameManager.Instance.SwitchToRaceScene();
            }
            return;
        }

        switch (CurrentStage)
        {
            case Stage.Accelerate:
                TutHud.TutorialText.Text = "Press RT to Accelerate";
                if (Input.IsActionPressed("accelerate")) _stageProgress += delta * 20;
                break;
            case Stage.Turn:
                TutHud.TutorialText.Text = "Use Left Joystick to Turn";
                if (Input.GetAxis("left", "right") != 0f && !_player.Velocity.IsZeroApprox()) _stageProgress += delta * 40;
                break;
            case Stage.Reverse:
                TutHud.TutorialText.Text = "Press LT to Break/Reverse";
                if (Input.IsActionPressed("decelerate")) _stageProgress += delta * 30;
                break;
        }

        TutHud.ProgressBar.Value = _stageProgress;

        if (_stageProgress < 100f) return;
        _stageProgress = 0;
        CurrentStage++;

        if (CurrentStage != Stage.Complete) return;
        TutHud.TutorialText.Text = "Press A to Start Race";
        TutHud.ProgressBar.Visible = false;
    }
}
