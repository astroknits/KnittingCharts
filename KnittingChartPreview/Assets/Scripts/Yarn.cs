using System;
using UnityEngine;
using UnityEngine.Serialization;
using YarnGenerator;

namespace YarnGenerator
{
    public class Yarn : MonoBehaviour
    {
        public float stitchLength = 1f;
        public float yarnWidth = 0.1f;

        public static void GenerateRow(float yarnWidth, float stitchLength)
        {
            // For now, each row only has one stitch in it

            // Create the mesh for the yarn in this row
            GameObject yarn = new GameObject("Yarn");
            MeshFilter meshFilter = yarn.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = yarn.AddComponent<MeshRenderer>();
            Mesh mesh = new Mesh();
            meshFilter.mesh = mesh;

            // Stitch stitch = new Stitch(yarnWidth, stitchLength);
            // stitch.GenerateCurve();

            // Generate curve for the stitch
            Vector3[] curve = GenerateCurve(stitchLength);

            // Set up vertices and triangles for the row based on the curve
            Vector3[] vertices = GenerateVertices(curve, yarnWidth);
            int[] triangles = GenerateTriangles();

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            // Assign a default material
            meshRenderer.material = new Material(Shader.Find("Standard"));
        }

        static float sigmoid(float x)
        {
            // sigmoid(x) = 1 / (1 + exp(-x))
            return 1.0f / (1.0f + (float) Math.Exp(-1.0f * x));
        }

        static float GetVerticalOffsetForStitch(int index)
        {
            float kappa = 0.25f;
            float scale = 4.0f; // range goes from -scale to +scale
            float shift = 0.8f;
            float pos = scale * (2 * (float) index / (float) KnitSettings.stitchRes - 0.5f);

            if (index * 2 > KnitSettings.stitchRes)
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

        static float GetVerticalOffset(int index)
        {
            return GetVerticalOffsetForStitch(index);
        }

        static float GetDepthOffset(int index)
        {
            return 0.0f;
        }

        static Vector3[] GenerateCurve(float length)
        {
            Vector3[] curve = new Vector3[KnitSettings.stitchRes + 1];
            for (int j = 0; j < KnitSettings.stitchRes + 1; j++)
            {
                float x = (float) j / (float) KnitSettings.stitchRes - 0.5f;
                float verticalOffset = GetVerticalOffset(j);
                float depthOffset = GetDepthOffset(j);
                curve[j] = new Vector3(
                    length * x, verticalOffset, depthOffset);
                // Debug.Log($"j, x, y: {j} {curve[j].x} {curve[j].y}");
            }

            return curve;
        }

        static Vector3[] GenerateCircle(float yarnWidth)
        {
            Vector3[] circle = new Vector3[KnitSettings.radialRes];
            for (int i = 0; i < KnitSettings.radialRes; i++)
            {
                float angle = Mathf.PI * 2 * i / KnitSettings.radialRes;
                circle[i] = new Vector3(
                    0.0f, yarnWidth * Mathf.Cos(angle), yarnWidth * Mathf.Sin(angle));
            }

            return circle;
        }

        static Vector3[] GenerateRotatedCircle(Vector3[] circle, Vector3[] curve, int j)
        {
            Vector3[] rotatedCircle = new Vector3[KnitSettings.radialRes];
            float theta = 0.0f;

            if (j < KnitSettings.stitchRes)
            {
                Vector3 diff = curve[j + 1] - curve[j];
                float length = (float) (Math.Sqrt(Math.Pow(diff.x, 2) + Math.Pow(diff.y, 2)));
                theta = (float) Math.Asin(diff.y / length);
            }

            for (int i = 0; i < KnitSettings.radialRes; i++)
            {
                rotatedCircle[i] = new Vector3(
                    curve[j].x + circle[i].y * (float) Math.Sin(theta),
                    curve[j].y - circle[i].y * (float) Math.Cos(theta),
                    curve[j].z + circle[i].z
                );
            }

            return rotatedCircle;
        }

        static Vector3[] GenerateVertices(Vector3[] curve, float yarnWidth)
        {
            // Generate vertices
            Vector3[] circle = GenerateCircle(yarnWidth);
            Vector3[] vertices = new Vector3[KnitSettings.radialRes * (KnitSettings.stitchRes + 1)];
            for (int j = 0; j < KnitSettings.stitchRes + 1; j++)
            {
                Vector3[] rotatedCircle = GenerateRotatedCircle(circle, curve, j);
                for (int i = 0; i < KnitSettings.radialRes; i++)
                {
                    vertices[j * KnitSettings.radialRes + i] = rotatedCircle[i];
                }
            }

            return vertices;
        }

        static int[] GenerateTriangles()
        {
            int[] triangles = new int[KnitSettings.radialRes * KnitSettings.stitchRes * 6];

            for (int i = 0; i < KnitSettings.radialRes; i++)
            {
                // Generate triangles
                for (int j = 0; j < KnitSettings.stitchRes; j++)
                {
                    int index = j * KnitSettings.radialRes + i;
                    int nextIndex = j * KnitSettings.radialRes + (i + 1) % KnitSettings.radialRes;
                    int triangleIndex = i * 6 + j * KnitSettings.radialRes * 6;
                    // Side triangles
                    triangles[triangleIndex] = nextIndex;
                    triangles[triangleIndex + 1] = index + KnitSettings.radialRes;
                    triangles[triangleIndex + 2] = index;
                    triangles[triangleIndex + 3] = nextIndex + KnitSettings.radialRes;
                    triangles[triangleIndex + 4] = index + KnitSettings.radialRes;
                    triangles[triangleIndex + 5] = nextIndex;
                }
            }

            return triangles;
        }


    }
}