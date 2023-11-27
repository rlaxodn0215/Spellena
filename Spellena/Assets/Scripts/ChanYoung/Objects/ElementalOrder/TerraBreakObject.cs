using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class TerraBreakObject : SpawnObject
{
    public ElementalOrderData elementalOrderData;

    public GameObject rangeArea;
    public GameObject hitEffect;

    float castingTime;
    float currentCastingTime = 0f;

    float lifeTime;
    float currentLifeTime = 0f;

    float hitCountTimer;
    float hitCountTimerFirst;
    float currentHitCountTimer = 0f;

    bool isFirst = true;

    bool isColliderOn = false;

    public GameObject hitCollider;

    List<string> hitObjects = new List<string>();

    void Start()
    {
        Init();
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            CheckTimer();
        }
    }

    void CheckTimer()
    {
        if(currentCastingTime > 0f)
        {
            currentCastingTime -= Time.deltaTime;
            if (currentCastingTime <= 0f)
            {
                RequestRPC("ActiveCollider");
            }
        }
        else
        {
            if (currentHitCountTimer <= 0f)
            {
                hitObjects.Clear();
                RequestRPC("ActiveHitEffect");
                if (isFirst == true)
                    currentHitCountTimer = hitCountTimerFirst;
                else
                    currentHitCountTimer = hitCountTimer;
            }
            else if(isFirst == true)
            {
                currentHitCountTimer -= Time.deltaTime;
                if (currentHitCountTimer <= 0f)
                    isFirst = false;
            }
            else
                currentHitCountTimer -= Time.deltaTime;
            currentLifeTime -= Time.deltaTime;
            if(currentLifeTime <= 0f)
                RequestRPC("RequestDestroy");
            
        }
    }

    void Init()
    {
        castingTime = elementalOrderData.terraBreakCastingTime;
        //2.5 + 0.5 ÃÑ 3ÃÊ
        lifeTime = elementalOrderData.terraBreakLifeTime + elementalOrderData.terraBreakLifeTimeFirst;
        hitCountTimer = elementalOrderData.terraBreakLifeTime / 4;
        hitCountTimerFirst = elementalOrderData.terraBreakLifeTimeFirst;

        hitCollider.GetComponent<TriggerEventer>().hitTriggerEvent += TriggerEvent;

        currentCastingTime = castingTime;
        currentLifeTime = lifeTime;

        BalanceAnimation();
    }

    void BalanceAnimation()
    {
        rangeArea.GetComponent<ParticleSystem>().startLifetime = castingTime;
        for(int i = 0; i < 6; i++)
        {
            hitEffect.transform.GetChild(i).GetComponent<ParticleSystem>().startLifetime = hitCountTimerFirst;
        }
    }

    void RequestRPC(string tunnelCommand)
    {
        object[] _tempData;
        if(tunnelCommand == "UpdateData")
        {
            _tempData = new object[6];
            _tempData[0] = tunnelCommand;
            _tempData[1] = currentCastingTime;
            _tempData[2] = currentLifeTime;
            _tempData[3] = currentHitCountTimer;
            _tempData[4] = isFirst;
            _tempData[5] = hitObjects.ToArray();
        }
        else if(tunnelCommand == "ActiveHitEffect")
        {
            _tempData = new object[2];
            _tempData[0] = tunnelCommand;
            _tempData[1] = isFirst;
        }
        else
        {
            _tempData = new object[2];
            _tempData[0] = tunnelCommand;
        }

        photonView.RPC("CallRPCTunnelElementalOrderSpell5", RpcTarget.AllBuffered, _tempData);
    }
    [PunRPC]
    public void CallRPCTunnelElementalOrderSpell5(object[] data)
    {
        if ((string)data[0] == "UpdateData")
            UpdateData(data);
        else if ((string)data[0] == "ActiveCollider")
            ActiveCollider();
        else if ((string)data[0] == "RequestDestroy")
            RequestDestroy();
        else if ((string)data[0] == "ActiveHitEffect")
            ActiveHitEffect(data);
    }

    void ActiveHitEffect(object[] data)
    {
        hitEffect.SetActive(true);
        if ((bool)data[1] == false)
        {
            for (int i = 0; i < 6; i++)
            {
                hitEffect.transform.GetChild(i).GetComponent<ParticleSystem>().Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                hitEffect.transform.GetChild(i).GetComponent<ParticleSystem>().startLifetime = hitCountTimer;
                hitEffect.transform.GetChild(i).GetComponent<ParticleSystem>().Play();
            }
        }
    }

    void RequestDestroy()
    {
        if(photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }

    void UpdateData(object[] data)
    {
        currentCastingTime = (float)data[1];
        currentLifeTime = (float)data[2];
        currentHitCountTimer = (float)data[3];
        isFirst = (bool)data[4];
        hitObjects = ((string[])data[5]).ToList();
    }

    void ActiveCollider()
    {
        isColliderOn = true;
    }

    void TriggerEvent(GameObject hitObject)
    {
        if(PhotonNetwork.IsMasterClient)
        {
            if (isColliderOn)
            {
                if (hitObject.transform.root.gameObject.name != hitObject.name)
                {
                    GameObject _rootObject = hitObject.transform.root.gameObject;
                    if (_rootObject.tag != tag)
                    {
                        for (int i = 0; i < hitObjects.Count; i++)
                        {
                            if (_rootObject.name == hitObjects[i])
                                return;
                        }

                        hitObjects.Add(_rootObject.name);
                        Vector3 _tempDirection = (_rootObject.transform.position - transform.position).normalized;
                        if (isFirst == true)
                        {
                            _rootObject.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.MasterClient,
                                 playerName, (int)(elementalOrderData.terraBreakDamageFirst), hitObject.name, _tempDirection, 20f);
                        }
                        else
                        {
                            _rootObject.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.MasterClient,
                                 playerName, (int)(elementalOrderData.terraBreakDamage / 4), hitObject.name, _tempDirection, 20f);
                        }
                    }
                }
            }
        }
    }    
}
