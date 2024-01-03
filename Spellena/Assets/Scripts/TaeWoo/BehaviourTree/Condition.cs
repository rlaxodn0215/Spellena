using System;

namespace BehaviourTree
{
    public class Condition : Node
    {
        private Func<bool> condition;

        public Condition()
        {
            this.condition = null;
        }

        public Condition(Func<bool> _condition)
        {
            this.condition = _condition;
        }

        public override NodeState Evaluate()
        {
            return condition() ? NodeState.Success : NodeState.Failure;
        }

    }
}
