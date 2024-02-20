using System;
using System.Collections.Generic;
using DefineDatas;

namespace BehaviorTree
{
    public class Condition : Node 
    {
        protected Func<bool> condition;
        
        public Condition() : base()
        {
            condition = null;
        }
        public Condition(Tree _tree, Func<bool> _condition, Node _TNode)
            :base(_tree, NodeType.Condition, new List<Node> { _TNode})
        {
            condition += _condition;
            tree = _tree;
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
