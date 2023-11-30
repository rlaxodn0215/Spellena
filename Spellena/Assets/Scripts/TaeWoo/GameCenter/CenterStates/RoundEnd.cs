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
            tempTimer = GameCenterTest.globalTimer;
            gameCenter.globalDesiredTimer = tempTimer + gameCenter.roundEndResultTime;
        }

        GameCenterTest.globalTimer += Time.deltaTime;

        if (GameCenterTest.globalTimer >= gameCenter.globalDesiredTimer)
        {

            if (GameCenterTest.roundA >= 2 || GameCenterTest.roundB >= 2)
            {
                gameCenter.currentGameState = GameCenterTest.GameState.GameResult;
                gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "victory", false);
                gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "defeat", false);
                Debug.Log("Game End");
            }

            else
            {
                gameCenter.currentGameState = GameCenterTest.GameState.GameReady;
                gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "roundWin", false);
                gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "roundLoose", false);
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

        gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "captured_Red", false);
        gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "captured_Blue", false);
        gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "extraObj", false);
        gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "redExtraUI", true);
        gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "redExtraObj", false);
        gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "blueExtraUI", true);
        gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "blueExtraObj", false);
        gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "etcUI", true);

        // 플레이어 소환 위치로 이동
        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (player.CustomProperties["SpawnPoint"] != null)
            {
                PhotonView view = PhotonView.Find((int)player.CustomProperties["CharacterViewID"]);
                if (view == null) continue;
                view.RPC("PlayerTeleport", RpcTarget.All, (Vector3)player.CustomProperties["SpawnPoint"]);
            }
        }

    }
}
