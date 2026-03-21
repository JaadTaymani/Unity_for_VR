using System;
using System.Collections.Generic;
using UnityEngine;

/// Generate intersection polygon of Miller plane (hkl) with a unit cube [0,1]^3,
/// using plane equation: h*x + k*y + l*z = 1 (fractional coordinates).
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MillerPlaneMesh : MonoBehaviour
{
    [Header("Miller Indices (h k l)")]
    public int h = 1;
    public int k = 0;
    public int l = 0;

    [Header("Unit Cell")]
    public Transform unitCell;      // assign the Cube transform (scale should be 1,1,1 recommended)
    public float cellSize = 1f;      // if you want a bigger cube visually

    [Header("Mesh Settings")]
    public float pointMergeEps = 1e-4f;

    private MeshFilter mf;

    void Awake()
    {
        mf = GetComponent<MeshFilter>();
    }

    void Start()
    {
        Rebuild();
    }

    void OnValidate()
    {
        if (Application.isPlaying) Rebuild();
    }

    public void Rebuild()
    {
        if (h == 0 && k == 0 && l == 0)
        {
            mf.sharedMesh = null;
            return;
        }

        // Build in fractional cube [0,1]^3, then map to world around unitCell
        var pts = ComputeIntersectionPolygon(h, k, l);
        if (pts.Count < 3)
        {
            mf.sharedMesh = null;
            return;
        }

        // Sort points around centroid within the plane (to form a proper polygon)
        var sorted = SortPolygonInPlane(pts, h, k, l);

        // Triangulate via fan (convex polygon — for cube-plane intersection it's convex)
        var mesh = BuildMesh(sorted);

        // Place mesh in world: align with unit cell cube
        ApplyTransformToMesh(mesh);

        mf.sharedMesh = mesh;
    }

    // --- Geometry core ---

    // 12 edges of cube [0,1]^3 defined by endpoint pairs
    private static readonly (Vector3 a, Vector3 b)[] CubeEdges = new (Vector3, Vector3)[]
    {
        // bottom square z=0
        (new Vector3(0,0,0), new Vector3(1,0,0)),
        (new Vector3(1,0,0), new Vector3(1,1,0)),
        (new Vector3(1,1,0), new Vector3(0,1,0)),
        (new Vector3(0,1,0), new Vector3(0,0,0)),
        // top square z=1
        (new Vector3(0,0,1), new Vector3(1,0,1)),
        (new Vector3(1,0,1), new Vector3(1,1,1)),
        (new Vector3(1,1,1), new Vector3(0,1,1)),
        (new Vector3(0,1,1), new Vector3(0,0,1)),
        // vertical edges
        (new Vector3(0,0,0), new Vector3(0,0,1)),
        (new Vector3(1,0,0), new Vector3(1,0,1)),
        (new Vector3(1,1,0), new Vector3(1,1,1)),
        (new Vector3(0,1,0), new Vector3(0,1,1)),
    };

    // plane function f(p) = h*x + k*y + l*z - 1
    private static float PlaneF(int h, int k, int l, Vector3 p)
        => h * p.x + k * p.y + l * p.z - 1f;

    private List<Vector3> ComputeIntersectionPolygon(int h, int k, int l)
    {
        var intersections = new List<Vector3>();

        foreach (var (a, b) in CubeEdges)
        {
            float fa = PlaneF(h, k, l, a);
            float fb = PlaneF(h, k, l, b);

            // If both exactly on plane (edge lies in plane) -> add endpoints
            // This happens for planes coincident with cube faces, e.g., (100): x=1 includes top/bottom edges on x=1 face.
            if (Mathf.Abs(fa) < 1e-6f && Mathf.Abs(fb) < 1e-6f)
            {
                AddUnique(intersections, a);
                AddUnique(intersections, b);
                continue;
            }

            // If one endpoint on plane
            if (Mathf.Abs(fa) < 1e-6f) AddUnique(intersections, a);
            if (Mathf.Abs(fb) < 1e-6f) AddUnique(intersections, b);

            // Proper crossing
            if (fa * fb < 0f)
            {
                float t = fa / (fa - fb); // in (0,1)
                Vector3 p = a + t * (b - a);
                AddUnique(intersections, p);
            }
        }

        return intersections;
    }

    private void AddUnique(List<Vector3> list, Vector3 p)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if ((list[i] - p).sqrMagnitude < pointMergeEps * pointMergeEps)
                return;
        }
        list.Add(p);
    }

    private List<Vector3> SortPolygonInPlane(List<Vector3> pts, int h, int k, int l)
    {
        // plane normal n = (h,k,l)
        Vector3 n = new Vector3(h, k, l).normalized;

        // centroid
        Vector3 c = Vector3.zero;
        foreach (var p in pts) c += p;
        c /= pts.Count;

        // pick a reference axis u in plane: any vector not parallel to n
        Vector3 refV = (Mathf.Abs(Vector3.Dot(n, Vector3.up)) < 0.9f) ? Vector3.up : Vector3.right;
        Vector3 u = Vector3.Cross(n, refV).normalized;
        Vector3 v = Vector3.Cross(n, u).normalized;

        // sort by angle around centroid
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

    private Mesh BuildMesh(List<Vector3> poly)
    {
        var mesh = new Mesh();
        mesh.name = $"MillerPlane_{h}{k}{l}";

        // vertices: polygon points
        Vector3[] verts = poly.ToArray();

        // triangles: fan from 0
        int triCount = (poly.Count - 2);
        int[] tris = new int[triCount * 3];
        for (int i = 0; i < triCount; i++)
        {
            tris[3 * i + 0] = 0;
            tris[3 * i + 1] = i + 1;
            tris[3 * i + 2] = i + 2;
        }

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private void ApplyTransformToMesh(Mesh mesh)
    {
        // Map fractional [0,1]^3 to cube centered at unitCell, scaled by cellSize
        // Unity Cube default is centered at (0,0,0) with size 1, so we shift by -0.5 to center.
        var verts = mesh.vertices;
        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 p = verts[i];
            p = (p - new Vector3(0.5f, 0.5f, 0.5f)) * cellSize;

            if (unitCell != null)
            {
                // place in unitCell local space then to world
                p = unitCell.TransformPoint(p);
                // bring back into this object's local space
                p = transform.InverseTransformPoint(p);
            }
            verts[i] = p;
        }
        mesh.vertices = verts;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}
