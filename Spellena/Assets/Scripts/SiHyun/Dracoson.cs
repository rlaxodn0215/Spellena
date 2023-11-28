using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;
using Photon.Pun;
using ExitGames.Client.Photon;
using System.Linq;
using HashTable = ExitGames.Client.Photon.Hashtable;

public class Dracoson : Character, IPunObservable
{
    public DracosonData dracosonData;
    public GameObject overlaycamera;
    public GameObject Aim;
    public Animator overlayAnimator;

    Vector3 defaultCameraLocalVec;

    float invocationCastingTime;
    float lungeHoldingTime;
    float lungeAttackTime;
    float throwTime;
    float skill1CastingTime;
    float skill1Time;
    float skill2CastingTime;
    float skill2ChannelingTime;
    float skill3CastingTime;
    float skill3ChannelingTime;
    float skill4Time;

    public enum SkillStateDracoson
    {
        None, Holding, Breathe, Flying, Skill1, Skill2, Skill3, Skill4
    }

    SkillStateDracoson skillState = SkillStateDracoson.None;

    bool isClicked = false;

    float[] coolDownTime = new float[4];
    float globalCastingTime = 0f;
    float holdingTime = 0f;
    float holdingStartTime = 0f;

    protected override void Awake()
    {
        base.Awake();
        if (photonView.IsMine)
        {
            //테스트 정보
            HashTable _tempTable = new HashTable();
            _tempTable.Add("CharacterViewID", photonView.ViewID);
            _tempTable.Add("IsAlive", true);
            PhotonNetwork.LocalPlayer.SetCustomProperties(_tempTable);
        }
    }
    void Init()
    {
        defaultCameraLocalVec = camera.transform.localPosition;
        dataHp = dracosonData.hp;
        hp = dracosonData.hp;
        moveSpeed = dracosonData.moveSpeed;
        backSpeed = dracosonData.backSpeed;
        sideSpeed = dracosonData.sideSpeed;
        runSpeedRatio = dracosonData.runSpeedRatio;
        sitSpeed = dracosonData.sitSpeed;
        sitSideSpeed = dracosonData.sitSideSpeed;
        sitBackSpeed = dracosonData.sitBackSpeed;
        jumpHeight = dracosonData.jumpHeight;
        headShotRatio = dracosonData.headShotRatio;

        invocationCastingTime = dracosonData.invocationCastingTime;
        lungeHoldingTime = dracosonData.lungeHoldingTime;
        lungeAttackTime = dracosonData.lungeAttackTime;
        throwTime = dracosonData.throwTime;
        skill1CastingTime = dracosonData.skill1CastingTime;
        skill1Time = dracosonData.skill1Time;
        skill2CastingTime = dracosonData.skill2CastingTime;
        skill2ChannelingTime = dracosonData.skill2ChannelingTime;
        skill3CastingTime = dracosonData.skill3CastingTime;
        skill3ChannelingTime = dracosonData.skill3ChannelingTime;
        skill4Time = dracosonData.skill4Time;

        overlayAnimator.fireEvents = false;
    }

    protected override void Start()
    {
        base.Start();
        Init();
    }

    protected override void Update()
    {
        base.Update();
        if (PhotonNetwork.IsMasterClient)
        {
            //CheckCoolDownTime();
            UpdateData();
            Debug.Log(skillState);
            CheckChargePhase();
        }

        if (photonView.IsMine)
        {
            CheckAnimator();
        }
    }

    void CheckCoolDownTime()
    {
        for (int i = 0; i < 4; i++)
        {
            if (coolDownTime[i] > 0f)
                coolDownTime[i] -= Time.deltaTime;
        }
        globalCastingTime -= Time.deltaTime;
        if (skillState == SkillStateDracoson.Holding)
        {
            if (globalCastingTime <= 0f)
            {
                CallSetAnimation("isHolding", false);
                skillState = SkillStateDracoson.None;
            }
        }
    }

    void CheckChargePhase()
    {
        if (isClicked)
        {
            CallSetAnimation("HoldingCancel", false);
            holdingTime = Time.time - holdingStartTime;

            float _chargePhase1 = 1.3f;
            float _chargePhase2 = 2.0f;
            float _chargePhase3 = 3.0f;
            float _chargePhaseOver = 4.0f;

            if (holdingTime >= _chargePhase1 && holdingTime < _chargePhase2)
            {
                Debug.Log("1단계 차지");
                chargeCount = 1;
            }
            else if (holdingTime >= _chargePhase2 && holdingTime < _chargePhase3)
            {
                Debug.Log("2단계 차지");
                chargeCount = 2;
            }
            else if (holdingTime >= _chargePhase3 && holdingTime < _chargePhaseOver)
            {
                Debug.Log("3단계 차지");
                chargeCount = 3;
            }
            else if (holdingTime >= _chargePhaseOver)
            {
                Debug.Log("오버 차지");
                chargeCount = 4;
            }
            else
            {
                chargeCount = 0;
                Debug.Log("차지 실패");
            }
        }
    }

    void UpdateData()
    {
        object[] _tempData = new object[3];
        _tempData[0] = "UpdateData";
        _tempData[1] = coolDownTime;
        _tempData[2] = skillState;
        photonView.RPC("CallRPCCulTistToAll", RpcTarget.AllBuffered, _tempData);
    }

