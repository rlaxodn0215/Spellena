using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using GameCenterDataType;

namespace FSM
{
    public class CharacterSelect : BaseState
    {
        [HideInInspector]
        public SelectingCharacter characterSelect;
        [HideInInspector]
        public CharacterSelectTimer characterSelectTimer;

        private PhotonView inGameUIView;

        public CharacterSelect(StateMachine stateMachine) :
            base("CharacterSelect", stateMachine)
        {
            characterSelectTimer.characterSelectTime = 1f;
            inGameUIView = ((GameCenter0)stateMachine).gameCenterObjs["InGameUI"].GetComponent<PhotonView>();
            if (inGameUIView == null) Debug.LogError("no inGameUIView");
        }

        public override void Enter()
        {
            characterSelect = ((GameCenter0)stateMachine).gameCenterObjs["CharacterSelect"].GetComponent<SelectingCharacter>();
            if (characterSelect == null) Debug.LogError("no characterSelect");
            ((GameCenter0)stateMachine).globalTimer.globalDesiredTime = Time.time + characterSelectTimer.characterSelectTime;
        }

        public override void FixedUpdate()
        {
            if (((GameCenter0)stateMachine).globalTimer.globalTime >=
                ((GameCenter0)stateMachine).globalTimer.globalDesiredTime)
            {
                stateMachine.ChangeState(((GameCenter0)stateMachine).GameStates[GameState.GameReady]);
            }
        }

        public override void Exit()
        {
            MakingCharacter();
            MakingTeamStateUI();
        }

        void MakingCharacter()
        {
            Transform[] playerSpawnA = Helper.FindObject(((GameCenter0)stateMachine).gameCenterObjs["playerSpawnPoints"], "TeamA").GetComponentsInChildren<Transform>(true);
            Transform[] playerSpawnB = Helper.FindObject(((GameCenter0)stateMachine).gameCenterObjs["playerSpawnPoints"], "TeamB").GetComponentsInChildren<Transform>(true);

            for (int i = 0; i < ((GameCenter0)stateMachine).playerList.playersA.Count; i++)
            {
                PlayerStat playerA = ((GameCenter0)stateMachine).playerList.playersA[i];

                ((GameCenter0)stateMachine).gameManagerView.RPC("ActiveObject", playerA.player, "GlobalUI", true);
                ((GameCenter0)stateMachine).gameManagerView.RPC("ActiveObject", playerA.player, "CharacterSelect", false);

                string choseCharacter = playerA.character;
                if (choseCharacter == null)
                {
                    choseCharacter = playerA.character = "Observer";
                    ((GameCenter0)stateMachine).playerList.playersA[i] = playerA;
                }

                GameObject playerCharacter = PhotonNetwork.Instantiate("Characters/" + choseCharacter,
                        playerSpawnA[i].position, Quaternion.identity);
                if (playerCharacter == null) continue;

                PhotonView[] views = playerCharacter.GetComponentsInChildren<PhotonView>();
                for (int j = 0; j < views.Length; j++)
                {
                    views[j].TransferOwnership(playerA.player.ActorNumber);
                }

                if (choseCharacter != "Observer")
                {
                    playerCharacter.GetComponent<PhotonView>().RPC("SetLocalPlayer", playerA.player);
                }

                else
                {
                    playerCharacter.GetComponent<PhotonView>().RPC("SetTag", RpcTarget.All, "TeamA");
                    playerCharacter.GetComponent<PhotonView>().RPC("ActiveObserver", playerA.player);
                    inGameUIView.RPC("DisActiveCrosshair", playerA.player);
                }

                playerCharacter.GetComponent<PhotonView>().RPC("ChangeName", RpcTarget.All, playerA.name);
            }

            for (int i = 0; i < ((GameCenter0)stateMachine).playerList.playersB.Count; i++)
            {
                PlayerStat playerB = ((GameCenter0)stateMachine).playerList.playersB[i];

                ((GameCenter0)stateMachine).gameManagerView.RPC("ActiveObject", playerB.player, "inGameUIObj", true);
                ((GameCenter0)stateMachine).gameManagerView.RPC("ActiveObject", playerB.player, "characterSelectObj", false);

                string choseCharacter = playerB.character;
                if (choseCharacter == null)
                {
                    choseCharacter = playerB.character = "Observer";
                    ((GameCenter0)stateMachine).playerList.playersB[i] = playerB;
                }

                GameObject playerCharacter = PhotonNetwork.Instantiate("Characters/" + choseCharacter,
                        playerSpawnB[i].position, Quaternion.identity);
                if (playerCharacter == null) continue;

                PhotonView[] views = playerCharacter.GetComponentsInChildren<PhotonView>();
                for (int j = 0; j < views.Length; j++)
                {
                    views[j].TransferOwnership(playerB.player.ActorNumber);
                }

                if (choseCharacter != "Observer")
                {
                    playerCharacter.GetComponent<PhotonView>().RPC("SetLocalPlayer", playerB.player);
                }

                else
                {
                    playerCharacter.GetComponent<PhotonView>().RPC("SetTag", RpcTarget.All, "TeamB");
                    playerCharacter.GetComponent<PhotonView>().RPC("ActiveObserver", playerB.player);
                    inGameUIView.RPC("DisActiveCrosshair", playerB.player);
                }

                playerCharacter.GetComponent<PhotonView>().RPC("ChangeName", RpcTarget.All, playerB.name);
            }
        }

        void MakingTeamStateUI()
        {
            for (int i = 0; i < ((GameCenter0)stateMachine).playerList.playersA.Count; i++)
            {
                for (int j = 0; j < ((GameCenter0)stateMachine).playerList.playersA.Count; j++)
                {
                    inGameUIView.RPC("ShowTeamState", ((GameCenter0)stateMachine).playerList.playersA[j].player,
                        ((GameCenter0)stateMachine).playerList.playersA[j].name);
                }
            }

            for (int i = 0; i < ((GameCenter0)stateMachine).playerList.playersB.Count; i++)
            {
                for (int j = 0; j < ((GameCenter0)stateMachine).playerList.playersB.Count; j++)
                {
                    inGameUIView.RPC("ShowTeamState", ((GameCenter0)stateMachine).playerList.playersB[j].player,
                        ((GameCenter0)stateMachine).playerList.playersB[j].name);
                }
            }      
        }
    } 
}