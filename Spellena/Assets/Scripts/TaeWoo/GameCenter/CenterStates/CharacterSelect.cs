using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Player;

namespace GameCenterTest0
{
    public class CharacterSelect : CenterState
    {
        bool isOnce = true;
        bool isCheckTimer = false;
        float tempTimer = 0.0f;

        public override void StateExecution()
        {
            if (!isCheckTimer)
            {
                isCheckTimer = !isCheckTimer;
                tempTimer = GameCenterTest.globalTimer;
                gameCenter.photonView.RPC("SetGlobalDesiredTimer", RpcTarget.All, tempTimer + gameCenter.characterSelectTime);
                ConnectCharacterSelect();
            }

            if (isOnce)
            {
                GameCenterTest.globalTimer += Time.deltaTime;
                gameCenter.characterSelectView.RPC("ReceiveTimerCount", RpcTarget.AllBuffered, gameCenter.globalDesiredTimer - GameCenterTest.globalTimer);
            }

            if (GameCenterTest.globalTimer >= gameCenter.globalDesiredTimer && isOnce)
            {
                isOnce = false;

                MakeSpawnPoint();
                ConnectInGameUI();
                StartCoroutine(MakeCharacter());
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

        void ConnectInGameUI()
        {
            if (gameCenter.inGameUIObj != null)
            {
                gameCenter.inGameUI = gameCenter.inGameUIObj.GetComponent<InGameUI>();
                gameCenter.inGameUIView = gameCenter.inGameUIObj.GetComponent<PhotonView>();
            }
        }

        void MakeSpawnPoint()
        {
            gameCenter.playerSpawnA = GameCenterTest.FindObject(gameCenter.playerSpawnPoints, "TeamA").GetComponentsInChildren<Transform>(true);
            gameCenter.playerSpawnB = GameCenterTest.FindObject(gameCenter.playerSpawnPoints, "TeamB").GetComponentsInChildren<Transform>(true);
        }

        IEnumerator MakeCharacter()
        {
            int aTeamIndex = 1;
            int bTeamIndex = 1;

            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                yield return new WaitForSeconds(0.2f);
                gameCenter.photonView.RPC("ActiveObject", player, "inGameUIObj", true);
                gameCenter.photonView.RPC("ActiveObject", player, "characterSelectObj", false);

                string choseCharacter = (string)player.CustomProperties["Character"];
                if (choseCharacter == null)
                {
                    GameCenterTest.ChangePlayerCustomProperties(player, "Character", "Observer");
                    choseCharacter = "Observer";

                    //GameCenterTest.ChangePlayerCustomProperties(player, "Character", "Aeterna");
                    //choseCharacter = "Aeterna";
                }

                if ((string)player.CustomProperties["Team"] == "A")     // A ÆÀ (Red)
                {
                    GameObject playerCharacter = PhotonNetwork.Instantiate("Characters/" + choseCharacter,
                        gameCenter.playerSpawnA[aTeamIndex].position, Quaternion.identity);
                    if (playerCharacter == null) continue;

                    PhotonView[] views = playerCharacter.GetComponentsInChildren<PhotonView>();

                    // foreach - > for ¹®À¸·Î
                    foreach (PhotonView view in views)
                    {
                        view.TransferOwnership(player.ActorNumber);
                    }

                    if (choseCharacter != "Observer")
                    {
                        playerCharacter.GetComponent<PhotonView>().RPC("IsLocalPlayer", player);
                        playerCharacter.GetComponent<Character>().SetTagServer("TeamA");
                    }

                    else
                    {
                        playerCharacter.GetComponent<PhotonView>().RPC("SetTag", RpcTarget.All, "TeamA");
                        gameCenter.inGameUIView.RPC("DisActiveCrosshair", player);
                        playerCharacter.GetComponent<PhotonView>().RPC("ActiveObserver", player);
                    }

                    playerCharacter.GetComponent<PhotonView>().RPC("ChangeName", RpcTarget.All, (string)player.CustomProperties["Name"]);

                    GameCenterTest.ChangePlayerCustomProperties(player, "CharacterViewID", playerCharacter.GetComponent<PhotonView>().ViewID);
                    GameCenterTest.ChangePlayerCustomProperties(player, "SpawnPoint", gameCenter.playerSpawnA[aTeamIndex].position);
                    aTeamIndex++;
                    gameCenter.playersA.Add(player);
                }

                else if ((string)player.CustomProperties["Team"] == "B")    // B ÆÀ (Blue)
                {
                    GameObject playerCharacter = PhotonNetwork.Instantiate("Characters/" + choseCharacter,
                        gameCenter.playerSpawnB[bTeamIndex].position, Quaternion.identity);
                    if (playerCharacter == null) continue;

                    PhotonView[] views = playerCharacter.GetComponentsInChildren<PhotonView>();

                    foreach (var view in views)
                    {
                        view.TransferOwnership(player.ActorNumber);
                    }

                    if (choseCharacter != "Observer")
                    {
                        playerCharacter.GetComponent<PhotonView>().RPC("IsLocalPlayer", player);
                        playerCharacter.GetComponent<Character>().SetTagServer("TeamB");
                    }

                    else
                    {
                        playerCharacter.GetComponent<PhotonView>().RPC("SetTag", RpcTarget.All, "TeamB");
                        gameCenter.inGameUIView.RPC("DisActiveCrosshair", player);
                        playerCharacter.GetComponent<PhotonView>().RPC("ActiveObserver", player);
                    }

                    playerCharacter.GetComponent<PhotonView>().RPC("ChangeName", RpcTarget.All, (string)player.CustomProperties["Name"]);

                    GameCenterTest.ChangePlayerCustomProperties(player, "CharacterViewID", playerCharacter.GetComponent<PhotonView>().ViewID);
                    GameCenterTest.ChangePlayerCustomProperties(player, "SpawnPoint", gameCenter.playerSpawnB[bTeamIndex].position);
                    bTeamIndex++;
                    gameCenter.playersB.Add(player);
                }
            }

            // MakeTeamStateUI
            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                yield return new WaitForSeconds(0.2f);
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

            gameCenter.photonView.RPC("BetweenBGMPlay", RpcTarget.AllBuffered, false);
            gameCenter.photonView.RPC("BetweenBGMVolumControl", RpcTarget.AllBuffered, 1.0f, true);

            gameCenter.currentGameState = GameCenterTest.GameState.GameReady;
            Debug.Log("GameReady in CharacterSelect");
        }

    }
}