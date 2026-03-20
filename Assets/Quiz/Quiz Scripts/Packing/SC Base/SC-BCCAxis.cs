using System.Collections.Generic;
using UnityEngine;

public class BCCAxis : MonoBehaviour
{
    public int N = 4;
    public float spacing = 1f;
    public float atomRadius = 0.3f;
    public float springRadius = 0.15f;
    public float axisRadius = 0.08f;

    private List<GameObject> atoms = new List<GameObject>();

    void Start()
    {
        CreateAtoms();
        CenterCrystal();
        CreateSprings();
        CreateAxis(); // new axis
    }

    void CreateAtoms()
    {
        for (int z = 0; z < N; z++)
        {
            for (int y = 0; y < N; y++)
            {
                for (int x = 0; x < N; x++)
                {
                    GameObject atom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    atom.transform.position = new Vector3(x, y, z) * spacing;
                    atom.transform.localScale = Vector3.one * atomRadius * 2f;

                    atom.GetComponent<Renderer>().material.color =
                        new Color(0f, 0.58f, 0.69f);

                    atom.transform.parent = transform;
                    atoms.Add(atom);
                }
            }
        }
    }

    void CenterCrystal()
    {
        Vector3 center = Vector3.zero;

        foreach (var atom in atoms)
            center += atom.transform.localPosition;

        center /= atoms.Count;

        foreach (var atom in atoms)
            atom.transform.localPosition -= center;
    }

    void CreateSprings()
    {
        for (int z = 0; z < N; z++)
        {
            for (int y = 0; y < N; y++)
            {
                for (int x = 0; x < N; x++)
                {
                    int i = x + y * N + z * N * N;
                    GameObject atom = atoms[i];

                    if (x < N - 1)
                        MakeSpring(atom, atoms[i + 1]);

                    if (y < N - 1)
                        MakeSpring(atom, atoms[i + N]);

                    if (z < N - 1)
                        MakeSpring(atom, atoms[i + N * N]);
                }
            }
        }
    }

    void MakeSpring(GameObject start, GameObject end)
    {
        Vector3 direction = end.transform.position - start.transform.position;
        float length = direction.magnitude;

        GameObject spring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        spring.transform.position = start.transform.position + direction / 2f;
        spring.transform.up = direction.normalized;

        spring.transform.localScale = new Vector3(
            springRadius,
            length / 2f,
            springRadius
        );

        spring.GetComponent<Renderer>().material.color = Color.orange;
        spring.transform.parent = transform;
    }

    void CreateAxis()
    {
        float half = (N - 1) * spacing / 2f;

        Vector3 start = new Vector3(-half, -half, -half);
        Vector3 end = new Vector3(half, half, half);

        Vector3 direction = end - start;
        float length = direction.magnitude;

        GameObject axis = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        axis.transform.parent = transform;

        axis.transform.localPosition = start + direction / 2f;
        axis.transform.up = direction.normalized;

        axis.transform.localScale = new Vector3(
            axisRadius,
            length / 1.5f,
            axisRadius
        );

        axis.GetComponent<Renderer>().material.color = Color.green;
    }

}
