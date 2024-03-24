using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkillInfo;

public class PlayerSkill : MonoBehaviourPunCallbacks, IPunObservable
{
    public ScriptableSkillData skillData;

    protected bool isReady = false;

    protected SkillState skillState = SkillState.None;

    protected float skillCoolDownTime = 0f;
    protected float skillCastingTime = 0f;
    protected float skillChannelingTime = 0f;

    private void FixedUpdate()
    {
        CheckTime();
    }

    /*
    기능 : 진행 중인 시간을 감소시키며 진행이 완료되면 특정 함수 실행
    */
    private void CheckTime()
    {
        if (skillCoolDownTime > 0f)
            skillCoolDownTime -= Time.fixedDeltaTime;

        if (skillCastingTime > 0f)
        {
            skillCastingTime -= Time.fixedDeltaTime;
            if (skillCastingTime <= 0f)
                ChangeNextRoute();
        }

        if (skillChannelingTime > 0f)
        {
            skillChannelingTime -= Time.fixedDeltaTime;
            if (skillChannelingTime <= 0f)
                ChangeNextRoute();
        }

        if(PhotonNetwork.IsMasterClient)
        {
            if (skillCoolDownTime <= 0f && isReady == false)
                photonView.RPC("NotifyReady", photonView.Owner);
        }    
    }

    /*
    기능 : 스킬 진행
    */
    virtual public void Play() 
    {

    }

    /*
    기능 : 스킬을 다음 상태로 전환
    */
    virtual protected void ChangeNextRoute()
    {

    }

    /*
    기능 : 이 스킬이 준비가 되어있는지 확인
    */
    public bool IsReady()
    {
        return isReady;
    }

    /*
    기능 : 이 스킬이 진행중인지 확인
    */
    public bool IsProgressing()
    {
        if (skillCastingTime > 0f || skillChannelingTime > 0f)
            return true;
        return false;
    }

    /*
    기능 : 마스터 클라이언트에서 로컬 클라이언트에게 스킬의 준비 완료를 알림
    */
    [PunRPC]
    public void NotifyReady()
    {
        isReady = true;
    }


    /*
    기능 : 포톤 뷰를 가진 새로운 오브젝트하기 위한 데이터 
    */
    protected object[] GetInstantiateData()
    {
        object[] _data = new object[2];
        _data[0] = transform.root.tag;
        _data[1] = photonView.ViewID;

        return _data;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}
