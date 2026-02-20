using System.Collections.Generic;
using UnityEngine;

public class WignerSeitzHex : MonoBehaviour
{
    public int radius = 3;
    public float latticeSpacing = 1f;

    public float pointRadius = 0.05f;
    public float bisectorLength = 0.6f;

    public Color cellColor = new Color(1f, 0.6f, 0.3f);
    public Color bisectorColor = Color.black;
    public Color pointColor = Color.black;

    void OnDrawGizmos()
    {
        List<Vector2> points = GenerateHexLattice();

        foreach (var p in points)
        {
            List<Vector2> neighbors = GetNearestNeighbors(p, points);

            // Only interior hex cells
            if (neighbors.Count != 6)
                continue;

            Vector3 centerWorld = LocalToWorld(p);

            // ---- central dot ----
            Gizmos.color = pointColor;
            Gizmos.DrawSphere(centerWorld, pointRadius);

            // ---- perpendicular bisectors ----
            Gizmos.color = bisectorColor;
            foreach (var n in neighbors)
            {
                Vector2 mid = (p + n) * 0.5f;
                Vector2 dir = (n - p).normalized;
                Vector2 perp = new Vector2(-dir.y, dir.x);

                Vector3 a = LocalToWorld(mid + perp * bisectorLength);
                Vector3 b = LocalToWorld(mid - perp * bisectorLength);

                Gizmos.DrawLine(a, b);
            }

            // ---- Wigner–Seitz polygon ----
            List<Vector2> cell = ComputeWignerSeitzCell(p, neighbors);

            Gizmos.color = cellColor;
            for (int i = 0; i < cell.Count; i++)
            {
                Vector3 a = LocalToWorld(cell[i]);
                Vector3 b = LocalToWorld(cell[(i + 1) % cell.Count]);
                Gizmos.DrawLine(a, b);
            }
        }
    }

    Vector3 LocalToWorld(Vector2 v)
    {
        return transform.TransformPoint(new Vector3(v.x, v.y, 0f));
    }

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
            if (Vector2.Distance(p, o) < latticeSpacing * 1.1f)
                neighbors.Add(o);
        }

        return neighbors;
    }

    List<Vector2> ComputeWignerSeitzCell(Vector2 center, List<Vector2> neighbors)
    {
        List<HalfPlane> planes = new List<HalfPlane>();

        foreach (var n in neighbors)
        {
            Vector2 mid = (center + n) * 0.5f;
            Vector2 normal = (n - center).normalized;
            planes.Add(new HalfPlane(mid, normal));
        }

        List<Vector2> poly = new List<Vector2>()
        {
            new Vector2(-2, -2),
            new Vector2( 2, -2),
            new Vector2( 2,  2),
            new Vector2(-2,  2)
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
            float t = Vector2.Dot(p - a, n) / Vector2.Dot(ab, n);
            return a + t * ab;
        }
    }
}
