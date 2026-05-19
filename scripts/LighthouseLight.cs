using Godot;
using System;

[Tool]
public partial class LighthouseLight : Node3D
{
    public override void _Ready()
    {
        SetProcess(true);
    }


    public async override void _Process(double delta)
    {
        Vector3 rot = RotationDegrees;
        rot.Y += 15f * (float)delta;
        RotationDegrees = rot;
    }
}
