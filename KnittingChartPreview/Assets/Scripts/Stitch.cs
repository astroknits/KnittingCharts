using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace YarnGenerator

{
    public abstract class Stitch
    {
        public StitchType stitchType;
        // # of stitches from previous row used by this stitch definition
        public int stitchLength;
        // Whether the stitch is knit or purl (indicates loop direction)
        public bool isPurlStitch;
        private Vector3[] genericCurve;

        public static Stitch GetStitch(StitchType stitchType)
        {
            switch (stitchType)
            {
                case StitchType.KnitStitch:
                    return new KnitStitch();
                case StitchType.PurlStitch:
                    return new PurlStitch();
                default:
                    return new KnitStitch();
            }
        }

        private Vector3[] GenerateGenericCurve(float yarnWidth, bool lastStitch, bool isPurlStitch)
        {
            int segments = KnitSettings.stitchRes;
            if (lastStitch)
            {
                segments += 1;
            }

            Vector3[] genericCurveCopy = new Vector3[segments];
            if (genericCurve != null && genericCurve.Length == segments)
            {
                Array.Copy(genericCurve, genericCurveCopy, genericCurve.Length);
                return genericCurveCopy;
            }

            genericCurve = new Vector3[segments];
            for (int j = 0; j < segments; j++)
            {
                genericCurveCopy[j] = GetLoop(j, yarnWidth, isPurlStitch);
            }

            Array.Copy(genericCurveCopy, genericCurve, genericCurveCopy.Length);
            return genericCurveCopy;
        }

        public Vector3[] GenerateCurve(int stitchNo, float yarnWidth,  bool lastStitch, bool isPurlStitch)
        {
            Vector3[] curveForStitch = GenerateGenericCurve(yarnWidth, lastStitch, isPurlStitch);

            // Each stitch takes up 2 natural units.  Therefore, the next stitch
            // needs an offset of 2.0f from the previous stitch
            Vector3 horizontalOffset = new Vector3(2.0f * stitchNo, 0, 0);
            for (int j = 0; j < curveForStitch.Length; j++)
            {
                curveForStitch[j] = curveForStitch[j] + horizontalOffset;
            }

            return curveForStitch;
        }
        
        public Vector3 GetLoop(int j, float yarnWidth, bool isPurlStitch)
        {
            float h = 1.0f; // height of stitches
            float a = 1.6f; // width of stitch
            float d = 0.3f; // depth curve factor for stitch
            float d2 = 2.1f * yarnWidth; // depth offset for stitch

            if (isPurlStitch)
            {
                d *= -1.0f;
                d2 *= -1.0f;
            }

            // j goes from 0 to stitchRes - 1 (or stitchRes for last segment)
            float angle = (float) j / (float) KnitSettings.stitchRes * 2 * (float) Math.PI;

            // parametric equation for stitch
            // eg from https://www.cs.cmu.edu/~kmcrane/Projects/Other/YarnCurve.pdf
            float xVal = (angle + a * (float) Math.Sin(2.0f * angle)) / (float)Math.PI;
            float yVal = h * (float)Math.Cos(angle + (float)Math.PI);
            float zVal = d * (float)Math.Cos(2.0f * angle) - d2;

            return new Vector3(xVal,yVal,zVal);
        }
    }

    public class KnitStitch : Stitch
    {
        public KnitStitch()
        {
            this.stitchLength = 1;
            this.isPurlStitch = false;
        }
    }
    
    public class PurlStitch : Stitch
    {
        public PurlStitch()
        {
            this.stitchLength = 1;
            this.isPurlStitch = true;
        }
    }
}