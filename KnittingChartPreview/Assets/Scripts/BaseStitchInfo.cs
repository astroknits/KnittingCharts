namespace YarnGenerator
{
    public class BaseStitchInfo
    {
        public BaseStitchType BaseStitchType;
        // # of baseStitches from previous row used by this stitch
        public int loopsConsumed;
        // # of baseStitches left on the needle at the end of this stitch
        public int loopsProduced;
        
        public static BaseStitchInfo GetBaseStitchInfo(BaseStitchType baseStitchType)
        {
            switch (baseStitchType)
            {
                case BaseStitchType.Knit:
                    return new Knit();
                case BaseStitchType.Purl:
                    return new Purl();
                case BaseStitchType.Knit2Tog:
                    return new Knit2Tog();
                case BaseStitchType.YarnOver:
                    return new YarnOver();
                case BaseStitchType.M1:
                    return new M1();
                default:
                    return new Knit();
            }
        }
    }
    
    public class Knit : BaseStitchInfo
    {
        public Knit() : base()
        {
            this.BaseStitchType = BaseStitchType.Knit;
            this.loopsConsumed = 1;
            this.loopsProduced = 1;
        }
    }

    public class Purl : BaseStitchInfo
    {
        public Purl(): base()
        {
            this.BaseStitchType = BaseStitchType.Purl;
            this.loopsConsumed = 1;
            this.loopsProduced = 1;
        }
    }
    
    public class Knit2Tog : BaseStitchInfo
    {
        public Knit2Tog() : base()
        {
            this.BaseStitchType = BaseStitchType.Knit2Tog;
            this.loopsConsumed = 2;
            this.loopsProduced = 1;
        }
    }
    
    public class M1 : BaseStitchInfo
    {
        public M1() : base()
        {
            this.BaseStitchType = BaseStitchType.M1;
            this.loopsConsumed = 1;
            this.loopsProduced = 2;
        }
    }

    public class YarnOver : BaseStitchInfo
    {
        public YarnOver() : base()
        {
            this.BaseStitchType = BaseStitchType.YarnOver;
            this.loopsConsumed = 0;
            this.loopsProduced = 1;
        }
    }
}