using DefineDatas;
namespace BehaviorTree
{
    public class ActionNode : Node
    {
        public ActionName actionName;
        public CoolTimer coolTimer;
        public ActionNode():base()
        { }
        public ActionNode(Tree tree, ActionName name)
            : base(tree, NodeType.Action, null)
        {
            actionName = name;
        }
        public ActionNode(Tree tree, ActionName name, float coolTime) 
            : base(tree, NodeType.Action, null)
        {
            if(coolTime > 0.0f) coolTimer = new CoolTimer(coolTime);
            actionName = name;
        }
    }
}