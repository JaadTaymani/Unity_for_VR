using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MillerPlaneInCell : MonoBehaviour
{
    [Header("Miller indices (h k l)")]
    public int h = 1;
    public int k = 1;
    public int l = 0;

    [Header("Link to your unit cell script (for spacing)")]
    public SimpleCubicStructure unitCell;  // drag UnitCell here
    public float pointMergeEps = 1e-4f;

    MeshFilter mf;

    void Awake() => mf = GetComponent<MeshFilter>();
    void Start() => Rebuild();

    public void Rebuild()
    {
        if (unitCell == null) { mf.sharedMesh = null; return; }
        if (h == 0 && k == 0 && l == 0) { mf.sharedMesh = null; return; }

        var pts = ComputeIntersectionPolygon(h, k, l);
        if (pts.Count < 3) { mf.sharedMesh = null; return; }

        var sorted = SortPolygonInPlane(pts, h, k, l);
        var mesh = BuildMesh(sorted);

        MapToLocalCell(mesh, unitCell.spacing);
        mf.sharedMesh = mesh;
    }

    // plane on fractional cube [0,1]^3: h*x + k*y + l*z = 1
    static float F(int h, int k, int l, Vector3 p) => h * p.x + k * p.y + l * p.z - 1f;

    static readonly (Vector3 a, Vector3 b)[] Edges =
    {
        (new(0,0,0), new(1,0,0)),
        (new(1,0,0), new(1,1,0)),
        (new(1,1,0), new(0,1,0)),
        (new(0,1,0), new(0,0,0)),
        (new(0,0,1), new(1,0,1)),
        (new(1,0,1), new(1,1,1)),
        (new(1,1,1), new(0,1,1)),
        (new(0,1,1), new(0,0,1)),
        (new(0,0,0), new(0,0,1)),
        (new(1,0,0), new(1,0,1)),
        (new(1,1,0), new(1,1,1)),
        (new(0,1,0), new(0,1,1)),
    };

    List<Vector3> ComputeIntersectionPolygon(int h, int k, int l)
    {
        var list = new List<Vector3>();

        foreach (var (a, b) in Edges)
        {
            float fa = F(h, k, l, a);
            float fb = F(h, k, l, b);

            if (Mathf.Abs(fa) < 1e-6f && Mathf.Abs(fb) < 1e-6f)
            {
                AddUnique(list, a);
                AddUnique(list, b);
                continue;
            }

            if (Mathf.Abs(fa) < 1e-6f) AddUnique(list, a);
            if (Mathf.Abs(fb) < 1e-6f) AddUnique(list, b);

            if (fa * fb < 0f)
            {
                float t = fa / (fa - fb);
                AddUnique(list, a + t * (b - a));
            }
        }

        return list;
    }

    void AddUnique(List<Vector3> list, Vector3 p)
    {
        float eps2 = pointMergeEps * pointMergeEps;
        for (int i = 0; i < list.Count; i++)
            if ((list[i] - p).sqrMagnitude < eps2) return;
        list.Add(p);
    }

    List<Vector3> SortPolygonInPlane(List<Vector3> pts, int h, int k, int l)
    {
        Vector3 n = new Vector3(h, k, l).normalized;

        Vector3 c = Vector3.zero;
        foreach (var p in pts) c += p;
        c /= pts.Count;

        Vector3 refV = (Mathf.Abs(Vector3.Dot(n, Vector3.up)) < 0.9f) ? Vector3.up : Vector3.right;
        Vector3 u = Vector3.Cross(n, refV).normalized;
        Vector3 v = Vector3.Cross(n, u).normalized;

        pts.Sort((p1, p2) =>
        {
            Vector3 d1 = p1 - c;
            Vector3 d2 = p2 - c;
            float a1 = Mathf.Atan2(Vector3.Dot(d1, v), Vector3.Dot(d1, u));
            float a2 = Mathf.Atan2(Vector3.Dot(d2, v), Vector3.Dot(d2, u));
            return a1.CompareTo(a2);
        });

        return pts;
    }

    Mesh BuildMesh(List<Vector3> poly)
    {
        var mesh = new Mesh();
        mesh.name = $"MillerPlane_{h}{k}{l}";
        mesh.vertices = poly.ToArray();

        int triCount = poly.Count - 2;
        int[] tris = new int[triCount * 3];
        for (int i = 0; i < triCount; i++)
        {
            tris[3*i+0] = 0;
            tris[3*i+1] = i + 1;
            tris[3*i+2] = i + 2;
        }

        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    void MapToLocalCell(Mesh mesh, float a)
    {
        var verts = mesh.vertices;
        for (int i = 0; i < verts.Length; i++)
        {
            // fractional [0,1] -> centered [-0.5,0.5] -> scale by a=spacing
            verts[i] = (verts[i] - new Vector3(0.5f, 0.5f, 0.5f)) * a;
        }
        mesh.vertices = verts;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    public void SetHKL(int newH, int newK, int newL)
{
    h = newH;
    k = newK;
    l = newL;
    Rebuild();
}

public Vector3Int GetHKL() => new Vector3Int(h, k, l);

}
