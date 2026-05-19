using Godot;
using System;

public partial class CloudGenerator : MultiMeshInstance3D
{
    [Export] private Mesh cloudMesh;
    [Export] private int cloudCount = 1;
    [Export] private Vector2 spawnRegion = new Vector2(200, 200);
    [Export] private Vector2 yOffset = new Vector2(10, 50);
    private Godot.Collections.Array<int> y_rotation_steps = new Godot.Collections.Array<int>{0, 90, 180, 270};
    private Vector3[] cloudPositions;
    private Basis[] cloudBasis;
    public override void _Ready()
    {
        // initial check
        if (cloudMesh == null || cloudCount <= 0)
        {
            GD.PrintErr("[CloudGenerator] no cloud mesh or invalid cloud count");
            return;
        }

        normalizedWind = normalizedWind = windDirection.Normalized();
        cloudPositions = new Vector3[cloudCount];
        cloudBasis = new Basis[cloudCount];

        // create new multimesh
        MultiMesh mm = new MultiMesh();
        mm.Mesh = cloudMesh;
        mm.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
        mm.InstanceCount = cloudCount;

        // for each cloud
        for (int cloud = 0; cloud < cloudCount; cloud++)
        {
            // create and apply unshaded mesh
            StandardMaterial3D mat = new StandardMaterial3D();
            mat.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            mat.AlbedoColor = new Color(1, 1, 1, 0.7f);
            cloudMesh.SurfaceSetMaterial(0, mat);


            // create random cloud position
            float height = (float)GD.RandRange(yOffset.X, yOffset.Y);
            Vector3 position = new Vector3(
                (float)GD.RandRange(-spawnRegion.X, spawnRegion.X),
                height,
                (float)GD.RandRange(-spawnRegion.Y, spawnRegion.Y)
            );
            cloudPositions[cloud] = position;


            // create random rotation
            Basis rotation = new Basis(
                new Vector3(0, 1, 0), // using axis
                Mathf.DegToRad(y_rotation_steps.PickRandom()) // and an angle
            );
            // create random scale (added on to the basis)
            float scale = height/10f;
            Basis scaledRotation = rotation.Scaled(new Vector3(scale, scale, scale));
            cloudBasis[cloud] = scaledRotation;


            // combine into a single transformation
            Transform3D transform = new Transform3D(scaledRotation, position);
            

            // set transform
            mm.SetInstanceTransform(cloud, transform);
        }

        Multimesh = mm;
    }

    [Export] private float cloudSpeed = 20f;
    private Vector3 windDirection = new Vector3(1, 0, 0);
    private Vector3 normalizedWind;
    public override void _Process(double delta)
    {
        for (int cloud = 0; cloud < cloudCount; cloud++)
        {
            // move cloud
            cloudPositions[cloud] += normalizedWind * cloudSpeed * (float)delta;

            // teleport to the back of the bounds if it reaches one end
            if (cloudPositions[cloud].X > spawnRegion.X)
                cloudPositions[cloud].X = -spawnRegion.X;
            
            Transform3D transform = new Transform3D(cloudBasis[cloud], cloudPositions[cloud]);
            Multimesh.SetInstanceTransform(cloud, transform);
        }
    }

}
