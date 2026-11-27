
using System.Collections.Generic;
using UnityEngine;

public abstract class StrokeRenderer : MonoBehaviour
{
    // Initialize the stroke rendering settings.
    public abstract void Initialize(
        Material material,
        Color color,
        float width
    );

    // Update the stroke geometry.
    public abstract void UpdateStroke(
        List<Vector3> points
    );
}
