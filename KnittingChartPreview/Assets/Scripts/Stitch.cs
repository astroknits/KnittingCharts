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

                // Index of the first loop consumed by this baseStitch
                int loopIndexConsumed = this.loopIndex + baseStitchIndex;

                // Create a list of the loops that are consumed by this baseStitch
                Loop[] loopsConsumedByBaseStitch = Array.Empty<Loop>();
                if (loopsConsumed is not null && baseStitchInfo.nLoopsConsumed > 0)
                {
                    loopsConsumedByBaseStitch = loopsConsumed.Skip(loopIndexConsumed).Take(baseStitchInfo.nLoopsConsumed).ToArray();
                }

                // Handle cable stitches which include holding stitches in front/behind
                // Default is to have stitchInfo.held == 0, which doesn't
                // need this logic.

                // If there are no held stitches, the index of the first produced loop
                // is equal to the index of the first consumed loop
                int loopIndexProduced = loopIndexConsumed;
                HoldDirection holdDirection = HoldDirection.None;
                if (this.stitchInfo.held != 0)
                {
                    // hold the first this.held baseStitches in front of/behind the
                    // needle and first knit the remaining
                    // this.nLoopsProduced - this.held baseStitches
                    if (baseStitchIndex >= this.stitchInfo.held)
                    {
                        loopIndexProduced = loopIndexConsumed - this.stitchInfo.held;
                        holdDirection = stitchInfo.holdDirection;
                    }
                    else
                    {
                        loopIndexProduced = loopIndexConsumed + this.stitchInfo.held;
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
                    loopIndexConsumed,
                    loopIndexProduced,
                    holdDirection,
                    loopsConsumedByBaseStitch);
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
