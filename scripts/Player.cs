using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float MaxAccelSpeed = 500.0f;
	[Export] public float RotationSpeed = 3.0f;
	[Export] public float Acceleration = 400.0f;
	[Export] public float Deceleration = 400.0f;

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
			float actualRotSpeed = 2 + (Math.Abs(velocity.Y) / MaxAccelSpeed) * RotationSpeed;
			Rotate(rot * actualRotSpeed * (float)delta);
		}

		// GD.Print(velocity);

		Velocity = velocity;
		MoveAndSlide();
	}
}
