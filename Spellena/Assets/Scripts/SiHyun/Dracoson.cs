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
    public GameObject minimapCamera;
    public GameObject aim;
    public Animator overlayAnimator;
    public Transform overlaySight;
    public GameObject overlayRightHand;
    public Transform staffTopForMe;
    public Transform staffTopForOther;
    float rightHandWeight = 0.04f;

    [Range(1, 3)]
    public int projectile = 1;

    [Range(0, 1)]
    public float weight = 0.5f;

    private GameObject currentObjectForMe;
    private GameObject currentObjectForOther;
    private int previouseChargeCount = 0;

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
        None, Holding, Breathe, Flying, Firing,
        Skill1, Skill1Ready,
        Skill2, Skill2Ready, Skill2Casting,
        Skill3, 
        Skill4
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

    [PunRPC]
    public override void IsLocalPlayer()
    {
        base.IsLocalPlayer();
        overlaycamera.SetActive(true);
        minimapCamera.SetActive(true);
        ChangeLayerRecursively(overlayRightHand.transform);
        Transform avatarForOtherRoot = transform.GetChild(0).GetChild(0).GetChild(1);//다른 사람들이 보는 자신의 아바타
        avatarForOtherRoot.GetComponentInChildren<MeshRenderer>().transform.gameObject.layer = 6;
        avatarForOtherRoot.GetComponentInChildren<MeshRenderer>().enabled = false;
    }

    void ChangeLayerRecursively(Transform targetTransform)
    {
        targetTransform.gameObject.layer = 8;

        foreach(Transform child in targetTransform)
        {
            ChangeLayerRecursively(child);
        }
    }


    protected override void Update()
    {
        base.Update();
        if (PhotonNetwork.IsMasterClient)
        {
            CheckCoolDownTime();
            CheckChargePhase();
            UpdateData();
            Debug.Log(skillState);
        }

        if (photonView.IsMine)
        {
            CheckAnimator();
            
            if (chargeCount != previouseChargeCount)
            {
                SetChargeEffect();
            }
        }
        Debug.Log("ChargeCount : " + chargeCount);
    }

    void SetChargeEffect()
    {
        photonView.RPC("DracosonChargeEffect", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void DracosonChargeEffect()
    {
        previouseChargeCount = chargeCount;

        switch (chargeCount)
        {
            case 1:
                InstantiateChargeEffect(chargeCount);
                break;
            case 2:
                Destroy(currentObjectForMe);
                Destroy(currentObjectForOther);
                InstantiateChargeEffect(chargeCount);
                break;
            case 3:
                Destroy(currentObjectForMe);
                Destroy(currentObjectForOther);
                InstantiateChargeEffect(chargeCount);
                break;
            case 0:
                Destroy(currentObjectForMe);
                Destroy(currentObjectForOther);
                PhotonNetwork.Destroy(currentObjectForMe);
                PhotonNetwork.Destroy(currentObjectForOther);
                break;
            default:
                break;

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
        if (skillState == SkillStateDracoson.Skill1)
        {
            if (globalCastingTime <= 0f)
            {
                CallSetAnimation("Skill1", false);
                skillState = SkillStateDracoson.None;
            }
        }
        else if(skillState == SkillStateDracoson.Skill2)
        {
            if(globalCastingTime <= 0f)
            {
                CallSetAnimation("Skill2", false);
                skillState = SkillStateDracoson.None;
                animator.SetLayerWeight(1, animator.GetLayerWeight(1) + Time.deltaTime * 8);
            }
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

    }

    void CheckChargePhase()
    {
        if (isClicked)
        {
            /*if (photonView.IsMine)
            {*/
                CallSetAnimation("HoldingCancel", false);
                holdingTime = Time.time - holdingStartTime;

                float _chargePhase1 = 1.3f;
                float _chargePhase2 = 2.0f;
                float _chargePhase3 = 3.0f;
                float _chargePhaseOver = 4.0f;

                if (holdingTime >= _chargePhase1 && holdingTime < _chargePhase2)
                {
                    object[] _tempData = new object[6];
                    _tempData[0] = "SetChargeCount";
                    _tempData[1] = 1;
                    _tempData[2] = true;
                    _tempData[3] = false;
                    _tempData[4] = false;
                    _tempData[5] = false;
                    RequestRPCCall(_tempData);
                    Debug.Log("1단계 차지");

                }
                else if (holdingTime >= _chargePhase2 && holdingTime < _chargePhase3)
                {
                    object[] _tempData = new object[6];
                    _tempData[0] = "SetChargeCount";
                    _tempData[1] = 2;
                    _tempData[2] = false;
                    _tempData[3] = true;
                    _tempData[4] = false;
                    _tempData[5] = false;
                    RequestRPCCall(_tempData);
                    Debug.Log("2단계 차지");

                }
                else if (holdingTime >= _chargePhase3 /*&& holdingTime < _chargePhaseOver*/)
                {
                    object[] _tempData = new object[6];
                    _tempData[0] = "SetChargeCount";
                    _tempData[1] = 3;
                    _tempData[2] = false;
                    _tempData[3] = false;
                    _tempData[4] = true;
                    _tempData[5] = false;
                    RequestRPCCall(_tempData);
                    Debug.Log("3단계 차지");

                }
                /*else if (holdingTime >= _chargePhaseOver)
                {
                    Debug.Log("오버 차지");
                    SetChargePhase(false, false, false, true);
                    chargeCount = 4;
                }*/
                else
                {
                    object[] _tempData = new object[6];
                    _tempData[0] = "SetChargeCount";
                    _tempData[1] = 0;
                    _tempData[2] = false;
                    _tempData[3] = false;
                    _tempData[4] = false;
                    _tempData[5] = false;
                    RequestRPCCall(_tempData);
                    Debug.Log("차지 실패");
                    previouseChargeCount = 0;
                }
            /*}*/
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
        Debug.Log("RPC 쏘는중");
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
        else if ((string)data[0] == "SetChargeCount")
            SetChargeCount(data);
    }

    void SetSkill(object[] data)
    {
        if (coolDownTime[(int)data[1] - 1] <= 0f)
        {
            if ((int)data[1] == 1)
            {
                skillState = SkillStateDracoson.Skill1Ready;
                CallSetAnimation("Skill1Ready", true);
            }
            else if ((int)data[1] == 2)
            {
                skillState = SkillStateDracoson.Skill2Ready;
                CallSetAnimation("Skill2Ready", true);
            }
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

    void InstantiateObject(int chargePhase)
    {
       
            Ray _tempRay = camera.GetComponent<Camera>().ScreenPointToRay(aim.transform.position);
            Quaternion _tempQ = Quaternion.LookRotation(_tempRay.direction);

            if (chargePhase != 0)
            {
                Debug.Log("투사체 발사");
                PhotonNetwork.Instantiate("SiHyun/Prefabs/Dracoson/Dragonic Flame Projectile " + chargePhase,
                _tempRay.origin + _tempRay.direction * 0.5f, _tempQ, data: null);
            }
            else
            {
                PhotonNetwork.Instantiate("SiHyun/Prefabs/Dracoson/Dragonic Flame Projectile " + projectile,
                _tempRay.origin + _tempRay.direction * 0.5f, _tempQ, data: null);
            }
    }

    void InstantiateChargeEffect(int chargePhase)
    {
        if (photonView.IsMine)
        {
            if (currentObjectForMe != null)
            {
                PhotonNetwork.Destroy(currentObjectForMe);
            }
            if (currentObjectForOther != null)
            {
                PhotonNetwork.Destroy(currentObjectForOther);
            }

            Quaternion _staffRotationForMe = Quaternion.LookRotation(staffTopForMe.forward);
            currentObjectForMe = PhotonNetwork.Instantiate("SiHyun/Prefabs/Dracoson/Dracoson Charge Effect " + chargePhase,
                staffTopForMe.position, _staffRotationForMe);

            currentObjectForMe.transform.parent = staffTopForMe.transform;
            currentObjectForMe.layer = 8;

            Quaternion _staffRotationForOther = Quaternion.LookRotation(staffTopForOther.forward);
            currentObjectForOther = PhotonNetwork.Instantiate("SiHyun/Prefabs/Dracoson/Dracoson Charge Effect " + chargePhase,
                staffTopForOther.position, _staffRotationForOther);

            currentObjectForOther.transform.parent = staffTopForOther.transform;
            currentObjectForOther.layer = 6;
        }
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
            InstantiateObject(chargeCount);
            skillState = SkillStateDracoson.None;
            holdingStartTime = 0f;
            holdingTime = 0f;
            chargeCount = 0;
            Debug.Log("홀딩 취소");
        }
    }

    void ClickMouse()
    {
        if (skillState == SkillStateDracoson.None)
        {
            // 용의 시선
            holdingStartTime = Time.time;
            CallSetAnimation("isHolding", true);
            CallSetAnimation("HoldingCancel", false);
            skillState = SkillStateDracoson.Holding;

        }
        else if (skillState == SkillStateDracoson.Skill1Ready)
        {
            //스킬1 사용
            coolDownTime[0] = 5f;
            globalCastingTime = 1f;
            skillState = SkillStateDracoson.Skill1;
            CallSetAnimation("Skill1Ready", false);
            CallSetAnimation("Skill1", true);
        }
        else if (skillState == SkillStateDracoson.Skill2Ready)
        {
            //스킬2 사용
            coolDownTime[1] = 5f;
            globalCastingTime = 1f;
            skillState = SkillStateDracoson.Skill2;
            CallSetAnimation("Skill2Ready", false);
            CallSetAnimation("Skill2", true);
            animator.SetLayerWeight(1, 0);
            animator.SetLayerWeight(1, animator.GetLayerWeight(1) - Time.deltaTime * 8);
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

    void SetChargeCount(object[] data)
    {
        photonView.RPC("SetChargeCount", RpcTarget.AllBuffered, (int)data[1]);
        photonView.RPC("SetChargePhase", RpcTarget.AllBuffered, data);
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
        Debug.Log("RPC를 모두에게 쏘는중");
    }

    [PunRPC]
    public void SetChargeCount(int chargePhase)
    {
        chargeCount = chargePhase;
    }

    [PunRPC]
    public void SetChargePhase(object[] data)
    {
        overlayAnimator.SetBool("ChargePhase1", (bool)data[2]);
        overlayAnimator.SetBool("ChargePhase2", (bool)data[3]);
        overlayAnimator.SetBool("ChargePhase3", (bool)data[4]);
        overlayAnimator.SetBool("ChargePhaseOver", (bool)data[5]);
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
            overlayAnimator.SetBool((string)data[1], (bool)data[2]);
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

    protected override void OnAnimatorIK()
    {
        base.OnAnimatorIK();

        if (overlayAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            LerpWeight(0.0045f);
        else if (overlayAnimator.GetCurrentAnimatorStateInfo(0).IsName("isHolding"))
            LerpWeight(weight);

        overlayAnimator.SetIKPosition(AvatarIKGoal.RightHand, overlaySight.position);
        overlayAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandWeight);
    }

    void LerpWeight(float weight)
    {
        rightHandWeight = Mathf.Lerp(rightHandWeight, weight, Time.deltaTime * 8f);
    }
}
