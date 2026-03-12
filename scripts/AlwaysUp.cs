using Godot;
using System;

public partial class AlwaysUp : Node2D
{
    public override void _Ready()
    {
        GlobalRotation = 0;
    }
    public override void _PhysicsProcess(double delta)
    {
        if (IsQueuedForDeletion()) return;
        GlobalRotation = 0;
    }
}
