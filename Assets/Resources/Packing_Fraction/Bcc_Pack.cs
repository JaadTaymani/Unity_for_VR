using UnityEngine;

public class Bcc_Pack : MonoBehaviour
{
    public int size = 2;
    public float latticeConstant = 2.0f;
    public float lineWidth = 0.05f;
    
    public Color cornerAtomColor = new Color(0.25f, 0.35f, 0.9f);  // Blue
    public Color bodyCenterAtomColor = new Color(0.9f, 0.35f, 0.25f);  // Red

    void Start()
    {
        BuildGrid();
    }

    void BuildGrid()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Body diagonal = sqrt(3) * latticeConstant
        // Distance from corner to body center = sqrt(3) * latticeConstant / 2
        // So radius = sqrt(3) * latticeConstant / 4
        float sphereRadius = Mathf.Sqrt(3f) * latticeConstant / 4f;

        Vector3 cubeMin = Vector3.zero;
        Vector3 cubeMax = Vector3.one * (size - 1) * latticeConstant;
        Vector3 cubeCenter = (cubeMin + cubeMax) / 2f;

        // Corner atoms - BLUE
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                for (int z = 0; z < size; z++)
                {
                    Vector3 pos = new Vector3(x, y, z) * latticeConstant - cubeCenter;
                    GameObject sphere = CreateClippedSphere(
                        pos,
                        sphereRadius,
                        cubeMin - cubeCenter,
                        cubeMax - cubeCenter,
                        cornerAtomColor
                    );
                    sphere.transform.SetParent(transform, false);
                }

        // Body-center atoms - RED
        for (int x = 0; x < size - 1; x++)
            for (int y = 0; y < size - 1; y++)
                for (int z = 0; z < size - 1; z++)
                {
                    Vector3 pos = (new Vector3(x, y, z) + new Vector3(0.5f, 0.5f, 0.5f)) * latticeConstant - cubeCenter;
                    GameObject sphere = CreateClippedSphere(
                        pos,
                        sphereRadius,
                        cubeMin - cubeCenter,
                        cubeMax - cubeCenter,
                        bodyCenterAtomColor
                    );
                    sphere.transform.SetParent(transform, false);
                }

        CreateBoundingBox(cubeMin - cubeCenter, cubeMax - cubeCenter);
    }

    GameObject CreateClippedSphere(Vector3 center, float radius, Vector3 cubeMin, Vector3 cubeMax, Color color)
    {
        GameObject sphere = new GameObject("ClippedSphere");
        sphere.transform.localPosition = center;

        MeshFilter mf = sphere.AddComponent<MeshFilter>();
        MeshRenderer mr = sphere.AddComponent<MeshRenderer>();

        Mesh mesh = GenerateClippedSphereMesh(center, radius, cubeMin, cubeMax, 30);
        mf.mesh = mesh;

        Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.color = color;

        mr.material = mat;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;

        return sphere;
    }

    Mesh GenerateClippedSphereMesh(Vector3 center, float radius, Vector3 cubeMin, Vector3 cubeMax, int segments)
    {
        Mesh mesh = new Mesh();
        var vertices = new System.Collections.Generic.List<Vector3>();
        var triangles = new System.Collections.Generic.List<int>();
        var normals = new System.Collections.Generic.List<Vector3>();

        for (int lat = 0; lat <= segments; lat++)
        {
            float theta = lat * Mathf.PI / segments;
            float sinTheta = Mathf.Sin(theta);
            float cosTheta = Mathf.Cos(theta);

            for (int lon = 0; lon <= segments; lon++)
            {
                float phi = lon * 2 * Mathf.PI / segments;
                float sinPhi = Mathf.Sin(phi);
                float cosPhi = Mathf.Cos(phi);

                Vector3 normal = new Vector3(
                    sinTheta * cosPhi,
                    cosTheta,
                    sinTheta * sinPhi
                );

                Vector3 vertex = normal * radius;
                Vector3 worldPos = center + vertex;

                Vector3 clampedWorld = new Vector3(
                    Mathf.Clamp(worldPos.x, cubeMin.x, cubeMax.x),
                    Mathf.Clamp(worldPos.y, cubeMin.y, cubeMax.y),
                    Mathf.Clamp(worldPos.z, cubeMin.z, cubeMax.z)
                );

                vertices.Add(clampedWorld - center);
                normals.Add(normal);
            }
        }

        for (int lat = 0; lat < segments; lat++)
        {
            for (int lon = 0; lon < segments; lon++)
            {
                int first = (lat * (segments + 1)) + lon;
                int second = first + segments + 1;

                triangles.Add(first);
                triangles.Add(second);
                triangles.Add(first + 1);

                triangles.Add(second);
                triangles.Add(second + 1);
                triangles.Add(first + 1);
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();

        return mesh;
    }

    void CreateBoundingBox(Vector3 min, Vector3 max)
    {
        GameObject box = new GameObject("BoundingBox");
        box.transform.SetParent(transform, false);

        LineRenderer lr = box.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.positionCount = 16;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;

        lr.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        lr.material.color = Color.white;

        Vector3[] positions = new Vector3[]
        {
            new Vector3(min.x, min.y, min.z), new Vector3(max.x, min.y, min.z),
            new Vector3(max.x, min.y, max.z), new Vector3(min.x, min.y, max.z),
            new Vector3(min.x, min.y, min.z), new Vector3(min.x, max.y, min.z),
            new Vector3(max.x, max.y, min.z), new Vector3(max.x, max.y, max.z),
            new Vector3(min.x, max.y, max.z), new Vector3(min.x, max.y, min.z),
            new Vector3(max.x, max.y, min.z), new Vector3(max.x, min.y, min.z),
            new Vector3(max.x, min.y, max.z), new Vector3(max.x, max.y, max.z),
            new Vector3(min.x, max.y, max.z), new Vector3(min.x, min.y, max.z)
        };

        lr.SetPositions(positions);
    }
}