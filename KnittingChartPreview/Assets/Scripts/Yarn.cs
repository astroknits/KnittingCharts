using UnityEngine;

public class CreateCylinder : MonoBehaviour
{
    public int numSegments = 30;
    public float radius = 1f;
    public float height = 2f;

    void Start()
    {
        CreateCylinderObject();
    }

    void CreateCylinderObject()
    {
        GameObject cylinder = new GameObject("Cylinder");
        MeshFilter meshFilter = cylinder.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = cylinder.AddComponent<MeshRenderer>();
        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        Vector3[] vertices = new Vector3[numSegments * 2];
        int[] triangles = new int[numSegments * 6];

        // Generate vertices
        for (int i = 0; i < numSegments; i++)
        {
            float angle = Mathf.PI * 2 * i / numSegments;
            float x = height / 2;
            float y = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            Debug.Log($"{x} {y} {z}");
            vertices[i] = new Vector3(x, y, z);
            vertices[i + numSegments] = new Vector3(-1.0f * x, y, z);
        }

        // Generate triangles
        for (int i = 0; i < numSegments; i++)
        {
            int nextIndex = (i + 1) % numSegments;

            // Side triangles
            triangles[i * 6] = i;
            triangles[i * 6 + 1] = i + numSegments;
            triangles[i * 6 + 2] = nextIndex;

            triangles[i * 6 + 3] = nextIndex;
            triangles[i * 6 + 4] = i + numSegments;
            triangles[i * 6 + 5] = nextIndex + numSegments;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        // Assign a default material
        meshRenderer.material = new Material(Shader.Find("Standard"));
    }
}