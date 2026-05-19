using Godot;
using System;

public partial class musicSlider : HSlider
{
    public override void _Ready()
    {
        AudioModule.Instance.SetVolume("Music", (float)Value);
    }
 
    public override void _ValueChanged(double newValue)
    {
        AudioModule.Instance.SetVolume("Music", (float)newValue);
    }

}
