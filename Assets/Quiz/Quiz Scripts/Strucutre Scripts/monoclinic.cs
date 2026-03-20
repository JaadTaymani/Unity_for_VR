using System.Collections.Generic;
using UnityEngine;

public class MonoclinicCell : MonoBehaviour
{
    public float a = 2f;
    public float b = 3f;
    public float c = 2.5f;

    [Header("Angle β (degrees)")]
    public float beta = 70f; // angle between a and c

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
        float rad = Mathf.Deg2Rad * beta;

        // Lattice vectors
        Vector3 A = new Vector3(a, 0, 0);
        Vector3 B = new Vector3(0, b, 0);
        Vector3 C = new Vector3(
            c * Mathf.Cos(rad),
            0,
            c * Mathf.Sin(rad)
        );

        Vector3[] corners = new Vector3[]
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
        int[,] edges = new int[,]
        {
            {0,1}, {0,2}, {0,3},
            {1,4}, {1,5},
            {2,4}, {2,6},
            {3,5}, {3,6},
            {4,7}, {5,7}, {6,7}
        };

        for (int i = 0; i < edges.GetLength(0); i++)
        {
            MakeBond(points[edges[i, 0]], points[edges[i, 1]]);
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