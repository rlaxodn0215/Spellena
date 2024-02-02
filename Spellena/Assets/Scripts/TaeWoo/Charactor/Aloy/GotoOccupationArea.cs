using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;
using DefineDatas;

public class GotoOccupationArea : AbilityNode
{
    private List<Transform> occupationPoints = new List<Transform>();
    private NavMeshAgent agent;
    private Animator animator;
    private GameObject arrowAniObj;
    private bool isNull;
    private int randomIndex = 0;

    public GotoOccupationArea(BehaviorTree.Tree tree, AbilityMaker abilityMaker, float coolTime)
        : base(tree, NodeName.Function_1, coolTime)
    {
        arrowAniObj = abilityMaker.abilityObjectTransforms[(int)AbilityMaker.AbilityObjectName.ArrowAniObject].gameObject;
        agent = abilityMaker.abilityObjectTransforms[(int)AbilityMaker.AbilityObjectName.CharacterTransform].GetComponent<NavMeshAgent>();
        animator = abilityMaker.abilityObjectTransforms[(int)AbilityMaker.AbilityObjectName.CharacterTransform].GetComponent<Animator>();
        agent.speed = abilityMaker.data.moveSpeed;
        agent.angularSpeed = abilityMaker.data.rotateSpeed;

        for(int i = 0; i < abilityMaker.abilityObjectTransforms[(int)AbilityMaker.AbilityObjectName.OccupationPoint].childCount; i++)
        {
            occupationPoints.Add(abilityMaker.abilityObjectTransforms[(int)AbilityMaker.AbilityObjectName.OccupationPoint].GetChild(i));
        }

        isNull = NullCheck();
    }
    public override NodeState Evaluate()
    {
        if (isNull)
        {
            state = NodeState.Failure;
            return state;
        }
        SetDataToRoot(DataContext.NodeStatus, this);
        RandomOccupationPosition();
        animator.SetBool(PlayerAniState.Move, true);
        arrowAniObj.SetActive(false);
        Debug.Log("GotoOccupationArea.. Point " + randomIndex);
        state = NodeState.Running;
        return state;
    }

    private void RandomOccupationPosition()
    {       
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            randomIndex = Random.Range(0, occupationPoints.Count);
            agent.destination = occupationPoints[randomIndex].position;
        }
        else
        {
            agent.isStopped = false;
        }
    }
    private bool NullCheck()
    {
        if (arrowAniObj == null)
        {
            Debug.LogError("ArrowAniObj 할당되지 않았습니다");
            return true;
        }
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent가 할당되지 않았습니다");
            return true;
        }
        if (animator == null)
        {
            Debug.LogError("Animator가 할당되지 않았습니다");
            return true;
        }
        return false;
    }
}
