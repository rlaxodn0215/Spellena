using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Player;

public class WorldKiller : MonoBehaviourPunCallbacks
{
    private void OnCollisionEnter(Collision collision)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (collision.transform.root.gameObject.GetComponent<Character>())
            {
                collision.transform.root.gameObject.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.AllBuffered, "World", 100000000, "world", Vector3.zero, 0.0f);
            }
        } 
    }
}
