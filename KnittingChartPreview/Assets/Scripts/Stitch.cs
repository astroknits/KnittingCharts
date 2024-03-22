using System;
using System.Linq;
using UnityEngine;


namespace YarnGenerator

{
    public class Stitch
    {
        // StitchInfo object, containing StitchType and set of
        // attributes that define the stitch.  
        public StitchInfo stitchInfo;
        // width of the yarn
        public float yarnWidth;
        // row number for the given stitch
        public int rowIndex;
        // stitch number for the given stitch
        public int stitchIndex;
        // starting loop number for the given stitch
        public int loopIndex;
        
        // list of loop objects
        public BaseStitch[] baseStitches;
        
        // list of consumed loops
        public Loop[] loopsConsumed;
        
        public Stitch(StitchInfo stitchInfo, int rowIndex, int stitchIndex, int loopIndex, float yarnWidth, Loop[] loopsConsumed)
        {
            this.stitchInfo = stitchInfo;
            this.rowIndex = rowIndex;
            this.stitchIndex = stitchIndex;
            this.loopIndex = loopIndex;
            this.yarnWidth = yarnWidth;
            this.loopsConsumed = loopsConsumed;
            GenerateBaseStitches();
        }

        public void GenerateBaseStitches()
        {
            baseStitches = new BaseStitch[this.stitchInfo.nBaseStitches];
            
            // Work through each of the baseStitches produced in the baseStitchTypeList
            for (int baseStitchIndex = 0; baseStitchIndex < this.stitchInfo.nBaseStitches; baseStitchIndex++)
            {
                BaseStitchType baseStitchType = stitchInfo.baseStitchInfoList[baseStitchIndex].BaseStitchType;
                BaseStitchInfo baseStitchInfo = BaseStitchInfo.GetBaseStitchInfo(baseStitchType);
                int loopIndexConsumed = this.loopIndex + baseStitchIndex;
                int loopIndexProduced = loopIndexConsumed;

                Loop[] loopsConsumedByBaseStitch = Array.Empty<Loop>();
                if (loopsConsumed is not null && baseStitchInfo.nLoopsConsumed > 0)
                {
                    loopsConsumedByBaseStitch = loopsConsumed.Skip(loopIndexProduced).Take(baseStitchInfo.nLoopsConsumed).ToArray();
                }
                bool heldInFront = false;
                bool heldBehind = false;
                if (this.stitchInfo.held != 0)
                {
                    // hold the first this.held baseStitches in front of/behind the
                    // needle and first knit the remaining
                    // this.nLoopsProduced - this.held baseStitches
                    if (baseStitchIndex >= this.stitchInfo.held)
                    {
                        loopIndexProduced = loopIndexConsumed - this.stitchInfo.held;
                        heldInFront = this.stitchInfo.front;
                        heldBehind = !this.stitchInfo.front;
                    }
                    else
                    {
                        loopIndexProduced = loopIndexConsumed + this.stitchInfo.held;
                        heldInFront = !this.stitchInfo.front;
                        heldBehind = this.stitchInfo.front;
                    }
                }

                baseStitches[baseStitchIndex] = new BaseStitch(
                    baseStitchInfo, 
                    yarnWidth, 
                    rowIndex, 
                    stitchIndex,
                    baseStitchIndex,
                    loopIndexConsumed,
                    loopIndexProduced,
                    heldInFront,
                    heldBehind,
                    loopsConsumedByBaseStitch);
            }
        }

        public void GenerateCurve()
        {
            // Work through each of the baseStitches produced in the baseStitchInfoList
            for (int i = 0; i < this.baseStitches.Length; i++)
            {
                baseStitches[i].GenerateCurve();
            }
        }

        public GameObject GenerateMesh(Material material)
            {
                // Generate curves for each loop of this stitch
                // loopNo = the index of the loop this stitch starts on
                // Create a curve for each loop in the stitch
                
                // Create parent GameObject under which to nest the mesh for each loop
                GameObject stitchGameObject = new GameObject($"Stitch - row {rowIndex} stitch {stitchIndex} loop {loopIndex}");

                // Work through each of the baseStitches produced in the baseStitchTypeList
                for (int i = 0; i < this.baseStitches.Length; i++)
                {
                    GameObject mesh = baseStitches[i].GenerateMesh(material);
                    mesh.transform.SetParent(stitchGameObject.transform);
                }

                return stitchGameObject;
            }
    }
}
