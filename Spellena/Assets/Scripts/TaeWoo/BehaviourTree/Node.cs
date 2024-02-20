using System.Collections.Generic;
using DefineDatas;

namespace BehaviorTree
{
    public class Node
    {
        protected NodeState state;
        public NodeType nodeType = NodeType.NONE;

        public Tree tree;
        public Node parent;
        protected List<Node> children = new List<Node>();

        private Dictionary<NodeData, Node> nodeData = new Dictionary<NodeData, Node>();

        public Node()
        {
            parent = null;
        }

        public Node(Tree useTree, NodeType type, List<Node> children)
        {
            tree = useTree;
            nodeType = type;
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

        // 해당 데이터를 현재 노드에 저장
        public void SetData(NodeData key, Node value)
        {
            nodeData[key] = value;
        }

        // 해당 데이터를 Root 노드에 저장
        public void SetDataToRoot(NodeData key, Node value)
        {
            Node temp = parent;
            Node root = this;

            while (temp != null)
            {
                root = temp;
                temp = temp.parent;
            }

            root.nodeData[key] = value;
        }

        public Node GetData(NodeData key)
        {
            Node value = null;
            if (nodeData.TryGetValue(key, out value))
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

        public bool ClearData(NodeData key)
        {
            if (nodeData.ContainsKey(key))
            {
                nodeData.Remove(key);
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

