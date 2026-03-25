using System.Collections.Generic;
using UnityEngine;

public class WignerSeitzHexVR : MonoBehaviour
{
    public int radius = 2;
    public float latticeSpacing = 1.2f;

    public float lineWidth = 0.03f;
    public float pointSize = 0.1f;

    Material lineMaterial;
    Material hexMaterial;
    Material centerHexMaterial;

    void Start()
    {
        // --- Materials (VR safe) ---
        lineMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        lineMaterial.color = Color.red;

        hexMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        hexMaterial.color = new Color(0.7f, 0.9f, 0.5f);

        centerHexMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        centerHexMaterial.color = new Color(1f, 0.75f, 0.5f);

        Generate();
    }

    void Generate()
    {
        List<Vector2> points = GenerateHexLattice();
        Vector2 centerPoint = Vector2.zero;

        foreach (var p in points)
        {
            Vector3 center = ToWorld(p);

            // ---- DRAW HEX GRID ----
            bool isCenter = (p == centerPoint);
            CreateHex(center, isCenter ? centerHexMaterial : hexMaterial);

            List<Vector2> neighbors = GetNearestNeighbors(p, points);

            if (p != centerPoint || neighbors.Count != 6)
                continue;

            // ---- CENTER POINT ----
            CreatePoint(center, Color.red);

            foreach (var n in neighbors)
            {
                CreatePoint(ToWorld(n), new Color(1f, 0.4f, 0.3f));

                // ---- BISECTOR ----
                Vector2 mid = (p + n) * 0.5f;
                Vector2 dir = (n - p).normalized;
                Vector2 perp = new Vector2(-dir.y, dir.x);

                Vector3 a = ToWorld(mid + perp * 0.8f);
                Vector3 b = ToWorld(mid - perp * 0.8f);

                DrawLine(a, b, Color.red);
            }

            // ---- WIGNER-SEITZ CELL ----
            List<Vector2> cell = ComputeWignerSeitzCell(p, neighbors);

            for (int i = 0; i < cell.Count; i++)
            {
                Vector3 a = ToWorld(cell[i]);
                Vector3 b = ToWorld(cell[(i + 1) % cell.Count]);

                DrawLine(a, b, Color.red);
            }
        }
    }

    // ---------- HEX ----------
    void CreateHex(Vector3 center, Material mat)
    {
        GameObject hex = new GameObject("Hex");
        hex.transform.SetParent(transform);
        hex.transform.position = center;

        MeshFilter mf = hex.AddComponent<MeshFilter>();
        MeshRenderer mr = hex.AddComponent<MeshRenderer>();
        mr.material = mat;

        Mesh mesh = new Mesh();

        float r = latticeSpacing * 0.6f;

        Vector3[] verts = new Vector3[7];
        verts[0] = Vector3.zero;

        for (int i = 0; i < 6; i++)
        {
            float angle = Mathf.Deg2Rad * (60 * i + 30);
            verts[i + 1] = new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r, 0);
        }

        List<int> tris = new List<int>();
        for (int i = 1; i <= 6; i++)
        {
            tris.Add(0);
            tris.Add(i);
            tris.Add(i == 6 ? 1 : i + 1);
        }

        mesh.vertices = verts;
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();

        mf.mesh = mesh;

        // ---- outline ----
        LineRenderer lr = hex.AddComponent<LineRenderer>();
        lr.positionCount = 7;
        lr.loop = true;

        for (int i = 0; i < 6; i++)
            lr.SetPosition(i, verts[i + 1]);

        lr.SetPosition(6, verts[1]);

        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;

        lr.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        lr.startColor = new Color(0.3f, 0.6f, 0.2f);
        lr.endColor = new Color(0.3f, 0.6f, 0.2f);

        lr.useWorldSpace = false;
    }

    void CreatePoint(Vector3 pos, Color color)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = pos;
        sphere.transform.localScale = Vector3.one * pointSize;
        sphere.transform.SetParent(transform);

        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.color = color;
        sphere.GetComponent<Renderer>().material = mat;
    }

    Vector3 ToWorld(Vector2 v)
    {
        return transform.TransformPoint(new Vector3(v.x, v.y, 2f));
    }

    void DrawLine(Vector3 a, Vector3 b, Color color)
    {
        GameObject lineObj = new GameObject("Line");
        lineObj.transform.SetParent(transform);

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, a);
        lr.SetPosition(1, b);

        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;

        lr.material = lineMaterial;
        lr.startColor = color;
        lr.endColor = color;

        lr.useWorldSpace = true;
    }

    // ---------- LATTICE ----------
    List<Vector2> GenerateHexLattice()
    {
        List<Vector2> pts = new List<Vector2>();

        float dx = latticeSpacing;
        float dy = Mathf.Sqrt(3f) * latticeSpacing / 2f;

        for (int q = -radius; q <= radius; q++)
        {
            for (int r = -radius; r <= radius; r++)
            {
                float x = dx * (q + 0.5f * r);
                float y = dy * r;
                pts.Add(new Vector2(x, y));
            }
        }

        return pts;
    }

    List<Vector2> GetNearestNeighbors(Vector2 p, List<Vector2> points)
    {
        List<Vector2> neighbors = new List<Vector2>();

        foreach (var o in points)
        {
            if (o == p) continue;

            float d = Vector2.Distance(p, o);

            if (d < latticeSpacing * 1.2f && d > 0.01f)
                neighbors.Add(o);
        }

        return neighbors;
    }

    // ---------- WIGNER-SEITZ ----------
    List<Vector2> ComputeWignerSeitzCell(Vector2 center, List<Vector2> neighbors)
    {
        List<HalfPlane> planes = new List<HalfPlane>();

        foreach (var n in neighbors)
        {
            Vector2 mid = (center + n) * 0.5f;
            Vector2 normal = (n - center).normalized;
            planes.Add(new HalfPlane(mid, normal));
        }

        float size = 20f;

        List<Vector2> poly = new List<Vector2>()
        {
            new Vector2(-size, -size),
            new Vector2( size, -size),
            new Vector2( size,  size),
            new Vector2(-size,  size)
        };

        foreach (var hp in planes)
            poly = ClipPolygon(poly, hp);

        return poly;
    }

    List<Vector2> ClipPolygon(List<Vector2> poly, HalfPlane plane)
    {
        List<Vector2> result = new List<Vector2>();

        for (int i = 0; i < poly.Count; i++)
        {
            Vector2 a = poly[i];
            Vector2 b = poly[(i + 1) % poly.Count];

            bool aIn = plane.IsInside(a);
            bool bIn = plane.IsInside(b);

            if (aIn && bIn)
                result.Add(b);
            else if (aIn && !bIn)
                result.Add(plane.Intersect(a, b));
            else if (!aIn && bIn)
            {
                result.Add(plane.Intersect(a, b));
                result.Add(b);
            }
        }

        return result;
    }

    struct HalfPlane
    {
        public Vector2 p;
        public Vector2 n;

        public HalfPlane(Vector2 point, Vector2 normal)
        {
            p = point;
            n = normal;
        }

        public bool IsInside(Vector2 v)
        {
            return Vector2.Dot(v - p, n) <= 0f;
        }

        public Vector2 Intersect(Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float denom = Vector2.Dot(ab, n);

            if (Mathf.Abs(denom) < 0.0001f)
                return a;

            float t = Vector2.Dot(p - a, n) / denom;
            return a + t * ab;
        }
    }
}