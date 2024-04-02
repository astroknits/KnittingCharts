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

        public Pattern(int nRows)
        { 
            this.nRows = nRows;
        }

        public abstract Row[] GetPatternDefinition();

        public void RenderPreview(float yarnWidth, Material material)
        {
            GameObject parent = new GameObject($"Pattern 1");
            for (int rowNumber = 0; rowNumber < this.nRows; rowNumber++)
            {
                Row row = this.rows[rowNumber];
                if (row is null)
                {
                    continue;
                }
                GameObject rowGameObject = row.GeneratePreview(yarnWidth, material);
                rowGameObject.transform.SetParent(parent.transform);
            }
        }
        
        public void GetPatternRows()
        {
            this.rows = GetPatternDefinition();
            SetAdjacentRows();
            // PrintLoopsInAdjacentRows();
            // UpdateLoops();
        }

        public void PrintLoopsInAdjacentRows()
        {
            foreach (Row row in this.rows)
            {
                if (row is null)
                {
                    continue;
                }
                foreach (BaseStitch baseStitch in row.GetBaseStitches())
                {
                    if (baseStitch.loopsConsumed is not null && baseStitch.loopsConsumed.Length > 0)
                    {
                        foreach (Loop consumed in baseStitch.loopsConsumed)
                        {
                            if (consumed is not null)
                            {
                                Debug.Log($"Row {row.rowIndex} {baseStitch.stitchIndex} {baseStitch.baseStitchInfo.BaseStitchType} {baseStitch.baseStitchIndex} consumed {consumed.rowIndex} {consumed.loopIndex}");
                            }
                            else
                            {
                                Debug.Log($"Row {row.rowIndex} {baseStitch.stitchIndex} {baseStitch.baseStitchInfo.BaseStitchType} {baseStitch.baseStitchIndex} consumed NULL");
                            }
                        }
                    } else
                    {
                        Debug.Log($"Row {row.rowIndex} {baseStitch.stitchIndex} {baseStitch.baseStitchInfo.BaseStitchType} {baseStitch.baseStitchIndex} consumed NULL TOTAL");
                    }

                    if (baseStitch.loopsProduced is not null && baseStitch.loopsProduced.Length > 0)
                    {
                        foreach (Loop produced in baseStitch.loopsProduced)
                        {
                            if (produced is not null)
                            {
                                Debug.Log($"Row {row.rowIndex} {baseStitch.stitchIndex} {baseStitch.baseStitchInfo.BaseStitchType} {baseStitch.baseStitchIndex} produced {produced.rowIndex} {produced.loopIndex}");
                            }
                            else
                            {
                                Debug.Log($"Row {row.rowIndex} {baseStitch.stitchIndex} {baseStitch.baseStitchInfo.BaseStitchType} {baseStitch.baseStitchIndex} produced NULL");
                            }
                        }
                    } else 
                    {
                        Debug.Log($"Row {row.rowIndex} {baseStitch.stitchIndex} {baseStitch.baseStitchInfo.BaseStitchType} {baseStitch.baseStitchIndex} produced NULL TOTAL");
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

                if (prevRow is not null)
                {
                    row.SetPreviousRow(prevRow);
                }
                prevRow = row;
            }
        }
    }
    
    public class BasicPattern: Pattern
    {
        private int stitchesPerRowStart;

        public BasicPattern(
            int nRows,
            int stitchesPerRowStart
            ): 
            base(nRows)
        {
            this.stitchesPerRowStart = stitchesPerRowStart;
            GetPatternRows();
        }

        public bool DoesRowIncrease(int rowNumber)
        {
            // return false;
            return (rowNumber == 4);
        }

        public bool DoesRowDecrease(int rowNumber)
        {
            return !(rowNumber == 0 || rowNumber == 3);
        }

        public Row[] GetPatternDefinitionOrig()
        {
            int stitchesPerRow = stitchesPerRowStart;

            Row prevRow = null;
            Row[] rows = new Row[nRows];
            for (int rowNumber = 0; rowNumber < nRows - 1; rowNumber++)
            {
                if (stitchesPerRow <= 2)
                {
                    continue;
                }
                
                if (DoesRowDecrease(rowNumber))
                {
                    stitchesPerRow -= 1;
                }
                
                if (DoesRowIncrease(rowNumber))
                {
                    stitchesPerRow += 1;
                }

                StitchType[] stitches = new StitchType[stitchesPerRow];
                
                for (int i = 0; i < stitchesPerRow - 1 - 2; i++)
                {
                    stitches[i] = StitchType.KnitStitch;
                }

                if (rowNumber == 3)
                {
                    stitches[stitchesPerRow - 1 - 1] = StitchType.KnitStitch;
                    stitches[stitchesPerRow - 1 - 0] = StitchType.KnitStitch;
                }
                else if (rowNumber == 4)
                {
                    stitches[stitchesPerRow - 1 - 1] = StitchType.YarnOverStitch;
                    stitches[stitchesPerRow - 1 - 0] = StitchType.PurlStitch;
                }
                else if (rowNumber == 0)
                {
                    stitches[stitchesPerRow - 1 - 1] = StitchType.KnitStitch;
                    stitches[stitchesPerRow - 1 - 0] = StitchType.KnitStitch;
                }
                else
                {
                    stitches[stitchesPerRow - 1 - 1] = StitchType.Knit2TogStitch;
                    stitches[stitchesPerRow - 1 - 0] = StitchType.KnitStitch;
                }

                Row row = new Row(rowNumber, prevRow, stitches);
                rows[rowNumber] = row;
                prevRow = row;
            }

            return rows;
        }

        public override Row[] GetPatternDefinition()
        {
            int stitchesPerRow = stitchesPerRowStart;

            Row prevRow = null;
            nRows = 17;
            Row[] rows = new Row[nRows];
            int rowNumber = 0;
            StitchType[] stitches = new StitchType[] {
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                };
            Row row = new Row(rowNumber, prevRow, stitches);
            prevRow = row;
            rows[rowNumber] = row;
            rowNumber = 1;
            stitches = new StitchType[] {
                StitchType.NoStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.SSKStitch,
                StitchType.KnitStitch,
                // StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.Knit2TogStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
            };
            row = new Row(rowNumber, prevRow, stitches);
            prevRow = row;
            rows[rowNumber] = row;            
            rowNumber = 2;
            stitches = new StitchType[] {
                StitchType.NoStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                // StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
            };
            row = new Row(rowNumber, prevRow, stitches);
            prevRow = row;
            rows[rowNumber] = row;
            rowNumber = 3;
            stitches = new StitchType[] {
                StitchType.NoStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.Knit2TogStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
            };
            row = new Row(rowNumber, prevRow, stitches);
            prevRow = row;
            rows[rowNumber] = row;
            rowNumber = 4;
            stitches = new StitchType[] {
                StitchType.NoStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
            };
            row = new Row(rowNumber, prevRow, stitches);
            prevRow = row;
            rows[rowNumber] = row;
            rowNumber = 5;
            stitches = new StitchType[] {
                StitchType.NoStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.Knit2TogStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
            };
            row = new Row(rowNumber, prevRow, stitches);
            prevRow = row;
            rows[rowNumber] = row;
            rowNumber = 6;
            stitches = new StitchType[] {
                StitchType.NoStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
            };
            row = new Row(rowNumber, prevRow, stitches);
            prevRow = row;
            rows[rowNumber] = row;
            rowNumber = 7;
            stitches = new StitchType[] {
                StitchType.NoStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.Knit2TogStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
            };
            row = new Row(rowNumber, prevRow, stitches);
            prevRow = row;
            rows[rowNumber] = row;
            rowNumber = 8;
            stitches = new StitchType[] {
                StitchType.NoStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
            };
            row = new Row(rowNumber, prevRow, stitches);
            prevRow = row;
            rows[rowNumber] = row;
            rowNumber = 9;
            stitches = new StitchType[] {
                StitchType.NoStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                StitchType.Knit2TogStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
            };
            row = new Row(rowNumber, prevRow, stitches);
            prevRow = row;
            rows[rowNumber] = row;
            rowNumber = 10;
            stitches = new StitchType[] {
                StitchType.NoStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
            };
            row = new Row(rowNumber, prevRow, stitches);
            prevRow = row;
            rows[rowNumber] = row;
            rowNumber = 11;
            stitches = new StitchType[] {
                StitchType.NoStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                StitchType.Knit2TogStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
            };
            row = new Row(rowNumber, prevRow, stitches);
            prevRow = row;
            rows[rowNumber] = row;
            rowNumber = 12;
            stitches = new StitchType[] {
                StitchType.NoStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
            };
            row = new Row(rowNumber, prevRow, stitches);
            prevRow = row;
            rows[rowNumber] = row;
            rowNumber = 13;
            stitches = new StitchType[] {
                StitchType.NoStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                StitchType.M1Stitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
            };
            row = new Row(rowNumber, prevRow, stitches);
            prevRow = row;
            rows[rowNumber] = row;
            rowNumber = 14;
            stitches = new StitchType[] {
                StitchType.NoStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
            };
            row = new Row(rowNumber, prevRow, stitches);
            prevRow = row;
            rows[rowNumber] = row;
            rowNumber = 15;
            stitches = new StitchType[] {
                StitchType.NoStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.YarnOverStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
            };
            row = new Row(rowNumber, prevRow, stitches);
            prevRow = row;
            rows[rowNumber] = row;
            rowNumber = 16;
            stitches = new StitchType[] {
                StitchType.NoStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
            };
            row = new Row(rowNumber, prevRow, stitches);
            prevRow = row;
            rows[rowNumber] = row;
            return rows;
        }
    }

    public class CablePattern: Pattern
    {
        private int padding;
        private int cableStitchesPerRow;
        private int cableBlockSize;
        private int cableSeparationSize;
        private int cableLength;

        public CablePattern(
            int nRows,
            int padding,
            int cableStitchesPerRow,
            int cableBlockSize,
            int cableSeparationSize,
            int cableLength
        ): base(nRows)
        {
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

            // Set the number of baseStitches per row to reflect the number of baseStitches for this pattern
            int loopsPerRow = GetActualLoopsPerRow();

            Row prevRow = null;
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

                Row row = new Row(rowNumber, prevRow, stitches);
                prevRow = row;
                rows[rowNumber] = row;
            }

            return rows;
        }

    }
}