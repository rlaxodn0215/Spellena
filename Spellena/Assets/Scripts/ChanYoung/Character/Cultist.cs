using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;
using Photon.Pun;
using ExitGames.Client.Photon;
using System.Linq;
using HashTable = ExitGames.Client.Photon.Hashtable;

public class Cultist : Character, IPunObservable
{
    public GameObject overlaycamera;
    public GameObject Aim;
    public Animator overlayAnimator;

    Vector3 defaultCameraLocalVec;

    //데이터 테이블 테스트 값
    float skill1CastingTime = 1.5f;
    float skill2CastingTime = 2f;
    float skill3CastingTime = 1.5f;

    public enum SkillStateCultist
    {
        None, Invocation, Lunge, Throw, Skill1Ready, Skill1Casting, Skill1Using,
        Skill2Ready, Skill2Casting, Skill2Channeling,
        Skill3Ready, Skill3Casting, Skill3Channeling,
        Skill4Ready, Skill4Casting
    }

    SkillStateCultist skillState = SkillStateCultist.None;

    bool isClicked = false;

    float[] coolDownTime = new float[4];
    float globalCastingTime = 0f;
    float globalChannelingTime = 0f;

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

    protected override void Start()
    {
        base.Start();
        Init();
    }

    void Init()
    {
        defaultCameraLocalVec = camera.transform.localPosition;
    }

    protected override void Update()
    {
        base.Update();
        if (PhotonNetwork.IsMasterClient)
        {
            CheckCoolDownTime();
            UpdateData();
            Debug.Log(skillState);
        }

        if(photonView.IsMine)
        {
            CheckAnimationSpeed();
            CheckAnimatorExtra();
        }
    }

