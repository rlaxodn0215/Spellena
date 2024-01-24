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
        private PhotonView characterSelectView;

        public CharacterSelect(StateMachine stateMachine) :
            base("CharacterSelect", stateMachine)
        {
            characterSelectTimer.characterSelectTime = 1f;
            inGameUIView = ((GameCenter0)stateMachine).gameCenterObjs["InGameUI"].GetComponent<PhotonView>();
            if (inGameUIView == null) Debug.LogError("no inGameUIView");
            characterSelectView = ((GameCenter0)stateMachine).gameCenterObjs["CharacterSelect"].GetComponent<PhotonView>();
            if (characterSelectView == null) Debug.LogError("no characterSelectView");
        }

        public override void Enter()
        {
            characterSelect = ((GameCenter0)stateMachine).gameCenterObjs["CharacterSelect"].GetComponent<SelectingCharacter>();
            if (characterSelect == null) Debug.LogError("no characterSelect");
            ((GameCenter0)stateMachine).globalTimer.globalDesiredTime = 
                ((GameCenter0)stateMachine).globalTimer.globalTime + characterSelectTimer.characterSelectTime;
            ActiveInGameObjs();
        }

        public override void FixedUpdate()
        {
            characterSelectView.RPC("ReceiveTimerCount", RpcTarget.AllBuffered, 
                ((GameCenter0)stateMachine).globalTimer.globalDesiredTime - ((GameCenter0)stateMachine).globalTimer.globalTime);

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

        void ActiveInGameObjs()
        {
            Dictionary<string, GameObject>.Enumerator iter
                     = ((GameCenter0)stateMachine).gameCenterObjs.GetEnumerator();

            while (iter.MoveNext())
            {
                KeyValuePair<string, GameObject> temp = iter.Current;
                GameObject obj = temp.Value;
                obj.SetActive(true);
            }

            ((GameCenter0)stateMachine).gameCenterObjs["InGameUI"].SetActive(false);
        }

        void MakingCharacter()
        {
            Transform[] playerSpawnA = Helper.FindObject(((GameCenter0)stateMachine).gameCenterObjs["PlayerSpawnPoints"], "TeamA").GetComponentsInChildren<Transform>(true);
            Transform[] playerSpawnB = Helper.FindObject(((GameCenter0)stateMachine).gameCenterObjs["PlayerSpawnPoints"], "TeamB").GetComponentsInChildren<Transform>(true);
            int indexA = 1, indexB = 1;

            Dictionary<string, PlayerStat>.Enumerator iter
                     = ((GameCenter0)stateMachine).players.GetEnumerator();

            Debug.Log(((GameCenter0)stateMachine).players.Count);

            while (iter.MoveNext())
            {
                KeyValuePair<string, PlayerStat> temp = iter.Current;
                PlayerStat player = temp.Value;
                //Debug.Log("<color=red>" + player.name + "</color>");

                ((GameCenter0)stateMachine).gameManagerView.RPC("ActiveObject", player.player, "InGameUI", true);
                ((GameCenter0)stateMachine).gameManagerView.RPC("ActiveObject", player.player, "CharacterSelect", false);

                string choseCharacter = player.character;
                //if (choseCharacter == null)
                //{
                //    choseCharacter = player.character = "Observer";
                //    ((GameCenter0)stateMachine).players[player.name] = player;
                //}

                if (choseCharacter == null)
                {
                    choseCharacter = player.character = "Aloy";
                    // 이름 할당이 안됨...
                    //((GameCenter0)stateMachine).players[player.name] = player;
                }

                Vector3 pos;

                if (player.team == ((GameCenter0)stateMachine).roundData.teamA)
                {
                    pos = playerSpawnA[indexA++].position;
                }

                else
                {
                    pos = playerSpawnB[indexB++].position;
                }

                GameObject playerCharacter = PhotonNetwork.Instantiate("Characters/" + choseCharacter, pos, Quaternion.identity);
                if (playerCharacter == null) continue;

                PhotonView[] views = playerCharacter.GetComponentsInChildren<PhotonView>();
                for (int i = 0; i < views.Length; i++)
                {
                    views[i].TransferOwnership(player.player.ActorNumber);
                }

                if (choseCharacter == "Aloy")
                {
                    playerCharacter.GetComponent<PhotonView>().RPC("SetLocalAI", player.player);
                    GameObject aloyPoolManager = PhotonNetwork.Instantiate("PoolManagers/AloyPoolManager", Vector3.zero, Quaternion.identity);
                    aloyPoolManager.GetComponent<PhotonView>().TransferOwnership(player.player.ActorNumber);
                    inGameUIView.RPC("DisActiveCrosshair", player.player);
                }

                else if (choseCharacter != "Observer") 
                {
                    playerCharacter.GetComponent<PhotonView>().RPC("SetTag", RpcTarget.All, "TeamA");
                    playerCharacter.GetComponent<PhotonView>().RPC("ActiveObserver", player.player);
                    inGameUIView.RPC("DisActiveCrosshair", player.player);
                }

                else
                {
                    playerCharacter.GetComponent<PhotonView>().RPC("SetLocalPlayer", player.player);
                }

                playerCharacter.GetComponent<PhotonView>().RPC("ChangeName", RpcTarget.All, player.name);
            }
        }

        void MakingTeamStateUI()
        {
            Dictionary<string, PlayerStat>.Enumerator iter
                       = ((GameCenter0)stateMachine).players.GetEnumerator();

            while (iter.MoveNext())
            {
                KeyValuePair<string, PlayerStat> temp = iter.Current;
                PlayerStat player = temp.Value;
                inGameUIView.RPC("ShowTeamState", RpcTarget.All, player.name);
            }  
        }
    } 
}