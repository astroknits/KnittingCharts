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
        // Number of stitches for this row
        public int nStitches;
        // Number of loops (ie basic stitches) in row
        public int nLoops;
        // Array of stitch objects
        public Stitch[] stitches;
        // Array of loop objects
        public Loop[] loops;

        public Row prevRow;
        public Row nextRow;

        public Row(int rowIndex, StitchType[] stitchTypes, float yarnWidth)
        {
            this.yarnWidth = yarnWidth;
            this.rowIndex = rowIndex;
            // Create array of Stitch objects
            this.stitches = GetStitches(stitchTypes);
            this.nStitches = stitchTypes.Length;
            this.loops = GetLoopsInRow();
            this.nLoops = this.loops.Length;
            this.prevRow = null;
            this.nextRow = null;
        }

        public Loop GetLoop(int i)
        {
            return this.loops[i];
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

        private Loop[] GetLoopsInRow()
        {
            int nLoops = 0;
            foreach (Stitch stitch in stitches)
            {
                nLoops += stitch.stitchInfo.loopsProduced;
            }

            Loop[] loops = new Loop[nLoops];

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