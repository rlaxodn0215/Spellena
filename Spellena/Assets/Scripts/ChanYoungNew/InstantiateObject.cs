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

        transform.position = (Vector3)data[1];

        playerPhotonView = PhotonNetwork.GetPhotonView((int)data[0]);
        playerData = playerPhotonView.GetComponent<PlayerCommon>().playerData;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        throw new System.NotImplementedException();
    }
}
