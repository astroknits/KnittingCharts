using UnityEngine;
using UnityEngine.Serialization;

public class Yarn : MonoBehaviour
{
    public int nRadialPoints = 4;
    public int nPoints = 1;
    public float width = 1f;
    public float length = 4f;

    void Start()
    {
        Debug.Log($"Hello");
        GenerateYarn(nRadialPoints, nPoints, width, length);
    }

    static float GetVerticalOffset(int row, int stitch)
    {
        return 0.0f;
    }
    
    static float GetDepthOffset(int row, int stitch)
    {
        return 0.0f;
    }

    public static void GenerateYarn(int n, int m, float radius, float height)
    {
        Debug.Log($"Generating Yarn: {n} {m} {radius} {height}");
        GameObject cylinder = new GameObject("Cylinder");
        MeshFilter meshFilter = cylinder.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = cylinder.AddComponent<MeshRenderer>();
        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        Vector3[] vertices = new Vector3[n * (m + 1)];
        int[] triangles = new int[n * 6];

        // Generate vertices
        for (int i = 0; i < n; i++)
        {
            float angle = Mathf.PI * 2 * i / n;
            float y = Mathf.Cos(angle) * radius + GetVerticalOffset(0, 0);
            float z = Mathf.Sin(angle) * radius + GetDepthOffset(0, 0);
            for (int j = 0; j < m + 1; j++)
            {
                float x = j / m * height - height/2;
                vertices[i + j * n] = new Vector3(x, y, z);
            }

            // Generate triangles
            int nextIndex = (i + 1) % n;
            for (int j = 0; j < m; j++)
            {
                int triangleIndex = i * 6 + j * 3;
                Debug.Log($"i, j, nextIndex: {i} {j} {nextIndex}, triangleIndex {triangleIndex}");
                // Side triangles
                triangles[triangleIndex] = i;
                triangles[triangleIndex + 1] = i + n;
                triangles[triangleIndex + 2] = nextIndex;
                triangles[triangleIndex + 3] = nextIndex;
                triangles[triangleIndex + 4] = i + n;
                triangles[triangleIndex + 5] = nextIndex + n;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        // Assign a default material
        meshRenderer.material = new Material(Shader.Find("Standard"));
    }
}