using System;
using System.Linq;
using UnityEngine;


namespace YarnGenerator

{
    public abstract class Stitch
    {
        // List of stitches to perform in order.  
        public StitchType stitchType;
        // width of the yarn
        public float yarnWidth;
        // row number for the given stitch
        public int rowIndex;
        // stitch number for the given stitch
        public int stitchIndex;
        // starting loop number for the given stitch
        public int loopIndex;

        // # of loops from previous row used by this stitch
        public int loopsConsumed;
        // # of loops left on the needle at the end of this stitch
        public int loopsProduced;
        // Loop types for this stitch (knit, purl)
        public LoopType[] loopTypeList;
        // list of loop objects
        public Loop[] loops;

        // The cable stitch is defined as any composite stitch
        // where the first X loops from the needle are picked up
        // and placed either in front of (front=true) or behind (front=false)
        // the needle, and the rest of the stitches are
        // knitted according to the loopTypeList before placing
        // the held stitches back on the needle and knitting those
        
        // held parameter indicates how many loops to hold on a stitch holder
        // before starting to knit
        public int held;
        // Indicates whether to place the first $held stitches
        // in front of (true) or behind (false) the needle
        public bool front;
        // List of stitches to perform in order.  Stitches are performed in
        // this order once the held stitches have been moved to the stitch holder

        public Stitch(int rowIndex, int stitchIndex, int loopIndex, float yarnWidth)
        {
            this.rowIndex = rowIndex;
            this.stitchIndex = stitchIndex;
            this.loopIndex = loopIndex;
            this.yarnWidth = yarnWidth;
            // loops get defined in subclass
            // this.loops = GetLoops();
        }
        
        public static Stitch GetStitch(StitchType stitchType, int rowIndex, int stitchIndex, int loopIndex, float yarnWidth)
        {
            switch (stitchType)
            {
                case StitchType.KnitStitch:
                    return new KnitStitch(rowIndex, stitchIndex, loopIndex, yarnWidth);
                case StitchType.PurlStitch:
                    return new PurlStitch(rowIndex, stitchIndex, loopIndex, yarnWidth);
                case StitchType.Cable1Lo1RStitch:
                    return new Cable1Lo1RStitch(rowIndex, stitchIndex, loopIndex, yarnWidth);
                case StitchType.Cable2Lo2RStitch:
                    return new Cable2Lo2RStitch(rowIndex, stitchIndex, loopIndex, yarnWidth);
                case StitchType.CableKnitStitch:
                    return new CableKnitStitch(rowIndex, stitchIndex, loopIndex, yarnWidth);
                case StitchType.CableKnitStitch4:
                    return new CableKnitStitch4(rowIndex, stitchIndex, loopIndex, yarnWidth);
                default:
                    return new KnitStitch(rowIndex, stitchIndex, loopIndex, yarnWidth);
            }
        }

        public Loop[] GetLoops()
        {
            loops = new Loop[this.loopsProduced];
            // Work through each of the loops produced in the loopTypeList
            for (int i = 0; i < this.loopsProduced; i++)
            {
                int loopIndexStart = this.loopIndex + i;
                int loopIndexEnd = loopIndexStart;
                bool heldInFront = false;
                bool heldBehind = false;
                if (this.held == 0)
                {
                    loopIndexEnd = loopIndexStart;
                }
                else
                {
                    // hold the first this.held loops in front of/behind the
                    // needle and first knit the remaining
                    // this.loopsProduced - this.held loops
                    if (i >= this.held)
                    {
                        loopIndexEnd = loopIndexStart - this.held;
                        heldInFront = this.front;
                        heldBehind = !this.front;
                    }
                    else
                    {
                        loopIndexEnd = loopIndexStart + this.held;
                        heldInFront = !this.front;
                        heldBehind = this.front;
                    }
                }
                Debug.Log($"loopIndexStart {loopIndexStart} front {front} heldInFront {heldInFront} heldbehind {heldBehind}");

                loops[i] = Loop.GetLoop(
                    loopTypeList[i], 
                    yarnWidth, 
                    rowIndex, 
                    loopIndexStart, 
                    loopIndexEnd,
                    heldInFront,
                    heldBehind);
            }

            return loops;
        }

