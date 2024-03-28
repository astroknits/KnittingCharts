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
        // (default false)
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
            GenerateLoopsProduced();
        }

        public void GenerateLoopsProduced()
        {
            this.loopsProduced = new Loop[this.baseStitchInfo.nLoopsProduced];
            for (int i = 0; i < this.baseStitchInfo.nLoopsProduced; i++)
            {
                Loop loop = new Loop(rowIndex, loopIndexProduced + i, this);
                this.loopsProduced[i] = loop;
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