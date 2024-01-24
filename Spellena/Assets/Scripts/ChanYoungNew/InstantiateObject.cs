using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateObject : MonoBehaviourPunCallbacks, IPunObservable
{
    protected PlayerData playerData;
    protected PhotonView playerPhotonView;

    protected object[] data;
    virtual protected void Start()
    {
        data = photonView.InstantiationData;

        playerPhotonView = PhotonNetwork.GetPhotonView((int)data[0]);
        playerData = playerPhotonView.GetComponent<PlayerCommon>().playerData;
        tag = (string)data[1];
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        throw new System.NotImplementedException();
    }
}
