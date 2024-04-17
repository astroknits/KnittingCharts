namespace YarnGenerator
{
    public abstract class StitchGridEdge
    {
        internal StitchGridNode producedBy;
        internal StitchGridNode consumedBy;
        
        public StitchGridEdge()
        {
            this.producedBy = null;
            this.consumedBy = null;
        }

        internal void SetConsumedBy(StitchGridNode consumedBy)
        {
            this.consumedBy = consumedBy;
        }

        internal void SetProducedBy(StitchGridNode producedBy)
        {
            this.producedBy = producedBy;
        }
    }

    public class StitchGridWaleEdge: StitchGridEdge
    {
        public StitchGridWaleEdge(): base()
        { }
    }

    public class StitchGridCourseEdge: StitchGridEdge
    {
        public StitchGridCourseEdge(): base()
        { }
    }
}