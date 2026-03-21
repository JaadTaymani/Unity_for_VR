using UnityEngine;

public class SCUnitCell : MonoBehaviour
{
    public float ax = 1.5f;
    public float ay = 1.5f;
    public float az = 1.5f;

    public float sphereScale = 0.4f;
    public float lineWidth = 0.03f;

    void Start()
    {
        BuildCell();
    }

    public void BuildCell()
    {
        foreach (Transform child in transform) Destroy(child.gameObject);

        float r = 0.5f * sphereScale;

        // define 8 corners (local space), centered at origin
        Vector3[] c =
        {
            new Vector3(0,0,0), new Vector3(ax,0,0), new Vector3(0,ay,0), new Vector3(ax,ay,0),
            new Vector3(0,0,az), new Vector3(ax,0,az), new Vector3(0,ay,az), new Vector3(ax,ay,az)
        };

        // center it
        Vector3 offset = new Vector3(ax, ay, az) * 0.5f;
        for (int i = 0; i < c.Length; i++) c[i] -= offset;

        // create corner atoms
        for (int i = 0; i < c.Length; i++)
        {
            var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            s.name = $"Corner_{i}";
            s.transform.SetParent(transform, false);
            s.transform.localPosition = c[i];
            s.transform.localScale = Vector3.one * sphereScale;
        }

        // 12 edges by index pairs
        int[,] edges =
        {
            {0,1},{0,2},{1,3},{2,3},
            {4,5},{4,6},{5,7},{6,7},
            {0,4},{1,5},{2,6},{3,7}
        };

        for (int e = 0; e < 12; e++)
            CreateEdge(c[edges[e,0]], c[edges[e,1]], r);
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
