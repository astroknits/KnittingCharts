namespace YarnGenerator
{
    public class YarnCache
    {
        public static YarnCache instance { get; private set; }
        private Yarn yarn = new Yarn();
        private StitchCache stitchCache = StitchCache.GetInstance();

        // Default constructor set to private to prevent direct
        // construction calls with the 'new' operator
        private YarnCache() { }
        
        public static YarnCache GetInstance()
        {
            if (instance == null)
            {
                instance = new YarnCache();
            }
            return instance;
        }

        public void GenerateRow(StitchType[] stitches, float yarnWidth, float gauge, int rowNumber)
        {
            yarn.GenerateRow(stitches, yarnWidth, gauge, rowNumber);
        }
        
        public Stitch GetStitch(StitchType stitchType, float gauge, bool forceUpdate)
        {
            return stitchCache.GetStitch(stitchType, gauge, forceUpdate);
        }
    }
}