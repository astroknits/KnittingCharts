using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace YarnGenerator
{
    public class BaseStitch
    {
        public BaseStitchInfo baseStitchInfo;

        // row number for the given stitch
        public int rowIndex;

        // stitch index for the given stitch
        public int stitchIndex;

        public int baseStitchIndex;

        // BaseStitch indices: actual integer baseStitch # for 
        // start and end of the stitch
        // baseStitch index for the given baseStitch (at base)
        public int loopIndexConsumed;

        // baseStitch index for the given baseStitch (once baseStitch is completed)
        public int loopIndexProduced;

        // for cables, whether the baseStitch is held in front or back
        // (default None)
        public HoldDirection holdDirection;

        public Loop[] loopsConsumed;
        public Loop[] loopsProduced;

        public BaseStitch(
            BaseStitchInfo baseStitchInfo,
            int rowIndex,
            int stitchIndex,
            int baseStitchIndex,
            int loopIndexConsumed,
            int loopIndexProduced,
            HoldDirection holdDirection,
            Loop prevLoop,
            Loop[] loopsConsumed
        )
        {
            this.baseStitchInfo = baseStitchInfo;
            this.rowIndex = rowIndex;
            this.stitchIndex = stitchIndex;
            this.baseStitchIndex = baseStitchIndex;

            this.loopIndexConsumed = loopIndexConsumed;
            this.loopIndexProduced = loopIndexProduced;
            this.holdDirection = holdDirection;
            this.loopsConsumed = loopsConsumed;
            SetLoopsConsumed();
            GenerateLoopsProduced(prevLoop);
        }

        public void SetLoopsConsumed()
        {
            foreach (Loop loop in this.loopsConsumed)
            {
                loop.SetConsumedBy(this);
            }
        }

        public void GenerateLoopsProduced(Loop prevLoop)
        {
            this.loopsProduced = new Loop[this.baseStitchInfo.nLoopsProduced];
            Loop prevLoopInStitch = prevLoop;
            for (int i = 0; i < this.baseStitchInfo.nLoopsProduced; i++)
            {
                Loop loop = new Loop(rowIndex, loopIndexProduced + i, prevLoopInStitch, this);
                this.loopsProduced[i] = loop;
            }
        }

        public void UpdateLoopsForBaseStitch()
        {
            foreach (Loop loop in this.loopsProduced)
            {
                if (baseStitchInfo.BaseStitchType == BaseStitchType.Knit2Tog)
                {
                    Loop[] prevRowConsumed = loop.producedBy.loopsConsumed;
                    Loop prevRowConsumedLoop = prevRowConsumed[1];
                    prevRowConsumedLoop.AddIndexOffset(-1, baseStitchInfo.shiftDirection);
                    prevRowConsumedLoop.producedBy.baseStitchInfo.stitchDepthFactorDict[HoldDirection.None] = 0.6f;

                    if (prevRowConsumedLoop.producedBy is not null)
                    {
                        foreach (Loop test in prevRowConsumedLoop.producedBy.loopsProduced)
                        {
                            if (test.producedBy.loopsConsumed is not null)
                            {
                                foreach (Loop test2 in test.producedBy.loopsConsumed)
                                {
                                    test2.AddXOffset(-0.4f, ShiftDirection.Right);
                                }
                            }
                        }
                    }
                }
            }
        } 

        public GameObject GenerateMesh(float yarnWidth, Material material)
        {
            YarnMeshGenerator yarnMeshGenerator = new YarnMeshGenerator(baseStitchInfo);
            return yarnMeshGenerator.GenerateMesh(
                yarnWidth,
                material,
                rowIndex,
                loopIndexConsumed,
                loopIndexProduced,
                loopsConsumed,
                loopsProduced,
                holdDirection
                );
        }


    }
}