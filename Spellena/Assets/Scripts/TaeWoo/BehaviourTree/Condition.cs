using System;

namespace BehaviourTree
{
    public class Condition : Node
    {
        private Func<bool> condition;
        private Node trueNode;
        private Node falseNode;

        public Condition()
        {
            condition = null;
        }

        public Condition(Func<bool> _condition, Node _trueNode, Node _falseNode)
        {
            condition += _condition;
            trueNode = _trueNode;
            falseNode = _falseNode;
        }

        public override NodeState Evaluate()
        {
            if(condition())
            {
                if(trueNode != null)
                    trueNode.Evaluate();
                return NodeState.Success;
            }

            else
            {
                if (falseNode != null)
                    falseNode.Evaluate();
                return NodeState.Failure;
            }
        }

    }
}
