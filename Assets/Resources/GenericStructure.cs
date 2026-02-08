using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GenericStructure : MonoBehaviour
{
    public int size = 3;
    public float spacing = 1.5f;
    public float sphereScale = 0.4f;
    public float lineWidth = 0.03f;
    public Material material;

    // The three basis vectors:
    public Vector3 a1;
    public Vector3 a2;
    public Vector3 a3;

    public List<StaticCrystal.Atom> basisAtoms = new();

    void Start()
    {
        LoadData("Assets/Resources/saved structures/graphite.json"); // Change this string to change the loaded file.
    }

    public void IncreaseSize()
    {
        size += 1;
        if (size > 5) // Limit size to prevent excessive growth
        {
            size = 5;
            return;
        }
        BuildGrid();
    }

    public void DecreaseSize()
    {
        size -= 1;
        if (size < 2) // Limit size to prevent excessive growth
        {
            size = 2;
            return;
        }
        BuildGrid();
    }

    void BuildGrid()
    {
        // Clear previous grid
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        Vector3 offset = (size - 1) * spacing * (a1 + a2 + a3) / 2f;
        float sphereRadius = 0.5f * sphereScale;

        for (int n1 = 0; n1 < size; n1++)
            for (int n2 = 0; n2 < size; n2++)
                for (int n3 = 0; n3 < size; n3++)
                {
                    Vector3 pos = spacing * (n1 * a1 + n2 * a2 + n3 * a3) - offset;
                    foreach (StaticCrystal.Atom atom in basisAtoms)
                    {
                        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        sphere.transform.SetParent(transform, false);
                        sphere.transform.localPosition = pos + spacing * atom.relativePos;
                        sphere.transform.localScale = Vector3.one * sphereScale;
                        material = sphere.GetComponent<MeshRenderer>().material;
                        material.color = atom.colour;
                    }

                    if (n1 < size - 1)
                        CreateEdge(pos, pos + spacing * a1, sphereRadius);

                    if (n2 < size - 1)
                        CreateEdge(pos, pos + spacing * a2, sphereRadius);

                    if (n3 < size - 1)
                        CreateEdge(pos, pos + spacing * a3, sphereRadius);
                }
    }

    void CreateEdge(Vector3 start, Vector3 end, float radius)
    {
        Vector3 dir = (end - start).normalized;
        start += dir * radius;
        end -= dir * radius;

        GameObject edge = new GameObject("Edge");
        edge.transform.SetParent(transform, false);

        LineRenderer lr = edge.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.material = new Material(Shader.Find("Sprites/Default"));
    }

    // Loads the data from the input path and creates the crystal structure:
    public void LoadData(string path)
    {
        string structureJSON = File.ReadAllText(path);
        SaveFile structureFile = JsonUtility.FromJson<SaveFile>(structureJSON);
        a1 = new(structureFile.A1[0], structureFile.A1[1], structureFile.A1[2]);
        a2 = new(structureFile.A2[0], structureFile.A2[1], structureFile.A2[2]);
        a3 = new(structureFile.A3[0], structureFile.A3[1], structureFile.A3[2]);
        for (int i = 0; i < structureFile.BasisAtoms.Count; i++)
        {
            basisAtoms.Add(new(
                new(x: structureFile.BasisAtoms[i].RelativePos[0], y: structureFile.BasisAtoms[i].RelativePos[1], z: structureFile.BasisAtoms[i].RelativePos[2]),
                new(r: structureFile.BasisAtoms[i].Colour[0], g: structureFile.BasisAtoms[i].Colour[1], b: structureFile.BasisAtoms[i].Colour[2])
            ));
        }
        BuildGrid();
    }
}
