using UnityEngine;
using TMPro;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MillerIndexGenerator : MonoBehaviour
{
    [Header("Miller Indices")]
    [Range(0, 10)] public int h = 1;
    [Range(0, 10)] public int k = 1;
    [Range(0, 10)] public int l = 1;

    [Header("Settings")]
    public float scale = 5f;
    public TextMeshProUGUI millerText; 
    public bool showAxesInGame = true;

    private void Start()
    {
        GeneratePlane();
        UpdateMillerText();
        
        if (showAxesInGame)
        {
            CreateVisualAxes();
        }
    }

    private void OnValidate()
    {
        if (!Application.isPlaying) return; 
        GeneratePlane();
        UpdateMillerText();
    }

    private void UpdateMillerText()
    {
        if (millerText != null)
            millerText.text = $"h = {h}   k = {k}   l = {l}";
    }

    // ---------- UI CONTROL METHODS ----------
    public void IncreaseSizeH() { h = Mathf.Min(h + 1, 10); GeneratePlane(); UpdateMillerText(); }
    public void IncreaseSizeK() { k = Mathf.Min(k + 1, 10); GeneratePlane(); UpdateMillerText(); }
    public void IncreaseSizeL() { l = Mathf.Min(l + 1, 10); GeneratePlane(); UpdateMillerText(); }
    public void DecreaseSizeH() { h = Mathf.Max(h - 1, 0); GeneratePlane(); UpdateMillerText(); }
    public void DecreaseSizeK() { k = Mathf.Max(k - 1, 0); GeneratePlane(); UpdateMillerText(); }
    public void DecreaseSizeL() { l = Mathf.Max(l - 1, 0); GeneratePlane(); UpdateMillerText(); }

    // ---------- AXIS GENERATION ----------
    private void CreateVisualAxes()
    {
        foreach (Transform child in transform) {
            if (child.name.StartsWith("Axis_") || child.name.StartsWith("Label_")) {
                Destroy(child.gameObject);
            }
        }

        CreateAxisLine(Vector3.right, Color.red, "H");
        CreateAxisLine(Vector3.up, Color.green, "K");
        CreateAxisLine(Vector3.forward, Color.blue, "L");
    }

    private void CreateAxisLine(Vector3 direction, Color color, string label)
    {
        // 1. Create the Cylinder
        GameObject axis = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        axis.name = "Axis_" + label;
        axis.transform.SetParent(this.transform);
        axis.transform.localPosition = direction * (scale * 0.5f);
        axis.transform.up = transform.TransformDirection(direction);
        axis.transform.localScale = new Vector3(0.02f, scale * 0.5f, 0.02f);

        Renderer rend = axis.GetComponent<Renderer>();
        rend.material = new Material(Shader.Find("Unlit/Color"));
        rend.material.color = color;
        Destroy(axis.GetComponent<Collider>());

        // 2. Create the Static Text Label
        GameObject textObj = new GameObject("Label_" + label);
        textObj.transform.SetParent(this.transform);
        
        // Positioned at the tip of the axis
        textObj.transform.localPosition = direction * (scale + 0.2f);
        
        // Reset rotation so it stays fixed in world space
        textObj.transform.localRotation = Quaternion.identity;

        TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = label;
        tmp.color = color;
        tmp.fontSize = 3; // Reduced from 8 to 3 for much smaller text
        tmp.alignment = TextAlignmentOptions.Center;

        // NOTE: BillboardText component is NOT added here anymore
    }

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
        poly = ClipByPlane(poly, new Vector3( 0,-1, 0),  s);  
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
            tris.Add(0); tris.Add(i + 1); tris.Add(i);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = poly.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mf.mesh = mesh;
    }

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
}