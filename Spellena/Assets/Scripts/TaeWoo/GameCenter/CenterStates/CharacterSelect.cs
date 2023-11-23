using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Player;

public class CharacterSelect : CenterState
{
    bool isCheckTimer = false;
    float tempTimer = 0.0f;

    float soundDecreaseTime = 5;
    float soundDecreaseSpeed = 1.5f;

    public override void StateExecution()
    {
        if (!isCheckTimer)
        {
            isCheckTimer = !isCheckTimer;
            tempTimer = gameCenter.globalTimer;
            gameCenter.globalDesiredTimer = tempTimer + gameCenter.characterSelectTime;
            ConnectCharacterSelect();
        }

        gameCenter.globalTimer += Time.deltaTime;

        gameCenter.characterSelectView.RPC("ReceiveTimerCount", RpcTarget.AllBufferedViaServer, gameCenter.globalDesiredTimer - gameCenter.globalTimer);

        // 캐릭터 선택

        //if(gameCenter.globalDesiredTimer - gameCenter.globalTimer <= soundDecreaseTime)
        //{
        //    if (gameCenter.bgmManagerView != null)
        //    {
        //        gameCenter.bgmManagerView.RPC("VolumeControl", RpcTarget.AllBufferedViaServer, soundDecreaseSpeed * Time.deltaTime / 10, false);
        //    }
        //}

        if (gameCenter.globalTimer >= gameCenter.globalDesiredTimer)
        {
            MakeSpawnPoint();
            MakeTeamStateUI();
            ConnectInGameUI();
            MakeCharacter();

            //gameCenter.bgmManagerView.RPC("PlayBGM", RpcTarget.AllBufferedViaServer, "DuringRound", 0.3f, true);
            gameCenter.currentGameState = GameCenterTest.GameState.GameReady;
        }
    }

    void ConnectCharacterSelect()
    {
        if (gameCenter.characterSelectObj != null)
        {
            gameCenter.characterSelectView = gameCenter.characterSelectObj.GetComponent<PhotonView>();
            gameCenter.characterSelect = gameCenter.characterSelectObj.GetComponent<SelectingCharacter>();
        }
    }

    void MakeCharacter()
    {
        int aTeamIndex = 1;
        int bTeamIndex = 1;

        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            //string choseCharacter = "Aeterna";
            // 캐릭터 프리팹 한 파일로 통일
            string choseCharacter = (string)player.CustomProperties["Character"];
            if (choseCharacter == null) GameCenterTest.ChangePlayerCustomProperties(player, "Character", "Aeterna");

            if ((string)player.CustomProperties["Team"]=="A")     // A 팀 (Red)
            {
                GameObject playerCharacter;

                if (choseCharacter=="Aeterna")
                {
                    playerCharacter = PhotonNetwork.Instantiate("TaeWoo/Prefabs/" + choseCharacter, gameCenter.playerSpawnA[aTeamIndex].position, Quaternion.identity);
                }

                else
                {
                    playerCharacter = PhotonNetwork.Instantiate("ChanYoung/Prefabs/" + choseCharacter, gameCenter.playerSpawnA[aTeamIndex].position, Quaternion.identity);
                }

                playerCharacter.GetComponent<PhotonView>().TransferOwnership(player.ActorNumber);
                playerCharacter.GetComponent<PhotonView>().RPC("IsLocalPlayer", player);
                playerCharacter.GetComponent<Character>().SetTagServer("TeamA");

                GameCenterTest.ChangePlayerCustomProperties(player, "CharacterViewID", playerCharacter.GetComponent<PhotonView>().ViewID);
                GameCenterTest.ChangePlayerCustomProperties(player, "SpawnPoint", gameCenter.playerSpawnA[aTeamIndex].position);
                aTeamIndex++;
                gameCenter.playersA.Add(player);
            }

            else if((string)player.CustomProperties["Team"] == "B")    // B 팀 (Blue)
            {
                GameObject playerCharacter;

                if (choseCharacter == "Aeterna")
                {
                    playerCharacter = PhotonNetwork.Instantiate("TaeWoo/Prefabs/" + choseCharacter, gameCenter.playerSpawnB[bTeamIndex].position, Quaternion.identity);
                }

                else
                {
                    playerCharacter = PhotonNetwork.Instantiate("ChanYoung/Prefabs/" + choseCharacter, gameCenter.playerSpawnB[bTeamIndex].position, Quaternion.identity);
                }

                playerCharacter.GetComponent<PhotonView>().TransferOwnership(player.ActorNumber);
                playerCharacter.GetComponent<PhotonView>().RPC("IsLocalPlayer", player);
                playerCharacter.GetComponent<Character>().SetTagServer("TeamB");

                GameCenterTest.ChangePlayerCustomProperties(player, "CharacterViewID", playerCharacter.GetComponent<PhotonView>().ViewID);
                GameCenterTest.ChangePlayerCustomProperties(player, "SpawnPoint", gameCenter.playerSpawnB[bTeamIndex].position);
                bTeamIndex++;
                gameCenter.playersB.Add(player);
            }
        }
    }

    void MakeTeamStateUI()
    {
        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if ((string)player.CustomProperties["Team"] == "A")
            {
                foreach (var playerA in gameCenter.playersA)
                {
                    gameCenter.inGameUIView.RPC("ShowTeamState", player, playerA.CustomProperties["Name"], "Aeterna");
                }
            }

            else if ((string)player.CustomProperties["Team"] == "B")
            {
                foreach (var playerB in gameCenter.playersB)
                {
                    gameCenter.inGameUIView.RPC("ShowTeamState", player, playerB.CustomProperties["Name"], "Aeterna");
                }
            }
        }
    }

    void ConnectInGameUI()
    {
        if (gameCenter.inGameUIObj != null)
        {
            gameCenter.inGameUI = gameCenter.inGameUIObj.GetComponent<InGameUI>();
            gameCenter.inGameUIView = gameCenter.inGameUIObj.GetComponent<PhotonView>();

            gameCenter.photonView.RPC("ActiveObject", RpcTarget.AllBufferedViaServer, "inGameUIObj", true);
            gameCenter.photonView.RPC("ActiveObject", RpcTarget.AllBufferedViaServer, "characterSelectObj", false);
        }
    }

    void MakeSpawnPoint()
    {
        gameCenter.playerSpawnA = GameCenterTest.FindObject(gameCenter.playerSpawnPoints, "TeamA").GetComponentsInChildren<Transform>(true);
        gameCenter.playerSpawnB = GameCenterTest.FindObject(gameCenter.playerSpawnPoints, "TeamB").GetComponentsInChildren<Transform>(true);
    }


}
