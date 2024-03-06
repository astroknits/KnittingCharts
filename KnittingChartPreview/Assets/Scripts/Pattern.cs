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
        
        public abstract Row[] GetPatternRows();

        public void RenderPreview(Material material)
        {
            GameObject parent = new GameObject($"Pattern 1");
            for (int rowNumber = 0; rowNumber < this.nRows; rowNumber++)
            {
                Row row = this.rows[rowNumber];
                GameObject rowGameObject = row.GeneratePreview(material);
                rowGameObject.transform.SetParent(parent.transform);
            }
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
            this.rows = GetPatternRows();
        }

        int GetTotalStitchesPerRow()
        {
            return cableStitchesPerRow + Math.Max((cableStitchesPerRow - 1) * cableSeparationSize, 0) + 2 * padding;
        }

        int GetActualLoopsPerRow()
        {
            return cableStitchesPerRow * cableBlockSize + Math.Max(cableStitchesPerRow - 1, 0) * cableSeparationSize + 2 * padding;
        }

        public override Row[] GetPatternRows()
        {
            Row[] rows = GetCablePatternRows();
            UpdateAdjacentRows(rows);
            UpdateAdjacentRowStitches(rows);
            return rows;
        }

        public void UpdateAdjacentRows(Row[] rows)
        {
            Row prevRow = null;
            foreach (Row row in rows)
            {
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
        
        public void UpdateAdjacentRowStitches(Row[] rows)
        {
            for (int i = 0; i < rows.Length; i ++)
            {
                Row row = rows[i];
                if (row.prevRow is null && row.nextRow is null)
                {
                    Debug.Log($"ROW.rowIndex: {row.rowIndex}; row.prevRow is NULL; row.nextRow is NULL");
                    continue;
                }
                else if (row.prevRow is null)
                {
                    Debug.Log($"ROW.rowIndex: {row.rowIndex}; row.prevRow is NULL; row.nextRow.rowIndex: {row.nextRow.rowIndex}");
                    var test = row.nextRow;
                }
                else if (row.nextRow is null)
                {
                    Debug.Log($"ROW.rowIndex: {row.rowIndex}; row.prevRow.rowIndex: {row.prevRow.rowIndex}; row.nextRow is NULL");
                }
                else
                {
                    Debug.Log($"ROW.rowIndex: {row.rowIndex}; row.prevRow.rowIndex: {row.prevRow.rowIndex}; row.nextRow.rowIndex: {row.nextRow.rowIndex}");
                }
            }
        }

        public Row[] GetCablePatternRows()
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