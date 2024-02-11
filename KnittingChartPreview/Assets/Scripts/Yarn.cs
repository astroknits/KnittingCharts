using UnityEngine;
using UnityEngine.Serialization;

public class CreateCylinder : MonoBehaviour
{
    [FormerlySerializedAs("numSegments")] public int numRadialSegments = 300;
    public int numSegments = 1;
    public float radius = 1f;
    public float height = 20f;

    void Start()
    {
        CreateCylinderObject();
    }
    
    private void OnGUI()
    {
        if (GUILayout.Button("Generate Cylinder"))
        {
            CreateCylinderObject();
        }
    }

    float GetVerticalOffset(int row, int stitch)
    {
        return 0.0f;
    }
    
    float GetDepthOffset(int row, int stitch)
    {
        return 0.0f;
    }
    
    void CreateCylinderObject()
    {
        GameObject cylinder = new GameObject("Cylinder");
        MeshFilter meshFilter = cylinder.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = cylinder.AddComponent<MeshRenderer>();
        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        Vector3[] vertices = new Vector3[numRadialSegments * (numSegments + 1)];
        int[] triangles = new int[numRadialSegments * 6];

        // Generate vertices
        for (int i = 0; i < numRadialSegments; i++)
        {
            float angle = Mathf.PI * 2 * i / numRadialSegments;
            float y = Mathf.Cos(angle) * radius + GetVerticalOffset(0, 0);
            float z = Mathf.Sin(angle) * radius + GetDepthOffset(0, 0);
            for (int j = 0; j < numSegments + 1; j++)
            {
                float x = j / numSegments * height - height/2;
                vertices[i + j * numRadialSegments] = new Vector3(x, y, z);
            }

            // Generate triangles
            int nextIndex = (i + 1) % numRadialSegments;
            for (int j = 0; j < numSegments; j++)
            {
                int triangleIndex = i * 6 + j * 3;
                Debug.Log($"i, j, nextIndex: {i} {j} {nextIndex}, triangleIndex {triangleIndex}");
                // Side triangles
                triangles[triangleIndex] = i;
                triangles[triangleIndex + 1] = i + numRadialSegments;
                triangles[triangleIndex + 2] = nextIndex;
                triangles[triangleIndex + 3] = nextIndex;
                triangles[triangleIndex + 4] = i + numRadialSegments;
                triangles[triangleIndex + 5] = nextIndex + numRadialSegments;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        // Assign a default material
        meshRenderer.material = new Material(Shader.Find("Standard"));
    }
}