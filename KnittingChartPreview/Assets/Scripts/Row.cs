using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace YarnGenerator
{
    public class Row
    {
        // Index for this row
        public int rowIndex;
        // Loop index offset for this row
        // For cases where the index of the first loop
        // is not zero (eg after right leaning decreases or increases)
        public int loopIndexOffset;

        // Array of stitch objects
        public Stitch[] stitches;
        public int nStitches;
        public int nBaseStitches;
        public int nLoopsConsumed;
        public int nLoopsProduced;

        public Row prevRow;
        public Row nextRow;

        internal BaseStitch[] baseStitches;
        internal Loop[] loopsConsumed;
        internal Loop[] loopsProduced;

        public Row(int rowIndex, Row prevRow, StitchType[] stitchTypes)
        {
            this.rowIndex = rowIndex;
            this.loopIndexOffset = 0;

            // Set the previous row, and set the loops consumed to
            // be equal to the set of loops produced by the previous row.
            SetPreviousRow(prevRow);
            // Set the number of stitches, base stitches, and
            // loops produced/consumed for this row
            Configure(stitchTypes);
            // Set the loops consumed by this row
            SetLoopsConsumed();
            // Create array of Stitch objects
            GenerateStitches(stitchTypes);
        }

        public void Configure(StitchType[] stitchTypes)
        {
            // Calculate number of Stitch objects
            this.nStitches = stitchTypes.Length;

            // Calculate number of BaseStitch objects for row
            // Also calculate the number of baseStitches consumed and produced
            // for this row (calc by looping through the BaseStitch objects)
            this.nBaseStitches = 0;
            this.nLoopsConsumed = 0;
            this.nLoopsProduced = 0;
            foreach (StitchType stitchType in stitchTypes)
            {
                StitchInfo stitchInfo = StitchInfo.GetStitchInfo(stitchType);
                this.nLoopsConsumed += stitchInfo.nLoopsConsumed;
                this.nLoopsProduced += stitchInfo.nLoopsProduced;
                this.nBaseStitches += stitchInfo.nBaseStitches;
            }
        }

        public void UpdateLoops()
        {
            foreach (Stitch stitch in stitches)
            {
                foreach (BaseStitch baseStitch in stitch.baseStitches)
                {
                    continue;
                    // baseStitch.UpdateLoopsForBaseStitch();
                }
            }
        }

        public int GetRowIndexOffset(BaseStitch baseStitch)
        {
            if (baseStitch.baseStitchInfo.BaseStitchType == BaseStitchType.SSK)
            {
                return -1;
            } else if (baseStitch.baseStitchInfo.BaseStitchType == BaseStitchType.Knit2Tog)
            {
                return -1;
            } else if (baseStitch.baseStitchInfo.BaseStitchType == BaseStitchType.YarnOver)
            {
                return 1;
            } else if (baseStitch.baseStitchInfo.BaseStitchType == BaseStitchType.M1)
            {
                return 1;
            }

            return 0;
        }

        public void SetPreviousRow(Row prevRowObj)
        {
            if (prevRowObj != null)
            {
                prevRow = prevRowObj;
                prevRowObj.nextRow = this;
            }
        }

        private void GenerateStitches(StitchType[] stitchTypes)
        {
            this.stitches = new Stitch[stitchTypes.Length];

            int loopIndexConsumed = 0;
            int loopIndexProduced = 0;
            Loop prevLoop = null;
            for (int stitchIndex = 0; stitchIndex < stitchTypes.Length; stitchIndex++)
            {
                StitchType stitchType = stitchTypes[stitchIndex];
                StitchInfo stitchInfo = StitchInfo.GetStitchInfo(stitchType);

                // Get loops consumed in the stitch
                // based on the start loop index for the stitch and number of loops consumed
                Loop[] loopsConsumedInStitch = GetLoopsConsumed(loopIndexConsumed, stitchInfo.nLoopsConsumed);

                Stitch stitch = new Stitch(
                    stitchInfo,
                    rowIndex,
                    stitchIndex,
                    loopIndexConsumed,
                    loopIndexProduced,
                    prevLoop,
                    loopsConsumedInStitch
                    );
                this.stitches[stitchIndex] = stitch;
                prevLoop = stitch.GetLoopsProduced().LastOrDefault();
                loopIndexConsumed += stitchInfo.nLoopsConsumed;
                loopIndexProduced += stitchInfo.nLoopsProduced;
            }
        }

        public void SetLoopsConsumed()
        {
            if (prevRow is not null)
            {
                loopsConsumed = prevRow.GetLoopsProduced();
            }
            else
            {
                Loop prevLoop = null;
                loopsConsumed = new Loop[nLoopsConsumed];
                for (int i = 0; i < nLoopsConsumed; i++)
                {
                    loopsConsumed[i] = new Loop(rowIndex - 1, i, prevLoop, null);
                }
            }
        }

        public Loop[] GetLoopsConsumed(int start, int nLoops)
        {
            if (loopsConsumed is null)
            {
                return Array.Empty<Loop>();
            }
            return loopsConsumed.Where(loop => !loop.IsNull()).Skip(start).Take(nLoops).ToArray();
        }

        public Loop[] GetLoopsProduced()
        {
            if (loopsProduced is not null)
            {
                return loopsProduced;
            }

            loopsProduced = new Loop[nLoopsProduced];

            int loopIndex = 0;
            foreach (Stitch stitch in stitches)
            {
                foreach (Loop loop in stitch.GetLoopsProduced())
                {
                    loopsProduced[loopIndex] = loop;
                    loopIndex++;
                }
            }

            return loopsProduced;
        }
        
        public BaseStitch[] GetBaseStitches()
        {
            // use cached value
            if (baseStitches is not null)
            {
                return baseStitches;
            }

            baseStitches = new BaseStitch[this.nBaseStitches];

            int baseStitchIndex = 0;
            for (int i = 0; i < stitches.Length; i++)
            {
                Stitch stitch = stitches[i];
                for (int j = 0; j < stitch.stitchInfo.nBaseStitches; j++)
                {
                    baseStitches[baseStitchIndex] = stitch.baseStitches[j];
                    baseStitchIndex += 1;
                }
            }
            return baseStitches;
        }

        public GameObject GeneratePreview(float yarnWidth, Material material)
        {
            GameObject rowGameObject = new GameObject($"Row {this.rowIndex} for yarnWidth {yarnWidth}");
            foreach (Stitch stitch in this.stitches)
            {
                GameObject stitchGameObject = stitch.GenerateMesh(yarnWidth, material);
                stitchGameObject.transform.SetParent(rowGameObject.transform);
            }

            return rowGameObject;
        }
    }
}