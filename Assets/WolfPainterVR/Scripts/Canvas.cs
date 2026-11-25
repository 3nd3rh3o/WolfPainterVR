using UnityEngine;

public class Canvas : MonoBehaviour
{

    public GameObject tip;
    public Color color;
    [Range(1f, 10f)]
    public float width = 1f;
    public Vector3 n;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        n = transform.rotation * transform.up;
    }

    // Update is called once per frame
    void Update()
    {

        // Etape 2: Calculer l
        // Le compute sert uniquement à mettre à jour la texture!
        Vector3 pos = tip.transform.position;
        n = transform.rotation * transform.up;


    }
}
