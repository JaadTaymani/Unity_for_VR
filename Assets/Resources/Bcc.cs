using UnityEngine;

public class BCCStructure : MonoBehaviour
{
    public int size = 3;
    public float spacing = 1.5f;
    public float sphereScale = 0.4f;
    public float lineWidth = 0.03f;

    void Start()
    {
        Vector3 offset = Vector3.one * (size - 1) * spacing / 2f;
        float sphereRadius = 0.5f * sphereScale;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                for (int z = 0; z < size; z++)
                {
                    // Corner atom
                    Vector3 cornerPos = new Vector3(x, y, z) * spacing - offset;
                    CreateSphere(cornerPos);

                    // Body-center atom (only if inside lattice)
                    if (x < size - 1 && y < size - 1 && z < size - 1)
                    {
                        Vector3 centerPos = cornerPos + Vector3.one * (spacing * 0.5f);
                        CreateSphere(centerPos);

                        // Connect center to 8 corners
                        for (int dx = 0; dx <= 1; dx++)
                            for (int dy = 0; dy <= 1; dy++)
                                for (int dz = 0; dz <= 1; dz++)
                                {
                                    Vector3 otherCorner =
                                        new Vector3(x + dx, y + dy, z + dz) * spacing - offset;

                                    CreateEdge(centerPos, otherCorner, sphereRadius);
                                }
                    }
                }
    }

    void CreateSphere(Vector3 pos)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.SetParent(transform, false);
        sphere.transform.localPosition = pos;
        sphere.transform.localScale = Vector3.one * sphereScale;
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
