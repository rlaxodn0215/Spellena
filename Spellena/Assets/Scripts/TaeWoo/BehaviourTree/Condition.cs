using System.Collections.Generic;
using System;

namespace BehaviourTree
{
    public class Condition : Node 
    {
        protected Func<bool> condition;

        public Condition() : base()
        {
            condition = null;
        }

        public Condition(Func<bool> _condition, Node _TNodes)
        {
            condition += _condition;
            Attach(_TNodes);
        }

        public override NodeState Evaluate()
        {
            if(condition())
            {
                //TRUE
                if(children[0] != null)
                    children[0].Evaluate();
                return NodeState.Success;
            }

            else
            {
                //FALSE
                return NodeState.Failure;
            }
        }

    }
}
