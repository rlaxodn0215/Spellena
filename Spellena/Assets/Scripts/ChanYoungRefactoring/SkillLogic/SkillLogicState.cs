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
    기능 : 진행 스킬로 전환되면 호출 -> 오버라이드
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
    기능 : 이 스킬이 진행 스킬인 경우 SkillController에서 FixedUpdate마다 호출
    */
    virtual public void PlayFixedUpdate()
    {
    }

    /*
    기능 : 이 스킬이 진행 스킬인 경우 SkillController에서 Update마다 호출
    */
    virtual public void PlayUpdate()
    {
    }

    /*
    기능 : FixedUpdate마다 타이머 진행
    */
    public void PlayTimer()
    {
        ProgressTimer();
    }

    /*
    기능 : 스킬이 종료 될 때 호출
    */
    virtual public void Quit()
    {
        isOver = false;
    }

    /*
      처음 스킬이 진행 스킬로 전환되면 호출
    */
    virtual protected void InitFirst()
    {
    }


    /*
    기능 : 쿨타임, 캐스팅, 채널링 시간을 FixedUpdate마다 진행
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
    기능 : 이 스킬의 쿨타임이 남아있는지 확인
    리턴 : 남아있으면 true, 아니면 false
    */
    public bool IsCoolDownTime()
    {
        if (coolDownTime > 0f)
            return true;
        return false;
    }

    /*
    기능 : 현재 스킬이 진행되고 있는 지 확인
    리턴 : 진행되고 있으면 true, 진행되지 않으면 false
    */
    public bool IsProgressing()
    {
        if (castingTime > 0f || channelingTime > 0f)
            return true;
        return false;
    }

    /*
    기능 : skillRoute에서 현재 진행 중인 스킬의 상태를 반환
    리턴 : 현재 진행 중인 스킬의 상태를 반환
    */
    public SkillRoute GetCurrentState()
    {
        return skillRoute[routeIndex];
    }

    /*
    기능 : 이 스킬을 다음 스킬로 전환
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
    기능 : 스킬의 로직을 수행 -> 오버라이드
    */
    virtual protected void PlaySkillLogic()
    {
    }

    /*
    애니메이션 변경 요청을 포톤 뷰 소유자 이외의 모든 플레이어에게 전송
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