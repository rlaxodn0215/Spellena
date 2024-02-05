using System.Collections.Generic;

namespace BehaviorTree
{
    public class Parallel : Node
    {
        int index = 0;
        public Parallel(Tree tree, NodeName name, List<Node> children) : base(tree, name, children) { }
        public override NodeState Evaluate()
        {
            if (GetData(DataContext.FixNode) == null)
                index = (index + 1) % children.Count;

            Node node = children[index];

            SetDataToRoot(DataContext.NodeStatus, this);

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