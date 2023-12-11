using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AngelStatue : MonoBehaviourPunCallbacks
{
    public GameObject inGameUIObj;
    private InGameUI inGameUI;

    [HideInInspector]
    public float angelStatueCoolTime;
    [HideInInspector]
    public bool isUsed = false;

    private void Start()
    {
        inGameUI = inGameUIObj.GetComponent<InGameUI>();
    }

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

    public void RequestAngelTimerUI(string team, bool isActive)
    {
        if(inGameUI)
        {
            inGameUI.ShowAngelTimerUI(angelStatueCoolTime,team, isActive);
        }
    }

}
