using System;
using System.Linq;
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

        public Row(int rowIndex, Row prevRow, StitchType[] stitchTypes, float yarnWidth)
        {
            this.yarnWidth = yarnWidth;
            this.rowIndex = rowIndex;
            SetPreviousRow(prevRow);
            Configure(stitchTypes);
        }

        public void Configure(StitchType[] stitchTypes)
        {
            // Create array of Stitch objects
            this.stitches = GetStitches(stitchTypes);
            // Calculate number of Stitch objects
            this.nStitches = this.stitches.Length;

            // Calculate number of BaseStitch objects for row
            // Also calculate the number of loops consumed and produced
            // for this row (calc by looping through the BaseStitch objects)
            this.nBaseStitches = 0;
            this.nLoopsConsumed = 0;
            this.nLoopsProduced = 0;
            foreach (Stitch stitch in stitches)
            {
                foreach (BaseStitch baseStitch in stitch.GetBaseStitches())
                {
                    this.nLoopsConsumed += baseStitch.BaseStitchInfo.loopsConsumed;
                    this.nLoopsProduced += baseStitch.BaseStitchInfo.loopsProduced;
                    this.nBaseStitches += 1;
                }
            }

        }
        public BaseStitch GetBaseStitch(int i)
        {
            return this.GetBaseStitches()[i];
        }

        public void SetPreviousRow(Row prevRowObj)
        {
            if (prevRowObj != null)
            {
                prevRow = prevRowObj;
                prevRowObj.nextRow = this;
            }
        }

        private Stitch[] GetStitches(StitchType[] stitchTypes)
        {
            Stitch[] stitches = new Stitch[stitchTypes.Length];

            int loopIndex = 0;
            for (int stitchIndex = 0; stitchIndex < stitchTypes.Length; stitchIndex++)
            {
                stitches[stitchIndex] = new Stitch(
                    stitchTypes[stitchIndex], rowIndex, stitchIndex, loopIndex, yarnWidth);
                loopIndex += stitches[stitchIndex].stitchInfo.loopsProduced;
            }

            return stitches;
        }
        
        public int GetLoopsConsumed()
        {
            int nConsumed = 0;
            foreach (Stitch stitch in stitches)
            {
                nConsumed += stitch.stitchInfo.loopsConsumed;
            }

            return nConsumed;
        }
        
        public int GetLoopsProduced()
        {
            int nProduced = 0;
            foreach (Stitch stitch in stitches)
            {
                nProduced += stitch.stitchInfo.loopsProduced;
            }

            return nProduced;
        }
        
        public BaseStitch[] GetBaseStitches()
        {
            BaseStitch[] loops = new BaseStitch[this.nBaseStitches];

            int loopIndex = 0;
            for (int i = 0; i < stitches.Length; i++)
            {
                Stitch stitch = stitches[i];
                for (int j = 0; j < stitch.stitchInfo.loopsProduced; j++)
                {
                    loops[loopIndex] = stitch.loops[j];
                    loopIndex += 1;
                }
            }
            return loops;
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