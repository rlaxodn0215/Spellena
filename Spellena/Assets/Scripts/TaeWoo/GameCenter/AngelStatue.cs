using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AngelStatue : MonoBehaviour
{
    [PunRPC]
    public void ChangeTeam(string team)
    {
        if(team == "A")
        {
            tag = "TeamA";
        }

        else if(team == "B")
        {
            tag = "TeamB";
        }
    }

}
