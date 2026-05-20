using Godot;
using System;

public partial class CloseButton : TextureButton
{
    [Export] private PanelContainer settingsContainer;
    private bool IsHovering;
    public override void _Ready()
    {        
        MouseEntered += onEnter;
        MouseExited += onExit;
    }

    public override void _Pressed()
    {
        if (settingsContainer == null) return;

        Tween moveRight = CreateTween();
        moveRight.SetEase(Tween.EaseType.Out);
        moveRight.SetTrans(Tween.TransitionType.Expo);
        moveRight.TweenProperty(settingsContainer, "position:x", -settingsContainer.Size.X, 1f);
    }


    private void onEnter()
    {
        startTween(new Vector2(0.8f, 0.8f));
    }
    private void onExit()
    {
        startTween(Vector2.One);
    }
    private void startTween(Vector2 scale)
    {
        Tween growTween = CreateTween();
        // growTween.SetEase(Tween.EaseType.Out);
        growTween.SetTrans(Tween.TransitionType.Expo);
        growTween.TweenProperty(this, "scale", scale, 0.15f);
    }
}
