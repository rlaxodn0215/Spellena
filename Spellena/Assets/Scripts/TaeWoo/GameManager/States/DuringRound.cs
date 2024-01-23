using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using GameCenterDataType;

namespace FSM
{
    public class DuringRound : BaseState
    {
        public DuringRoundData duringRoundData;
        private DuringRoundStandardData duringRoundStandardData;

        public DeathCamUI deathUI;
        public PhotonView deathUIView;
        public PlayerStats playerStat;
        public InGameUI inGameUI;
        public PhotonView inGameUIView;

        public DuringRound(StateMachine stateMachine) :
            base("DuringRound", stateMachine)
        {
            InitDuringRoundData();
            InitDuringRoundStandardData();
        }

        void InitDuringRoundData()
        {
            duringRoundData = new DuringRoundData();
            ((GameCenter0)stateMachine).roundData.roundCount_A = 
                ((GameCenter0)stateMachine).roundData.roundCount_B = 1;
            duringRoundData.teamAOccupying = duringRoundData.teamBOccupying = 0;
            duringRoundData.currentOccupationTeam = "";
            duringRoundData.teamA = "A";
            duringRoundData.teamB = "B";
            duringRoundData.playerRespawnQue = new Queue<PlayerStat>();
            duringRoundData.flag = new BitFlag(0x0000);

            deathUI = ((GameCenter0)stateMachine).gameCenterObjs["DeathUI"].GetComponent<DeathCamUI>();
            if (deathUI == null) Debug.LogError("no deathUI");
            deathUIView = ((GameCenter0)stateMachine).gameCenterObjs["DeathUI"].GetComponent<PhotonView>();
            if (deathUIView == null) Debug.LogError("no deathUIView");
            playerStat = ((GameCenter0)stateMachine).gameCenterObjs["PlayerStats"].GetComponent<PlayerStats>();
            if (playerStat == null) Debug.LogError("no playerStat");
            inGameUI = ((GameCenter0)stateMachine).gameCenterObjs["InGameUI"].GetComponent<InGameUI>();
            if (inGameUI == null) Debug.LogError("no inGameUI");
            inGameUIView = ((GameCenter0)stateMachine).gameCenterObjs["InGameUI"].GetComponent<PhotonView>();
        }

        //scriptable object 적용
        void InitDuringRoundStandardData()
        {
            duringRoundStandardData.playerRespawnTime = 6;
            duringRoundStandardData.assistTime = 10;

            duringRoundStandardData.angelStatueCoolTime = 30.0f;
            duringRoundStandardData.angelStatueHpPerTime = 10;
            duringRoundStandardData.angelStatueContinueTime = 10;

            duringRoundStandardData.occupyingGaugeRate = 40f;
            duringRoundStandardData.occupyingReturnTime = 3f;
            duringRoundStandardData.occupyingRate = 10f;
            duringRoundStandardData.occupyingComplete = 99f;
            duringRoundStandardData.roundEndTime = 5f;
        }

        public override void FixedUpdate()
        {
            OccupyBarCounting();
            OccupyAreaCounting();
            CheckingPlayerReSpawn();
            CheckingRoundEnd();
        }

