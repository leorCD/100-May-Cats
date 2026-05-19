using Godot;
using System;

public partial class OptionsButton : Button
{
    [Export] private PanelContainer settingsContainer;
    private bool visible = false;
    private bool IsHovering;
    public override void _Ready()
    {        
        MouseEntered += onEnter;
        MouseExited += onExit;

        if (settingsContainer != null)
        {
            settingsContainer.Position = new Vector2(settingsContainer.Position.X - settingsContainer.Size.X, settingsContainer.Position.Y);
            settingsContainer.Visible = false;
        }
    }

    public override void _Pressed()
    {
        if (settingsContainer == null) return;

        Tween moveRight = CreateTween();
        moveRight.SetEase(Tween.EaseType.In);
        moveRight.SetTrans(Tween.TransitionType.Expo);
        if (visible == false)
        {
            settingsContainer.Visible = true;
            moveRight.TweenProperty(settingsContainer, "position:x", 0, 1f);
            moveRight.Finished += () => visible = true;
        }
        else
        {
            moveRight.TweenProperty(settingsContainer, "position:x", -settingsContainer.Size.X, 1f);
            moveRight.Finished += () => {
                visible = false;
                settingsContainer.Visible = false;
            };
        }

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
