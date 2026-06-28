using Godot;
using System;

public partial class TutorialClose : TextureButton
{
    public override void _Pressed()
    {
        AudioModule.Instance.PlaySFX("res://audio/sfx/typewriter.mp3");
        
        var parent = this.GetParent();

        Tween close = CreateTween().SetParallel();
        close.SetTrans(Tween.TransitionType.Expo);
        close.TweenProperty(parent, "modulate:a", 0f, 1.0);

        close.Finished += () => parent.QueueFree();
    }
}
