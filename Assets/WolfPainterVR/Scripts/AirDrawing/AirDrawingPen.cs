
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

    private StrokeRenderer currentStroke;
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

        CreateStroke(selectedMaterial);
    }

    public void StopDrawing()
    {
        isDrawing = false;

        // Remove incomplete strokes.
        if (currentStroke != null && rawPoints.Count < 2)
        {
            Destroy(currentStroke.gameObject);
        }

        currentStroke = null;
        rawPoints.Clear();
    }

    // Set the color for future strokes.
    public void SetColor(Color newColor)
    {
        color = newColor;
    }

    // Return the current pen color.
    public Color GetColor()
    {
        return color;
    }

    private void CreateStroke(Material material)
    {
        rawPoints.Clear();

        GameObject strokeObject = new GameObject("AirStroke");

        // Select the rendering strategy.
        switch (renderMode)
        {
            case StrokeRenderMode.LineRenderer:

                strokeObject.name = "AirStroke_Line";

                currentStroke =
                    strokeObject.AddComponent<LineStrokeRenderer>();

                break;

            case StrokeRenderMode.TubeMesh:

                strokeObject.name = "AirStroke_Tube";

                TubeStrokeRenderer tubeRenderer =
                    strokeObject.AddComponent<TubeStrokeRenderer>();

                tubeRenderer.SetRadialSegments(radialSegments);

                currentStroke = tubeRenderer;

                break;
        }

        currentStroke.Initialize(material, color, width);

        AddPoint(tip.position);
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

        // Delegate geometry updates to the rendering strategy.
        currentStroke.UpdateStroke(displayPoints);
    }
}
