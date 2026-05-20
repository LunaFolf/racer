using Godot;
using System;

public partial class Racer : CharacterBody2D
{
	[Flags]
	private enum DebugMode
	{
		TargetPos = 1 << 1,
		Label = 1 << 2
	}

	[Export] public float MaxAccelSpeed = 200.0f;
	private float upgradeSpeedMultiplier = 1f;
	[Export] private float _actualMaxAccelSpeed;
	[Export] public float RotationSpeed = 3.0f;
	private float upgradeRotMultiplier = 1f;
	[Export] public float Acceleration = 200.0f;
	private float upgradeAccelMultiplier = 1f;
	[Export] public float Deceleration = 400.0f;
	[Export] public CarParticleSystem CarParticleSystem;
	[Export] private Color _racerColor = new (GD.Randf(), GD.Randf(), GD.Randf());
	[Export] private DebugMode _currentDebugMode;
	[Export] private Label _debugLabel;
	[Export] private Node2D _debugTargetPos;
	[Export] public AudioStreamPlayer2D RacingSFX;

	[Export] public GpuParticles2D Explosion;
	[Export] public Polygon2D Sprite;
	[Export] public int NumberOfGoals { get; set; }
	[Signal] public delegate void GoalEnteredEventHandler(int goalNumber);

	private Goal _goal;
	private Vector2 _targetPos;
	private double _splitTime;
	private double _stageTime;
	private int _lapCounter = 1;
	private int _goalCounter = 1;
	private RaceScene _raceScene;
	private float laneOffset;

	public int RacerNumber;
	private int _racePosition;
	public int RacePosition
	{
		get => _racePosition;
		set
		{
			_racePosition = value;
			_actualMaxAccelSpeed = (MaxAccelSpeed * upgradeSpeedMultiplier) + _racePosition * 20;
		}
	}

	public void ExplosionAnimation()
	{
		Explosion.Restart();
		Explosion.Emitting = true;
		Sprite.Visible = false;
		CarParticleSystem.Visible = false;
	}

	public void Reset()
	{
		_goalCounter = 1;
		_splitTime = 0;
		_stageTime = 0;
		Velocity = Vector2.Zero;
		MoveAndSlide();
		FindGoal();
	}

	public void SetRaceScene(RaceScene scene)
	{
		_raceScene = scene;
	}

	public override void _Ready()
	{
		upgradeSpeedMultiplier += GameManager.Instance.BotUpgradeMults.Speed;
		upgradeRotMultiplier += GameManager.Instance.BotUpgradeMults.Turning;
		upgradeAccelMultiplier += GameManager.Instance.BotUpgradeMults.Traction;

		_actualMaxAccelSpeed = MaxAccelSpeed * upgradeSpeedMultiplier;

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
		if (IsQueuedForDeletion()) return;
		_splitTime += delta;
		_stageTime += delta;
	}

	public void FindGoal()
	{
		Goal goal = GetTree().GetRoot().GetNodeOrNull<Goal>("RaceScene/Goals/Goal" + _goalCounter);

		if (goal != null)
		{
			GD.Print("Goal found!");
			_goal = goal;
			_targetPos = _goal.GlobalPosition; // TODO: Randomise a position on the goal, so each car races a little differently
			Vector2 dir = _goal.GlobalTransform.X;

			if (laneOffset == 0)
			{
                var goalWidth = _goal.Width - 8;
                laneOffset = GD.RandRange(-(goalWidth / 4), goalWidth / 4) * 2;
            }

			_targetPos += dir * laneOffset;
			if ((_currentDebugMode & DebugMode.TargetPos) != 0 && _debugTargetPos != null) _debugTargetPos.Position = _targetPos;

			GD.Print("Target: " + _targetPos);
		}
		else _goal = null;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (IsQueuedForDeletion()) return;
		string debugText = "";
		if ((_currentDebugMode & DebugMode.Label) != 0 && _debugLabel != null) debugText = "Racer " + RacerNumber + " " + _goalCounter + "/" + NumberOfGoals;
		Vector2 velocity = Velocity;

		if (GameManager.Instance.GameState != GameManager.State.Racing) return;

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
				velocity = velocity.MoveToward(forward * _actualMaxAccelSpeed * accel,
					(Acceleration * upgradeAccelMultiplier) * (float)delta);
			}
			else
			{
				velocity = velocity.MoveToward(Vector2.Zero, (Deceleration * upgradeAccelMultiplier) * (float)delta);
			}

			if (rot != 0 && !velocity.IsZeroApprox())
			{
				float forwardSpeed = velocity.Dot(GlobalTransform.Y);
				float actualRotSpeed = 2 + (Math.Abs(forwardSpeed) / _actualMaxAccelSpeed) *
					(RotationSpeed * upgradeRotMultiplier);
				Rotate(rot * actualRotSpeed * (float)delta);
			}

			float speedPercent = velocity.Length() / _actualMaxAccelSpeed;

			RacingSFX.VolumeDb = Math.Min(-20, -80 + (60 * speedPercent));

            CarParticleSystem.ThrusterSpeed = speedPercent;
            CarParticleSystem.ThrusterAngle = RotationDegrees;

            //CarParticleSystem.DebrisParticles.AmountRatio = speedPercent;
            //CarParticleSystem.TireProcessMaterial.Gravity = new Vector3(GlobalTransform.Y.X * 94, GlobalTransform.Y.Y * 94, 0);

            float driftPercent = Math.Abs(velocity.Dot(GlobalTransform.X)) / _actualMaxAccelSpeed;
			float tireMarkLifetime = Math.Max(0.01f, driftPercent);

			//CarParticleSystem.LeftTireParticles.Lifetime = tireMarkLifetime;
			//CarParticleSystem.RightTireParticles.Lifetime = tireMarkLifetime;

			if ((_currentDebugMode & DebugMode.Label) != 0 && _debugLabel != null) debugText += "\n" + "Accel: " + accel + "\nRot: " + rot + "\nSpeed: " + velocity.Length() + "";
		}

		if ((_currentDebugMode & DebugMode.Label) != 0 && _debugLabel != null) _debugLabel.Text = debugText;

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

	public void _on_goal_entered(int goalNumber)
	{
		if (goalNumber != _goalCounter) return;
		_goalCounter++;

		_raceScene.SetSplitTime(RacerNumber, _splitTime);
		_splitTime = 0;

		if (_goalCounter >= NumberOfGoals)
		{
			_lapCounter++;

			_goalCounter = 1;
			_raceScene.SetStageTime(RacerNumber, _stageTime);
			_raceScene.SetRacerLap(RacerNumber);
			_stageTime = 0;
			FindGoal();
			return;
		}

		if (_lapCounter >= 2)
		{
			_raceScene.EndRace(false);
			return;
		}

		_raceScene.SetRacerGoal(RacerNumber, _goalCounter);

		FindGoal();
	}
}
