using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using GameCenterDataType;

namespace FSM
{
    public class LoadingScene : BaseState
    {
        public List<int> redTeamActorNums = new List<int>();
        public List<int> blueTeamActorNums = new List<int>();

        public LoadingScene(StateMachine stateMachine) :
            base("LoadingScene", stateMachine)
        {}

        public override void Enter()
        {
            SetPlayerDatas();
        }

        void SetPlayerDatas()
        {
            for(int i = 0; i < redTeamActorNums.Count; i++)
            {
                int j;
                for(j  = 0; j < PhotonNetwork.PlayerList.Length; i++)
                {
                    if(PhotonNetwork.PlayerList[i].ActorNumber == redTeamActorNums[i])
                    {
                        ((GameCenter0)stateMachine).players.Add(PhotonNetwork.PlayerList[i].NickName, 
                            InitPlayerData(PhotonNetwork.PlayerList[i], i, true));
                        break;
                    }
                }
                if (j >= PhotonNetwork.PlayerList.Length)
                    Debug.LogError("액터넘버에 해당하는 플레이어 못 찾음");
            }

            for (int i = 0; i < blueTeamActorNums.Count; i++)
            {
                int j;
                for (j = 0; j < PhotonNetwork.PlayerList.Length; i++)
                {
                    if (PhotonNetwork.PlayerList[i].ActorNumber == blueTeamActorNums[i])
                    {
                        ((GameCenter0)stateMachine).players.Add(PhotonNetwork.PlayerList[i].NickName, 
                            InitPlayerData(PhotonNetwork.PlayerList[i], i, false));
                        break;
                    }
                }
                if (j >= PhotonNetwork.PlayerList.Length)
                    Debug.LogError("액터넘버에 해당하는 플레이어 못 찾음");
            }
        }

        PlayerStat InitPlayerData(Photon.Realtime.Player player, int index, bool isRedTeam)
        {
            PlayerStat playerData = new PlayerStat();

            playerData.index = index;
            playerData.name = player.NickName;
            playerData.player = player;
            playerData.attackedData = new List<AssistData>();

            if (isRedTeam) playerData.team = "A";
            else playerData.team = "B";

            playerData.isAlive = true;
            playerData.respawnTime = 10000000.0f;

            return playerData;
        }
    }
}