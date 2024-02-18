using System;
using System.Linq;
using UnityEngine;

namespace YarnGenerator
{
    public class Yarn
    {
        public float gauge = 1f;
        public float yarnWidth = 0.1f;
        private StitchCache stitchCache = StitchCache.GetInstance();

        public void GenerateRow(
            StitchType[] stitches, float yarnWidth, float gauge, int rowNumber)
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
                stitches, gauge, yarnWidth, rowNumber);
            
            // Set up triangles for the row based on the vertices
            int[] triangles = GenerateTriangles(rowVertices);

            mesh.vertices = rowVertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            // Assign a default material
            meshRenderer.material = new Material(Shader.Find("Standard"));
        }
        
        internal Vector3[] GenerateCircle(float yarnWidth)
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

        internal Vector3[] RotateCircleAboutZAxis(
            Vector3[] circle, Vector3[] curve, int j)
        {
            Vector3[] rotatedCircle = new Vector3[KnitSettings.radialRes];
            float theta = 0.0f;

            if (j < curve.Length - 1)
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

        internal Vector3[] GenerateVerticesForRow(
            StitchType[] stitches, float gauge, float yarnWidth, int rowNumber)
        {
            Vector3[] rowCurve = Array.Empty<Vector3>();
            for (int k = 0; k < stitches.Length; k++)
            {
                StitchType stitchType = stitches[k];
                // Generate curve for the stitch
                Stitch stitch = stitchCache.GetStitch(stitchType, gauge, false);
                Vector3[] rowCurve1 = stitch.GenerateCurve(k, (k == stitches.Length - 1));
                // and add to the vertices row array
                rowCurve = rowCurve.Concat(rowCurve1).ToArray();
            }

            // Set up vertices for the stitch based on the stitch curve
            Vector3[] rowVertices = GenerateVerticesForCurve(rowCurve, yarnWidth);
            for (int j = 0; j < rowVertices.Length; j++)
            {
                rowVertices[j].y += rowNumber;
            }
            return rowVertices;
        }
        internal Vector3[] GenerateVerticesForCurve(Vector3[] curve, float yarnWidth)
        {
            // Generate vertices
            Vector3[] circle = GenerateCircle(yarnWidth);
            Vector3[] vertices = new Vector3[
                KnitSettings.radialRes * curve.Length
            ];
            for (int j = 0; j < curve.Length; j++)
            {
                Vector3[] rotatedCircle = RotateCircleAboutZAxis(circle, curve, j);
                for (int i = 0; i < KnitSettings.radialRes; i++)
                {
                    vertices[j * KnitSettings.radialRes + i] = rotatedCircle[i];
                }
            }

            return vertices;
        }

        internal int[] GenerateTriangles(Vector3[] rowVertices)
        {
            int xSegments = (int) rowVertices.Length / KnitSettings.radialRes;
            int[] triangles = new int[rowVertices.Length * 6];

            int triangleIndex = 0;
            for (int i = 0; i < KnitSettings.radialRes; i++)
            {
                for (int j = 0; j < xSegments - 1; j++)
                {
                    if (j > 0)
                    {
                        int index = j * KnitSettings.radialRes + i;
                        int nextIndex = j * KnitSettings.radialRes + (i + 1) % KnitSettings.radialRes;
                        // int triangleIndex = j * KnitSettings.radialRes * 6 + i;
                        // Side triangles
                        triangles[triangleIndex] = nextIndex;
                        triangles[triangleIndex + 1] = index + KnitSettings.radialRes;
                        triangles[triangleIndex + 2] = index;
                        triangles[triangleIndex + 3] = nextIndex + KnitSettings.radialRes;
                        triangles[triangleIndex + 4] = index + KnitSettings.radialRes;
                        triangles[triangleIndex + 5] = nextIndex;
                    }
                    triangleIndex += 6;
                }
            }

            return triangles;
        }


    }
}