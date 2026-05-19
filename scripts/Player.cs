using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float MaxAccelSpeed = 500.0f;
	private float upgradeSpeedMultiplier = 1f;
	private float _actualMaxAccelSpeed;
	[Export] public float RotationSpeed = 3.0f;
	private float upgradeRotMultiplier = 1f;
	[Export] public float Acceleration = 400.0f;
	private float upgradeAccelMultiplier = 1f;
	[Export] public float Deceleration = 400.0f;
	[Export] public AlwaysUp BackgroundSprite;
	[Export] public HUD Hud;
	[Export] public CarParticleSystem CarParticleSystem;
	[Export] public MainCamera Camera;

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
		Camera.Hud = Hud;
		upgradeSpeedMultiplier += GameManager.Instance.PlayerUpgradesMults.Speed;
		upgradeRotMultiplier += GameManager.Instance.PlayerUpgradesMults.Turning;
		upgradeAccelMultiplier += GameManager.Instance.PlayerUpgradesMults.Traction;

        _actualMaxAccelSpeed = MaxAccelSpeed * upgradeSpeedMultiplier;

        _currentZoom = _defaultCameraZoom;

		GD.Print(MaxAccelSpeed, _actualMaxAccelSpeed);
    }

	public override void _Process(double delta)
	{
		if (IsQueuedForDeletion()) return;
		_splitTime += delta;
		_stageTime += delta;

        CalculateScore();
    }

	public override void _PhysicsProcess(double delta)
	{
		if (IsQueuedForDeletion()) return;

		if (GameManager.Instance.GameState is not (GameManager.State.Racing or GameManager.State.Tutorial))
		{
			Hud.Bloom.SetShaderParameter("bloom_spread", 1 + GameManager.Instance.BeatBloom);
			return;
		}
        
		Vector2 velocity = Velocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		float accel = Input.GetAxis("accelerate", "decelerate");
		float rot = Input.GetAxis("left", "right");

		if (GameManager.Instance.GameState == GameManager.State.Tutorial)
		{
			var tutorialScene = GetParent<Tutorial>();

			GD.Print(tutorialScene.CurrentStage);

			if (tutorialScene.CurrentStage < Tutorial.Stage.Reverse) accel = Math.Min(0, accel);
			if (tutorialScene.CurrentStage < Tutorial.Stage.Turn) rot = 0;
		}

		if (accel != 0)
		{
			var oldVelocity = velocity;
			velocity = velocity.MoveToward(GlobalTransform.Y * _actualMaxAccelSpeed * accel,
				(Acceleration * upgradeAccelMultiplier) * (float)delta);
			// TODO: Calculate drift lag and offset by traction control upgrades
			//GD.Print(Math.Abs(velocity.Aspect()));
		}
		else
		{
			velocity = velocity.MoveToward(Vector2.Zero, (Deceleration * upgradeAccelMultiplier) * (float)delta);
		}

		float actualRotSpeed = 0;

		if (rot != 0 && !velocity.IsZeroApprox())
		{
			float forwardSpeed = velocity.Dot(GlobalTransform.Y);
			actualRotSpeed = 2 + (Math.Abs(forwardSpeed) / _actualMaxAccelSpeed) *
				(RotationSpeed * upgradeRotMultiplier);
			Rotate(rot * actualRotSpeed * (float)delta);
		}

		float speedPercent = velocity.Length() / MaxAccelSpeed;

		GameManager.Instance.MusicPlayer.VolumeDb = Math.Min(-20, -32 + 12 * speedPercent);
        Camera.shake = speedPercent - 1;
		Hud.Bloom.SetShaderParameter("bloom_spread", 1 + Math.Max(0, speedPercent - 1) + GameManager.Instance.BeatBloom * 2);
		Hud.Bloom.SetShaderParameter("bloom_intensity", 1 + Math.Max(0, speedPercent - 1) * 0.5);

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

		float driftPercent = Math.Abs(velocity.Dot(GlobalTransform.X)) / _actualMaxAccelSpeed;
		float tireMarkLifetime = Math.Max(0.01f, driftPercent);

		Velocity = velocity;
		var collide = MoveAndSlide();

		if (!collide) return;

		var move = false;

		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			var collision = GetSlideCollision(i);
			if (collision.GetCollider() is not (Player or Racer)) continue;
			move = true;
			Velocity = Velocity.Bounce(collision.GetNormal());
		}

		if (move) MoveAndSlide();
	}

	private void CalculateScore()
	{
		if (_raceScene == null) return;
		var distanceFromNextGoal = _raceScene.GoalManager.DistanceToGoal(Position, _goalCounter - 1);
		float score = (_goalCounter - 1) * 500; // Count number of goals passed
		score += (500 - distanceFromNextGoal); // Add distance travelled so far to next goal
		score -= 250; // offset starting goal being 750 instead of 500
		score += (_lapCounter - 1) * (NumberOfGoals * 500); // Add offset for laps

		GameManager.Instance.PlayerScore = (int)score;

		Hud.SetScoreText(Math.Max(score, 0));
	}

	public void _on_goal_entered(int goalNumber)
	{
		if (goalNumber != _goalCounter) return;
		_goalCounter++;

		_raceScene.SetSplitTime(0, _splitTime);
		_splitTime = 0;

		GD.Print("GoalCounter: " + _goalCounter + " | NumberOfGoals: " + NumberOfGoals);
		if (_goalCounter >= NumberOfGoals)
		{
			GD.Print("Lapped");
			_lapCounter++;

			_goalCounter = 1;
			_raceScene.SetStageTime(0, _stageTime);
			_raceScene.SetRacerLap(0);
			_stageTime = 0;
			return;
		}

        if (_lapCounter >= 2)
        {
            GD.Print("Game Ended");
            // Race Finished
            _raceScene.EndRace(true);
            return;
        }

        _raceScene.SetRacerGoal(0, _goalCounter);
	}
}
