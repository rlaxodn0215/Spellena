using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Player;

public class CharacterSelect : CenterState
{
    public override void StateExecution()
    {
        gameCenter.gameStateString = "캐릭터 선택";

        // 캐릭터 선택
        gameCenter.globalTimer -= Time.deltaTime;

        if (gameCenter.globalTimer <= 0.0f)
        {
            // 선택한 캐릭터로 소환 및 태그 설정
            // 적 플레이어는 빨강 쉐이더 적용

            gameCenter.globalTimer = gameCenter.readyTime;
            gameCenter.currentGameState = GameCenterTest.GameState.GameReady;
            gameCenter.globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "inGameUI", true);

            MakeCharacter();
            MakeTeamStateUI();
        }
    }
    void MakeCharacter()
    {
        int aTeamIndex = 1;
        int bTeamIndex = 1;

        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            //현재 캐릭터는 에테르나 고정, 접속 순서에 따라 A,B 팀 나뉨
            //if((string)player.CustomProperties["Team"]=="A")
            string choseCharacter = "Aeterna";

            if (player.ActorNumber % 2 == 0)     // A 팀 (Red)
            {
                GameObject playerCharacter
                    = PhotonNetwork.Instantiate("TaeWoo/Prefabs/" + choseCharacter, gameCenter.playerSpawnA[aTeamIndex].position, Quaternion.identity);

                playerCharacter.GetComponent<PhotonView>().TransferOwnership(player.ActorNumber);
                playerCharacter.GetComponent<PhotonView>().RPC("IsLocalPlayer", player);
                playerCharacter.GetComponent<Character>().SetTagServer("TeamA");

                GameCenterTest.ChangePlayerCustomProperties(player, "CharacterViewID", playerCharacter.GetComponent<PhotonView>().ViewID);
                GameCenterTest.ChangePlayerCustomProperties(player, "Team", "A");
                GameCenterTest.ChangePlayerCustomProperties(player, "SpawnPoint", gameCenter.playerSpawnA[aTeamIndex].position);
                aTeamIndex++;
                gameCenter.playersA.Add(player);
            }

            else                // B 팀 (Blue)
            {
                GameObject playerCharacter
                    = PhotonNetwork.Instantiate("TaeWoo/Prefabs/" + choseCharacter, gameCenter.playerSpawnB[bTeamIndex].position, Quaternion.identity);

                playerCharacter.GetComponent<PhotonView>().TransferOwnership(player.ActorNumber);
                playerCharacter.GetComponent<PhotonView>().RPC("IsLocalPlayer", player);
                playerCharacter.GetComponent<Character>().SetTagServer("TeamB");

                GameCenterTest.ChangePlayerCustomProperties(player, "CharacterViewID", playerCharacter.GetComponent<PhotonView>().ViewID);
                GameCenterTest.ChangePlayerCustomProperties(player, "Team", "B");
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
                    gameCenter.globalUIView.RPC("ShowTeamState", player, playerA.CustomProperties["Name"], "Aeterna");
                }
            }

            else if ((string)player.CustomProperties["Team"] == "B")
            {
                foreach (var playerB in gameCenter.playersB)
                {
                    gameCenter.globalUIView.RPC("ShowTeamState", player, playerB.CustomProperties["Name"], "Aeterna");
                }
            }
        }
    }


}
