using UnityEngine;
using UnityEngine.SocialPlatforms;

public class Canvas : MonoBehaviour
{
    public GameObject tip;
    public Color color;
    public float width;
    public ComputeShader paintShader;
    public RenderTexture targetTexture;
    public float opacity;
    public float hardness;

    private int kernel;
    private Bounds meshBounds;

    private void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        meshBounds = mesh.bounds;
        kernel = paintShader.FindKernel("CSMain");
        paintShader.SetTexture(kernel, "_Result", targetTexture);
    }

    private void Update()
    {

        Vector3 normal = transform.up;
        Vector3 position = tip.transform.position;
        Vector3 projectedPosition = position -
            normal * Vector3.Dot(position - transform.position, normal);
        Vector3 localPosition = transform.InverseTransformPoint(projectedPosition);

        float u = (localPosition.x - meshBounds.min.x) / meshBounds.size.x;
        float v = (localPosition.z - meshBounds.min.z) / meshBounds.size.z;
        Vector2 pixelPosition = new Vector2(u * targetTexture.width, v * targetTexture.height);
        
        paintShader.SetVector("_Position", new Vector4(pixelPosition.x, pixelPosition.y, 0f, 0f));
        paintShader.SetVector("_Color", color);
        paintShader.SetFloat("_Size", width);
        paintShader.SetFloat("_Opacity", opacity);
        paintShader.SetFloat("_Hardness", hardness);
        paintShader.Dispatch(kernel, 1, 1, 1);
    }
}