        public void GenerateCurve()
        {
            // Work through each of the loops produced in the loopTypeList
            for (int i = 0; i < this.loops.Length; i++)
            {
                loops[i].GenerateCurve();
            }
        }

        public GameObject GenerateMesh(Material material)
            {
                // Generate curves for each loop of this stitch
                // loopNo = the index of the loop this stitch starts on
                // Create a curve for each loop in the stitch
                
                // Create parent GameObject under which to nest the mesh for each loop
                GameObject stitchGameObject = new GameObject($"Stitch - row {rowIndex} stitch {stitchIndex} loop {loopIndex}");

                // Work through each of the loops produced in the loopTypeList
                for (int i = 0; i < this.loops.Length; i++)
                {
                    GameObject mesh = loops[i].GenerateMesh(material);
                    mesh.transform.SetParent(stitchGameObject.transform);
                }

                return stitchGameObject;
            }
    }

    public class KnitStitch : Stitch
    {
        public KnitStitch(int rowIndex, int stitchIndex, int loopIndex, float yarnWidth) : 
            base(rowIndex, stitchIndex, loopIndex, yarnWidth)
        {
            this.stitchType = StitchType.KnitStitch;
            this.loopsConsumed = 1;
            this.loopsProduced = 1;
            this.held = 0;
            this.front = false;
            this.loopTypeList = new LoopType[1] {LoopType.Knit};
            this.loops = GetLoops();
        }
    }

    public class PurlStitch : Stitch
    {
        public PurlStitch(int rowIndex, int stitchIndex, int loopIndex, float yarnWidth) :
            base(rowIndex, stitchIndex, loopIndex, yarnWidth)
        {
            this.stitchType = StitchType.PurlStitch;
            this.loopsConsumed = 1;
            this.loopsProduced = 1;
            this.held = 0;
            this.front = false;
            this.loopTypeList = new LoopType[1] {LoopType.Purl};
            this.loops = GetLoops();
        }
    }

    public class Cable1Lo1RStitch : Stitch
    {
        public Cable1Lo1RStitch(int rowIndex, int stitchIndex, int loopIndex, float yarnWidth) : 
            base(rowIndex, stitchIndex, loopIndex, yarnWidth)
        {
            this.stitchType = StitchType.Cable1Lo1RStitch;
            this.loopsConsumed = 2;
            this.loopsProduced = 2;
            this.held = 1;
            this.front = false;
            this.loopTypeList = new LoopType[2] {LoopType.Knit, LoopType.Knit};
            this.loops = GetLoops();
        }
    }
    
    public class Cable2Lo2RStitch : Stitch
    {
        public Cable2Lo2RStitch(int rowIndex, int stitchIndex, int loopIndex, float yarnWidth) :
            base(rowIndex, stitchIndex, loopIndex, yarnWidth)
        {
            this.stitchType = StitchType.Cable2Lo2RStitch;
            this.loopsConsumed = 4;
            this.loopsProduced = 4;
            this.held = 2;
            this.front = false;
            this.loopTypeList = new LoopType[4] 
                {LoopType.Knit, LoopType.Knit, LoopType.Knit, LoopType.Knit};
            this.loops = GetLoops();
        }
    }
    
    public class CableKnitStitch : Stitch
    {
        public CableKnitStitch(int rowIndex, int stitchIndex, int loopIndex, float yarnWidth) :
            base(rowIndex, stitchIndex, loopIndex, yarnWidth)
        {
            this.stitchType = StitchType.CableKnitStitch;
            this.loopsConsumed = 2;
            this.loopsProduced = 2;
            this.held = 0;
            this.front = true;
            this.loopTypeList = new LoopType[2] {LoopType.Knit, LoopType.Knit};
            this.loops = GetLoops();
        }
    }
    
    public class CableKnitStitch4 : Stitch
    {
        public CableKnitStitch4(int rowIndex, int stitchIndex, int loopIndex, float yarnWidth) :
            base(rowIndex, stitchIndex, loopIndex, yarnWidth)
        {
            this.stitchType = StitchType.CableKnitStitch4;
            this.loopsConsumed = 4;
            this.loopsProduced = 4;
            this.held = 0;
            this.front = true;
            this.loopTypeList = new LoopType[4] 
                {LoopType.Knit, LoopType.Knit, LoopType.Knit, LoopType.Knit};
            this.loops = GetLoops();
        }
    }
}
