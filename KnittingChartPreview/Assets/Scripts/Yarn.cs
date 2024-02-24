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

        public GameObject GenerateRow(
            StitchType[] stitches, float yarnWidth, int rowNumber, Material material)
        {
            // For now, each row only has one stitch in it

            // Create the mesh for the yarn in this row
            GameObject yarn = new GameObject($"Row {rowNumber} for {yarnWidth}/1.0f");
            MeshFilter meshFilter = yarn.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = yarn.AddComponent<MeshRenderer>();
            Mesh mesh = new Mesh();
            meshFilter.mesh = mesh;

            // Set up vertices for the row based on the curve
            Vector3[] rowVertices = GenerateVerticesForRow(
                stitches, yarnWidth, rowNumber);
            
            // Set up triangles for the row based on the vertices
            int[] triangles = GenerateTriangles(rowVertices);

            mesh.vertices = rowVertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            // Assign a default material

            meshRenderer.material = material; 
            return yarn;
        }

        internal Vector3[] GenerateCircle(float yarnWidth, Vector3[] curve, int j)
        {
            /* Create circle of points in a plane normal to the direction
             * of the curve.
             * Start by generating a circle of points in the y-z plane, and
             * apply rotations about the z-axis (theta) and y-axis (phi).
             */
            Vector3[] circle = new Vector3[KnitSettings.radialRes];
            float theta = 0.0f;
            float phi = 0.0f;
            float cosTheta = 1.0f;
            float sinTheta = 0.0f;
            float cosPhi = 1.0f;
            float sinPhi = 0.0f;

            // If j >= curve.Length, theta and phi remain the default values of 0.0f
            if (j < curve.Length - 1)
            {
                // Direction vector, acts as the normal to the circle
                Vector3 diff = curve[j + 1] - curve[j];

                // Calculate theta (angle from z-axis) and phi (angle from y-axis)
                float length = (float) (Math.Sqrt(Math.Pow(diff.x, 2) + Math.Pow(diff.y, 2) + Math.Pow(diff.z, 2)));
                theta = (float) Math.Asin(diff.y / length);
                phi = (float) Math.Asin(diff.z / length / Math.Cos(theta));

                // Precalculate the sine and cosine of the angles
                cosTheta = (float)Math.Cos(theta);
                sinTheta = (float)Math.Sin(theta);
                cosPhi = (float)Math.Cos(phi);
                sinPhi = (float)Math.Sin(phi);
            }


            for (int i = 0; i < KnitSettings.radialRes; i++)
            {
                // Angle runs from 0 to 2*Pi
                float angle = Mathf.PI * 2 * i / KnitSettings.radialRes;

                // generates circle of width yarnWidth in y-z plane
                // for each point in the stitch curve
                float cx = 0.0f;
                float cy = yarnWidth * Mathf.Cos(angle);
                float cz = yarnWidth * Mathf.Sin(angle);

                // Now rotate the circle so its normal is
                // in direction of the diff vector
                // 
                // Calculate 3d rotations
                // https://stackoverflow.com/questions/14607640/rotating-a-vector-in-3d-space
                // rotate around the z axis first (theta)
                float dx = cx * cosTheta - cy * sinTheta;
                float dy = cx * sinTheta + cy * cosTheta;
                float dz = cz;
                // rotate around the y axis second (phi)
                dx = dx * cosPhi + dz * sinPhi;
                dz = -1.0f * dx * sinPhi + dz * cosPhi;

                // Add the circle at the point curve[j]
                circle[i] = new Vector3(
                    curve[j].x + dx,
                    curve[j].y - dy,
                    curve[j].z + dz
                );
            }

            return circle;
        }

        internal Vector3[] GenerateVerticesForRow(
            StitchType[] stitches, float yarnWidth, int rowNumber)
        {
            Vector3[] rowCurve = Array.Empty<Vector3>();
            int loopNo = 0;
            for (int k = 0; k < stitches.Length; k++)
            {
                StitchType stitchType = stitches[k];

                // instantiate the stitch
                Stitch stitch = Stitch.GetStitch(stitchType, k);

                Debug.Log($"loopNo: {loopNo}, k={k}");
                // Get the curve for the stitch
                Vector3[] rowCurve1 = stitch.GenerateCurve(
                    loopNo, yarnWidth, (k == stitches.Length - 1));

                // and add to the vertices row array
                rowCurve = rowCurve.Concat(rowCurve1).ToArray();
                loopNo += stitch.loopsProduced;
            }

            // Set up vertices for the stitch based on the stitch curve
            Vector3[] rowVertices = GenerateVerticesForCurve(rowCurve, yarnWidth);
            for (int j = 0; j < rowVertices.Length; j++)
            {
                rowVertices[j].y += rowNumber * (1 + 1.0f - 3.0f * yarnWidth);
            }
            return rowVertices;
        }
        internal Vector3[] GenerateVerticesForCurve(Vector3[] curve, float yarnWidth)
        {
            // Generate vertices
            Vector3[] vertices = new Vector3[
                KnitSettings.radialRes * curve.Length
            ];
            for (int j = 0; j < curve.Length; j++)
            {
                Vector3[] rotatedCircle = GenerateCircle(yarnWidth, curve, j);
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