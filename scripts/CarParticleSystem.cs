using Godot;
using System;

public partial class CarParticleSystem : Node2D
{
	[Export] public GpuParticles2D ThrusterParticles;
	public float ThrusterSpeed;
	public float ThrusterAngle;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ThrusterParticles.ProcessMaterial = (ParticleProcessMaterial)ThrusterParticles.ProcessMaterial.Duplicate(true);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		ParticleProcessMaterial Material = (ParticleProcessMaterial)ThrusterParticles.ProcessMaterial;
		Material.InitialVelocityMin = 98 * ThrusterSpeed;
		Material.InitialVelocityMax = 98 * ThrusterSpeed;

		Material.AngleMax = ThrusterAngle;
		Material.AngleMin = ThrusterAngle;

		ThrusterParticles.ProcessMaterial = Material;

    }
}
