using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CultistDagger : SpawnObject
{
    float daggerSpeed = 100f;

    private Rigidbody daggerRigidbody;
    private void Start()
    {
        daggerRigidbody = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            daggerRigidbody.MovePosition(transform.position + Time.deltaTime * transform.forward * daggerSpeed);
        }
    }

    void CallRPCTunnel(string tunnelCommand)
    {
        object[] _tempData;
        if (tunnelCommand == "UpdateData")
        {
            _tempData = new object[2];
            _tempData[0] = tunnelCommand;
            _tempData[1] = transform.position;
        }
        else
        {
            _tempData = new object[2];
            _tempData[0] = tunnelCommand;
        }

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

    private void OnTriggerEnter(Collider other)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (other.gameObject.layer == 11)
            {
                Debug.Log("ÆÄ±«");
                CallRPCTunnel("RequestDestroy");
            }
            else if(other.gameObject.GetComponent<Character>() != null)
            {
                GameObject _tempObject = other.gameObject;
                if(_tempObject.tag != tag)
                {
                    Debug.Log("µ¥¹ÌÁö");
                    //_tempObject.GetComponent<PhotonView>().RPC("PlayerDamaged");
                }
            }
        }
    }
}
