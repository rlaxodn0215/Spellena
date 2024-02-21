using System;
using System.Collections.Generic;
using UnityEngine;
using ListenerNodeData;

public class PlayerObserver : MonoBehaviour
{
    private List<ListenNode> listenNodes = new List<ListenNode>();
    private List<ListenNode> listener = new List<ListenNode>();

    private void Update()
    {
        ListenState();
    }

    private void ListenState()
    {
        for(int i = 0; i < listenNodes.Count; i++)
        {
            if (listenNodes[i].index != listener[i].index)
            {
                listenNodes[i].NotifyChange();//예시 : routeIndex가 변화하였을 때 호출
                listener[i].index = listenNodes[i].index;
            }
        }
    }
    
    public void RaiseListenNode(ListenNode listenNode)
    {
        ListenNode _listener = new ListenNode();

        _listener.index = listenNode.index;

        listenNodes.Add(listenNode);
        listener.Add(_listener);
    }

    public void LowerListenNode(ListenNode listenNode)
    {
        for(int i = 0; i < listenNodes.Count; i++)
        {
            if (listenNodes[i] == listenNode)
            {
                listenNodes.RemoveAt(i);
                listener.RemoveAt(i);
            }
        }
    }
}
