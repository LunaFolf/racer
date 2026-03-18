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

    private Godot.Collections.Dictionary<int, double> StageTime = new() { [0] = 0 };
    private Godot.Collections.Dictionary<int, double> SplitTime = new() { [0] = 0 };
    private Godot.Collections.Dictionary<int, int> RacerLaps = new() { [0] = 0 };
    private Godot.Collections.Dictionary<int, int> RacerGoals = new() { [0] = 1 };
    private Godot.Collections.Dictionary<int, CharacterBody2D> Racers = new();
    private int raceCountdown = 5;
    public override void _Ready()
    {
        _player = new Player();
        _player.Camera = MainCameraScene.Instantiate<MainCamera>();
        _player.SetRaceScene(this);
        _player.Hud = Hud;
        AddChild( _player );
        _player.AddChild(_player.Camera);

        GenerateRace();
        StartRaceCountdown();
    }

    public void StartRaceCountdown()
    {
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

    public void EndRace()
    {
        GameManager.Instance.SetGameState(GameManager.State.Ending);
        foreach (var racerId in Racers.Keys)
        {
            var racer = Racers[racerId];
            Racers.Remove(racerId);
            racer.QueueFree();
        }

        GetTree().ChangeSceneToFile("res://scenes/shop.tscn");
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

        Hud.SetPosition(GameManager.Instance.PlayerRacePosition + " / " + (RacerManager.MaxRacers + 1));
    }

    public void GenerateRace()
    {
        GD.Print("starting goal gen");
        GoalManager.StartGeneration();
        GD.Print("goal gen done");

        GD.Print("player: ", _player);

        Hud.Minimap.SetMap(GoalManager.TrackPoints);
        _player.NumberOfGoals = GoalManager.GoalCounter;
        Racers.Add(0, _player);
        RacerManager.MaxRacers = 9;
        RacerManager.GenerateRacers(GoalManager.GoalCounter, this);

        foreach (Racer racer in RacerManager.Racers)
        {
            Racers.Add(racer.RacerNumber, racer);

            StageTime.Add(racer.RacerNumber, 0);
            SplitTime.Add(racer.RacerNumber, 0);
            RacerLaps.Add(racer.RacerNumber, 0);
            RacerGoals.Add(racer.RacerNumber, 1);
        }
    }

    public void _on_race_countdown_timer_timeout()
    {
        raceCountdown--;
        Hud.SetCountdown(raceCountdown.ToString());
        if (raceCountdown > 0) return;

        Hud.SetCountdown("");
        Timer.Stop();
        Timer.QueueFree();

        GameManager.Instance.SetGameState(GameManager.State.Racing);
    }
}
