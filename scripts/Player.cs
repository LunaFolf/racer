using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float MaxAccelSpeed = 500.0f;
	private float upgradeSpeedMultiplier = 1f;
	private float _actualMaxAccelSpeed;
	[Export] public float RotationSpeed = 3.0f;
	[Export] public float Acceleration = 400.0f;
	[Export] public float Deceleration = 400.0f;
	[Export] public AlwaysUp BackgroundSprite;
	[Export] public HUD Hud;
	[Export] public CarParticleSystem CarParticleSystem;
	[Export] public Camera2D Camera;

	private static float _defaultCameraZoom = 1f;
	private static float _zoomedCameraZoom = _defaultCameraZoom + 1f;
	private float _currentZoom;
	private double _splitTime;
	private double _stageTime;
	private int _lapCounter = 1;
	private int _goalCounter = 1;
	private float bgOffset = 10f;

	[Signal] public delegate void GoalEnteredEventHandler(int goalNumber);

	private RaceScene _raceScene;

	private int _racePosition;
	public int RacePosition
	{
		get => _racePosition;
		set
		{
			_racePosition = value;
			_actualMaxAccelSpeed = (MaxAccelSpeed * upgradeSpeedMultiplier) + (_racePosition - 1) * 10;
		}
	}

	public void Reset()
	{
		_lapCounter = 1;
		_goalCounter = 1;
		_splitTime = 0;
		_stageTime = 0;
		Velocity = Vector2.Zero;
		MoveAndSlide();
	}

	public void SetRaceScene(RaceScene scene)
	{
		_raceScene = scene;
	}

	[Export] public int NumberOfGoals { get; set; }

	public override void _Ready()
	{
		foreach (var upgrade in GameManager.Instance.PlayerUpgrades)
		{
			GD.Print("upgrade ", upgrade);
			if (upgrade.type == PlayerUpgrade.Type.SPEED)
			{
				upgradeSpeedMultiplier += upgrade.multiplier;
			}
		}

        _actualMaxAccelSpeed = MaxAccelSpeed * upgradeSpeedMultiplier;

        _currentZoom = _defaultCameraZoom;

		GD.Print(MaxAccelSpeed, _actualMaxAccelSpeed);
    }

	public override void _Process(double delta)
	{
		if (IsQueuedForDeletion()) return;
		_splitTime += delta;
		_stageTime += delta;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (IsQueuedForDeletion()) return;
        if (GameManager.Instance.GameState != GameManager.State.Racing) return;
        
		Vector2 velocity = Velocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		float accel = Input.GetAxis("accelerate", "decelerate");
		float rot = Input.GetAxis("left", "right");

		if (accel != 0)
		{
			var oldVelocity = velocity;
			velocity = velocity.MoveToward(GlobalTransform.Y * _actualMaxAccelSpeed * accel, Acceleration * (float)delta);
			// TODO: Calculate drift lag and offset by traction control upgrades
			GD.Print(Math.Abs(velocity.Aspect()));
		}
		else
		{
			velocity = velocity.MoveToward(Vector2.Zero, Deceleration * (float)delta);
		}

		float actualRotSpeed = 0;

		if (rot != 0 && !velocity.IsZeroApprox())
		{
			float forwardSpeed = velocity.Dot(GlobalTransform.Y);
			actualRotSpeed = 2 + (Math.Abs(forwardSpeed) / _actualMaxAccelSpeed) * RotationSpeed;
			Rotate(rot * actualRotSpeed * (float)delta);
		}

		float speedPercent = velocity.Length() / _actualMaxAccelSpeed;

		//var bgPos = BackgroundSprite.GlobalPosition;
		//bgPos.Y = speedPercent * bgOffset;
		//bgPos.X = actualRotSpeed * bgOffset;
  //      BackgroundSprite.GlobalPosition = bgPos;

		CarParticleSystem.ThrusterSpeed = speedPercent;
		CarParticleSystem.ThrusterAngle = RotationDegrees;

		float vibrationStrength = 0.5f;
		float weakVibration = Math.Clamp(speedPercent, 0, 1) * vibrationStrength;
		float strongVibration = Math.Clamp(actualRotSpeed - 2, 0, 1) * vibrationStrength;

		Input.StartJoyVibration(0, weakVibration, strongVibration, 0);

		if (Camera != null)
		{
			float targetZoom = _zoomedCameraZoom - Mathf.Clamp(speedPercent, 0, 1);
			_currentZoom = Mathf.Lerp(_currentZoom, targetZoom, 5f * (float)delta);
			Camera.Zoom = new Vector2(_currentZoom, _currentZoom);
		}

		//CarParticleSystem.DebrisParticles.AmountRatio = speedPercent;
		//CarParticleSystem.TireProcessMaterial.Gravity = new Vector3(GlobalTransform.Y.X * 94, GlobalTransform.Y.Y * 94, 0);

		float driftPercent = Math.Abs(velocity.Dot(GlobalTransform.X)) / _actualMaxAccelSpeed;
		float tireMarkLifetime = Math.Max(0.01f, driftPercent);

		//CarParticleSystem.LeftTireParticles.Lifetime = tireMarkLifetime;
		//CarParticleSystem.RightTireParticles.Lifetime = tireMarkLifetime;

		Velocity = velocity;
		MoveAndSlide();
	}

	public void _on_goal_entered(int goalNumber)
	{
		if (goalNumber != _goalCounter) return;
		_goalCounter++;

		_raceScene.SetSplitTime(0, _splitTime);
		_splitTime = 0;

		if (_goalCounter >= 2 && _lapCounter >= 2)
		{
			// Race Finished
			_raceScene.EndRace();
		}
		if (_goalCounter > NumberOfGoals)
		{
			_lapCounter++;

			_goalCounter = 1;
			_raceScene.SetStageTime(0, _stageTime);
			_raceScene.SetRacerLap(0);
			_stageTime = 0;
			return;
		}

		_raceScene.SetRacerGoal(0, _goalCounter);
	}
}
