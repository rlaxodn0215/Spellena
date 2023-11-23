using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RoundEnd : CenterState
{
    bool isCheckTimer = false;
    float tempTimer = 0.0f;

    public override void StateExecution()
    {
        if (!isCheckTimer)
        {
            isCheckTimer = !isCheckTimer;
            tempTimer = gameCenter.globalTimer;
            gameCenter.globalDesiredTimer = tempTimer + gameCenter.roundEndResultTime;
        }

        gameCenter.globalTimer += Time.deltaTime;

        if (gameCenter.globalTimer >= gameCenter.globalDesiredTimer)
        {
            if (gameCenter.roundA >= 2 || gameCenter.roundB >= 2)
            {
                gameCenter.currentGameState = GameCenterTest.GameState.MatchEnd;
                gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "victory", false);
                gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "defeat", false);
                Debug.Log("Game End");
            }

            else
            {
                gameCenter.currentGameState = GameCenterTest.GameState.GameReady;
                gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "roundWin", false);
                gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "roundLoose", false);
                isCheckTimer = !isCheckTimer;
                ResetRound();
            }
        }
    }

    void ResetRound()
    {
        gameCenter.teamAOccupying = 0;
        gameCenter.teamBOccupying = 0;
        gameCenter.occupyingReturnTimer = 0.0f;
        gameCenter.roundEndTimer = 0.0f;
        gameCenter.currentOccupationTeam = "";
        gameCenter.occupyingA.rate = 0.0f;
        gameCenter.occupyingB.rate = 0.0f;
        gameCenter.occupyingTeam.name = "";
        gameCenter.occupyingTeam.rate = 0.0f;

        gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "captured_Red", false);
        gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "captured_Blue", false);
        gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "extraObj", false);
        gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "redExtraUI", true);
        gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "redExtraObj", false);
        gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "blueExtraUI", true);
        gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "blueExtraObj", false);
        gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "etcUI", true);

        // 플레이어 소환 위치로 이동
        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (player.CustomProperties["SpawnPoint"] != null)
            {
                PhotonView view = PhotonView.Find((int)player.CustomProperties["CharacterViewID"]);
                if (view == null) continue;
                view.RPC("PlayerTeleport", RpcTarget.AllBufferedViaServer, (Vector3)player.CustomProperties["SpawnPoint"]);
            }
        }

    }
}
