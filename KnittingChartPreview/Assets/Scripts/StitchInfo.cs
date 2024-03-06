namespace YarnGenerator
{
    public class StitchInfo
    {
        // List of stitches to perform in order.  
        public StitchType stitchType;
        // # of loops from previous row used by this stitch
        public int loopsConsumed;
        // # of loops left on the needle at the end of this stitch
        public int loopsProduced;

        // List of loop types for this stitch (knit, purl), to perform.
        // Stitches are performed in this order once the
        // held stitches have been moved to the stitch holder
        public LoopInfo[] loopInfoList;
        
        // The cable stitch is defined as any composite stitch
        // where the first X loops from the needle are picked up
        // and placed either in front of (front=true) or behind (front=false)
        // the needle, and the rest of the stitches are
        // knitted according to the loopInfoList before placing
        // the held stitches back on the needle and knitting those
        
        // held parameter indicates how many loops to hold on a stitch holder
        // before starting to knit
        public int held;
        // Indicates whether to place the first $held stitches
        // in front of (true) or behind (false) the needle
        public bool front;


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
                default:
                    return new KnitStitch();
            }
        }

        public LoopInfo[] GetLoopInfoList(LoopType[] loopTypeList)
        {
            LoopInfo[] loopInfoList = new LoopInfo[loopTypeList.Length];
            for (int i = 0; i < loopInfoList.Length; i++)
            {
                loopInfoList[i] = LoopInfo.GetLoopInfo(loopTypeList[i]);
            }

            return loopInfoList;
        }
    }
    
    public class KnitStitch : StitchInfo
    {
        public KnitStitch() : 
            base()
        {
            this.stitchType = StitchType.KnitStitch;
            this.loopsConsumed = 1;
            this.loopsProduced = 1;
            this.held = 0;
            this.front = false;
            LoopType[] loopTypeList = new LoopType[1]
            {
                LoopType.Knit
            };
            this.loopInfoList = GetLoopInfoList(loopTypeList);
        }
    }

    public class PurlStitch : StitchInfo
    {
        public PurlStitch() : base()
        {
            this.stitchType = StitchType.PurlStitch;
            this.loopsConsumed = 1;
            this.loopsProduced = 1;
            this.held = 0;
            this.front = false;
            LoopType[] loopTypeList = new LoopType[1]
            {
                LoopType.Purl
            };
            this.loopInfoList = GetLoopInfoList(loopTypeList);

        }
    }

    public class Cable1Lo1RStitch : StitchInfo
    {
        public Cable1Lo1RStitch() : base()
        {
            this.stitchType = StitchType.Cable1Lo1RStitch;
            this.loopsConsumed = 2;
            this.loopsProduced = 2;
            this.held = 1;
            this.front = false;
            LoopType[] loopTypeList = new LoopType[2]
            {
                LoopType.Knit,
                LoopType.Knit
            };
            this.loopInfoList = GetLoopInfoList(loopTypeList);
        }
    }
    
    public class Cable2Lo2RStitch : StitchInfo
    {
        public Cable2Lo2RStitch() : base()
        {
            this.stitchType = StitchType.Cable2Lo2RStitch;
            this.loopsConsumed = 4;
            this.loopsProduced = 4;
            this.held = 2;
            this.front = false;
            LoopType[] loopTypeList = new LoopType[4]
            {
                LoopType.Knit,
                LoopType.Knit,
                LoopType.Knit,
                LoopType.Knit
            };
            this.loopInfoList = GetLoopInfoList(loopTypeList);
        }
    }
    
    public class CableKnitStitch : StitchInfo
    {
        public CableKnitStitch() : base()
        {
            this.stitchType = StitchType.CableKnitStitch;
            this.loopsConsumed = 2;
            this.loopsProduced = 2;
            this.held = 0;
            this.front = true;
            LoopType[] loopTypeList = new LoopType[2]
            {
                LoopType.Knit,
                LoopType.Knit
            };
            this.loopInfoList = GetLoopInfoList(loopTypeList);
        }
    }
    
    public class CableKnitStitch4 : StitchInfo
    {
        public CableKnitStitch4() : base()
        {
            this.stitchType = StitchType.CableKnitStitch4;
            this.loopsConsumed = 4;
            this.loopsProduced = 4;
            this.held = 0;
            this.front = true;
            LoopType[] loopTypeList = new LoopType[4] {
                LoopType.Knit,
                LoopType.Knit,
                LoopType.Knit,
                LoopType.Knit
            };
            this.loopInfoList = GetLoopInfoList(loopTypeList);
        }
    }
}