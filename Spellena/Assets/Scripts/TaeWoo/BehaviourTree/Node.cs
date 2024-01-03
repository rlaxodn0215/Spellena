using System.Collections;
using System.Collections.Generic;

namespace BehaviourTree
{
    public enum NodeState
    {
        Running,
        Success,
        Failure
    }

    public class Node
    {
        protected NodeState state;

        public Node parent;
        protected List<Node> children = new List<Node>();

        private Dictionary<string, object> dataContext = new Dictionary<string, object>();

        public Node()
        {
            parent = null;
        }

        public Node(List<Node> children)
        {
            foreach (Node child in children)
                Attach(child);
        }

        private void Attach(Node node)
        {
            node.parent = this;
            children.Add(node);
        }

        public virtual NodeState Evaluate() => NodeState.Failure;

        public void SetData(string key, object value)
        {
            dataContext[key] = value;
        }

        public object GetData(string key)
        {
            object value = null;
            if (dataContext.TryGetValue(key, out value))
                return value;

            // 부모 노드에 해당 데이터가 있는지 확인
            Node node = parent;
            while(node !=null)
            {
                value = GetData(key);
                if (value != null)
                    return value;
                node = node.parent;
            }

            return null;
        }

        public bool ClearData(string key)
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

