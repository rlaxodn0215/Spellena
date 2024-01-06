using System.Collections.Generic;
using System;

namespace BehaviourTree
{
    public class Condition : Node
    {
        private Func<bool> condition;

        public Condition() : base()
        {
            condition = null;
        }

        public Condition(Func<bool> _condition, List<Node> _TNodes)
            : base(_TNodes)
        {
            condition += _condition;
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
                //if (children[1] != null)
                    //children[1].Evaluate();
                return NodeState.Failure;
            }
        }

    }
}
