using UnityEngine;

namespace YarnGenerator
{
    public class Pattern
    {
        // Array of row objects
        public Row[] rows;
        // nRows is the length of rows
        public int nRows;
        // Width is the number of loops in the row with most loops
        public int width;

        public Pattern(Row[] rows)
        {
            this.rows = rows;
            this.nRows = rows.Length;
            this.width = 0;
            foreach (Row row in rows)
            {
                if (row.nLoops > this.width)
                {
                    this.width = row.nLoops;
                }
            }
        }

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
}