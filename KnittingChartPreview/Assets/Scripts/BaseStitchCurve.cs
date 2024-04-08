using System;
using System.Linq;
using UnityEngine;

namespace YarnGenerator
{
    public class BaseStitchCurve
    {
        public int stitchRes = KnitSettings.stitchRes;

        public BaseStitchInfo baseStitchInfo;

        internal float yarnWidth;
        internal float stitchHeight;  // height of stitches
        internal float stitchWidth;  // width of stitch
        internal float stitchDepthFactor;  // depth curve factor for stitch
        internal float stitchDepthOffset;  // depth offset for stitch
        internal Vector3[] curve; // curve definition for stitch
        internal float loopXStart; // translation
        internal float loopXOffset; // rotation
        internal float loopYOffset; // KnitSettings.stitchHeight * rowIndex * (2.0f - 3.0f * yarnWidth)
        internal float rotationAngle; // rotation of stitch from upright
        internal float loopLength; // length of the loop, for scaling

        public BaseStitchCurve(
            BaseStitchInfo baseStitchInfo,
            HoldDirection holdDirection,
            float yarnWidth,
            int rowIndex,
            int loopIndexConsumed,
            int loopIndexProduced,
            Loop[] loopsConsumed,
            Loop[] loopsProduced
        )
        {
            this.baseStitchInfo = baseStitchInfo;
            this.yarnWidth = yarnWidth;
            this.stitchHeight = baseStitchInfo.stitchHeight;
            this.stitchWidth = baseStitchInfo.stitchWidth;
            this.stitchDepthFactor = baseStitchInfo.stitchDepthFactorDict[holdDirection];
            this.stitchDepthOffset = baseStitchInfo.stitchDepthOffset;
            if (baseStitchInfo.isPurlStitch)
            {
                this.stitchDepthFactor = -1.0f * stitchDepthFactor;
                this.stitchDepthOffset = -1.0f * stitchDepthOffset;
            }
            float consumedIndex = GetConsumedIndex(loopIndexConsumed, loopsConsumed, 0);
            float producedIndex = GetProducedIndex(loopIndexProduced, loopsProduced, 0);
            loopXStart = 2.0f * consumedIndex + yarnWidth;
            loopXOffset = (float) producedIndex - (float) consumedIndex;
            Debug.Log($"   {loopXStart}/{loopXOffset}");
            loopYOffset = KnitSettings.stitchHeight * rowIndex * (2.0f - 3.0f * yarnWidth);
            rotationAngle = (float) Math.Atan(
                loopXOffset * KnitSettings.stitchWidth / KnitSettings.stitchHeight
                );
            /*
            loopLength = (float) Math.Sqrt(
                (float)Math.Pow(2.0f * loopXOffset * KnitSettings.stitchWidth, 2) +
                (float)Math.Pow(2.0f * KnitSettings.stitchHeight, 2))/2.0f;
            */
            loopLength = 1.0f;
            rotationAngle = rotationAngle * 180.0f / (float) Math.PI;
        }

        public static BaseStitchCurve GetBaseStitchCurve(
            BaseStitchInfo baseStitchInfo,
            HoldDirection holdDirection,
            float yarnWidth,
            int rowIndex,
            int loopIndexConsumed,
            int loopIndexProduced,
            Loop[] loopsConsumed,
            Loop[] loopsProduced
            )
        {
            switch (baseStitchInfo.BaseStitchType)
            {
                case BaseStitchType.None:
                    return new NoStitchCurve(
                        baseStitchInfo,
                        holdDirection,
                        yarnWidth,
                        rowIndex,
                        loopIndexConsumed,
                        loopIndexProduced,
                        loopsConsumed,
                        loopsProduced);
                case BaseStitchType.Knit:
                    return new KnitStitchCurve(
                        baseStitchInfo,
                        holdDirection,
                        yarnWidth,
                        rowIndex,
                        loopIndexConsumed,
                        loopIndexProduced,
                        loopsConsumed,
                        loopsProduced);
                case BaseStitchType.Purl:
                    return new PurlStitchCurve(
                        baseStitchInfo,
                        holdDirection, 
                        yarnWidth,
                        rowIndex,
                        loopIndexConsumed,
                        loopIndexProduced,
                        loopsConsumed,
                        loopsProduced);
                case BaseStitchType.Knit2Tog:
                    return new Knit2TogStitchCurve(
                        baseStitchInfo,
                        holdDirection, 
                        yarnWidth,
                        rowIndex,
                        loopIndexConsumed,
                        loopIndexProduced,
                        loopsConsumed,
                        loopsProduced);
                case BaseStitchType.YarnOver:
                    return new YarnOverStitchCurve(
                        baseStitchInfo,
                        holdDirection, 
                        yarnWidth,
                        rowIndex,
                        loopIndexConsumed,
                        loopIndexProduced,
                        loopsConsumed,
                        loopsProduced);
                default:
                    return new KnitStitchCurve(
                        baseStitchInfo,
                        holdDirection, 
                        yarnWidth,
                        rowIndex,
                        loopIndexConsumed,
                        loopIndexProduced,
                        loopsConsumed,
                        loopsProduced);
            }
        }

        public virtual void GenerateCurve()
        {
            // Default rendered loop: basic knit stitch

            // check offset between where stitch was and where it ends up
            // (if it's a cable stitch that crosses over)

            GenerateSingleCurve(yarnWidth, stitchDepthFactor, stitchDepthOffset);
        }

