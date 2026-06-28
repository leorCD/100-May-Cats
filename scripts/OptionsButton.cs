
using Godot;
using System;

public partial class OptionsButton : PanelContainer
{
    private bool isPanelVisible  = false;
    private bool IsHovering;
    public override void _Ready()
    {       
        this.Visible = true;
        this.Position = new Vector2(this.Position.X - this.Size.X, this.Position.Y);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("escape"))
        {
            isPanelVisible = !isPanelVisible;
            float targetX = isPanelVisible ? 0 : -this.Size.X;

            Tween move = CreateTween();
            move.SetEase(Tween.EaseType.Out);
            move.SetTrans(Tween.TransitionType.Expo);
            move.TweenProperty(this, "position:x", targetX, 1f);
        }
    }
}