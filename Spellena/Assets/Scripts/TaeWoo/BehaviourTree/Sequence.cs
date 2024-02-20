using System.Collections.Generic;
using DefineDatas;

namespace BehaviorTree
{
    //AND 와 같은 역활
    public class Sequence : Node
    {
        public Sequence() : base() { }
        public Sequence(Tree tree, List<Node> children) : base(tree, NodeType.Sequence, children) { }

        public override NodeState Evaluate()
        {
            bool anyChildIsRunning = false;

            foreach(Node node in children)
            {
                switch(node.Evaluate())
                {
                    case NodeState.Failure:
                        state = NodeState.Failure;
                        return state;
                    case NodeState.Success:
                        continue;
                    case NodeState.Running:
                        anyChildIsRunning = true;
                        continue;
                    default:
                        state = NodeState.Success;
                        return state;
                }
            }

            state = anyChildIsRunning ? NodeState.Running : NodeState.Success;
            return state;
        }
    }
}