        void OccupyBarCounting()
        {
            //지역이 점령되어있으면 점령한 팀의 점령비율이 높아진다.
            if (duringRoundData.currentOccupationTeam == duringRoundData.teamA)
            {
                if (duringRoundData.teamBOccupying > 0)
                {
                    if (!duringRoundData.flag.BitCompare((uint)StateCheck.isFighting) &&
                        duringRoundData.teamAOccupying > 0)
                    {
                        //inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "fighting", true);
                        duringRoundData.flag.BitAdd((uint)StateCheck.isFighting);
                    }
                }
                else
                {

                    //duringRoundData.occupyingA.rate += Time.fixedDeltaTime * occupyingRate;//약 1.8초당 1씩 오름
                    if (duringRoundData.flag.BitCompare((uint)StateCheck.isFighting))
                    {
                        //inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "fighting", false);
                        duringRoundData.flag.BitSub((uint)StateCheck.isFighting);
                    }
                }

                if (duringRoundData.flag.BitCompare((uint)StateCheck.OccupyBarCountOnce))
                {
                    //bgmManagerView.RPC("PlayAudio", RpcTarget.All, "Occupying", 0.7f, false, true, "BGM");
                    duringRoundData.flag.BitSub((uint)StateCheck.OccupyBarCountOnce);
                }

                if (duringRoundData.occupyingA.rate >= duringRoundStandardData.occupyingComplete)
                    duringRoundData.occupyingA.rate = duringRoundStandardData.occupyingComplete;
            }

            else if (duringRoundData.currentOccupationTeam == duringRoundData.teamB)
            {

                if (duringRoundData.teamAOccupying > 0)
                {
                    if (!duringRoundData.flag.BitCompare((uint)StateCheck.isFighting) &&
                        duringRoundData.teamBOccupying > 0)
                    {
                        //inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "fighting", true);
                        duringRoundData.flag.BitAdd((uint)StateCheck.isFighting);
                    }
                }
                else
                {
                    duringRoundData.occupyingB.rate += Time.fixedDeltaTime * duringRoundStandardData.occupyingRate;//약 1.8초당 1씩 오름

                    if (duringRoundData.flag.BitCompare((uint)StateCheck.isFighting))
                    {
                        //inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "fighting", false);
                        duringRoundData.flag.BitSub((uint)StateCheck.isFighting);
                    }
                }

                if (duringRoundData.flag.BitCompare((uint)StateCheck.OccupyBarCountOnce))
                {
                    //bgmManagerView.RPC("PlayAudio", RpcTarget.All, "Occupying", 0.7f, false, true, "BGM");
                    duringRoundData.flag.BitSub((uint)StateCheck.OccupyBarCountOnce);
                }

                if (duringRoundData.occupyingB.rate >= duringRoundStandardData.occupyingComplete)
                    duringRoundData.occupyingB.rate = duringRoundStandardData.occupyingComplete;
            }
        }

        void OccupyAreaCounting()//점령 지역에 플레이어가 몇 명 점령하고 있는지 확인
        {
            duringRoundData.teamAOccupying = 0;
            duringRoundData.teamBOccupying = 0;

            GameObject temp;

            //Debug.Log("<color=green>" + "TeamAOccupying : " + gameCenter.teamAOccupying + "TeamBOccupying : " + gameCenter.teamBOccupying + "</color>");

            for (int i = 0; i < ((GameCenter0)stateMachine).playerList.playersA.Count; i++)
            {
                temp = Helper.FindObjectWithViewID(((GameCenter0)stateMachine).playerList.playersA[i].characterViewID);
                if (temp == null) continue;

                //if (temp.GetComponent<Character>() == null) continue;
                //if (temp.GetComponent<Character>().isOccupying == true)
                {
                    duringRoundData.teamAOccupying++;
                }
            }

            for (int i = 0; i < ((GameCenter0)stateMachine).playerList.playersB.Count; i++)
            {
                temp = Helper.FindObjectWithViewID(((GameCenter0)stateMachine).playerList.playersB[i].characterViewID);
                if (temp == null) continue;

                //if (temp.GetComponent<Character>() == null) continue;
                //if (temp.GetComponent<Character>().isOccupying == true)
                {
                    duringRoundData.teamBOccupying++;
                }
            }

            //Debug.Log("<color=green>" + "TeamAOccupying : " + gameCenter.teamAOccupying + "TeamBOccupying : " + gameCenter.teamBOccupying + "</color>");

            if (duringRoundData.teamAOccupying > 0 &&
                duringRoundData.teamBOccupying > 0)
            {
                //서로 교전 중이라는 것을 알림
                duringRoundData.occupyingReturnTimer = 0f;
                if (!duringRoundData.flag.BitCompare((uint)StateCheck.isFighting))
                {
                    //inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "fighting", true);
                    Debug.Log("<color=blue>" + "Active fighting" + "</color>");
                    duringRoundData.roundEndTimer = duringRoundStandardData.roundEndTime;
                    duringRoundData.flag.BitAdd((uint)StateCheck.isFighting);
                }
            }

            else if (duringRoundData.teamAOccupying > 0)//A팀 점령
            {
                ChangeOccupyingRate(duringRoundData.teamAOccupying, duringRoundData.teamA);
                duringRoundData.occupyingReturnTimer = 0f;
                if (duringRoundData.flag.BitCompare((uint)StateCheck.isFighting))
                {
                    //inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "fighting", false);
                    duringRoundData.flag.BitSub((uint)StateCheck.isFighting);
                }
            }
            else if (duringRoundData.teamBOccupying > 0)//B팀 점령
            {
                ChangeOccupyingRate(duringRoundData.teamBOccupying, duringRoundData.teamB);
                duringRoundData.occupyingReturnTimer = 0f;
                if (duringRoundData.flag.BitCompare((uint)StateCheck.isFighting))
                {
                    //inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "fighting", false);
                    Debug.Log("<color=blue>" + "DisActive fighting" + "</color>");
                    duringRoundData.flag.BitSub((uint)StateCheck.isFighting);
                }
            }
            else
            {
                duringRoundData.occupyingReturnTimer += Time.fixedDeltaTime;
                if (duringRoundData.flag.BitCompare((uint)StateCheck.isFighting))
                {
                    //inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "fighting", false);
                    Debug.Log("<color=blue>" + "DisActive fighting" + "</color>");
                    duringRoundData.flag.BitSub((uint)StateCheck.isFighting);
                }
            }

            if (duringRoundData.occupyingReturnTimer >= duringRoundStandardData.occupyingReturnTime)
            {
                if (duringRoundData.occupyingTeam.rate > 0f)
                {
                    duringRoundData.occupyingTeam.rate -= Time.fixedDeltaTime;
                    //bgmManagerView.RPC("StopAudio", RpcTarget.All, "Occupying");

                    if (duringRoundData.occupyingTeam.rate < 0f)
                    {
                        duringRoundData.occupyingTeam.rate = 0f;
                        duringRoundData.occupyingTeam.name = "";
                    }
                }
            }

        }

