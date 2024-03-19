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
            // Also calculate the number of baseStitches consumed and produced
            // for this row (calc by looping through the BaseStitch objects)
            this.nBaseStitches = 0;
            this.nLoopsConsumed = 0;
            this.nLoopsProduced = 0;
            foreach (Stitch stitch in stitches)
            {
                foreach (BaseStitch baseStitch in stitch.GetBaseStitches())
                {
                    this.nLoopsConsumed += baseStitch.baseStitchInfo.nLoopsConsumed;
                    this.nLoopsProduced += baseStitch.baseStitchInfo.nLoopsProduced;
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

            Loop[] prevRowLoops = GetLoopsProducedByPrevRow();
            int loopIndex = 0;
            for (int stitchIndex = 0; stitchIndex < stitchTypes.Length; stitchIndex++)
            {
                StitchType stitchType = stitchTypes[stitchIndex];
                Debug.Log($"Row {rowIndex} in GetStitches: stitch {stitchIndex}/{stitchType}");
                StitchInfo stitchInfo = StitchInfo.GetStitchInfo(stitchType);
                Loop[] loopsConsumed = null;
                if (stitchInfo.loopsConsumed > 0)
                {
                    int endStitchLoopIndex = loopIndex + stitchInfo.loopsConsumed - 1;
                    Debug.Log($"       Row {rowIndex} stitchInfo.loopsConsumed: {stitchInfo.loopsConsumed}, endStitchLoopIndex = {loopIndex} + {stitchInfo.loopsConsumed} - 1 = {endStitchLoopIndex}");
                    loopsConsumed = GetLoopsProducedByPrevRow(loopIndex, endStitchLoopIndex);
                    foreach (Loop loop in loopsConsumed)
                    {
                        Debug.Log($"             rowIndex {loop.rowIndex} {loop.loopIndex}");
                    }
                }
                stitches[stitchIndex] = new Stitch(
                    stitchInfo,
                    rowIndex,
                    stitchIndex,
                    loopIndex,
                    yarnWidth,
                    loopsConsumed
                    );
                loopIndex += stitchInfo.loopsConsumed;
            }

            return stitches;
        }

        public Loop[] GetLoopsProducedByPrevRow(int startLoop, int endLoop)
        {
            Debug.Log($"GetLoopsProducedByPrevRow. {startLoop} {endLoop}");
            if (prevRow is null)
            {
                Debug.Log($"    Is null.");
                return new Loop[0];
            }
            
            Debug.Log($"    rowIndex {rowIndex} loopsConsumed: Loop[{endLoop} - {startLoop}]");
            Loop[] loopsConsumed = new Loop[endLoop - startLoop + 1];
            
            int loopIndex = 0;
            // cycle through all stitches from previous row
            Debug.Log($"prevRow.stitches.Length: {prevRow.stitches.Length}");
            foreach (Stitch stitch in prevRow.stitches)
            {
                Debug.Log($"         stitch.baseStitches.Length: {stitch.baseStitches.Length}");

                // cycle through all baseStitches for each stitch in previous row
                foreach (BaseStitch baseStitch in stitch.baseStitches)
                {
                    Debug.Log($"             baseStitch.loopsProduced: {baseStitch.loopsProduced}");
                    for (int i = 0; i  < baseStitch.loopsProduced.Length; i++)
                    {
                        Loop loop = baseStitch.loopsProduced[i];
                        if (loopIndex >= startLoop && loopIndex >= endLoop)
                        {
                            Debug.Log($"              {i}: {loopIndex} {startLoop} {endLoop}");
                            loopsConsumed[i] = loop;
                        }
                        loopIndex += 1;
                    }
                }
            }

            Debug.Log($"loopsConsumed.Length: {loopsConsumed.Length}");
            return loopsConsumed;
        }
        
        public Loop[] GetLoopsProducedByPrevRow()
        {
            return GetLoopsProducedByPrevRow(0, this.nLoopsConsumed);
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
                    loops[loopIndex] = stitch.baseStitches[j];
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