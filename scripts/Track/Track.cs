using Godot;
using System;

public partial class Track : Node2D
{
	// Called when the node enters the scene tree for the first time.

	[Export] public Node2D Entrance;
	[Export] public Node2D Exit;
	[Export] public Label DebugCoords;

	private bool _debug = false;

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
	public override void _Ready()
	{
		if (!_debug) DebugCoords.QueueFree();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (!_debug) return;
		DebugCoords.SetPosition(GlobalPosition - new Vector2(90, 90));
		DebugCoords.SetText(GlobalPosition.X / 500 + ", " + -(GlobalPosition.Y / 500));
	}
}
