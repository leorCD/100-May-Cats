using Godot;
using System;

public partial class Cat : Node3D, IInteractable
{
    [Signal] public delegate void catFoundWithArgumentEventHandler(Cat cat);

    [Export] private Godot.Collections.Array<Texture2D> catTextures;
    private Sprite3D spriteObj;
    private Texture2D _spriteTexture;
    [Export] public Texture2D texture
    {
        get => _spriteTexture;
        set
        {
            _spriteTexture = value;

            if (IsInsideTree())
            {
                spriteObj = GetNode<Sprite3D>("Sprite3D");
                spriteObj.Texture = value;
            }
        }
    }

    public override void _Ready()
    {
        if (SaveManager.Instance.IsCollected(this.Name))
        {
            this.QueueFree();
            return;
        }

        spriteObj = GetNode<Sprite3D>("Sprite3D");
        spriteObj.Texture = catTextures.PickRandom();
        SetColor();
    }

    public void Interact()
    {
        SaveManager.Instance.CollectCat(this.Name);
        StaticBody3D collider = GetNode<StaticBody3D>("StaticBody3D");
        EmitSignal(SignalName.catFoundWithArgument, this);
        
        PlaySound();
        collider.QueueFree();
    }




    private Color GetRandomColor()
    {
        var rng = new RandomNumberGenerator();
        rng.Randomize();

        Color[] colors = {
            new Color("829e7d"),
            new Color("905f44"),
            new Color("ffffe5"),
            new Color("dfc9c4"),
            new Color("8f8a87"),
            new Color("905f43"),
            new Color("ffffff"),
            new Color("82807f"),
            new Color("36635f"),
            new Color("727976"),
            new Color("698385"),
            new Color("e2e4e2"),
            new Color("ceaa82"),
        };

        return colors[rng.Randi() % colors.Length];
    }
    private void SetColor()
    {
        var mat = (ShaderMaterial)spriteObj.MaterialOverride.Duplicate();
        spriteObj.MaterialOverride = mat;
        mat.SetShaderParameter("texture_albedo", spriteObj.Texture);
        mat.SetShaderParameter("replace_color", GetRandomColor());
    }

    [Export] public Godot.Collections.Array<AudioStreamMP3> meowSfxs = new Godot.Collections.Array<AudioStreamMP3>();
    [Export] public Godot.Collections.Array<AudioStreamMP3> popSfxs = new Godot.Collections.Array<AudioStreamMP3>();
    
    private void PlaySound()
    {
        string randomMeowFile = meowSfxs.PickRandom().GetPath();
        AudioModule.Instance.PlaySFX(randomMeowFile);

        string randomPopFile = popSfxs.PickRandom().GetPath();
        AudioModule.Instance.PlaySFX(randomPopFile);
    }
}