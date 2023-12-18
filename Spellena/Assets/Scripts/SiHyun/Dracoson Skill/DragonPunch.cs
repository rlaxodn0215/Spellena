using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DragonPunch : SpawnObject
{
    public TriggerEventer triggerEventer;
    public DracosonData dracosonData;
    public GameObject projectile;

    private Vector3 initialPosition;
    private Vector3 projectileDirection;
    private float knockbackForce = 2f;
    private float moveDistance = 100f;
    private float moveSpeed = 20f;
    private float deleteTime = 5f;
    List<string> hitObjects = new List<string>();

    private void Start()
    {
        Init();
        initialPosition = projectile.transform.position;
        projectileDirection = projectile.transform.forward;
    }

    void Update()
    {
        deleteTime -= Time.deltaTime;
        if (deleteTime <= 0f)
            CallRPCTunnel("RequestDestroy");


    }

    void Init()
    {
        triggerEventer.hitTriggerEvent += TriggerEvent;
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
                                playerName, (int)(dracosonData.skill2Damage), hitObject.name, transform.forward, 20f);

                            Vector3 _knockbackDirection =
                                     (_rootObject.transform.position - transform.position).normalized;
                            Debug.Log(_knockbackDirection);
                            _rootObject.GetComponent<PhotonView>().RPC("PlayerKnockBack", RpcTarget.AllBuffered,
                                _knockbackDirection, knockbackForce);
                            /*Debug.Log("끌려가는 함수 작동 해줘 제발 슈발");
                            _rootObject.GetComponent<PhotonView>().RPC("MovePlayerWithDuration", RpcTarget.AllBuffered,
                                projectileDirection, moveDistance, moveSpeed);*/
                        }
                    }
                }
            }
        }
    }

    void TriggerEventPushOut(GameObject hitObject)
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
                                playerName, (int)(dracosonData.skill2Damage), hitObject.name, transform.forward, 20f);

                            Debug.Log("나중에 추가한 트리거만 발동되는듯?");
                            Vector3 _knockbackDirection =
                                     (_rootObject.transform.position - transform.position).normalized;
                            Debug.Log(_knockbackDirection);
                            _rootObject.GetComponent<PhotonView>().RPC("PlayerKnockBack", RpcTarget.AllBuffered,
                                _knockbackDirection, knockbackForce);
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


