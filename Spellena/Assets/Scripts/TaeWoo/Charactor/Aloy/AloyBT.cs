using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourTree;

public class AloyBT : BehaviourTree.Tree
{
    public Transform occupationPoint;

    public GameObject bowAniObj;
    public GameObject arrowAniObj;

    public Transform basicAttackTransform;

    private Animator animator;

    private int skillNum = 1;
    private int[] skillTimer;

    private GameObject aloyPoolObj;
    private List<CheckGauge> gaugeList = new List<CheckGauge>();
    private CheckEnemy checkEnemy;
    private GotoOccupationArea gotoOccupationArea;
    private AloyBasicAttack aloyBasicAttack;
    private AloyLaserAttack aloyLaserAttack;

    void InitData()
    {
        checkEnemy = new CheckEnemy(transform, bowAniObj, 120f, 40f, LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Wall"));
        skillTimer = new int[skillNum];
        animator = GetComponent<Animator>();
        if (animator == null) Debug.LogError("Animator가 할당되지 않았습니다");

        for(int i = 0; i < skillNum; i++)
        {
            gaugeList.Add(new CheckGauge(2.0f));
        }

        aloyPoolObj = GameObject.Find("AloyPoolManager");
        if (aloyPoolObj == null) Debug.LogError("AloyPoolManager을 찾을 수 없습니다");

        gotoOccupationArea = new GotoOccupationArea(transform, occupationPoint);
        aloyBasicAttack = new AloyBasicAttack(transform, basicAttackTransform, bowAniObj, arrowAniObj, aloyPoolObj,checkEnemy, gaugeList[0]);
        //aloyLaserAttack = new AloyLaserAttack(transform, checkEnemy, gaugeList[1], 20);
    }

    //Start()
    protected override Node SetupTree()
    {
        InitData();

        Node root = new Selector(new List<Node>
        {
            new Condition(checkEnemy.EnemyInRange,
                new List<Node>
                {
                    new RandomSelector(new List<Node>
                    {
                        aloyBasicAttack,
                        //aloyLaserAttack
                    }),
                }
            ),
            gotoOccupationArea
        }
        );

        return root;
    }

    protected override void Update()
    {
        base.Update();
        CoolTimer();
    }

    void CoolTimer()
    {
        for (int i = 0; i < gaugeList.Count; i++)
        {
            gaugeList[i].UpdateCurCoolTime();
        }
    }

    void OnAnimatorIK()
    {
        SetLookAtObj();
    }

    void SetLookAtObj()
    {
        if (animator == null) return;
        animator.SetLookAtWeight(1f, 0.9f);
        if (checkEnemy.Enemy == null) return;
        animator.SetLookAtPosition(checkEnemy.Enemy.position);
    }
}
