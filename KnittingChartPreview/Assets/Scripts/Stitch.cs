using System;
using System.Linq;
using UnityEngine;


namespace YarnGenerator

{
    public class Stitch
    {
        // StitchInfo object, containing StitchType and set of
        // attributes that define the stitch.  
        public StitchInfo stitchInfo;
        // width of the yarn
        public float yarnWidth;
        // row number for the given stitch
        public int rowIndex;
        // stitch number for the given stitch
        public int stitchIndex;
        // starting loop number for the given stitch
        public int loopIndex;
        
        // list of loop objects
        public Loop[] loops;
        
        public Stitch(StitchType stitchType, int rowIndex, int stitchIndex, int loopIndex, float yarnWidth)
        {
            this.stitchInfo = StitchInfo.GetStitchInfo(stitchType);
            this.rowIndex = rowIndex;
            this.stitchIndex = stitchIndex;
            this.loopIndex = loopIndex;
            this.yarnWidth = yarnWidth;
            this.loops = GetLoops();
        }

        public Loop[] GetLoops()
        {
            loops = new Loop[this.stitchInfo.loopsProduced];
            // Work through each of the loops produced in the loopTypeList
            for (int i = 0; i < this.stitchInfo.loopsProduced; i++)
            {
                int loopIndexStart = this.loopIndex + i;
                int loopIndexEnd = loopIndexStart;
                bool heldInFront = false;
                bool heldBehind = false;
                if (this.stitchInfo.held == 0)
                {
                    loopIndexEnd = loopIndexStart;
                }
                else
                {
                    // hold the first this.held loops in front of/behind the
                    // needle and first knit the remaining
                    // this.loopsProduced - this.held loops
                    if (i >= this.stitchInfo.held)
                    {
                        loopIndexEnd = loopIndexStart - this.stitchInfo.held;
                        heldInFront = this.stitchInfo.front;
                        heldBehind = !this.stitchInfo.front;
                    }
                    else
                    {
                        loopIndexEnd = loopIndexStart + this.stitchInfo.held;
                        heldInFront = !this.stitchInfo.front;
                        heldBehind = this.stitchInfo.front;
                    }
                }

                loops[i] = new Loop(
                    stitchInfo.loopInfoList[i].loopType, 
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
            // Work through each of the loops produced in the loopInfoList
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
}
