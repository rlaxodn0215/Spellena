using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OccupyingSystem : MonoBehaviourPunCallbacks
{
    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.layer == 15)
            other.GetComponent<Character>().isOccupying = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 15)
            other.GetComponent<Character>().isOccupying = false;
    }

}
