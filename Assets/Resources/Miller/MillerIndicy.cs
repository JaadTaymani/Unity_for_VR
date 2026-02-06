using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MillerIndexGenerator : MonoBehaviour
{
    public int h = 1, k = 1, l = 1;
    public float scale = 5f; // Scale of the unit cell

    void OnValidate()
    {
        GeneratePlane();
    }

    void GeneratePlane()
    {
        Mesh mesh = new Mesh();

        // Calculate intercepts (avoid division by zero)
        float x = h == 0 ? 1000 : 1f / h;
        float y = k == 0 ? 1000 : 1f / k;
        float z = l == 0 ? 1000 : 1f / l;

        Vector3[] vertices = new Vector3[3];
        vertices[0] = new Vector3(x, 0, 0) * scale;
        vertices[1] = new Vector3(0, y, 0) * scale;
        vertices[2] = new Vector3(0, 0, z) * scale;

        int[] triangles = new int[] { 0, 1, 2, 0, 2, 1 }; // Double sided

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
    }

    void OnDrawGizmos()
    {
        // Draw the Unit Cell boundaries
        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube(transform.position + new Vector3(0.5f, 0.5f, 0.5f) * scale, Vector3.one * scale);
    }
}
