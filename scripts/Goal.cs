using Godot;
using System;

public partial class Goal : Area2D
{
    [Export] public int GoalNumber { get; set; }
    public void _on_body_entered(Node2D body)
    {
        GD.Print("Goal " + GoalNumber + " entered!");
        GD.Print(body);
        body.EmitSignal("GoalEntered", GoalNumber);
    }
}
