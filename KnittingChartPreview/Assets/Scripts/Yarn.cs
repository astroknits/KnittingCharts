using System;
using UnityEngine;
using UnityEngine.Serialization;
using YarnGenerator;

public class Yarn : MonoBehaviour
{
    public float stitchLength = 1f;
    public float yarnWidth = 0.1f;

    public static void GenerateRow(int radialRes, int stitchRes, float yarnWidth, float stitchLength)
    {
        GameObject yarn = new GameObject("Yarn");
        MeshFilter meshFilter = yarn.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = yarn.AddComponent<MeshRenderer>();
        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        // Generate curve
        Vector3[] curve = GenerateCurve(stitchLength, stitchRes);
        
        mesh.vertices = GenerateVertices(curve, radialRes, stitchRes, yarnWidth, stitchLength);
        mesh.triangles = GenerateTriangles(radialRes, stitchRes);
        mesh.RecalculateNormals();

        // Assign a default material
        meshRenderer.material = new Material(Shader.Find("Standard"));
    }

    static float sigmoid(float x)
    {
        // sigmoid(x) = 1 / (1 + exp(-x))
        return 1.0f / (1.0f + (float) Math.Exp(-1.0f * x));
    }

    static float GetVerticalOffsetForStitch(int index, int stitchRes)
    {
        float kappa = 0.25f;
        float scale = 4.0f; // range goes from -scale to +scale
        float shift = 0.8f;
        float pos = scale * (2 * (float) index / (float) stitchRes - 0.5f);

        if (index * 2 > stitchRes)
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

    static float GetVerticalOffset(int index, int stitchRes)
    {
        return GetVerticalOffsetForStitch(index, stitchRes);
    }
    
    static float GetDepthOffset(int index)
    {
        return 0.0f;
    }

    static Vector3[] GenerateCurve(float length, int stitchRes)
    {
        Vector3[] curve = new Vector3[stitchRes + 1];
        for (int j = 0; j < stitchRes + 1; j++)
        {
            float x = (float)j / (float)stitchRes - 0.5f;
            float verticalOffset = GetVerticalOffset(j, stitchRes);
            float depthOffset = GetDepthOffset(j);
            curve[j] = new Vector3(
                length * x,  verticalOffset, depthOffset);
            // Debug.Log($"j, x, y: {j} {curve[j].x} {curve[j].y}");
        }
        return curve;
    }

    static Vector3[] GenerateCircle(float yarnWidth, int radialRes)
    {
        Vector3[] circle = new Vector3[radialRes];
        for (int i = 0; i < radialRes; i++)
        {
            float angle = Mathf.PI * 2 * i / radialRes;
            circle[i] = new Vector3(
                0.0f, yarnWidth * Mathf.Cos(angle), yarnWidth * Mathf.Sin(angle));
        }
        return circle;
    }

    static Vector3[] GenerateRotatedCircle(Vector3[] circle, Vector3[] curve, int radialRes, int stitchRes, int j)
    {
        Vector3[] rotatedCircle = new Vector3[radialRes];
        float theta = 0.0f;

        if (j < stitchRes)
        {
            Vector3 diff = curve[j + 1] - curve[j];
            float length = (float)(Math.Sqrt(Math.Pow(diff.x, 2) + Math.Pow(diff.y, 2)));
            theta = (float) Math.Asin(diff.y / length);
        }

        for (int i = 0; i < radialRes; i++)
        {
            rotatedCircle[i] = new Vector3(
                curve[j].x + circle[i].y * (float) Math.Sin(theta),
                curve[j].y - circle[i].y * (float) Math.Cos(theta),
                curve[j].z + circle[i].z
            );
        }

        return rotatedCircle;
    }
    static Vector3[] GenerateVertices(Vector3[] curve, int radialRes, int stitchRes, float yarnWidth, float stitchLength)
    {
        // Generate vertices
        Vector3[] circle = GenerateCircle(yarnWidth, radialRes);
        Vector3[] vertices = new Vector3[radialRes * (stitchRes + 1)];
        for (int j = 0; j < stitchRes + 1; j++)
        {
                Vector3[] rotatedCircle = GenerateRotatedCircle(circle, curve, radialRes, stitchRes, j);
                for (int i = 0; i < radialRes; i++)
                {
                    vertices[j * radialRes + i] = rotatedCircle[i];
                }
        }
        return vertices;
    }

    static int[] GenerateTriangles(int radialRes, int stitchRes)
    {
        int[] triangles = new int[radialRes * stitchRes * 6];
        
        for (int i = 0; i < radialRes; i++)
        {
            // Generate triangles
            for (int j = 0; j < stitchRes; j++)
            {
                int index = j * radialRes + i;
                int nextIndex = j * radialRes + (i + 1) % radialRes;
                int triangleIndex = i * 6 + j * radialRes * 6;
                // Side triangles
                triangles[triangleIndex] = nextIndex;
                triangles[triangleIndex + 1] = index + radialRes;
                triangles[triangleIndex + 2] = index;
                triangles[triangleIndex + 3] = nextIndex + radialRes;
                triangles[triangleIndex + 4] = index + radialRes;
                triangles[triangleIndex + 5] = nextIndex;
            }
        }

        return triangles;
    }


}