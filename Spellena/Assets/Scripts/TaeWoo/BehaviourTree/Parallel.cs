using System.Collections.Generic;

namespace BehaviourTree
{
    public class Parallel : Node
    {
        int index = 0;

        public Parallel() : base() { }

        public Parallel(NodeName name, List<Node> children) : base(name, children) { }

        public override NodeState Evaluate()
        {
            if (GetData(DataContext.IsNoSkillDoing) == null)
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