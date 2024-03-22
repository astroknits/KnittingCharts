using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace YarnGenerator
{
    public class Row
    {
        public float yarnWidth;
        // Index for this row
        public int rowIndex;
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

        public Row(int rowIndex, Row prevRow, StitchType[] stitchTypes, float yarnWidth)
        {
            this.yarnWidth = yarnWidth;
            this.rowIndex = rowIndex;

            // Set the previous row, and set the loops consumed to
            // be equal to the set of loops produced by the previous row.
            SetPreviousRow(prevRow);
            SetLoopsConsumed();
            // Generate the stitches for this row, and set the number of
            // stitches, base stitches, and loops produced/consumed
            Configure(stitchTypes);
        }

        public void Configure(StitchType[] stitchTypes)
        {
            // Create array of Stitch objects
            GenerateStitches(stitchTypes);
            // Calculate number of Stitch objects
            this.nStitches = this.stitches.Length;

            // Calculate number of BaseStitch objects for row
            // Also calculate the number of baseStitches consumed and produced
            // for this row (calc by looping through the BaseStitch objects)
            this.nBaseStitches = 0;
            this.nLoopsConsumed = 0;
            this.nLoopsProduced = 0;
            foreach (Stitch stitch in stitches)
            {
                foreach (BaseStitch baseStitch in stitch.baseStitches)
                {
                    this.nLoopsConsumed += baseStitch.baseStitchInfo.nLoopsConsumed;
                    this.nLoopsProduced += baseStitch.baseStitchInfo.nLoopsProduced;
                    this.nBaseStitches += 1;
                }
            }

        }
        public BaseStitch GetBaseStitch(int i)
        {
            return GetBaseStitches()[i];
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

            int loopIndex = 0;
            for (int stitchIndex = 0; stitchIndex < stitchTypes.Length; stitchIndex++)
            {
                StitchType stitchType = stitchTypes[stitchIndex];
                StitchInfo stitchInfo = StitchInfo.GetStitchInfo(stitchType);
                Loop[] loopsConsumedInStitch = GetLoopsConsumed(loopIndex, stitchInfo.nLoopsConsumed);
                Stitch stitch = new Stitch(stitchInfo, rowIndex, stitchIndex, loopIndex, yarnWidth, loopsConsumedInStitch);
                this.stitches[stitchIndex] = stitch;
                loopIndex += stitchInfo.nLoopsConsumed;
            }
        }

        public void SetLoopsConsumed()
        {
            if (prevRow is not null)
            {
                loopsConsumed = prevRow.GetLoopsProduced();
            }
        }

        public Loop[] GetLoopsConsumed(int start, int nLoops)
        {
            if (loopsConsumed is null)
            {
                return Array.Empty<Loop>();
            }
            return loopsConsumed.Skip(start).Take(nLoops).ToArray();
        }

        public Loop[] GetLoopsProduced()
        {
            if (loopsProduced is not null)
            {
                return loopsProduced;
            }

            loopsProduced = new Loop[nLoopsConsumed];
            
            foreach (Stitch stitch in stitches)
            {
                // cycle through all baseStitches for each stitch in row
                foreach (BaseStitch baseStitch in stitch.baseStitches)
                {
                    for (int i = 0; i  < baseStitch.loopsProduced.Length; i++)
                    {
                        loopsProduced[i] = baseStitch.loopsProduced[i];
                    }
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

        public GameObject GeneratePreview(Material material)
        {
            GameObject rowGameObject = new GameObject($"Row {this.rowIndex} for yarnWidth {this.yarnWidth}");
            foreach (Stitch stitch in this.stitches)
            {
                stitch.GenerateCurve();
                GameObject stitchGameObject = stitch.GenerateMesh(material);
                stitchGameObject.transform.SetParent(rowGameObject.transform);
            }

            return rowGameObject;
        }
    }
}