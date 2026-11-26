
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public static class TubeMeshGenerator
{
    public static void GenerateMesh(
        Mesh mesh,
        List<Vector3> points,
        float radius,
        int radialSegments)
    {
        mesh.Clear();

        if (points.Count < 2)
        {
            return;
        }

        radialSegments = Mathf.Max(3, radialSegments);
        radius = Mathf.Max(0.0001f, radius);

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        Vector3 previousTangent = Vector3.zero;
        Vector3 normal = Vector3.zero;
        Vector3 firstTangent = Vector3.zero;

        // Generate a circular section at each curve point.
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 tangent = GetTangent(points, i);

            if (tangent.sqrMagnitude < 0.000001f)
            {
                tangent = i == 0
                    ? Vector3.forward
                    : previousTangent;
            }

            if (i == 0)
            {
                firstTangent = tangent;
                normal = GetInitialNormal(tangent);
            }
            else
            {
                // Transport the previous frame along the curve.
                Quaternion rotation = Quaternion.FromToRotation(
                    previousTangent,
                    tangent
                );

                normal = rotation * normal;

                normal = Vector3.ProjectOnPlane(
                    normal,
                    tangent
                ).normalized;

                if (normal.sqrMagnitude < 0.000001f)
                {
                    normal = GetInitialNormal(tangent);
                }
            }

            Vector3 binormal = Vector3.Cross(
                tangent,
                normal
            ).normalized;

            // Duplicate the first vertex for the UV seam.
            for (int j = 0; j <= radialSegments; j++)
            {
                float angle = 2f * Mathf.PI * j / radialSegments;

                Vector3 direction =
                    Mathf.Cos(angle) * normal
                    + Mathf.Sin(angle) * binormal;

                vertices.Add(points[i] + radius * direction);
                normals.Add(direction);

                uvs.Add(new Vector2(
                    (float)j / radialSegments,
                    (float)i / (points.Count - 1)
                ));
            }

            previousTangent = tangent;
        }

        int ringSize = radialSegments + 1;

        // Connect consecutive sections with triangles.
        for (int i = 0; i < points.Count - 1; i++)
        {
            for (int j = 0; j < radialSegments; j++)
            {
                int a = i * ringSize + j;
                int b = a + 1;
                int c = a + ringSize;
                int d = c + 1;

                triangles.Add(a);
                triangles.Add(b);
                triangles.Add(c);

                triangles.Add(b);
                triangles.Add(d);
                triangles.Add(c);
            }
        }

        // Close both ends of the tube.
        AddCap(
            vertices, normals, uvs, triangles,
            points[0],
            -firstTangent,
            0,
            radialSegments,
            true
        );

        AddCap(
            vertices, normals, uvs, triangles,
            points[points.Count - 1],
            previousTangent,
            (points.Count - 1) * ringSize,
            radialSegments,
            false
        );

        mesh.indexFormat = vertices.Count > 65535
            ? IndexFormat.UInt32
            : IndexFormat.UInt16;

        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);

        mesh.RecalculateBounds();
    }

    private static Vector3 GetTangent(
        List<Vector3> points,
        int index)
    {
        int previous = Mathf.Max(index - 1, 0);
        int next = Mathf.Min(index + 1, points.Count - 1);

        return (points[next] - points[previous]).normalized;
    }

    private static Vector3 GetInitialNormal(Vector3 tangent)
    {
        Vector3 reference =
            Mathf.Abs(Vector3.Dot(tangent, Vector3.up)) > 0.99f
            ? Vector3.right
            : Vector3.up;

        return Vector3.Cross(tangent, reference).normalized;
    }

    private static void AddCap(
        List<Vector3> vertices,
        List<Vector3> normals,
        List<Vector2> uvs,
        List<int> triangles,
        Vector3 center,
        Vector3 normal,
        int ringStart,
        int radialSegments,
        bool isStart)
    {
        int centerIndex = vertices.Count;

        vertices.Add(center);
        normals.Add(normal);
        uvs.Add(new Vector2(0.5f, 0.5f));

        int capStart = vertices.Count;

        // Duplicate ring vertices to obtain flat cap normals.
        for (int j = 0; j < radialSegments; j++)
        {
            vertices.Add(vertices[ringStart + j]);
            normals.Add(normal);

            float angle = 2f * Mathf.PI * j / radialSegments;

            uvs.Add(new Vector2(
                0.5f + 0.5f * Mathf.Cos(angle),
                0.5f + 0.5f * Mathf.Sin(angle)
            ));
        }

        for (int j = 0; j < radialSegments; j++)
        {
            int current = capStart + j;
            int next = capStart + (j + 1) % radialSegments;

            triangles.Add(centerIndex);

            if (isStart)
            {
                triangles.Add(next);
                triangles.Add(current);
            }
            else
            {
                triangles.Add(current);
                triangles.Add(next);
            }
        }
    }
}
