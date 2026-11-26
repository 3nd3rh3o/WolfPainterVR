
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

    [Header("Drawing state")]
    [SerializeField] private bool isDrawing = false;

    private LineRenderer currentLine;
    private Vector3 lastPosition;

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
        }

        Vector3 currentPosition = tip.position;

        if (Vector3.Distance(lastPosition, currentPosition) >= minDistance)
        {
            AddPoint(currentPosition);
        }
    }

    public void StartDrawing()
    {
        if (currentLine != null || tip == null || lineMaterial == null)
        {
            return;
        }

        isDrawing = true;
        CreateLine();
    }

    public void StopDrawing()
    {
        isDrawing = false;

        if (currentLine != null && currentLine.positionCount < 2)
        {
            Destroy(currentLine.gameObject);
        }

        currentLine = null;
    }

    private void CreateLine()
    {
        GameObject lineObject = new GameObject("AirStroke");

        currentLine = lineObject.AddComponent<LineRenderer>();

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
        int index = currentLine.positionCount;

        currentLine.positionCount++;
        currentLine.SetPosition(index, position);

        lastPosition = position;
    }
}
