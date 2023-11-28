using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;
using Photon.Pun;
using ExitGames.Client.Photon;
using System.Linq;
using HashTable = ExitGames.Client.Photon.Hashtable;
//using static UnityEditor.Progress;

public class Cultist : Character, IPunObservable
{
    public CultistData CultistData;
    public GameObject overlayCamera;
    public GameObject overlaySightRight;
    public GameObject overlaySightLeft;
    public GameObject minimapCamera;
    public GameObject aim;

    public GameObject dagger;
    public GameObject overlayDagger;

    public Animator overlayAnimator;

    public GameObject lungeEffect;
    public GameObject throwPosition;

    Vector3 defaultCameraLocalVec;

    //데이터 테이블 테스트 값
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

    float skill4Weight = 0f;

    float overlayLeftHandWeight = 0.4f;
    float overlayRightHandWeight = 0.4f;

    public enum SkillStateCultist
    {
        None, Invocation, Lunge, Throw, Skill1Ready, Skill1Casting, Skill1Using,
        Skill2Ready, Skill2Casting, Skill2Channeling,
        Skill3Ready, Skill3Casting, Skill3Channeling,
        Skill4Ready, Skill4Casting
    }

    SkillStateCultist skillState = SkillStateCultist.None;

    bool isClicked = false;
    bool isClicked2 = false;

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
        dataHp = CultistData.hp;
        hp = CultistData.hp;
        moveSpeed = CultistData.moveSpeed;
        backSpeed = CultistData.backSpeed;
        sideSpeed = CultistData.sideSpeed;
        runSpeedRatio = CultistData.runSpeedRatio;
        sitSpeed = CultistData.sitSpeed;
        sitSideSpeed = CultistData.sitSideSpeed;
        sitBackSpeed = CultistData.sitBackSpeed;
        jumpHeight = CultistData.jumpHeight;
        headShotRatio = CultistData.headShotRatio;

        invocationCastingTime = CultistData.invocationCastingTime;
        lungeHoldingTime = CultistData.lungeHoldingTime;
        lungeAttackTime = CultistData.lungeAttackTime;
        throwTime = CultistData.throwTime;
        skill1CastingTime = CultistData.skill1CastingTime;
        skill1Time = CultistData.skill1Time;
        skill2CastingTime = CultistData.skill2CastingTime;
        skill2ChannelingTime = CultistData.skill2ChannelingTime;
        skill3CastingTime = CultistData.skill3CastingTime;
        skill3ChannelingTime = CultistData.skill3ChannelingTime;
        skill4Time = CultistData.skill4Time;

