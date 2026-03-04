using Godot;
using System;

public partial class Racer : CharacterBody2D
{
	// Called when the node enters the scene tree for the first time.
	[Export] public float MaxAccelSpeed = 200.0f;
	[Export] private float _actualMaxAccelSpeed;
	[Export] public float RotationSpeed = 3.0f;
	[Export] public float Acceleration = 200.0f;
	[Export] public float Deceleration = 400.0f;

	[Export] public CarParticleSystem CarParticleSystem;

	[Export] public int NumberOfGoals { get; set; }

	[Signal] public delegate void GoalEnteredEventHandler(int goalNumber);

	private Area2D _goal;
	private Vector2 _targetPos;
	private int _goalCounter = 1;
	public override void _Ready()
	{
		FindGoal();
		GD.Print("RacerController Ready!");
		GD.Print("Goal: " + _goal);
	}

	private void FindGoal()
	{
		Area2D goal = GetTree().GetRoot().GetNodeOrNull<Area2D>("Game/Goals/Goal" + _goalCounter);

		if (goal != null)
		{
			GD.Print("Goal found!");
			_goal = goal;
			_targetPos = _goal.GlobalPosition; // TODO: Randomise a position on the goal, so each car races a little differently
			// float offset = 24f * (float)GD.RandRange(-1f, 1f);
			Vector2 dir = _goal.GlobalTransform.X;

			float laneOffset = GD.RandRange(-8, 8) * 2;
			_targetPos += dir * laneOffset;

			GD.Print("Target: " + _targetPos);
		}
		else
		{
			// Error out, can't race without a goal!
			// GD.PrintErr("No goal found!");
			_goal = null;
		}

		_actualMaxAccelSpeed = MaxAccelSpeed;
		// _actualMaxAccelSpeed = MaxAccelSpeed - (GD.Randf() - .5f) * 100;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		if (_goal != null)
		{
			Vector2 forward = -GlobalTransform.Y;

			Vector2 directionToGoal = (_targetPos - GlobalPosition).Normalized();
			float dot = forward.Dot(directionToGoal);
			float cross = forward.Cross(directionToGoal);
			float rot = Mathf.Clamp(cross, -1f, 1f);
			float accel = Mathf.Clamp(dot, 0f, 1f);

			accel *= Mathf.Clamp(dot, 0.3f, 1f); // Slow down when not facing the goal.
			// if (GlobalPosition.DistanceTo(_targetPos) < 100f)
			// {
			// 	accel *= 0.2f;
			// }

			if (accel > 0.01f)
			{
				velocity = velocity.MoveToward(forward * _actualMaxAccelSpeed * accel, Acceleration * (float)delta);
			}
			// else
			// {
			// 	velocity = velocity.MoveToward(Vector2.Zero, Deceleration * (float)delta);
			// }

			if (rot != 0 && !velocity.IsZeroApprox())
			{
				float forwardSpeed = velocity.Dot(GlobalTransform.Y);
				float actualRotSpeed = 2 + (Math.Abs(forwardSpeed) / MaxAccelSpeed) * RotationSpeed;
				Rotate(rot * actualRotSpeed * (float)delta);
			}

			float speedPercent = velocity.Length() / MaxAccelSpeed;
			CarParticleSystem.DebrisParticles.AmountRatio = speedPercent;
			CarParticleSystem.TireProcessMaterial.Gravity = new Vector3(GlobalTransform.Y.X * 94, GlobalTransform.Y.Y * 94, 0);

			float driftPercent = Math.Abs(velocity.Dot(GlobalTransform.X)) / MaxAccelSpeed;
			float tireMarkLifetime = Math.Max(0.01f, driftPercent);

			CarParticleSystem.LeftTireParticles.Lifetime = tireMarkLifetime;
			CarParticleSystem.RightTireParticles.Lifetime = tireMarkLifetime;
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	public void _on_goal_entered(int goalNumber)
	{
		GD.Print("I entered goal " + goalNumber + " out of " + NumberOfGoals + "");
		if (goalNumber != _goalCounter) return;
		_goalCounter++;
		if (_goalCounter > NumberOfGoals) _goalCounter = 1; // For debugging :3
		GD.Print("Moving to next goal: " + _goalCounter + "");
		FindGoal();
	}
}
