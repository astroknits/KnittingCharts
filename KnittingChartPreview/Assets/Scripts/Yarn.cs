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

    static Vector3[] GenerateCurve(float length, int m)
    {
        Vector3[] curve = new Vector3[m + 1];
        for (int j = 0; j < m + 1; j++)
        {
            float x = (float)j / (float)m - 0.5f;
            float verticalOffset = GetVerticalOffset(j, m);
            float depthOffset = GetDepthOffset(j);
            curve[j] = new Vector3(
                length * x,  verticalOffset, depthOffset);
            // Debug.Log($"j, x, y: {j} {curve[j].x} {curve[j].y}");
        }
        return curve;
    }

    static Vector3[] GenerateCircle(float width, int n)
    {
        Vector3[] circle = new Vector3[n];
        for (int i = 0; i < n; i++)
        {
            float angle = Mathf.PI * 2 * i / n;
            circle[i] = new Vector3(
                0.0f, width * Mathf.Cos(angle), width * Mathf.Sin(angle));
        }

        return circle;
    }

    static Vector3[] GenerateRotatedCircle(Vector3[] circle, Vector3[] curve, int n, int m, int j)
    {
        Vector3[] rotatedCircle = new Vector3[n];
        float theta = 0.0f;

        if (j < m)
        {
            // theta = (float) (Vector3.Angle(curve[j], curve[j + 1]) * Math.PI / 180.0);
            Vector3 diff = curve[j + 1] - curve[j];
            float length = (float)(Math.Sqrt(Math.Pow(diff.x, 2) + Math.Pow(diff.y, 2)));
            theta = (float)Math.Acos(diff.x/length);
            Debug.Log($"theta: {theta}");
        }

        for (int i = 0; i < n; i++)
        {
            float xOffset = circle[i].y * (float) Math.Sin(theta);
            float yOffset = circle[i].y * (float) Math.Cos(theta);
            if (2 * j < m)
            {
                Debug.Log($"j, m: {j}, {m}");
                xOffset = -1.0f * xOffset;
                yOffset = 1.0f * yOffset;
            }
            else
            {
                Debug.Log($" not: j, m: {j}, {m}");
            }
            rotatedCircle[i] = new Vector3(
                curve[j].x +  xOffset,
                curve[j].y +  yOffset,
                curve[j].z + circle[i].z
            );
        }

        return rotatedCircle;
    }
    static Vector3[] GenerateVertices(int n, int m, float width, float length)
    {
        // Generate curve
        Vector3[] curve = GenerateCurve(length, m);
        Vector3[] circle = GenerateCircle(width, n);
        
        // Generate vertices
        Vector3[] vertices = new Vector3[n * (m + 1)];
        for (int j = 0; j < m + 1; j++)
        {
                Vector3[] rotatedCircle = GenerateRotatedCircle(circle, curve, n, m, j);
                for (int i = 0; i < n; i++)
                {
                    vertices[j * n + i] = rotatedCircle[i];
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

    public static void GenerateYarn(int n, int m, float width, float length)
    {
        // Debug.Log($"Generating Yarn: {n} {m} {width} {length}");
        GameObject cylinder = new GameObject("Yarn");
        MeshFilter meshFilter = cylinder.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = cylinder.AddComponent<MeshRenderer>();
        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        mesh.vertices = GenerateVertices(n, m, width, length);
        mesh.triangles = GenerateTriangles(n, m);
        mesh.RecalculateNormals();

        // Assign a default material
        meshRenderer.material = new Material(Shader.Find("Standard"));
    }
}