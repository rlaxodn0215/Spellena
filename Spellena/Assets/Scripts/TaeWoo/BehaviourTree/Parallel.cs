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
            // 실행하는 ActionNode가 고정되어 있지 않으면  
            if (GetData(NodeData.FixNode) == null)
                index = (index + 1) % children.Count;

            Node node = children[index];

            // 현재 접근하는 Node 저장
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