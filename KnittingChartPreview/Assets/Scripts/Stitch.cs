using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace YarnGenerator

{
    public abstract class Stitch
    {
        public StitchType stitchType;
        // # of loops from previous row used by this stitch
        public int loopsConsumed;
        // # of loops left on the needle at the end of this stitch
        public int loopsProduced;
        protected Vector3[] genericCurve;

        public static Stitch GetStitch(StitchType stitchType)
        {
            switch (stitchType)
            {
                case StitchType.KnitStitch:
                    return new KnitStitch();
                case StitchType.PurlStitch:
                    return new PurlStitch();
                case StitchType.Cable1Lo1RStitch:
                    return new Cable1Lo1RStitch();
                default:
                    return new KnitStitch();
            }
        }

        public abstract Vector3[] GenerateCurve(int loopNo, float yarnWidth, bool lastStitch);
        
    }
    
    public abstract class BasicStitch : Stitch
    {
        // Whether the stitch is knit or purl (indicates loop direction)
        public bool isPurlStitch;
        
        public Vector3 GetLoop(int j, float yarnWidth)
        {
            float h = 1.0f; // height of stitches
            float a = 1.6f; // width of stitch
            float d = 0.3f; // depth curve factor for stitch
            float d2 = 2.1f * yarnWidth; // depth offset for stitch

            if (this.isPurlStitch)
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
        
        public override Vector3[] GenerateCurve(int loopNo, float yarnWidth,  bool lastStitch)
        {
            Vector3[] curveForStitch = GenerateGenericCurve(yarnWidth, lastStitch);

            // Each stitch takes up 2 natural units.  Therefore, the next stitch
            // needs an offset of 2.0f from the previous stitch
            Vector3 horizontalOffset = new Vector3(2.0f * loopNo, 0, 0);
            for (int j = 0; j < curveForStitch.Length; j++)
            {
                curveForStitch[j] = curveForStitch[j] + horizontalOffset;
            }

            return curveForStitch;
        }
        
        public Vector3[] GenerateGenericCurve(float yarnWidth, bool lastStitch)
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
                genericCurveCopy[j] = GetLoop(j, yarnWidth);
            }

            Array.Copy(genericCurveCopy, genericCurve, genericCurveCopy.Length);
            return genericCurveCopy;
        }
    }

    public class KnitStitch : BasicStitch
    {
        public KnitStitch()
        {
            this.loopsConsumed = 1;
            this.loopsProduced = 1;
            this.isPurlStitch = false;
        }
    }
    
    public class PurlStitch : BasicStitch
    {
        public PurlStitch()
        {
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
        private YarnCache yarnCache = YarnCache.GetInstance();
        
        public override Vector3[] GenerateCurve(int loopNo, float yarnWidth,  bool lastStitch)
        {
            // loopNo = the index of the loop this stitch starts on
            // Create a curve for the whole cable stitch
            Vector3[] curveForStitch = Array.Empty<Vector3>();;

            // Work through each of the loops produced in the stitchTypeList
            for (int i = 0; i < this.loopsProduced; i++)
            {
                int offset = loopNo + i;
                Stitch stitch = Stitch.GetStitch(stitchTypeList[i]);
                curveForStitch = curveForStitch.Concat(
                    stitch.GenerateCurve(offset, yarnWidth, lastStitch)).ToArray();
            }

            return curveForStitch;
        }
    }

    public class Cable1Lo1RStitch : CableStitch
    {
        public Cable1Lo1RStitch()
        {
            this.loopsConsumed = 2;
            this.loopsProduced = 2;
            this.held = 1;
            this.front = false;
            this.stitchTypeList = new StitchType[2];
            this.stitchTypeList[0] = StitchType.KnitStitch;
            this.stitchTypeList[1] = StitchType.KnitStitch;

        }
    }
}
