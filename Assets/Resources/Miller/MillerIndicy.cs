using UnityEngine;
using TMPro;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MillerIndexGenerator : MonoBehaviour
{
    [Range(0, 10)] public int h = 1;
    [Range(0, 10)] public int k = 1;
    [Range(0, 10)] public int l = 1;
    public float scale = 5f;
    public TextMeshProUGUI millerText;

    void OnValidate() { GeneratePlane(); UpdateMillerText(); }

    void UpdateMillerText()
    {
        if (millerText != null)
            millerText.text = $"h = {h}   k = {k}   l = {l}";
    }

    public void IncreaseSizeH() { h = Mathf.Min(h + 1, 10); GeneratePlane(); UpdateMillerText(); }
    public void IncreaseSizeK() { k = Mathf.Min(k + 1, 10); GeneratePlane(); UpdateMillerText(); }
    public void IncreaseSizeL() { l = Mathf.Min(l + 1, 10); GeneratePlane(); UpdateMillerText(); }
    public void DecreaseSizeH() { h = Mathf.Max(h - 1, 0); GeneratePlane(); UpdateMillerText(); }
    public void DecreaseSizeK() { k = Mathf.Max(k - 1, 0); GeneratePlane(); UpdateMillerText(); }
    public void DecreaseSizeL() { l = Mathf.Max(l - 1, 0); GeneratePlane(); UpdateMillerText(); }

    // ---------- PLANE GENERATION ----------
    void GeneratePlane()
{
    var mf = GetComponent<MeshFilter>();

    if (h == 0 && k == 0 && l == 0) { mf.mesh = null; return; }

    float s = scale;
    Vector3 normal = new Vector3(h, k, l);
    float d = s;

    Vector3 n = normal.normalized;
    Vector3 t1 = Vector3.Cross(n, Mathf.Abs(n.x) < 0.9f ? Vector3.right : Vector3.up).normalized;
    Vector3 t2 = Vector3.Cross(n, t1).normalized;

    Vector3 pointOnPlane = FindPointOnPlane(h, k, l, d);

    float big = s * 4f;
    List<Vector3> poly = new List<Vector3>
    {
        pointOnPlane + ( t1 + t2) * big,
        pointOnPlane + (-t1 + t2) * big,
        pointOnPlane + (-t1 - t2) * big,
        pointOnPlane + ( t1 - t2) * big,
    };

    poly = ClipByPlane(poly, new Vector3( 1, 0, 0),  0);
    poly = ClipByPlane(poly, new Vector3(-1, 0, 0),  s);  
    poly = ClipByPlane(poly, new Vector3( 0, 1, 0),  0);
    poly = ClipByPlane(poly, new Vector3( 0,-1, 0),  s);  // was -s 
    poly = ClipByPlane(poly, new Vector3( 0, 0, 1),  0);
    poly = ClipByPlane(poly, new Vector3( 0, 0,-1),  s);  

    if (poly == null || poly.Count < 3) { mf.mesh = null; return; }

    Vector3 center = Vector3.zero;
    foreach (var p in poly) center += p;
    center /= poly.Count;

    Vector3 ref0 = (poly[0] - center).normalized;
    poly.Sort((a, b) =>
    {
        float angleA = SignedAngle(ref0, (a - center).normalized, n);
        float angleB = SignedAngle(ref0, (b - center).normalized, n);
        return angleA.CompareTo(angleB);
    });

    List<int> tris = new List<int>();
    for (int i = 1; i < poly.Count - 1; i++)
    {
        tris.Add(0); tris.Add(i); tris.Add(i + 1);
        tris.Add(0); tris.Add(i + 1); tris.Add(i); // back face
    }

    Mesh mesh = new Mesh();
    mesh.vertices = poly.ToArray();
    mesh.triangles = tris.ToArray();
    mesh.RecalculateNormals();
    mesh.RecalculateBounds();
    mf.mesh = mesh;
}
    // Sutherland-Hodgman clip against half-space: dot(clipNormal, p) + d >= 0
    List<Vector3> ClipByPlane(List<Vector3> poly, Vector3 clipNormal, float d)
    {
        if (poly == null || poly.Count == 0) return null;
        List<Vector3> output = new List<Vector3>();
        int count = poly.Count;

        for (int i = 0; i < count; i++)
        {
            Vector3 cur  = poly[i];
            Vector3 next = poly[(i + 1) % count];
            float dc = Vector3.Dot(clipNormal, cur)  + d;
            float dn = Vector3.Dot(clipNormal, next) + d;

            if (dc >= -1e-5f) output.Add(cur);

            if ((dc > 1e-5f && dn < -1e-5f) || (dc < -1e-5f && dn > 1e-5f))
            {
                float t = dc / (dc - dn);
                output.Add(Vector3.Lerp(cur, next, t));
            }
        }
        return output.Count < 3 ? null : output;
    }

    // Find a point on the plane h*x + k*y + l*z = d
    Vector3 FindPointOnPlane(int h, int k, int l, float d)
    {
        if (h != 0) return new Vector3(d / h, 0, 0);
        if (k != 0) return new Vector3(0, d / k, 0);
        return new Vector3(0, 0, d / l);
    }

    float SignedAngle(Vector3 from, Vector3 to, Vector3 axis)
    {
        float angle = Mathf.Acos(Mathf.Clamp(Vector3.Dot(from, to), -1f, 1f));
        Vector3 cross = Vector3.Cross(from, to);
        if (Vector3.Dot(axis, cross) < 0) angle = -angle;
        return angle;
    }

    // ---------- GIZMOS ----------
    void OnDrawGizmos()
    {
        Vector3 origin = transform.position + Vector3.one * scale * 0.5f;
        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube(origin, Vector3.one * scale);
#if UNITY_EDITOR
        Handles.Label(origin + Vector3.right   * scale * 0.55f, "X");
        Handles.Label(origin + Vector3.up      * scale * 0.55f, "Y");
        Handles.Label(origin + Vector3.forward * scale * 0.55f, "Z");
#endif
    }
}