        void ChangeOccupyingRate(int num, string name) //점령 게이지 변화
        {
            if (duringRoundData.occupyingTeam.name == name)
            {
                if (duringRoundData.currentOccupationTeam == name) return;

                if (duringRoundData.occupyingTeam.rate >= 100)
                {
                    duringRoundData.currentOccupationTeam = name;
                    duringRoundData.occupyingTeam.name = "";
                    duringRoundData.occupyingTeam.rate = 0f;

                    //bgmManagerView.RPC("PlayAudio", RpcTarget.All, "Occupation", 1.0f, false, true, "BGM");

                    if (duringRoundData.currentOccupationTeam == "A")
                    {
                        //inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "captured_Red", true);
                        //inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "captured_Blue", false);
                        //inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "extraObj", false);
                        //inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "blueExtraObj", false);
                        //inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "blueExtraUI", true);

                        //angleStatue.GetComponent<PhotonView>().RPC("ChangeTeam", RpcTarget.All, "A");
                    }

                    else if (duringRoundData.currentOccupationTeam == "B")
                    {
                        //inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "captured_Red", false);
                        //inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "captured_Blue", true);
                        //inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "extraObj", false);
                        //inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "redExtraObj", false);
                        //inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "redExtraUI", true);

                        //angleStatue.GetComponent<PhotonView>().RPC("ChangeTeam", RpcTarget.All, "B");

                    }
                }

                else
                {
                    duringRoundData.occupyingTeam.rate += duringRoundStandardData.occupyingGaugeRate * Time.fixedDeltaTime;
                    //bgmManagerView.RPC("PlayAudio", RpcTarget.All, "Occupying", 1.0f, true, false, "BGM");
                }
            }
            else if (duringRoundData.occupyingTeam.name == "")
            {
                if (duringRoundData.currentOccupationTeam == name)
                    return;
                duringRoundData.occupyingTeam.name = name;
                duringRoundData.occupyingTeam.rate += duringRoundStandardData.occupyingGaugeRate * Time.fixedDeltaTime;
            }

            else
            {
                duringRoundData.occupyingTeam.rate -= duringRoundStandardData.occupyingGaugeRate * Time.fixedDeltaTime;
                //bgmManagerView.RPC("StopAudio", RpcTarget.All, "Occupying");
                if (duringRoundData.occupyingTeam.rate < 0)
                {
                    duringRoundData.occupyingTeam.name = "";
                    duringRoundData.occupyingTeam.rate = 0;
                }
            }
        }

