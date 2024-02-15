using System.Collections.Generic;
using DefineDatas;

namespace BehaviorTree
{
    //OR 와 같은 역활
    public class Selector : Node
    {
        public Selector() : base() { }
        public Selector(Tree tree, List<Node> children) 
            : base(tree, NodeType.Selector, children) { }

        public override NodeState Evaluate()
        {
            foreach (Node node in children)
            {
                switch (node.Evaluate())
                {
                    case NodeState.Failure:
                        continue;
                    case NodeState.Success:
                        state = NodeState.Success;
                        return state;
                    case NodeState.Running:
                        state = NodeState.Running;
                        return state;
                    default:
                        continue;
                }
            }

            SetDataToRoot(NodeData.NodeStatus, this);
            state = NodeState.Failure;
            return state;
        }
    }
}
