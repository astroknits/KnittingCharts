using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace YarnGenerator
{
    public class YarnMeshGenerator
    {
        public int radialRes = KnitSettings.radialRes;
        public int stitchRes = KnitSettings.stitchRes;

        public BaseStitchInfo baseStitchInfo;

        public YarnMeshGenerator(BaseStitchInfo baseStitchInfo)
        {
            this.baseStitchInfo = baseStitchInfo;
        }

        public GameObject GenerateMesh(
            float yarnWidth,
            Material material,
            int rowIndex,
            int loopIndexConsumed,
            int loopIndexProduced,
            Loop[] loopsConsumed,
            Loop[] loopsProduced,
            HoldDirection holdDirection
            )
        {
            // Get the curve for the stitch
            Vector3[] curve = GenerateCurve(
                yarnWidth,
                loopIndexConsumed,
                loopIndexProduced,
                loopsConsumed,
                loopsProduced,
                holdDirection
                );

            // Set up vertices for the row based on the curve
            Vector3[] vertices = GenerateVertices(
                yarnWidth, rowIndex, curve);

            // Set up triangles for the row based on the vertices
            int[] triangles = GenerateTriangles(curve);

            // Create the mesh from the vertices & triangles
            return GetMesh(
                yarnWidth, material, vertices, triangles, loopIndexConsumed);
        }

        public GameObject GetMesh(
            float yarnWidth,
            Material material,
            Vector3[] vertices,
            int[] triangles,
            int loopIndexConsumed
            )
        {
            // Create the mesh for the yarn in this row
            GameObject gameObject =
                new GameObject($"BaseStitch {loopIndexConsumed} for yarnWidth {yarnWidth}");
            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
            Mesh mesh = new Mesh();
            // default IndexFormat is 16 bits, which has vertex limit of 65k
            // setting to 32 bits allows for up to 4 billion vertices per mesh
            mesh.indexFormat = IndexFormat.UInt32;
            meshFilter.mesh = mesh;

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            // Assign a default material
            meshRenderer.material = material;
            return gameObject;
        }

        internal Vector3[] GenerateVertices(float yarnWidth, int rowIndex, Vector3[] curve)
        {
            // Note that nLoopsProduced > 1 or nLoopsConsumed > 1
            // is not currently supported.
            // Once it is, we'll have to create vertices for more than
            // one baseStitch.

            // Set up vertices for the stitch based on the stitch curve
            Vector3[] vertices = new Vector3[
                curve.Length * radialRes
            ];
            for (int j = 0; j < curve.Length; j++)
            {
                Vector3[] rotatedCircle = GenerateCircle(yarnWidth, curve, j);
                for (int i = 0; i < radialRes; i++)
                {
                    int index = j * radialRes + i;
                    vertices[index] = rotatedCircle[i];
                    // Shift the y position to the correct row
                    vertices[index].y += rowIndex * (2.0f - 3.0f * yarnWidth);
                }
            }

            return vertices;
        }

        internal int[] GenerateTriangles(Vector3[] curve)
        {
            int[] triangles = new int[curve.Length * radialRes * 6];

            int triangleIndex = 0;
            for (int j = 0; j < curve.Length - 1; j++)
            {
                for (int i = 0; i < radialRes; i++)
                {
                    int index = j * radialRes + i % radialRes;
                    int nextIndex = j * radialRes + (i + 1) % radialRes;

                    /*
                    // Debugging triangle generation
                    Vector3[] test = new Vector3[2];
                    test[0] = rowVertices[index];
                    test[1] = rowVertices[index + radialRes];
                    Stitch.DrawLine(test);
                    */

                    // Side triangles
                    triangles[triangleIndex] = nextIndex;
                    triangles[triangleIndex + 1] = index + radialRes;
                    triangles[triangleIndex + 2] = index;
                    triangles[triangleIndex + 3] = nextIndex + radialRes;
                    triangles[triangleIndex + 4] = index + radialRes;
                    triangles[triangleIndex + 5] = nextIndex;
                    triangleIndex += 6;
                }
            }

            return triangles;
        }

        public int GetConsumedIndex(int loopIndexConsumed, Loop[] loopsConsumed)
        {
            // check offset between where stitch was and where it ends up
            // (if it's a cable stitch that crosses over)
            int cons = loopIndexConsumed;
            // Note we are currently not supporting generating curves
            // for base stitches with loopsConsumed != 1 or loopsProduced != 1
            if (loopsConsumed is not null && loopsConsumed.Length > 0)
            {
                cons = loopsConsumed[0].loopIndex;
            }

            return cons;
        }

        public int GetProducedIndex(int loopIndexProduced, Loop[] loopsProduced)
        {
            // check offset between where stitch was and where it ends up
            // (if it's a cable stitch that crosses over)
            int prod = loopIndexProduced;
            // Note we are currently not supporting generating curves
            // for base stitches with loopsConsumed != 1 or loopsProduced != 1
            if (loopsProduced is not null && loopsProduced.Length > 0)
            {
                prod = loopsProduced[0].loopIndex;
            }

            return prod;
        }

        public Vector3[] GenerateCurve(
            float yarnWidth,
            int loopIndexConsumed,
            int loopIndexProduced,
            Loop[] loopsConsumed,
            Loop[] loopsProduced,
            HoldDirection holdDirection)
        {

            int cons = GetConsumedIndex(loopIndexConsumed, loopsConsumed);
            int prod = GetProducedIndex(loopIndexProduced, loopsProduced);

            float loopXStart = 2.0f * cons + yarnWidth;
            float loopXOffset = (float) prod - (float) cons;

            Vector3[] curve = new Vector3[stitchRes];
            for (int j = 0; j < stitchRes; j++)
            {
                curve[j] = GetLoopValueForSegment(yarnWidth, holdDirection, j);
            }

            // Each stitch takes up 2 natural units.  Therefore, the next stitch
            // needs an offset of 2.0f from the previous stitch
            Vector3 horizontalOffset = new Vector3(loopXStart, 0, 0);

            for (int j = 0; j < curve.Length; j++)
            {
                curve[j] = curve[j] + horizontalOffset;

                // Apply shear if there is a loopXOffset
                curve[j].x += loopXOffset * (1 + yarnWidth);
                curve[j].x += loopXOffset * (curve[j].y);
            }

            // DrawLine(curveForLoop);

            return curve;
        }

        public Vector3 GetLoopValueForSegment(float yarnWidth, HoldDirection holdDirection, int j)
        {
            float h = 1.0f; // height of stitches
            float a = 1.6f; // width of stitch
            float d = 0.3f; // depth curve factor for stitch
            float d2 = 2.1f * yarnWidth; // depth offset for stitch

            // j goes from 0 to stitchRes - 1 (or stitchRes for last segment)
            float angle = (float) j / (float) stitchRes * 2.0f * (float) Math.PI;

            if (holdDirection == HoldDirection.Front)
            {
                d = 0.5f;
            }
            else if (holdDirection == HoldDirection.Back)
            {
                d = 0.20f;
            }

            if
                (this.baseStitchInfo.BaseStitchType == BaseStitchType.Purl)
            {
                d *= -1.0f;
                d2 *= -1.0f;
            }

            if (this.baseStitchInfo.BaseStitchType == BaseStitchType.Knit2Tog)
            {
                d *= 2.0f;
                d2 *= 2.0f;
            }

            // parametric equation for stitch
            // eg from https://www.cs.cmu.edu/~kmcrane/Projects/Other/YarnCurve.pdf
            float xVal = (float) (angle + a * (float) Math.Sin(2.0f * angle)) / (float) Math.PI;
            float yVal = h * (float) Math.Cos(angle + (float) Math.PI);
            float zVal = d * (float) Math.Cos(2.0f * angle) - d2;

            return new Vector3(xVal, yVal, zVal);
        }

        internal Vector3[] GenerateCircle(float yarnWidth, Vector3[] curve, int j)
        {
            /* Create circle of points in a plane normal to the direction
             * of the curve.
             * Start by generating a circle of points in the y-z plane, and
             * apply rotations about the z-axis (theta) and y-axis (phi).
             */
            Vector3[] circle = new Vector3[radialRes];
            Vector3 normal = Vector3.right;

            // If j >= curve.Length, theta and phi remain the default values of 0.0f
            if (j < curve.Length - 1)
            {
                // Direction vector, acts as the normal to the circle
                normal = (curve[j + 1] - curve[j]).normalized;
            }

            for (int i = 0; i < radialRes; i++)
            {
                // Angle runs from 0 to 2*Pi
                float angle = (float) Mathf.PI * 2.0f * (float) i / (float) radialRes;

                // generate circle of width yarnWidth in y-z plane
                // for each point in the stitch curve
                Vector3 circleVector = new Vector3(
                    0.0f,
                    yarnWidth * (float) Mathf.Cos(angle),
                    yarnWidth * (float) Mathf.Sin(angle)
                );

                // Rotate the circle so its normal is
                // in direction of the diff vector
                var rotation = Quaternion.FromToRotation(Vector3.right, normal);

                // Add the circle at the point curve[j]
                circle[i] = curve[j] + rotation * circleVector;
            }

            return circle;
        }

        public static void DrawLine(Vector3[] vectorCurve)
        {
            for (int j = 0; j < vectorCurve.Length; j++)
            {
                if (j >= vectorCurve.Length - 1)
                {
                    continue;
                }

                Vector3 v1 = vectorCurve[j];
                Vector3 v2 = vectorCurve[j + 1];

                Debug.DrawLine(v1, v2, Color.green, 2, false);
            }
        }
    }
}