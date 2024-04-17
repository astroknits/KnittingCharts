using UnityEngine;

namespace YarnGenerator
{
    public abstract class StitchGridNode
    {
        internal int rowIndex;
        internal int stitchIndex;
        internal StitchGridNodeType nodeType;
        internal int nConsumed;
        internal int nProduced;

        internal StitchGridNode prevNode;
        internal StitchGridNode nextNode;
        internal StitchGridCourseEdge courseEdgeIn;
        internal StitchGridCourseEdge courseEdgeOut;

        internal abstract GameObject GenerateMesh(float yarnWidth, Material material);

        public StitchGridNode(int rowIndex, int stitchIndex)
        {
            this.rowIndex = rowIndex;
            this.stitchIndex = stitchIndex;
            this.prevNode = null;
            this.nextNode = null;
            this.courseEdgeIn = null;
            this.courseEdgeOut = null;
        }

        public static StitchGridNode GetNextNode(
            StitchGridNodeType nodeType,
            int rowIndex,
            int stitchIndex,
            StitchGridNode prevNode,
            StitchGridCourseEdge courseEdgeIn
        )
        {
            StitchGridNode node;
            switch (nodeType)
            {
                case StitchGridNodeType.Knit:
                    node = new StitchGridKnit(rowIndex, stitchIndex);
                    break;
                default:
                    node = new StitchGridKnit(rowIndex, stitchIndex);
                    break;
            }
            node.prevNode = prevNode;
            if (prevNode is not null)
            {
                node.prevNode.SetNextNode(node);
            }
            node.courseEdgeIn = courseEdgeIn;
            if (courseEdgeIn is not null)
            {
                courseEdgeIn.SetConsumedBy(node);
            }

            return node;
        }

        public static StitchGridNode GetPrevNode(
            StitchGridNodeType nodeType,
            int rowIndex,
            int stitchIndex,
            StitchGridNode nextNode,
            StitchGridCourseEdge courseEdgeOut
        )
        {
            StitchGridNode node;
            switch (nodeType)
            {
                case StitchGridNodeType.Knit:
                    node = new StitchGridKnit(rowIndex, stitchIndex);
                    break;
                default:
                    node = new StitchGridKnit(rowIndex, stitchIndex);
                    break;
            }
            if (nextNode is not null)
            {
                node.nextNode = nextNode;
                node.nextNode.SetPrevNode(node);
            }
            if (courseEdgeOut is not null)
            {
                node.courseEdgeOut = courseEdgeOut;
                courseEdgeOut.SetProducedBy(node);
            }

            return node;
        }

        public void SetPrevNode(StitchGridNode prevNode)
        {
            this.prevNode = prevNode;
        }

        public void SetNextNode(StitchGridNode nextNode)
        {
            this.nextNode = nextNode;
        }
    }

    public class StitchGridKnit : StitchGridNode
    {
        public StitchGridKnit(
            int rowIndex,
            int stitchIndex
            ) : base(rowIndex, stitchIndex)
        {
            this.nodeType = StitchGridNodeType.Knit;
            this.nConsumed = 1;
            this.nProduced = 1;
        }

        internal override GameObject GenerateMesh(float yarnWidth, Material material)
        {
            StitchGridMeshGenerator stitchGridMeshGenerator = new StitchGridMeshGenerator(nodeType);
            return stitchGridMeshGenerator.GenerateGameObject(
                yarnWidth,
                material,
                rowIndex,
                stitchIndex,
                stitchIndex
            );
        }
    }

    /*
    public class StitchGridCastOn : StitchGridNode
    {
        public StitchGridCastOn(
            int rowIndex,
            int stitchIndex
        ) : base(rowIndex, stitchIndex)
        {
            this.nodeType = StitchGridNodeType.CastOn;
            this.nConsumed = 0;
            this.nProduced = 1;
        }

        internal override void RenderPreview()
        {
            
        }
    }
    */
}