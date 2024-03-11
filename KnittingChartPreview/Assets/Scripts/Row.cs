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

        public Row prevRow;
        public Row nextRow;

        public Row(int rowIndex, StitchType[] stitchTypes, float yarnWidth)
        {
            this.yarnWidth = yarnWidth;
            this.rowIndex = rowIndex;
            // Create array of Stitch objects
            this.stitches = GetStitches(stitchTypes);
            this.nStitches = this.stitches.Length;
            this.nBaseStitches = GetNBaseStitches();
            this.prevRow = null;
            this.nextRow = null;
        }

        public BaseStitch GetBaseStitch(int i)
        {
            return this.GetBaseStitches()[i];
        }

        public void SetPreviousRow(Row prevRowObj)
        {
            prevRow = prevRowObj;
        }

        public void SetNextRow(Row nextRowObj)
        {
            nextRow = nextRowObj;
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

        public int GetNBaseStitches()
        {
            int nBaseStitches = 0;
            foreach (Stitch stitch in stitches)
            {
                nBaseStitches += stitch.stitchInfo.nBaseStitches;
            }

            return nBaseStitches;
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