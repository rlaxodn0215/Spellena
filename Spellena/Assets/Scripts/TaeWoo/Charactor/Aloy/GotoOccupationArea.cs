using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviourTree;

public class GotoOccupationArea : Node
{
    private Transform playerTrasform;
    private NavMeshAgent agent;

    private Transform occupationPoint;

    public GotoOccupationArea() { }

    public GotoOccupationArea(Transform _playerTrasform, Transform _occupationPoint)
    {
        playerTrasform = _playerTrasform;
        agent = playerTrasform.GetComponent<NavMeshAgent>();
        if (agent == null) Debug.LogError("NavMeshAgent가 할당되지 않았습니다");
        occupationPoint = _occupationPoint;
    }

    public override NodeState Evaluate()
    {
        agent.destination = occupationPoint.position;

        Debug.Log("GotoOccupationArea..");

        state = NodeState.Running;
        return state;
    }
}
