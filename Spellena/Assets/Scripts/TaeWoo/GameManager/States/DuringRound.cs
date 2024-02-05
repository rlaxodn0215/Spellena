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
        public OccupationArea occupationArea;
        public AngleStatue angleStatue;

        public DuringRound(StateMachine stateMachine) :
            base("DuringRound", stateMachine)
        {
            InitDuringRoundData();
            InitDuringRoundStandardData();
        }

        public override void FixedUpdate()
        {
            OccupyBarCounting();
            OccupyAreaCounting();
            CheckingPlayerReSpawn();
            CheckingRoundEnd();
            SerializeInGameUI();
        }

        public override void Exit()
        {
            RoundEndCounting();
            SerializeInGameUI();
        }

        void InitDuringRoundData()
        {
            duringRoundData = new DuringRoundData();
            ((GameCenter0)stateMachine).roundData.roundCount_A =
                ((GameCenter0)stateMachine).roundData.roundCount_B = 0;
            ((GameCenter0)stateMachine).roundData.teamA = "A";
            ((GameCenter0)stateMachine).roundData.teamB = "B";
            duringRoundData.teamAOccupying = duringRoundData.teamBOccupying = 0;
            duringRoundData.currentOccupationTeam = "";
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
            if (inGameUI == null) Debug.LogError("no inGameUIView");
            occupationArea = ((GameCenter0)stateMachine).gameCenterObjs["OccupationArea"].GetComponent<OccupationArea>();
            if (occupationArea == null) Debug.LogError("no occupationArea");
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

            inGameUI.duringRoundStandardData = duringRoundStandardData;
        }

        void OccupyBarCounting()
        {
            //지역이 점령되어있으면 점령한 팀의 점령비율이 높아진다.
            if (duringRoundData.currentOccupationTeam == ((GameCenter0)stateMachine).roundData.teamA)
            {
                if (duringRoundData.teamBOccupying > 0)
                {
                    if (!duringRoundData.flag.BitCompare((uint)StateCheck.isFighting) &&
                        duringRoundData.teamAOccupying > 0)
                    {
                        inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "fighting", true);
                        duringRoundData.flag.BitAdd((uint)StateCheck.isFighting);
                    }
                }
                else
                {

                    duringRoundData.occupyingA.rate += Time.fixedDeltaTime * duringRoundStandardData.occupyingRate;//약 1.8초당 1씩 오름
                    if (duringRoundData.flag.BitCompare((uint)StateCheck.isFighting))
                    {
                        inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "fighting", false);
                        duringRoundData.flag.BitSub((uint)StateCheck.isFighting);
                    }
                }

                if (duringRoundData.flag.BitCompare((uint)StateCheck.OccupyBarCountOnce))
                {
                    ((GameCenter0)stateMachine).bgmManagerView.RPC("PlayAudio", RpcTarget.All, "Occupying", 0.7f, false, true, "BGM");
                    duringRoundData.flag.BitSub((uint)StateCheck.OccupyBarCountOnce);
                }

                if (duringRoundData.occupyingA.rate >= duringRoundStandardData.occupyingComplete)
                    duringRoundData.occupyingA.rate = duringRoundStandardData.occupyingComplete;
            }

            else if (duringRoundData.currentOccupationTeam == ((GameCenter0)stateMachine).roundData.teamB)
            {

                if (duringRoundData.teamAOccupying > 0)
                {
                    if (!duringRoundData.flag.BitCompare((uint)StateCheck.isFighting) &&
                        duringRoundData.teamBOccupying > 0)
                    {
                        inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "fighting", true);
                        duringRoundData.flag.BitAdd((uint)StateCheck.isFighting);
                    }
                }
                else
                {
                    duringRoundData.occupyingB.rate += Time.fixedDeltaTime * duringRoundStandardData.occupyingRate;//약 1.8초당 1씩 오름

                    if (duringRoundData.flag.BitCompare((uint)StateCheck.isFighting))
                    {
                        inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "fighting", false);
                        duringRoundData.flag.BitSub((uint)StateCheck.isFighting);
                    }
                }

                if (duringRoundData.flag.BitCompare((uint)StateCheck.OccupyBarCountOnce))
                {
                    ((GameCenter0)stateMachine).bgmManagerView.RPC("PlayAudio", RpcTarget.All, "Occupying", 0.7f, false, true, "BGM");
                    duringRoundData.flag.BitSub((uint)StateCheck.OccupyBarCountOnce);
                }

                if (duringRoundData.occupyingB.rate >= duringRoundStandardData.occupyingComplete)
                    duringRoundData.occupyingB.rate = duringRoundStandardData.occupyingComplete;
            }
        }

        void OccupyAreaCounting()//점령 지역에 플레이어가 몇 명 점령하고 있는지 확인
        {
            duringRoundData.teamAOccupying = occupationArea.GetTeamCount("A");
            duringRoundData.teamBOccupying = occupationArea.GetTeamCount("B");

            //Debug.Log("<color=green>" + "TeamAOccupying : " + gameCenter.teamAOccupying + "TeamBOccupying : " + gameCenter.teamBOccupying + "</color>");

            if (duringRoundData.teamAOccupying > 0 &&
                duringRoundData.teamBOccupying > 0)
            {
                //서로 교전 중이라는 것을 알림
                duringRoundData.occupyingReturnTimer = 0f;
                if (!duringRoundData.flag.BitCompare((uint)StateCheck.isFighting))
                {
                    inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "fighting", true);
                    duringRoundData.roundEndTimer = duringRoundStandardData.roundEndTime;
                    duringRoundData.flag.BitAdd((uint)StateCheck.isFighting);
                }
            }

            else if (duringRoundData.teamAOccupying > 0)//A팀 점령
            {
                ChangeOccupyingRate(duringRoundData.teamAOccupying, ((GameCenter0)stateMachine).roundData.teamA);
                duringRoundData.occupyingReturnTimer = 0f;
                if (duringRoundData.flag.BitCompare((uint)StateCheck.isFighting))
                {
                    inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "fighting", false);
                    duringRoundData.flag.BitSub((uint)StateCheck.isFighting);
                }
            }
            else if (duringRoundData.teamBOccupying > 0)//B팀 점령
            {
                ChangeOccupyingRate(duringRoundData.teamBOccupying, ((GameCenter0)stateMachine).roundData.teamB);
                duringRoundData.occupyingReturnTimer = 0f;
                if (duringRoundData.flag.BitCompare((uint)StateCheck.isFighting))
                {
                    inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "fighting", false);
                    Debug.Log("<color=blue>" + "DisActive fighting" + "</color>");
                    duringRoundData.flag.BitSub((uint)StateCheck.isFighting);
                }
            }
            else
            {
                duringRoundData.occupyingReturnTimer += Time.fixedDeltaTime;
                if (duringRoundData.flag.BitCompare((uint)StateCheck.isFighting))
                {
                    inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "fighting", false);
                    ((GameCenter0)stateMachine).bgmManagerView.RPC("StopAudio", RpcTarget.All, "Occupying");
                    Debug.Log("<color=blue>" + "DisActive fighting" + "</color>");
                    duringRoundData.flag.BitSub((uint)StateCheck.isFighting);
                }
            }

            if (duringRoundData.occupyingReturnTimer >= duringRoundStandardData.occupyingReturnTime)
            {
                if (duringRoundData.occupyingTeam.rate > 0f)
                {
                    duringRoundData.occupyingTeam.rate -= Time.fixedDeltaTime;

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

                    ((GameCenter0)stateMachine).bgmManagerView.RPC("PlayAudio", RpcTarget.All, "Occupation", 1.0f, false, true, "BGM");

                    if (duringRoundData.currentOccupationTeam == ((GameCenter0)stateMachine).roundData.teamA)
                    {
                        ChangeOccupyingUI(true);
                    }

                    else
                    {
                        ChangeOccupyingUI(false);
                    }
                }

                else
                {
                    duringRoundData.occupyingTeam.rate += duringRoundStandardData.occupyingGaugeRate * Time.fixedDeltaTime;
                    //((GameCenter0)stateMachine).bgmManagerView.RPC("PlayAudio", RpcTarget.All, "Occupying", 1.0f, true, false, "BGM");
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
                //((GameCenter0)stateMachine).bgmManagerView.RPC("StopAudio", RpcTarget.All, "Occupying");
                if (duringRoundData.occupyingTeam.rate < 0)
                {
                    duringRoundData.occupyingTeam.name = "";
                    duringRoundData.occupyingTeam.rate = 0;
                }
            }
        }

        void ChangeOccupyingUI(bool isATeam)
        {
            inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "captured_Red", isATeam);
            inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "captured_Blue", !isATeam);
            inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "extraObj", !isATeam);
            inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "blueExtraObj", !isATeam);
            inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "blueExtraUI", isATeam);

            if(isATeam)
                angleStatue.GetComponent<PhotonView>().RPC("ChangeTeam", RpcTarget.All, "A");
            else
                angleStatue.GetComponent<PhotonView>().RPC("ChangeTeam", RpcTarget.All, "B");
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
                //view.RPC("PlayerReBornForAll", RpcTarget.All, playerStat.spawnPoint);
                //view.RPC("PlayerReBornPersonal", playerStat.player);
                deathUIView.RPC("DisableDeathCamUI", playerStat.player);
                playerStat.isAlive = true;
                playerStat.respawnTime = 10000000.0f;
                inGameUIView.RPC("ShowTeamLifeDead", RpcTarget.All, playerStat.name, false);
            }

        }

        void CheckingRoundEnd()
        {
            if (duringRoundData.occupyingA.rate >= duringRoundStandardData.occupyingComplete &&
                duringRoundData.currentOccupationTeam == ((GameCenter0)stateMachine).roundData.teamA)
            {
                RoundEndUI(true);
                if (duringRoundData.teamBOccupying <= 0)
                    duringRoundData.roundEndTimer -= Time.fixedDeltaTime;
            }

            else if (duringRoundData.occupyingB.rate >= duringRoundStandardData.occupyingComplete &&
                duringRoundData.currentOccupationTeam == ((GameCenter0)stateMachine).roundData.teamB)
            {
                RoundEndUI(false);
                if (duringRoundData.teamAOccupying <= 0)
                    duringRoundData.roundEndTimer -= Time.fixedDeltaTime;
            }

            else
            {
                duringRoundData.roundEndTimer = duringRoundStandardData.roundEndTime;
            }

            if (duringRoundData.roundEndTimer <= 0.0f)
            {
                //라운드 종료
                stateMachine.ChangeState(((GameCenter0)stateMachine).GameStates[GameState.RoundEnd]);
            }
        }

        void RoundEndUI(bool isATeam)
        {
            inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "extraObj", isATeam);
            inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "redExtraUI", !isATeam);
            inGameUIView.RPC("ActiveInGameUIObj", RpcTarget.All, "redExtraObj", isATeam);

            ((GameCenter0)stateMachine).bgmManagerView.RPC("PlayAudio", RpcTarget.All, "RoundAlmostEnd", 1.0f, true, true, "BGM");
        }

        void RoundEndCounting()
        {
            if (duringRoundData.currentOccupationTeam == ((GameCenter0)stateMachine).roundData.teamA)
            {
                duringRoundData.occupyingA.rate = 100;
                ((GameCenter0)stateMachine).roundData.roundCount_A++;
            }

            else
            {
                duringRoundData.occupyingB.rate = 100;
                ((GameCenter0)stateMachine).roundData.roundCount_B++;
            }
        }

        void SerializeInGameUI()
        {
            inGameUI.globalTimer = ((GameCenter0)stateMachine).globalTimer.globalTime;
            inGameUI.duringRoundData = duringRoundData;
        }

        [PunRPC]
        public void UpdateTotalDamage(string victim, string enemy, int damage)
        {
            if (!((GameCenter0)stateMachine).players.ContainsKey(enemy)) return;
            PlayerStat temp = ((GameCenter0)stateMachine).players[enemy];

            temp.totalDamage += damage;
            ((GameCenter0)stateMachine).players[enemy] = temp;

            temp = ((GameCenter0)stateMachine).players[victim];

            AssistData assist;
            assist.name = enemy;
            assist.time = ((GameCenter0)stateMachine).globalTimer.globalTime + duringRoundStandardData.assistTime;
            temp.attackedData.Add(assist);

            ((GameCenter0)stateMachine).players[victim] = temp;
        }

        [PunRPC]
        public void UpdatePlayerDead(string victim, string killer)
        {
            SettingVictim(victim);
            SettingKiller(killer, victim);
        }

        void SettingVictim(string victim)
        {
            PlayerStat temp = ((GameCenter0)stateMachine).players[victim];
            temp.isAlive = false;
            temp.deadCount++;
            temp.respawnTime = ((GameCenter0)stateMachine).globalTimer.globalTime
                + duringRoundStandardData.playerRespawnTime;
            CheckDealAssist(temp.attackedData, victim);
            temp.attackedData.Clear();
            ((GameCenter0)stateMachine).players[victim] = temp;
        }

        void CheckDealAssist(List<AssistData> datas, string victim)
        {
            PlayerStat temp;

            for (int i = 0; i < datas.Count - 1; i++)
            {
                if (datas[i].time <= ((GameCenter0)stateMachine).globalTimer.globalTime)
                {
                    temp = ((GameCenter0)stateMachine).players[datas[i].name];
                    temp.assistCount++;
                    inGameUIView.RPC("ShowAssistUI", temp.player, victim);
                    ((GameCenter0)stateMachine).players[datas[i].name] = temp;
                }
            }
        }

        void SettingKiller(string killer, string victim)
        {
            PlayerStat temp = ((GameCenter0)stateMachine).players[killer];
            temp.killCount++;
            inGameUIView.RPC("ShowKillUI", temp.player, victim);
            ((GameCenter0)stateMachine).players[killer] = temp;
        }
    }
}