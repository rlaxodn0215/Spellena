using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonicBreathe : SpawnObject
{
    public DracosonData dracosonData;

    private float deleteTime;
    List<string> hitObjects = new List<string>();
    private float checkTimer = 0f;
    private float resetTimer = 0.4f;
    private void Start()
    {
        Init();
    }

    void Update()
    {
        deleteTime -= Time.deltaTime;

        checkTimer -= Time.deltaTime;
        if (checkTimer <= 0f)
        {
            hitObjects.Clear();
            checkTimer = resetTimer;
        }

        if (deleteTime <= 0f)
            CallRPCTunnel("RequestDestroy");
    }

    void Init()
    {
        deleteTime = 5f;
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

    private void OnParticleCollision(GameObject other)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log(other.name);
            if (!hitObjects.Contains(other.name))
            {
                hitObjects.Add(other.name);
                if (other.GetComponent<Character>() != null)
                {
                    Debug.Log("여기까지 왔다면 사실상 실행 : "+other.name);
                    //if(_rootObject.tag != tag)
                    {
                        other.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.AllBuffered,
                                playerName, (int)(dracosonData.skill1Damage), other.name, transform.forward, 20f);

                        object[] _data = new object[3];
                        _data[0] = name;
                        _data[1] = tag;
                        _data[2] = "Flooring";
                        PhotonNetwork.Instantiate("SiHyun/Prefabs/Dracoson/Dragonic Breathe Flooring",
                            other.transform.position, Quaternion.identity, data: _data);
                    }
                }
            }
        }
    }
}
