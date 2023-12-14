using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DracosonMagicCircle : SpawnObject
{

    private float deleteTime = 3.0f;
    // Start is called before the first frame update
    void Start()
    {
        deleteTime = 3.0f;
    }

    // Update is called once per frame
    void Update()
    {
        deleteTime -= Time.deltaTime;
        if (deleteTime <= 0f)
            CallRPCTunnel("RequestDestroy");
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
}
