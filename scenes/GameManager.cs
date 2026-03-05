using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GameManager : Node2D
{
    [Export] public HUD Hud;
    [Export] public GoalManager GoalManager;
    [Export] public RacerManager RacerManager;
    private Godot.Collections.Dictionary<int, double> StageTime = new() { [0] = 0 };
    private Godot.Collections.Dictionary<int, double> SplitTime = new() { [0] = 0 };
    private Godot.Collections.Dictionary<int, string> RacerNames = new() { [0] = "Player" };
    private Godot.Collections.Dictionary<int, int> RacerLaps = new() { [0] = 0 };
    private Godot.Collections.Dictionary<int, int> RacerGoals = new() { [0] = 1 };
    private Godot.Collections.Dictionary<int, Node> Racers = new();
    [Export] public Player Player;

    [Signal] public delegate void SetSplitTimeEventHandler(int racerId, double time);
    [Signal] public delegate void SetStageTimeEventHandler(int racerId, double time);
    [Signal] public delegate void SetRacerLapEventHandler(int racerId);
    [Signal] public delegate void SetRacerGoalEventHandler(int racerId, int goal);
    public override void _Ready()
    {
        GoalManager.StartGeneration();
        Player.NumberOfGoals = GoalManager.GoalCounter;
        Racers.Add(0, Player);
        RacerManager.MaxRacers = 9;
        RacerManager.GenerateRacers(GoalManager.GoalCounter);

        foreach (Racer racer in RacerManager.Racers)
        {
            Racers.Add(racer.RacerNumber, racer);

            StageTime.Add(racer.RacerNumber, 0);
            SplitTime.Add(racer.RacerNumber, 0);
            RacerNames.Add(racer.RacerNumber, "Racer " + racer.RacerNumber);

            RacerLaps.Add(racer.RacerNumber, 0);
            RacerGoals.Add(racer.RacerNumber, 1);
        }
    }

    public override void _Process(double delta)
    {
        UpdatePositionsList();
    }

    private void UpdatePositionsList()
    {
        string positions = "";

        int positionCounter = 0;

        var ordered = Racers.Keys
            .OrderByDescending(id => RacerLaps[id])
            .ThenByDescending(id => RacerGoals[id])
            .ThenBy(id => GoalManager.DistanceToGoal(
                ((CharacterBody2D)Racers[id]).GlobalPosition, RacerGoals[id] - 1
                )); // TODO: Sort by distance to next goal

        int playerPosition = 1;

        foreach (var racerId in ordered)
        {
            positionCounter++;
            if (racerId == 0) playerPosition = positionCounter;
            positions += positionCounter + ": " + RacerNames[racerId] + "\n";

            Racers[racerId].Set("RacePosition", positionCounter);
        }

        Hud.SetPositions(positions);
        Hud.SetRanking(playerPosition + " / " + (RacerManager.MaxRacers + 1));
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
}
