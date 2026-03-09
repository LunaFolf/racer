using Godot;
using System;

public partial class CarParticleSystem : Node2D
{
	[Export] public GpuParticles2D DebrisParticles;
	[Export] public GpuParticles2D LeftTireParticles, RightTireParticles;
	[Export] private ParticleProcessMaterial _tireProcessMaterial;

	public ParticleProcessMaterial TireProcessMaterial;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		TireProcessMaterial = (ParticleProcessMaterial)_tireProcessMaterial.Duplicate(true);
		LeftTireParticles.ProcessMaterial = TireProcessMaterial;
		 RightTireParticles.ProcessMaterial = TireProcessMaterial;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
