using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;
using DefineDatas;
using System;
public class AloyBT : BehaviorTree.Tree
{
    // 캐릭터 기능 설정
    public List<ActionName> characterActions;

    [HideInInspector]
    public Transform lookTransform;
    private Transform aimingTrasform;
    private Vector3 lookPosition;
    private List<ActionNode> actions = new List<ActionNode>();
    private ActionNodeMaker actionNodeMaker;
    private Animator animator;

    void InitData()
    {
        UnityEngine.Random.InitState(DefineNumber.RandomInitNum);
        animator = GetComponent<Animator>();
        if (animator == null) ErrorManager.SaveErrorData(ErrorCode.AloyBT_animator_NULL);
        actionNodeMaker = GetComponent<ActionNodeMaker>();
        if (actionNodeMaker == null) ErrorManager.SaveErrorData(ErrorCode.AloyBT_abilityMaker_NULL);
        aimingTrasform = actionNodeMaker.actionObjectTransforms[(int)ActionObjectName.AimingTransform];
        if(aimingTrasform == null) ErrorManager.SaveErrorData(ErrorCode.AloyBT_aimingTrasform_NULL);
        for (int i = 0; i < characterActions.Count; i++)
            actions.Add(actionNodeMaker.MakeActionNode(characterActions[i]));      
    }
    protected override Node SetupTree()
    {
        InitData();
        Node root = new Selector(this, new List<Node>
        {
            new EnemyDetector(this,
                new Parallel(this, new List<Node>
                {
                    actions[(int)ActionName.NormalArrowAttack],
                    actions[(int)ActionName.BallArrowAttack],
                    actions[(int)ActionName.ArrowRainAttack]
                }),
            actionNodeMaker.actionObjectTransforms),
            actions[(int)ActionName.GotoOccupationArea]
        }
        );
        root.SetDataToRoot(NodeData.NodeStatus, root);
        StartCoroutine(CoolTimer());
        ErrorCheck();
        return root;
    }

    void ErrorCheck()
    {
        try
        {
            // 에러가 발생했는지 확인
            if (ErrorManager.isErrorOccur) 
                throw new Exception("에러 발생 시간 : " + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
        }

        catch (Exception e)
        {
            // 에러 발생 시간 저장 후 게임 종료
            ErrorManager.SaveErrorData(e.Message);
            Application.Quit();
        }
    }

    protected override void Update()
    {
        base.Update();
        ShowNodeState();
    }

    IEnumerator CoolTimer()
    {
        while (true)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                if(actions[i].coolTimer != null)
                    actions[i].coolTimer.UpdateCoolTime(Time.deltaTime);
            }

            yield return null;
        }
    }

    void OnAnimatorIK()
    {
        SetLookAtObj();
    }

    void SetLookAtObj()
    {
        if (animator == null || lookTransform == null) return;      
        animator.SetLookAtWeight(PlayerLookAtWeight.weight, PlayerLookAtWeight.bodyWeight);
        lookPosition = lookTransform.position + Offset.AimOffset;
        animator.SetLookAtPosition(lookPosition);
        aimingTrasform.LookAt(lookPosition);
    }

    // 현재 어떤 Node에 있는지 확인
    void ShowNodeState()
    {
        Debug.Log("<color=orange>" + root.GetData(NodeData.NodeStatus).nodeType + "</color>");
    }
}
