using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MillerIndexGenerator : MonoBehaviour
{
    public int h = 1, k = 1, l = 1;
    public float scale = 5f;

    void OnValidate()
    {
        GeneratePlane();
    }

    void GeneratePlane()
    {
        Mesh mesh = new Mesh();

        float x = h == 0 ? 1000 : 1f / h;
        float y = k == 0 ? 1000 : 1f / k;
        float z = l == 0 ? 1000 : 1f / l;

        Vector3[] vertices = new Vector3[3];
        vertices[0] = new Vector3(x, 0, 0) * scale;
        vertices[1] = new Vector3(0, y, 0) * scale;
        vertices[2] = new Vector3(0, 0, z) * scale;

        mesh.vertices = vertices;
        mesh.triangles = new int[] { 0, 1, 2, 0, 2, 1 };
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
    }

    void OnDrawGizmos()
    {
        Vector3 origin = transform.position;

        // Unit cell
        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube(
            origin + Vector3.one * scale * 0.5f,
            Vector3.one * scale
        );

        DrawAxis(origin, Vector3.right, Color.red, "X");
        DrawAxis(origin, Vector3.up, Color.green, "Y");
        DrawAxis(origin, Vector3.forward, Color.blue, "Z");
    }

    void DrawAxis(Vector3 origin, Vector3 direction, Color color, string label)
    {
        float axisLength = scale;
        float arrowSize = scale * 0.08f;

        Vector3 end = origin + direction * axisLength;

        Gizmos.color = color;
        Gizmos.DrawLine(origin, end);

        // Arrowhead (two lines)
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 160, 0) * Vector3.forward;
        Vector3 left  = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 200, 0) * Vector3.forward;

        Gizmos.DrawLine(end, end + right * arrowSize);
        Gizmos.DrawLine(end, end + left * arrowSize);

        #if UNITY_EDITOR
        Handles.color = color;
        Handles.Label(end + direction * arrowSize * 0.6f, label);
        #endif
    }
}
