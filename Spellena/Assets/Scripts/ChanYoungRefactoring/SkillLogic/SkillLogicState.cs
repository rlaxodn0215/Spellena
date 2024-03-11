using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkillLogic;
using Photon.Pun;
using SkillRouteData;
using System;

public class SkillLogicState : MonoBehaviourPunCallbacks, ISkillLogic, IPunObservable
{
    public BalanceData balanceData;

    public bool isSkill = true;

    private bool isFirst = true;

    protected float coolDownTime = 0f;
    protected float castingTime = 0f;
    protected float channelingTime = 0f;

    protected List<SkillRoute> skillRoute = new List<SkillRoute>();
    protected int routeIndex = 0;

    [HideInInspector]
    public bool isOver = false;
    [HideInInspector]
    public bool isReadyToAnimation = false;


    /*
    ��� : ���� ��ų�� ��ȯ�Ǹ� ȣ�� -> �������̵�
    */
    virtual public void Enter()
    {
        if(isFirst)
        {
            isFirst = false;
            InitFirst();
        }
    }

    /*
    ��� : �� ��ų�� ���� ��ų�� ��� SkillController���� FixedUpdate���� ȣ��
    */
    virtual public void PlayFixedUpdate()
    {
    }

    /*
    ��� : �� ��ų�� ���� ��ų�� ��� SkillController���� Update���� ȣ��
    */
    virtual public void PlayUpdate()
    {
    }

    /*
    ��� : FixedUpdate���� Ÿ�̸� ����
    */
    public void PlayTimer()
    {
        ProgressTimer();
    }

    /*
    ��� : ��ų�� ���� �� �� ȣ��
    */
    virtual public void Quit()
    {
        isOver = false;
    }

    /*
      ó�� ��ų�� ���� ��ų�� ��ȯ�Ǹ� ȣ��
    */
    virtual protected void InitFirst()
    {
    }


    /*
    ��� : ��Ÿ��, ĳ����, ä�θ� �ð��� FixedUpdate���� ����
    */
    public void ProgressTimer()
    {
        if (coolDownTime > 0f)
            coolDownTime -= Time.deltaTime;
        if (castingTime > 0f)
        {
            castingTime -= Time.deltaTime;
            if (castingTime <= 0f)
                ChangeNextRoute();
        }
        if (channelingTime > 0f)
        {
            channelingTime -= Time.deltaTime;
            if (channelingTime <= 0f)
                ChangeNextRoute();
        }
    }

    /*
    ��� : �� ��ų�� ��Ÿ���� �����ִ��� Ȯ��
    ���� : ���������� true, �ƴϸ� false
    */
    public bool IsCoolDownTime()
    {
        if (coolDownTime > 0f)
            return true;
        return false;
    }

    /*
    ��� : ���� ��ų�� ����ǰ� �ִ� �� Ȯ��
    ���� : ����ǰ� ������ true, ������� ������ false
    */
    public bool IsProgressing()
    {
        if (castingTime > 0f || channelingTime > 0f)
            return true;
        return false;
    }

    /*
    ��� : skillRoute���� ���� ���� ���� ��ų�� ���¸� ��ȯ
    ���� : ���� ���� ���� ��ų�� ���¸� ��ȯ
    */
    public SkillRoute GetCurrentState()
    {
        return skillRoute[routeIndex];
    }

    /*
    ��� : �� ��ų�� ���� ��ų�� ��ȯ
    */
    public void ChangeNextRoute()
    {
        routeIndex++;
        if (routeIndex >= skillRoute.Count)
        {
            isOver = true;
            routeIndex = 0;
        }
        PlaySkillLogic();
    }

    /*
    ��� : ��ų�� ������ ���� -> �������̵�
    */
    virtual protected void PlaySkillLogic()
    {
    }

    /*
    �ִϸ��̼� ���� ��û�� ���� �� ������ �̿��� ��� �÷��̾�� ����
    */
    [PunRPC]
    public void SyncAnimationCall()
    {
        isReadyToAnimation = true;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}