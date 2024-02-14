using System;
using System.Linq;
using UnityEngine;

namespace YarnGenerator
{
    public class Yarn : MonoBehaviour
    {
        public float gauge = 1f;
        public float yarnWidth = 0.1f;

        public static void GenerateRow(
            StitchType[] stitches, float yarnWidth, float gauge)
        {
            // For now, each row only has one stitch in it

            // Create the mesh for the yarn in this row
            GameObject yarn = new GameObject("Yarn");
            MeshFilter meshFilter = yarn.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = yarn.AddComponent<MeshRenderer>();
            Mesh mesh = new Mesh();
            meshFilter.mesh = mesh;

            // Set up vertices for the row based on the curve
            Vector3[] rowVertices = GenerateVerticesForRow(
                stitches, gauge, yarnWidth);
            
            // Set up triangles for the row based on the vertices
            int[] triangles = GenerateTriangles();

            mesh.vertices = rowVertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            // Assign a default material
            meshRenderer.material = new Material(Shader.Find("Standard"));
        }
        
        static Vector3[] GenerateCircle(float yarnWidth)
        {
            // generates circle of width yarnWidth in y-z plane
            // for each point in the stitch curve
            Vector3[] circle = new Vector3[KnitSettings.radialRes];
            for (int i = 0; i < KnitSettings.radialRes; i++)
            {
                float angle = Mathf.PI * 2 * i / KnitSettings.radialRes;
                circle[i] = new Vector3(
                    0.0f,
                    yarnWidth * Mathf.Cos(angle),
                    yarnWidth * Mathf.Sin(angle)
                    );
            }

            return circle;
        }

        static Vector3[] GenerateRotatedCircle(
            Vector3[] circle, Vector3[] curve, int j)
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

        static Vector3[] GenerateVerticesForRow(
            StitchType[] stitches, float gauge, float yarnWidth)
        {
            Vector3[] rowVertices = Array.Empty<Vector3>();
            foreach (StitchType stitch in stitches)
            {
                // Generate curve for the stitch
                Vector3[] stitchCurve = Stitch.GenerateCurve(stitch, gauge);
                
                // Set up vertices for the stitch based on the stitch curve
                // and add to the vertices row array
                rowVertices = rowVertices.Concat(
                    GenerateVerticesForStitch(stitchCurve, yarnWidth)
                ).ToArray();
            }
            
            return rowVertices;
        }
        static Vector3[] GenerateVerticesForStitch(Vector3[] curve, float yarnWidth)
        {
            // Generate vertices
            Vector3[] circle = GenerateCircle(yarnWidth);
            Vector3[] vertices = new Vector3[
                KnitSettings.radialRes * (KnitSettings.stitchRes + 1)
            ];
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