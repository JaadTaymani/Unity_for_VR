using System.Collections.Generic;
using UnityEngine;

public class HexagonalCell : MonoBehaviour
{
    public float a = 2f;
    public float c = 3f;

    public float atomRadius = 0.2f;
    public float bondRadius = 0.05f;

    private List<Vector3> points = new List<Vector3>();

    void Start()
    {
        CreateCell();
        CreateEdges();
        CenterCell();
    }

    void CreateCell()
    {
        // 120° lattice in XZ plane, Y is vertical
        Vector3 A = new Vector3(a, 0, 0);
        Vector3 B = new Vector3(-a / 2f, 0, Mathf.Sqrt(3f) * a / 2f);
        Vector3 C = new Vector3(0, c, 0);

        Vector3[] corners = new Vector3[]
        {
            Vector3.zero,   // 0
            A,              // 1
            B,              // 2
            A + B,          // 3

            C,              // 4
            A + C,          // 5
            B + C,          // 6
            A + B + C       // 7
        };

        foreach (Vector3 p in corners)
        {
            GameObject atom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            atom.transform.position = p;
            atom.transform.localScale = Vector3.one * atomRadius * 2f;

            atom.GetComponent<Renderer>().material.color = new Color(0f, 0.58f, 0.69f);
            atom.transform.parent = transform;

            points.Add(p);
        }
    }

    void CreateEdges()
    {
        // --- Bottom face edges ---
        MakeBond(points[0], points[1]);
        MakeBond(points[1], points[3]);
        MakeBond(points[3], points[2]);
        MakeBond(points[2], points[0]);

        // --- Top face edges ---
        MakeBond(points[4], points[5]);
        MakeBond(points[5], points[7]);
        MakeBond(points[7], points[6]);
        MakeBond(points[6], points[4]);

        // --- Vertical edges ---
        MakeBond(points[0], points[4]);
        MakeBond(points[1], points[5]);
        MakeBond(points[2], points[6]);
        MakeBond(points[3], points[7]);

        // --- Add ONE shortest diagonal per face ---
        AddShortestDiagonal(0, 1, 3, 2); // bottom
        AddShortestDiagonal(4, 5, 7, 6); // top
    }

    void AddShortestDiagonal(int a, int b, int c, int d)
    {
        float d1 = Vector3.Distance(points[a], points[c]);
        float d2 = Vector3.Distance(points[b], points[d]);

        if (d1 < d2)
            MakeBond(points[a], points[c]);
        else
            MakeBond(points[b], points[d]);
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

