using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;
using DefineDatas;
using System;
public class AloyBT : BehaviorTree.Tree
{
    // 캐릭터 스킬 설정
    public List<AbilityMaker.SkillName> characterSkills;
    // 캐릭터 기능 설정(ex.거점이동)
    public List<AbilityMaker.FunctionName> characterFunctions;

    [HideInInspector]
    public Transform lookTransform;

    private Transform aimingTrasform;
    private List<AbilityNode> skills = new List<AbilityNode>();
    private Dictionary<AbilityMaker.FunctionName, AbilityNode> functions
        = new Dictionary<AbilityMaker.FunctionName, AbilityNode>();
    private AbilityMaker abilityMaker;
    private Animator animator;

    void InitData()
    {
        UnityEngine.Random.InitState(DefineNumber.RandomInitNum);
        animator = GetComponent<Animator>();
        if (animator == null) ErrorDataMaker.SaveErrorData(ErrorCode.AloyBT_animator_NULL);
        abilityMaker = GetComponent<AbilityMaker>();
        if (abilityMaker == null) ErrorDataMaker.SaveErrorData(ErrorCode.AloyBT_abilityMaker_NULL);
        aimingTrasform = abilityMaker.abilityObjectTransforms[(int)AbilityMaker.AbilityObjectName.AimingTransform];
        if(aimingTrasform == null) ErrorDataMaker.SaveErrorData(ErrorCode.AloyBT_aimingTrasform_NULL);
        for (int i = 0; i < characterSkills.Count; i++)
            skills.Add(abilityMaker.MakeSkill(characterSkills[i]));      
        for (int i = 0; i < characterFunctions.Count; i++)
            functions[characterFunctions[i]] = abilityMaker.MakeFunction(characterFunctions[i]);
    }
    protected override Node SetupTree()
    {
        InitData();
        Node root = new Selector(this, NodeName.Selector ,new List<Node>
        {
            new EnemyDetector(this, EnemyDetector.EnemyDetectType.CheckEnemyInSight, 
                new Parallel(this, NodeName.Parallel, MakeSkillNode()),abilityMaker),
            functions[AbilityMaker.FunctionName.GotoOccupationArea]
        }
        );
        root.SetDataToRoot(DataContext.NodeStatus, root);
        StartCoroutine(CoolTimer());
        ErrorCheck();
        return root;
    }

    List<Node> MakeSkillNode()
    {
        List<Node> temp = new List<Node>();
        for (int i = 0; i < skills.Count; i++)
            temp.Add(skills[i]);
        return temp;
    }
    void ErrorCheck()
    {
        try
        {
            if (ErrorDataMaker.isErrorOccur) 
                throw new Exception("에러 발생 시간 : " + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
        }

        catch (Exception e)
        {
            ErrorDataMaker.SaveErrorData(e.Message);
            Application.Quit();
        }
    }

    //protected override void Update()
    //{
    //    base.Update();
    //    ShowNodeState();
    //}

    IEnumerator CoolTimer()
    {
        while (true)
        {
            for (int i = 0; i < skills.Count; i++)
            {
                if(skills[i].coolTimer !=null)
                    skills[i].coolTimer.UpdateCoolTime(Time.deltaTime);
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
        animator.SetLookAtPosition(lookTransform.position);
        aimingTrasform.LookAt(lookTransform.position);
    }

    //// 현재 어떤 Node에 있는지 확인
    //void ShowNodeState()
    //{
    //    Debug.Log("<color=orange>" + root.GetData(DataContext.NodeStatus).nodeName + "</color>");
    //}
}
