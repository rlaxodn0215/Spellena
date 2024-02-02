using System.Collections.Generic;

namespace BehaviorTree
{
    public enum NodeState
    {
        Running,
        Success,
        Failure
    }

    public enum NodeName
    {
        NONE,
        Sequence,
        Selector,
        Parallel,
        Condition_1,
        Function_1,
        Skill_1,
        Skill_2,
        Skill_3,
        Skill_4
    }

    public enum DataContext
    {
        NodeStatus,
        FixNode,
    }

    public class Node
    {
        protected NodeState state;
        public NodeName nodeName = NodeName.NONE;

        public Tree tree;
        public Node parent;
        protected List<Node> children = new List<Node>();

        private Dictionary<DataContext, Node> dataContext 
            = new Dictionary<DataContext, Node>();

        public Node()
        {
            parent = null;
        }

        public Node(Tree useTree ,NodeName name, List<Node> children)
        {
            tree = useTree;
            nodeName = name;

            if(children != null)
            foreach (Node child in children)
                Attach(child);
        }

        protected void Attach(Node node)
        {
            node.parent = this;
            children.Add(node);
        }

        public virtual NodeState Evaluate()
        {
            return NodeState.Failure;
        }

        public void SetData(DataContext key, Node value)
        {
            dataContext[key] = value;
        }

        public void SetDataToRoot(DataContext key, Node value)
        {
            Node temp = parent;
            Node root = this;

            while (temp != null)
            {
                root = temp;
                temp = temp.parent;
            }

            root.dataContext[key] = value;
        }

        public Node GetData(DataContext key)
        {
            Node value = null;
            if (dataContext.TryGetValue(key, out value))
                return value;

            // 부모 노드에 해당 데이터가 있는지 확인
            Node node = parent;
            while (node != null)
            {
                value = node.GetData(key);
                if (value != null)
                    return value;
                node = node.parent;
            }

            return null;
        }

        public bool ClearData(DataContext key)
        {
            if (dataContext.ContainsKey(key))
            {
                dataContext.Remove(key);
                return true;
            }

            Node node = parent;
            while (node != null)
            {
                if (node.ClearData(key))
                    return true;
                node = node.parent;
            }

            return false;
        }
    }
}