        public void GenerateSingleCurve(
            float yarnWidth,
            float stitchDepthFactor,
            float stitchDepthOffset)
        {
            curve = new Vector3[stitchRes];
            float xMin = 1000000;
            float xMax = 0;
            float yMin = 1000000;
            float yMax = 0;
            for (int j = 0; j < stitchRes; j++)
            {
                curve[j] = GetLoopValueForSegment(yarnWidth, stitchDepthFactor, stitchDepthOffset, j);
                if (curve[j].x > xMax)
                {
                    xMax = curve[j].x;
                }

                if (curve[j].x < xMin)
                {
                    xMin = curve[j].x;
                }
                if (curve[j].y > yMax)
                {
                    yMax = curve[j].y;
                }

                if (curve[j].y < yMin)
                {
                    yMin = curve[j].y;
                }
            }

            for (int j = 0; j < curve.Length; j++)
            {
                curve[j].x = -1.0f * (curve[j].x - (xMax - xMin)/2);
                curve[j].y = curve[j].y - yMin;
            }
        }

        public Vector3 GetLoopValueForSegment(float yarnWidth, float stitchDepthFactor, float stitchDepthOffset, int j)
        {
            // j goes from 0 to stitchRes - 1 (or stitchRes for last segment)
            // angle goes from 0 to  2pi
            float angle = (float) j / (float) stitchRes * 2.0f * (float) Math.PI;

            // parametric equation for stitch
            // eg from https://www.cs.cmu.edu/~kmcrane/Projects/Other/YarnCurve.pdf
            float xVal = (float) (angle + stitchWidth * (float) Math.Sin(2.0f * angle)) / (float) Math.PI;
            // max xVal -> sin = 1 when angle=45 or 135 or 225 or 315.  
            // min xVal -> sin = 0 when angle=0, 90, 180, 270, 360
            // min xVal = 0
            // max xVal = 315/180.  
            // -> 
            float yVal = KnitSettings.stitchHeight * stitchHeight * (float) Math.Cos(angle + (float) Math.PI);
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
        public NoStitchCurve(
            BaseStitchInfo baseStitchInfo,
            HoldDirection holdDirection,
            float yarnWidth,
            int rowIndex,
            int loopIndexConsumed,
            int loopIndexProduced,
            Loop[] loopsConsumed,
            Loop[] loopsProduced
        ) : base(
            baseStitchInfo,
            holdDirection,
            yarnWidth,
            rowIndex,
            loopIndexConsumed,
            loopIndexProduced,
            loopsConsumed,
            loopsProduced
        )
        {
        }

        public override void GenerateCurve()
        {
            curve = Array.Empty<Vector3>();
        }
    }

    public class KnitStitchCurve : BaseStitchCurve
    {
        public KnitStitchCurve(
            BaseStitchInfo baseStitchInfo,
            HoldDirection holdDirection,
            float yarnWidth,
            int rowIndex,
            int loopIndexConsumed,
            int loopIndexProduced,
            Loop[] loopsConsumed,
            Loop[] loopsProduced
        ) : base(
            baseStitchInfo,
            holdDirection,
            yarnWidth,
            rowIndex,
            loopIndexConsumed,
            loopIndexProduced,
            loopsConsumed,
            loopsProduced
        )
        {
        }
    }

    public class PurlStitchCurve : BaseStitchCurve
    {
        public PurlStitchCurve(
            BaseStitchInfo baseStitchInfo,
            HoldDirection holdDirection,
            float yarnWidth,
            int rowIndex,
            int loopIndexConsumed,
            int loopIndexProduced,
            Loop[] loopsConsumed,
            Loop[] loopsProduced
        ) : base(
            baseStitchInfo,
            holdDirection,
            yarnWidth,
            rowIndex,
            loopIndexConsumed,
            loopIndexProduced,
            loopsConsumed,
            loopsProduced
        )
        {
        }
    }

    public class Knit2TogStitchCurve : BaseStitchCurve
    {
        public Knit2TogStitchCurve(
            BaseStitchInfo baseStitchInfo,
            HoldDirection holdDirection,
            float yarnWidth,
            int rowIndex,
            int loopIndexConsumed,
            int loopIndexProduced,
            Loop[] loopsConsumed,
            Loop[] loopsProduced
        ) : base(
            baseStitchInfo,
            holdDirection,
            yarnWidth,
            rowIndex,
            loopIndexConsumed,
            loopIndexProduced,
            loopsConsumed,
            loopsProduced
        )
        {
        }
    }

    public class SSKStitchCurve : BaseStitchCurve
    {
        public SSKStitchCurve(
            BaseStitchInfo baseStitchInfo,
            HoldDirection holdDirection,
            float yarnWidth,
            int rowIndex,
            int loopIndexConsumed,
            int loopIndexProduced,
            Loop[] loopsConsumed,
            Loop[] loopsProduced
        ) : base(
            baseStitchInfo,
            holdDirection,
            yarnWidth,
            rowIndex,
            loopIndexConsumed,
            loopIndexProduced,
            loopsConsumed,
            loopsProduced
        )
        {
        }
    }

    public class YarnOverStitchCurve : BaseStitchCurve
    {
        public YarnOverStitchCurve(
            BaseStitchInfo baseStitchInfo,
            HoldDirection holdDirection,
            float yarnWidth,
            int rowIndex,
            int loopIndexConsumed,
            int loopIndexProduced,
            Loop[] loopsConsumed,
            Loop[] loopsProduced
        ) : base(
            baseStitchInfo,
            holdDirection,
            yarnWidth,
            rowIndex,
            loopIndexConsumed,
            loopIndexProduced,
            loopsConsumed,
            loopsProduced
        )
        {
        }

        public override void GenerateCurve()
        {
            curve = Array.Empty<Vector3>();
        }
    }
}