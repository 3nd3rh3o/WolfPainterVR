
using System.Collections.Generic;
using UnityEngine;

public class LineStrokeRenderer : StrokeRenderer
{
    private LineRenderer lineRenderer;

    // Initialize the LineRenderer.
    public override void Initialize(
        Material material,
        Color color,
        float width)
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.useWorldSpace = true;
        lineRenderer.sharedMaterial = material;

        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;

        lineRenderer.numCapVertices = 8;
        lineRenderer.numCornerVertices = 8;

        lineRenderer.positionCount = 0;
    }

    // Update the line positions.
    public override void UpdateStroke(List<Vector3> points)
    {
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }
}
