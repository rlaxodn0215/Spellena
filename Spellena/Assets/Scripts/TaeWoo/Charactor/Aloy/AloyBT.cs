using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using BehaviourTree;

public class AloyBT : BehaviourTree.Tree
{
    public Transform occupationPoint;

    public GameObject bowAniObj;
    public GameObject arrowAniObj;

    public Transform aimingTransform;
    public Transform arrowStrikeTransform;

    private Animator animator;

    private GameObject aloyPoolObj;
    private List<CheckGauge> gaugeList = new List<CheckGauge>();

    private GotoOccupationArea gotoOccupationArea;
    private AloyBasicAttack aloyBasicAttack;
    private AloyPreciseShot aloyPreciseShot;
    private AloyPurifyBeam aloyPurifyBeam;
    private AloyArrowStrike aloyArrowStrike;

    void InitData()
    {
        animator = GetComponent<Animator>();
        if (animator == null) Debug.LogError("Animator가 할당되지 않았습니다");

        gaugeList.Add(new CheckGauge(2.0f));
        gaugeList.Add(new CheckGauge(8.0f));
        gaugeList.Add(new CheckGauge(10.0f));
        gaugeList.Add(new CheckGauge(20.0f));

        aloyPoolObj = GameObject.Find("AloyPoolManager");
        if (aloyPoolObj == null) Debug.LogError("AloyPoolManager을 찾을 수 없습니다");

        gotoOccupationArea = new GotoOccupationArea
            (transform, occupationPoint, arrowAniObj);
        aloyBasicAttack = new AloyBasicAttack
            (transform, aimingTransform, bowAniObj, arrowAniObj, aloyPoolObj, gaugeList[0]);
        aloyPreciseShot = new AloyPreciseShot
            (transform, aimingTransform, bowAniObj, arrowAniObj, aloyPoolObj, gaugeList[1]);
        aloyPurifyBeam = new AloyPurifyBeam
            (transform, aimingTransform, bowAniObj, arrowAniObj, gaugeList[2]);
        aloyArrowStrike = new AloyArrowStrike
            (transform, arrowStrikeTransform, bowAniObj, arrowAniObj, aloyPoolObj, gaugeList[3]);
    }

    //Start()
    protected override Node SetupTree()
    {
        InitData();

        Node root = new Selector(new List<Node>
        {
            new CheckEnemy(
                new Parallel(new List<Node>
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

        root.SetDataToRoot("Status", "None");  
        return root;
    }

    protected override void Update()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            base.Update();
            CoolTimer();
            ShowState();
        }
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
        if (root.GetData("Enemy") == null) return;
        animator.SetLookAtWeight(1f, 0.9f);
        animator.SetLookAtPosition(((Transform)root.GetData("Enemy")).position);
        aimingTransform.LookAt(((Transform)root.GetData("Enemy")).position);
    }

    void ShowState()
    {
        Debug.Log("<color=orange>" + (string)root.GetData("Status") + "</color>");
    }
}
