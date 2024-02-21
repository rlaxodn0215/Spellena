using System;
using System.Collections.Generic;

namespace ListenerNodeData
{
    public class ListenNode
    {
        public int index;
        public Action<ListenNode> NotifyDataChanged;

        public void NotifyChange()
        {
            NotifyDataChanged?.Invoke(this);
        }
    }

    public enum ListenNodeDataType
    {

    }
}
