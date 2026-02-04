using UnityEngine;

public class SimpleCubicStructure : MonoBehaviour
{
    public int size = 3;
    public float spacing = 1.5f;
    public float sphereScale = 0.4f;
    public float lineWidth = 0.03f;

    void Start()
    {
        BuildGrid();
    }

    public void IncreaseSize()
    {
        size += 1;
        BuildGrid();
    }

    void BuildGrid()
    {
        // Clear previous grid
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        Vector3 offset = Vector3.one * (size - 1) * spacing / 2f;
        float sphereRadius = 0.5f * sphereScale;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                for (int z = 0; z < size; z++)
                {
                    Vector3 pos = new Vector3(x, y, z) * spacing - offset;

                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.transform.SetParent(transform, false);
                    sphere.transform.localPosition = pos;
                    sphere.transform.localScale = Vector3.one * sphereScale;

                    if (x < size - 1)
                        CreateEdge(pos, pos + Vector3.right * spacing, sphereRadius);

                    if (y < size - 1)
                        CreateEdge(pos, pos + Vector3.up * spacing, sphereRadius);

                    if (z < size - 1)
                        CreateEdge(pos, pos + Vector3.forward * spacing, sphereRadius);
                }
    }

    void CreateEdge(Vector3 start, Vector3 end, float radius)
    {
        Vector3 dir = (end - start).normalized;
        start += dir * radius;
        end -= dir * radius;

        GameObject edge = new GameObject("Edge");
        edge.transform.SetParent(transform, false);

        LineRenderer lr = edge.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.material = new Material(Shader.Find("Sprites/Default"));
    }
}
