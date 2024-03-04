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

        // Loop indices: actual integer loop # for 
        // start and end of the stitch
        // loop index for the given loop (at base)
        public int loopIndexStart;
        // loop index for the given loop (once loop is completed)
        public int loopIndexEnd;
        // Loop offset
        // Loop start location along x axis (can be loopIndexStart
        // but might be offset by a bit)
        public float loopXStart;
        // offset by which to shear the loop
        // (default value is loopIndexEnd - loopIndexStart
        // but might be tweaked by loops in a row above/below)
        public float loopXOffset;

        // # of loops from previous row used by this stitch
        public int loopsConsumed;
        // # of loops left on the needle at the end of this stitch
        public int loopsProduced;
        // for cables, whether the loop is held in front or back
        // (default false)
        public bool heldInFront;
        public bool heldBehind;

        public int radialRes = KnitSettings.radialRes;
        public int stitchRes = KnitSettings.stitchRes;

        public Loop(int rowIndex, float yarnWidth, int loopIndexStart, int loopIndexEnd, bool heldInFront, bool heldBehind)
        {
            this.rowIndex = rowIndex;
            this.yarnWidth = yarnWidth;
            this.loopIndexStart = loopIndexStart;
            this.loopIndexEnd = loopIndexEnd;
            this.heldInFront = heldInFront;
            this.heldBehind = heldBehind;
            SetLoopStartAndOffset();
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

        public GameObject GetMesh(Vector3[] vertices, int[] triangles, Material material)
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
            
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            // Assign a default material
            meshRenderer.material = material;
            return gameObject;
        }

        public GameObject GenerateMesh(Material material)
        {
            // Get the curve for the stitch
            Vector3[] curve = GenerateCurve();
            
            // Set up vertices for the row based on the curve
            Vector3[] vertices = GenerateVertices(curve);

            // Set up triangles for the row based on the vertices
            int[] triangles = GenerateTriangles(vertices);

            // Create the mesh from the vertices & triangles
            return GetMesh(vertices, triangles, material);
        }

        internal Vector3[] GenerateCircle(Vector3[] curve, int j)
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
                    this.yarnWidth * (float) Mathf.Cos(angle),
                    this.yarnWidth * (float) Mathf.Sin(angle)
                    );

                // Rotate the circle so its normal is
                // in direction of the diff vector
                var rotation = Quaternion.FromToRotation(Vector3.right, normal);

                // Add the circle at the point curve[j]
                circle[i] = curve[j] + rotation * circleVector;
            }

            return circle;
        }

        internal Vector3[] GenerateVertices(Vector3[] curve)
        {
            // Note that loopsProduced > 1 or loopsConsumed > 1
            // is not currently supported.
            // Once it is, we'll have to create vertices for more than
            // one loop.

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
                radialRes * curve.Length
            ];
            for (int j = 0; j < curve.Length; j++)
            {
                Vector3[] rotatedCircle = GenerateCircle(curve, j);
                Loop.DrawLine(rotatedCircle);
                for (int i = 0; i < radialRes; i++)
                {
                    vertices[j * radialRes + i] = rotatedCircle[i];
                }
            }

            return vertices;
        }

        internal int[] GenerateTriangles(Vector3[] rowVertices)
        {
            int[] triangles = new int[this.loopsProduced * stitchRes * radialRes * 6];

            int triangleIndex = 0;
            for (int j = 0; j < this.loopsProduced * stitchRes - 1; j++)
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

        public Vector3 GetLoopValueForSegment(int j)
        {
            float h = 1.0f; // height of stitches
            float a = 1.6f; // width of stitch
            float d = 0.3f; // depth curve factor for stitch
            float d2 = 2.1f * this.yarnWidth; // depth offset for stitch

            // j goes from 0 to stitchRes - 1 (or stitchRes for last segment)
            float angle = (float) j / (float) stitchRes * 2.0f * (float) Math.PI;

            if (heldInFront)
            {
                d = 0.5f;
            } else if (heldBehind)
            {
                d = 0.20f;
            }

            if (this.loopType == LoopType.Purl)
            {
                d *= -1.0f;
                d2 *= -1.0f;
            }

            // parametric equation for stitch
            // eg from https://www.cs.cmu.edu/~kmcrane/Projects/Other/YarnCurve.pdf
            float xVal = (float) (angle + a * (float) Math.Sin(2.0f * angle)) / (float) Math.PI;
            float yVal = h * (float) Math.Cos(angle + (float) Math.PI);
            float zVal = d * (float) Math.Cos(2.0f * angle) - d2;

            return new Vector3(xVal, yVal, zVal);
        }

        public void SetLoopStartAndOffset()
        {
            // check offset between where stitch was and where it ends up
            // (if it's a cable stitch that crosses over)
            loopXStart = 2.0f * loopIndexStart + yarnWidth;
            loopXOffset = (float)loopIndexEnd - (float)loopIndexStart;
        }
        
        public Vector3[] GenerateCurve()
        {
            // check offset between where stitch was and where it ends up
            // (if it's a cable stitch that crosses over)
            Vector3[] curveForStitch = new Vector3[stitchRes];
            for (int j = 0; j < stitchRes; j++)
            {
                curveForStitch[j] = GetLoopValueForSegment(j);
            }

            // Each stitch takes up 2 natural units.  Therefore, the next stitch
            // needs an offset of 2.0f from the previous stitch
            Vector3 horizontalOffset = new Vector3(loopXStart, 0, 0);

            for (int j = 0; j < curveForStitch.Length; j++)
            {
                curveForStitch[j] = curveForStitch[j] + horizontalOffset;

                // Apply shear if there is a loopXOffset
                curveForStitch[j].x += loopXOffset * (1 + yarnWidth);
                curveForStitch[j].x += loopXOffset * (curveForStitch[j].y);
            }

            // DrawLine(curveForStitch);

            return curveForStitch;
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
        public Knit(
            int rowIndex,
            float yarnWidth,
            int loopIndexStart,
            int loopIndexEnd,
            bool heldInFront,
            bool heldBehind
            ) : 
            base(rowIndex, yarnWidth, loopIndexStart, loopIndexEnd, heldInFront, heldBehind)
        {
            this.loopType = LoopType.Knit;
            this.loopsConsumed = 1;
            this.loopsProduced = 1;
        }
    }
    public class Purl : Loop
    {
        public Purl(
            int rowIndex,
            float yarnWidth, 
            int loopIndexStart,
            int loopIndexEnd,
            bool heldInFront,
            bool heldBehind
            ) : 
            base(rowIndex, yarnWidth, loopIndexStart, loopIndexEnd, heldInFront, heldBehind)
        {
            this.loopType = LoopType.Purl;
            this.loopsConsumed = 1;
            this.loopsProduced = 1;
        }
    }
}