using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Test : MonoBehaviourPunCallbacks,IPunObservable
{

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
    }
}