    protected override void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            CheckChanneling();
        }
        base.FixedUpdate();
    }

    void CheckChanneling()
    {
        if(skillState == SkillStateCultist.Skill2Channeling)
        {
            moveVec = Vector3.zero;
        }
    }

    void CheckCoolDownTime()
    {
        for (int i = 0; i < 4; i++)
        {
            if (coolDownTime[i] > 0f)
                coolDownTime[i] -= Time.deltaTime;
        }

        if(globalCastingTime > 0f)
            globalCastingTime -= Time.deltaTime;
        if (globalChannelingTime > 0f)
            globalChannelingTime -= Time.deltaTime;

        if(skillState == SkillStateCultist.Invocation)
        {
            if(globalCastingTime <= 0f)
                CallResetAnimation();
        }
        else if (skillState == SkillStateCultist.Lunge || skillState == SkillStateCultist.Throw)
        {
            if (globalCastingTime <= 0f)
            {
                CallResetAnimation();
                skillState = SkillStateCultist.None;
            }
        }
        else if(skillState == SkillStateCultist.Skill1Casting)
        {
            if(globalCastingTime <= 0f)
            {
                CallResetAnimation();
                skillState = SkillStateCultist.Skill1Using;
                coolDownTime[0] = 5f;
            }
        }
        else if(skillState == SkillStateCultist.Skill1Using)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                skillState = SkillStateCultist.None;
        }
        else if(skillState == SkillStateCultist.Skill2Casting)
        {
            if(globalCastingTime <= 0f)
            {
                globalChannelingTime = 5f;
                skillState = SkillStateCultist.Skill2Channeling;
            }
        }
        else if(skillState == SkillStateCultist.Skill2Channeling)
        {
            if (globalChannelingTime <= 0f)
            {
                CallResetAnimation();
                skillState = SkillStateCultist.None;
                coolDownTime[1] = 5f;
            }
        }
        else if(skillState == SkillStateCultist.Skill3Casting)
        {
            if(globalCastingTime <= 0f)
            {
                globalChannelingTime = 5f;
                skillState = SkillStateCultist.Skill3Channeling;
            }
        }
        else if(skillState == SkillStateCultist.Skill3Channeling)
        {
            if(globalChannelingTime <= 0f)
            {
                CallResetAnimation();
                skillState = SkillStateCultist.None;
                coolDownTime[2] = 5f;
            }
        }
        else if(skillState == SkillStateCultist.Skill4Casting)
        {
            if(globalCastingTime <= 0f)
            {
                CallResetAnimation();
                skillState = SkillStateCultist.None;
                coolDownTime[3] = 10f;
            }
        }
    }

    void UpdateData()
    {
        object[] _tempData = new object[5];
        _tempData[0] = "UpdateData";
        _tempData[1] = coolDownTime;
        _tempData[2] = skillState;
        _tempData[3] = globalCastingTime;
        _tempData[4] = globalChannelingTime;
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
            }
            else
            {
                if(skillState == SkillStateCultist.Lunge)
                {
                    object[] _tempData = new object[2];
                    _tempData[0] = "CancelHolding";
                    RequestRPCCall(_tempData);
                }
            }
        }
    }

    void OnMouseButton2()
    {
        if(photonView.IsMine)
        {
            if(skillState == SkillStateCultist.Invocation)
            {
                object[] _tempData = new object[2];
                _tempData[0] = "ClickMouse2";
                RequestRPCCall(_tempData);
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
        else if ((string)data[0] == "ClickMouse2")
            ClickMouse2();
        else if ((string)data[0] == "CancelHolding")
            CancelHolding();


    }
    void SetSkill(object[] data)
    {
        if (coolDownTime[(int)data[1] - 1] <= 0f)
        {
            if ((int)data[1] == 1)
                skillState = SkillStateCultist.Skill1Ready;
            else if ((int)data[1] == 2)
                skillState = SkillStateCultist.Skill2Ready;
            else if ((int)data[1] == 3)
                skillState = SkillStateCultist.Skill3Ready;
            else if ((int)data[1] == 4)
                skillState = SkillStateCultist.Skill4Ready;
        }
    }

    void CancelSkill()
    {
        if (skillState == SkillStateCultist.Skill1Ready || skillState == SkillStateCultist.Skill2Ready
            || skillState == SkillStateCultist.Skill3Ready || skillState == SkillStateCultist.Skill4Ready)
        {
            skillState = SkillStateCultist.None;
        }
    }

    void CancelHolding()
    {
        //홀딩 캔슬
        if (skillState == SkillStateCultist.Lunge)
        {
            globalCastingTime = 0f;
            CallSetAnimation("isLunge", false);
            skillState = SkillStateCultist.None;
        }
    }

    void ClickMouse()
    {
        if (skillState == SkillStateCultist.None)
        {
            globalCastingTime = 1f;
            CallSetAnimation("isInvocation", true);
            skillState = SkillStateCultist.Invocation;
        }
        else if(skillState == SkillStateCultist.Invocation)
        {
            if(globalCastingTime <= 0f)
            {
                globalCastingTime = 1.5f;
                CallSetAnimation("isLunge", true);
                skillState = SkillStateCultist.Lunge;
            }
        }
        else if(skillState == SkillStateCultist.Skill1Ready)
        {
            //스킬1 사용
            skillState = SkillStateCultist.Skill1Casting;
            globalCastingTime = 1.5f;
            CallSetAnimation("isSkill1", true);
        }
        else if(skillState == SkillStateCultist.Skill2Ready)
        {
            //스킬2 사용
            skillState = SkillStateCultist.Skill2Casting;
            globalCastingTime = 2f;
            CallSetAnimation("isSkill2", true);
        }
        else if (skillState == SkillStateCultist.Skill3Ready)
        {
            //스킬3 사용
            skillState = SkillStateCultist.Skill3Casting;
            globalCastingTime = 1.5f;
            CallSetAnimation("isSkill3", true);
        }
        else if (skillState == SkillStateCultist.Skill4Ready)
        {
            //스킬4 사용
            skillState = SkillStateCultist.Skill4Casting;
            globalCastingTime = 10f;
            CallSetAnimation("isSkill4", true);
        }
    }

    void ClickMouse2()
    {
        if (skillState == SkillStateCultist.Invocation)
        {
            if (globalCastingTime <= 0f)
            {
                globalCastingTime = 1.5f;
                CallSetAnimation("isThrow", true);
                skillState = SkillStateCultist.Throw;
            }
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

    void CallResetAnimation()
    {
        object[] _tempData = new object[2];
        _tempData[0] = "ResetAnimation";
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
        else if ((string)data[0] == "ResetAnimation")
            ResetAnimation();
    }

    void CheckAnimationSpeed()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Skill1Casting"))
            SetAnimationSpeed("Skill1CastingSpeed");
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Skill2"))
            SetAnimationSpeed("Skill2CastingSpeed");
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Skill3"))
            SetAnimationSpeed("Skill3CastingSpeed");
    }

    void SetAnimationSpeed(string state)
    {
        float _animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
        float _normalizedSpeed = _animationLength / skill1CastingTime;
        animator.SetFloat(state, _normalizedSpeed);
    }

    void ResetAnimation()
    {
        if(photonView.IsMine)
        {
            animator.SetBool("isSkill1", false);
            animator.SetBool("isSkill2", false);
            animator.SetBool("isSkill3", false);
            animator.SetBool("isSkill4", false);
            animator.SetBool("isLunge", false);
            animator.SetBool("isThrow", false);
            animator.SetBool("isInvocation", false);
        }
    }

    void SetAnimation(object[] data)
    {
        if(photonView.IsMine)
        {
            animator.SetBool((string)data[1], (bool)data[2]);
        }
    }

    void UpdateDataByMasterClient(object[] data)
    {
        coolDownTime = (float[])data[1];
        skillState = (SkillStateCultist)data[2];
        globalCastingTime = (float)data[3];
        globalChannelingTime = (float)data[4];
    }

    void CheckAnimatorExtra()
    {
        if(skillState == SkillStateCultist.Skill4Casting)
        {
            animator.SetLayerWeight(4, 1);
        }
        else
        {
            animator.SetLayerWeight(4, 0);
        }
    }

    protected override void OnAnimatorIK()
    {
        base.OnAnimatorIK();
    }
}