        void CheckingPlayerReSpawn()
        {
            if (duringRoundData.playerRespawnQue.Count == 0) return;
            PlayerStat playerStat = duringRoundData.playerRespawnQue.Peek();

            if(playerStat.respawnTime <= ((GameCenter0)stateMachine).globalTimer.globalTime)
            {
                Debug.Log("부활");
                duringRoundData.playerRespawnQue.Dequeue();
                PhotonView view = PhotonView.Find(playerStat.characterViewID);
                view.RPC("PlayerReBornForAll", RpcTarget.All, playerStat.spawnPoint);
                view.RPC("PlayerReBornPersonal", playerStat.player);
                //deathUIView.RPC("DisableDeathCamUI", playerStat.player);
                playerStat.isAlive = true;
                playerStat.respawnTime = 10000000.0f;

                if (playerStat.team == "A")
                {
                    ((GameCenter0)stateMachine).playerList.playersA[playerStat.index] = playerStat;

                    // 팀원 부활 알리기
                    for (int i = 0; i < ((GameCenter0)stateMachine).playerList.playersA.Count; i++)
                    {
                        //inGameUIView.RPC("ShowTeamLifeDead", playersA[i].player, playerData.name, false);
                    }
                }

                else
                {

                    ((GameCenter0)stateMachine).playerList.playersB[playerStat.index] = playerStat;

                    // 팀원 부활 알리기
                    for (int i = 0; i < ((GameCenter0)stateMachine).playerList.playersB.Count; i++)
                    {
                        //inGameUIView.RPC("ShowTeamLifeDead", playersB[i].player, playerData.name, false);
                    }
                }
            }

        }

