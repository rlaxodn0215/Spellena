using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonSight : SpawnObject
{
    public TriggerEventer triggerEventer;
    public DracosonData dracosonData;

    List<string> hitObjects = new List<string>();

    private int skillDamage;

    private void Start()
    {
        Init();
    }

    void Update()
    {
    }

    void Init()
    {
        triggerEventer.hitTriggerEvent += TriggerEvent;
    }

    public void SetChargePhase(int phase)
    {
        switch (phase)
        {
            case 1:
                skillDamage = (int)(dracosonData.dragonSightChargePhase1Damage);
                break;
            case 2:
                skillDamage = (int)(dracosonData.dragonSightChargePhase2Damage);
                break;
            case 3:
                skillDamage = (int)(dracosonData.dragonSightChargePhase3Damage);
                break;
            default:
                skillDamage = (int)(dracosonData.dragonSightChargePhase1Damage);
                break;
        }
    }


    void CallRPCTunnel(string tunnelCommand)
    {
        object[] _tempData;
        _tempData = new object[2];
        _tempData[0] = tunnelCommand;

        photonView.RPC("CallRPCTunnelDracosonDragonSpin", RpcTarget.AllBuffered, _tempData);
    }

    [PunRPC]
    public void CallRPCTunnelDracosonDragonSpin(object[] data)
    {
        if ((string)data[0] == "RequestDestroy")
            RequestDestroy();
    }

    void RequestDestroy()
    {
        if (photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }

    void TriggerEvent(GameObject hitObject)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (hitObject.transform.root.gameObject.name != hitObject.name)
            {
                GameObject _rootObject = hitObject.transform.root.gameObject;
                if (!hitObjects.Contains(_rootObject.name))
                {
                    hitObjects.Add(_rootObject.name);
                    if (_rootObject.GetComponent<Character>() != null)
                    {
                        //if(_rootObject.tag != tag)
                        {
                            _rootObject.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.AllBuffered,
                                playerName, skillDamage, hitObject.name, transform.forward, 20f);
                            PhotonNetwork.Destroy(transform.gameObject);
                        }
                    }
                }
            }
        }
    }

    [PunRPC]
    void DestoryObject(GameObject hitObject)
    {
        // 모든 클라이언트에서 해당 오브젝트를 삭제
        PhotonView _pv = hitObject.GetComponent<PhotonView>();
        if (_pv != null && _pv.IsMine)
        {
            PhotonNetwork.Destroy(hitObject);
        }
    }

}


