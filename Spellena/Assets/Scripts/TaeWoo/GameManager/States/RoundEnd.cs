using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using GameCenterDataType;

namespace FSM
{
    public class RoundEnd : BaseState
    {
        private DuringRoundData duringRoundData;
        private PhotonView inGameUIView;
        private PhotonView deathUIView;

        private RoundEndStandardData roundEndStandardData;

        public RoundEnd(StateMachine stateMachine) :
             base("RoundEnd", stateMachine)
        {
            roundEndStandardData.roundEndNumber = 2;
            roundEndStandardData.roundEndResultTime = 6;

            inGameUIView =
                ((DuringRound)(((GameCenter0)stateMachine).GameStates[GameState.DuringRound])).inGameUIView;
            deathUIView =
                ((DuringRound)(((GameCenter0)stateMachine).GameStates[GameState.DuringRound])).deathUIView;
        }

        public override void Enter()
        {
            ((GameCenter0)stateMachine).globalTimer.globalDesiredTime =
                Time.time + roundEndStandardData.roundEndResultTime;

            duringRoundData = 
                ((DuringRound)(((GameCenter0)stateMachine).GameStates[GameState.DuringRound])).duringRoundData;
        }

        public override void Update()
        {
            if(Time.time >= ((GameCenter0)stateMachine).globalTimer.globalDesiredTime)
            {
                if (((GameCenter0)stateMachine).roundData.roundCount_A >= 2 ||
                ((GameCenter0)stateMachine).roundData.roundCount_B >= 2)
                {
                    stateMachine.ChangeState(((GameCenter0)stateMachine).GameStates[GameState.GameResult]);
                }

                else
                {
                    stateMachine.ChangeState(((GameCenter0)stateMachine).GameStates[GameState.GameReady]);
                }
            }

        }

        public override void Exit()
        {
            ResetRound();
        }

        void ResetRound()
        {
            duringRoundData.teamAOccupying = 0;
            duringRoundData.teamBOccupying = 0;
            duringRoundData.occupyingReturnTimer = 0.0f;
            duringRoundData.roundEndTimer = 0.0f;
            duringRoundData.currentOccupationTeam = "";
            duringRoundData.occupyingA.rate = 0.0f;
            duringRoundData.occupyingB.rate = 0.0f;
            duringRoundData.occupyingTeam.name = "";
            duringRoundData.occupyingTeam.rate = 0.0f;

            ((DuringRound)(((GameCenter0)stateMachine).
                GameStates[GameState.DuringRound])).duringRoundData = duringRoundData;

            inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "victory", false);
            inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "defeat", false);
            inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "roundWin", false);
            inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "roundLoose", false);

            inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "captured_Red", false);
            inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "captured_Blue", false);
            inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "extraObj", false);
            inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "redExtraUI", true);
            inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "redExtraObj", false);
            inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "blueExtraUI", true);
            inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "blueExtraObj", false);

            inGameUIView.RPC("DisableAllKillLog", RpcTarget.All);


            // 플레이어 초기화
            for(int i = 0; i < ((GameCenter0)stateMachine).playerList.playersA.Count; i++)
            {
                PlayerStat playerData = ((GameCenter0)stateMachine).playerList.playersA[i];
                playerData.isAlive = true;
                playerData.respawnTime = 100000000.0f;
                playerData.ultimateCount = 0;

                if (playerData.character == "Observer") continue;
                PhotonView view = PhotonView.Find(playerData.characterViewID);
                if (view == null) continue;

                view.RPC("AddUltimatePoint", RpcTarget.AllBuffered, 0);
                view.RPC("PlayerReBornForAll", RpcTarget.All, playerData.spawnPoint);
                view.RPC("PlayerReBornPersonal", playerData.player);

                deathUIView.RPC("DisableDeathCamUI", playerData.player);

                for (int j = 0; j < ((GameCenter0)stateMachine).playerList.playersA.Count; j++)
                {
                    PlayerStat playerData1 = ((GameCenter0)stateMachine).playerList.playersA[j];
                    inGameUIView.RPC("ShowTeamLifeDead", playerData1.player, playerData1.name, false);
                }

                view.GetComponent<BuffDebuffChecker>().ritualStacks = 0;
            }

            for (int i = 0; i < ((GameCenter0)stateMachine).playerList.playersB.Count; i++)
            {
                PlayerStat playerData = ((GameCenter0)stateMachine).playerList.playersB[i];
                playerData.isAlive = true;
                playerData.respawnTime = 100000000.0f;
                playerData.ultimateCount = 0;

                if (playerData.character == "Observer") continue;
                PhotonView view = PhotonView.Find(playerData.characterViewID);
                if (view == null) continue;

                view.RPC("AddUltimatePoint", RpcTarget.AllBuffered, 0);
                view.RPC("PlayerReBornForAll", RpcTarget.All, playerData.spawnPoint);
                view.RPC("PlayerReBornPersonal", playerData.player);

                deathUIView.RPC("DisableDeathCamUI", playerData.player);

                for (int j = 0; j < ((GameCenter0)stateMachine).playerList.playersB.Count; j++)
                {
                    PlayerStat playerData1 = ((GameCenter0)stateMachine).playerList.playersB[j];
                    inGameUIView.RPC("ShowTeamLifeDead", playerData1.player, playerData1.name, false);
                }

                view.GetComponent<BuffDebuffChecker>().ritualStacks = 0;
            }
        }
    }
}