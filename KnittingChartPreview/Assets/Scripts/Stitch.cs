using System;
using UnityEngine;

namespace YarnGenerator

{
    public abstract class Stitch
    {
        public StitchType stitchType;
        // # of stitches from previous row used by this stitch definition
        public int stitchLength;
        // Length of each individual stitch
        public float gauge;
        private Vector3[] genericCurve;

        public static Stitch GetStitch(StitchType stitchType, float gauge)
        {
            switch (stitchType)
            {
                case StitchType.KnitStitch:
                    return new KnitStitch(gauge);
                case StitchType.PurlStitch:
                    return new KnitStitch(gauge);
                default:
                    return new KnitStitch(gauge);
            }
        }

        private Vector3[] GenerateGenericCurve(bool lastStitch)
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
                genericCurveCopy[j] = GetLoop(j);
            }

            Array.Copy(genericCurveCopy, genericCurve, genericCurveCopy.Length);
            return genericCurveCopy;
        }

        public Vector3[] GenerateCurve(int stitchNo, bool lastStitch)
        {
            Vector3[] curveForStitch = GenerateGenericCurve(lastStitch);

            Vector3 horizontalOffset = new Vector3(this.gauge * stitchNo, 0, 0);
            for (int j = 0; j < curveForStitch.Length; j++)
            {
                curveForStitch[j] = curveForStitch[j] + horizontalOffset;
            }

            return curveForStitch;
        }
        
        public abstract Vector3 GetLoop(int i);

    }

    public class KnitStitch : Stitch
    {
        public KnitStitch(float gauge)
        {
            this.stitchLength = 1;
            this.gauge = gauge;
        }
        
        public override Vector3 GetLoop(int j)
        {
            float h = 1.0f; // height of stitches
            float a = 1.6f; // width of stitch
            float d = 0.3f; // depth offset for stitch

            // j goes from 0 to stitchRes - 1 (or stitchRes for last segment)
            float angle = (float) j / (float) KnitSettings.stitchRes * 2 * (float) Math.PI;

            // parametric equation for stitch
            // eg from https://www.cs.cmu.edu/~kmcrane/Projects/Other/YarnCurve.pdf
            float xVal = (angle + a * (float) Math.Sin(2.0f * angle)) / (float)Math.PI;
            float yVal = h * (float)Math.Cos(angle + (float)Math.PI);
            float zVal = d * (float)Math.Cos(2.0f * angle);

            return new Vector3(xVal,yVal,zVal);
        }
    }
    
}