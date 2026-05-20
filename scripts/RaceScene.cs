using Godot;
using System;
using System.Linq;

public partial class RaceScene : Node2D
{
    [Export] public HUD Hud;
    [Export] public GoalManager GoalManager;
    [Export] public RacerManager RacerManager;
    [Export] PackedScene PlayerPackedScene;
    [Export] PackedScene MainCameraScene;
    private Player _player;
    [Export] public Timer Timer;
    [Export] public Timer GameOverTimer;
    [Export] public AudioStreamPlayer CountdownSound;

    private Godot.Collections.Dictionary<int, double> StageTime = new() { [0] = 0 };
    private Godot.Collections.Dictionary<int, double> SplitTime = new() { [0] = 0 };
    private Godot.Collections.Dictionary<int, int> RacerLaps = new() { [0] = 0 };
    private Godot.Collections.Dictionary<int, int> RacerGoals = new() { [0] = 1 };
    private Godot.Collections.Dictionary<int, CharacterBody2D> Racers = new();
    private int raceCountdown = 5;

    public override void _Ready()
    {
        GD.Print("Turn off HUD");
        Hud.SetPositionVisible(false);
        Hud.SetScoreVisible(false);

        GD.Print("Get Player");
        _player = PlayerPackedScene.Instantiate<Player>();
        GD.Print("Get Camera");
        _player.Camera = MainCameraScene.Instantiate<MainCamera>();
        _player.SetRaceScene(this);
        _player.Hud = Hud;
        AddChild( _player );
        _player.AddChild(_player.Camera);

        GD.Print("Going to gen");
        GenerateRace();
        StartRaceCountdown();

        GD.Print("Bot Upgrades:");
        // GD.Print(GameManager.Instance.BotUpgradeMults);
        GD.Print("Speed: " + GameManager.Instance.BotUpgradeMults.Speed + ", Traction: " +
                 GameManager.Instance.BotUpgradeMults.Traction + ", Turning: " +
                 GameManager.Instance.BotUpgradeMults.Turning + "");
    }

    public void StartRaceCountdown()
    {
        _player.Camera.shake = 0;
        Timer.Start();
    }

    public void RemoveRacer(int racerNumber)
    {
        Racers.Remove(racerNumber);
    }

    public override void _Process(double delta)
    {
        if (IsQueuedForDeletion()) return;
        if (GameManager.Instance.GameState != GameManager.State.Racing) return;
        UpdatePositionsList();
    }

    public void SetSplitTime(int racerId, double time)
    {
        SplitTime[racerId] = time;
        StageTime[racerId] += time;
    }
    public void SetStageTime(int racerId, double time)
    {
        StageTime[racerId] = time;
    }

    public void SetRacerLap(int racerId)
    {
        RacerLaps[racerId] += 1;
    }
    public void SetRacerGoal(int racerId, int goal)
    {
        RacerGoals[racerId] = goal;
    }

    public void EndRace(bool playerWon)
    {
        GameManager.Instance.SetGameState(GameManager.State.Ending);
        Input.StopJoyVibration(0);

        if (playerWon)
        {
            GD.Print("Player Won");
            foreach (var racerId in Racers.Keys)
            {
                var racer = Racers[racerId];
                Racers.Remove(racerId);
                racer.QueueFree();
            }

            GetTree().ChangeSceneToFile("res://scenes/shop.tscn");
        }
        else
        {
            GD.Print("Player Lost");

            foreach (var racerId in Racers.Keys)
            {

                if (racerId == 0)
                {
                    var player = (Player)Racers[0];
                    player.ExplosionAnimation();
                }
                else
                {
                    var racer = (Racer)Racers[racerId];
                    racer.ExplosionAnimation();
                }
            }

            GameOverTimer.Start();
            // GetTree().ChangeSceneToFile("res://scenes/gameover.tscn");
        }
    }

    private void UpdatePositionsList()
    {
        if (GameManager.Instance.GameState != GameManager.State.Racing) return;
        int positionCounter = 0;

        var ordered = Racers.Keys
            .OrderByDescending(id => RacerLaps[id])
            .ThenByDescending(id => RacerGoals[id])
            .ThenBy(id => GoalManager.DistanceToGoal(
                Racers[id].GlobalPosition, RacerGoals[id] - 1
            ));

        foreach (var racerId in ordered)
        {
            if (racerId == 0) GameManager.Instance.PlayerRacePosition = positionCounter + 1;

            Racers[racerId].Set("RacePosition", positionCounter);

            positionCounter++;
        }

        Hud.SetPositionText(GameManager.Instance.PlayerRacePosition + " / " + (RacerManager.MaxRacers + 1));
    }

    public void GenerateRace()
    {
        GD.Print("starting goal gen");
        GoalManager.StartGeneration();
        GD.Print("============================================");
        GD.Print("goal gen done");
        GD.Print("============================================");

        GD.Print("player: ", _player);

        Hud.Minimap.SetMap(GoalManager.TrackPoints);
        _player.NumberOfGoals = GoalManager.GoalCounter;
        Racers.Add(0, _player);
        RacerManager.GenerateRacers(GoalManager.GoalCounter, this);

        foreach (Racer racer in RacerManager.Racers)
        {
            Racers.Add(racer.RacerNumber, racer);

            StageTime.Add(racer.RacerNumber, 0);
            SplitTime.Add(racer.RacerNumber, 0);
            RacerLaps.Add(racer.RacerNumber, 0);
            RacerGoals.Add(racer.RacerNumber, 1);
        }

        Hud.Minimap.SetPlayer(_player);
        Hud.Minimap.SetRaceManager(RacerManager);
    }

    public void _on_GameOverTimer_timeout()
    {
        GetTree().ChangeSceneToFile("res://scenes/gameover.tscn");
    }

    public void _on_race_countdown_timer_timeout()
    {
        raceCountdown--;
        Hud.SetCountdownText(raceCountdown.ToString());
        if (raceCountdown > 0)
        {
            CountdownSound.Play();
            return;
        }

        Hud.SetCountdownText("");
        Timer.Stop();
        Timer.QueueFree();

        Hud.SetPositionVisible(true);
        Hud.SetScoreVisible(true);

        CountdownSound.SetPitchScale(2);
        CountdownSound.Play();

        GameManager.Instance.SetGameState(GameManager.State.Racing);
    }
}
