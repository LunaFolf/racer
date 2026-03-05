using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float MaxAccelSpeed = 500.0f;
	[Export] private float _actualMaxAccelSpeed;
	[Export] public float RotationSpeed = 3.0f;
	[Export] public float Acceleration = 400.0f;
	[Export] public float Deceleration = 400.0f;

	[Export] public Camera2D Camera;
	private static float _defaultCameraZoom = 1.5f;
	private static float _zoomedCameraZoom = _defaultCameraZoom + 1f;
	private float _currentZoom;

	[Export] public GameManager GameManager;
	[Export] public HUD Hud;
	[Export] public CarParticleSystem CarParticleSystem;
	[Signal] public delegate void GoalEnteredEventHandler(int goalNumber);

	private int _racePosition;
	public int RacePosition
	{
		get => _racePosition;
		set
		{
			_racePosition = value;
			_actualMaxAccelSpeed = MaxAccelSpeed + (_racePosition - 1) * 20;
		}
	}

	private double _splitTime = 0;
	private double _stageTime = 0;
	private int _lapCounter = 1;
	private int _goalCounter = 1;
	[Export] public int NumberOfGoals { get; set; }

	public override void _Ready()
	{
		_actualMaxAccelSpeed = MaxAccelSpeed;
		_currentZoom = _defaultCameraZoom;
	}

	public override void _Process(double delta)
	{
		_splitTime += delta;
		_stageTime += delta;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		float accel = Input.GetAxis("accelerate", "decelerate");
		float rot = Input.GetAxis("left", "right");

		if (accel != 0)
		{
			velocity = velocity.MoveToward(GlobalTransform.Y * _actualMaxAccelSpeed * accel, Acceleration * (float)delta);
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

		if (Camera != null)
		{
			float targetZoom = _zoomedCameraZoom - Mathf.Clamp(speedPercent, 0, 1);
			_currentZoom = Mathf.Lerp(_currentZoom, targetZoom, 5f * (float)delta);
			Camera.Zoom = new Vector2(_currentZoom, _currentZoom);
		}

		CarParticleSystem.DebrisParticles.AmountRatio = speedPercent;
		CarParticleSystem.TireProcessMaterial.Gravity = new Vector3(GlobalTransform.Y.X * 94, GlobalTransform.Y.Y * 94, 0);

		float driftPercent = Math.Abs(velocity.Dot(GlobalTransform.X)) / _actualMaxAccelSpeed;
		float tireMarkLifetime = Math.Max(0.01f, driftPercent);

		CarParticleSystem.LeftTireParticles.Lifetime = tireMarkLifetime;
		CarParticleSystem.RightTireParticles.Lifetime = tireMarkLifetime;

		Velocity = velocity;
		MoveAndSlide();
	}

	public void _on_goal_entered(int goalNumber)
	{
		if (goalNumber != _goalCounter) return;
		_goalCounter++;

		GameManager.EmitSignal("SetSplitTime", 0, _splitTime);
		_splitTime = 0;

		if (_goalCounter > NumberOfGoals)
		{
			_lapCounter++;

			_goalCounter = 1;
			GameManager.EmitSignal("SetStageTime", 0, _stageTime);
			GameManager.EmitSignal("SetRacerLap", 0);
			_stageTime = 0;
		}

		GameManager.EmitSignal("SetRacerGoal", 0, _goalCounter);
	}
}
