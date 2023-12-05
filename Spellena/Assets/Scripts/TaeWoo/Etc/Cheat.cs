using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Cheat : MonoBehaviourPunCallbacks
{
    void Update()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            if(Input.GetKeyDown(KeyCode.F5))
            {

            }

            if (Input.GetKeyDown(KeyCode.F6))
            {

            }

            if (Input.GetKeyDown(KeyCode.F7))
            {

            }

            if (Input.GetKeyDown(KeyCode.F8))
            {

            }
        }
    }
}
