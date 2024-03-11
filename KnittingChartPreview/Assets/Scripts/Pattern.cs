using System;
using UnityEngine;

namespace YarnGenerator
{
    public abstract class Pattern
    {
        public float yarnWidth;

        // Array of row objects
        public Row[] rows;

        // nRows is the length of rows
        public int nRows;

        public Pattern(float yarnWidth, int nRows)
        { 
            this.yarnWidth = yarnWidth;
            this.nRows = nRows;
        }

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
                foreach (BaseStitch baseStitch in row.GetBaseStitches())
                {
                    if (baseStitch.consumes is not null)
                    {
                        foreach (BaseStitch consumes in baseStitch.consumes)
                        {
                            continue;
                        }
                    }

                    if (baseStitch.produces is not null)
                    {
                        foreach (BaseStitch produces in baseStitch.produces)
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
                if (row.prevRow is null)
                {
                    continue;
                }

                // Set the baseStitches that are consumed by this stitch
                int consumedIndex = 0;
                for (int i = 0; i < row.GetBaseStitches().Length; i++)
                {
                    BaseStitch baseStitch = row.GetBaseStitch(i);
                    for (int j = 0; j < baseStitch.BaseStitchInfo.loopsConsumed; j++)
                    {
                        BaseStitch prevBaseStitch = row.prevRow.GetBaseStitch(consumedIndex);
                        baseStitch.SetConsumes(j, prevBaseStitch);
                        consumedIndex += 1;
                    }
                }
            }
        }
    }
    
    public class BasicPattern: Pattern
    {
        private int stitchesPerRowStart;

        public BasicPattern(
            float yarnWidth,
            int nRows,
            int stitchesPerRowStart
            ): 
            base(yarnWidth, nRows)
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

                rows[rowNumber] = new Row(rowNumber, stitches, yarnWidth);
            }

            return rows;
        }

        public override Row[] GetPatternDefinition()
        {
            int stitchesPerRow = stitchesPerRowStart;

            nRows = 14;
            Row[] rows = new Row[nRows];
            int rowNumber = 0;
            stitchesPerRow = 8;
            StitchType[] stitches = new StitchType[8] {
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                };
            rows[rowNumber] = new Row(rowNumber, stitches, yarnWidth);
            rowNumber = 1;
            stitchesPerRow = 7;
            stitches = new StitchType[7] {
                StitchType.KnitStitch,
                // StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.Knit2TogStitch,
                StitchType.KnitStitch,
            };
            rows[rowNumber] = new Row(rowNumber, stitches, yarnWidth);
            rowNumber = 2;
            stitchesPerRow = 7;
            stitches = new StitchType[] {
                StitchType.KnitStitch,
                // StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
            };
            rows[rowNumber] = new Row(rowNumber, stitches, yarnWidth);
            rowNumber = 2;
            stitchesPerRow = 6;
            stitches = new StitchType[] {
                StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.Knit2TogStitch,
                StitchType.KnitStitch,
            };
            rows[rowNumber] = new Row(rowNumber, stitches, yarnWidth);
            rowNumber = 3;
            stitchesPerRow = 6;
            stitches = new StitchType[] {
                StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
            };
            rows[rowNumber] = new Row(rowNumber, stitches, yarnWidth);
            rowNumber = 4;
            stitchesPerRow = 5;
            stitches = new StitchType[] {
                StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.Knit2TogStitch,
                StitchType.KnitStitch,
            };
            rows[rowNumber] = new Row(rowNumber, stitches, yarnWidth);
            rowNumber = 5;
            stitchesPerRow = 5;
            stitches = new StitchType[] {
                StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
            };
            rows[rowNumber] = new Row(rowNumber, stitches, yarnWidth);
            rowNumber = 6;
            stitchesPerRow = 4;
            stitches = new StitchType[] {
                StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.Knit2TogStitch,
                StitchType.KnitStitch,
            };
            rows[rowNumber] = new Row(rowNumber, stitches, yarnWidth);
            rowNumber = 7;
            stitchesPerRow = 4;
            stitches = new StitchType[] {
                StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
            };
            rows[rowNumber] = new Row(rowNumber, stitches, yarnWidth);
            rowNumber = 8;
            stitchesPerRow = 3;
            stitches = new StitchType[] {
                StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                StitchType.Knit2TogStitch,
                StitchType.KnitStitch,
            };
            rows[rowNumber] = new Row(rowNumber, stitches, yarnWidth);
            rowNumber = 9;
            stitchesPerRow = 3;
            stitches = new StitchType[] {
                StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
            };
            rows[rowNumber] = new Row(rowNumber, stitches, yarnWidth);
            rowNumber = 10;
            stitchesPerRow = 2;
            stitches = new StitchType[] {
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                StitchType.Knit2TogStitch,
                StitchType.KnitStitch,
            };
            rows[rowNumber] = new Row(rowNumber, stitches, yarnWidth);
            rowNumber = 11;
            stitchesPerRow = 2;
            stitches = new StitchType[] {
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.KnitStitch,
            };
            rows[rowNumber] = new Row(rowNumber, stitches, yarnWidth);
            rowNumber = 12;
            stitchesPerRow = 3;
            stitches = new StitchType[] {
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                // StitchType.KnitStitch,
                StitchType.KnitStitch,
                StitchType.YarnOverStitch,
                StitchType.KnitStitch,
            };
            rows[rowNumber] = new Row(rowNumber, stitches, yarnWidth);
            rowNumber = 13;
            stitchesPerRow = 3;
            stitches = new StitchType[] {
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
            rows[rowNumber] = new Row(rowNumber, stitches, yarnWidth);
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
            float yarnWidth,
            int nRows,
            int padding,
            int cableStitchesPerRow,
            int cableBlockSize,
            int cableSeparationSize,
            int cableLength
        ): base(yarnWidth, nRows)
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