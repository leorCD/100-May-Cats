using Godot;
using System;

public partial class CameraArm : SpringArm3D
{
    [Export] Node3D adornee = null;
    [Export] double mouse_sensitivity = 0.1;
    [Export] float yOffset = 0f;
    [Export] public float minDistance = 2.5f;
    [Export] public float maxDistance = 5f;
    public bool active { get; set; } = false;
    private float targetDistance;

    private float zoomSpeed = 6f;
    private float zoomStep = 0.025f;
    private float zoomVelocity = 0f;
    private float zoomAcceleration = 0.05f;
    private float zoomDecay = 0.85f; // multiplied each frame, lower = faster decay
    
    public override void _Ready()
    {
        targetDistance = maxDistance;

        if (adornee != null)
            GlobalPosition = adornee.GlobalPosition + new Vector3(0, yOffset, 0);
    }

    public override void _Process(double delta)
    {
        SpringLength = Mathf.Lerp(SpringLength, targetDistance, (float)delta * zoomSpeed);

        targetDistance += zoomVelocity;
        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        zoomVelocity *= zoomDecay;
    }


    public override void _Input(InputEvent @event)
    {
        if (!active) return;

        // pan camera
        if (@event is InputEventMouseMotion mouseMotion && mouseMotion.ButtonMask.HasFlag(MouseButtonMask.Right))
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

        // zoom in and out
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.WheelDown)
                zoomVelocity += zoomAcceleration;
            if (mouseButton.ButtonIndex == MouseButton.WheelUp)
                zoomVelocity -= zoomAcceleration;
        }
    }
}
