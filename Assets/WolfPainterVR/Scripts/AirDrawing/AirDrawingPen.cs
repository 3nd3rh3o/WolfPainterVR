
using System.Collections.Generic;
using UnityEngine;

public enum StrokeRenderMode
{
    LineRenderer,
    TubeMesh
}

public class AirDrawingPen : MonoBehaviour
{
    [Header("Pen")]
    [SerializeField] private Transform tip;

    [Header("Rendering")]
    [SerializeField]
    private StrokeRenderMode renderMode = StrokeRenderMode.LineRenderer;

    [Header("Drawing settings")]
    [SerializeField] private Color color = Color.blue;
    [SerializeField] private float width = 0.01f;
    [SerializeField] private float minDistance = 0.005f;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private Material tubeMaterial;

    [Header("Tube settings")]
    [SerializeField, Range(3, 32)]
    private int radialSegments = 8;

    [Header("Curve smoothing")]
    [SerializeField] private bool useSmoothing = true;

    [SerializeField, Range(1, 20)]
    private int subdivisions = 5;

    [Header("Drawing state")]
    [SerializeField] private bool isDrawing = false;

    private GameObject currentStroke;
    private LineRenderer currentLine;
    private Mesh currentMesh;

    private StrokeRenderMode currentRenderMode;
    private Vector3 lastPosition;

    // Store original positions before interpolation.
    private List<Vector3> rawPoints = new List<Vector3>();

    private void Start()
    {
        if (tip == null)
        {
            Debug.LogError("Pen tip is missing.");
            enabled = false;
        }
    }

    private void Update()
    {
        if (!isDrawing)
        {
            if (currentStroke != null)
            {
                StopDrawing();
            }

            return;
        }

        if (currentStroke == null)
        {
            StartDrawing();

            if (currentStroke == null)
            {
                return;
            }
        }

        Vector3 currentPosition = tip.position;

        // Sample a point only after sufficient movement.
        if (Vector3.Distance(lastPosition, currentPosition) >= minDistance)
        {
            AddPoint(currentPosition);
        }
    }

    public void StartDrawing()
    {
        if (currentStroke != null || tip == null)
        {
            return;
        }

        Material selectedMaterial =
            renderMode == StrokeRenderMode.LineRenderer
                ? lineMaterial
                : tubeMaterial;

        if (selectedMaterial == null)
        {
            Debug.LogError("Selected rendering material is missing.");
            isDrawing = false;
            return;
        }

        isDrawing = true;

        // Keep the selected rendering mode for the entire stroke.
        currentRenderMode = renderMode;

        CreateStroke();
    }

    public void StopDrawing()
    {
        isDrawing = false;

        // Remove incomplete strokes.
        if (currentStroke != null && rawPoints.Count < 2)
        {
            if (currentMesh != null)
            {
                Destroy(currentMesh);
            }

            Destroy(currentStroke);
        }

        currentStroke = null;
        currentLine = null;
        currentMesh = null;

        rawPoints.Clear();
    }

    private void CreateStroke()
    {
        rawPoints.Clear();

        currentStroke = new GameObject("AirStroke");

        switch (currentRenderMode)
        {
            case StrokeRenderMode.LineRenderer:
                CreateLine();
                break;

            case StrokeRenderMode.TubeMesh:
                CreateTube();
                break;
        }

        AddPoint(tip.position);
    }

    private void CreateLine()
    {
        currentStroke.name = "AirStroke_Line";

        currentLine = currentStroke.AddComponent<LineRenderer>();

        currentLine.useWorldSpace = true;
        currentLine.sharedMaterial = lineMaterial;

        currentLine.startColor = color;
        currentLine.endColor = color;

        currentLine.startWidth = width;
        currentLine.endWidth = width;

        currentLine.numCapVertices = 8;
        currentLine.numCornerVertices = 8;

        currentLine.positionCount = 0;
    }

    private void CreateTube()
    {
        currentStroke.name = "AirStroke_Tube";

        MeshFilter meshFilter =
            currentStroke.AddComponent<MeshFilter>();

        MeshRenderer meshRenderer =
            currentStroke.AddComponent<MeshRenderer>();

        currentMesh = new Mesh();
        currentMesh.name = "AirStrokeMesh";
        currentMesh.MarkDynamic();

        meshFilter.sharedMesh = currentMesh;
        meshRenderer.sharedMaterial = tubeMaterial;

        // Apply a per-stroke color without duplicating the material.
        MaterialPropertyBlock properties =
            new MaterialPropertyBlock();

        properties.SetColor("_BaseColor", color);

        meshRenderer.SetPropertyBlock(properties);
    }

    private void AddPoint(Vector3 position)
    {
        rawPoints.Add(position);
        lastPosition = position;

        UpdateStroke();
    }

    private void UpdateStroke()
    {
        List<Vector3> displayPoints;

        if (useSmoothing)
        {
            displayPoints = CatmullRomSpline.GenerateCurve(
                rawPoints,
                subdivisions
            );
        }
        else
        {
            displayPoints = rawPoints;
        }

        // Update the geometry using the active rendering mode.
        switch (currentRenderMode)
        {
            case StrokeRenderMode.LineRenderer:
                currentLine.positionCount = displayPoints.Count;
                currentLine.SetPositions(displayPoints.ToArray());
                break;

            case StrokeRenderMode.TubeMesh:
                TubeMeshGenerator.GenerateMesh(
                    currentMesh,
                    displayPoints,
                    width * 0.5f,
                    radialSegments
                );
                break;
        }
    }
}
