namespace YarnGenerator
{
    public class Row
    {
        // Index for this row
        public int nRow;
        // Number of stitches for this row
        public int nStitches;
        // Number of loops (ie basic stitches) in row
        public int nLoops;
        // Array of stitch objects
        public Stitch[] stitches;

        public Row(int nRow, StitchType[] stitchTypes)
        {
            this.nRow = nRow;
            // Create array of Stitch objects
            this.stitches = GetStitches(stitchTypes);
            this.nStitches = stitchTypes.Length;
            this.nLoops = GetLoopsInRow(this.stitches);
        }

        private static Stitch[] GetStitches(StitchType[] stitchTypes)
        {
            Stitch[] stitches = new Stitch[stitchTypes.Length];

            for (int j = 0; j < stitchTypes.Length; j++)
            {
                stitches[j] = Stitch.GetStitch(stitchTypes[j], j);
            }

            return stitches;
        }

        private static int GetLoopsInRow(Stitch[] stitches)
        {
            int nLoops = 0;
            foreach (Stitch stitch in stitches)
            {
                nLoops += stitch.loopsProduced;
            }

            return nLoops;
        }
    }
}