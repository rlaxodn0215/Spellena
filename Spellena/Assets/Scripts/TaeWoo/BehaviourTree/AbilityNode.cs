namespace BehaviorTree
{
    public class AbilityNode : Node
    {
        public CoolTimer coolTimer;
        public AbilityNode():base()
        { }
        public AbilityNode(Tree tree, NodeName skillName, float coolTime) 
            : base(tree,skillName, null)
        {
            if(coolTime > 0.0f) coolTimer = new CoolTimer(coolTime);
        }
    }
}