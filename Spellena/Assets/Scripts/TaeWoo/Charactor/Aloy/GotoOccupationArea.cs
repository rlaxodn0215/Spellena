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
    private int randomIndex = 0;

    public GotoOccupationArea(BehaviorTree.Tree tree, AbilityMaker abilityMaker, float coolTime)
        : base(tree, NodeName.Function_1, coolTime)
    {
        arrowAniObj = abilityMaker.abilityObjectTransforms[(int)AbilityMaker.AbilityObjectName.ArrowAniObject].gameObject;
        if (arrowAniObj == null) ErrorDataMaker.SaveErrorData(ErrorCode.GotoOccupationArea_arrowAniObj_NULL);

        agent = abilityMaker.abilityObjectTransforms[(int)AbilityMaker.AbilityObjectName.CharacterTransform].GetComponent<NavMeshAgent>();
        if (agent == null) ErrorDataMaker.SaveErrorData(ErrorCode.GotoOccupationArea_agent_NULL);

        animator = abilityMaker.abilityObjectTransforms[(int)AbilityMaker.AbilityObjectName.CharacterTransform].GetComponent<Animator>();
        if (animator == null) ErrorDataMaker.SaveErrorData(ErrorCode.GotoOccupationArea_animator_NULL);

        agent.speed = abilityMaker.data.moveSpeed;
        agent.angularSpeed = abilityMaker.data.rotateSpeed;

        for(int i = 0; i < abilityMaker.abilityObjectTransforms[(int)AbilityMaker.AbilityObjectName.OccupationPoint].childCount; i++)
        {
            occupationPoints.Add(abilityMaker.abilityObjectTransforms[(int)AbilityMaker.AbilityObjectName.OccupationPoint].GetChild(i));
        }
    }
    public override NodeState Evaluate()
    {
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
}
