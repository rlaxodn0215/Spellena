using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameReady : CenterState
{
    public override void StateExecution()
    {
        gameCenter.gameStateString = "Ready";

        gameCenter.globalTimer -= Time.deltaTime;
        if (gameCenter.globalTimer <= 0.0f)
        {
            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                // 적 쉐이더 적용
                PhotonView view = PhotonView.Find((int)player.CustomProperties["CharacterViewID"]);
                view.RPC("SetEnemyLayer", player);

                // 어시스트 타이머 설정
                Hashtable playerData = player.CustomProperties;
                // 어시스트 타이머 할당 -> AssistTime_ + 적 플레이어 캐릭터 ViewID
                if((string)player.CustomProperties["Team"] == "A")
                {
                    Dictionary<string, float> temp = new Dictionary<string, float>();
                    Dictionary<string, float> temp1 = new Dictionary<string, float>();

                    foreach (var enemyPlayer in gameCenter.playersB)
                    {
                        temp.Add("AssistTime_" + enemyPlayer.CustomProperties["CharacterViewID"], 0.0f);
                    }

                    playerData.Add("DealAssist", temp);

                    foreach (var enemyPlayer in gameCenter.playersA)
                    {
                        temp1.Add("AssistTime_" + enemyPlayer.CustomProperties["CharacterViewID"], 0.0f);
                    }

                    playerData.Add("HealAssist", temp1);
                }

                else if ((string)player.CustomProperties["Team"] == "B")
                {
                    Dictionary<string, float> temp = new Dictionary<string, float>();
                    Dictionary<string, float> temp1 = new Dictionary<string, float>();

                    foreach (var enemyPlayer in gameCenter.playersA)
                    {
                        temp.Add("AssistTime_" + enemyPlayer.CustomProperties["CharacterViewID"], 0.0f);
                    }

                    playerData.Add("DealAssist", temp);

                    foreach (var enemyPlayer in gameCenter.playersB)
                    {
                        temp1.Add("AssistTime_" + enemyPlayer.CustomProperties["CharacterViewID"], 0.0f);
                    }

                    playerData.Add("HealAssist", temp1);
                }

                player.SetCustomProperties(playerData);
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
