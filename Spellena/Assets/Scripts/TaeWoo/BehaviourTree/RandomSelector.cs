using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTree
{
    public class RandomSelector : Node
    {
        int randomIndex = 0;

        public RandomSelector() : base() { }

        public RandomSelector(List<Node> children) : base(children) { }

        public override NodeState Evaluate()
        {
            Node node = children[randomIndex];
            if(node.IsSkillDoing())
                randomIndex = Random.Range(0, children.Count);

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