    void OnSkill1()
    {
        if (photonView.IsMine)
            CallSkill(1);
    }

    void OnSkill2()
    {
        if (photonView.IsMine)
            CallSkill(2);
    }

    void OnSkill3()
    {
        if (photonView.IsMine)
            CallSkill(3);
    }

    void OnSkill4()
    {
        if (photonView.IsMine)
            CallSkill(4);
    }

    void CallSkill(int num)
    {
        object[] _tempObject = new object[2];
        _tempObject[0] = "SetSkill";
        _tempObject[1] = num;
        RequestRPCCall(_tempObject);
    }

    void OnButtonCancel()
    {
        if (photonView.IsMine)
        {
            object[] _tempData = new object[2];
            _tempData[0] = "CancelSkill";
            RequestRPCCall(_tempData);
        }
    }

    void OnMouseButton()
    {
        if (photonView.IsMine)
        {
            isClicked = !isClicked;
            if (isClicked)
            {
                object[] _tempData = new object[2];
                _tempData[0] = "ClickMouse";
                RequestRPCCall(_tempData);
                Debug.Log("마우스 좌 클릭");
                holdingStartTime = Time.time;
            }
            else
            {
                if (skillState == SkillStateDracoson.Holding)
                {
                    object[] _tempData = new object[2];
                    _tempData[0] = "CancelHolding";
                    RequestRPCCall(_tempData);
                    Debug.Log("마우스 좌 클릭 취소");
                }
            }
        }
    }

    //마스터 클라이언트로 요청
    void RequestRPCCall(object[] data)
    {
        photonView.RPC("CallRPCCultistMasterClient", RpcTarget.MasterClient, data);
    }

    [PunRPC]
    public void CallRPCCultistMasterClient(object[] data)
    {
        if ((string)data[0] == "SetSkill")
            SetSkill(data);
        else if ((string)data[0] == "CancelSkill")
            CancelSkill();
        else if ((string)data[0] == "ClickMouse")
            ClickMouse();
        else if ((string)data[0] == "CancelHolding")
            CancelHolding();


    }
    void SetSkill(object[] data)
    {
        if (coolDownTime[(int)data[1] - 1] <= 0f)
        {
            if ((int)data[1] == 1)
                skillState = SkillStateDracoson.Skill1;
            else if ((int)data[1] == 2)
                skillState = SkillStateDracoson.Skill2;
            else if ((int)data[1] == 3)
                skillState = SkillStateDracoson.Skill3;
            else if ((int)data[1] == 4)
                skillState = SkillStateDracoson.Skill4;
        }
    }

    void CancelSkill()
    {
        skillState = SkillStateDracoson.None;
    }

    void CancelHolding()
    {
        //홀딩 캔슬
        if (skillState == SkillStateDracoson.Holding)
        {
            if (chargeCount == 0)
            {
                CallSetAnimation("HoldingCancel", true);
            }
            globalCastingTime = 0f;
            CallSetAnimation("isHolding", false);
            skillState = SkillStateDracoson.None;
            holdingStartTime = 0f;
            holdingTime = 0f;
            Debug.Log("홀딩 취소");
        }
    }

    void ClickMouse()
    {
        if (skillState == SkillStateDracoson.None)
        {
            // 용의 시선
            globalCastingTime = 1f;
            CallSetAnimation("isHolding", true);
            skillState = SkillStateDracoson.Holding;
        }
        else if (skillState == SkillStateDracoson.Skill1)
        {
            //스킬1 사용
            coolDownTime[0] = 5f;
            CallSetAnimation("Skill1", true);
        }
        else if (skillState == SkillStateDracoson.Skill2)
        {
            //스킬2 사용
            coolDownTime[1] = 5f;
        }
        else if (skillState == SkillStateDracoson.Skill3)
        {
            //스킬3 사용
            coolDownTime[2] = 5f;
        }
        else if (skillState == SkillStateDracoson.Skill4)
        {
            //스킬4 사용
            coolDownTime[3] = 5f;
        }
    }

    void CallSetAnimation(string parameter, bool isParameter)
    {
        object[] _tempData = new object[3];
        _tempData[0] = "SetAnimation";
        _tempData[1] = parameter;
        _tempData[2] = isParameter;
        ResponseRPCCall(_tempData);
    }

    //마스터 클라이언트가 모든 클라이언트에게
    void ResponseRPCCall(object[] data)
    {
        photonView.RPC("CallRPCCulTistToAll", RpcTarget.AllBuffered, data);
    }

    [PunRPC]
    public void CallRPCCulTistToAll(object[] data)
    {
        if ((string)data[0] == "UpdateData")
            UpdateDataByMasterClient(data);
        else if ((string)data[0] == "SetAnimation")
            SetAnimation(data);
    }

    void SetAnimation(object[] data)
    {
        if (photonView.IsMine)
        {
            animator.SetBool((string)data[1], (bool)data[2]);
        }
    }

    void UpdateDataByMasterClient(object[] data)
    {
        coolDownTime = (float[])data[1];
        skillState = (SkillStateDracoson)data[2];
    }

    void CheckAnimator()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Invocation"))
        {
            animator.SetBool("isInvocation", false);
        }
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Throw"))
        {
            animator.SetBool("isThrow", false);
        }
    }
}
