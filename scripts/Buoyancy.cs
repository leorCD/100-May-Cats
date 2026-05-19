using Godot;
using System;

[Tool]
public partial class Buoyancy : Node3D
{
    [Export] public MeshInstance3D Water { get; set; }
    [Export] public float WindRippleDegScaler { get; set; } = 1.0f;
    [Export] public float VertexSpeedScaler { get; set; } = 1.0f;
    [Export] public Vector2 Dir { get; set; } = new Vector2(-0.2f, 1.0f);
    [Export] public Texture2D NorRand1 { get; set; }

    // Optional: add Node3D children to your boat at hull corners for rotation
    [Export] public Node3D[] SamplePoints { get; set; } = [];
    [Export] public float BobSmoothing { get; set; } = 5.0f;
    [Export] public float yOffset = 0;

    private Image _image;

    public override void _Ready()
    {
        if (Water == null) { GD.PrintErr("Water is null"); return; }

        var mat = Water.GetActiveMaterial(0) as ShaderMaterial;
        if (mat == null) { GD.PrintErr("Not a ShaderMaterial"); return; }

        WindRippleDegScaler = (float)mat.GetShaderParameter("windRippleDegScaler");
        VertexSpeedScaler   = (float)mat.GetShaderParameter("vertexSpeedScaler");
        Dir                 = (Vector2)mat.GetShaderParameter("dir");
        NorRand1            = mat.GetShaderParameter("norRand1").As<Texture2D>();

        if (NorRand1 == null) { GD.PrintErr("norRand1 is null"); return; }

        // NoiseTexture2D generates async — wait for it to be ready
        if (NorRand1 is NoiseTexture2D noiseTexture)
        {
            if (noiseTexture.GetImage() == null)
            {
                // GD.Print("Waiting for noise texture to generate...");
                noiseTexture.Changed += OnTextureReady;
            }
            else
            {
                LoadImage(NorRand1);
            }
        }
        else
        {
            LoadImage(NorRand1);
        }
    }

    private void OnTextureReady()
    {
        // GD.Print("Noise texture ready!");
        if (NorRand1 is NoiseTexture2D noiseTexture)
            noiseTexture.Changed -= OnTextureReady;
        LoadImage(NorRand1);
    }

    private void LoadImage(Texture2D tex)
    {
        _image = tex.GetImage();
        if (_image == null) { GD.PrintErr("GetImage() still null"); return; }
        _image.Convert(Image.Format.Rgbaf);
        // GD.Print("Buoyancy ready!");
    }

    private float GetWaveHeight(Vector2 worldXZ, float time)
    {
        if (_image == null) return 0f;

        // Mirrors the shader's vertex() displacement exactly:
        // texture(norRand1, (dir * vertexSpeedScaler * TIME + VERTEX.xz) * 0.01).r
        Vector2 uv = (Dir * VertexSpeedScaler * time + worldXZ) * 0.01f;

        // Wrap to simulate repeat_enable
        uv = uv - new Vector2(Mathf.Floor(uv.X), Mathf.Floor(uv.Y));

        int px = (int)(uv.X * _image.GetWidth())  % _image.GetWidth();
        int py = (int)(uv.Y * _image.GetHeight()) % _image.GetHeight();

        float r = _image.GetPixel(px, py).R;

        return WindRippleDegScaler * 0.5f * (r - 0.5f);
    }

    public override void _Process(double delta)
    {
        if (Water == null || _image == null) return;

        float time = Time.GetTicksMsec() / 1000.0f;
        float waterY = Water.GlobalPosition.Y;

        if (SamplePoints.Length > 0)
        {
            float totalHeight = 0f;
            Vector3[] heights = new Vector3[SamplePoints.Length];

            for (int i = 0; i < SamplePoints.Length; i++)
            {
                var pt = SamplePoints[i];
                float waveY = GetWaveHeight(new Vector2(pt.GlobalPosition.X, pt.GlobalPosition.Z), time);
                heights[i] = new Vector3(pt.GlobalPosition.X, waterY + waveY, pt.GlobalPosition.Z);
                totalHeight += waterY + waveY;
            }

            var pos = GlobalPosition;
            float targetY = (totalHeight / SamplePoints.Length) + yOffset;
            pos.Y = Mathf.Lerp(GlobalPosition.Y, targetY, (float)delta * BobSmoothing);
            GlobalPosition = pos;

            if (SamplePoints.Length == 4)
            {
                Vector3 front = (heights[0] + heights[1]) / 2f;
                Vector3 back  = (heights[2] + heights[3]) / 2f;
                Vector3 left  = (heights[0] + heights[2]) / 2f;
                Vector3 right = (heights[1] + heights[3]) / 2f;

                float pitch = Mathf.Atan2(back.Y  - front.Y, (front - back).Length());
                float roll  = Mathf.Atan2(right.Y - left.Y,  (left - right).Length());

                Rotation = new Vector3(pitch, Rotation.Y, roll);
            }
        }
        else
        {
            float waveOffset = GetWaveHeight(new Vector2(GlobalPosition.X, GlobalPosition.Z), time);
            var pos = GlobalPosition;
            float targetY = waterY + waveOffset + yOffset;
            pos.Y = Mathf.Lerp(GlobalPosition.Y, targetY, (float)delta * BobSmoothing);
            GlobalPosition = pos;
        }
    }
}