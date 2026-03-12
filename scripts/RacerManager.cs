using Godot;
using System;
using System.Collections.Generic;

public partial class RacerManager : Node
{
	[Export] public PackedScene RacerScene;
	[Export] public int MaxRacers = 1;
	private List<Racer> _racers = new ();
	public List<Racer> Racers => _racers;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

    }

	public void GenerateRacers(int numberOfGoals, RaceScene raceScene)
	{
		for (int i = 1; i <= MaxRacers; i++)
		{
			var newRacer = RacerScene.Instantiate<Racer>();
			newRacer.RacerNumber = i;
			newRacer.SetRaceScene(raceScene);
			newRacer.NumberOfGoals = numberOfGoals;
			newRacer.Position = new Vector2(0, 90 + (i - 1) * 45);
			AddChild(newRacer);

			_racers.Add(newRacer);
		}
	}
}
