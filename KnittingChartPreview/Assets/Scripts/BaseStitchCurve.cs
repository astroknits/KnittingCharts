using System;
using UnityEngine;

namespace YarnGenerator
{
    public class BaseStitchCurve
    {
        public int stitchRes = KnitSettings.stitchRes;

        public BaseStitchInfo baseStitchInfo;

        internal float stitchHeight;  // height of stitches
        internal float stitchWidth;  // width of stitch
        internal float stitchDepthFactor;  // depth curve factor for stitch
        internal float stitchDepthOffset;  // depth offset for stitch

        public BaseStitchCurve(BaseStitchInfo baseStitchInfo, HoldDirection holdDirection)
        {
            this.baseStitchInfo = baseStitchInfo;
            this.stitchHeight = baseStitchInfo.stitchHeight;
            this.stitchWidth = baseStitchInfo.stitchWidth;
            this.stitchDepthFactor = baseStitchInfo.stitchDepthFactorDict[holdDirection];
            Debug.Log($"{baseStitchInfo.BaseStitchType}/{holdDirection}");
            this.stitchDepthOffset = baseStitchInfo.stitchDepthOffset;
        }

        public static BaseStitchCurve GetBaseStitchCurve(BaseStitchInfo baseStitchInfo, HoldDirection holdDirection)
        {
            switch (baseStitchInfo.BaseStitchType)
            {
                case BaseStitchType.Knit:
                    return new KnitStitchCurve(baseStitchInfo, holdDirection);
                case BaseStitchType.Purl:
                    return new PurlStitchCurve(baseStitchInfo, holdDirection);
                case BaseStitchType.Knit2Tog:
                    return new Knit2TogStitchCurve(baseStitchInfo, holdDirection);
                default:
                    return new KnitStitchCurve(baseStitchInfo, holdDirection);
            }
        }

        public Vector3[] GenerateCurve(
            float yarnWidth,
            int loopIndexConsumed,
            int loopIndexProduced,
            Loop[] loopsConsumed,
            Loop[] loopsProduced)
        {
            int cons = GetConsumedIndex(loopIndexConsumed, loopsConsumed);
            int prod = GetProducedIndex(loopIndexProduced, loopsProduced);

            float loopXStart = 2.0f * cons + yarnWidth;
            float loopXOffset = (float) prod - (float) cons;
            return GenerateSingleCurve(yarnWidth, loopXStart, loopXOffset);
        }

        public Vector3[] GenerateSingleCurve(
            float yarnWidth,
            float loopXStart,
            float loopXOffset)
        {
            Vector3[] curve = new Vector3[stitchRes];
            for (int j = 0; j < stitchRes; j++)
            {
                curve[j] = GetLoopValueForSegment(yarnWidth, j);
            }

            curve = ApplyHorizontalOffset(yarnWidth, loopXStart, loopXOffset,  curve);

            for (int j = 0; j < curve.Length; j++)
            {
                curve[j].x = -1.0f * curve[j].x;
            }
            return curve;
        }

        public Vector3[] ApplyHorizontalOffset(float yarnWidth, float loopXStart, float loopXOffset, Vector3[] curve)
        {
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

            return curve;
        }
        public Vector3 GetLoopValueForSegment(float yarnWidth, int j)
        {
            // j goes from 0 to stitchRes - 1 (or stitchRes for last segment)
            float angle = (float) j / (float) stitchRes * 2.0f * (float) Math.PI;

            // parametric equation for stitch
            // eg from https://www.cs.cmu.edu/~kmcrane/Projects/Other/YarnCurve.pdf
            float xVal = (float) (angle + stitchWidth * (float) Math.Sin(2.0f * angle)) / (float) Math.PI;
            float yVal = stitchHeight * (float) Math.Cos(angle + (float) Math.PI);
            float zVal = stitchDepthFactor * (float) Math.Cos(2.0f * angle) - stitchDepthOffset * yarnWidth;

            return new Vector3(xVal, yVal, zVal);
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
    }

    public class KnitStitchCurve : BaseStitchCurve
    {
        public KnitStitchCurve(BaseStitchInfo baseStitchInfo, HoldDirection holdDirection) : base(baseStitchInfo, holdDirection)
        {
        }
    }

    public class PurlStitchCurve : BaseStitchCurve
    {
        public PurlStitchCurve(BaseStitchInfo baseStitchInfo, HoldDirection holdDirection) : base(baseStitchInfo, holdDirection)
        {
        }
    }

    public class Knit2TogStitchCurve : BaseStitchCurve
    {
        public Knit2TogStitchCurve(BaseStitchInfo baseStitchInfo, HoldDirection holdDirection) : base(baseStitchInfo, holdDirection)
        {
        }
    }

    public class SSKStitchCurve : BaseStitchCurve
    {
        public SSKStitchCurve(BaseStitchInfo baseStitchInfo, HoldDirection holdDirection) : base(baseStitchInfo, holdDirection)
        {
        }
    }
}