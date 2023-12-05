using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CultistDagger : SpawnObject
{
    public TriggerEventer triggerEventer;
    public CultistData cultistData;

    float daggerSpeed = 5f;

    float lifeTime = 10f;
    float currentLifeTime = 0f;

    private Rigidbody daggerRigidbody;
    private void Start()
    {
        daggerRigidbody = GetComponent<Rigidbody>();
        Init();
    }
    private void Update()
    {
        CheckLifeTime();
    }

    private void FixedUpdate()
    {
        daggerRigidbody.MovePosition(transform.position + Time.deltaTime * transform.forward * daggerSpeed);
    }

    void CheckLifeTime()
    {
        currentLifeTime -= Time.deltaTime;
        if(currentLifeTime <= 0f)
            CallRPCTunnel("RequestDestroy");
    }

    void Init()
    {
        triggerEventer.hitTriggerEvent += TriggerEvent;
        currentLifeTime = lifeTime;
    }

    void CallRPCTunnel(string tunnelCommand)
    {
        object[] _tempData;
        _tempData = new object[2];
        _tempData[0] = tunnelCommand;
        
        photonView.RPC("CallRPCTunnelCultistDagger", RpcTarget.AllBuffered, _tempData);
    }

    [PunRPC]
    public void CallRPCTunnelCultistDagger(object[] data)
    {
        if ((string)data[0] == "RequestDestroy")
            RequestDestroy();
    }

    void RequestDestroy()
    {
        if(photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }

    void TriggerEvent(GameObject hitObject)
    {
        Debug.LogError(hitObject.name);

        if (PhotonNetwork.IsMasterClient)
        {
            if (hitObject.gameObject.layer == 11 || hitObject.tag == "Wall")
                CallRPCTunnel("RequestDestroy");
            if (hitObject.transform.root.gameObject.name != hitObject.name)
            {
                GameObject _rootObject = hitObject.transform.root.gameObject;
                if (_rootObject.GetComponent<Character>() != null)
                {
                    if (_rootObject.tag != tag)
                    {
                        _rootObject.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.AllBuffered,
                         playerName, (int)(cultistData.skill1Damage), hitObject.name, transform.forward, 20f);
                        CallRPCTunnel("RequestDestroy");
                    }
                }
            }
        }
    }
}
