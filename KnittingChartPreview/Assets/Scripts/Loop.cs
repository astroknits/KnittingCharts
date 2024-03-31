
using UnityEngine;

namespace YarnGenerator
{
    public class Loop
    {
        // row number for the given stitch
        public int rowIndex;
        // Loop # in the given row
        public int loopIndex;
        
        // Loop location offset
        public Vector3 offset;

        public BaseStitch consumbedBy;
        public BaseStitch producedBy;

        public Loop(int rowIndex, int loopIndex, BaseStitch producedBy)
        {
            this.rowIndex = rowIndex;
            this.loopIndex = loopIndex;

            // Initialize offset as zero
            this.offset = Vector3.zero;
            
            // Set producedBy BaseStitches
            this.producedBy = producedBy;
            // Set consumedBy BaseStitches as null.
            // This will get updated only if there is a
            // next row of stitches defined (otherwise stays null).
            this.consumbedBy = null;
        }

        public void SetConsumedBy(BaseStitch baseStitch)
        {
            this.consumbedBy = baseStitch;
        }

        public float GetIndex()
        {
            return (float)loopIndex + offset.x;
        }

        public void AddOffset(Vector3 offsetToAdd)
        {
            offset += offsetToAdd;
        }
        
        public void AddXOffset(float xOffsetToAdd)
        {
            offset.x += xOffsetToAdd;
        }
        
        public void AddYOffset(float yOffsetToAdd)
        {
            offset.y += yOffsetToAdd;
        }
        
        public void AddZOffset(float zOffsetToAdd)
        {
            offset.z += zOffsetToAdd;
        }
        
    }
}