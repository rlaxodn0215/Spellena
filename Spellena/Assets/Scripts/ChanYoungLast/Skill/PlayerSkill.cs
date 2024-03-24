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
    ��� : ���� ���� �ð��� ���ҽ�Ű�� ������ �Ϸ�Ǹ� Ư�� �Լ� ����
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
    ��� : ��ų ����
    */
    virtual public void Play() 
    {

    }

    /*
    ��� : ��ų�� ���� ���·� ��ȯ
    */
    virtual protected void ChangeNextRoute()
    {

    }

    /*
    ��� : �� ��ų�� �غ� �Ǿ��ִ��� Ȯ��
    */
    public bool IsReady()
    {
        return isReady;
    }

    /*
    ��� : �� ��ų�� ���������� Ȯ��
    */
    public bool IsProgressing()
    {
        if (skillCastingTime > 0f || skillChannelingTime > 0f)
            return true;
        return false;
    }

    /*
    ��� : ������ Ŭ���̾�Ʈ���� ���� Ŭ���̾�Ʈ���� ��ų�� �غ� �ϷḦ �˸�
    */
    [PunRPC]
    public void NotifyReady()
    {
        isReady = true;
    }


    /*
    ��� : ���� �並 ���� ���ο� ������Ʈ�ϱ� ���� ������ 
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
