using System.Collections.Generic;
namespace YarnGenerator
{
    public class StitchCache
    {
        public static StitchCache instance { get; private set; }
        
        public static StitchCache GetInstance()
        {
            if (instance == null)
            {
                instance = new StitchCache();
            }
            return instance;
        }
        
        private Dictionary<StitchType, Stitch> stitches = new
            Dictionary<StitchType, Stitch>();

        public Stitch GetStitch(StitchType stitchType, bool forceUpdate)
        {
            if (! stitches.ContainsKey(stitchType) || forceUpdate)
            {
                Stitch stitch = Stitch.GetStitch(stitchType);
                stitches[stitchType] = stitch;
                return stitch;
            }

            return stitches.GetValueOrDefault(stitchType);
        }
        
    }
}