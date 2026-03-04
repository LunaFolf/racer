using Godot;
using System;

public partial class CarParticleSystem : Node2D
{
	[Export] public GpuParticles2D DebrisParticles;
	[Export] public GpuParticles2D LeftTireParticles, RightTireParticles;
	[Export] public ParticleProcessMaterial TireProcessMaterial;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
