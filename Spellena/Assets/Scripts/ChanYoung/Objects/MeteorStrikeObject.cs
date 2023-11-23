using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public GameObject RangeArea;

    List<string> hitObjects = new List<string>();

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
                    RequestRPC("RequestDestroy");
            }
        }
        RequestRPC("UpdateData");
    }

    void Init()
    {
        castingTime = elementalOrderData.meteorStrikeCastingTime;
        lifeTime = elementalOrderData.meteorStrikeLifeTime * 3;
        hitCollider.GetComponent<TriggerEventer>().hitTriggerEvent += TriggerEvent;

        currentCastingTime = castingTime;
        currentLifeTime = lifeTime;
        BalanceAnimation();
    }

    void BalanceAnimation()
    {
        float _tempLifeTime = elementalOrderData.meteorStrikeLifeTime;
        RangeArea.GetComponent<ParticleSystem>().startLifetime = castingTime * 0.85f;
        hitEffect.transform.GetChild(0).GetComponent<ParticleSystem>().startLifetime = _tempLifeTime * 2;
        hitEffect.transform.GetChild(0).GetComponent<ParticleSystem>().startDelay = _tempLifeTime;
        hitEffect.transform.GetChild(1).GetComponent<ParticleSystem>().startLifetime = _tempLifeTime * 1.5f;
        hitEffect.transform.GetChild(1).GetComponent<ParticleSystem>().startDelay = _tempLifeTime;
        hitEffect.transform.GetChild(2).GetComponent<ParticleSystem>().startLifetime = _tempLifeTime;
        hitEffect.transform.GetChild(3).GetComponent<ParticleSystem>().startLifetime = _tempLifeTime;
        hitEffect.transform.GetChild(4).GetComponent<ParticleSystem>().startLifetime = _tempLifeTime;
        hitEffect.transform.GetChild(5).GetComponent<ParticleSystem>().startLifetime = _tempLifeTime;
        hitEffect.transform.GetChild(5).GetComponent<ParticleSystem>().startDelay = _tempLifeTime;
        hitEffect.transform.GetChild(6).GetComponent<ParticleSystem>().startLifetime = _tempLifeTime * 0.5f;
        hitEffect.transform.GetChild(6).GetComponent<ParticleSystem>().startDelay = _tempLifeTime;
        hitEffect.transform.GetChild(7).GetComponent<ParticleSystem>().startLifetime = _tempLifeTime * 2;
        hitEffect.transform.GetChild(7).GetComponent<ParticleSystem>().startDelay = _tempLifeTime;
        hitEffect.transform.GetChild(7).GetChild(0).GetComponent<ParticleSystem>().startLifetime = _tempLifeTime * 1.25f;
        hitEffect.transform.GetChild(7).GetChild(0).GetComponent<ParticleSystem>().startDelay = _tempLifeTime * 0.6f;
    }

    void TriggerEvent(GameObject hitObject)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (isColliderOn)
            {
                if(hitObject.transform.root.gameObject.name != hitObject.name)
                {
                    GameObject _rootObject = hitObject.transform.root.gameObject;
                    if(_rootObject.tag != tag)
                    {
                        for(int i = 0; i < hitObjects.Count; i++)
                        {
                            if (_rootObject.name == hitObjects[i])
                                return;
                        }
                        hitObjects.Add(_rootObject.name);
                        _rootObject.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.MasterClient,
                         playerName, (int)(elementalOrderData.meteorStrikeDamage), hitObject.name, transform.forward, 20f);
                    }
                }
            }
        }
    }

    void RequestRPC(string tunnelCommand)
    {
        object[] _tempData;
        if (tunnelCommand == "UpdateData")
        {
            _tempData = new object[4];
            _tempData[0] = tunnelCommand;
            _tempData[1] = currentCastingTime;
            _tempData[2] = currentLifeTime;
            _tempData[3] = hitObjects.ToArray();
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
        hitObjects = ((string[])data[3]).ToList();
    }
}
