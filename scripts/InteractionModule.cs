using Godot;
using System;

public partial class InteractionModule : Node3D
{
	private IInteractable interactionObject;
	private Camera3D camera;
	public override void _Ready()
	{
		camera = Owner as Camera3D;
	}

    public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mousePressed && mousePressed.Pressed)
		{
			if (mousePressed.ButtonIndex == MouseButton.Left)
			if (interactionObject != null)
			{
				interactionObject.Interact();
			}
		}
	}



	public override void _Process(double delta)
	{
		interactionObject = ShootRay();
	}

	private IInteractable ShootRay()
	{
		IInteractable interactable = null;

		Vector2 mousePositionOnScreen = GetViewport().GetMousePosition();
		float rayLength = 100f;

		Vector3 rayOrigin = camera.ProjectRayOrigin(mousePositionOnScreen);
		Vector3 rayDirection = camera.ProjectRayNormal(mousePositionOnScreen);
		Vector3 rayEnd = rayOrigin + rayDirection * rayLength;

		PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);
		
		PhysicsDirectSpaceState3D directSpaceState = GetViewport().GetWorld3D().DirectSpaceState;
		Godot.Collections.Dictionary result = directSpaceState.IntersectRay(query);

		if (result.Count > 0)
		{   
			Node collider = result["collider"].As<Node>();
			interactable = collider.Owner as IInteractable;
			return interactable;
		}

		return null;
	}
}
