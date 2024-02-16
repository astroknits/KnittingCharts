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

            Vector3[] genericCurve1 = new Vector3[segments];
            if (genericCurve != null && genericCurve.Length == segments)
            {
                Array.Copy(genericCurve, genericCurve1, genericCurve.Length);
                return genericCurve1;
            }

            genericCurve = new Vector3[segments];
            for (int j = 0; j < segments; j++)
            {
                // x is defined from -0.5f to 0.5f for a given stitch
                float x = (float) j / (float) KnitSettings.stitchRes - 0.5f;
                float verticalOffset = GetVerticalOffset(x);
                float depthOffset = GetDepthOffset(x);
                genericCurve1[j] = new Vector3(
                    this.gauge * x, verticalOffset, depthOffset);
            }

            Array.Copy(genericCurve1, genericCurve, genericCurve1.Length);
            return genericCurve1;
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

        public float GetVerticalOffset(float x)
        {
            float scale = 4.0f; // range goes from -scale to +scale
            float shift = 0.8f;
            float xPos = scale * (2 * x + 0.5f);

            if (x > 0)
            {
                xPos = scale - (xPos + shift);
            }
            else
            {
                xPos = xPos - shift;
            }

            float res = GetStitchShape(xPos);
            return res;
        }
        
        public float GetDepthOffset(float x)
        {
            return 0.0f;
        }
        
        public abstract float GetStitchShape(float x);

        protected static float sigmoid(float kappa, float x)
        {
            // sigmoid(x) = 1 / (1 + exp(-x))
            return 1.0f / (1.0f + (float) Math.Exp(-1.0f * x / kappa));
        }
    }

    public class KnitStitch : Stitch
    {
        public KnitStitch(float gauge)
        {
            this.stitchLength = 1;
            this.gauge = gauge;
        }

        public override float GetStitchShape(float x)
        {
            float kappa = 0.25f;
            return sigmoid(kappa, x);
        }
        
    }
    
}