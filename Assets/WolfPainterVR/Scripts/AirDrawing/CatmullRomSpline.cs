
using System.Collections.Generic;
using UnityEngine;

public static class CatmullRomSpline
{
    // Interpolate a position between p1 and p2 using four control points.
    public static Vector3 Interpolate(
        Vector3 p0,
        Vector3 p1,
        Vector3 p2,
        Vector3 p3,
        float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        // Uniform Catmull-Rom interpolation formula.
        return 0.5f * (
            2f * p1
            + (-p0 + p2) * t
            + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2
            + (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }

    // Generate a smooth curve from the sampled positions.
    public static List<Vector3> GenerateCurve(
        List<Vector3> points,
        int subdivisions)
    {
        List<Vector3> curve = new List<Vector3>();

        if (points.Count < 2)
        {
            curve.AddRange(points);
            return curve;
        }

        subdivisions = Mathf.Max(1, subdivisions);

        for (int i = 0; i < points.Count - 1; i++)
        {
            // Duplicate endpoints when neighboring points are missing.
            Vector3 p0 = points[Mathf.Max(i - 1, 0)];
            Vector3 p1 = points[i];
            Vector3 p2 = points[i + 1];
            Vector3 p3 = points[Mathf.Min(i + 2, points.Count - 1)];

            // Exclude t = 1 to avoid duplicating shared endpoints.
            for (int j = 0; j < subdivisions; j++)
            {
                float t = (float)j / subdivisions;

                Vector3 point = Interpolate(p0, p1, p2, p3, t);

                curve.Add(point);
            }
        }

        // Add the final endpoint excluded from the loop.
        curve.Add(points[points.Count - 1]);

        return curve;
    }
}
