namespace YarnGenerator
{
    public class LoopInfo
    {
        public LoopType loopType;
        // # of loops from previous row used by this stitch
        public int loopsConsumed;
        // # of loops left on the needle at the end of this stitch
        public int loopsProduced;
        
        public static LoopInfo GetLoopInfo(LoopType loopType)
        {
            switch (loopType)
            {
                case LoopType.Knit:
                    return new Knit();
                case LoopType.Purl:
                    return new Purl();
                case LoopType.Knit2Tog:
                    return new Knit2Tog();
                default:
                    return new Knit();
            }
        }
    }
    
    public class Knit : LoopInfo
    {
        public Knit() : base()
        {
            this.loopType = LoopType.Knit;
            this.loopsConsumed = 1;
            this.loopsProduced = 1;
        }
    }

    public class Purl : LoopInfo
    {
        public Purl(): base()
        {
            this.loopType = LoopType.Purl;
            this.loopsConsumed = 1;
            this.loopsProduced = 1;
        }
    }
    
    public class Knit2Tog : LoopInfo
    {
        public Knit2Tog() : base()
        {
            this.loopType = LoopType.Knit2Tog;
            this.loopsConsumed = 2;
            this.loopsProduced = 1;
        }
    }
    
    public class M1 : LoopInfo
    {
        public M1() : base()
        {
            this.loopType = LoopType.M1;
            this.loopsConsumed = 1;
            this.loopsProduced = 2;
        }
    }

    public class YarnOver : LoopInfo
    {
        public YarnOver() : base()
        {
            this.loopType = LoopType.YarnOver;
            this.loopsConsumed = 0;
            this.loopsProduced = 1;
        }
    }
}