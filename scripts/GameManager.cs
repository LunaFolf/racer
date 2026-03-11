using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

// Notes
// 1. Collisions are fucked lol
// 2. Need markers on floor for when a corner is coming
// 3. Minimap?
// 4. Ramps and Obstacles?

public partial class GameManager : Node2D
{
    public static GameManager Instance;

    public enum State
    {
        MainMenu,
        Paused,
        Starting,
        Racing,
        Ending,
        Finished,
        Shop
    }

    private State _gameState = State.MainMenu;
    public State GameState => _gameState;

    [Export] public HUD Hud;
    [Export] public GoalManager GoalManager;
    [Export] public RacerManager RacerManager;
    private Godot.Collections.Dictionary<int, double> StageTime = new() { [0] = 0 };
    private Godot.Collections.Dictionary<int, double> SplitTime = new() { [0] = 0 };
    private Godot.Collections.Dictionary<int, int> RacerLaps = new() { [0] = 0 };
    private Godot.Collections.Dictionary<int, int> RacerGoals = new() { [0] = 1 };
    private Godot.Collections.Dictionary<int, CharacterBody2D> Racers = new();
    [Export] public Player Player;

    [Signal] public delegate void SetSplitTimeEventHandler(int racerId, double time);
    [Signal] public delegate void SetStageTimeEventHandler(int racerId, double time);
    [Signal] public delegate void SetRacerLapEventHandler(int racerId);
    [Signal] public delegate void SetRacerGoalEventHandler(int racerId, int goal);
    [Signal] public delegate void EndRaceEventHandler();

    private int _playerRacePosition = 0;
    public int PlayerRacePosition => _playerRacePosition;

    private int raceCountdown = 5;
    [Export] Timer RaceCountdownTimer;
    public override void _Ready()
    {
        Instance = this;
        GD.Print("GameManager Ready!");
    }

    public void GenerateRace()
    {
        GD.Print("starting goal gen");
        GoalManager.StartGeneration();
        GD.Print("goal gen done");


        Hud.Minimap.SetMap(GoalManager.TrackPoints);
        Player.NumberOfGoals = GoalManager.GoalCounter;
        Racers.Add(0, Player);
        RacerManager.MaxRacers = 9;
        RacerManager.GenerateRacers(GoalManager.GoalCounter);

        foreach (Racer racer in RacerManager.Racers)
        {
            Racers.Add(racer.RacerNumber, racer);

            StageTime.Add(racer.RacerNumber, 0);
            SplitTime.Add(racer.RacerNumber, 0);
            RacerLaps.Add(racer.RacerNumber, 0);
            RacerGoals.Add(racer.RacerNumber, 1);
        }
    }

    public void StartRaceCountdown()
    {
        RaceCountdownTimer.Start();
    }

    public void SetGameState(State state)
    {
        _gameState = state;
    }

    public void RemoveRacer(int racerNumber)
    {
        Racers.Remove(racerNumber);
    }

    public override void _Process(double delta)
    {
        if (GameState != State.Racing) return;
        UpdatePositionsList();
    }

    private void UpdatePositionsList()
    {
        int positionCounter = 0;

        var ordered = Racers.Keys
            .OrderByDescending(id => RacerLaps[id])
            .ThenByDescending(id => RacerGoals[id])
            .ThenBy(id => GoalManager.DistanceToGoal(
                Racers[id].GlobalPosition, RacerGoals[id] - 1
                ));

        foreach (var racerId in ordered)
        {
            if (racerId == 0) _playerRacePosition = positionCounter + 1;

            Racers[racerId].Set("RacePosition", positionCounter);

            positionCounter++;
        }

        Hud.SetPosition(_playerRacePosition + " / " + (RacerManager.MaxRacers + 1));
    }

    public void _on_set_split_time(int racerId, double time)
    {
        SplitTime[racerId] = time;
        StageTime[racerId] += time;
        UpdatePositionsList();
    }
    public void _on_set_stage_time(int racerId, double time)
    {
        StageTime[racerId] = time;
        UpdatePositionsList();
    }

    public void _on_set_racer_lap(int racerId)
    {
        RacerLaps[racerId] += 1;
    }
    public void _on_set_racer_goal(int racerId, int goal)
    {
        RacerGoals[racerId] = goal;
    }

    public void StartRace()
    {
        GD.Print("Starting Race (Switching Scene)");
        var nextScene = (PackedScene)ResourceLoader.Load("res://scenes/game.tscn");
        GetTree().ChangeSceneToPacked(nextScene);
        RaceCountdownTimer = new Timer();
        RaceCountdownTimer.WaitTime = 1;
        GD.Print("Goal Manager", GoalManager);
        GenerateRace();
        StartRaceCountdown();
    }

    public void _on_end_race()
    {
        SetGameState(State.Ending);
        foreach (var racerId in Racers.Keys)
        {
            Racers.Remove(racerId);
        }

        GetTree().ChangeSceneToFile("res://scenes/shop.tscn");
    }

    public void _on_race_countdown_timer_timeout()
    {
        raceCountdown--;
        Hud.SetCountdown(raceCountdown.ToString());
        if (raceCountdown > 0) return;

        Hud.SetCountdown("");
        RaceCountdownTimer.Stop();
        RaceCountdownTimer.QueueFree();

        SetGameState(State.Racing);
    }
}
