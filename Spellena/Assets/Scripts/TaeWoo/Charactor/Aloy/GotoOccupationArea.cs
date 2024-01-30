using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviourTree;
using CoroutineMaker;

public class GotoOccupationArea : Node
{
    private Transform playerTransform;
    private NavMeshAgent agent;

    private List<Transform> occupationPoints = new List<Transform>();
    private Vector3 movePoint;
    private Animator animator;
    private GameObject arrowAniObj;

    private int randomIndex = 0;

    public GotoOccupationArea() { }

    public GotoOccupationArea(Transform _playerTransform,
        Transform _occupationPoint, GameObject _arrowAniObj) : base(NodeName.GotoOccupationArea, null)
    {
        playerTransform = _playerTransform;
        arrowAniObj = _arrowAniObj;

        agent = playerTransform.GetComponent<NavMeshAgent>();
        if (agent == null) Debug.LogError("NavMeshAgent가 할당되지 않았습니다");
        animator = playerTransform.GetComponent<Animator>();
        if (animator == null) Debug.LogError("Animator가 할당되지 않았습니다");

        for(int i = 0; i < _occupationPoint.childCount; i++)
        {
            occupationPoints.Add(_occupationPoint.GetChild(i));
        }

        agent.destination = occupationPoints[randomIndex].position;
    }

    public override NodeState Evaluate()
    {
        // 제게
        SetDataToRoot(DataContext.NodeStatus, this);

        RandomOccupationPosition();

        animator.SetBool("Move", true);
        arrowAniObj.SetActive(false);

        Debug.Log("GotoOccupationArea.. Point " + randomIndex);

        state = NodeState.Running;
        return state;
    }

    private void RandomOccupationPosition()
    {
        Random.InitState(7); //seed를 초기화하여 모두 동일하게 작용
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            randomIndex = Random.Range(0, occupationPoints.Count);
            agent.destination = occupationPoints[randomIndex].position;
            agent.speed = 3;
        }

        else
        {
            agent.isStopped = false;
        }
    }
}
