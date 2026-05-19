using Godot;
using System;

public partial class sfxSlider : HSlider
{
    public override void _Ready()
    {
        AudioModule.Instance.SetVolume("SFX", (float)Value);
    }

    public override void _ValueChanged(double newValue)
    {
        AudioModule.Instance.SetVolume("SFX", (float)newValue);
    }

}
