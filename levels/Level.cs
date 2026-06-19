using Godot;
using System;

public partial class Level : Node3D
{
    [Export] private GpuParticles2D confetti1;
    [Export] private GpuParticles2D confetti2;

    [Export] public Label catsFoundLabel;
    private int totalCats = 0;
    private int foundCats = 0;
    public override void _Ready()
    {
        // play music
        AudioModule.Instance.SetVolume("Music", 0.1f);
        AudioModule.Instance.PlayMusic("res://audio/music/lighthouse.mp3"); 

        LoadCats();
    }

    private void LoadCats()
    {
        Node catsFolder = GetNode<Node>("cats");
        foreach (Cat cat in catsFolder.GetChildren())
        {
            if (cat.Visible == false) continue;

            totalCats++;

            if (SaveManager.Instance.IsCollected(cat.Name)) // if the cat is already collected according to the save file
            {
                foundCats++; // count it without waiting for the signal

                
            }
            else // otherwise for cats that arent collected
            {
                cat.catFoundWithArgument += OnCatFound; // subscribe to the signal so they are properly functional and removeable
            }
        }
        
        if (foundCats >= totalCats)
        {
            catsFoundLabel.LabelSettings.FontColor = new Color("ffffba");
            confetti1.Emitting = true;
            confetti2.Emitting = true;
        }

        // GD.Print(totalCats + " total cats in this level");
        UpdateCatLabel();
    }
    private void OnCatFound(Cat cat)
    {
        // GD.Print("found cat");
        foundCats++;
        UpdateCatLabel();

        VanishCat(cat);

        if (foundCats >= totalCats)
        {
            catsFoundLabel.LabelSettings.FontColor = new Color("ffffba");
            confetti1.Emitting = true;
            confetti2.Emitting = true;
        }
    }

    private void VanishCat(Cat cat)
    {
        Sprite3D catSprite = cat.GetNode<Sprite3D>("Sprite3D");

        Tween disappearTween = CreateTween();
        disappearTween.TweenProperty(catSprite, "modulate:a", 0.0, 0.75);

        Tween upTween = CreateTween();
        upTween.TweenProperty(catSprite, "position", catSprite.Position + new Vector3(0, 0.5f, 0), 1.0);

        disappearTween.Finished += () => cat.QueueFree();
    }

    private void UpdateCatLabel()
    {
        catsFoundLabel.Text = foundCats.ToString();
    }
}
