using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCenterDataType
{
    public struct PlayerStat
    {
        public int index;
        public string name;
        public Photon.Realtime.Player player;
        public string character;
        public int characterViewID;
        public string team;

        public int killCount;
        public int deadCount;
        public int assistCount;
        public int ultimateCount;

        public int totalDamage;
        public int totalHeal;
        public bool isAlive;

        public float angleStatueCoolTime;
        public string killerName;

        public float respawnTime;
        public Vector3 spawnPoint;

        public string damagePart;
        public Vector3 damageDirection;
        public float damageForce;

        public int playerAssistViewID;
    }

    public struct OccupyingTeam
    {
        public string name;
        public float rate;
    }

    public struct Occupation
    {
        public float rate;
    }

    public struct GlobalTimer
    {
        // 전체 타이머
        public float globalTime;
        // 목표 전체 타이머 값
        public float globalDesiredTime;
    }

    public struct LoadingTimer
    {
        //[Tooltip("맵, 캐릭터 로딩 타임")]
        public float loadingTime;
    }

    public struct CharacterSelectTimer
    {
        //[Tooltip("캐릭터 선택 타임")]
        public float characterSelectTime;
    }

    public struct PlayerList
    {
        public List<PlayerStat> playersA; // Red
        public List<PlayerStat> playersB; // Blue 
    }

    public struct RoundData
    {
        // 라운드 점수
        public int roundCount_A;
        public int roundCount_B;
    }

    public struct DuringRoundData
    {
        // 점령 거점 차지하는 비율
        public int teamAOccupying;
        public int teamBOccupying;

        // 거점 전환 타이머
        public float occupyingReturnTimer;
        // 추가시간 타이머
        public float roundEndTimer;
        // 현재 점령중인 팀
        public string currentOccupationTeam;

        // A팀의 점령도
        public Occupation occupyingA;
        // B팀의 점령도
        public Occupation occupyingB;
        // 점령 게이지 바
        public OccupyingTeam occupyingTeam;

        // 팀 이름
        public string teamA;
        public string teamB;

        // 플레이어 부활 큐
        public Queue<PlayerStat> playerRespawnQue;

        public BitFlag flag;
    }
    public struct GameReadyStandardData
    {
        //[Tooltip("대기실 준비 시간")]
        public float readyTime;
    }


    public struct DuringRoundStandardData
    {
        //[Tooltip("플레이어 리스폰 타임")]
        public float playerRespawnTime;
        //[Tooltip("어시스트 타임")]
        public float assistTime;

        //[Tooltip("민병대 쿨타임")]
        public float angelStatueCoolTime;
        //[Tooltip("민병대 초당 체력 증가량")]
        public int angelStatueHpPerTime;
        //[Tooltip("민병대 효과 지속 시간")]
        public int angelStatueContinueTime;

        //[Tooltip("거점 전환 원 먹는 비율")]
        public float occupyingGaugeRate;
        //[Tooltip("거점 전환하는 시간")]
        public float occupyingReturnTime;
        //[Tooltip("거점 % 먹는 비율")]
        public float occupyingRate;
        //[Tooltip("추가시간이 발생하는 기준 게이지")]
        public float occupyingComplete;
        //[Tooltip("추가 시간")]
        public float roundEndTime;
    }

    public struct RoundEndStandardData
    {
        //[Tooltip("라운드 결과 확인 시간")]
        public float roundEndResultTime;
        //[Tooltip("게임 종료 라운드")]
        public int roundEndNumber;
    }

    public enum GameState
    {
        InTheLobby,
        InTheRoom,
        LoadingScene,
        CharacterSelect,
        GameReady,
        DuringRound,
        RoundEnd,
        GameResult
    }

    public enum StateCheck
    {
        OccupyBarCountOnce = 0x0001,
        isFighting = 0x0002
    }

    public struct BitFlag
    {
        public uint flag;
        public BitFlag(uint _flag)
        {
            this.flag = _flag;
        }

        public void BitAdd(uint _num)
        {
            this.flag |= _num;
        }

        public void BitSub(uint _num)
        {
            uint temp = this.flag & _num;
            this.flag ^= temp;
        }

        public bool BitCompare(uint _num)
        {
            return ((this.flag ^ _num) == 0);
        }

    }
}
