using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;
using DefineDatas;

public class GotoOccupationArea : ActionNode
{
    private List<Transform> occupationPoints = new List<Transform>();
    private NavMeshAgent agent;
    private Animator animator;
    private GameObject arrowAniObj;
    private int randomIndex = 0;

    public GotoOccupationArea(BehaviorTree.Tree tree, List<Transform> actionObjectTransforms, ScriptableObject data)
        : base(tree, ActionName.GotoOccupationArea)
    {
        arrowAniObj = actionObjectTransforms[(int)ActionObjectName.ArrowAniObject].gameObject;
        if (arrowAniObj == null) ErrorManager.SaveErrorData(ErrorCode.GotoOccupationArea_arrowAniObj_NULL);

        agent = actionObjectTransforms[(int)ActionObjectName.CharacterTransform].GetComponent<NavMeshAgent>();
        if (agent == null) ErrorManager.SaveErrorData(ErrorCode.GotoOccupationArea_agent_NULL);

        animator = actionObjectTransforms[(int)ActionObjectName.CharacterTransform].GetComponent<Animator>();
        if (animator == null) ErrorManager.SaveErrorData(ErrorCode.GotoOccupationArea_animator_NULL);

        agent.speed = ((GotoOccupationAreaData)data).moveSpeed;
        agent.angularSpeed = ((GotoOccupationAreaData)data).rotateSpeed;

        for(int i = 0; i < actionObjectTransforms[(int)ActionObjectName.OccupationPoint].childCount; i++)
        {
            occupationPoints.Add(actionObjectTransforms[(int)ActionObjectName.OccupationPoint].GetChild(i));
        }
    }
    public override NodeState Evaluate()
    {
        SetDataToRoot(NodeData.NodeStatus, this);
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
