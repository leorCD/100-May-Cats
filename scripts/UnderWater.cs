using Godot;
using System;

public partial class UnderWater : ColorRect
{
    [Export] private Camera3D mainCamera;

    public override void _Process(double delta)
    {
        if (mainCamera == null) return;
        
        Visible = (mainCamera.GlobalPosition.Y <= 1f) ? true : false;
    }

}
