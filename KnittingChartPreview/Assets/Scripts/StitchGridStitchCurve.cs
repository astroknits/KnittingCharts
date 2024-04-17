using System;
using System.Linq;
using UnityEngine;

namespace YarnGenerator
{
    public class StitchGridStitchCurve
    {
        public int stitchRes = KnitSettings.stitchRes;

        public StitchGridNodeType nodeType;

        internal float yarnWidth;
        internal float stitchHeight;  // height of stitches
        internal float stitchWidth;  // width of stitch
        internal float stitchDepthFactor;  // depth curve factor for stitch
        internal float stitchDepthOffset;  // depth offset for stitch
        internal Vector3[] curve; // curve definition for stitch
        internal float loopXStart; // translation
        internal float loopXOffset; // rotation
        internal float loopYOffset; // KnitSettings.stitchHeight * rowIndex * (2.0f - 3.0f * yarnWidth)

        public StitchGridStitchCurve(
            StitchGridNodeType nodeType,
            float yarnWidth,
            int rowIndex,
            int loopIndexConsumed,
            int loopIndexProduced
        )
        {
            this.nodeType = nodeType;
            this.yarnWidth = yarnWidth;
            this.stitchHeight = 1.0f;
            this.stitchWidth = 1.6f;
            this.stitchDepthFactor = 0.3f;
            this.stitchDepthOffset = 2.1f;

            float consumedIndex = loopIndexConsumed;
            float producedIndex = loopIndexProduced;
            loopXStart = 2.0f * consumedIndex + yarnWidth;
            loopXOffset = (float) producedIndex - (float) consumedIndex;
            loopYOffset = KnitSettings.stitchHeight * rowIndex * (2.0f - 3.0f * yarnWidth);
        }

        public static StitchGridStitchCurve GetStitchGridStitchCurve(
            StitchGridNodeType nodeType,
            float yarnWidth,
            int rowIndex,
            int loopIndexConsumed,
            int loopIndexProduced
            )
        {
            switch (nodeType)
            {
                case StitchGridNodeType.Knit:
                    return new StitchGridKnitStitchCurve(
                        nodeType,
                        yarnWidth,
                        rowIndex,
                        loopIndexConsumed,
                        loopIndexProduced);
                default:
                    return new StitchGridKnitStitchCurve(
                        nodeType,
                        yarnWidth,
                        rowIndex,
                        loopIndexConsumed,
                        loopIndexProduced);
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
    }

    public class StitchGridKnitStitchCurve : StitchGridStitchCurve
    {
        public StitchGridKnitStitchCurve(
            StitchGridNodeType nodeType,
            float yarnWidth,
            int rowIndex,
            int loopIndexConsumed,
            int loopIndexProduced
        ) : base(
            nodeType,
            yarnWidth,
            rowIndex,
            loopIndexConsumed,
            loopIndexProduced)
        {
        }

        public override void GenerateCurve()
        {
            GenerateSingleCurve(yarnWidth, stitchDepthFactor, stitchDepthOffset);
        }
    }
}