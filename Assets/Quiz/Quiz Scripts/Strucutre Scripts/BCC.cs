using System.Collections.Generic;
using UnityEngine;

public class CrystalBCC : MonoBehaviour
{
    [Header("Crystal Settings")]
    public int N = 4;
    public float spacing = 1f;
    public float atomRadius = 0.18f;
    public float springRadius = 0.15f;

    private List<GameObject> atoms = new();   // corner atoms
    private List<GameObject> atoms2 = new();  // body-center atoms

    void Start()
    {
        ClearOld();

        // 1. CREATE ALL ATOMS FIRST
        CreateCornerAtoms();
        CreateBodyCenterAtoms();

        // 2. CENTER THE CRYSTAL
        CenterCrystal();

        // 3. CREATE ALL SPRINGS AFTER CENTERING
        CreateCornerSprings();
        CreateBodyCenterSprings();
    }

    void ClearOld()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        atoms.Clear();
        atoms2.Clear();
    }

    // ---------------- CREATE ATOMS ----------------
    void CreateCornerAtoms()
    {
        for (int z = 0; z < N; z++)
            for (int y = 0; y < N; y++)
                for (int x = 0; x < N; x++)
                    atoms.Add(CreateAtom(new Vector3(x, y, z) * spacing, new Color(0f, 0.58f, 0.69f)));
    }

    void CreateBodyCenterAtoms()
    {
        for (int z = 0; z < N - 1; z++)
            for (int y = 0; y < N - 1; y++)
                for (int x = 0; x < N - 1; x++)
                    atoms2.Add(CreateAtom(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f) * spacing, Color.red));
    }

    GameObject CreateAtom(Vector3 pos, Color col)
    {
        GameObject atom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        atom.transform.position = pos;
        atom.transform.localScale = Vector3.one * atomRadius * 2f;
        atom.GetComponent<Renderer>().material.color = col;
        atom.transform.parent = transform;
        return atom;
    }

    // ---------------- CENTER CRYSTAL ----------------
    void CenterCrystal()
    {
        Vector3 min = Vector3.positiveInfinity;
        Vector3 max = Vector3.negativeInfinity;

        void UpdateBounds(List<GameObject> list)
        {
            foreach (var atom in list)
            {
                Vector3 pos = atom.transform.localPosition;
                min = Vector3.Min(min, pos);
                max = Vector3.Max(max, pos);
            }
        }

        UpdateBounds(atoms);
        UpdateBounds(atoms2);

        Vector3 center = (min + max) / 2f;

        foreach (var atom in atoms) atom.transform.localPosition -= center;
        foreach (var atom in atoms2) atom.transform.localPosition -= center;
    }

    // ---------------- CREATE SPRINGS ----------------
    void CreateCornerSprings()
    {
        for (int z = 0; z < N; z++)
            for (int y = 0; y < N; y++)
                for (int x = 0; x < N; x++)
                {
                    int i = x + y * N + z * N * N;

                    if (x < N - 1) MakeSpring(atoms[i], atoms[i + 1]);
                    if (y < N - 1) MakeSpring(atoms[i], atoms[i + N]);
                    if (z < N - 1) MakeSpring(atoms[i], atoms[i + N * N]);
                }
    }

    void CreateBodyCenterSprings()
    {
        for (int z = 0; z < N - 1; z++)
            for (int y = 0; y < N - 1; y++)
                for (int x = 0; x < N - 1; x++)
                {
                    int i = x + y * (N - 1) + z * (N - 1) * (N - 1);
                    GameObject center = atoms2[i];

                    int[,] corners =
                    {
                        {x, y, z},
                        {x + 1, y, z},
                        {x, y + 1, z},
                        {x, y, z + 1},
                        {x + 1, y + 1, z},
                        {x + 1, y, z + 1},
                        {x, y + 1, z + 1},
                        {x + 1, y + 1, z + 1}
                    };

                    for (int c = 0; c < 8; c++)
                    {
                        int j = corners[c, 0] + corners[c, 1] * N + corners[c, 2] * N * N;
                        MakeSpring(center, atoms[j]);
                    }
                }
    }

    void MakeSpring(GameObject a, GameObject b)
    {
        Vector3 d = b.transform.position - a.transform.position;
        float L = d.magnitude;

        GameObject s = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        s.name = "Spring";
        s.transform.position = a.transform.position + d / 2f;
        s.transform.up = d.normalized;
        s.transform.localScale = new Vector3(
            0.15f * atomRadius,
            L / 2f,
            0.15f * atomRadius
        );

        s.GetComponent<Renderer>().material.color = Color.orange;
        s.transform.parent = transform;
    }
}

