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
            Debug.Log("isCheckTimer");
        }

        GameCenterTest.globalTimer += Time.deltaTime;

        if (GameCenterTest.globalTimer >= gameCenter.globalDesiredTimer)
        {
            if (GameCenterTest.roundA >= 2 || GameCenterTest.roundB >= 2)
            {
                gameCenter.currentGameState = GameCenterTest.GameState.GameResult;
                gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "victory", false);
                gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "defeat", false);              
            }

            else
            {
                gameCenter.currentGameState = GameCenterTest.GameState.GameReady;
                gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "roundWin", false);
                gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "roundLoose", false);

                if (isCheckTimer)
                {
                    isCheckTimer = !isCheckTimer;
                    ResetRound();
                    Debug.Log("ResetRound");
                }
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

        // 플레이어 초기화
        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if ((string)player.CustomProperties["Character"] == "Observer") continue;

            PhotonView view = PhotonView.Find((int)player.CustomProperties["CharacterViewID"]);
            if (view == null) continue;
            GameCenterTest.ChangePlayerCustomProperties(player, "IsAlive", true);
            GameCenterTest.ChangePlayerCustomProperties(player, "ReSpawnTime", 100000000.0f);
            GameCenterTest.ChangePlayerCustomProperties(player, "UltimateCount", 0);

            view.RPC("AddUltimatePoint", RpcTarget.AllBuffered, 0);

            if ((string)player.CustomProperties["Team"] == "A")
            {
                view.RPC("PlayerReBornForAll", RpcTarget.All, (Vector3)player.CustomProperties["SpawnPoint"]);
            }

            else if ((string)player.CustomProperties["Team"] == "B")
            {
                view.RPC("PlayerReBornForAll", RpcTarget.All, (Vector3)player.CustomProperties["SpawnPoint"]);
            }

            view.RPC("PlayerReBornPersonal", player);
            gameCenter.deathUIView.RPC("DisableDeathCamUI", player);

            if ((string)player.CustomProperties["Team"] == "A")
            {
                foreach (var playerA in gameCenter.playersA)
                {
                    gameCenter.inGameUIView.RPC("ShowTeamLifeDead", playerA, (string)player.CustomProperties["Name"], false);
                }
            }

            else if ((string)player.CustomProperties["Team"] == "B")
            {
                foreach (var playerB in gameCenter.playersB)
                {
                    gameCenter.inGameUIView.RPC("ShowTeamLifeDead", playerB, (string)player.CustomProperties["Name"], false);
                }
            }

        }

    }
}
