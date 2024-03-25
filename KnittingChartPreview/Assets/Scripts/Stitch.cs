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
        public int loopIndexConsumed;
        // starting loop number for the given stitch
        public int loopIndexProduced;
        
        // list of loop objects
        public BaseStitch[] baseStitches;
        
        // list of consumed loops
        public Loop[] loopsConsumed;

        public Stitch(StitchInfo stitchInfo, int rowIndex, int stitchIndex, int loopIndexConsumed, int loopIndexProduced, float yarnWidth, Loop[] loopsConsumed)
        {
            this.stitchInfo = stitchInfo;
            this.rowIndex = rowIndex;
            this.stitchIndex = stitchIndex;
            this.loopIndexConsumed = loopIndexConsumed;
            this.loopIndexProduced = loopIndexProduced;
            this.yarnWidth = yarnWidth;
            this.loopsConsumed = loopsConsumed;
            GenerateBaseStitches();
        }

        public void GenerateBaseStitches()
        {
            baseStitches = new BaseStitch[this.stitchInfo.nBaseStitches];
            
            int loopIndexConsumed1 = 0;
            int loopIndexProduced1 = 0;
            // Work through each of the baseStitches produced in the baseStitchTypeList
            for (int baseStitchIndex = 0; baseStitchIndex < this.stitchInfo.nBaseStitches; baseStitchIndex++)
            {
                BaseStitchType baseStitchType = stitchInfo.baseStitchInfoList[baseStitchIndex].BaseStitchType;
                BaseStitchInfo baseStitchInfo = BaseStitchInfo.GetBaseStitchInfo(baseStitchType);

                // Create a list of the loops that are consumed by this baseStitch
                Loop[] loopsConsumedByBaseStitch = Array.Empty<Loop>();
                if (loopsConsumed is not null && baseStitchInfo.nLoopsConsumed > 0)
                {
                    loopsConsumedByBaseStitch = loopsConsumed.Skip(loopIndexConsumed1).Take(baseStitchInfo.nLoopsConsumed).ToArray();
                }

                // Handle cable stitches which include holding stitches in front/behind
                // Default is to have stitchInfo.held == 0, which doesn't
                // need this logic.

                // If there are no held stitches, the index of the first produced loop
                // is equal to the index of the first consumed loop
                int loopIndexProduced2 = loopIndexProduced1;
                HoldDirection holdDirection = HoldDirection.None;
                if (this.stitchInfo.held != 0)
                {
                    // hold the first this.held baseStitches in front of/behind the
                    // needle and first knit the remaining
                    // this.nLoopsProduced - this.held baseStitches
                    if (baseStitchIndex >= this.stitchInfo.held)
                    {
                        loopIndexProduced1 = loopIndexProduced2 - this.stitchInfo.held;
                        holdDirection = stitchInfo.holdDirection;
                    }
                    else
                    {
                        loopIndexProduced1 = loopIndexProduced2 + this.stitchInfo.held;
                        if (stitchInfo.holdDirection == HoldDirection.Front)
                        {
                            holdDirection = HoldDirection.Back;
                        } else if (stitchInfo.holdDirection == HoldDirection.Back)
                        {
                            holdDirection = HoldDirection.Front;
                        }
                    }
                }

                baseStitches[baseStitchIndex] = new BaseStitch(
                    baseStitchInfo,
                    yarnWidth,
                    rowIndex,
                    stitchIndex,
                    baseStitchIndex,
                    this.loopIndexConsumed + loopIndexConsumed1,
                    this.loopIndexProduced + loopIndexProduced2,
                    holdDirection,
                    loopsConsumedByBaseStitch);

                loopIndexConsumed1 += baseStitchInfo.nLoopsConsumed;
                loopIndexProduced1 += baseStitchInfo.nLoopsProduced;
            }
        }

        public Loop[] GetLoopsProduced()
        {
            Loop[] loopsProduced = new Loop[stitchInfo.nLoopsProduced];

            // cycle through all baseStitches for each stitch in row
            int j = 0;
            foreach (BaseStitch baseStitch in baseStitches)
            {
                for (int i = 0; i  < baseStitch.loopsProduced.Length; i++)
                {
                    loopsProduced[j] = baseStitch.loopsProduced[i];
                    j++;
                }
            }

            return loopsProduced;
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
                GameObject stitchGameObject = new GameObject($"Stitch - row {rowIndex} stitch {stitchIndex} loop {loopIndexConsumed}");

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
