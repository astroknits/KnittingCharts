namespace YarnGenerator
{
    public class StitchInfo
    {
        // List of stitches to perform in order.  
        public StitchType stitchType;
        // # of baseStitches that comprise this stitch
        public int nBaseStitches;
        // # of loops from previous row used by this stitch
        public int nLoopsConsumed;
        // # of loops left on the needle at the end of this stitch
        public int nLoopsProduced;

        // List of base stitches for this stitch (eg knit, purl),
        // to perform.
        // Stitches are performed in this order once the
        // held stitches have been moved to the stitch holder
        public BaseStitchInfo[] baseStitchInfoList;
        
        // The cable stitch is defined as any composite stitch
        // where the first X baseStitches from the needle are picked up
        // and placed either in front of (front=true) or behind (front=false)
        // the needle, and the rest of the stitches are
        // knitted according to the baseStitchInfoList before placing
        // the held stitches back on the needle and knitting those
        
        // held parameter indicates how many baseStitches to hold on a stitch holder
        // before starting to knit
        public int held;
        // Indicates whether to place the first $held stitches
        // in front of (true) or behind (false) the needle
        public HoldDirection holdDirection;
        // If decreasing, indicates which direction the
        // following stitches will slant (eg m1L -> left, ssk -> left)
        public ShiftDirection shiftDirection;


        public StitchInfo()
        {
            this.held = 0;
            this.holdDirection = HoldDirection.None;
            this.shiftDirection = ShiftDirection.None;
        }

        public static StitchInfo GetStitchInfo(StitchType stitchType)
        {
            switch (stitchType)
            {
                case StitchType.KnitStitch:
                    return new KnitStitch();
                case StitchType.PurlStitch:
                    return new PurlStitch();
                case StitchType.Cable1Lo1RStitch:
                    return new Cable1Lo1RStitch();
                case StitchType.Cable2Lo2RStitch:
                    return new Cable2Lo2RStitch();
                case StitchType.CableKnitStitch:
                    return new CableKnitStitch();
                case StitchType.CableKnitStitch4:
                    return new CableKnitStitch4();
                case StitchType.Knit2TogStitch:
                    return new Knit2TogStitch();
                case StitchType.SSKStitch:
                    return new SSKStitch();
                case StitchType.YarnOverStitch:
                    return new YarnOverStitch();
                case StitchType.M1Stitch:
                    return new M1Stitch();
                default:
                    return new KnitStitch();
            }
        }

        public BaseStitchInfo[] GetBaseStitchInfoList(BaseStitchType[] baseStitchTypeList)
        {
            BaseStitchInfo[] baseStitchInfoList = new BaseStitchInfo[baseStitchTypeList.Length];
            for (int i = 0; i < baseStitchInfoList.Length; i++)
            {
                baseStitchInfoList[i] = BaseStitchInfo.GetBaseStitchInfo(baseStitchTypeList[i]);
            }

            return baseStitchInfoList;
        }
    }
    
    public class KnitStitch : StitchInfo
    {
        public KnitStitch() : 
            base()
        {
            this.stitchType = StitchType.KnitStitch;
            this.nBaseStitches = 1;
            this.nLoopsConsumed = 1;
            this.nLoopsProduced = 1;
            BaseStitchType[] baseStitchTypeList = new BaseStitchType[1]
            {
                BaseStitchType.Knit
            };
            this.baseStitchInfoList = GetBaseStitchInfoList(baseStitchTypeList);
        }
    }

    public class PurlStitch : StitchInfo
    {
        public PurlStitch() : base()
        {
            this.stitchType = StitchType.PurlStitch;
            this.nBaseStitches = 1;
            this.nLoopsConsumed = 1;
            this.nLoopsProduced = 1;
            BaseStitchType[] baseStitchTypeList = new BaseStitchType[1]
            {
                BaseStitchType.Purl
            };
            this.baseStitchInfoList = GetBaseStitchInfoList(baseStitchTypeList);

        }
    }

    public class SSKStitch : StitchInfo
    {
        public SSKStitch() : 
            base()
        {
            this.stitchType = StitchType.SSKStitch;
            this.nBaseStitches = 1;
            this.nLoopsConsumed = 2;
            this.nLoopsProduced = 1;
            this.shiftDirection = ShiftDirection.Left;
            BaseStitchType[] baseStitchTypeList = new BaseStitchType[1]
            {
                BaseStitchType.SSK
            };
            this.baseStitchInfoList = GetBaseStitchInfoList(baseStitchTypeList);
        }
    }

    public class Knit2TogStitch : StitchInfo
    {
        public Knit2TogStitch() : 
            base()
        {
            this.stitchType = StitchType.Knit2TogStitch;
            this.nBaseStitches = 1;
            this.nLoopsConsumed = 2;
            this.nLoopsProduced = 1;
            this.shiftDirection = ShiftDirection.Right;
            BaseStitchType[] baseStitchTypeList = new BaseStitchType[1]
            {
                BaseStitchType.Knit2Tog
            };
            this.baseStitchInfoList = GetBaseStitchInfoList(baseStitchTypeList);
        }
    }

    public class M1Stitch : StitchInfo
    {
        public M1Stitch() : 
            base()
        {
            this.stitchType = StitchType.M1Stitch;
            this.nBaseStitches = 1;
            this.nLoopsConsumed = 1;
            this.nLoopsProduced = 2;
            BaseStitchType[] baseStitchTypeList = new BaseStitchType[1]
            {
                BaseStitchType.M1
            };
            this.baseStitchInfoList = GetBaseStitchInfoList(baseStitchTypeList);
        }
    }

    public class YarnOverStitch : StitchInfo
    {
        public YarnOverStitch() : 
            base()
        {
            this.stitchType = StitchType.YarnOverStitch;
            this.nBaseStitches = 1;
            this.nLoopsConsumed = 0;
            this.nLoopsProduced = 1;
            BaseStitchType[] baseStitchTypeList = new BaseStitchType[1]
            {
                BaseStitchType.YarnOver
            };
            this.baseStitchInfoList = GetBaseStitchInfoList(baseStitchTypeList);
        }
    }

    public class Cable1Lo1RStitch : StitchInfo
    {
        public Cable1Lo1RStitch() : base()
        {
            this.stitchType = StitchType.Cable1Lo1RStitch;
            this.nBaseStitches = 2;
            this.nLoopsConsumed = 2;
            this.nLoopsProduced = 2;
            this.held = 1;
            this.holdDirection = HoldDirection.Back;
            BaseStitchType[] baseStitchTypeList = new BaseStitchType[2]
            {
                BaseStitchType.Knit,
                BaseStitchType.Knit
            };
            this.baseStitchInfoList = GetBaseStitchInfoList(baseStitchTypeList);
        }
    }
    
    public class Cable2Lo2RStitch : StitchInfo
    {
        public Cable2Lo2RStitch() : base()
        {
            this.stitchType = StitchType.Cable2Lo2RStitch;
            this.nBaseStitches = 4;
            this.nLoopsConsumed = 4;
            this.nLoopsProduced = 4;
            this.held = 2;
            this.holdDirection = HoldDirection.Back;
            BaseStitchType[] baseStitchTypeList = new BaseStitchType[4]
            {
                BaseStitchType.Knit,
                BaseStitchType.Knit,
                BaseStitchType.Knit,
                BaseStitchType.Knit
            };
            this.baseStitchInfoList = GetBaseStitchInfoList(baseStitchTypeList);
        }
    }
    
    public class CableKnitStitch : StitchInfo
    {
        public CableKnitStitch() : base()
        {
            this.stitchType = StitchType.CableKnitStitch;
            this.nBaseStitches = 2;
            this.nLoopsConsumed = 2;
            this.nLoopsProduced = 2;
            BaseStitchType[] baseStitchTypeList = new BaseStitchType[2]
            {
                BaseStitchType.Knit,
                BaseStitchType.Knit
            };
            this.baseStitchInfoList = GetBaseStitchInfoList(baseStitchTypeList);
        }
    }
    
    public class CableKnitStitch4 : StitchInfo
    {
        public CableKnitStitch4() : base()
        {
            this.stitchType = StitchType.CableKnitStitch4;
            this.nBaseStitches = 4;
            this.nLoopsConsumed = 4;
            this.nLoopsProduced = 4;
            BaseStitchType[] baseStitchTypeList = new BaseStitchType[4] {
                BaseStitchType.Knit,
                BaseStitchType.Knit,
                BaseStitchType.Knit,
                BaseStitchType.Knit
            };
            this.baseStitchInfoList = GetBaseStitchInfoList(baseStitchTypeList);
        }
    }
}