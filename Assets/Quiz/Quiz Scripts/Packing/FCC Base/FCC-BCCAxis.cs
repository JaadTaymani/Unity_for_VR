using System.Collections.Generic;
using UnityEngine;

public class FCCoBCCAxis : MonoBehaviour
{
    [Header("Crystal Settings")]
    public int N = 4;
    public float spacing = 1f;
    public float atomRadius = 0.1f;
    public float springRadius = 0.15f;
    public float axisRadius = 0.08f;

    private List<GameObject> atoms = new();
    private List<GameObject> atoms2 = new();
    private List<GameObject> atoms3 = new();
    private List<GameObject> atoms4 = new();

    void Start()
    {
        ClearOld();

        // 1. CREATE ALL ATOMS FIRST
        CreateCornerAtoms();
        CreateAtoms2();
        CreateAtoms3();
        CreateAtoms4();

        // 2. CENTER THE CRYSTAL
        CenterCrystal();

        // 3. CREATE ALL SPRINGS AFTER CENTERING
        CreateCornerSprings();
        CreateAtoms2Springs();
        CreateAtoms3Springs();
        CreateAtoms4Springs();

        // 4. CREATE FCC CLOSE-PACKED AXIS
        CreateAxis();
    }

    void ClearOld()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);
    }

    // ---------------- CREATE ATOMS ----------------
    void CreateCornerAtoms()
    {
        for (int z = 0; z < N; z++)
            for (int y = 0; y < N; y++)
                for (int x = 0; x < N; x++)
                    atoms.Add(CreateAtom(new Vector3(x, y, z) * spacing));
    }

    void CreateAtoms2()
    {
        for (int z = 0; z < N - 1; z++)
            for (int y = 0; y < N - 1; y++)
                for (int x = 0; x < N; x++)
                    atoms2.Add(CreateAtom(new Vector3(x, y + 0.5f, z + 0.5f) * spacing));
    }

    void CreateAtoms3()
    {
        for (int z = 0; z < N - 1; z++)
            for (int y = 0; y < N; y++)
                for (int x = 0; x < N - 1; x++)
                    atoms3.Add(CreateAtom(new Vector3(x + 0.5f, y, z + 0.5f) * spacing));
    }

    void CreateAtoms4()
    {
        for (int z = 0; z < N; z++)
            for (int y = 0; y < N - 1; y++)
                for (int x = 0; x < N - 1; x++)
                    atoms4.Add(CreateAtom(new Vector3(x + 0.5f, y + 0.5f, z) * spacing));
    }

    GameObject CreateAtom(Vector3 pos)
    {
        GameObject atom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        atom.transform.position = pos;
        atom.transform.localScale = Vector3.one * atomRadius * 2f;
        atom.GetComponent<Renderer>().material.color = new Color(0f, 0.58f, 0.69f);
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
        UpdateBounds(atoms3);
        UpdateBounds(atoms4);

        Vector3 center = (min + max) / 2f;

        foreach (var atom in atoms) atom.transform.localPosition -= center;
        foreach (var atom in atoms2) atom.transform.localPosition -= center;
        foreach (var atom in atoms3) atom.transform.localPosition -= center;
        foreach (var atom in atoms4) atom.transform.localPosition -= center;
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

    void CreateAtoms2Springs()
    {
        for (int z = 0; z < N - 1; z++)
            for (int y = 0; y < N - 1; y++)
                for (int x = 0; x < N; x++)
                {
                    int i = x + y * N + z * (N - 1) * N;
                    ConnectCorners(atoms2[i], new int[,] {
                        {x, y, z},
                        {x, y + 1, z},
                        {x, y, z + 1},
                        {x, y + 1, z + 1}
                    });
                }
    }

    void CreateAtoms3Springs()
    {
        for (int z = 0; z < N - 1; z++)
            for (int y = 0; y < N; y++)
                for (int x = 0; x < N - 1; x++)
                {
                    int i = x + y * (N - 1) + z * (N - 1) * N;
                    ConnectCorners(atoms3[i], new int[,] {
                        {x, y, z},
                        {x + 1, y, z},
                        {x, y, z + 1},
                        {x + 1, y, z + 1}
                    });
                }
    }

    void CreateAtoms4Springs()
    {
        for (int z = 0; z < N; z++)
            for (int y = 0; y < N - 1; y++)
                for (int x = 0; x < N - 1; x++)
                {
                    int i = x + y * (N - 1) + z * (N - 1) * (N - 1);
                    ConnectCorners(atoms4[i], new int[,] {
                        {x, y, z},
                        {x + 1, y, z},
                        {x, y + 1, z},
                        {x + 1, y + 1, z}
                    });
                }
    }

    void MakeSpring(GameObject a, GameObject b)
    {
        Vector3 dir = b.transform.position - a.transform.position;
        float length = dir.magnitude;

        GameObject spring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        spring.name = "Spring";
        spring.transform.position = a.transform.position + dir / 2f;
        spring.transform.up = dir.normalized;
        spring.transform.localScale = new Vector3(springRadius, length / 2f, springRadius);
        spring.GetComponent<Renderer>().material.color = Color.orange;
        spring.transform.parent = transform;
    }

    void ConnectCorners(GameObject atom, int[,] corners)
    {
        for (int i = 0; i < 4; i++)
        {
            int j = corners[i, 0] + corners[i, 1] * N + corners[i, 2] * N * N;
            MakeSpring(atom, atoms[j]);
        }
    }

    void CreateAxis()
    {
        float half = (N - 1) * spacing / 2f;

        Vector3 start = new Vector3(-half, -half, half);
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