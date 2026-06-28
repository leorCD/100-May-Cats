using Godot;
using System;

public partial class ExitButton : TextureButton
{
    public override void _Ready()
    {
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        this.PivotOffsetRatio = pivot;

        MouseEntered += onEnter;
        MouseExited += onExit;
    }

    public override void _Pressed()
    {
        AudioModule.Instance.PlaySFX("res://audio/sfx/typewriter.mp3");
        GetTree().Quit();
    }

    
    
    private void onEnter()
    {
        startTween(new Vector2(0.9f, 0.9f));
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
