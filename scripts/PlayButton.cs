using Godot;
using System;

public partial class PlayButton : TextureButton
{
    public override void _Ready()
    {
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        this.PivotOffsetRatio = pivot;

        MouseEntered += onEnter;
        MouseExited += onExit;
    }

    [Export] private Control title;
    [Export] private Control exit;
    [Export] private Control blur;
    [Export] private Control tutorialPage;
    [Export] private CameraArm cameraArm;
    [Export] private Control control1;
    [Export] private Control control2;
    [Export] private Control control3;
    [Export] private Control vignette;
    public override void _Pressed()
    {
        AudioModule.Instance.PlaySFX("res://audio/sfx/typewriter.mp3");
        var windowSize = GetViewport().GetVisibleRect().Size;

        Tween fadeIn = CreateTween().SetParallel();
        fadeIn.SetTrans(Tween.TransitionType.Quad);

        fadeIn.TweenProperty(control1, "modulate:a", 1f, 2.0);
        fadeIn.TweenProperty(control2, "modulate:a", 1f, 2.0);
        fadeIn.TweenProperty(control3, "modulate:a", 1f, 2.0);
        fadeIn.TweenMethod(Callable.From((float value) =>
            vignette.Material.Set("shader_parameter/intensity", value)), 0f, 2.0f, 2.0);


        Tween fadeOut = CreateTween().SetParallel();
        fadeOut.SetTrans(Tween.TransitionType.Quad);

        fadeOut.TweenMethod(Callable.From((float value) =>
            blur.Material.Set("shader_parameter/strength", value)), 3.3f, 0f, 2.0);
        fadeOut.TweenMethod(Callable.From((float value) =>
            blur.Material.Set("shader_parameter/mix_percentage", value)), 0.122f, 0f, 2.0);
        fadeOut.TweenProperty(title, "position:y", title.Position.Y - windowSize.Y, 2.0); // up
        fadeOut.TweenProperty(exit, "position:y", exit.Position.Y + windowSize.Y, 2.0);  // down
        fadeOut.TweenProperty(this, "position:y", Position.Y + windowSize.Y, 2.0);       // down
        fadeOut.Finished += () =>
        {
            blur.QueueFree();
            title.QueueFree();
            exit.QueueFree();
            this.QueueFree();
            cameraArm.active = true;
        };

        tutorialPage.Visible = true;
        Tween openTutorial = CreateTween();
        openTutorial.SetEase(Tween.EaseType.Out);
        openTutorial.SetTrans(Tween.TransitionType.Expo);
        openTutorial.TweenProperty(tutorialPage, "position:y", 0, 1.0);
        openTutorial.TweenProperty(tutorialPage, "modulate:a", 1f, 1.0);
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
