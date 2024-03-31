
using UnityEngine;

namespace YarnGenerator
{
    public class Loop
    {
        // row number for the given stitch
        public int rowIndex;
        // Loop # in the given row
        public int loopIndex;

        public int indexOffset;

        // Loop location offset
        public Vector3 offset;

        // keep track of adjacent loops in same row,
        // to be able to traverse the row to
        // apply offsets due to increases/decreases
        public Loop prevLoop;
        public Loop nextLoop;

        public BaseStitch consumbedBy;
        public BaseStitch producedBy;

        public Loop(int rowIndex, int loopIndex, Loop prevLoop, BaseStitch producedBy)
        {
            this.rowIndex = rowIndex;
            this.loopIndex = loopIndex;

            // Initialize offset as zero
            this.offset = Vector3.zero;

            this.indexOffset = 0;

            this.prevLoop = prevLoop;
            this.nextLoop = null;
            if (this.prevLoop is not null)
            {
                this.prevLoop.nextLoop = this;
            }

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
            return (float)loopIndex + (float)indexOffset + offset.x;
        }

        public void AddIndexOffset(int offset, ShiftDirection shiftDirection)
        {
            indexOffset += offset;
            if (shiftDirection == ShiftDirection.Left)
            {
                Loop loop = this.prevLoop;
                while (loop is not null)
                {
                    loop.AddIndexOffset(offset, shiftDirection);
                    loop = loop.prevLoop;
                }
            } else if (shiftDirection == ShiftDirection.Right)
            {
                Loop loop = this.nextLoop;
                while (loop is not null)
                {
                    loop.AddIndexOffset(offset, shiftDirection);
                    loop = loop.nextLoop;
                }
            }
        }

        public void AddOffset(Vector3 offsetToAdd)
        {
            offset += offsetToAdd;
        }
        
        public void AddXOffset(float xOffsetToAdd, ShiftDirection shiftDirection)
        {
            offset.x += xOffsetToAdd;
            if (shiftDirection == ShiftDirection.Left)
            {
                Loop loop = this.prevLoop;
                while (loop is not null)
                {
                    loop.AddXOffset(xOffsetToAdd, shiftDirection);
                    loop = loop.prevLoop;
                }
            } else if (shiftDirection == ShiftDirection.Right)
            {
                Loop loop = this.nextLoop;
                while (loop is not null)
                {
                    loop.AddXOffset(xOffsetToAdd, shiftDirection);
                    loop = loop.nextLoop;
                }
            }
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