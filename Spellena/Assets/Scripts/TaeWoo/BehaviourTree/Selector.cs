using System.Collections.Generic;

namespace BehaviorTree
{
    //OR 와 같은 역활
    public class Selector : Node
    {
        public Selector() : base() { }
        public Selector(Tree tree, NodeName name, List<Node> children) : base(tree, name, children) { }

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

            SetDataToRoot(DataContext.NodeStatus, this);
            state = NodeState.Failure;
            return state;
        }
    }
}
