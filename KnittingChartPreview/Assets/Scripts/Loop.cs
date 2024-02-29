using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace YarnGenerator
{
    public class Loop
    {
        public LoopType loopType;

        public float yarnWidth;
        
        // row number for the given stitch
        public int rowIndex;

        // loop index for the given loop (at base)
        public int loopIndexStart;
        // loop index for the given loop (once loop is completed)
        public int loopIndexEnd;

        // # of loops from previous row used by this stitch
        public int loopsConsumed;
        // # of loops left on the needle at the end of this stitch
        public int loopsProduced;
        // for cables, whether the loop is held in front or back
        // (default false)
        public bool heldInFront;
        public bool heldBehind;

        public Loop(int rowIndex, float yarnWidth, int loopIndexStart, int loopIndexEnd, bool heldInFront, bool heldBehind)
        {
            this.rowIndex = rowIndex;
            this.yarnWidth = yarnWidth;
            this.loopIndexStart = loopIndexStart;
            this.loopIndexEnd = loopIndexEnd;
            this.heldInFront = heldInFront;
            this.heldBehind = heldBehind;
        }

        public static Loop GetLoop(
            LoopType loopType,
            float yarnWidth,
            int rowIndex,
            int loopIndexStart,
            int loopIndexEnd,
            bool heldInFront,
            bool heldBehind)
        {
            switch (loopType)
            {
                case LoopType.Knit:
                    return new Knit(
                        rowIndex, yarnWidth, loopIndexStart, loopIndexEnd, heldInFront, heldBehind);
                case LoopType.Purl:
                    return new Purl(
                        rowIndex, yarnWidth, loopIndexStart, loopIndexEnd, heldInFront, heldBehind);
                default:
                    return new Knit(
                        rowIndex, yarnWidth, loopIndexStart, loopIndexEnd, heldInFront, heldBehind);
            }
        }

        public GameObject GetMesh(Material material)
        {
            // Create the mesh for the yarn in this row
            GameObject gameObject = new GameObject($"Loop {this.loopIndexStart} for yarnWidth {this.yarnWidth}");
            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
            Mesh mesh = new Mesh();
            // default IndexFormat is 16 bits, which has vertex limit of 65k
            // setting to 32 bits allows for up to 4 billion vertices per mesh
            mesh.indexFormat = IndexFormat.UInt32;
            meshFilter.mesh = mesh;

            // Set up vertices for the row based on the curve
            Vector3[] vertices = GenerateVertices();

            // Set up triangles for the row based on the vertices
            int[] triangles = GenerateTriangles(vertices);

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            // Assign a default material

            meshRenderer.material = material;
            return gameObject;
        }

        internal Vector3[] GenerateCircle(Vector3[] curve, int j)
        {
            /* Create circle of points in a plane normal to the direction
             * of the curve.
             * Start by generating a circle of points in the y-z plane, and
             * apply rotations about the z-axis (theta) and y-axis (phi).
             */
            Vector3[] circle = new Vector3[KnitSettings.radialRes];
            float theta = 0.0f; // (float) Math.PI; // (float) Math.PI / 2.0f;
            float phi = 0.0f; // (float) Math.PI; // * 2.0f;
            float cosTheta = (float) Math.Cos(theta);
            float sinTheta = (float) Math.Sin(theta);
            float cosPhi = (float) Math.Cos(phi);
            float sinPhi = (float) Math.Sin(phi);

            // If j >= curve.Length, theta and phi remain the default values of 0.0f
            // LEK debug
            if (j < curve.Length - 1)
            {
                // Direction vector, acts as the normal to the circle
                Vector3 diff = curve[j + 1] - curve[j];

                // Calculate theta (angle from z-axis) and phi (angle from y-axis)
                float length = (float) (Math.Sqrt(Math.Pow(diff.x, 2) + Math.Pow(diff.y, 2) + Math.Pow(diff.z, 2)));
                theta = (float) Math.Asin(diff.y / length);
                phi = (float) Math.Asin(diff.z / length / (float) Math.Cos(theta));

                // Precalculate the sine and cosine of the angles
                cosTheta = (float) Math.Cos(theta);
                sinTheta = (float) Math.Sin(theta);
                cosPhi = (float) Math.Cos(phi);
                sinPhi = (float) Math.Sin(phi);
            }

            for (int i = 0; i < KnitSettings.radialRes; i++)
            {
                // Angle runs from 0 to 2*Pi
                float angle = (float) Mathf.PI * 2.0f * (float) i / (float) KnitSettings.radialRes;

                // generates circle of width yarnWidth in y-z plane
                // for each point in the stitch curve
                float cx = 0.0f;
                float cy = this.yarnWidth * (float) Mathf.Cos(angle);
                float cz = this.yarnWidth * (float) Mathf.Sin(angle);

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

        internal Vector3[] GenerateVertices()
        {
            // Note that loopsProduced > 1 or loopsConsumed > 1
            // is not currently supported.
            // Once it is, we'll have to create vertices for more than
            // one loop.

            // Get the curve for the stitch
            //             int loopNoStart, int loopNoEnd, bool cableFront)

            Vector3[] curve = GenerateCurve();

            // Set up vertices for the stitch based on the stitch curve
            Vector3[] stitchVertices = GenerateVerticesForCurve(curve);
            for (int j = 0; j < stitchVertices.Length; j++)
            {
                stitchVertices[j].y += this.rowIndex * (1 + 1.0f - 3.0f * this.yarnWidth);
            }

            return stitchVertices;
        }

        internal Vector3[] GenerateVerticesForCurve(Vector3[] curve)
        {
            // Generate vertices
            Vector3[] vertices = new Vector3[
                KnitSettings.radialRes * curve.Length
            ];
            for (int j = 0; j < curve.Length; j++)
            {
                Vector3[] rotatedCircle = GenerateCircle(curve, j);
                Loop.DrawLine(rotatedCircle);
                for (int i = 0; i < KnitSettings.radialRes; i++)
                {
                    vertices[j * KnitSettings.radialRes + i] = rotatedCircle[i];
                }
            }

            return vertices;
        }

        internal int[] GenerateTriangles(Vector3[] rowVertices)
        {
            int[] triangles = new int[this.loopsProduced * KnitSettings.stitchRes * KnitSettings.radialRes * 6];

            int triangleIndex = 0;
            for (int j = 0; j < this.loopsProduced * KnitSettings.stitchRes - 1; j++)
            {
                for (int i = 0; i < KnitSettings.radialRes; i++)
                {
                    int index = j * KnitSettings.radialRes + i % KnitSettings.radialRes;
                    int nextIndex = j * KnitSettings.radialRes + (i + 1) % KnitSettings.radialRes;

                    /*
                    // Debugging triangle generation
                    Vector3[] test = new Vector3[2];
                    test[0] = rowVertices[index];
                    test[1] = rowVertices[index + KnitSettings.radialRes];
                    Stitch.DrawLine(test);
                    */

                    // Side triangles
                    triangles[triangleIndex] = nextIndex;
                    triangles[triangleIndex + 1] = index + KnitSettings.radialRes;
                    triangles[triangleIndex + 2] = index;
                    triangles[triangleIndex + 3] = nextIndex + KnitSettings.radialRes;
                    triangles[triangleIndex + 4] = index + KnitSettings.radialRes;
                    triangles[triangleIndex + 5] = nextIndex;
                    triangleIndex += 6;
                }
            }

            return triangles;
        }

        public Vector3 GenerateLoopCurve(int j)
        {
            float h = 1.0f; // height of stitches
            float a = 1.6f; // width of stitch
            float d = 0.3f; // depth curve factor for stitch
            float d2 = 2.1f * this.yarnWidth; // depth offset for stitch

            if (heldInFront)
            {
                d = 0.90f;
            } else if (heldBehind)
            {
                d = 1.10f;
            }

            if (this.loopType == LoopType.Purl)
            {
                d *= -1.0f;
                d2 *= -1.0f;
            }

            // j goes from 0 to stitchRes - 1 (or stitchRes for last segment)
            float angle = (float) j / (float) KnitSettings.stitchRes * 2.0f * (float) Math.PI;

            // parametric equation for stitch
            // eg from https://www.cs.cmu.edu/~kmcrane/Projects/Other/YarnCurve.pdf
            float xVal = (float) (angle + a * (float) Math.Sin(2.0f * angle)) / (float) Math.PI;
            float yVal = h * (float) Math.Cos(angle + (float) Math.PI);
            float zVal = d * (float) Math.Cos(2.0f * angle) - d2;

            return new Vector3(xVal, yVal, zVal);
        }

        public Vector3[] GenerateCurve()
        {
            int loopOffset = loopIndexEnd - loopIndexStart;

            Vector3[] curveForStitch = GenerateGenericCurve();

            // Each stitch takes up 2 natural units.  Therefore, the next stitch
            // needs an offset of 2.0f from the previous stitch
            Vector3 horizontalOffset = new Vector3(2.0f * loopIndexStart, 0, 0);
            for (int j = 0; j < curveForStitch.Length; j++)
            {
                curveForStitch[j] = curveForStitch[j] + horizontalOffset;

                // Apply shear if there is a stitchOffset
                if (loopOffset != 0)
                {
                    curveForStitch[j].x += loopOffset + loopOffset * (curveForStitch[j].y);
                }
            }

            // DrawLine(curveForStitch);

            return curveForStitch;
        }

        public Vector3[] GenerateGenericCurve()
        {
            int segments = KnitSettings.stitchRes;

            Vector3[] genericCurve = new Vector3[segments];
            for (int j = 0; j < segments; j++)
            {
                genericCurve[j] = GenerateLoopCurve(j);
            }

            return genericCurve;
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

    public class Knit : Loop
    {
        public Knit(int rowIndex, float yarnWidth, int loopIndexStart, int loopIndexEnd, bool heldInFront, bool heldBehind) : 
            base(rowIndex, yarnWidth, loopIndexStart, loopIndexEnd, heldInFront, heldBehind)
        {
            this.loopType = LoopType.Knit;
            this.yarnWidth = yarnWidth;
            this.loopsConsumed = 1;
            this.loopsProduced = 1;
        }
    }
    public class Purl : Loop
    {
        public Purl(int rowIndex, float yarnWidth,  int loopIndexStart, int loopIndexEnd, bool heldInFront, bool heldBehind) : 
            base(rowIndex, yarnWidth, loopIndexStart, loopIndexEnd, heldInFront, heldBehind)
        {
            this.loopType = LoopType.Purl;
            this.yarnWidth = yarnWidth;
            this.loopsConsumed = 1;
            this.loopsProduced = 1;
        }
    }
}