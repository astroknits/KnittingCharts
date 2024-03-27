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

        public bool IsStitchHeld(int baseStitchIndex)
        {
            return baseStitchIndex < this.stitchInfo.held;
        }

        public HoldDirection GetHoldDirection(int baseStitchIndex)
        {
            // Handle cable stitches which include holding stitches in front/behind
            // Default is to have stitchInfo.held == 0, which doesn't
            // need this logic.

            // If there are no held stitches, get the held direction
            // based on whether this current BaseStitch is being held or not
            HoldDirection holdDirection = HoldDirection.None;
            if (this.stitchInfo.held != 0)
            {
                // hold the first this.held baseStitches in front of/behind the
                // needle and first knit the remaining
                // this.nLoopsProduced - this.held baseStitches
                if (IsStitchHeld(baseStitchIndex))
                {
                    holdDirection = stitchInfo.holdDirection;
                }
                else
                {
                    if (stitchInfo.holdDirection == HoldDirection.Front)
                    {
                        holdDirection = HoldDirection.Back;
                    }
                    else if (stitchInfo.holdDirection == HoldDirection.Back)
                    {
                        holdDirection = HoldDirection.Front;
                    }
                }
            }

            return holdDirection;
        }

        public Loop[] GetLoopsConsumedByBaseStitch(int loopIndexConsumed, BaseStitchInfo baseStitchInfo)
        {
            // Create a list of the loops that are consumed by this baseStitch
            Loop[] loopsConsumedByBaseStitch = Array.Empty<Loop>();
            if (loopsConsumed is not null && baseStitchInfo.nLoopsConsumed > 0)
            {
                loopsConsumedByBaseStitch = loopsConsumed.Skip(loopIndexConsumed)
                    .Take(baseStitchInfo.nLoopsConsumed).ToArray();
            }

            return loopsConsumedByBaseStitch;
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

                Loop[] loopsConsumedByBaseStitch = GetLoopsConsumedByBaseStitch(loopIndexConsumed1, baseStitchInfo);

                HoldDirection holdDirection = GetHoldDirection(baseStitchIndex);

                baseStitches[baseStitchIndex] = new BaseStitch(
                    baseStitchInfo,
                    yarnWidth,
                    rowIndex,
                    stitchIndex,
                    baseStitchIndex,
                    this.loopIndexConsumed + loopIndexConsumed1,
                    this.loopIndexProduced + loopIndexProduced1,
                    holdDirection,
                    loopsConsumedByBaseStitch);

                loopIndexConsumed1 += baseStitchInfo.nLoopsConsumed;
                loopIndexProduced1 += baseStitchInfo.nLoopsProduced;
            }
        }

        public BaseStitch GetOrderedBaseStitch(int baseStitchIndex)
        {
            // If there are no held stitches, just return the base stitch
            // at location baseStitchIndex
            if (stitchInfo.held == 0)
            {
                return baseStitches[baseStitchIndex];
            }
            // If there are held stitches, put the held stitches
            // on the needle after the other loops from this stitch have
            // already been knitted, i.e. swap the order of the held/non-held loops
            // produced by this stitch
            if (IsStitchHeld(baseStitchIndex))
            {
                return baseStitches[baseStitchIndex + stitchInfo.held];
            }
            return baseStitches[baseStitchIndex - stitchInfo.held];
        }

        public Loop[] GetLoopsProduced()
        {
            Loop[] loopsProduced = new Loop[stitchInfo.nLoopsProduced];
            
            // cycle through all baseStitches for each stitch in row
            int loopProdducedIndex = 0;
            for (int baseStitchIndex=0; baseStitchIndex<baseStitches.Length; baseStitchIndex++)
            {
                BaseStitch baseStitch = GetOrderedBaseStitch(baseStitchIndex);

                for (int loopIndex = 0; loopIndex  < baseStitch.loopsProduced.Length; loopIndex++)
                {
                    loopsProduced[loopProdducedIndex] = baseStitch.loopsProduced[loopIndex];
                    loopProdducedIndex++;
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
                string name = $"Stitch - row {rowIndex} stitch {stitchIndex} loop {loopIndexConsumed}";
                GameObject stitchGameObject = new GameObject(name);

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