        void CheckingRoundEnd()
        {
            if (duringRoundData.occupyingA.rate >= duringRoundStandardData.occupyingComplete &&
                duringRoundData.currentOccupationTeam == duringRoundData.teamA)
            {
                //inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "extraObj", true);
                //inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "redExtraUI", false);
                //inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "redExtraObj", true);
                //bgmManagerView.RPC("PlayAudio", RpcTarget.All, "RoundAlmostEnd", 1.0f, true, true, "BGM");

                if (duringRoundData.teamBOccupying <= 0)
                    duringRoundData.roundEndTimer -= Time.fixedDeltaTime;
            }

            else if (duringRoundData.occupyingB.rate >= duringRoundStandardData.occupyingComplete &&
                duringRoundData.currentOccupationTeam == duringRoundData.teamB)
            {
                //inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "extraObj", true);
                //inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "blueExtraUI", false);
                //inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "blueExtraObj", true);
                //bgmManagerView.RPC("PlayAudio", RpcTarget.All, "RoundAlmostEnd", 1.0f, true, true, "BGM");

                if (duringRoundData.teamAOccupying <= 0)
                    duringRoundData.roundEndTimer -= Time.fixedDeltaTime;
            }

            else
            {
                duringRoundData.roundEndTimer = duringRoundStandardData.roundEndTime;
            }

            if (duringRoundData.roundEndTimer <= 0.0f)
            {
                Debug.Log("라운드 승패 결정!!!!!!!!!!");

                if (duringRoundData.currentOccupationTeam == duringRoundData.teamA)
                {
                    duringRoundData.occupyingA.rate = 100;

                    if(duringRoundData.roundCount_A < duringRoundStandardData.roundEndNumber)
                    {
                        duringRoundData.roundCount_A++;
                        //inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "redFirstPoint", true);

                        for(int i = 0; i < ((GameCenter0)stateMachine).playerList.playersA.Count; i++)
                        {
                            PlayerStat playerData = ((GameCenter0)stateMachine).playerList.playersA[i];
                            //inGameUIView.RPC("ShowRoundWin", playerData.player,roundCount_A + roundCount_B);
                            //bgmManagerView.RPC("PlayAudio", playerData.player, "RoundWin", 1.0f, false, true, "BGM");
                        }

                        for (int i = 0; i < ((GameCenter0)stateMachine).playerList.playersB.Count; i++)
                        {
                            PlayerStat playerData = ((GameCenter0)stateMachine).playerList.playersB[i];
                            //inGameUIView.RPC("ShowRoundLoose", playerData.player,roundCount_A + roundCount_B);
                            //bgmManagerView.RPC("PlayAudio", playerData.player, "RoundLoose", 1.0f, false, true, "BGM");
                        }

                    }

                    else
                    {
                        //inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "redSecondPoint", true);

                        for (int i = 0; i < ((GameCenter0)stateMachine).playerList.playersA.Count; i++)
                        {
                            PlayerStat playerData = ((GameCenter0)stateMachine).playerList.playersA[i];
                            //inGameUIView.RPC("ActiveInGameUIObj", playerData.player, "victory", true);
                            //bgmManagerView.RPC("PlayAudio", playerData.player, "RoundWin", 1.0f, false, true, "BGM");
                        }

                        for (int i = 0; i < ((GameCenter0)stateMachine).playerList.playersB.Count; i++)
                        {
                            PlayerStat playerData = ((GameCenter0)stateMachine).playerList.playersB[i];
                            //inGameUIView.RPC("ActiveInGameUIObj", playerData.player, "defeat", true);
                            //bgmManagerView.RPC("PlayAudio", playerData.player, "RoundLoose", 1.0f, false, true, "BGM");
                        }
                    }

                }

                else if (duringRoundData.currentOccupationTeam == duringRoundData.teamB)
                {
                    duringRoundData.occupyingB.rate = 100;

                    if (duringRoundData.roundCount_B < duringRoundStandardData.roundEndNumber)
                    {
                        duringRoundData.roundCount_B++;
                        //inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "blueFirstPoint", true);

                        for (int i = 0; i < ((GameCenter0)stateMachine).playerList.playersA.Count; i++)
                        {
                            PlayerStat playerData = ((GameCenter0)stateMachine).playerList.playersA[i];
                            //inGameUIView.RPC("ShowRoundLoose", playerData.player,roundCount_A + roundCount_B);
                            //bgmManagerView.RPC("PlayAudio", playerData.player, "RoundLoose", 1.0f, false, true, "BGM");
                        }

                        for (int i = 0; i < ((GameCenter0)stateMachine).playerList.playersB.Count; i++)
                        {
                            PlayerStat playerData = ((GameCenter0)stateMachine).playerList.playersB[i];
                            //inGameUIView.RPC("ShowRoundWin", playerData.player,roundCount_A + roundCount_B);
                            //bgmManagerView.RPC("PlayAudio", playerData.player, "RoundWin", 1.0f, false, true, "BGM");
                        }

                    }

                    else
                    {
                        //inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "blueSecondPoint", true);

                        for (int i = 0; i < ((GameCenter0)stateMachine).playerList.playersA.Count; i++)
                        {
                            PlayerStat playerData = ((GameCenter0)stateMachine).playerList.playersA[i];
                            //inGameUIView.RPC("ActiveInGameUIObj", playerData.player, "defeat", true);
                            //bgmManagerView.RPC("PlayAudio", playerData.player, "RoundLoose", 1.0f, false, true, "BGM");
                        }

                        for (int i = 0; i < ((GameCenter0)stateMachine).playerList.playersB.Count; i++)
                        {
                            PlayerStat playerData = ((GameCenter0)stateMachine).playerList.playersB[i];
                            //inGameUIView.RPC("ActiveInGameUIObj", playerData.player, "victory", true);
                            //bgmManagerView.RPC("PlayAudio", playerData.player, "RoundWin", 1.0f, false, true, "BGM");
                        }
                    }
                }

                //라운드 종료
                stateMachine.ChangeState(((GameCenter0)stateMachine).GameStates[GameState.RoundEnd]);
            }
        }
    }
}