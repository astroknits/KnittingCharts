using System;
using System.Linq;
using UnityEngine;

namespace YarnGenerator
{
    public class Row
    {
        // Index for this row
        public int rowIndex;
        // Number of stitches for this row
        public int nStitches;
        // Number of loops (ie basic stitches) in row
        public int nLoops;
        // Array of stitch objects
        public Stitch[] stitches;
        public float yarnWidth;

        public Row(int rowIndex, StitchType[] stitchTypes, float yarnWidth)
        {
            this.rowIndex = rowIndex;
            this.yarnWidth = yarnWidth;
            // Create array of Stitch objects
            this.stitches = GetStitches(stitchTypes);
            this.nStitches = stitchTypes.Length;
            this.nLoops = GetLoopsInRow();
        }

        private Stitch[] GetStitches(StitchType[] stitchTypes)
        {
            Stitch[] stitches = new Stitch[stitchTypes.Length];

            int loopIndex = 0;
            for (int stitchIndex = 0; stitchIndex < stitchTypes.Length; stitchIndex++)
            {
                stitches[stitchIndex] = Stitch.GetStitch(
                    stitchTypes[stitchIndex], rowIndex, stitchIndex, loopIndex, yarnWidth);
                loopIndex += stitches[stitchIndex].loopsProduced;
            }

            return stitches;
        }

        private int GetLoopsInRow()
        {
            int nLoops = 0;
            foreach (Stitch stitch in stitches)
            {
                nLoops += stitch.loopsProduced;
            }

            return nLoops;
        }

        public GameObject GeneratePreview(Material material)
        {
            GameObject rowGameObject = new GameObject($"Row {this.rowIndex} for yarnWidth {this.yarnWidth}");
            foreach (Stitch stitch in this.stitches)
            {
                GameObject stitchGameObject = stitch.GenerateMesh(material);
                stitchGameObject.transform.SetParent(rowGameObject.transform);
            }

            return rowGameObject;
        }
    }
}