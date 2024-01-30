using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using BehaviourTree;

static class PlayerAniState
{
    public static string Move = "Move";
    public static string CheckEnemy = "CheckEnemy";
    public static string AvoidRight = "AvoidRight";
    public static string AvoidLeft = "AvoidLeft";
    public static string AvoidForward = "AvoidForward";
    public static string AvoidBack = "AvoidBack";
    public static string Aim = "Aim";
    public static string Draw = "Draw";
    public static string Shoot = "Shoot";
}

public class AloyBT : BehaviourTree.Tree
{
    public Transform occupationPoint;

    public GameObject bowAniObj;
    public GameObject arrowAniObj;

    public Transform aimingTransform;
    public Transform arrowStrikeStart;
    public Transform downArrowTrans;

    private Animator animator;

    private List<Gauge> gaugeList = new List<Gauge>();

    private GotoOccupationArea gotoOccupationArea;

    private AloyBasicAttack aloyBasicAttack;
    private AloyPreciseShot aloyPreciseShot;
    private AloyPurifyBeam aloyPurifyBeam;
    private AloyArrowStrike aloyArrowStrike;

    private Coroutine coolTimeCoroutine;

    void InitData()
    {
        animator = GetComponent<Animator>();
        if (animator == null) Debug.LogError("Animator가 할당되지 않았습니다");

        gaugeList.Add(new Gauge(2.0f));
        gaugeList.Add(new Gauge(8.0f));
        gaugeList.Add(new Gauge(10.0f));
        gaugeList.Add(new Gauge(20.0f));

        gotoOccupationArea = new GotoOccupationArea
            (transform, occupationPoint, arrowAniObj);
        aloyBasicAttack = new AloyBasicAttack
            (transform, aimingTransform, bowAniObj, arrowAniObj, gaugeList[0]);
        aloyPreciseShot = new AloyPreciseShot
            (transform, aimingTransform, bowAniObj, arrowAniObj, gaugeList[1]);
        aloyPurifyBeam = new AloyPurifyBeam
            (transform, aimingTransform, bowAniObj, arrowAniObj, gaugeList[2]);
        aloyArrowStrike = new AloyArrowStrike
            (transform, arrowStrikeStart, downArrowTrans, bowAniObj, arrowAniObj, gaugeList[3]);

        coolTimeCoroutine = StartCoroutine(CoolTimer());
    }

    //Start()
    protected override Node SetupTree()
    {
        InitData();

        Node root = new Selector(NodeName.Selector ,new List<Node>
        {
            new CheckEnemy(
                new Parallel(NodeName.Parallel, new List<Node>
                    {
                        aloyBasicAttack,
                        aloyPreciseShot,
                        aloyPurifyBeam,
                        aloyArrowStrike
                    }),
                transform, bowAniObj, 150f, 30f,
                LayerMask.GetMask("Player")),
            gotoOccupationArea
        }
        );

        root.SetDataToRoot(DataContext.NodeStatus, root);  
        return root;
    }

    protected override void Update()
    {
        //if(PhotonNetwork.IsMasterClient)
        {
            base.Update();
            ShowState();
        }
    }

    IEnumerator CoolTimer()
    {
        while (true)
        {
            for (int i = 0; i < gaugeList.Count; i++)
            {
                gaugeList[i].UpdateCurCoolTime(Time.deltaTime);
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
        if (animator == null) return;
        if (root.GetData(DataContext.EnemyTransform) == null) return;
        animator.SetLookAtWeight(1f, 0.9f);

        if(root.GetData(DataContext.NotSensingEnemy) == null)
        {
            animator.SetLookAtPosition(((CheckEnemy)root.GetData(DataContext.EnemyTransform)).enemyTransform.position);
            aimingTransform.LookAt(((CheckEnemy)root.GetData(DataContext.EnemyTransform)).enemyTransform.position);
        }

        else if (root.GetData(DataContext.NotSensingEnemy).nodeName
            == aloyArrowStrike.nodeName)
        {
            animator.SetLookAtPosition(((AloyArrowStrike)root.GetData(DataContext.NotSensingEnemy)).attackTransform.position);
            aimingTransform.LookAt(((AloyArrowStrike)root.GetData(DataContext.NotSensingEnemy)).attackTransform.position);
        }

    }

    void ShowState()
    {
        Debug.Log("<color=orange>" + root.GetData(DataContext.NodeStatus).nodeName + "</color>");
    }
}
