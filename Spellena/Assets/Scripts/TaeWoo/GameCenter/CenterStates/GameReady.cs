using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class GameReady : CenterState
{
    public override void StateExecution()
    {
        gameCenter.gameStateString = "Ready";

        gameCenter.globalTimer -= Time.deltaTime;
        if (gameCenter.globalTimer <= 0.0f)
        {
            //적 쉐이더 적용
            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                PhotonView view = PhotonView.Find((int)player.CustomProperties["CharacterViewID"]);
                view.RPC("SetEnemyLayer", player);
            }

            gameCenter.currentGameState = GameCenterTest.GameState.DuringRound;
            gameCenter.globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "etcUI", false);

            if(gameCenter.roundA > 0 || gameCenter.roundB > 0) ResetRound();
        }
    }

    void ResetRound()
    {
        gameCenter.currentOccupationTeam = "";
        gameCenter.occupyingA = new GameCenterTest.Occupation();
        gameCenter.occupyingB = new GameCenterTest.Occupation();
        gameCenter.occupyingTeam = new GameCenterTest.OccupyingTeam();
        gameCenter.occupyingReturnTimer = 0f;
        gameCenter.roundEndTimer = 0;
        gameCenter.globalTimer = gameCenter.readyTime;
        gameCenter.teamAOccupying = 0;
        gameCenter.teamBOccupying = 0;

        gameCenter.globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "captured_Red", false);
        gameCenter.globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "captured_Blue", false);
        gameCenter.globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "extraObj", false);
        gameCenter.globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "redExtraUI", true);
        gameCenter.globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "redExtraObj", false);
        gameCenter.globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "blueExtraUI", true);
        gameCenter.globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "blueExtraObj", false);
        gameCenter.globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "etcUI", true);

        foreach (Photon.Realtime.Player player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (player.CustomProperties["SpawnPoint"] != null)
            {
                PhotonView view = PhotonView.Find((int)player.CustomProperties["CharacterViewID"]);
                if (view == null) continue;
                view.RPC("PlayerTeleport", RpcTarget.AllBuffered, (Vector3)player.CustomProperties["SpawnPoint"]);
            }
        }

    }

}
