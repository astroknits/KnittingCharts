using System;
using System.Linq;
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
            this.stitchDepthOffset = baseStitchInfo.stitchDepthOffset;
            if (baseStitchInfo.isPurlStitch)
            {
                this.stitchDepthFactor = -1.0f * stitchDepthFactor;
                this.stitchDepthOffset = -1.0f * stitchDepthOffset;
            }
        }

        public static BaseStitchCurve GetBaseStitchCurve(BaseStitchInfo baseStitchInfo, HoldDirection holdDirection)
        {
            switch (baseStitchInfo.BaseStitchType)
            {
                case BaseStitchType.None:
                    return new NoStitchCurve(baseStitchInfo, holdDirection);
                case BaseStitchType.Knit:
                    return new KnitStitchCurve(baseStitchInfo, holdDirection);
                case BaseStitchType.Purl:
                    return new PurlStitchCurve(baseStitchInfo, holdDirection);
                case BaseStitchType.Knit2Tog:
                    return new Knit2TogStitchCurve(baseStitchInfo, holdDirection);
                case BaseStitchType.YarnOver:
                    return new YarnOverStitchCurve(baseStitchInfo, holdDirection);
                default:
                    return new KnitStitchCurve(baseStitchInfo, holdDirection);
            }
        }

        public virtual Vector3[] GenerateCurve(
            float yarnWidth,
            int loopIndexConsumed,
            int loopIndexProduced,
            Loop[] loopsConsumed,
            Loop[] loopsProduced)
        {
            // Default rendered loop: basic knit stitch

            // check offset between where stitch was and where it ends up
            // (if it's a cable stitch that crosses over)
            float cons = GetConsumedIndex(loopIndexConsumed, loopsConsumed, 0);
            float prod = GetProducedIndex(loopIndexProduced, loopsProduced, 0);

            return GenerateSingleCurve(yarnWidth, cons, prod, stitchDepthFactor, stitchDepthOffset);
        }

        public Vector3[] GenerateSingleCurve(
            float yarnWidth,
            float consumedIndex,
            float producedIndex,
            float stitchDepthFactor,
            float stitchDepthOffset)
        {
            float loopXStart = 2.0f * consumedIndex + yarnWidth;
            float loopXOffset = (float) producedIndex - (float) consumedIndex;
            Vector3[] curve = new Vector3[stitchRes];
            for (int j = 0; j < stitchRes; j++)
            {
                curve[j] = GetLoopValueForSegment(yarnWidth, stitchDepthFactor, stitchDepthOffset, j);
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
        public Vector3 GetLoopValueForSegment(float yarnWidth, float stitchDepthFactor, float stitchDepthOffset, int j)
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

        public float GetConsumedIndex(int loopIndexConsumed, Loop[] loopsConsumed, int index)
        {
            // Get the index of the consumed stitch at provided index.
            // If there are no loops consumed, default to using the value
            // of loopIndexConsumed
            float cons = loopIndexConsumed;

            if (loopsConsumed is not null && loopsConsumed.Length > 0)
            {
                cons = loopsConsumed[index].GetIndex();
            }

            return cons;
        }

        public float GetProducedIndex(int loopIndexProduced, Loop[] loopsProduced, int index)
        {
            // Get the index of the produced stitch at provided index.
            // If there are no loops produced, default to using the value
            // of loopIndexProduced
            float prod = loopIndexProduced;
            if (loopsProduced is not null && loopsProduced.Length > 0)
            {
                prod = loopsProduced[index].GetIndex();
            }

            return prod;
        }
    }

    public class NoStitchCurve : BaseStitchCurve
    {
        public NoStitchCurve(BaseStitchInfo baseStitchInfo, HoldDirection holdDirection) : base(baseStitchInfo, holdDirection)
        {
        }
        
        public override Vector3[] GenerateCurve(
            float yarnWidth,
            int loopIndexConsumed,
            int loopIndexProduced,
            Loop[] loopsConsumed,
            Loop[] loopsProduced)
        {
            return Array.Empty<Vector3>();
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

    public class YarnOverStitchCurve : BaseStitchCurve
    {
        public YarnOverStitchCurve(BaseStitchInfo baseStitchInfo, HoldDirection holdDirection) : base(baseStitchInfo, holdDirection)
        {
        }

        public override Vector3[] GenerateCurve(
            float yarnWidth,
            int loopIndexConsumed,
            int loopIndexProduced,
            Loop[] loopsConsumed,
            Loop[] loopsProduced)
        {
            return Array.Empty<Vector3>();
        }
    }
}