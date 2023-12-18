using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OccupyingSystem : MonoBehaviourPunCallbacks
{
    private void OnTriggerStay(Collider other)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (other.transform.root.gameObject.layer == LayerMask.NameToLayer("Player")
                && other.transform.root.gameObject.GetComponent<Character>().isAlive)
                other.transform.root.gameObject.GetComponent<Character>().isOccupying = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (other.transform.root.gameObject.layer == LayerMask.NameToLayer("Player")
                && other.transform.root.gameObject.GetComponent<Character>().isAlive)
                other.transform.root.gameObject.GetComponent<Character>().isOccupying = false;
        }
    }

}
