using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameResult : CenterState
{
    bool isOnce = true;

    public override void StateExecution()
    {
        if(isOnce)
        {
            isOnce = false;

            foreach(var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                PhotonView view = PhotonView.Find((int)player.CustomProperties["CharacterViewID"]);
                if (view == null) continue;
                PhotonNetwork.Destroy(view);
            }

            gameCenter.gameResultObj.SetActive(true);
        }
    }
}

