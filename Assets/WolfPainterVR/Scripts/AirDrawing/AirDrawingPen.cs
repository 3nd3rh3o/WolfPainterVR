
using System.Collections.Generic;
using UnityEngine;

public class AirDrawingPen : MonoBehaviour
{
    [Header("Pen")]
    [SerializeField] private Transform tip;

    [Header("Drawing settings")]
    [SerializeField] private Color color = Color.blue;
    [SerializeField] private float width = 0.01f;
    [SerializeField] private float minDistance = 0.005f;
    [SerializeField] private Material lineMaterial;

    [Header("Curve smoothing")]
    [SerializeField] private bool useSmoothing = true;

    [SerializeField, Range(1, 20)]
    private int subdivisions = 5;

    [Header("Drawing state")]
    [SerializeField] private bool isDrawing = false;

    private LineRenderer currentLine;
    private Vector3 lastPosition;

    // Store original positions before interpolation.
    private List<Vector3> rawPoints = new List<Vector3>();

    private void Start()
    {
        if (tip == null || lineMaterial == null)
        {
            Debug.LogError("Pen tip or line material is missing.");
            enabled = false;
        }
    }

    private void Update()
    {
        if (!isDrawing)
        {
            if (currentLine != null)
            {
                StopDrawing();
            }

            return;
        }

        if (currentLine == null)
        {
            StartDrawing();

            if (currentLine == null)
            {
                return;
            }
        }

        Vector3 currentPosition = tip.position;

        // Sample a new point only if the pen has moved far enough.
        if (Vector3.Distance(lastPosition, currentPosition) >= minDistance)
        {
            AddPoint(currentPosition);
        }
    }

    // Start a new independent stroke.
    public void StartDrawing()
    {
        if (currentLine != null || tip == null || lineMaterial == null)
        {
            return;
        }

        isDrawing = true;
        CreateLine();
    }

    // End the current stroke without removing completed drawings.
    public void StopDrawing()
    {
        isDrawing = false;

        // Remove incomplete strokes.
        if (currentLine != null && rawPoints.Count < 2)
        {
            Destroy(currentLine.gameObject);
        }

        currentLine = null;
        rawPoints.Clear();
    }

    private void CreateLine()
    {
        rawPoints.Clear();

        GameObject lineObject = new GameObject("AirStroke");

        currentLine = lineObject.AddComponent<LineRenderer>();

        // Keep the stroke independent of the pen's movements.
        currentLine.useWorldSpace = true;

        currentLine.sharedMaterial = lineMaterial;

        currentLine.startColor = color;
        currentLine.endColor = color;

        currentLine.startWidth = width;
        currentLine.endWidth = width;

        currentLine.numCapVertices = 8;
        currentLine.numCornerVertices = 8;

        currentLine.positionCount = 0;

        AddPoint(tip.position);
    }

    private void AddPoint(Vector3 position)
    {
        rawPoints.Add(position);
        lastPosition = position;

        UpdateLine();
    }

    // Update the displayed stroke with optional spline interpolation.
    private void UpdateLine()
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

        currentLine.positionCount = displayPoints.Count;
        currentLine.SetPositions(displayPoints.ToArray());
    }
}
