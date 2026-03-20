using UnityEngine;
using TMPro;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Miller : MonoBehaviour
{
    [Header("Miller Indices")]
    [Range(0, 10)] public int h = 1;
    [Range(0, 10)] public int k = 1;
    [Range(0, 10)] public int l = 1;

    [Header("Crystal Settings")]
    public int N = 2;
    public float spacing = 1.3f;

    public TextMeshProUGUI millerText;

    float scale;
    Vector3 offset;

    void OnValidate()
    {
        MI_UpdateCrystalData();
        MI_GeneratePlane();
        MI_UpdateMillerText();
    }

    void MI_UpdateCrystalData()
    {
        scale = (N - 1) * spacing;
        offset = new Vector3(scale / 2f, scale / 2f, scale / 2f);
    }

    void MI_UpdateMillerText()
    {
        if (millerText != null)
            millerText.text = $"({h} {k} {l})";
    }

    void MI_Refresh()
    {
        MI_UpdateCrystalData();
        MI_GeneratePlane();
        MI_UpdateMillerText();
    }

    public void IncreaseSizeH() { h++; MI_Refresh(); }
    public void IncreaseSizeK() { k++; MI_Refresh(); }
    public void IncreaseSizeL() { l++; MI_Refresh(); }

    public void DecreaseSizeH() { h--; MI_Refresh(); }
    public void DecreaseSizeK() { k--; MI_Refresh(); }
    public void DecreaseSizeL() { l--; MI_Refresh(); }

    // ---------- PLANE ----------
    void MI_GeneratePlane()
    {
        var mf = GetComponent<MeshFilter>();

        if (h == 0 && k == 0 && l == 0)
        {
            mf.mesh = null;
            return;
        }

        float s = scale;

        Vector3 normal = new Vector3(h, k, l);
        Vector3 n = normal.normalized;

        float d = s;

        Vector3 t1 = Vector3.Cross(n, Mathf.Abs(n.x) < 0.9f ? Vector3.right : Vector3.up).normalized;
        Vector3 t2 = Vector3.Cross(n, t1).normalized;

        Vector3 pointOnPlane = MI_FindPointOnPlane(h, k, l, d);

        float big = s * 2f;

        List<Vector3> poly = new List<Vector3>
        {
            pointOnPlane + ( t1 + t2) * big,
            pointOnPlane + (-t1 + t2) * big,
            pointOnPlane + (-t1 - t2) * big,
            pointOnPlane + ( t1 - t2) * big,
        };

        poly = MI_ClipByPlane(poly, new Vector3( 1, 0, 0),  0);
        poly = MI_ClipByPlane(poly, new Vector3(-1, 0, 0),  s);

        poly = MI_ClipByPlane(poly, new Vector3( 0, 1, 0),  0);
        poly = MI_ClipByPlane(poly, new Vector3( 0,-1, 0),  s);

        poly = MI_ClipByPlane(poly, new Vector3( 0, 0, 1),  0);
        poly = MI_ClipByPlane(poly, new Vector3( 0, 0,-1),  s);

        if (poly == null || poly.Count < 3)
        {
            mf.mesh = null;
            return;
        }

        for (int i = 0; i < poly.Count; i++)
        {
            poly[i] -= offset;
        }

        Vector3 center = Vector3.zero;
        foreach (var p in poly) center += p;
        center /= poly.Count;

        Vector3 ref0 = (poly[0] - center).normalized;

        poly.Sort((a, b) =>
        {
            float angleA = MI_SignedAngle(ref0, (a - center).normalized, n);
            float angleB = MI_SignedAngle(ref0, (b - center).normalized, n);
            return angleA.CompareTo(angleB);
        });

        List<int> tris = new List<int>();
        for (int i = 1; i < poly.Count - 1; i++)
        {
            tris.Add(0); tris.Add(i); tris.Add(i + 1);
            tris.Add(0); tris.Add(i + 1); tris.Add(i);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = poly.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        mf.mesh = mesh;
    }

    // ---------- HELPERS ----------
    List<Vector3> MI_ClipByPlane(List<Vector3> poly, Vector3 normal, float d)
    {
        if (poly == null || poly.Count == 0) return null;

        List<Vector3> output = new List<Vector3>();

        for (int i = 0; i < poly.Count; i++)
        {
            Vector3 cur = poly[i];
            Vector3 next = poly[(i + 1) % poly.Count];

            float dc = Vector3.Dot(normal, cur) + d;
            float dn = Vector3.Dot(normal, next) + d;

            if (dc >= 0) output.Add(cur);

            if ((dc > 0 && dn < 0) || (dc < 0 && dn > 0))
            {
                float t = dc / (dc - dn);
                output.Add(Vector3.Lerp(cur, next, t));
            }
        }

        return output.Count < 3 ? null : output;
    }

    Vector3 MI_FindPointOnPlane(int h, int k, int l, float d)
    {
        if (h != 0) return new Vector3(d / h, 0, 0);
        if (k != 0) return new Vector3(0, d / k, 0);
        return new Vector3(0, 0, d / l);
    }

    float MI_SignedAngle(Vector3 from, Vector3 to, Vector3 axis)
    {
        float angle = Mathf.Acos(Mathf.Clamp(Vector3.Dot(from, to), -1f, 1f));
        if (Vector3.Dot(axis, Vector3.Cross(from, to)) < 0)
            angle = -angle;
        return angle;
    }

    void OnDrawGizmos()
    {
        MI_UpdateCrystalData();
        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(scale, scale, scale));
    }
}