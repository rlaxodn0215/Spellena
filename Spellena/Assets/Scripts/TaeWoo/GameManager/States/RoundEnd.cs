using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace FSM
{
    public class RoundEnd : BaseState
    {
        public RoundEnd(StateMachine stateMachine) :
             base("RoundEnd", stateMachine)
        {}

        public override void Enter()
        {
            ((GameManagerFSM)stateMachine).gameManagerStat.globalDesiredTimer =
                ((GameManagerFSM)stateMachine).gameManagerStat.globalTimer + 
                ((GameManagerFSM)stateMachine).gameManagerStat.roundEndResultTime;
        }

        public override void Update()
        {
            ((GameManagerFSM)stateMachine).gameManagerStat.globalTimer += Time.deltaTime;
        }

        public override void Exit()
        {
            if (((GameManagerFSM)stateMachine).gameManagerStat.roundCount_A >= 2 ||
                ((GameManagerFSM)stateMachine).gameManagerStat.roundCount_B >= 2)
            {
                stateMachine.ChangeState(((GameManagerFSM)stateMachine).gameManagerStat.
                    GameStates[GameManagerStat.GameState.GameResult]);
                ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "victory", false);
                ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "defeat", false);
            }

            else
            {
                stateMachine.ChangeState(((GameManagerFSM)stateMachine).gameManagerStat.
                    GameStates[GameManagerStat.GameState.GameReady]);
                ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "roundWin", false);
                ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "roundLoose", false);
                ResetRound();
                Debug.Log("ResetRound");
            }
        }

        void ResetRound()
        {
            ((GameManagerFSM)stateMachine).gameManagerStat.teamAOccupying = 0;
            ((GameManagerFSM)stateMachine).gameManagerStat.teamBOccupying = 0;
            ((GameManagerFSM)stateMachine).gameManagerStat.occupyingReturnTimer = 0.0f;
            ((GameManagerFSM)stateMachine).gameManagerStat.roundEndTimer = 0.0f;
            ((GameManagerFSM)stateMachine).gameManagerStat.currentOccupationTeam = "";
            ((GameManagerFSM)stateMachine).gameManagerStat.occupyingA.rate = 0.0f;
            ((GameManagerFSM)stateMachine).gameManagerStat.occupyingB.rate = 0.0f;
            ((GameManagerFSM)stateMachine).gameManagerStat.occupyingTeam.name = "";
            ((GameManagerFSM)stateMachine).gameManagerStat.occupyingTeam.rate = 0.0f;

            ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "captured_Red", false);
            ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "captured_Blue", false);
            ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "extraObj", false);
            ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "redExtraUI", true);
            ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "redExtraObj", false);
            ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "blueExtraUI", true);
            ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "blueExtraObj", false);

            ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("DisableAllKillLog", RpcTarget.All);


            // 플레이어 초기화
            for(int i = 0; i < ((GameManagerFSM)stateMachine).gameManagerStat.playersA.Count; i++)
            {
                GameManagerStat.PlayerData playerData = ((GameManagerFSM)stateMachine).gameManagerStat.playersA[i];
                playerData.isAlive = true;
                playerData.respawnTime = 100000000.0f;
                playerData.ultimateCount = 0;

                if (playerData.character == "Observer") continue;
                PhotonView view = PhotonView.Find(playerData.characterViewID);
                if (view == null) continue;

                view.RPC("AddUltimatePoint", RpcTarget.AllBuffered, 0);
                view.RPC("PlayerReBornForAll", RpcTarget.All, playerData.spawnPoint);
                view.RPC("PlayerReBornPersonal", playerData.player);

                ((GameManagerFSM)stateMachine).gameManagerStat.deathUIView.RPC("DisableDeathCamUI", playerData.player);

                for (int j = 0; j < ((GameManagerFSM)stateMachine).gameManagerStat.playersA.Count; j++)
                {
                    GameManagerStat.PlayerData playerData1 = ((GameManagerFSM)stateMachine).gameManagerStat.playersA[j];
                    ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ShowTeamLifeDead", playerData1.player, playerData1.name, false);
                }

                view.GetComponent<BuffDebuffChecker>().ritualStacks = 0;
            }

            for (int i = 0; i < ((GameManagerFSM)stateMachine).gameManagerStat.playersB.Count; i++)
            {
                GameManagerStat.PlayerData playerData = ((GameManagerFSM)stateMachine).gameManagerStat.playersB[i];
                playerData.isAlive = true;
                playerData.respawnTime = 100000000.0f;
                playerData.ultimateCount = 0;

                if (playerData.character == "Observer") continue;
                PhotonView view = PhotonView.Find(playerData.characterViewID);
                if (view == null) continue;

                view.RPC("AddUltimatePoint", RpcTarget.AllBuffered, 0);
                view.RPC("PlayerReBornForAll", RpcTarget.All, playerData.spawnPoint);
                view.RPC("PlayerReBornPersonal", playerData.player);

                ((GameManagerFSM)stateMachine).gameManagerStat.deathUIView.RPC("DisableDeathCamUI", playerData.player);

                for (int j = 0; j < ((GameManagerFSM)stateMachine).gameManagerStat.playersB.Count; j++)
                {
                    GameManagerStat.PlayerData playerData1 = ((GameManagerFSM)stateMachine).gameManagerStat.playersB[j];
                    ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ShowTeamLifeDead", playerData1.player, playerData1.name, false);
                }

                view.GetComponent<BuffDebuffChecker>().ritualStacks = 0;
            }
        }
    }
}