using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Yarn : MonoBehaviour
{
    public int nRadialPoints = 4;
    public int nPoints = 1;
    public float width = 0.1f;
    public float length = 1f;

    //
    // void Start()
    // {
    //     GenerateYarn(nRadialPoints, nPoints, width, length);
    // }

    static float sigmoid(float x)
    {
        // sigmoid(x) = 1 / (1 + exp(-x))
        return 1.0f / (1.0f + (float) Math.Exp(-1.0f * x));
    }

    static float GetVerticalOffsetForStitch(int index, int totalSegments)
    {
        float kappa = 0.25f;
        float scale = 4.0f; // range goes from -scale to +scale
        float shift = 0.8f;
        float pos = scale * (2 * (float) index / (float) totalSegments - 0.5f);

        if (index * 2 > totalSegments)
        {
            pos = scale - (pos + shift);
        }
        else
        {
            pos = pos - shift;
        }
        
        float res = sigmoid(pos / kappa);
        // Debug.Log($"index {index}, totalSegments {totalSegments}, fraction: {pos}, offset: {res}");
        return res;
    }

    static float GetVerticalOffset(int index, int totalSegments)
    {
        return GetVerticalOffsetForStitch(index, totalSegments);
    }
    
    static float GetDepthOffset(int index)
    {
        return 0.0f;
    }

    static Vector3[] GenerateCurve(int m)
    {
        Vector3[] curve = new Vector3[m + 1];
        for (int j = 0; j < m + 1; j++)
        {
            float x = (float)j / (float)m - 0.5f;
            float verticalOffset = GetVerticalOffset(j, m);
            float depthOffset = GetDepthOffset(j);
            curve[j] = new Vector3(
                x,  verticalOffset, depthOffset);
            // Debug.Log($"j, curve: {j} {curve[j]}");
        }
        return curve;
    }

    static Vector3[] GenerateCircle(Vector3[] curve, int n, int m, float radius, float height, int j)
    {
        Vector3[] circle = new Vector3[n];
        float x = (float)j / (float)m * height - height / 2;

        for (int i = 0; i < n; i++)
        {
            float angle = Mathf.PI * 2 * i / n;
            float y = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            float verticalOffset = curve[j].y;
            float depthOffset = curve[j].z;
            circle[i] = new Vector3(
                x, y + verticalOffset, z + depthOffset);
        }

        return circle;
    }
    
    static Vector3[] GenerateVertices(int n, int m, float radius, float height)
    {
        // Generate curve
        Vector3[] curve = GenerateCurve(m);
        
        // Generate vertices
        Vector3[] vertices = new Vector3[n * (m + 1)];
        for (int j = 0; j < m + 1; j++)
        {
                Vector3[] circle = GenerateCircle(curve, n, m, radius, height, j);
                for (int i = 0; i < n; i++)
                {
                    vertices[j * n + i] = circle[i];
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
        // Debug.Log($"Generating Yarn: {n} {m} {radius} {height}");
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