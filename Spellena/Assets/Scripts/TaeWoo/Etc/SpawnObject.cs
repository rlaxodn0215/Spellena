using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Player
{
    public class SpawnObject : MonoBehaviourPunCallbacks, IPunObservable
    {
        public int ID;                      // «ÿ¥Á player¿« viewID
        public string objName;
        public Vector3 direction;

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            
        }
    }
}