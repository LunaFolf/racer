using Godot;
using Godot.Collections;
using System;

public partial class Track : Node2D
{
	// Called when the node enters the scene tree for the first time.

	[Export] public Node2D Entrance;
	[Export] public Goal Exit;
	[Export] public Label DebugCoords;

	[Flags]
	public enum TrackDir
	{
		Top = 1 << 1,
		Right = 1 << 2,
		Bottom = 1 << 3,
		Left = 1 << 4
	}

	public enum TrackType
	{
		Start,
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

	public static TrackDir NextClockwise(TrackDir direction)
	{
		if (direction == TrackDir.Top) return TrackDir.Right;
		if (direction == TrackDir.Right) return TrackDir.Bottom;
		if (direction == TrackDir.Bottom) return TrackDir.Left;
		return TrackDir.Top;
	}

	public static TrackDir Opposite(TrackDir direction)
	{
		if (direction == TrackDir.Top) return TrackDir.Bottom;
		if (direction == TrackDir.Right) return TrackDir.Left;
		if (direction == TrackDir.Bottom) return TrackDir.Top;
		return TrackDir.Right;
	}

	public static TrackDir RotateDirection(TrackDir direction, TrackRotation rotation)
	{
		for (int i = 0; i < (int)rotation; i++)
		{
			direction = NextClockwise(direction);
		}

		return direction;
	}
}
