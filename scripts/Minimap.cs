using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;

public partial class Minimap : Node2D
{
	[Export] public Vector2[] TrackPoints = [];
	[Export] public Line2D TrackLine;
	[Export] public Polygon2D PlayerDot;
	[Export] public Sprite2D FinishLine;
	[Export] public float RotationSpeed = 2f;
	private Player _player;
	private List<Polygon2D> RacerDots;
	private RacerManager RaceManager;
	private Vector2 _startingPoint;
	private float _scale;

	private float _targetMapSize = 192;

	public override void _Ready()
	{
		UpdateLine();
	}

	public void SetPlayer(Player player)
	{
		_player = player;
	}

	public void SetRaceManager(RacerManager RaceManager)
	{
		this.RaceManager = RaceManager;
		SetRacers();
	}

	public void SetRacers()
	{
		RacerDots = new();

		foreach (var racer in RaceManager.Racers)
		{
			var color = racer.Modulate;
			color.A = .75f;

			var sprite = PlayerDot.Duplicate() as Polygon2D;
			sprite.Name = racer.Name;
			sprite.Modulate = color;

			TrackLine.AddChild(sprite);
			RacerDots.Add(sprite);
		}
	}

	public void SetMap(Vector2[] points)
	{
		TrackPoints = points;
		UpdateLine();
	}

	public override void _PhysicsProcess(double delta)
	{
		

		var trackLength = 500;
		var playerMovementScale = trackLength / _scale;

		if (_player != null)
		{
			// RotationDegrees = Mathf.Lerp(RotationDegrees, -_player.RotationDegrees, (float)delta * RotationSpeed);
			PlayerDot.Position = _startingPoint + (_player.Position / playerMovementScale);
			PlayerDot.RotationDegrees = _player.RotationDegrees;
			RotationDegrees = -_player.RotationDegrees;
		}

		if (RaceManager.Racers != null)
		{
			foreach (var racer in RaceManager.Racers)
			{
				var index = racer.RacerNumber - 1;
				RacerDots[index].Position = _startingPoint + (racer.Position / playerMovementScale);
				RacerDots[index].RotationDegrees = racer.RotationDegrees;
			}
		}

		
	}

	private void UpdateLine()
	{
		TrackLine.ClearPoints();

		float smallX = float.MaxValue, smallY = float.MaxValue;
		float bigX = float.MinValue, bigY = float.MinValue;

		foreach (var point in TrackPoints)
		{
			if (point.X < smallX) smallX = point.X;
			if (point.Y < smallY) smallY = point.Y;

			if (point.X > bigX) bigX = point.X;
			if (point.Y > bigY) bigY = point.Y;
		}

		var pointWidth = bigX - smallX;
		var pointHeight = bigY - smallY;

		var xMult = _targetMapSize / pointWidth;
		var yMult = _targetMapSize / pointHeight;
		_scale = Mathf.Min(xMult, yMult);

		var first = true;
		foreach (var point in TrackPoints)
		{
			var normalized = new Vector2(point.X - smallX, -(point.Y - smallY));
			var scaled = normalized * _scale;

			TrackLine.AddPoint(scaled);

			if (!first) continue;
			first = false;
			_startingPoint = scaled;
		}

		FinishLine.Position = TrackLine.Points[1] + (TrackPoints[TrackPoints.Length - 1] / _scale);
	}
}
