using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace temp
{
    public class GameResult : CenterState
    {
        bool isOnce = true;

        public override void StateExecution()
        {
            if (isOnce)
            {
                isOnce = false;
                gameCenter.photonView.RPC("ShowGameResult", RpcTarget.AllBuffered);
            }
        }
    }

}