using Godot;
using System;

public partial class AlwaysUp : Node2D
{
    [Export] private bool _followCameraRotation = true;
    [Export] public float RotationSpeed = .5f;
    private Camera2D _camera;
    public override void _Ready()
    {
        GlobalRotation = 0;
    }
    public override void _PhysicsProcess(double delta)
    {
        if (IsQueuedForDeletion()) return;
        if (!_followCameraRotation)
        {
            GlobalRotation = 0;
            return;
        }

        if (_camera == null && GameManager.Instance.MainCamera != null) _camera = GameManager.Instance.MainCamera;
        GlobalRotation = Mathf.Lerp(GlobalRotation, _camera.GlobalRotation, (float)delta * RotationSpeed);
    }
}
