using Godot;
using System;

public partial class Camera : Camera2D
{
	private Node2D _parent;

	public override void _Ready()
	{
		_parent = GetParent<Node2D>();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_parent == null) return;

		Vector2 forward = _parent.Transform.Y.Normalized();
		Position = forward * 48;
	}
}
