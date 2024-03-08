using System;
using UnityEngine;

namespace YarnGenerator
{
    public abstract class Pattern
    {
        // Array of row objects
        public Row[] rows;

        // nRows is the length of rows
        public int nRows;
        
        public abstract Row[] GetPatternDefinition();

        public void RenderPreview(Material material)
        {
            GameObject parent = new GameObject($"Pattern 1");
            for (int rowNumber = 0; rowNumber < this.nRows; rowNumber++)
            {
                Row row = this.rows[rowNumber];
                if (row is null)
                {
                    continue;
                }
                GameObject rowGameObject = row.GeneratePreview(material);
                rowGameObject.transform.SetParent(parent.transform);
            }
        }
        
        public void GetPatternRows()
        {
            this.rows = GetPatternDefinition();
            SetAdjacentRows();
            SetLoopsInAdjacentRows();
            PrintLoopsInAdjacentRows();
        }

        public void PrintLoopsInAdjacentRows()
        {
            foreach (Row row in this.rows)
            {
                if (row is null)
                {
                    continue;
                }
                foreach (Loop loop in row.loops)
                {
                    if (loop.consumes is not null)
                    {
                        foreach (Loop consumes in loop.consumes)
                        {
                            continue;
                        }
                    }

                    if (loop.produces is not null)
                    {
                        foreach (Loop produces in loop.produces)
                        {
                            if (produces is null)
                            {
                                continue;
                            }
                        }
                    }
                }
            }
        }

        public void SetAdjacentRows()
        {
            Row prevRow = null;
            foreach (Row row in rows)
            {
                if (row is null)
                {
                    continue;
                }

                if (prevRow is null)
                {
                    prevRow = row;
                }
                else
                {
                    row.SetPreviousRow(prevRow);
                    prevRow.SetNextRow(row);
                    prevRow = row;
                }
            }
        }
        
        public void SetLoopsInAdjacentRows()
        {
            foreach (Row row in rows)
            {
                if (row is null)
                {
                    continue;
                }
                Debug.Log($"ROW {row.rowIndex}");
                if (row.prevRow is null && row.nextRow is null)
                {
                    continue;
                }

                // Set the loops that are consumed by this stitch
                if (!(row.prevRow is null))
                {
                    int consumedIndex = 0;
                    for (int i = 0; i < row.nLoops; i++)
                    {
                        Loop loop = row.loops[i];
                        Debug.Log($"    Loop {loop.loopIndexStart} - {loop.loopInfo.loopsConsumed} ({loop.loopInfo.loopType})");
                        for (int j = 0; j < loop.loopInfo.loopsConsumed; j++)
                        {
                            Loop prevLoop = row.prevRow.GetLoop(consumedIndex);
                            loop.SetConsumes(j, prevLoop);
                            Debug.Log($" (i, j) = ({i}, {j}): for {loop.rowIndex} {loop.loopInfo.loopType} {loop.loopIndexStart}: setting consumes {prevLoop.rowIndex} {prevLoop.loopIndexStart}");
                            consumedIndex += 1;
                        }
                    }
                }
            }
        }
    }
    
    public class BasicPattern: Pattern
    {
        private float yarnWidth;

        public BasicPattern(float yarnWidth, int nRows)
        {
            this.yarnWidth = yarnWidth;
            this.nRows = nRows;
            GetPatternRows();
        }

        public override Row[] GetPatternDefinition()
        {
            // calculate # stitches per row
            int stitchesPerRow = 5;
            
            Row[] rows = new Row[nRows];
            for (int rowNumber = 0; rowNumber < nRows; rowNumber++)
            {
                if (stitchesPerRow <= 0)
                {
                    continue;
                }

                StitchType[] stitches = new StitchType[stitchesPerRow];

                for (int i = 0; i < stitchesPerRow - 1; i++)
                {
                    stitches[i] = StitchType.KnitStitch;
                }

                stitches[stitchesPerRow - 1] = StitchType.Knit2TogStitch;
                stitchesPerRow -= 1;
                
                rows[rowNumber] = new Row(rowNumber, stitches, yarnWidth);
            }

            return rows;
        }

    }

    public class CablePattern: Pattern
    {
        private float yarnWidth;
        private int padding;
        private int cableStitchesPerRow;
        private int cableBlockSize;
        private int cableSeparationSize;
        private int cableLength;

        public CablePattern(
            float yarnWidth,
            int nRows,
            int padding,
            int cableStitchesPerRow,
            int cableBlockSize,
            int cableSeparationSize,
            int cableLength
        )
        {
            this.yarnWidth = yarnWidth;
            this.nRows = nRows;
            this.padding = padding;
            this.cableStitchesPerRow = cableStitchesPerRow;
            this.cableBlockSize = cableBlockSize;
            this.cableSeparationSize = cableSeparationSize;
            this.cableLength = cableLength;
            GetPatternRows();
        }

        int GetTotalStitchesPerRow()
        {
            return cableStitchesPerRow + Math.Max((cableStitchesPerRow - 1) * cableSeparationSize, 0) + 2 * padding;
        }

        int GetActualLoopsPerRow()
        {
            return cableStitchesPerRow * cableBlockSize + Math.Max(cableStitchesPerRow - 1, 0) * cableSeparationSize + 2 * padding;
        }

        public override Row[] GetPatternDefinition()
        {
            // calculate # stitches per row
            int stitchesPerRow = GetTotalStitchesPerRow();

            // Set the number of loops per row to reflect the number of loops for this pattern
            int loopsPerRow = GetActualLoopsPerRow();

            Row[] rows = new Row[nRows];
            for (int rowNumber = 0; rowNumber < nRows; rowNumber++)
            {
                StitchType[] stitches = new StitchType[stitchesPerRow];

                int stitchIndex = 0;
                for (int i = 0; i < padding; i++)
                {
                    stitches[stitchIndex] = StitchType.PurlStitch;
                    stitchIndex += 1;
                }


                StitchType cableStitchType;
                StitchType nonCableStitchType;
                switch (cableBlockSize)
                {
                    case 2:
                        cableStitchType = StitchType.Cable1Lo1RStitch;
                        nonCableStitchType = StitchType.CableKnitStitch;
                        break;
                    case 4:
                        cableStitchType = StitchType.Cable2Lo2RStitch;
                        nonCableStitchType = StitchType.CableKnitStitch4;
                        break;
                    default:
                        cableStitchType = StitchType.Cable2Lo2RStitch;
                        nonCableStitchType = StitchType.CableKnitStitch;
                        break;
                }
                for (int i = 0; i < cableStitchesPerRow; i++)
                {
                    if (rowNumber % cableLength == 0)
                    {
                        stitches[stitchIndex] = cableStitchType;
                    }
                    else
                    {
                        stitches[stitchIndex] = nonCableStitchType;
                    }

                    stitchIndex += 1;
                    if (cableStitchesPerRow > 1 && i < cableStitchesPerRow - 1)
                    {
                        for (int j = 0; j < cableSeparationSize; j++)
                        {
                            stitches[stitchIndex] = StitchType.PurlStitch;
                            stitchIndex += 1;
                        }
                    }
                }

                for (int i = 0; i < padding; i++)
                {
                    stitches[stitchIndex] = StitchType.PurlStitch;
                    stitchIndex += 1;
                }

                rows[rowNumber] = new Row(rowNumber, stitches, yarnWidth);
            }

            return rows;
        }

    }
}