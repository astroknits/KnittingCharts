using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace YarnGenerator

{
    public abstract class Stitch
    {
        public StitchType stitchType;
        // stitch number for the given row
        public int index;
        // # of loops from previous row used by this stitch
        public int loopsConsumed;
        // # of loops left on the needle at the end of this stitch
        public int loopsProduced;

        public Stitch(int index)
        {
            this.index = index;
        }
        
        public static Stitch GetStitch(StitchType stitchType, int index)
        {
            switch (stitchType)
            {
                case StitchType.KnitStitch:
                    return new KnitStitch(index);
                case StitchType.PurlStitch:
                    return new PurlStitch(index);
                case StitchType.Cable1Lo1RStitch:
                    return new Cable1Lo1RStitch(index);
                case StitchType.Cable2Lo2RStitch:
                    return new Cable2Lo2RStitch(index);
                case StitchType.CableKnitStitch:
                    return new CableKnitStitch(index);
                case StitchType.CableKnitStitch4:
                    return new CableKnitStitch4(index);
                default:
                    return new KnitStitch(index);
            }
        }

        public abstract Vector3[] GenerateCurve(int loopNo, float yarnWidth);
        public abstract Vector3[] GenerateCurve(int loopNoStart, int loopNoEnd, float yarnWidth, bool cableFront);

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
        
        public static GameObject GenerateRowPreview(Row row, float yarnWidth, Material material)
        {
            // Create the mesh for the yarn in this row
            GameObject yarn = new GameObject($"Row {row.nRow} for {yarnWidth}/1.0f");
            MeshFilter meshFilter = yarn.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = yarn.AddComponent<MeshRenderer>();
            Mesh mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt32;
            meshFilter.mesh = mesh;

            // Set up vertices for the row based on the curve
            Vector3[] rowVertices = GenerateVerticesForRow(
                row, yarnWidth, row.nRow);
            
            // Set up triangles for the row based on the vertices
            int[] triangles = GenerateTriangles(row, rowVertices);

            mesh.vertices = rowVertices;
            mesh.triangles = triangles;
            Debug.Log($"There are {rowVertices.Length} vertices.");
            mesh.RecalculateNormals();

            // Assign a default material

            meshRenderer.material = material; 
            return yarn;
        }

        internal static Vector3[] GenerateCircle(float yarnWidth, Vector3[] curve, int j)
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
                cosTheta = (float)Math.Cos(theta);
                sinTheta = (float)Math.Sin(theta);
                cosPhi = (float)Math.Cos(phi);
                sinPhi = (float)Math.Sin(phi);
            }

            for (int i = 0; i < KnitSettings.radialRes; i++)
            {
                // Angle runs from 0 to 2*Pi
                float angle = (float)Mathf.PI * 2.0f * (float)i / (float)KnitSettings.radialRes;

                // generates circle of width yarnWidth in y-z plane
                // for each point in the stitch curve
                float cx = 0.0f;
                float cy = yarnWidth * (float)Mathf.Cos(angle);
                float cz = yarnWidth * (float)Mathf.Sin(angle);

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

        internal static Vector3[] GenerateVerticesForRow(
            Row row, float yarnWidth, int rowNumber)
        {
            Vector3[] rowCurve = Array.Empty<Vector3>();
            int loopNo = 0;
            for (int k = 0; k < row.nStitches; k++)
            {
                Stitch stitch = row.stitches[k];
                // Get the curve for the stitch
                Vector3[] rowCurve1 = stitch.GenerateCurve(
                    loopNo, yarnWidth);

                // and add to the vertices row array
                rowCurve = rowCurve.Concat(rowCurve1).ToArray();
                loopNo += stitch.loopsProduced;
            }

            // Stitch.DrawLine(rowCurve);
            // Set up vertices for the stitch based on the stitch curve
            Vector3[] rowVertices = GenerateVerticesForCurve(rowCurve, yarnWidth);
            for (int j = 0; j < rowVertices.Length; j++)
            {
                rowVertices[j].y += (float)rowNumber * (1.0f + 1.0f - 3.0f * yarnWidth);
            }
            return rowVertices;
        }
        internal static Vector3[] GenerateVerticesForCurve(Vector3[] curve, float yarnWidth)
        {
            // Generate vertices
            Vector3[] vertices = new Vector3[
                KnitSettings.radialRes * curve.Length
            ];
            for (int j = 0; j < curve.Length; j++)
            {
                Vector3[] rotatedCircle = GenerateCircle(yarnWidth, curve, j);
                Stitch.DrawLine(rotatedCircle);
                for (int i = 0; i < KnitSettings.radialRes; i++)
                {
                    vertices[j * KnitSettings.radialRes + i] = rotatedCircle[i];
                }
            }

            return vertices;
        }

        internal static int[] GenerateTriangles(Row row, Vector3[] rowVertices)
        {
            int[] triangles = new int[row.nLoops * KnitSettings.stitchRes * KnitSettings.radialRes * 6];

            int triangleIndex = 0;
            for (int j = 0; j < row.nLoops * KnitSettings.stitchRes - 1; j++)
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
                    triangles[triangleIndex] = index;
                    triangles[triangleIndex + 1] = index + KnitSettings.radialRes;
                    triangles[triangleIndex + 2] = nextIndex;
                    triangles[triangleIndex + 3] = nextIndex;
                    triangles[triangleIndex + 4] = index + KnitSettings.radialRes;
                    triangles[triangleIndex + 5] = nextIndex + KnitSettings.radialRes;
                    triangleIndex += 6;
                }
            }

            return triangles;
        }
    }
    
    public abstract class BasicStitch : Stitch
    {
        // Whether the stitch is knit or purl (indicates loop direction)
        public bool isPurlStitch;
        
        protected BasicStitch(int index) : base(index)
        {
        }

        public Vector3 GetLoop(int j, float yarnWidth, bool cableFront)
        {
            float h = 1.0f; // height of stitches
            float a = 1.6f; // width of stitch
            float d = 0.3f; // depth curve factor for stitch
            float d2 = 2.1f * yarnWidth; // depth offset for stitch

            if (cableFront)
            {
                d = 0.90f;
            }

            if (this.isPurlStitch)
            {
                d *= -1.0f;
                d2 *= -1.0f;
            }

            // j goes from 0 to stitchRes - 1 (or stitchRes for last segment)
            float angle = (float)j / (float) KnitSettings.stitchRes * 2.0f * (float) Math.PI;

            // parametric equation for stitch
            // eg from https://www.cs.cmu.edu/~kmcrane/Projects/Other/YarnCurve.pdf
            float xVal = (float)(angle + a * (float) Math.Sin(2.0f * angle)) / (float)Math.PI;
            float yVal = h * (float)Math.Cos(angle + (float)Math.PI);
            float zVal = d * (float)Math.Cos(2.0f * angle) - d2;

            return new Vector3(xVal,yVal,zVal);
        }
        
        public override Vector3[] GenerateCurve(
            int loopNoStart, int loopNoEnd, float yarnWidth, bool cableFront)
        {
            int loopOffset = loopNoEnd - loopNoStart;

            Vector3[] curveForStitch = GenerateGenericCurve(yarnWidth, cableFront);

            // Each stitch takes up 2 natural units.  Therefore, the next stitch
            // needs an offset of 2.0f from the previous stitch
            Vector3 horizontalOffset = new Vector3(2.0f * loopNoStart, 0, 0);
            for (int j = 0; j < curveForStitch.Length; j++)
            {
                curveForStitch[j] = curveForStitch[j] + horizontalOffset;

                // Apply shear if there is a stitchOffset
                if (loopOffset != 0)
                {
                    curveForStitch[j].x +=  loopOffset + loopOffset * (curveForStitch[j].y);
                }
            }

            // DrawLine(curveForStitch);

            return curveForStitch;
        }
        
        public override Vector3[] GenerateCurve(int loopNo, float yarnWidth)
        {
            return GenerateCurve(loopNo, loopNo, yarnWidth, false);
        }
        
        
        
        public Vector3[] GenerateGenericCurve(float yarnWidth, bool cableFront)
        {
            int segments = KnitSettings.stitchRes;

            Vector3[] genericCurve = new Vector3[segments];
            for (int j = 0; j < segments; j++)
            {
                genericCurve[j] = GetLoop(j, yarnWidth, cableFront);
            }

            return genericCurve;
        }
    }

    public class KnitStitch : BasicStitch
    {
        public KnitStitch(int index) : base(index)
        {
            this.stitchType = StitchType.KnitStitch;
            this.index = index;
            this.loopsConsumed = 1;
            this.loopsProduced = 1;
            this.isPurlStitch = false;
        }
    }
    
    public class PurlStitch : BasicStitch
    {
        public PurlStitch(int index) : base(index)
        {
            this.stitchType = StitchType.PurlStitch;
            this.index = index;
            this.loopsConsumed = 1;
            this.loopsProduced = 1;
            this.isPurlStitch = true;
        }
    }
    
    public abstract class CableStitch : Stitch
    {
        // The cable stitch is defined as any composite stitch
        // where the first X loops from the needle are picked up
        // and placed either in front of (front=true) or behind (front=false)
        // the needle, and the rest of the stitches are
        // knitted according to the stitchTypeList before placing
        // the held stitches back on the needle and knitting those
        
        // Indicates how many loops to hold on a stitch holder
        // before starting to knit
        public int held;
        // Indicates whether to place the first $held stitches
        // in front of (true) or behind (false) the needle
        public bool front; 
        // List of stitches to perform in order, once the held stitches
        // have been moved to the stitch holder
        public StitchType[] stitchTypeList;
        
        protected CableStitch(int index, StitchType stitchType) : base(index)
        {
            this.stitchType = stitchType;
        }
        
        public override Vector3[] GenerateCurve(int loopNo, float yarnWidth)
        {
            // loopNo = the index of the loop this stitch starts on
            // Create a curve for the whole cable stitch
            Vector3[] curveForStitch = Array.Empty<Vector3>();;

            // Work through each of the loops produced in the stitchTypeList
            for (int i = 0; i < this.loopsProduced; i++)
            {
                int xStart = loopNo + i;
                int xEnd = 0;
                bool front = false;
                // hold the first this.held stitches in front of/behind the
                // needle and first knit the remaining
                // this.loopsProduced - this.held stitches
                if (i >= this.held)
                {
                    xEnd = xStart - this.held;
                    front = (!this.front);
                }
                else
                {
                    xEnd = xStart + this.held;
                    front = (this.front);
                }

                Stitch stitch = Stitch.GetStitch(stitchTypeList[i], i);
                Vector3[] curve = stitch.GenerateCurve(xStart, xEnd, yarnWidth, front);
                // DrawLine(curve);
                curveForStitch = curveForStitch.Concat(curve).ToArray();
            }
            // DrawLine(curveForStitch);

            return curveForStitch;
        }
        public override Vector3[] GenerateCurve(int loopNoStart, int loopNoEnd, float yarnWidth, bool cableFront)
        {
            throw new NotImplementedException();
        }
    }

    public class Cable1Lo1RStitch : CableStitch
    {
        public Cable1Lo1RStitch(int index) : base(index, StitchType.Cable1Lo1RStitch)
        {
            this.index = index;
            this.loopsConsumed = 2;
            this.loopsProduced = 2;
            this.held = 1;
            this.front = false;
            this.stitchTypeList = new StitchType[2];
            this.stitchTypeList[0] = StitchType.KnitStitch;
            this.stitchTypeList[1] = StitchType.KnitStitch;

        }
    }
    
    public class Cable2Lo2RStitch : CableStitch
    {
        public Cable2Lo2RStitch(int index) : base(index, StitchType.Cable2Lo2RStitch)
        {
            this.index = index;
            this.loopsConsumed = 4;
            this.loopsProduced = 4;
            this.held = 2;
            this.front = false;
            this.stitchTypeList = new StitchType[4];
            this.stitchTypeList[0] = StitchType.KnitStitch;
            this.stitchTypeList[1] = StitchType.KnitStitch;
            this.stitchTypeList[2] = StitchType.KnitStitch;
            this.stitchTypeList[3] = StitchType.KnitStitch;
        }
    }
    
    public class CableKnitStitch : CableStitch
    {
        public CableKnitStitch(int index) : base(index, StitchType.CableKnitStitch)
        {
            this.index = index;
            this.loopsConsumed = 2;
            this.loopsProduced = 2;
            this.held = 0;
            this.front = true;
            this.stitchTypeList = new StitchType[2];
            this.stitchTypeList[0] = StitchType.KnitStitch;
            this.stitchTypeList[1] = StitchType.KnitStitch;

        }
    }
    
    public class CableKnitStitch4 : CableStitch
    {
        public CableKnitStitch4(int index) : base(index, StitchType.CableKnitStitch4)
        {
            this.index = index;
            this.loopsConsumed = 4;
            this.loopsProduced = 4;
            this.held = 0;
            this.front = true;
            this.stitchTypeList = new StitchType[4];
            this.stitchTypeList[0] = StitchType.KnitStitch;
            this.stitchTypeList[1] = StitchType.KnitStitch;
            this.stitchTypeList[2] = StitchType.KnitStitch;
            this.stitchTypeList[3] = StitchType.KnitStitch;

        }
    }
}
