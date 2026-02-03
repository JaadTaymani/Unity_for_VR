using UnityEngine;

public class FCCStructure : MonoBehaviour
{
    public int size = 3; // Number of unit cells in each direction
    public float spacing = 1.5f;
    public float sphereScale = 0.4f;
    public float lineWidth = 0.03f;

    void Start()
    {
        // Center the whole structure around the parent object
        Vector3 offset = Vector3.one * (size - 1) * spacing / 2f;
        float sphereRadius = 0.5f * sphereScale;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    // 1. Create the Corner Atom
                    Vector3 cornerPos = new Vector3(x, y, z) * spacing - offset;
                    CreateAtom(cornerPos);

                    // 2. Create Face-Centered Atoms (Only if within bounds of a cell)
                    // We place atoms at the centers of the XY, XZ, and YZ planes

                    // XY Face center
                    if (x < size - 1 && y < size - 1)
                    {
                        Vector3 faceXY = cornerPos + new Vector3(0.5f, 0.5f, 0) * spacing;
                        CreateAtom(faceXY);
                    }

                    // XZ Face center
                    if (x < size - 1 && z < size - 1)
                    {
                        Vector3 faceXZ = cornerPos + new Vector3(0.5f, 0, 0.5f) * spacing;
                        CreateAtom(faceXZ);
                    }

                    // YZ Face center
                    if (y < size - 1 && z < size - 1)
                    {
                        Vector3 faceYZ = cornerPos + new Vector3(0, 0.5f, 0.5f) * spacing;
                        CreateAtom(faceYZ);
                    }

                    // 3. Create Edges (Standard Grid Lines)
                    if (x < size - 1) CreateEdge(cornerPos, cornerPos + Vector3.right * spacing, sphereRadius);
                    if (y < size - 1) CreateEdge(cornerPos, cornerPos + Vector3.up * spacing, sphereRadius);
                    if (z < size - 1) CreateEdge(cornerPos, cornerPos + Vector3.forward * spacing, sphereRadius);
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
        // Offset the line so it doesn't clip through the center of the sphere
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
