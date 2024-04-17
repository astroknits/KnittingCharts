using UnityEngine;

namespace YarnGenerator
{
    public abstract class StitchGrid
    {
        internal int nRows;
        internal int stitchesPerRowStart;
        internal StitchGridNode root;
        internal StitchGridNode[][] nodes;

        internal abstract StitchGridNodeType[][] GetPatternNodeTypes();

        public StitchGrid(int nRows, int stitchesPerRowStart)
        {
            this.nRows = nRows;
            this.stitchesPerRowStart = stitchesPerRowStart;
            this.nodes = new StitchGridNode[nRows][];
            for (int index = 0; index < nRows; index++)
            {
                this.nodes[index] = new StitchGridNode[stitchesPerRowStart];
            }
            StitchGridNodeType[][] nodeTypes = GetPatternNodeTypes();
            GeneratePattern(nodeTypes);
        }

        internal void GeneratePattern(StitchGridNodeType[][] stitchGridNodeType)
        {
            // Loop through the stitches in each row, from left to right.
            // Note the pattern is worked flat, and so this order is not reflective
            // of the yarn direction
            // At beginning of pattern, set the prevNode and courseEdgeIn to null
            StitchGridNode prevNode = null;
            StitchGridCourseEdge courseEdgeIn = null;
            for (int rowIndex = 0; rowIndex < nRows; rowIndex++)
            {
                // For each row, set the prevNode and courseEdgeIn to null
                // StitchGridNode prevNode = null;
                // StitchGridCourseEdge courseEdgeIn = null;
                for (int stitchIndex = 0; stitchIndex < stitchesPerRowStart; stitchIndex++)
                {
                    StitchGridNode node;
                    // Every other row is the wrong/purl side; reverse order of stitches
                    int index = stitchIndex;
                    if (stitchIndex % 2 == 1)
                    {
                        index = stitchesPerRowStart - stitchIndex;
                    }
                    node = StitchGridNode.GetNextNode(
                        stitchGridNodeType[rowIndex][stitchIndex], 
                        rowIndex,
                        index,
                        prevNode,
                        courseEdgeIn
                    );
                    nodes[rowIndex][index] = node;
                    prevNode = node;
                    courseEdgeIn = node.courseEdgeOut;
                }
            }
        }

        public void RenderPreview(float yarnWidth, Material material)
        {
            GameObject parent = new GameObject($"StitchGrid 1");
            for (int rowNumber = 0; rowNumber < this.nRows; rowNumber++)
            {
                for (int stitchNumber = 0; stitchNumber < stitchesPerRowStart; stitchNumber++)
                {
                    GameObject go = nodes[rowNumber][stitchNumber].GenerateMesh(yarnWidth, material);
                    go.transform.SetParent(parent.transform);
                }
            }
        }
    }

    public class JerseyStitchGrid : StitchGrid
    {
        public JerseyStitchGrid(
            int nRows,
            int stitchesPerRowStart
        ) : base(nRows, stitchesPerRowStart)
        { }

        internal override StitchGridNodeType[][] GetPatternNodeTypes()
        {
            StitchGridNodeType[][] nodeTypes = new StitchGridNodeType[nRows][];
            for (int rowIndex = 0; rowIndex < nRows; rowIndex++)
            {
                nodeTypes[rowIndex] = new StitchGridNodeType[stitchesPerRowStart];
                for (int stitchIndex = 0; stitchIndex < stitchesPerRowStart; stitchIndex++)
                {
                    nodeTypes[rowIndex][stitchIndex] = StitchGridNodeType.Knit;
                }
            }
            return nodeTypes;
        }
    }
}