using System.Collections.Generic;
using UnityEngine;

public class DiamondUnitCell : MonoBehaviour
{
    public float cellSize = 2f;
    public float atomRadius = 0.25f;
    public float bondRadius = 0.06f;
    public float bondCutoff = 1.2f;

    private List<GameObject> atoms = new List<GameObject>();

    void Start()
    {
        CreateUnitCell();
        CenterCell();
        CreateBonds();
    }

    void CreateUnitCell()
    {
        // Fractional coordinates of diamond cubic conventional unit cell
        Vector3[] positions = new Vector3[]
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(0f, 0.1875f, 0.1875f),
            new Vector3(0.1875f, 0f, 0.1875f),
            new Vector3(0.1875f, 0.1875f, 0f),

            new Vector3(0.09f, 0.09f, 0.09f),
            new Vector3(0.09f, 0.285f, 0.285f),
            new Vector3(0.285f, 0.09f, 0.285f),
            new Vector3(0.285f, 0.285f, 0.09f)
        };

        foreach (Vector3 frac in positions)
        {
            Vector3 pos = frac * cellSize;

            GameObject atom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            atom.transform.position = pos;
            atom.transform.localScale = Vector3.one * atomRadius * 2f;

            atom.GetComponent<Renderer>().material.color = new Color(0f, 0.58f, 0.69f);

            atom.transform.parent = transform;
            atoms.Add(atom);
        }
    }

    void CenterCell()
    {
        Vector3 center = Vector3.zero;

        foreach (var atom in atoms)
            center += atom.transform.localPosition;

        center /= atoms.Count;

        foreach (var atom in atoms)
            atom.transform.localPosition -= center;
    }

    void CreateBonds()
    {
        for (int i = 0; i < atoms.Count; i++)
        {
            for (int j = i + 1; j < atoms.Count; j++)
            {
                float dist = Vector3.Distance(
                    atoms[i].transform.position,
                    atoms[j].transform.position
                );

                if (dist < bondCutoff)
                {
                    MakeBond(atoms[i], atoms[j]);
                }
            }
        }
    }

    void MakeBond(GameObject a, GameObject b)
    {
        Vector3 dir = b.transform.position - a.transform.position;
        float length = dir.magnitude;

        GameObject bond = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        bond.transform.position = a.transform.position + dir / 2f;
        bond.transform.up = dir.normalized;

        bond.transform.localScale = new Vector3(
            bondRadius,
            length / 2f,
            bondRadius
        );

        bond.GetComponent<Renderer>().material.color = Color.orange;

        bond.transform.parent = transform;
    }
}
