using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace FSM
{
    public class DuringRound : BaseState
    {
        private Queue<GameManagerStat.PlayerData> playerRespawnQue
            = new Queue<GameManagerStat.PlayerData>();

        bool OccupyBarCountOnce = true;
        bool isFighting = false;

        public DuringRound(StateMachine stateMachine) :
            base("DuringRound", stateMachine)
        {

        }

        public override void Enter()
        {

        }

        public override void Update()
        {
            ((GameManagerFSM)stateMachine).gameManagerStat.globalTimer += Time.deltaTime;

            OccupyBarCounting();
            OccupyAreaCounting();
            CheckingPlayerReSpawn();
            CheckingRoundEnd();
        }

        public override void Exit()
        {

        }

        void OccupyBarCounting()
        {
            //지역이 점령되어있으면 점령한 팀의 점령비율이 높아진다.
            if (((GameManagerFSM)stateMachine).gameManagerStat.currentOccupationTeam 
                == ((GameManagerFSM)stateMachine).gameManagerStat.teamA)
            {
                if (((GameManagerFSM)stateMachine).gameManagerStat.teamBOccupying > 0)
                {
                    if (!isFighting && ((GameManagerFSM)stateMachine).gameManagerStat.teamAOccupying > 0)
                    {
                        ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "fighting", true);
                        Debug.Log("<color=blue>" + "Active fighting" + "</color>");
                        isFighting = !isFighting;
                    }
                }
                else
                {

                    ((GameManagerFSM)stateMachine).gameManagerStat.occupyingA.rate += Time.deltaTime * ((GameManagerFSM)stateMachine).gameManagerStat.occupyingRate;//약 1.8초당 1씩 오름
                    if (isFighting)
                    {
                        ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "fighting", false);
                        Debug.Log("<color=blue>" + "DisActive fighting" + "</color>");
                        isFighting = !isFighting;
                    }
                }

                if (OccupyBarCountOnce)
                {
                    ((GameManagerFSM)stateMachine).gameManagerStat.bgmManagerView.RPC("PlayAudio", RpcTarget.All, "Occupying", 0.7f, false, true, "BGM");
                    OccupyBarCountOnce = false;
                }

                if (((GameManagerFSM)stateMachine).gameManagerStat.occupyingA.rate >= ((GameManagerFSM)stateMachine).gameManagerStat.occupyingComplete)
                    ((GameManagerFSM)stateMachine).gameManagerStat.occupyingA.rate = ((GameManagerFSM)stateMachine).gameManagerStat.occupyingComplete;
            }

            else if (((GameManagerFSM)stateMachine).gameManagerStat.currentOccupationTeam 
                == ((GameManagerFSM)stateMachine).gameManagerStat.teamB)
            {

                if (((GameManagerFSM)stateMachine).gameManagerStat.teamAOccupying > 0)
                {
                    if (!isFighting && ((GameManagerFSM)stateMachine).gameManagerStat.teamBOccupying > 0)
                    {
                        ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "fighting", true);
                        Debug.Log("<color=blue>" + "Active fighting" + "</color>");
                        isFighting = !isFighting;
                    }
                }
                else
                {
                    ((GameManagerFSM)stateMachine).gameManagerStat.occupyingB.rate += Time.deltaTime * ((GameManagerFSM)stateMachine).gameManagerStat.occupyingRate;//약 1.8초당 1씩 오름

                    if (isFighting)
                    {
                        ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "fighting", false);
                        Debug.Log("<color=blue>" + "DisActive fighting" + "</color>");
                        isFighting = !isFighting;
                    }
                }

                if (OccupyBarCountOnce)
                {
                    ((GameManagerFSM)stateMachine).gameManagerStat.bgmManagerView.RPC("PlayAudio", RpcTarget.All, "Occupying", 0.7f, false, true, "BGM");
                    OccupyBarCountOnce = false;
                }

                if (((GameManagerFSM)stateMachine).gameManagerStat.occupyingB.rate >= ((GameManagerFSM)stateMachine).gameManagerStat.occupyingComplete)
                    ((GameManagerFSM)stateMachine).gameManagerStat.occupyingB.rate = ((GameManagerFSM)stateMachine).gameManagerStat.occupyingComplete;
            }
        }

        void OccupyAreaCounting()//점령 지역에 플레이어가 몇 명 점령하고 있는지 확인
        {
            ((GameManagerFSM)stateMachine).gameManagerStat.teamAOccupying = 0;
            ((GameManagerFSM)stateMachine).gameManagerStat.teamBOccupying = 0;

            GameObject temp;

            //Debug.Log("<color=green>" + "TeamAOccupying : " + gameCenter.teamAOccupying + "TeamBOccupying : " + gameCenter.teamBOccupying + "</color>");

            for (int i = 0; i < ((GameManagerFSM)stateMachine).gameManagerStat.playersA.Count; i++)
            {
                temp = Helper.FindObjectWithViewID(((GameManagerFSM)stateMachine).gameManagerStat.playersA[i].characterViewID);
                if (temp == null) continue;

                //if (temp.GetComponent<Character>() == null) continue;
                //if (temp.GetComponent<Character>().isOccupying == true)
                {
                    ((GameManagerFSM)stateMachine).gameManagerStat.teamAOccupying++;
                }
            }

            for (int i = 0; i < ((GameManagerFSM)stateMachine).gameManagerStat.playersB.Count; i++)
            {
                temp = Helper.FindObjectWithViewID(((GameManagerFSM)stateMachine).gameManagerStat.playersB[i].characterViewID);
                if (temp == null) continue;

                //if (temp.GetComponent<Character>() == null) continue;
                //if (temp.GetComponent<Character>().isOccupying == true)
                {
                    ((GameManagerFSM)stateMachine).gameManagerStat.teamBOccupying++;
                }
            }

            //Debug.Log("<color=green>" + "TeamAOccupying : " + gameCenter.teamAOccupying + "TeamBOccupying : " + gameCenter.teamBOccupying + "</color>");

            if (((GameManagerFSM)stateMachine).gameManagerStat.teamAOccupying > 0 &&
                ((GameManagerFSM)stateMachine).gameManagerStat.teamBOccupying > 0)
            {
                //서로 교전 중이라는 것을 알림
                ((GameManagerFSM)stateMachine).gameManagerStat.occupyingReturnTimer = 0f;
                if (!isFighting)
                {
                    ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "fighting", true);
                    Debug.Log("<color=blue>" + "Active fighting" + "</color>");
                    ((GameManagerFSM)stateMachine).gameManagerStat.roundEndTimer 
                        = ((GameManagerFSM)stateMachine).gameManagerStat.roundEndTime;
                    isFighting = !isFighting;
                }
            }

            else if (((GameManagerFSM)stateMachine).gameManagerStat.teamAOccupying > 0)//A팀 점령
            {
                ChangeOccupyingRate(((GameManagerFSM)stateMachine).gameManagerStat.teamAOccupying,
                    ((GameManagerFSM)stateMachine).gameManagerStat.teamA);
                ((GameManagerFSM)stateMachine).gameManagerStat.occupyingReturnTimer = 0f;
                if (isFighting)
                {
                    ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "fighting", false);
                    Debug.Log("<color=blue>" + "DisActive fighting" + "</color>");
                    isFighting = !isFighting;
                }
            }
            else if (((GameManagerFSM)stateMachine).gameManagerStat.teamBOccupying > 0)//B팀 점령
            {
                ChangeOccupyingRate(((GameManagerFSM)stateMachine).gameManagerStat.teamBOccupying,
                    ((GameManagerFSM)stateMachine).gameManagerStat.teamB);
                ((GameManagerFSM)stateMachine).gameManagerStat.occupyingReturnTimer = 0f;
                if (isFighting)
                {
                    ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "fighting", false);
                    Debug.Log("<color=blue>" + "DisActive fighting" + "</color>");
                    isFighting = !isFighting;
                }
            }
            else
            {
                ((GameManagerFSM)stateMachine).gameManagerStat.occupyingReturnTimer += Time.deltaTime;
                if (isFighting)
                {
                    ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "fighting", false);
                    Debug.Log("<color=blue>" + "DisActive fighting" + "</color>");
                    isFighting = !isFighting;
                }
            }

            if (((GameManagerFSM)stateMachine).gameManagerStat.occupyingReturnTimer >=
                ((GameManagerFSM)stateMachine).gameManagerStat.occupyingReturnTime)
            {
                if (((GameManagerFSM)stateMachine).gameManagerStat.occupyingTeam.rate > 0f)
                {
                    ((GameManagerFSM)stateMachine).gameManagerStat.occupyingTeam.rate -= Time.deltaTime;
                    ((GameManagerFSM)stateMachine).gameManagerStat.bgmManagerView.RPC("StopAudio", RpcTarget.All, "Occupying");

                    if (((GameManagerFSM)stateMachine).gameManagerStat.occupyingTeam.rate < 0f)
                    {
                        ((GameManagerFSM)stateMachine).gameManagerStat.occupyingTeam.rate = 0f;
                        ((GameManagerFSM)stateMachine).gameManagerStat.occupyingTeam.name = "";
                    }
                }
            }

        }

        void ChangeOccupyingRate(int num, string name) //점령 게이지 변화
        {
            if (((GameManagerFSM)stateMachine).gameManagerStat.occupyingTeam.name == name)
            {
                if (((GameManagerFSM)stateMachine).gameManagerStat.currentOccupationTeam == name) return;

                if (((GameManagerFSM)stateMachine).gameManagerStat.occupyingTeam.rate >= 100)
                {
                    ((GameManagerFSM)stateMachine).gameManagerStat.currentOccupationTeam = name;
                    ((GameManagerFSM)stateMachine).gameManagerStat.occupyingTeam.name = "";
                    ((GameManagerFSM)stateMachine).gameManagerStat.occupyingTeam.rate = 0f;

                    ((GameManagerFSM)stateMachine).gameManagerStat.bgmManagerView.RPC("PlayAudio", RpcTarget.All, "Occupation", 1.0f, false, true, "BGM");

                    if (((GameManagerFSM)stateMachine).gameManagerStat.currentOccupationTeam == "A")
                    {
                        ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "captured_Red", true);
                        ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "captured_Blue", false);
                        ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "extraObj", false);
                        ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "blueExtraObj", false);
                        ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "blueExtraUI", true);

                        //((GameManagerFSM)stateMachine).gameManagerStat.angleStatue.GetComponent<PhotonView>().RPC("ChangeTeam", RpcTarget.All, "A");
                    }

                    else if (((GameManagerFSM)stateMachine).gameManagerStat.currentOccupationTeam == "B")
                    {
                        ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "captured_Red", false);
                        ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "captured_Blue", true);
                        ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "extraObj", false);
                        ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "redExtraObj", false);
                        ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "redExtraUI", true);

                        //((GameManagerFSM)stateMachine).gameManagerStat.angleStatue.GetComponent<PhotonView>().RPC("ChangeTeam", RpcTarget.All, "B");

                    }
                }

                else
                {
                    ((GameManagerFSM)stateMachine).gameManagerStat.occupyingTeam.rate += ((GameManagerFSM)stateMachine).gameManagerStat.occupyingGaugeRate * Time.deltaTime;
                    ((GameManagerFSM)stateMachine).gameManagerStat.bgmManagerView.RPC("PlayAudio", RpcTarget.All, "Occupying", 1.0f, true, false, "BGM");
                }
            }
            else if (((GameManagerFSM)stateMachine).gameManagerStat.occupyingTeam.name == "")
            {
                if (((GameManagerFSM)stateMachine).gameManagerStat.currentOccupationTeam == name)
                    return;
                ((GameManagerFSM)stateMachine).gameManagerStat.occupyingTeam.name = name;
                ((GameManagerFSM)stateMachine).gameManagerStat.occupyingTeam.rate += ((GameManagerFSM)stateMachine).gameManagerStat.occupyingGaugeRate * Time.deltaTime;
            }

            else
            {
                ((GameManagerFSM)stateMachine).gameManagerStat.occupyingTeam.rate -= ((GameManagerFSM)stateMachine).gameManagerStat.occupyingGaugeRate * Time.deltaTime;
                ((GameManagerFSM)stateMachine).gameManagerStat.bgmManagerView.RPC("StopAudio", RpcTarget.All, "Occupying");
                if (((GameManagerFSM)stateMachine).gameManagerStat.occupyingTeam.rate < 0)
                {
                    ((GameManagerFSM)stateMachine).gameManagerStat.occupyingTeam.name = "";
                    ((GameManagerFSM)stateMachine).gameManagerStat.occupyingTeam.rate = 0;
                }
            }
        }

        void CheckingPlayerReSpawn()
        {
            if (playerRespawnQue.Count == 0) return;
            GameManagerStat.PlayerData playerData = playerRespawnQue.Peek();

            if(playerData.respawnTime <= ((GameManagerFSM)stateMachine).gameManagerStat.globalTimer)
            {
                Debug.Log("부활");
                playerRespawnQue.Dequeue();
                PhotonView view = PhotonView.Find(playerData.characterViewID);
                view.RPC("PlayerReBornForAll", RpcTarget.All, playerData.spawnPoint);
                view.RPC("PlayerReBornPersonal", playerData.player);
                ((GameManagerFSM)stateMachine).gameManagerStat.deathUIView.RPC("DisableDeathCamUI", playerData.player);
                playerData.isAlive = true;
                playerData.respawnTime = 10000000.0f;

                if (playerData.team == "A")
                {
                    ((GameManagerFSM)stateMachine).gameManagerStat.                  
                        playersA[playerData.index] = playerData;

                    // 팀원 부활 알리기
                    for (int i = 0; i < ((GameManagerFSM)stateMachine).gameManagerStat.playersA.Count; i++)
                    {
                        ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView
                            .RPC("ShowTeamLifeDead", ((GameManagerFSM)stateMachine).gameManagerStat.playersA[i].player, playerData.name, false);
                    }
                }

                else
                {
                    ((GameManagerFSM)stateMachine).gameManagerStat.
                        playersB[playerData.index] = playerData;

                    // 팀원 부활 알리기
                    for (int i = 0; i < ((GameManagerFSM)stateMachine).gameManagerStat.playersB.Count; i++)
                    {
                        ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView
                            .RPC("ShowTeamLifeDead", ((GameManagerFSM)stateMachine).gameManagerStat.playersB[i].player, playerData.name, false);
                    }
                }
            }

        }

        void CheckingRoundEnd()
        {
            if (((GameManagerFSM)stateMachine).gameManagerStat.occupyingA.rate >= ((GameManagerFSM)stateMachine).gameManagerStat.occupyingComplete &&
                ((GameManagerFSM)stateMachine).gameManagerStat.currentOccupationTeam == ((GameManagerFSM)stateMachine).gameManagerStat.teamA)
            {
                ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "extraObj", true);
                ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "redExtraUI", false);
                ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "redExtraObj", true);
                ((GameManagerFSM)stateMachine).gameManagerStat.bgmManagerView.RPC("PlayAudio", RpcTarget.All, "RoundAlmostEnd", 1.0f, true, true, "BGM");

                if (((GameManagerFSM)stateMachine).gameManagerStat.teamBOccupying <= 0)
                    ((GameManagerFSM)stateMachine).gameManagerStat.roundEndTimer -= Time.deltaTime;
            }

            else if (((GameManagerFSM)stateMachine).gameManagerStat.occupyingB.rate >= 
                ((GameManagerFSM)stateMachine).gameManagerStat.occupyingComplete &&
                ((GameManagerFSM)stateMachine).gameManagerStat.currentOccupationTeam
                == ((GameManagerFSM)stateMachine).gameManagerStat.teamB)
            {
                ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "extraObj", true);
                ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "blueExtraUI", false);
                ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "blueExtraObj", true);
                ((GameManagerFSM)stateMachine).gameManagerStat.bgmManagerView.RPC("PlayAudio", RpcTarget.All, "RoundAlmostEnd", 1.0f, true, true, "BGM");

                if (((GameManagerFSM)stateMachine).gameManagerStat.teamAOccupying <= 0)
                    ((GameManagerFSM)stateMachine).gameManagerStat.roundEndTimer -= Time.deltaTime;
            }

            else
            {
                ((GameManagerFSM)stateMachine).gameManagerStat.roundEndTimer = 
                    ((GameManagerFSM)stateMachine).gameManagerStat.roundEndTime;
            }

            if (((GameManagerFSM)stateMachine).gameManagerStat.roundEndTimer <= 0.0f)
            {
                Debug.Log("라운드 승패 결정!!!!!!!!!!");

                if (((GameManagerFSM)stateMachine).gameManagerStat.currentOccupationTeam ==
                    ((GameManagerFSM)stateMachine).gameManagerStat.teamA)
                {
                    ((GameManagerFSM)stateMachine).gameManagerStat.occupyingA.rate = 100;

                    if(((GameManagerFSM)stateMachine).gameManagerStat.roundCount_A <
                        ((GameManagerFSM)stateMachine).gameManagerStat.roundEndNumber)
                    {
                        ((GameManagerFSM)stateMachine).gameManagerStat.roundCount_A++;
                        ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "redFirstPoint", true);

                        for(int i = 0; i < ((GameManagerFSM)stateMachine).gameManagerStat.playersA.Count; i++)
                        {
                            GameManagerStat.PlayerData playerData = ((GameManagerFSM)stateMachine).gameManagerStat.playersA[i];
                            ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ShowRoundWin", playerData.player,
                                ((GameManagerFSM)stateMachine).gameManagerStat.roundCount_A + ((GameManagerFSM)stateMachine).gameManagerStat.roundCount_B);
                            ((GameManagerFSM)stateMachine).gameManagerStat.bgmManagerView.RPC("PlayAudio", playerData.player, "RoundWin", 1.0f, false, true, "BGM");
                        }

                        for (int i = 0; i < ((GameManagerFSM)stateMachine).gameManagerStat.playersB.Count; i++)
                        {
                            GameManagerStat.PlayerData playerData = ((GameManagerFSM)stateMachine).gameManagerStat.playersB[i];
                            ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ShowRoundLoose", playerData.player,
                                ((GameManagerFSM)stateMachine).gameManagerStat.roundCount_A + ((GameManagerFSM)stateMachine).gameManagerStat.roundCount_B);
                            ((GameManagerFSM)stateMachine).gameManagerStat.bgmManagerView.RPC("PlayAudio", playerData.player, "RoundLoose", 1.0f, false, true, "BGM");
                        }

                    }

                    else
                    {
                        ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "redSecondPoint", true);

                        for (int i = 0; i < ((GameManagerFSM)stateMachine).gameManagerStat.playersA.Count; i++)
                        {
                            GameManagerStat.PlayerData playerData = ((GameManagerFSM)stateMachine).gameManagerStat.playersA[i];
                            ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", playerData.player, "victory", true);
                            ((GameManagerFSM)stateMachine).gameManagerStat.bgmManagerView.RPC("PlayAudio", playerData.player, "RoundWin", 1.0f, false, true, "BGM");
                        }

                        for (int i = 0; i < ((GameManagerFSM)stateMachine).gameManagerStat.playersB.Count; i++)
                        {
                            GameManagerStat.PlayerData playerData = ((GameManagerFSM)stateMachine).gameManagerStat.playersB[i];
                            ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", playerData.player, "defeat", true);
                            ((GameManagerFSM)stateMachine).gameManagerStat.bgmManagerView.RPC("PlayAudio", playerData.player, "RoundLoose", 1.0f, false, true, "BGM");
                        }
                    }

                }

                else if (((GameManagerFSM)stateMachine).gameManagerStat.currentOccupationTeam
                    == ((GameManagerFSM)stateMachine).gameManagerStat.teamB)
                {
                    ((GameManagerFSM)stateMachine).gameManagerStat.occupyingB.rate = 100;

                    if (((GameManagerFSM)stateMachine).gameManagerStat.roundCount_B <
                        ((GameManagerFSM)stateMachine).gameManagerStat.roundEndNumber)
                    {
                        ((GameManagerFSM)stateMachine).gameManagerStat.roundCount_B++;
                        ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "blueFirstPoint", true);

                        for (int i = 0; i < ((GameManagerFSM)stateMachine).gameManagerStat.playersA.Count; i++)
                        {
                            GameManagerStat.PlayerData playerData = ((GameManagerFSM)stateMachine).gameManagerStat.playersA[i];
                            ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ShowRoundLoose", playerData.player,
                                ((GameManagerFSM)stateMachine).gameManagerStat.roundCount_A + ((GameManagerFSM)stateMachine).gameManagerStat.roundCount_B);
                            ((GameManagerFSM)stateMachine).gameManagerStat.bgmManagerView.RPC("PlayAudio", playerData.player, "RoundLoose", 1.0f, false, true, "BGM");
                        }

                        for (int i = 0; i < ((GameManagerFSM)stateMachine).gameManagerStat.playersB.Count; i++)
                        {
                            GameManagerStat.PlayerData playerData = ((GameManagerFSM)stateMachine).gameManagerStat.playersB[i];
                            ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ShowRoundWin", playerData.player,
                                ((GameManagerFSM)stateMachine).gameManagerStat.roundCount_A + ((GameManagerFSM)stateMachine).gameManagerStat.roundCount_B);
                            ((GameManagerFSM)stateMachine).gameManagerStat.bgmManagerView.RPC("PlayAudio", playerData.player, "RoundWin", 1.0f, false, true, "BGM");
                        }

                    }

                    else
                    {
                        ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "blueSecondPoint", true);

                        for (int i = 0; i < ((GameManagerFSM)stateMachine).gameManagerStat.playersA.Count; i++)
                        {
                            GameManagerStat.PlayerData playerData = ((GameManagerFSM)stateMachine).gameManagerStat.playersA[i];
                            ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", playerData.player, "defeat", true);
                            ((GameManagerFSM)stateMachine).gameManagerStat.bgmManagerView.RPC("PlayAudio", playerData.player, "RoundLoose", 1.0f, false, true, "BGM");
                        }

                        for (int i = 0; i < ((GameManagerFSM)stateMachine).gameManagerStat.playersB.Count; i++)
                        {
                            GameManagerStat.PlayerData playerData = ((GameManagerFSM)stateMachine).gameManagerStat.playersB[i];
                            ((GameManagerFSM)stateMachine).gameManagerStat.inGameUIView.RPC("ActiveInGameUIObj", playerData.player, "victory", true);
                            ((GameManagerFSM)stateMachine).gameManagerStat.bgmManagerView.RPC("PlayAudio", playerData.player, "RoundWin", 1.0f, false, true, "BGM");
                        }
                    }
                }

                //라운드 종료
                stateMachine.ChangeState(((GameManagerFSM)stateMachine).gameManagerStat.GameStates[GameManagerStat.GameState.RoundEnd]);
            }
        }
    }
}