using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;

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
        
        public void GeneratePattern(StitchType[,] pattern, float yarnWidth, Material material)
        {
            GameObject parent = new GameObject($"Pattern {yarnWidth}");
            for (int rowNumber = 0; rowNumber < pattern.GetLength(0); rowNumber++)
            {
                StitchType[] stitches = new StitchType[pattern.GetLength(1)];
                for (int i = 0; i < pattern.GetLength(1); i++)
                {
                    stitches[i] = pattern[rowNumber, i];
                }
                GameObject row = yarn.GenerateRow(stitches, yarnWidth, rowNumber, material);
                row.transform.SetParent(parent.transform);
            }
        }

        public void GenerateRow(StitchType[] stitches, float yarnWidth, int rowNumber, Material material)
        {
            yarn.GenerateRow(stitches, yarnWidth, rowNumber, material);
        }
        
        public Stitch GetStitch(StitchType stitchType, bool forceUpdate)
        {
            return stitchCache.GetStitch(stitchType, forceUpdate);
        }
    }
}