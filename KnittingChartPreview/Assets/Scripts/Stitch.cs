using System;
using UnityEngine;

namespace YarnGenerator

{
    public class Stitch
    {
        public StitchType stitchType;
        // number of stitches from previous row used by this stitch definition
        public int stitchLength;

        public static Vector3[] GenerateCurve(StitchType stitchType, float gauge)
        {
            switch (stitchType)
            {
                case StitchType.KnitStitch:
                    return KnitStitch.GenerateCurve(gauge);
                case StitchType.PurlStitch:
                    return PurlStitch.GenerateCurve(gauge);
                default:
                    return KnitStitch.GenerateCurve(gauge);
            }
        }
    }

    public class KnitStitch
    {
        public static int stitchLength = 1;
        
        public static Vector3[] GenerateCurve(float gauge)
        {
            Vector3[] curve = new Vector3[KnitSettings.stitchRes + 1];
            for (int j = 0; j < KnitSettings.stitchRes + 1; j++)
            {
                float x = (float) j / (float) KnitSettings.stitchRes - 0.5f;
                float verticalOffset = GetVerticalOffset(j);
                float depthOffset = GetDepthOffset(j);
                curve[j] = new Vector3(
                    gauge * x, verticalOffset, depthOffset);
                // Debug.Log($"j, x, y: {j} {curve[j].x} {curve[j].y}");
            }

            return curve;        }
        static float sigmoid(float x)
        {
            // sigmoid(x) = 1 / (1 + exp(-x))
            return 1.0f / (1.0f + (float) Math.Exp(-1.0f * x));
        }

        static float GetVerticalOffset(int index)
        {
            float kappa = 0.25f;
            float scale = 4.0f; // range goes from -scale to +scale
            float shift = 0.8f;
            float pos = scale * (2 * (float) index / (float) KnitSettings.stitchRes - 0.5f);

            if (index * 2 > KnitSettings.stitchRes)
            {
                pos = scale - (pos + shift);
            }
            else
            {
                pos = pos - shift;
            }

            float res = sigmoid(pos / kappa);
            return res;
        }
        
        static float GetDepthOffset(int index)
        {
            return 0.0f;
        }
        
    }
    
    public class PurlStitch
    {
        public static int stitchLength = 1;

        public static Vector3[] GenerateCurve(float gauge)
        {
            Vector3[] curve = new Vector3[KnitSettings.stitchRes + 1];
            for (int j = 0; j < KnitSettings.stitchRes + 1; j++)
            {
                float x = (float) j / (float) KnitSettings.stitchRes - 0.5f;
                float verticalOffset = GetVerticalOffset(j);
                float depthOffset = GetDepthOffset(j);
                curve[j] = new Vector3(
                    gauge * x, verticalOffset, depthOffset);
                // Debug.Log($"j, x, y: {j} {curve[j].x} {curve[j].y}");
            }

            return curve;
        }
        
        static float sigmoid(float x)
        {
            // sigmoid(x) = 1 / (1 + exp(-x))
            return 1.0f / (1.0f + (float) Math.Exp(-1.0f * x));
        }

        static float GetVerticalOffset(int index)
        {
            float kappa = 0.25f;
            float scale = 4.0f; // range goes from -scale to +scale
            float shift = 0.8f;
            float pos = scale * (2 * (float) index / (float) KnitSettings.stitchRes - 0.5f);

            if (index * 2 > KnitSettings.stitchRes)
            {
                pos = scale - (pos + shift);
            }
            else
            {
                pos = pos - shift;
            }

            float res = sigmoid(pos / kappa);
            return res;
        }

        static float GetDepthOffset(int index)
        {
            return 0.0f;
        }

    }
}