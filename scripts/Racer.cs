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

	public int RacerNumber;

	private int _racePosition;
	public int RacePosition
	{
		get => _racePosition;
		set
		{
			_racePosition = value;
			_actualMaxAccelSpeed = MaxAccelSpeed + _racePosition * 20;
		}
	}

	[Export] public CarParticleSystem CarParticleSystem;

	[Export] private Color _racerColor = new (GD.Randf(), GD.Randf(), GD.Randf());

	[Signal] public delegate void GoalEnteredEventHandler(int goalNumber);

	[Flags]
	private enum DebugMode
	{
		TargetPos = 1 << 1,
		Label = 1 << 2
	}
	[Export] private DebugMode _currentDebugMode;
	[Export] private Label _debugLabel;
	[Export] private Node2D _debugTargetPos;

	private Goal _goal;
	private Vector2 _targetPos;
	private double _splitTime = 0;
	private double _stageTime = 0;
	private int _goalCounter = 1;
	[Export] public int NumberOfGoals { get; set; }

	public void Reset()
	{
		_goalCounter = 1;
		_splitTime = 0;
		_stageTime = 0;
		Velocity = Vector2.Zero;
		MoveAndSlide();
		FindGoal();
	}

	[Export] public GameManager GameManager;
	public override void _Ready()
	{
		FindGoal();
		GD.Print("RacerController Ready!");
		GD.Print("Goal: " + _goal);

		Modulate = _racerColor;

		if ((_currentDebugMode & DebugMode.Label) == 0)
		{
			_debugLabel.QueueFree();
		}
		else
		{
			_debugLabel.Modulate = new Color(1,1,1);
		}

		if ((_currentDebugMode & DebugMode.TargetPos) == 0)
		{
			_debugTargetPos.QueueFree();
		}
		else
		{
			_debugTargetPos.Modulate = _racerColor;
		}

	}

	public override void _Process(double delta)
	{
		_splitTime += delta;
		_stageTime += delta;
	}

	public void FindGoal()
	{
		Goal goal = GetTree().GetRoot().GetNodeOrNull<Goal>("Game/Goals/Goal" + _goalCounter);

		if (goal != null)
		{
			GD.Print("Goal found!");
			_goal = goal;
			_targetPos = _goal.GlobalPosition; // TODO: Randomise a position on the goal, so each car races a little differently
			Vector2 dir = _goal.GlobalTransform.X;

			var goalWidth = _goal.Width - 8;

			var laneOffset = GD.RandRange(-(goalWidth/4), goalWidth/4) * 2;
			_targetPos += dir * (float)laneOffset;
			if ((_currentDebugMode & DebugMode.TargetPos) != 0 && _debugTargetPos != null) _debugTargetPos.Position = _targetPos;

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
		string debugText = "";
		if ((_currentDebugMode & DebugMode.Label) != 0 && _debugLabel != null) debugText = "Racer " + RacerNumber + " " + _goalCounter + "/" + NumberOfGoals;
		Vector2 velocity = Velocity;

		if (_goal != null)
		{
			Vector2 forward = -GlobalTransform.Y;

			Vector2 directionToGoal = (_targetPos - GlobalPosition).Normalized();
			float dot = forward.Dot(directionToGoal);
			float cross = forward.Cross(directionToGoal);
			float rot = Mathf.Clamp(cross, -1f, 1f);
			float accel = Mathf.Clamp(dot, -1f, 1f);

			accel *= Mathf.Clamp(dot, 0.3f, 1f);
			accel *= Mathf.Clamp(1f - Mathf.Abs(rot), 0.3f, 1f);

			if (!Mathf.IsEqualApprox(accel, 0f))
			{
				velocity = velocity.MoveToward(forward * _actualMaxAccelSpeed * accel, Acceleration * (float)delta);
			}
			else
			{
				velocity = velocity.MoveToward(Vector2.Zero, Deceleration * (float)delta);
			}

			if (rot != 0 && !velocity.IsZeroApprox())
			{
				float forwardSpeed = velocity.Dot(GlobalTransform.Y);
				float actualRotSpeed = 2 + (Math.Abs(forwardSpeed) / _actualMaxAccelSpeed) * RotationSpeed;
				Rotate(rot * actualRotSpeed * (float)delta);
			}

			float speedPercent = velocity.Length() / _actualMaxAccelSpeed;
			CarParticleSystem.DebrisParticles.AmountRatio = speedPercent;
			CarParticleSystem.TireProcessMaterial.Gravity = new Vector3(GlobalTransform.Y.X * 94, GlobalTransform.Y.Y * 94, 0);

			float driftPercent = Math.Abs(velocity.Dot(GlobalTransform.X)) / _actualMaxAccelSpeed;
			float tireMarkLifetime = Math.Max(0.01f, driftPercent);

			CarParticleSystem.LeftTireParticles.Lifetime = tireMarkLifetime;
			CarParticleSystem.RightTireParticles.Lifetime = tireMarkLifetime;

			if ((_currentDebugMode & DebugMode.Label) != 0 && _debugLabel != null) debugText += "\n" + "Accel: " + accel + "\nRot: " + rot + "\nSpeed: " + velocity.Length() + "";
		}

		if ((_currentDebugMode & DebugMode.Label) != 0 && _debugLabel != null) _debugLabel.Text = debugText;

		Velocity = velocity;
		MoveAndSlide();
	}

	public void _on_goal_entered(int goalNumber)
	{
		if (goalNumber != _goalCounter) return;
		_goalCounter++;

		GameManager.EmitSignal("SetSplitTime", RacerNumber, _splitTime);
		_splitTime = 0;

		if (_goalCounter > NumberOfGoals)
		{
			_goalCounter = -1;
			GameManager.EmitSignal("SetStageTime", RacerNumber, _stageTime);
			GameManager.EmitSignal("SetRacerLap", RacerNumber);
			_stageTime = 0;
			QueueFree();
			return;
		}

		GameManager.EmitSignal("SetRacerGoal", RacerNumber, _goalCounter);

		FindGoal();
	}

	public void _on_tree_exiting()
	{
		GameManager.RemoveRacer(RacerNumber);
	}
}
