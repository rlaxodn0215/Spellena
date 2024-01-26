using System.Collections.Generic;

namespace BehaviourTree
{
    public class Parallel : Node
    {
        int index = 0;

        public Parallel() : base() { }

        public Parallel(List<Node> children) : base(children) { }

        public override NodeState Evaluate()
        {
            if (GetData("IsNoSkillDoing") == null)
                index = (index + 1) % children.Count;

            Node node = children[index];

            SetDataToRoot("Status", "RandomSelector");

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