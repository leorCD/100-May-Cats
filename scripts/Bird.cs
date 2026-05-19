using Godot;
using System;

public partial class Bird : Sprite3D
{
    public override void _Ready()
    {
        AnimationPlayer animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        GD.Randomize();
        GetTree().CreateTimer((float)GD.RandRange(1, 20)/10f).Timeout += () =>
        {
            animPlayer.Play("default");
        };
    }
}
