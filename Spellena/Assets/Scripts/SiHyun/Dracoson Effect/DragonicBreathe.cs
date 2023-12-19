using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonicBreathe : SpawnObject
{
    public DracosonData dracosonData;

    List<string> hitObjects = new List<string>();
    private float deleteTime;
    private float checkTimer = 0f;
    private float resetTimer = 0.4f;

    Transform sightDirection;

    private void Start()
    {
        Init();
    }

    void FixedUpdate()
    {
        deleteTime -= Time.deltaTime;

        checkTimer -= Time.deltaTime;
        if (checkTimer <= 0f)
        {
            hitObjects.Clear();
            checkTimer = resetTimer;
        }

        if (deleteTime <= 1.5f)
            transform.GetComponent<ParticleSystem>().Stop();
        else if (deleteTime <= 0f)
            if(PhotonNetwork.IsMasterClient)
                CallRPCTunnel("RequestDestroy");
    }
    void Init()
    {
        deleteTime = 6.5f;
    }

    void CallRPCTunnel(string tunnelCommand)
    {
        object[] _tempData;
        _tempData = new object[2];
        _tempData[0] = tunnelCommand;

        photonView.RPC("CallRPCTunnelDracosonDragonSpin", RpcTarget.AllBuffered, _tempData);
    }

    public void SetInfo(Transform sight)
    {
        sightDirection = sight;
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
            if (!hitObjects.Contains(other.name))
            {
                hitObjects.Add(other.name);
                if (other.GetComponent<Character>() != null)
                {
                    Debug.Log("여기까지 왔다면 사실상 실행 : "+other.name);
                    //if(_rootObject.tag != tag)
                    {
                        other.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.AllBuffered,
                                playerName, (int)(dracosonData.dragonicBreatheDamage), other.name, transform.forward, 20f);
                        Debug.Log(other + "데미지 줬다 난 몰라" );

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
