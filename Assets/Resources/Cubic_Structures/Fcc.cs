using UnityEngine;

public class FCCStructure : MonoBehaviour
{
    public int size = 3;          // Number of unit cells in each direction
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
        if (size > 5)
            size = 5;

        BuildGrid();
    }

    public void DecreaseSize()
    {
        size -= 1;
        if (size < 2)
            size = 2;

        BuildGrid();
    }

    void BuildGrid()
    {
        // Clear previous structure
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Center the whole structure around the parent object
        Vector3 offset = Vector3.one * (size - 1) * spacing / 2f;
        float sphereRadius = 0.5f * sphereScale;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    // Corner atom
                    Vector3 cornerPos = new Vector3(x, y, z) * spacing - offset;
                    CreateAtom(cornerPos);

                    // Face-centered atoms
                    if (x < size - 1 && y < size - 1)
                        CreateAtom(cornerPos + new Vector3(0.5f, 0.5f, 0) * spacing);

                    if (x < size - 1 && z < size - 1)
                        CreateAtom(cornerPos + new Vector3(0.5f, 0, 0.5f) * spacing);

                    if (y < size - 1 && z < size - 1)
                        CreateAtom(cornerPos + new Vector3(0, 0.5f, 0.5f) * spacing);

                    // Edges
                    if (x < size - 1)
                        CreateEdge(cornerPos, cornerPos + Vector3.right * spacing, sphereRadius);

                    if (y < size - 1)
                        CreateEdge(cornerPos, cornerPos + Vector3.up * spacing, sphereRadius);

                    if (z < size - 1)
                        CreateEdge(cornerPos, cornerPos + Vector3.forward * spacing, sphereRadius);
                }
            }
        }
    }

    void CreateAtom(Vector3 position)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.SetParent(transform, false);
        sphere.transform.localPosition = position;
        sphere.transform.localScale = Vector3.one * sphereScale;
    }

    void CreateEdge(Vector3 start, Vector3 end, float radius)
    {
        Vector3 dir = (end - start).normalized;

        // Offset line so it doesn't clip through spheres
        Vector3 lineStart = start + dir * radius;
        Vector3 lineEnd = end - dir * radius;

        GameObject edge = new GameObject("Edge");
        edge.transform.SetParent(transform, false);

        LineRenderer lr = edge.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.positionCount = 2;
        lr.SetPosition(0, lineStart);
        lr.SetPosition(1, lineEnd);
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.material = new Material(Shader.Find("Sprites/Default"));
    }
}
