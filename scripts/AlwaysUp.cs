using Godot;
using System;

public partial class AlwaysUp : Node2D
{
    [Export] private bool _followCameraRotation = true;
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
        GlobalRotation = _camera == null ? 0 : _camera.GlobalRotation;
    }
}
