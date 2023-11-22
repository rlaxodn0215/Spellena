using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorStrikeObject : SpawnObject
{
    public ElementalOrderData elementalOrderData;

    float castingTime;
    float currentCastingTime = 0f;
    float lifeTime;
    float currentLifeTime = 0f;

    public GameObject hitCollider;
    public GameObject hitEffect;

    bool isColliderOn = false;
    void Start()
    {
        Init();
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
            CheckTimer();
    }

    void CheckTimer()
    {
        if(currentCastingTime > 0f)
            currentCastingTime -= Time.deltaTime;
        else
        {
            if(isColliderOn == false)
                RequestRPC("ActiveCollider");
            if(currentLifeTime > 0f)
            {
                currentLifeTime -= Time.deltaTime;
                if (currentLifeTime <= 0f)
                {
                    RequestRPC("RequestDestroy");
                    Debug.Log("»Ñ½¤");
                }
            }
        }
        RequestRPC("UpdateData");
    }

    void Init()
    {
        castingTime = elementalOrderData.meteorStrikeCastingTime;
        lifeTime = elementalOrderData.meteorStrikeLifeTime;
        hitCollider.GetComponent<TriggerEventer>().hitTriggerEvent += TriggerEvent;

        currentCastingTime = castingTime;
        currentLifeTime = lifeTime;
    }

    void TriggerEvent(GameObject gameObject)
    {
        if (isColliderOn)
        {
            Debug.Log(gameObject);
        }
    }

    void RequestRPC(string tunnelCommand)
    {
        object[] _tempData;
        if (tunnelCommand == "UpdateData")
        {
            _tempData = new object[3];
            _tempData[0] = tunnelCommand;
            _tempData[1] = currentCastingTime;
            _tempData[2] = currentLifeTime;
        }
        else
        {
            _tempData = new object[2];
            _tempData[0] = tunnelCommand;
        }
        
        photonView.RPC("CallRPCTunnelElementalOrderSpell4", RpcTarget.AllBuffered, _tempData);
    }

    [PunRPC]
    public void CallRPCTunnelElementalOrderSpell4(object[] data)
    {
        if ((string)data[0] == "ActiveCollider")
            ActiveCollider();
        else if ((string)data[0] == "RequestDestroy")
            RequestDestroy();
        else if ((string)data[0] == "UpdateData")
            UpdateData(data);
    }
    void ActiveCollider()
    {
        isColliderOn = true;
        hitEffect.SetActive(true);
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
    }
}
