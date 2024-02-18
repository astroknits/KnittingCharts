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
        
        private Dictionary<StitchType, Dictionary<float, Stitch>> stitches = new
            Dictionary<StitchType, Dictionary<float, Stitch>>();

        public Stitch GetStitch(StitchType stitchType, float gauge, bool forceUpdate)
        {
            Dictionary<float, Stitch> gauges;
            if (! stitches.ContainsKey(stitchType))
            {
                // Instantiate new dictionary for this stitch type
                gauges = new Dictionary<float, Stitch>();
                Stitch stitch = Stitch.GetStitch(stitchType, gauge);
                gauges.Add(gauge, stitch);
                stitches.Add(stitchType, gauges);
                return stitch;
            }

            gauges = stitches.GetValueOrDefault(stitchType);
            if (! gauges.ContainsKey(gauge) || forceUpdate)
            {
                Stitch stitch = Stitch.GetStitch(stitchType, gauge);
                gauges[gauge] = stitch;
                stitches.Add(stitchType, gauges);
                return stitch;
            }
            return stitches.GetValueOrDefault(stitchType).GetValueOrDefault(gauge);
        }
        
    }
}