using System.Collections.Generic;
using UnityEngine;

public class TetragonalCell : MonoBehaviour
{
    public float a = 2f;   // x and y
    public float c = 3f;   // z (different)

    public float atomRadius = 0.2f;
    public float bondRadius = 0.05f;

    private List<Vector3> points = new List<Vector3>();

    void Start()
    {
        CreateCorners();
        CreateEdges();
        CenterCell();
    }

    void CreateCorners()
    {
        Vector3[] corners = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(a, 0, 0),
            new Vector3(0, a, 0),
            new Vector3(0, 0, c),

            new Vector3(a, a, 0),
            new Vector3(a, 0, c),
            new Vector3(0, a, c),
            new Vector3(a, a, c)
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
