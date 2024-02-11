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

    static Vector3[] GenerateVertices(int n, int m, float radius, float height)
    {
        Vector3[] vertices = new Vector3[n * (m + 1)];
        
        // Generate vertices
        for (int i = 0; i < n; i++)
        {
            float angle = Mathf.PI * 2 * i / n;
            float y = Mathf.Cos(angle) * radius + GetVerticalOffset(0, 0);
            float z = Mathf.Sin(angle) * radius + GetDepthOffset(0, 0);
            for (int j = 0; j < m + 1; j++)
            {
                float x = j / m * height - height / 2;
                vertices[i + j * n] = new Vector3(x, y, z);
            }
        }

        return vertices;
    }

    static int[] GenerateTriangles(int n, int m)
    {
        int[] triangles = new int[n * m * 6];
        
        for (int i = 0; i < n; i++)
        {
            // Generate triangles
            for (int j = 0; j < m; j++)
            {
                int index = j * n + i;
                int nextIndex = j * n + (i + 1) % n;
                int triangleIndex = i * 6 + j * n * 6;
                // Side triangles
                triangles[triangleIndex] = nextIndex;
                triangles[triangleIndex + 1] = index + n;
                triangles[triangleIndex + 2] = index;
                triangles[triangleIndex + 3] = nextIndex + n;
                triangles[triangleIndex + 4] = index + n;
                triangles[triangleIndex + 5] = nextIndex;
            }
        }

        return triangles;
    }

    public static void GenerateYarn(int n, int m, float radius, float height)
    {
        Debug.Log($"Generating Yarn: {n} {m} {radius} {height}");
        GameObject cylinder = new GameObject("Yarn");
        MeshFilter meshFilter = cylinder.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = cylinder.AddComponent<MeshRenderer>();
        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        mesh.vertices = GenerateVertices(n, m, radius, height);
        mesh.triangles = GenerateTriangles(n, m);
        mesh.RecalculateNormals();

        // Assign a default material
        meshRenderer.material = new Material(Shader.Find("Standard"));
    }
}