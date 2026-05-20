using Godot;
using System;

public partial class MainCamera : Camera2D
{
	// Called when the node enters the scene tree for the first time.
	public float shake = 0;
	private int noise_y;
	public HUD Hud;

	private FastNoiseLite _noise = new FastNoiseLite();

	public override void _Ready()
	{
		GameManager.Instance.MainCamera = this;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (shake <= 0f)
		{
			shake = 0;
			Offset = new Vector2 (0, 0);
			Hud.MainHUDGroup.Position = Offset;
            return;
        }
		noise_y += 1;

		float amount = (float)Math.Pow(shake, 3);
		Rotation = 0.1f * amount * _noise.GetNoise2D(0, noise_y);
		var x = 100 * amount * _noise.GetNoise2D(1000, noise_y);
		var y = 75 * amount * _noise.GetNoise2D(2000, noise_y);

		Offset = new Vector2(x, y);
        Hud.MainHUDGroup.Position = Offset;
    }
}
