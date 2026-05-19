using Godot;
using System;

public partial class CameraArm : SpringArm3D
{
    [Export] Node3D adornee = null;
    [Export] double mouse_sensitivity = 0.15;
    [Export] float yOffset = 0f;

    private float targetDistance;
    private float zoomSpeed = 6f;
    [Export] public float minDistance = 2.5f;
    [Export] public float maxDistance = 5f;
    
    public override void _Ready()
    {
        targetDistance = maxDistance;

        if (adornee != null)
            GlobalPosition = adornee.GlobalPosition + new Vector3(0, yOffset, 0);
    }

    public override void _Process(double delta)
    {
        SpringLength = Mathf.Lerp(SpringLength, targetDistance, (float)delta * zoomSpeed);
    }


    public override void _Input(InputEvent @event)
    {
        // pan camera
        if (Input.IsMouseButtonPressed(MouseButton.Right))
        {
            if (@event is InputEventMouseMotion mouseMotion)
            {
                Vector3 rotation = RotationDegrees;

                // clamp up and down rotation so you dont go in a full circle
                rotation.X -= (float)(mouseMotion.Relative.Y * mouse_sensitivity);
                rotation.X = Math.Clamp(rotation.X, -90f, 5f); 

                // wrap horizontal rotation so the numbers arent huge but stay within the circle (0, 360)
                rotation.Y -= (float)(mouseMotion.Relative.X * mouse_sensitivity);
                rotation.Y = Mathf.Wrap(rotation.Y, 0f, 360f);
                
                RotationDegrees = rotation;
            }
        }

        // zoom in and out
        if (@event is InputEventMouseButton mouseButton)
        {
            float zoomStep = (maxDistance / 50f);
            if (mouseButton.ButtonIndex == MouseButton.WheelDown)
                targetDistance = Mathf.Min(targetDistance + zoomStep, maxDistance);
            if (mouseButton.ButtonIndex == MouseButton.WheelUp)
                targetDistance = Mathf.Max(targetDistance - zoomStep, minDistance);
        }
    }
}
