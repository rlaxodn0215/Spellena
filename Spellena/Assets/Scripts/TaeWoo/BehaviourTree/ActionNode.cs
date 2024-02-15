using DefineDatas;
namespace BehaviorTree
{
    public class ActionNode : Node
    {
        public CoolTimer coolTimer;
        public ActionNode():base()
        { }
        public ActionNode(Tree tree, NodeType type)
            : base(tree, type, null)
        { }
        public ActionNode(Tree tree, NodeType type, float coolTime) 
            : base(tree, type, null)
        {
            if(coolTime > 0.0f) coolTimer = new CoolTimer(coolTime);
        }
    }
}