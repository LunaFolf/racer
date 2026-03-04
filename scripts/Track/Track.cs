using Godot;
using System;

public partial class Track : Node2D
{
	// Called when the node enters the scene tree for the first time.

	[Export] public Node2D Entrance;
	[Export] public Node2D Exit;

	public enum TrackDir
	{
		Top,
		Right,
		Bottom,
		Left
	}

	public enum TrackType
	{
		Straight,
		CornerCw,
		CornerCCw
	}

	public enum TrackRotation
	{
		Deg0,
		Deg90,
		Deg180,
		Deg270
	}

	public TrackDir NextClockwise(TrackDir direction)
	{
		if (direction == TrackDir.Top) return TrackDir.Right;
		if (direction == TrackDir.Right) return TrackDir.Bottom;
		if (direction == TrackDir.Bottom) return TrackDir.Left;
		return TrackDir.Top;
	}

	public TrackDir RotateDirection(TrackDir direction, TrackRotation rotation)
	{
		for (int i = 0; i < (int)rotation; i++)
		{
			direction = NextClockwise(direction);
		}

		return direction;
	}
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
