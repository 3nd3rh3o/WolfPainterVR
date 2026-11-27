
using System.Collections.Generic;
using UnityEngine;

public class TubeStrokeRenderer : StrokeRenderer
{
    private Mesh mesh;
    private float radius;
    private int radialSegments = 8;

    // Configure the tube resolution.
    public void SetRadialSegments(int segments)
    {
        radialSegments = Mathf.Max(3, segments);
    }

    // Initialize the tube rendering components.
    public override void Initialize(
        Material material,
        Color color,
        float width)
    {
        radius = width * 0.5f;

        MeshFilter meshFilter =
            gameObject.AddComponent<MeshFilter>();

        MeshRenderer meshRenderer =
            gameObject.AddComponent<MeshRenderer>();

        mesh = new Mesh();
        mesh.name = "AirStrokeMesh";
        mesh.MarkDynamic();

        meshFilter.sharedMesh = mesh;
        meshRenderer.sharedMaterial = material;

        // Apply a per-stroke color.
        MaterialPropertyBlock properties =
            new MaterialPropertyBlock();

        properties.SetColor("_BaseColor", color);

        meshRenderer.SetPropertyBlock(properties);
    }

    // Update the tube geometry.
    public override void UpdateStroke(List<Vector3> points)
    {
        TubeMeshGenerator.GenerateMesh(
            mesh,
            points,
            radius,
            radialSegments
        );
    }

    // Release the generated mesh.
    private void OnDestroy()
    {
        if (mesh != null)
        {
            Destroy(mesh);
        }
    }
}
