using System;
using System.Collections.Generic;
using System.Linq;

namespace YarnGenerator
{
    public class BaseStitchInfo
    {
        public BaseStitchType BaseStitchType;

        // indicates whether the stitch is knitwise or purlwise
        public bool isPurlStitch;
        // # of baseStitches from previous row used by this stitch
        public int nLoopsConsumed;
        // # of baseStitches left on the needle at the end of this stitch
        public int nLoopsProduced;
        // Direction to shift loops (if the stitch results in an increase or decrease)
        public ShiftDirection shiftDirection;

        internal float stitchHeight;  // height of stitches
        internal float stitchWidth;  // width of stitch
        internal Dictionary<HoldDirection, float> stitchDepthFactorDict;  // depth curve factor for stitch
        internal float stitchDepthOffset;  // depth offset for stitch

        public BaseStitchInfo()
        {
            isPurlStitch = false;
            stitchHeight = 1.0f;
            stitchWidth = 1.6f;
            stitchDepthFactorDict = new Dictionary<HoldDirection, float>()
            {
                {HoldDirection.None, 0.3f},
                {HoldDirection.Front, 0.55f},
                {HoldDirection.Back, 0.10f}
            };  
            stitchDepthOffset = 2.1f;
            this.shiftDirection = ShiftDirection.None;
        }
        
        public static BaseStitchInfo GetBaseStitchInfo(BaseStitchType baseStitchType)
        {
            switch (baseStitchType)
            {
                case BaseStitchType.None:
                    return new None();
                case BaseStitchType.Knit:
                    return new Knit();
                case BaseStitchType.Purl:
                    return new Purl();
                case BaseStitchType.Knit2Tog:
                    return new Knit2Tog();
                case BaseStitchType.Knit3Tog:
                    return new Knit3Tog();
                case BaseStitchType.SSK:
                    return new SSK();
                case BaseStitchType.YarnOver:
                    return new YarnOver();
                case BaseStitchType.M1:
                    return new M1();
                case BaseStitchType.K1KtblK1:
                    return new K1KtblK1();
                default:
                    return new Knit();
            }
        }
    }
    
    public class None : BaseStitchInfo
    {
        public None() : base()
        {
            this.BaseStitchType = BaseStitchType.None;
            this.nLoopsConsumed = 0;
            this.nLoopsProduced = 1;
        }
    }

    public class Knit : BaseStitchInfo
    {
        public Knit() : base()
        {
            this.BaseStitchType = BaseStitchType.Knit;
            this.nLoopsConsumed = 1;
            this.nLoopsProduced = 1;
        }
    }

    public class Purl : BaseStitchInfo
    {
        public Purl(): base()
        {
            this.isPurlStitch = true;
            this.BaseStitchType = BaseStitchType.Purl;
            this.nLoopsConsumed = 1;
            this.nLoopsProduced = 1;
        }
    }
    
    public class Knit2Tog : BaseStitchInfo
    {
        public Knit2Tog() : base()
        {
            this.BaseStitchType = BaseStitchType.Knit2Tog;
            this.nLoopsConsumed = 2;
            this.nLoopsProduced = 1;
            this.shiftDirection = ShiftDirection.Right;
        }
    }
    
    public class Knit3Tog : BaseStitchInfo
    {
        public Knit3Tog() : base()
        {
            this.BaseStitchType = BaseStitchType.Knit3Tog;
            this.nLoopsConsumed = 3;
            this.nLoopsProduced = 1;
            this.shiftDirection = ShiftDirection.None;
        }
    }

    public class SSK : BaseStitchInfo
    {
        public SSK() : base()
        {
            this.BaseStitchType = BaseStitchType.SSK;
            this.nLoopsConsumed = 2;
            this.nLoopsProduced = 1;
            this.shiftDirection = ShiftDirection.Left;
        }
    }

    public class M1 : BaseStitchInfo
    {
        public M1() : base()
        {
            this.BaseStitchType = BaseStitchType.M1;
            this.nLoopsConsumed = 1;
            this.nLoopsProduced = 2;
            this.shiftDirection = ShiftDirection.Left;
        }
    }

    public class K1KtblK1 : BaseStitchInfo
    {
        public K1KtblK1() : base()
        {
            this.BaseStitchType = BaseStitchType.K1KtblK1;
            this.nLoopsConsumed = 1;
            this.nLoopsProduced = 3;
            this.shiftDirection = ShiftDirection.None;
        }
    }

    public class YarnOver : BaseStitchInfo
    {
        public YarnOver() : base()
        {
            this.BaseStitchType = BaseStitchType.YarnOver;
            this.nLoopsConsumed = 0;
            this.nLoopsProduced = 1;
            this.shiftDirection = ShiftDirection.Left;
        }
    }
}