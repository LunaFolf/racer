using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float MaxAccelSpeed = 500.0f;
	[Export] public float RotationSpeed = 3.0f;
	[Export] public float Acceleration = 400.0f;
	[Export] public float Deceleration = 400.0f;

	[Export] public Camera2D Camera;
	private static float _defaultCameraZoom = 1.5f;
	private static float _zoomedCameraZoom = _defaultCameraZoom + 1f;
	private float _currentZoom;

	[Export] public CarParticleSystem CarParticleSystem;

	public override void _Ready()
	{
		_currentZoom = _defaultCameraZoom;
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
			velocity = velocity.MoveToward(GlobalTransform.Y * MaxAccelSpeed * accel, Acceleration * (float)delta);
		}
		else
		{
			velocity = velocity.MoveToward(Vector2.Zero, Deceleration * (float)delta);
		}

		if (rot != 0 && !velocity.IsZeroApprox())
		{
			float forwardSpeed = velocity.Dot(GlobalTransform.Y);
			float actualRotSpeed = 2 + (Math.Abs(forwardSpeed) / MaxAccelSpeed) * RotationSpeed;
			Rotate(rot * actualRotSpeed * (float)delta);
		}

		float speedPercent = velocity.Length() / MaxAccelSpeed;
		float targetZoom = _zoomedCameraZoom - speedPercent;
		_currentZoom = Mathf.Lerp(_currentZoom, targetZoom, 5f * (float)delta);
		Camera.Zoom = new Vector2(_currentZoom, _currentZoom);

		CarParticleSystem.DebrisParticles.AmountRatio = speedPercent;
		CarParticleSystem.TireProcessMaterial.Gravity = new Vector3(GlobalTransform.Y.X * 94, GlobalTransform.Y.Y * 94, 0);

		float driftPercent = Math.Abs(velocity.Dot(GlobalTransform.X)) / MaxAccelSpeed;
		float tireMarkLifetime = Math.Max(0.01f, driftPercent);

		CarParticleSystem.LeftTireParticles.Lifetime = tireMarkLifetime;
		CarParticleSystem.RightTireParticles.Lifetime = tireMarkLifetime;

		// GD.Print(velocity);

		Velocity = velocity;
		MoveAndSlide();
	}
}
