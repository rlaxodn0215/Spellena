using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AngleStatue : MonoBehaviour
{
    [PunRPC]
    public void ChangeTeam(string team)
    {
        if (team == "A")
        {
            gameObject.tag = "TeamA";
        }

        else
        {
            gameObject.tag = "TeamB";
        }
    }
}
