using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class DuringRound : CenterState
{
    public override void StateExecution()
    {
        gameCenter.globalTimer += Time.deltaTime;

        //지역이 점령되어있으면 점령한 팀의 점령비율이 높아진다.
        if (gameCenter.currentOccupationTeam == gameCenter.teamA)
        {
            gameCenter.occupyingA.rate += Time.deltaTime * gameCenter.occupyingRate;//약 1.8초당 1씩 오름
            if (gameCenter.occupyingA.rate >= gameCenter.occupyingComplete)
                gameCenter.occupyingA.rate = gameCenter.occupyingComplete;
        }
        else if (gameCenter.currentOccupationTeam == gameCenter.teamB)
        {
            gameCenter.occupyingB.rate += Time.deltaTime * gameCenter.occupyingRate;
            if (gameCenter.occupyingB.rate >= gameCenter.occupyingComplete)
                gameCenter.occupyingB.rate = gameCenter.occupyingComplete;
        }

        OccupyAreaCounts();
        CheckPlayerReSpawn();
        CheckRoundEnd();
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
    {
         if (targetPlayer != null && changedProps != null)
         {
             //pararmeter로 변경된 key값을 찾는다.
             string param = (string)targetPlayer.CustomProperties["ParameterName"];
             targetPlayer.CustomProperties["ParameterName"] = "none";

            switch (param)
            {
                 case "TotalDamage":
                     if (gameCenter.globalUIView == null) break;
                    gameCenter.globalUIView.RPC("ShowDamageUI", targetPlayer);
                     break;
                 case "KillCount":
                     if (gameCenter.globalUIView == null) break;
                    gameCenter.globalUIView.RPC("ShowKillUI", targetPlayer, gameCenter.tempVictim);
                    gameCenter.globalUIView.RPC("ShowKillLog", RpcTarget.AllBufferedViaServer, targetPlayer.CustomProperties["Name"],
                         gameCenter.tempVictim, ((string)targetPlayer.CustomProperties["Team"] == "A"), targetPlayer.ActorNumber);
                     break;
                 case "DeadCount":
                    GameCenterTest.ChangePlayerCustomProperties (targetPlayer, "IsAlive", false);
                    GameCenterTest.ChangePlayerCustomProperties (targetPlayer, "ReSpawnTime", gameCenter.globalTimer + gameCenter.playerRespawnTime);

                    gameCenter.tempVictim = (string)targetPlayer.CustomProperties["Name"];
                    gameCenter.ShowTeamMateDead((string)targetPlayer.CustomProperties["Team"], (string)targetPlayer.CustomProperties["Name"]);

                     PhotonView view = PhotonView.Find((int)targetPlayer.CustomProperties["CharacterViewID"]);
                     if (view == null) break;

                     view.RPC("PlayerDeadForAll", RpcTarget.AllBufferedViaServer, (string)targetPlayer.CustomProperties["DamagePart"],
                         (Vector3)targetPlayer.CustomProperties["DamageDirection"], (float)targetPlayer.CustomProperties["DamageForce"]);
                     view.RPC("PlayerDeadPersonal", targetPlayer);
                     break;
                 default:
                     break;
             }

         }
        
    }

    void OccupyAreaCounts()//점령 지역에 플레이어가 몇 명 점령하고 있는지 확인
    {
        gameCenter.teamAOccupying = 0;
        gameCenter.teamBOccupying = 0;

        GameObject temp;

        for (int i = 0; i < gameCenter.playersA.Count; i++)
        {
            temp = gameCenter.FindObjectWithViewID((int)gameCenter.playersA[i].CustomProperties["CharacterViewID"]);

            if (temp.GetComponent<Character>().isOccupying == true)
            {
                gameCenter.teamAOccupying++;
            }
        }

        for (int i = 0; i < gameCenter.playersB.Count; i++)
        {
            temp = gameCenter.FindObjectWithViewID((int)gameCenter.playersB[i].CustomProperties["CharacterViewID"]);

            if (temp.GetComponent<Character>().isOccupying == true)
            {
                gameCenter.teamBOccupying++;
            }
        }

        if (gameCenter.teamAOccupying > 0 && gameCenter.teamBOccupying > 0)
        {
            //서로 교전 중이라는 것을 알림
            gameCenter.occupyingReturnTimer = 0f;
            //globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "fighting", true);
        }
        else if (gameCenter.teamAOccupying > 0)//A팀 점령
        {
            gameCenter.ChangeOccupyingRate(gameCenter.teamAOccupying, gameCenter.teamA);
            gameCenter.occupyingReturnTimer = 0f;
            //globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "fighting", false);
        }
        else if (gameCenter.teamBOccupying > 0)//B팀 점령
        {
            gameCenter.ChangeOccupyingRate(gameCenter.teamBOccupying, gameCenter.teamB);
            gameCenter.occupyingReturnTimer = 0f;
            //globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "fighting", false);
        }
        else
        {
            gameCenter.occupyingReturnTimer += Time.deltaTime;
            //globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "fighting", false);
        }

        if (gameCenter.occupyingReturnTimer >= gameCenter.occupyingReturnTime)
        {
            if (gameCenter.occupyingTeam.rate > 0f)
            {
                gameCenter.occupyingTeam.rate -= Time.deltaTime;
                if (gameCenter.occupyingTeam.rate < 0f)
                {
                    gameCenter.occupyingTeam.rate = 0f;
                    gameCenter.occupyingTeam.name = "";
                }
            }
        }

    }

    void CheckPlayerReSpawn()
    {
        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if ((bool)player.CustomProperties["IsAlive"] && (bool)player.CustomProperties["IsAlive"] == true) continue;
            if ((float)player.CustomProperties["ReSpawnTime"] <= gameCenter.globalTimer)
            {
                PhotonView view = PhotonView.Find((int)player.CustomProperties["CharacterViewID"]);
                GameCenterTest.ChangePlayerCustomProperties(player, "IsAlive", true);

                if ((string)player.CustomProperties["Team"] == "A")
                {
                    view.RPC("PlayerReBornForAll", RpcTarget.AllBufferedViaServer, (Vector3)player.CustomProperties["SpawnPoint"]);
                }

                else if ((string)player.CustomProperties["Team"] == "B")
                {
                    view.RPC("PlayerReBornForAll", RpcTarget.AllBufferedViaServer, (Vector3)player.CustomProperties["SpawnPoint"]);
                }

                view.RPC("PlayerReBornPersonal", player);

                // 팀원 부활 알리기

                if ((string)player.CustomProperties["Team"] == "A")
                {
                    foreach (var playerA in gameCenter.playersA)
                    {
                        gameCenter.globalUIView.RPC("ShowTeamLifeDead", playerA, (string)player.CustomProperties["Name"], false);
                    }
                }

                else if ((string)player.CustomProperties["Team"] == "B")
                {
                    foreach (var playerB in gameCenter.playersB)
                    {
                        gameCenter.globalUIView.RPC("ShowTeamLifeDead", playerB, (string)player.CustomProperties["Name"], false);
                    }
                }
            }
        }
    }

    void CheckRoundEnd()
    {
        if (gameCenter.occupyingA.rate >= gameCenter.occupyingComplete &&
            gameCenter.currentOccupationTeam == gameCenter.teamA && gameCenter.teamBOccupying <= 0)
        {
            gameCenter.globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "extraObj", true);
            gameCenter.globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "redExtraUI", false);
            gameCenter.globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "redExtraObj", true);
            gameCenter.roundEndTimer -= Time.deltaTime;

        }
        else if (gameCenter.occupyingB.rate >= gameCenter.occupyingComplete &&
            gameCenter.currentOccupationTeam == gameCenter.teamB && gameCenter.teamAOccupying <= 0)
        {
            gameCenter.globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "extraObj", true);
            gameCenter.globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "blueExtraUI", false);
            gameCenter.globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "blueExtraObj", true);
            gameCenter.roundEndTimer -= Time.deltaTime;
        }
        else
        {
            gameCenter.roundEndTimer = gameCenter.roundEndTime;
        }

        if (gameCenter.roundEndTimer <= 0.0f)
        {
            //라운드 종료
            if (gameCenter.currentOccupationTeam == gameCenter.teamA)
            {
                gameCenter.occupyingA.rate = 100;
                gameCenter.roundA++;

                if (gameCenter.roundA == 1)
                {
                    gameCenter.globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "redFirstPoint", true);

                    foreach (var player in gameCenter.playersA)
                    {
                        gameCenter.globalUIView.RPC("ShowRoundWin", player, gameCenter.roundA + gameCenter.roundB);
                    }

                    foreach (var player in gameCenter.playersB)
                    {
                        gameCenter.globalUIView.RPC("ShowRoundLoose", player, gameCenter.roundA + gameCenter.roundB);
                    }
                }

                else if (gameCenter.roundA == 2)
                {
                    gameCenter.globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "redSecondPoint", true);

                    foreach (var player in gameCenter.playersA)
                    {
                        gameCenter.globalUIView.RPC("ActiveUI", player, "victory", true);
                    }

                    foreach (var player in gameCenter.playersB)
                    {
                        gameCenter.globalUIView.RPC("ActiveUI", player, "defeat", true);
                    }
                }

            }

            else if (gameCenter.currentOccupationTeam == gameCenter.teamB)
            {
                gameCenter.occupyingB.rate = 100;
                gameCenter.roundB++;

                if (gameCenter.roundB == 1)
                {
                    gameCenter.globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "blueFirstPoint", true);

                    foreach (var player in gameCenter.playersB)
                    {
                        gameCenter.globalUIView.RPC("ShowRoundWin", player, gameCenter.roundA + gameCenter.roundB);
                    }

                    foreach (var player in gameCenter.playersA)
                    {
                        gameCenter.globalUIView.RPC("ShowRoundLoose", player, gameCenter.roundA + gameCenter.roundB);
                    }
                }

                else if (gameCenter.roundB == 2)
                {
                    gameCenter.globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "blueSecondPoint", true);

                    foreach (var player in gameCenter.playersB)
                    {
                        gameCenter.globalUIView.RPC("ActiveUI", player, "victory", true);
                    }

                    foreach (var player in gameCenter.playersA)
                    {
                        gameCenter.globalUIView.RPC("ActiveUI", player, "defeat", true);
                    }
                }
            }

            //라운드 종료
            gameCenter.currentGameState = GameCenterTest.GameState.RoundEnd;
            gameCenter.globalTimer = gameCenter.roundEndResultTime;

        }
    }


}