        overlayDagger.SetActive(false);
    }

    protected override void Update()
    {
        base.Update();
        if (PhotonNetwork.IsMasterClient)
        {
            CheckCoolDownTime();
            UpdateData();
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

    [PunRPC]
    public override void IsLocalPlayer()
    {
        base.IsLocalPlayer();
        overlayCamera.SetActive(true);
        minimapCamera.SetActive(true);
        dagger.SetActive(false);
    }

    void CheckChanneling()
    {
        if(skillState == SkillStateCultist.Skill2Channeling
            || skillState == SkillStateCultist.Skill4Casting)
            moveVec = Vector3.zero;

        if (skillState == SkillStateCultist.Lunge)
        {
            moveVec = Vector3.zero;
            rigidbody.MovePosition(rigidbody.transform.position + transform.forward * moveSpeed * runSpeedRatio * Time.deltaTime);
            animator.SetInteger("VerticalSpeed", (int)moveVec.z);
            animator.SetInteger("HorizontalSpeed", (int)moveVec.x);
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
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Throw"))
            {
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.35f)
                {
                    if (overlayDagger.active == true)
                    {
                        Ray _tempRay = camera.GetComponent<Camera>().ScreenPointToRay(aim.transform.position);
                        Quaternion _tempQ = Quaternion.LookRotation(_tempRay.direction);
                        PhotonNetwork.Instantiate("ChanYoung/Prefabs/Cultist/Dagger", _tempRay.origin + _tempRay.direction * 0.5f, _tempQ);
                        overlayDagger.SetActive(false);
                    }
                }
            }

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
                globalChannelingTime = skill2ChannelingTime;
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
                globalChannelingTime = skill3ChannelingTime;
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
            isClicked2 = !isClicked2;
            if (isClicked2)
            {
                if (skillState == SkillStateCultist.Invocation)
                {
                    object[] _tempData = new object[2];
                    _tempData[0] = "ClickMouse2";
                    RequestRPCCall(_tempData);
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
        else if ((string)data[0] == "ClickMouse2")
            ClickMouse2();
        else if ((string)data[0] == "CancelHolding")
            CancelHolding();


    }
    void SetSkill(object[] data)
    {
        if (coolDownTime[(int)data[1] - 1] <= 0f)
        {
            {
                if (skillState == SkillStateCultist.Skill1Ready ||
                    skillState == SkillStateCultist.Skill2Ready ||
                    skillState == SkillStateCultist.Skill3Ready ||
                    skillState == SkillStateCultist.Skill4Ready ||
                    skillState == SkillStateCultist.None)
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
        }
    }

    void CancelSkill()
    {
        if (skillState == SkillStateCultist.Skill1Ready || skillState == SkillStateCultist.Skill2Ready
            || skillState == SkillStateCultist.Skill3Ready || skillState == SkillStateCultist.Skill4Ready
            || (skillState == SkillStateCultist.Invocation && globalCastingTime <= 0f))
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

            CallStopLunge();
        }
    }

    void CallStopLunge()
    {
        object[] _tempData = new object[2];
        _tempData[0] = "StopLunge";
        ResponseRPCCall(_tempData);
    }

    void ClickMouse()
    {
        if (skillState == SkillStateCultist.None)
        {
            globalCastingTime = invocationCastingTime;
            CallSetAnimation("isInvocation", true);
            skillState = SkillStateCultist.Invocation;
        }
        else if(skillState == SkillStateCultist.Invocation)
        {
            if(globalCastingTime <= 0f)
            {
                globalCastingTime = lungeHoldingTime;
                CallSetAnimation("isLunge", true);
                skillState = SkillStateCultist.Lunge;

                RunLungeEffect();
            }
        }
        else if(skillState == SkillStateCultist.Skill1Ready)
        {
            //스킬1 사용
            skillState = SkillStateCultist.Skill1Casting;
            globalCastingTime = skill1CastingTime;
            CallSetAnimation("isSkill1", true);
        }
        else if(skillState == SkillStateCultist.Skill2Ready)
        {
            //스킬2 사용
            skillState = SkillStateCultist.Skill2Casting;
            globalCastingTime = skill2CastingTime;
            CallSetAnimation("isSkill2", true);
        }
        else if (skillState == SkillStateCultist.Skill3Ready)
        {
            //스킬3 사용
            skillState = SkillStateCultist.Skill3Casting;
            globalCastingTime = skill3CastingTime;
            CallSetAnimation("isSkill3", true);
        }
        else if (skillState == SkillStateCultist.Skill4Ready)
        {
            //스킬4 사용
            skillState = SkillStateCultist.Skill4Casting;
            globalCastingTime = skill4Time;
            CallSetAnimation("isSkill4", true);
        }
    }

    void ClickMouse2()
    {
        if (skillState == SkillStateCultist.Invocation)
        {
            if (globalCastingTime <= 0f)
            {
                globalCastingTime = throwTime;
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
        else if ((string)data[0] == "StopLunge")
            StopLunge();
    }

    void StopLunge()
    {
        /*
        for (int i = 0; i < 5; i++)
        {
            lungeEffect.transform.GetChild(i).GetComponent<ParticleSystem>().Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        */
    }

    void RunLungeEffect()
    {
        for(int i = 0; i < 5; i++)
        {
            lungeEffect.transform.GetChild(i).GetComponent<ParticleSystem>().Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            lungeEffect.transform.GetChild(i).GetComponent<ParticleSystem>().startLifetime = lungeHoldingTime + lungeAttackTime;
            lungeEffect.transform.GetChild(i).GetComponent<ParticleSystem>().Play(false);
        }
    }

    void CheckAnimationSpeed()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Invocation"))
            SetAnimationSpeed("InvocationSpeed", invocationCastingTime);
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("LungeHolding"))
            SetAnimationSpeed("LungeHoldingSpeed", lungeHoldingTime);
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("LungeAttack"))
            SetAnimationSpeed("LungeAttackSpeed", lungeAttackTime);
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Throw"))
            SetAnimationSpeed("ThrowSpeed", throwTime);
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Skill1Casting"))
            SetAnimationSpeed("Skill1CastingSpeed", skill1CastingTime);
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Skill1"))
            SetAnimationSpeed("Skill1Speed", skill1Time);
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Skill2"))
            SetAnimationSpeed("Skill2CastingSpeed", skill2CastingTime);
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Skill3Casting"))
            SetAnimationSpeed("Skill3CastingSpeed", skill3CastingTime);
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Skill3"))
            SetAnimationSpeed("Skill3Speed", skill3ChannelingTime);
        else if (animator.GetCurrentAnimatorStateInfo(4).IsName("Skill4"))
            SetAnimationSpeedExtra("Skill4CastingSpeed", skill4Time);
        
    }

    void SetAnimationSpeed(string state, float animationTime)
    {
        float _animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
        float _normalizedSpeed = _animationLength / animationTime;
        animator.SetFloat(state, _normalizedSpeed);
        overlayAnimator.SetFloat(state, _normalizedSpeed);
    }

    void SetAnimationSpeedExtra(string state, float animationTime)
    {
        float _animationLength = animator.GetCurrentAnimatorStateInfo(4).length;
        float _normalizedSpeed = _animationLength / animationTime;
        animator.SetFloat(state, _normalizedSpeed);
        overlayAnimator.SetFloat(state, _normalizedSpeed);
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

            overlayAnimator.SetBool("isSkill1", false);
            overlayAnimator.SetBool("isSkill2", false);
            overlayAnimator.SetBool("isSkill3", false);
            overlayAnimator.SetBool("isSkill4", false);
            overlayAnimator.SetBool("isLunge", false);
            overlayAnimator.SetBool("isThrow", false);
            overlayAnimator.SetBool("isInvocation", false);
        }
    }

    void SetAnimation(object[] data)
    {
        if(photonView.IsMine)
        {
            animator.SetBool((string)data[1], (bool)data[2]);
            overlayAnimator.SetBool((string)data[1], (bool)data[2]);
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
        if (skillState == SkillStateCultist.Skill4Casting)
            skill4Weight = Mathf.Lerp(skill4Weight, 1f, Time.deltaTime * 8);
        else
            skill4Weight = Mathf.Lerp(skill4Weight, 0f, Time.deltaTime * 8);
        animator.SetLayerWeight(4, skill4Weight);

        //돌진 특수
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("LungeAttack"))
        {
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f)
                overlayDagger.SetActive(false);
        }
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Invocation"))
        {
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f)
                overlayDagger.SetActive(true);
        }
    }

    protected override void OnAnimatorIK()
    {
        base.OnAnimatorIK();

        if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Idle"))
            LerpWeight(0.5f);
        else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Invocation"))
            LerpWeight(0f);
        else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("LungeHolding"))
            LerpWeight(0.2f);
        else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("LungeAttack"))
            LerpWeight(0.25f);
        else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Throw"))
            LerpWeight(0f);
        else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Skill1Casting"))
            LerpWeight(0f);
        else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Skill1"))
            LerpWeight(0.7f);
        else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Skill2"))
            LerpWeight(0.2f);
        else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Skill3Casting"))
            LerpWeight(0.2f);
        else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Skill3"))
            LerpWeight(0.2f);
        else if(overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Skill4"))
            LerpWeight(0f);

        overlayAnimator.SetIKPosition(AvatarIKGoal.LeftHand, overlaySightLeft.transform.position);
        overlayAnimator.SetIKPosition(AvatarIKGoal.RightHand, overlaySightRight.transform.position);
        overlayAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, overlayLeftHandWeight);
        overlayAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, overlayRightHandWeight);
    }

    void LerpWeight(float weight)
    {
        overlayLeftHandWeight = Mathf.Lerp(overlayLeftHandWeight, weight, Time.deltaTime * 8f);
        overlayRightHandWeight = Mathf.Lerp(overlayRightHandWeight, weight, Time.deltaTime * 8f);
    }
}
