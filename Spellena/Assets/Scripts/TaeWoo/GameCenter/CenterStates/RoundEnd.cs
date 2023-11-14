using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RoundEnd : CenterState
{
    public override void StateExecution()
    {
        gameCenter.globalTimer -= Time.deltaTime;

        photonView.RPC("TimeScaling", RpcTarget.AllBuffered, 0.3f);

        if (gameCenter.globalTimer <= 0.0f)
        {
            gameCenter.globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "roundWin", false);
            gameCenter.globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "roundLoose", false);

            photonView.RPC("TimeScaling", RpcTarget.AllBuffered, 1.0f);

            if (gameCenter.roundA >= 2 || gameCenter.roundB >= 2)
            {
                gameCenter.currentGameState = GameCenterTest.GameState.MatchEnd;
                Debug.Log("Game End");
            }

            else
            {
                gameCenter.currentGameState = GameCenterTest.GameState.GameReady;
            }
        }
    }
}
