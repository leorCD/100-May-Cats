using Godot;

public partial class LightRays : ColorRect
{
    [Export] public NodePath LightPath { get; set; }
    private DirectionalLight3D _light;

    [Export] public NodePath CameraPath { get; set; }
    private Camera3D _camera;

    public override void _Ready()
    {
        Visible = true;
        _light = GetNode<DirectionalLight3D>(LightPath);
        _camera = GetNode<Camera3D>(CameraPath);
    }

    public override void _Process(double delta)
    {
        var pos = _camera.UnprojectPosition(_camera.GlobalPosition - (-_light.GlobalBasis.Z.Normalized()));
        (Material as ShaderMaterial)?.SetShaderParameter("light_source_pos", pos);
        (Material as ShaderMaterial)?.SetShaderParameter("light_source_dir", -_light.GlobalBasis.Z);
        (Material as ShaderMaterial)?.SetShaderParameter("camera_dir", -_camera.GlobalBasis.Z);
    }
}