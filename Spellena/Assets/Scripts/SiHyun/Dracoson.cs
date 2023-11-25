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
    public GameObject overlaycamera;
    public GameObject Aim;
    public Animator overlayAnimator;

    public enum SkillStateCultist
    {
        None, Invocation, Lunge, Throw, Skill1, Skill2, Skill3, Skill4
    }

    SkillStateCultist skillState = SkillStateCultist.None;

    bool isClicked = false;

    float[] coolDownTime = new float[4];
    float globalCastingTime = 0f;

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
        if (skillState == SkillStateCultist.Lunge || skillState == SkillStateCultist.Throw)
        {
            if (globalCastingTime <= 0f)
            {
                CallSetAnimation("isLunge", false);
                skillState = SkillStateCultist.None;
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
            }
            else
            {
                if (skillState == SkillStateCultist.Lunge)
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
        if (photonView.IsMine)
        {
            if (skillState == SkillStateCultist.Invocation)
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
                skillState = SkillStateCultist.Skill1;
            else if ((int)data[1] == 2)
                skillState = SkillStateCultist.Skill2;
            else if ((int)data[1] == 3)
                skillState = SkillStateCultist.Skill3;
            else if ((int)data[1] == 4)
                skillState = SkillStateCultist.Skill4;
        }
    }

    void CancelSkill()
    {
        skillState = SkillStateCultist.None;
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
            //기원
            globalCastingTime = 1f;
            CallSetAnimation("isInvocation", true);
            skillState = SkillStateCultist.Invocation;
        }
        else if (skillState == SkillStateCultist.Invocation)
        {
            if (globalCastingTime <= 0f)
            {
                //급습
                globalCastingTime = 1.5f;
                CallSetAnimation("isLunge", true);
                skillState = SkillStateCultist.Lunge;
            }
        }
        else if (skillState == SkillStateCultist.Skill1)
        {
            //스킬1 사용
            coolDownTime[0] = 5f;
        }
        else if (skillState == SkillStateCultist.Skill2)
        {
            //스킬2 사용
            coolDownTime[1] = 5f;
        }
        else if (skillState == SkillStateCultist.Skill3)
        {
            //스킬3 사용
            coolDownTime[2] = 5f;
        }
        else if (skillState == SkillStateCultist.Skill4)
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

    void ClickMouse2()
    {
        if (skillState == SkillStateCultist.Invocation)
        {
            if (globalCastingTime <= 0f)
            {
                //투척
                globalCastingTime = 1.5f;
                CallSetAnimation("isThrow", true);
                skillState = SkillStateCultist.Throw;
            }
        }
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
        skillState = (SkillStateCultist)data[2];
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
