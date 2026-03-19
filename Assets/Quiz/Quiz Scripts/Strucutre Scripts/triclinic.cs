using System.Collections.Generic;
using UnityEngine;

public class TriclinicCell : MonoBehaviour
{
    public float a = 2f;
    public float b = 2.2f;
    public float c = 2.5f;

    [Header("Angles in degrees")]
    public float alpha = 70f; // between b and c
    public float beta  = 80f; // between a and c
    public float gamma = 75f; // between a and b

    public float atomRadius = 0.2f;
    public float bondRadius = 0.05f;

    private List<Vector3> latticePoints = new List<Vector3>();

    void Start()
    {
        Vector3 A, B, C;
        BuildLatticeVectors(out A, out B, out C);

        CreateCorners(A, B, C);
        CreateEdges();
        CenterCell();
    }

    void BuildLatticeVectors(out Vector3 A, out Vector3 B, out Vector3 C)
    {
        float radA = Mathf.Deg2Rad * alpha;
        float radB = Mathf.Deg2Rad * beta;
        float radG = Mathf.Deg2Rad * gamma;

        // A along x-axis
        A = new Vector3(a, 0, 0);

        // B in xy-plane
        B = new Vector3(
            b * Mathf.Cos(radG),
            b * Mathf.Sin(radG),
            0
        );

        // C is fully 3D
        float cx = c * Mathf.Cos(radB);
        float cy = c * (Mathf.Cos(radA) - Mathf.Cos(radB) * Mathf.Cos(radG)) / Mathf.Sin(radG);

        float cz = Mathf.Sqrt(
            c * c - cx * cx - cy * cy
        );

        C = new Vector3(cx, cy, cz);
    }

    void CreateCorners(Vector3 A, Vector3 B, Vector3 C)
    {
        Vector3[] points = new Vector3[]
        {
            Vector3.zero,
            A,
            B,
            C,
            A + B,
            A + C,
            B + C,
            A + B + C
        };

        foreach (Vector3 p in points)
        {
            GameObject atom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            atom.transform.position = p;
            atom.transform.localScale = Vector3.one * atomRadius * 2f;

            atom.GetComponent<Renderer>().material.color = new Color(0f, 0.58f, 0.69f);
            atom.transform.parent = transform;

            latticePoints.Add(atom.transform.position);
        }
    }

    void CreateEdges()
    {
        // Define the 8 corners again (same order as before)
        Vector3[] p = latticePoints.ToArray();

        // Manually connect only true edges
        int[,] edges = new int[,]
        {
            {0,1}, {0,2}, {0,3},   // from origin
            {1,4}, {1,5},          // along edges
            {2,4}, {2,6},
            {3,5}, {3,6},
            {4,7}, {5,7}, {6,7}
        };

        for (int i = 0; i < edges.GetLength(0); i++)
        {
            MakeBond(p[edges[i,0]], p[edges[i,1]]);
        }
    }

    void MakeBond(Vector3 a, Vector3 b)
    {
        Vector3 dir = b - a;
        float length = dir.magnitude;

        GameObject bond = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        bond.transform.position = a + dir / 2f;
        bond.transform.up = dir.normalized;

        bond.transform.localScale = new Vector3(
            bondRadius,
            length / 2f,
            bondRadius
        );

        bond.GetComponent<Renderer>().material.color = Color.orange;
        bond.transform.parent = transform;
    }

    void CenterCell()
    {
        Vector3 center = Vector3.zero;

        foreach (Transform child in transform)
            center += child.localPosition;

        center /= transform.childCount;

        foreach (Transform child in transform)
            child.localPosition -= center;
    }
}