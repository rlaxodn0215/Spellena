using System.Collections.Generic;
using DefineDatas;

namespace BehaviorTree
{
    public class Parallel : Node
    {
        int index = 0;
        public Parallel(Tree tree, List<Node> children) : base(tree, NodeType.Parallel, children) { }
        public override NodeState Evaluate()
        {
            if (GetData(NodeData.FixNode) == null)
                index = (index + 1) % children.Count;

            Node node = children[index];

            SetDataToRoot(NodeData.NodeStatus, this);

            switch (node.Evaluate())
            {
                case NodeState.Failure:
                    state = NodeState.Failure;
                    return state;
                case NodeState.Success:
                    state = NodeState.Success;
                    return state;
                case NodeState.Running:
                    state = NodeState.Running;
                    return state;
                default:
                    state = NodeState.Failure;
                    return state;
            }
            
        }
    }
}