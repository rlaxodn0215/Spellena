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

    int ownerNum;

    Vector3 defaultCameraLocalVec;

    //데이터 테이블 테스트 값
    float invocationCastingTime;
    float lungeHoldingTime;
    float lungeAttackTime;
    float throwCastingTime;
    float skill1CastingTime;
    float skill1ChannelingTime;
    float skill2CastingTime;
    float skill2ChannelingTime;
    float skill3CastingTime;
    float skill3ChannelingTime;
    float skill4CastingTime;

    float skill4Weight = 0f;

    float overlayLeftHandWeight = 0.4f;
    float overlayRightHandWeight = 0.4f;

    public enum SkillStateCultist
    {
        None, Invocation, 
        LungeHolding, LungeAttack,
        Throw,
        Skill1Ready, Skill1Casting, Skill1Channeling,
        Skill2Ready, Skill2Casting, Skill2Channeling,
        Skill3Ready, Skill3Casting, Skill3Channeling,
        Skill4Ready, Skill4Casting
    }

    SkillStateCultist skillState = SkillStateCultist.None;

    //0 : 스킬1, 1 : 스킬2, 2 : 스킬3, 3 : 스킬4
    float[] skillCoolDownTime = new float[4];
    float[] skillCastingTime = new float[4];
    float[] skillChannelingTime = new float[4];

    //0 : 의식, 1 : 급습 홀딩, 2 : 급습 공격, 3 : 투척
    float[] normalCastingTime = new float[4];

    //0 : 왼쪽 마우스, 1 : 오른쪽 마우스
    bool[] isClicked = new bool[2];

    bool isDaggerOn = false;

    float globalCastingTime = 0f;
    float globalChannelingTime = 0f;

    Vector3 aimPos;
    Vector3 aimDirection;

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


            object[] _tempData = new object[2];
            _tempData[0] = "SetOwnerNum";
            _tempData[1] = photonView.OwnerActorNr;
            RequestRPCCall(_tempData);
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
        throwCastingTime = CultistData.throwTime;
        skill1CastingTime = CultistData.skill1CastingTime;
        skill1ChannelingTime = CultistData.skill1Time;
        skill2CastingTime = CultistData.skill2CastingTime;
        skill2ChannelingTime = CultistData.skill2ChannelingTime;
        skill3CastingTime = CultistData.skill3CastingTime;
        skill3ChannelingTime = CultistData.skill3ChannelingTime;
        skill4CastingTime = CultistData.skill4Time;
    }

    [PunRPC]
    public override void IsLocalPlayer()
    {
        base.IsLocalPlayer();
        overlayCamera.SetActive(true);
        minimapCamera.SetActive(true);
        dagger.SetActive(false);

        dagger.layer = 6;
        for (int i = 0; i < 5; i++)
        {
            lungeEffect.transform.GetChild(i).gameObject.layer = 6;
        }
    }

    protected override void Update()
    {
        base.Update();
        CheckCoolDownTimeForAll();

        if (PhotonNetwork.IsMasterClient)
            CheckOnMasterClient();

        if(photonView.IsMine)
        {
            CheckAnimationSpeed();
            CheckAnimatorExtra();
        }
    }

    protected override void FixedUpdate()
    {
        if (photonView.IsMine)
            CheckChanneling();
        base.FixedUpdate();
    }

    void CheckChanneling()
    {
        if (skillState == SkillStateCultist.Skill2Channeling
            || skillState == SkillStateCultist.Skill4Casting)
            moveVec = Vector3.zero;

        if (skillState == SkillStateCultist.LungeHolding)
        {
            moveVec = Vector3.zero;
            rigidbody.MovePosition(rigidbody.transform.position + transform.forward * moveSpeed * runSpeedRatio * Time.deltaTime);
            animator.SetInteger("VerticalSpeed", (int)moveVec.z);
            animator.SetInteger("HorizontalSpeed", (int)moveVec.x);
        }
    }



    //모든 클라이언트에서 작동
    void CheckCoolDownTimeForAll()
    {
        CheckCoolDownTimeLoop(ref skillCoolDownTime);
        CheckCoolDownTimeLoop(ref skillCastingTime);
        CheckCoolDownTimeLoop(ref skillChannelingTime);
        CheckCoolDownTimeLoop(ref normalCastingTime);
    }

    void CheckCoolDownTimeLoop(ref float[] times)
    {
        for(int i = 0; i < times.Length; i++)
        {
            if (times[i] > 0f)
                times[i] -= Time.deltaTime;
        }
    }

    //로컬 클라이언트에서 작동

    //마스터 클라이언트에서만 작동
    void CheckOnMasterClient()
    {
        if (skillState == SkillStateCultist.Invocation)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Invocation"))
            {
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f)
                    CallSetDagger(true);
            }

            if (normalCastingTime[0] <= 0f)
            {
                CallResetAnimation();
                CallSetData("OnlySkillState", 0, 0f);
            }
        }
        else if (skillState == SkillStateCultist.LungeHolding)
        {
            if (normalCastingTime[1] <= 0f)
            {
                CallResetAnimation();
                skillState = SkillStateCultist.LungeAttack;
                CallSetData("normalCastingTime", 2, lungeAttackTime);
            }
        }
        else if (skillState == SkillStateCultist.LungeAttack)
        {
            if (normalCastingTime[2] <= 0f)
            {
                CallResetAnimation();
                skillState = SkillStateCultist.None;
                CallSetDagger(false);
                CallSetData("OnlySkillState", 0, 0f);
            }
        }
        else if (skillState == SkillStateCultist.Throw)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Throw"))
            {
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.35f)
                {
                    if (dagger.active == true)
                    {
                        dagger.SetActive(false);
                        CallInstantiateObject("Dagger");
                        CallSetDagger(false);
                    }
                }

                if (normalCastingTime[3] <= 0f)
                {
                    skillState = SkillStateCultist.None;
                    CallResetAnimation();
                    CallSetData("OnlySkillState", 0, 0f);
                }
            }
        }
        else if (skillState == SkillStateCultist.Skill1Casting)
        {
            if (skillCastingTime[0] <= 0f)
            {
                skillState = SkillStateCultist.Skill1Channeling;
                CallResetAnimation();
                CallSetData("skillChannelingTime", 0, skill1ChannelingTime);
            }
        }
        else if (skillState == SkillStateCultist.Skill1Channeling)
        {
            if (skillChannelingTime[0] <= 0f)
            {
                skillState = SkillStateCultist.None;
                CallSetData("OnlySkillState", 0, 0f);
                //쿨타임 적용
            }
        }
        else if (skillState == SkillStateCultist.Skill2Casting)
        {
            if (skillCastingTime[1] <= 0f)
            {
                skillState = SkillStateCultist.Skill2Channeling;
                CallSetData("skillChannelingTime", 1, skill2ChannelingTime);
            }
        }
        else if (skillState == SkillStateCultist.Skill2Channeling)
        {
            if (skillChannelingTime[1] <= 0f)
            {
                skillState = SkillStateCultist.None;
                CallResetAnimation();
                CallSetData("OnlySkillState", 0, 0f);
                //쿨타임 적용
            }
        }
        else if (skillState == SkillStateCultist.Skill3Casting)
        {
            if (skillCastingTime[2] <= 0f)
            {
                skillState = SkillStateCultist.Skill3Channeling;
                CallSetData("skillChannelingTime", 2, skill3ChannelingTime);
            }
        }
        else if (skillState == SkillStateCultist.Skill3Channeling)
        {
            if (skillChannelingTime[2] <= 0f)
            {
                skillState = SkillStateCultist.None;
                CallResetAnimation();
                CallSetData("OnlySkillState", 0, 0f);
                //쿨타임 적용
            }
        }
        else if (skillState == SkillStateCultist.Skill4Casting)
        {
            if (skillCastingTime[3] <= 0f)
            {
                skillState = SkillStateCultist.None;
                CallResetAnimation();
                CallSetData("OnlySkillState", 0, 0f);
                //쿨타임 적용
            }
        }
    }

    //입력 이벤트
    void OnSkill1()
    {
        if (photonView.IsMine)
            CallSetSkill(1);
    }

    void OnSkill2()
    {
        if (photonView.IsMine)
            CallSetSkill(2);
    }

    void OnSkill3()
    {
        if (photonView.IsMine)
            CallSetSkill(3);
    }

    void OnSkill4()
    {
        if (photonView.IsMine)
            CallSetSkill(4);
    }

    void OnMouseButton()
    {
        if (photonView.IsMine)
        {
            if (!isClicked[0])
            {
                object[] _tempData = new object[2];
                _tempData[0] = "ClickMouse";
                _tempData[1] = 0;
                RequestRPCCall(_tempData);
            }
            else
            {
                object[] _tempData = new object[2];
                _tempData[0] = "CancelHolding";
                RequestRPCCall(_tempData);
            }
            isClicked[0] = !isClicked[0];
        }
    }

    void OnMouseButton2()
    {
        if (photonView.IsMine)
        {
            if (!isClicked[1])
            {
                object[] _tempData = new object[2];
                _tempData[0] = "ClickMouse";
                _tempData[1] = 1;
                RequestRPCCall(_tempData);
            }
            isClicked[1] = !isClicked[1];
        }
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


    //요청 및 응답
    void RequestRPCCall(object[] data)
    {
        photonView.RPC("CallRPCCultistToMasterClient", RpcTarget.MasterClient, data);
    }

    [PunRPC]
    public void CallRPCCultistToMasterClient(object[] data)
    {
        if ((string)data[0] == "SetSkill")
            SetSkill(data);
        else if ((string)data[0] == "ClickMouse")
            ClickMouse(data);
        else if ((string)data[0] == "CancelHolding")
            CancelHolding();
        else if ((string)data[0] == "CancelSkill")
            CancelSkill();
        else if ((string)data[0] == "SetOwnerNum")
            ResponseRPCCall(data);
    }

    void ResponseRPCCall(object[] data)
    {
        photonView.RPC("CallRPCCulTistToAll", RpcTarget.AllBuffered, data);
    }

    [PunRPC]
    public void CallRPCCulTistToAll(object[] data)
    {
        if ((string)data[0] == "UpdateData")
            UpdateData(data);
        if ((string)data[0] == "SetAnimation")
            SetAnimation(data);
        else if ((string)data[0] == "SetOwnerNum")
            SetOwnerNum(data);
        else if ((string)data[0] == "SetDagger")
            SetDagger(data);
        else if ((string)data[0] == "ResetAnimation")
            ResetAnimation();
        else if ((string)data[0] == "InstantiateObject")
            InstantiateObject(data);

    }

    //요청
    void CallSetSkill(int num)
    {
        object[] _tempObject = new object[2];
        _tempObject[0] = "SetSkill";
        _tempObject[1] = num;
        RequestRPCCall(_tempObject);
    }


    //요청 처리
    void SetSkill(object[] data)
    {
        int _skillNum = (int)data[1];
        if (skillCoolDownTime[_skillNum - 1] <= 0f)
        {
            {
                if (skillState == SkillStateCultist.Skill1Ready ||
                    skillState == SkillStateCultist.Skill2Ready ||
                    skillState == SkillStateCultist.Skill3Ready ||
                    skillState == SkillStateCultist.Skill4Ready ||
                    skillState == SkillStateCultist.None ||
                    skillState == SkillStateCultist.Invocation)
                {
                    if (_skillNum == 1)
                        skillState = SkillStateCultist.Skill1Ready;
                    else if (_skillNum == 2)
                        skillState = SkillStateCultist.Skill2Ready;
                    else if (_skillNum == 3)
                        skillState = SkillStateCultist.Skill3Ready;
                    else if (_skillNum == 4)
                        skillState = SkillStateCultist.Skill4Ready;
                }
            }
        }
    }
    void ClickMouse(object[] data)
    {
        // 0은 왼쪽 1은 오른쪽
        int mouseCode = (int)data[1];
        if(mouseCode == 0)
        {
            if (skillState == SkillStateCultist.None)
            {
                //의식
                skillState = SkillStateCultist.Invocation;
                CallSetAnimation("isInvocation", true);
                CallSetData("normalCastingTime", 0, invocationCastingTime);
            }
            else if(skillState == SkillStateCultist.Invocation)
            {
                //급습
                skillState = SkillStateCultist.LungeHolding;
                CallSetAnimation("isLunge", true);
                CallSetData("normalCastingTime", 1, lungeHoldingTime);
            }
            else if (skillState == SkillStateCultist.Skill1Ready)
            {
                //스킬1 사용
                skillState = SkillStateCultist.Skill1Casting;
                CallSetAnimation("isSkill1", true);
                CallSetData("skillCastingTime", 0, skill1CastingTime);
            }
            else if (skillState == SkillStateCultist.Skill2Ready)
            {
                //스킬2 사용
                skillState = SkillStateCultist.Skill2Casting;
                CallSetAnimation("isSkill2", true);
                CallSetData("skillCastingTime", 1, skill2CastingTime);
            }
            else if (skillState == SkillStateCultist.Skill3Ready)
            {
                //스킬3 사용
                skillState = SkillStateCultist.Skill3Casting;
                CallSetAnimation("isSkill3", true);
                CallSetData("skillCastingTime", 2, skill3CastingTime);
            }
            else if (skillState == SkillStateCultist.Skill4Ready)
            {
                //스킬4 사용
                skillState = SkillStateCultist.Skill4Casting;
                CallSetAnimation("isSkill4", true);
                CallSetData("skillCastingTime", 3, skill4CastingTime);
            }
        }
        else
        {
            if(skillState == SkillStateCultist.Invocation)
            {
                //투척
                skillState = SkillStateCultist.Throw;
                CallSetAnimation("isThrow", true);
                CallSetData("normalCastingTime", 3, throwCastingTime);
            }
        }
    }

    void CancelHolding()
    {
        //홀딩 캔슬
        if (skillState == SkillStateCultist.LungeHolding)
        {
            skillState = SkillStateCultist.LungeAttack;
            CallSetAnimation("isLunge", false);
            CallSetData("normalCastingTime", 1, 0f);
            CallSetData("normalCastingTime", 2, lungeAttackTime);
        }
    }

    void CancelSkill()
    {
        if (skillState == SkillStateCultist.Skill1Ready || skillState == SkillStateCultist.Skill2Ready
            || skillState == SkillStateCultist.Skill3Ready || skillState == SkillStateCultist.Skill4Ready
            || (skillState == SkillStateCultist.Invocation && normalCastingTime[0] <= 0f))
        {
            skillState = SkillStateCultist.None;
            CallSetData("OnlySkillState", 0, 0f);
        }
    }

    //응답
    void CallSetData(string timeType, int index, float newTime)
    {
        object[] _tempData = new object[5];
        _tempData[0] = "UpdateData";
        _tempData[1] = skillState;
        _tempData[2] = timeType;
        _tempData[3] = index;
        _tempData[4] = newTime;
        ResponseRPCCall(_tempData);
    }
    void CallSetAnimation(string parameter, bool isParameter)
    {
        object[] _tempData = new object[3];
        _tempData[0] = "SetAnimation";
        _tempData[1] = parameter;
        _tempData[2] = isParameter;
        ResponseRPCCall(_tempData);
    }

    void CallSetDagger(bool isActive)
    {
        object[] _tempData = new object[2];
        _tempData[0] = "SetDagger";
        _tempData[1] = isActive;
        ResponseRPCCall(_tempData);
    }

    void CallResetAnimation()
    {
        object[] _tempData = new object[2];
        _tempData[0] = "ResetAnimation";
        ResponseRPCCall(_tempData);
    }

    void CallInstantiateObject(string objectName)
    {
        object[] _tempData = new object[3];
        _tempData[0] = "InstantiateObject";
        _tempData[1] = objectName;
        ResponseRPCCall(_tempData);
    }

    //응답 처리
    void UpdateData(object[] data)
    {
        skillState = (SkillStateCultist)data[1];

        string _timeType = (string)data[2];
        int _index = (int)data[3];
        float _newTime = (float)data[4];

        if(_timeType == "normalCastingTime")
            normalCastingTime[_index] = _newTime;
        else if(_timeType == "skillCastingTime")
            skillCastingTime[_index] = _newTime;
        else if(_timeType == "skillChannelingTime")
            skillChannelingTime[_index] = _newTime;
    }

    void SetAnimation(object[] data)
    {
        if (photonView.IsMine)
        {
            animator.SetBool((string)data[1], (bool)data[2]);
            overlayAnimator.SetBool((string)data[1], (bool)data[2]);
        }
    }

    void SetOwnerNum(object[] data)
    {
        ownerNum = (int)data[1];
    }

    void SetDagger(object[] data)
    {
        bool _isActive = (bool)data[1];
        if (photonView.IsMine)
            overlayDagger.SetActive(_isActive);
        dagger.SetActive(_isActive);
    }

    void ResetAnimation()
    {
        if (photonView.IsMine)
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

    void InstantiateObject(object[] data)
    {
        if (photonView.IsMine)
        {
            if ((string)data[1] == "Dagger")
            {
                Ray _tempRay = camera.GetComponent<Camera>().ScreenPointToRay(aim.transform.position);
                Quaternion _tempQ = Quaternion.LookRotation(_tempRay.direction);

                object[] _data = new object[3];
                _data[0] = name;
                _data[1] = tag;
                _data[2] = "Dagger";

                PhotonNetwork.Instantiate("ChanYoung/Prefabs/Cultist/Dagger", _tempRay.origin + _tempRay.direction * 0.5f, _tempQ);
            }
        }
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
            SetAnimationSpeed("ThrowSpeed", throwCastingTime);
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Skill1Casting"))
            SetAnimationSpeed("Skill1CastingSpeed", skill1CastingTime);
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Skill1"))
            SetAnimationSpeed("Skill1Speed", skill1ChannelingTime);
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Skill2"))
            SetAnimationSpeed("Skill2CastingSpeed", skill2CastingTime);
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Skill3Casting"))
            SetAnimationSpeed("Skill3CastingSpeed", skill3CastingTime);
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Skill3"))
            SetAnimationSpeed("Skill3Speed", skill3ChannelingTime);
        else if (animator.GetCurrentAnimatorStateInfo(4).IsName("Skill4"))
            SetAnimationSpeedExtra("Skill4CastingSpeed", skill4CastingTime);
        
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
    }

    //손동작 애니메이션에 따른 IK적용
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
