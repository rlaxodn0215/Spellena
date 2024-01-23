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
        private RoundEndStandardData roundEndStandardData;

        private PhotonView inGameUIView;
        private PhotonView deathUIView;
        private PhotonView bgmManagerView;

        public RoundEnd(StateMachine stateMachine) :
             base("RoundEnd", stateMachine)
        {
            roundEndStandardData.roundEndNumber = 2;
            roundEndStandardData.roundEndResultTime = 6;

            inGameUIView =
                ((DuringRound)(((GameCenter0)stateMachine).GameStates[GameState.DuringRound])).inGameUIView;
            deathUIView =
                ((DuringRound)(((GameCenter0)stateMachine).GameStates[GameState.DuringRound])).deathUIView;
            bgmManagerView =
                ((GameCenter0)stateMachine).bgmManagerView;
        }

        public override void Enter()
        {
            ((GameCenter0)stateMachine).globalTimer.globalDesiredTime =
                Time.time + roundEndStandardData.roundEndResultTime;

            duringRoundData = 
                ((DuringRound)(((GameCenter0)stateMachine).GameStates[GameState.DuringRound])).duringRoundData;

            ShowRoundResult();
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

        void ShowRoundResult()
        {
            int countA = ((GameCenter0)stateMachine).roundData.roundCount_A;
            int countB = ((GameCenter0)stateMachine).roundData.roundCount_B;

            if (duringRoundData.occupyingA.rate >= 100)
            {
                inGameUIView.RPC("ShowRoundPoint", RpcTarget.All, "A", countA);

                Dictionary<string, PlayerStat>.Enumerator iterA 
                    = ((GameCenter0)stateMachine).playerList.playersA.GetEnumerator();

                while(iterA.MoveNext())
                {
                    KeyValuePair<string, PlayerStat> temp = iterA.Current;
                    PlayerStat playerData = temp.Value;

                    if (countA < roundEndStandardData.roundEndNumber)
                        inGameUIView.RPC("ShowRoundWin", playerData.player, countA + countB);
                    else
                        inGameUIView.RPC("ActiveInGameUIObj", playerData.player, "victory", true);

                    bgmManagerView.RPC("PlayAudio", playerData.player, "RoundWin", 1.0f, false, true, "BGM");
                }

                Dictionary<string, PlayerStat>.Enumerator iterB
                     = ((GameCenter0)stateMachine).playerList.playersB.GetEnumerator();

                while (iterB.MoveNext())
                {
                    KeyValuePair<string, PlayerStat> temp = iterB.Current;
                    PlayerStat playerData = temp.Value;

                    if (countA < roundEndStandardData.roundEndNumber)
                        inGameUIView.RPC("ShowRoundLoose", playerData.player, countA + countB);
                    else
                        inGameUIView.RPC("ActiveInGameUIObj", playerData.player, "defeat", true);

                    bgmManagerView.RPC("PlayAudio", playerData.player, "RoundLoose", 1.0f, false, true, "BGM");
                }

            }

            else
            {
                inGameUIView.RPC("ShowRoundPoint", RpcTarget.All, "B", countB);

                Dictionary<string, PlayerStat>.Enumerator iterA
                     = ((GameCenter0)stateMachine).playerList.playersA.GetEnumerator();

                while (iterA.MoveNext())
                {
                    KeyValuePair<string, PlayerStat> temp = iterA.Current;
                    PlayerStat playerData = temp.Value;

                    if (countB < roundEndStandardData.roundEndNumber)
                        inGameUIView.RPC("ShowRoundLoose", playerData.player, countA + countB);
                    else
                        inGameUIView.RPC("ActiveInGameUIObj", playerData.player, "defeat", true);

                    bgmManagerView.RPC("PlayAudio", playerData.player, "RoundLoose", 1.0f, false, true, "BGM");
                }

                Dictionary<string, PlayerStat>.Enumerator iterB
                     = ((GameCenter0)stateMachine).playerList.playersB.GetEnumerator();

                while (iterB.MoveNext())
                {
                    KeyValuePair<string, PlayerStat> temp = iterB.Current;
                    PlayerStat playerData = temp.Value;

                    if (countA < roundEndStandardData.roundEndNumber)
                        inGameUIView.RPC("ShowRoundWin", playerData.player, countA + countB);
                    else
                        inGameUIView.RPC("ActiveInGameUIObj", playerData.player, "victory", true);

                    bgmManagerView.RPC("PlayAudio", playerData.player, "RoundWin", 1.0f, false, true, "BGM");
                }
            }
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
            Dictionary<string, PlayerStat>.Enumerator iterA
                     = ((GameCenter0)stateMachine).playerList.playersA.GetEnumerator();

            while (iterA.MoveNext())
            {
                KeyValuePair<string, PlayerStat> temp = iterA.Current;
                PlayerStat playerData = temp.Value;

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
                inGameUIView.RPC("InitTeamLifeDead", playerData.player);

                view.GetComponent<BuffDebuffChecker>().ritualStacks = 0;

            }

            Dictionary<string, PlayerStat>.Enumerator iterB
                    = ((GameCenter0)stateMachine).playerList.playersB.GetEnumerator();

            while (iterB.MoveNext())
            {
                KeyValuePair<string, PlayerStat> temp = iterB.Current;
                PlayerStat playerData = temp.Value;

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
                inGameUIView.RPC("InitTeamLifeDead", playerData.player);

                view.GetComponent<BuffDebuffChecker>().ritualStacks = 0;
            }

        }
    }
}