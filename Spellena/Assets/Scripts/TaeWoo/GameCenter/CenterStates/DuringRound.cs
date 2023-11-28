using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class DuringRound : CenterState
{
    //public enum RoundState
    //{
    //    None,
    //    StandBy,
    //    RoundStart,
    //    Occupying,
    //    Occupied,
    //    RoundEnd
    //}

    //public RoundState currentRoundState = RoundState.None;
    //public RoundState updateRoundState = RoundState.StandBy;

    bool checkRoundEndOnce = true;
    bool OccupyBarCountOnce = true;

    public override void StateExecution()
    {
        GameCenterTest.globalTimer += Time.deltaTime;

        OccupyBarCount();
        OccupyAreaCounts();
        CheckPlayerReSpawn();
        CheckRoundEnd();
    }

    void OccupyBarCount()
    {
        //지역이 점령되어있으면 점령한 팀의 점령비율이 높아진다.
        if (gameCenter.currentOccupationTeam == gameCenter.teamA)
        {
            gameCenter.occupyingA.rate += Time.deltaTime * gameCenter.occupyingRate;//약 1.8초당 1씩 오름

            if(OccupyBarCountOnce)
            {
                //Debug.Log("A OccupyBarCount");
                gameCenter.bgmManagerView.RPC("PlayBGM", RpcTarget.AllBufferedViaServer, "Occupying", 0.7f, true);
                OccupyBarCountOnce = false;
            }

            if (gameCenter.occupyingA.rate >= gameCenter.occupyingComplete)
                gameCenter.occupyingA.rate = gameCenter.occupyingComplete;
        }

        else if (gameCenter.currentOccupationTeam == gameCenter.teamB)
        {
            gameCenter.occupyingB.rate += Time.deltaTime * gameCenter.occupyingRate;

            if (OccupyBarCountOnce)
            {
                gameCenter.bgmManagerView.RPC("PlayBGM", RpcTarget.AllBufferedViaServer, "Occupying", 0.7f, true);
                OccupyBarCountOnce = false;
            }

            if (gameCenter.occupyingB.rate >= gameCenter.occupyingComplete)
                gameCenter.occupyingB.rate = gameCenter.occupyingComplete;
        }
    }


    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
    {
         if (targetPlayer != null && changedProps != null)
         {
             //pararmeter로 변경된 key값을 찾는다.
             string param = (string)targetPlayer.CustomProperties["ParameterName"];
             targetPlayer.CustomProperties["ParameterName"] = "";

            PhotonView view = PhotonView.Find((int)targetPlayer.CustomProperties["CharacterViewID"]);
            if (view == null) return;

            switch (param)
            {
                 case "TotalDamage":
                     if (gameCenter.inGameUIView == null) break;
                    gameCenter.inGameUIView.RPC("ShowDamageUI", targetPlayer);
                    // 해당 플레이어에 대한 어시스트 타이머 연결
                    string victimViewID = (string)targetPlayer.CustomProperties["PlayerAssistViewID"];
                    if (victimViewID == null) break;
                    targetPlayer.CustomProperties["PlayerAssistViewID"] = "";

                    Dictionary<string, float> temp = (Dictionary<string, float>)targetPlayer.CustomProperties["DealAssist"];
                    temp["AssistTime_" + victimViewID] = GameCenterTest.globalTimer + gameCenter.assistTime;
                    GameCenterTest.ChangePlayerCustomProperties(targetPlayer, "DealAssist", temp);
                    break;
                case "TotalHeal":
                    string healedViewID = (string)targetPlayer.CustomProperties["PlayerAssistViewID"];
                    if (healedViewID == null) break;
                    targetPlayer.CustomProperties["PlayerAssistViewID"] = "";

                    Dictionary<string, float> temp1 = (Dictionary<string, float>)targetPlayer.CustomProperties["HealAssist"];
                    temp1["AssistTime_" + healedViewID] = GameCenterTest.globalTimer + gameCenter.assistTime;
                    GameCenterTest.ChangePlayerCustomProperties(targetPlayer, "HealAssist", temp1);
                    break;
                 case "KillCount":
                     if (gameCenter.inGameUIView == null) break;
                    gameCenter.inGameUIView.RPC("ShowKillUI", targetPlayer, gameCenter.tempVictim);
                    gameCenter.inGameUIView.RPC("ShowKillLog", RpcTarget.AllBufferedViaServer, targetPlayer.CustomProperties["Name"],
                         gameCenter.tempVictim, ((string)targetPlayer.CustomProperties["Team"] == "A"), targetPlayer.ActorNumber);
                    CheckPlayerHealAssist(targetPlayer);
                    view.RPC("SetUltimatePoint", targetPlayer);
                    break;
                 case "DeadCount":
                    GameCenterTest.ChangePlayerCustomProperties (targetPlayer, "IsAlive", false);
                    GameCenterTest.ChangePlayerCustomProperties (targetPlayer, "ReSpawnTime", GameCenterTest.globalTimer + gameCenter.playerRespawnTime);

                    gameCenter.tempVictim = (string)targetPlayer.CustomProperties["Name"];
                    ShowTeamMateDead((string)targetPlayer.CustomProperties["Team"], (string)targetPlayer.CustomProperties["Name"]);

                     view.RPC("PlayerDeadForAll", RpcTarget.AllBufferedViaServer, (string)targetPlayer.CustomProperties["DamagePart"],
                         (Vector3)targetPlayer.CustomProperties["DamageDirection"], (float)targetPlayer.CustomProperties["DamageForce"]);
                     view.RPC("PlayerDeadPersonal", targetPlayer);
                    gameCenter.deathUIView.RPC("ShowKillerData", targetPlayer, (string)targetPlayer.CustomProperties["KillerName"]);

                    CheckPlayerDealAssist(targetPlayer,(string)targetPlayer.CustomProperties["KillerName"]);
                    break;
                case "AngelStatueCoolTime":
                    if(GameCenterTest.globalTimer >= (float)targetPlayer.CustomProperties["AngelStatueCoolTime"])
                    {
                        GameCenterTest.ChangePlayerCustomProperties(targetPlayer, "AngelStatueCoolTime", GameCenterTest.globalTimer + gameCenter.angelStatueCoolTime);
                        StartCoroutine(AngelStatue(targetPlayer));
                    }
                    break;
                 default:
                     break;
             }

         }
        
    }

    IEnumerator AngelStatue(Photon.Realtime.Player player)
    {
        for(int i = 0; i < gameCenter.angelStatueContinueTime; i++)
        {
            PhotonView view = PhotonView.Find((int)player.CustomProperties["CharacterViewID"]);
            if (view == null) break;

            view.RPC("AngelStatueHP", RpcTarget.AllBufferedViaServer, gameCenter.angelStatueHpPerTime);
            yield return new WaitForSeconds(1);
        }
    }

    void OccupyAreaCounts()//점령 지역에 플레이어가 몇 명 점령하고 있는지 확인
    {
        gameCenter.teamAOccupying = 0;
        gameCenter.teamBOccupying = 0;

        GameObject temp;

        for (int i = 0; i < gameCenter.playersA.Count; i++)
        {
            temp = GameCenterTest.FindObjectWithViewID((int)gameCenter.playersA[i].CustomProperties["CharacterViewID"]);

            if (temp.GetComponent<Character>().isOccupying == true)
            {
                gameCenter.teamAOccupying++;
            }
        }

        for (int i = 0; i < gameCenter.playersB.Count; i++)
        {
            temp = GameCenterTest.FindObjectWithViewID((int)gameCenter.playersB[i].CustomProperties["CharacterViewID"]);

            if (temp.GetComponent<Character>().isOccupying == true)
            {
                gameCenter.teamBOccupying++;
            }
        }

        if (gameCenter.teamAOccupying > 0 && gameCenter.teamBOccupying > 0)
        {
            //서로 교전 중이라는 것을 알림
            gameCenter.occupyingReturnTimer = 0f;
            //gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "fighting", true);
        }
        else if (gameCenter.teamAOccupying > 0)//A팀 점령
        {
            ChangeOccupyingRate(gameCenter.teamAOccupying, gameCenter.teamA);
            gameCenter.occupyingReturnTimer = 0f;
            
            //gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "fighting", false);
        }
        else if (gameCenter.teamBOccupying > 0)//B팀 점령
        {
            ChangeOccupyingRate(gameCenter.teamBOccupying, gameCenter.teamB);
            gameCenter.occupyingReturnTimer = 0f;
            //gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "fighting", false);
        }
        else
        {
            gameCenter.occupyingReturnTimer += Time.deltaTime;
            //gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "fighting", false);
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
            if ((float)player.CustomProperties["ReSpawnTime"] <= GameCenterTest.globalTimer)
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
                gameCenter.deathUIView.RPC("DisableDeathCamUI", player);

                // 팀원 부활 알리기

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

    void CheckPlayerDealAssist(Photon.Realtime.Player player, string killerName)
    {
        // player은 죽은 플레이어
        if((string)player.CustomProperties["Team"]=="A")
        {
            foreach(var teamPlayer in gameCenter.playersB)
            {
                // 살인자 제외
                if ((string)teamPlayer.CustomProperties["Name"] == killerName)
                {
                    //Debug.LogError("killerName");
                    continue;
                }

                foreach (var assist in (Dictionary<string, float>)teamPlayer.CustomProperties["DealAssist"])
                {
                    if (assist.Value >= GameCenterTest.globalTimer)
                    {
                        //Debug.LogError("Deal Assist!!");
                        PhotonView view = PhotonView.Find((int)teamPlayer.CustomProperties["CharacterViewID"]);
                        if (view == null) continue;
                        view.RPC("SetChargePoint", teamPlayer);
                        int temp = (int)teamPlayer.CustomProperties["AsisstCount"];
                        GameCenterTest.ChangePlayerCustomProperties(teamPlayer, "Asisst", temp + 1);
                    }
                }
            }
        }

        else if((string)player.CustomProperties["Team"] == "B")
        {
            foreach (var teamPlayer in gameCenter.playersA)
            {
                // 살인자 제외
                if ((string)teamPlayer.CustomProperties["Name"] == killerName)
                {
                    //Debug.LogError("killerName");
                    continue;
                }

                foreach (var assist in (Dictionary<string, float>)teamPlayer.CustomProperties["DealAssist"])
                {
                    if (assist.Value >= GameCenterTest.globalTimer)
                    {
                        //Debug.LogError("Deal Assist!!");
                        PhotonView view = PhotonView.Find((int)teamPlayer.CustomProperties["CharacterViewID"]);
                        if (view == null) continue;
                        view.RPC("SetChargePoint", teamPlayer);
                        int temp = (int)teamPlayer.CustomProperties["AsisstCount"];
                        GameCenterTest.ChangePlayerCustomProperties(teamPlayer, "Asisst", temp + 1);
                    }
                }
            }
        }
        
    }

    void CheckPlayerHealAssist(Photon.Realtime.Player player)
    {
        if ((string)player.CustomProperties["Team"] == "A")
        {
            foreach (var teamPlayer in gameCenter.playersA)
            {
                foreach (var assist in (Dictionary<string, float>)teamPlayer.CustomProperties["HealAssist"])
                {
                    if (assist.Value >= GameCenterTest.globalTimer)
                    {
                        //Debug.LogError("Heal Assist!!");
                        PhotonView view = PhotonView.Find((int)teamPlayer.CustomProperties["CharacterViewID"]);
                        if (view == null) continue;
                        view.RPC("SetChargePoint", teamPlayer);
                        int temp = (int)teamPlayer.CustomProperties["AsisstCount"];
                        GameCenterTest.ChangePlayerCustomProperties(teamPlayer, "Asisst", temp + 1);
                    }
                }
            }
        }

        else if ((string)player.CustomProperties["Team"] == "B")
        {
            foreach (var teamPlayer in gameCenter.playersB)
            {
                foreach (var assist in (Dictionary<string, float>)teamPlayer.CustomProperties["HealAssist"])
                {
                    if (assist.Value >= GameCenterTest.globalTimer)
                    {
                        //Debug.LogError("Heal Assist!!");
                        PhotonView view = PhotonView.Find((int)teamPlayer.CustomProperties["CharacterViewID"]);
                        if (view == null) continue;
                        view.RPC("SetChargePoint", teamPlayer);
                        int temp = (int)teamPlayer.CustomProperties["AsisstCount"];
                        GameCenterTest.ChangePlayerCustomProperties(teamPlayer, "Asisst", temp + 1);
                    }
                }
            }
        }
    }

    public void ShowTeamMateDead(string team, string deadName)
    {
        if (team == "A")
        {
            foreach (var player in gameCenter.playersA)
            {
                gameCenter.inGameUIView.RPC("ShowTeamLifeDead", player, deadName, true);
            }
        }

        else if (team == "B")
        {
            foreach (var player in gameCenter.playersB)
            {
                gameCenter.inGameUIView.RPC("ShowTeamLifeDead", player, deadName, true);
            }
        }
    }

    public void ChangeOccupyingRate(int num, string name) //점령 게이지 변화
    {
        if (gameCenter.occupyingTeam.name == name)
        {
            if (gameCenter.currentOccupationTeam == name)
                return;
            gameCenter.occupyingTeam.rate += gameCenter.occupyingGaugeRate * Time.deltaTime;
            if (gameCenter.occupyingTeam.rate >= 100)
            {
                gameCenter.currentOccupationTeam = name;
                gameCenter.occupyingTeam.name = "";
                gameCenter.occupyingTeam.rate = 0f;

                gameCenter.bgmManagerView.RPC("PlayBGM", RpcTarget.AllBufferedViaServer, "Occupation", 1.0f, false);

                if (gameCenter.currentOccupationTeam == "A")
                {
                    gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "captured_Red", true);
                    gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "captured_Blue", false);
                    gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "extraObj", false);
                    gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "blueExtraObj", false);
                    gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "blueExtraUI", true);

                    gameCenter.angleStatue.GetComponent<PhotonView>().RPC("ChangeTeam", RpcTarget.AllBufferedViaServer, "A");
                }

                else if (gameCenter.currentOccupationTeam == "B")
                {
                    gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "captured_Red", false);
                    gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "captured_Blue", true);
                    gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "extraObj", false);
                    gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "redExtraObj", false);
                    gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "redExtraUI", true);

                    gameCenter.angleStatue.GetComponent<PhotonView>().RPC("ChangeTeam", RpcTarget.AllBufferedViaServer, "B");

                }
            }
        }
        else if (gameCenter.occupyingTeam.name == "")
        {
            if (gameCenter.currentOccupationTeam == name)
                return;
            gameCenter.occupyingTeam.name = name;
            gameCenter.occupyingTeam.rate += gameCenter.occupyingGaugeRate * Time.deltaTime;
        }
        else
        {
            gameCenter.occupyingTeam.rate -= gameCenter.occupyingGaugeRate * Time.deltaTime;
            if (gameCenter.occupyingTeam.rate < 0)
            {
                gameCenter.occupyingTeam.name = "";
                gameCenter.occupyingTeam.rate = 0;
            }
        }
    }

    void CheckRoundEnd()
    {
        if (gameCenter.occupyingA.rate >= gameCenter.occupyingComplete &&
            gameCenter.currentOccupationTeam == gameCenter.teamA && gameCenter.teamBOccupying <= 0)
        {
            if(checkRoundEndOnce)
            {
                gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "extraObj", true);
                gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "redExtraUI", false);
                gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "redExtraObj", true);
                gameCenter.bgmManagerView.RPC("PlayBGM", RpcTarget.AllBufferedViaServer, "RoundAlmostEnd", 1.0f, true);
                checkRoundEndOnce = false;
            }

            gameCenter.roundEndTimer -= Time.deltaTime;
        }

        else if (gameCenter.occupyingB.rate >= gameCenter.occupyingComplete &&
            gameCenter.currentOccupationTeam == gameCenter.teamB && gameCenter.teamAOccupying <= 0)
        {
            if (checkRoundEndOnce)
            {
                gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "extraObj", true);
                gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "blueExtraUI", false);
                gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "blueExtraObj", true);
                gameCenter.bgmManagerView.RPC("PlayBGM", RpcTarget.AllBufferedViaServer, "RoundAlmostEnd", 1.0f, true);
                checkRoundEndOnce = false;
            }

            gameCenter.roundEndTimer -= Time.deltaTime;
        }

        else
        {
            gameCenter.roundEndTimer = gameCenter.roundEndTime;
            checkRoundEndOnce = true;
        }

        if (gameCenter.roundEndTimer <= 0.0f)
        {
            //라운드 종료
            if (gameCenter.currentOccupationTeam == gameCenter.teamA)
            {
                gameCenter.occupyingA.rate = 100;
                GameCenterTest.roundA++;

                if (GameCenterTest.roundA == 1)
                {
                    gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "redFirstPoint", true);

                    foreach (var player in gameCenter.playersA)
                    {
                        gameCenter.inGameUIView.RPC("ShowRoundWin", player, GameCenterTest.roundA + GameCenterTest.roundB);
                        gameCenter.bgmManagerView.RPC("PlayBGM", player, "RoundWin", 1.0f, false);
                    }

                    foreach (var player in gameCenter.playersB)
                    {
                        gameCenter.inGameUIView.RPC("ShowRoundLoose", player, GameCenterTest.roundA + GameCenterTest.roundB);
                        gameCenter.bgmManagerView.RPC("PlayBGM", player, "RoundLoose", 1.0f, false);
                    }
                }

                else if (GameCenterTest.roundA == 2)
                {
                    gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "redSecondPoint", true);

                    foreach (var player in gameCenter.playersA)
                    {
                        gameCenter.inGameUIView.RPC("ActiveInGameUIObj", player, "victory", true);
                        gameCenter.bgmManagerView.RPC("PlayBGM", player, "RoundWin", 1.0f, false);
                    }

                    foreach (var player in gameCenter.playersB)
                    {
                        gameCenter.inGameUIView.RPC("ActiveInGameUIObj", player, "defeat", true);
                        gameCenter.bgmManagerView.RPC("PlayBGM", player, "RoundLoose", 1.0f, false);
                    }
                }

            }

            else if (gameCenter.currentOccupationTeam == gameCenter.teamB)
            {
                gameCenter.occupyingB.rate = 100;
                GameCenterTest.roundB++;

                if (GameCenterTest.roundB == 1)
                {
                    gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "blueFirstPoint", true);

                    foreach (var player in gameCenter.playersB)
                    {
                        gameCenter.inGameUIView.RPC("ShowRoundWin", player, GameCenterTest.roundA + GameCenterTest.roundB);
                        gameCenter.bgmManagerView.RPC("PlayBGM", player, "RoundWin", 1.0f, false);
                    }

                    foreach (var player in gameCenter.playersA)
                    {
                        gameCenter.inGameUIView.RPC("ShowRoundLoose", player, GameCenterTest.roundA + GameCenterTest.roundB);
                        gameCenter.bgmManagerView.RPC("PlayBGM", player, "RoundLoose", 1.0f, false);
                    }
                }

                else if (GameCenterTest.roundB == 2)
                {
                    gameCenter.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.AllBufferedViaServer, "blueSecondPoint", true);

                    foreach (var player in gameCenter.playersB)
                    {
                        gameCenter.inGameUIView.RPC("ActiveInGameUIObj", player, "victory", true);
                        gameCenter.bgmManagerView.RPC("PlayBGM", player, "RoundWin", 1.0f, false);
                    }

                    foreach (var player in gameCenter.playersA)
                    {
                        gameCenter.inGameUIView.RPC("ActiveInGameUIObj", player, "defeat", true);
                        gameCenter.bgmManagerView.RPC("PlayBGM", player, "RoundLoose", 1.0f, false);
                    }
                }
            }

            //라운드 종료
            gameCenter.currentGameState = GameCenterTest.GameState.RoundEnd;

        }
    }


}
