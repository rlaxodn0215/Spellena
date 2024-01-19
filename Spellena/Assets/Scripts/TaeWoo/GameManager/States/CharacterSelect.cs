using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace FSM
{
    public class CharacterSelect : BaseState
    {
        public CharacterSelect(StateMachine stateMachine) :
            base("CharacterSelect", stateMachine)
        { }

        public override void Enter()
        {
            ((GameManagerFSM)stateMachine).gameManagerStat.globalTimer = Time.time;

            ((GameManagerFSM)stateMachine).gameManagerStat.globalDesiredTimer
                = Time.time + ((GameManagerFSM)stateMachine).gameManagerStat.characterSelectTime;
        }

        public override void Update()
        {
            ((GameManagerFSM)stateMachine).gameManagerStat.globalTimer += Time.deltaTime;

            if (((GameManagerFSM)stateMachine).gameManagerStat.globalTimer >=
                ((GameManagerFSM)stateMachine).gameManagerStat.characterSelectTime)
            {
                stateMachine.ChangeState(((GameManagerFSM)stateMachine).gameManagerStat.GameStates[GameManagerStat.GameState.GameReady]);
            }
        }

        public override void Exit()
        {
            MakingCharacter();
            MakingTeamStateUI();
        }

        void MakingCharacter()
        {
            for (int i = 0; i < ((GameManagerFSM)stateMachine).gameManagerStat.playersA.Count; i++)
            {
                GameManagerStat.PlayerData playerA = ((GameManagerFSM)stateMachine).gameManagerStat.playersA[i];

                ((GameManagerFSM)stateMachine).gameManagerStat.gameManagerPhotonView.
                    RPC("ActiveObject", playerA.player, "inGameUIObj", true);
                ((GameManagerFSM)stateMachine).gameManagerStat.gameManagerPhotonView.
                    RPC("ActiveObject", playerA.player, "characterSelectObj", false);

                string choseCharacter = playerA.character;
                if (choseCharacter == null)
                {
                    choseCharacter = playerA.character = "Observer";
                    ((GameManagerFSM)stateMachine).gameManagerStat.playersA[i] = playerA;
                }

                GameObject playerCharacter = PhotonNetwork.Instantiate("Characters/" + choseCharacter,
                        ((GameManagerFSM)stateMachine).gameManagerStat.playerSpawnA[i].position, Quaternion.identity);
                if (playerCharacter == null) continue;

                PhotonView[] views = playerCharacter.GetComponentsInChildren<PhotonView>();
                for (int j = 0; j < views.Length; j++)
                {
                    views[j].TransferOwnership(playerA.player.ActorNumber);
                }

                if (choseCharacter != "Observer")
                {
                    playerCharacter.GetComponent<PhotonView>().RPC("IsLocalPlayer", playerA.player);
                    //playerCharacter.GetComponent<Character>().SetTagServer("TeamA");
                }

                else
                {
                    playerCharacter.GetComponent<PhotonView>().RPC("SetTag", RpcTarget.All, "TeamA");
                    //gameCenter.inGameUIView.RPC("DisActiveCrosshair", player);
                    //playerCharacter.GetComponent<PhotonView>().RPC("ActiveObserver", player);
                }

                playerCharacter.GetComponent<PhotonView>().RPC("ChangeName", RpcTarget.All, playerA.name);
            }

            for (int i = 0; i < ((GameManagerFSM)stateMachine).gameManagerStat.playersB.Count; i++)
            {
                GameManagerStat.PlayerData playerB = ((GameManagerFSM)stateMachine).gameManagerStat.playersB[i];

                ((GameManagerFSM)stateMachine).gameManagerStat.gameManagerPhotonView.
                    RPC("ActiveObject", playerB.player, "inGameUIObj", true);
                ((GameManagerFSM)stateMachine).gameManagerStat.gameManagerPhotonView.
                    RPC("ActiveObject", playerB.player, "characterSelectObj", false);

                string choseCharacter = playerB.character;
                if (choseCharacter == null)
                {
                    choseCharacter = playerB.character = "Observer";
                    ((GameManagerFSM)stateMachine).gameManagerStat.playersB[i] = playerB;
                }

                GameObject playerCharacter = PhotonNetwork.Instantiate("Characters/" + choseCharacter,
                        ((GameManagerFSM)stateMachine).gameManagerStat.playerSpawnA[i].position, Quaternion.identity);
                if (playerCharacter == null) continue;

                PhotonView[] views = playerCharacter.GetComponentsInChildren<PhotonView>();
                for (int j = 0; j < views.Length; j++)
                {
                    views[j].TransferOwnership(playerB.player.ActorNumber);
                }

                if (choseCharacter != "Observer")
                {
                    playerCharacter.GetComponent<PhotonView>().RPC("IsLocalPlayer", playerB.player);
                    //playerCharacter.GetComponent<Character>().SetTagServer("TeamA");
                }

                else
                {
                    playerCharacter.GetComponent<PhotonView>().RPC("SetTag", RpcTarget.All, "TeamA");
                    //gameCenter.inGameUIView.RPC("DisActiveCrosshair", player);
                    //playerCharacter.GetComponent<PhotonView>().RPC("ActiveObserver", player);
                }

                playerCharacter.GetComponent<PhotonView>().RPC("ChangeName", RpcTarget.All, playerB.name);
            }
        }

        void MakingTeamStateUI()
        {
            for (int i = 0; i < ((GameManagerFSM)stateMachine).gameManagerStat.playersA.Count; i++)
            {
                for (int j = 0; j < ((GameManagerFSM)stateMachine).gameManagerStat.playersA.Count; j++)
                {
                    //gameCenter.inGameUIView.RPC("ShowTeamState", player, playerA.CustomProperties["Name"], "Aeterna");
                }
            }

            for (int i = 0; i < ((GameManagerFSM)stateMachine).gameManagerStat.playersB.Count; i++)
            {
                for (int j = 0; j < ((GameManagerFSM)stateMachine).gameManagerStat.playersB.Count; j++)
                {
                    //gameCenter.inGameUIView.RPC("ShowTeamState", player, playerB.CustomProperties["Name"], "Aeterna");
                }
            }      
        }
    } 
}