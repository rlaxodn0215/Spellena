using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using FSM;

public class GameManagerStat
{
    public GameManagerStat(StateMachine stateMachine) 
    {
        LinKingGameStates(stateMachine);
        LinkingProperties(stateMachine);
    }

    void LinKingGameStates(StateMachine stateMachine)
    {
        GameStates[GameState.InTheLobby] = null;
        GameStates[GameState.InTheRoom] = new InTheRoom(stateMachine);
        GameStates[GameState.LoadingScene] = new LoadingScene(stateMachine);
        GameStates[GameState.CharacterSelect] = new CharacterSelect(stateMachine);
        GameStates[GameState.GameReady] = new GameReady(stateMachine);
        GameStates[GameState.DuringRound] = new DuringRound(stateMachine);
        GameStates[GameState.RoundEnd] = new RoundEnd(stateMachine);
        GameStates[GameState.GameResult] = new GameResult(stateMachine);
    }

    void LinkingProperties(StateMachine stateMachine)
    {
        playerSpawnA = Helper.FindObject(((GameManagerFSM)stateMachine).playerSpawnPoints, "TeamA").GetComponentsInChildren<Transform>(true);
        playerSpawnB = Helper.FindObject(((GameManagerFSM)stateMachine).playerSpawnPoints, "TeamB").GetComponentsInChildren<Transform>(true);

        gameManagerPhotonView = ((GameManagerFSM)stateMachine).GetComponent<PhotonView>();
        //gameCenter.characterSelect = gameCenter.characterSelectObj.GetComponent<SelectingCharacter>();
        characterSelectView = ((GameManagerFSM)stateMachine).characterSelectObj.GetComponent<PhotonView>();
        bgmManager = ((GameManagerFSM)stateMachine).bgmManagerObj.GetComponent<BGMManager>();
        bgmManagerView = ((GameManagerFSM)stateMachine).bgmManagerObj.GetComponent<PhotonView>();
        deathUI = ((GameManagerFSM)stateMachine).deathUIObj.GetComponent<DeathCamUI>();
        deathUIView = ((GameManagerFSM)stateMachine).deathUIObj.GetComponent<PhotonView>();
        playerStat = ((GameManagerFSM)stateMachine).playerStatObj.GetComponent<PlayerStats>();
        inGameUI = ((GameManagerFSM)stateMachine).inGameUIObj.GetComponent<InGameUI>();
        inGameUIView = ((GameManagerFSM)stateMachine).inGameUIObj.GetComponent<PhotonView>();
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

    public struct PlayerData
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

    // 게임 상태 저장 Dictionary
    [HideInInspector]
    public Dictionary<GameState, BaseState> GameStates 
        = new Dictionary<GameState, BaseState>();

    // 라운드 점수
    [HideInInspector]
    public int roundCount_A = 1;
    [HideInInspector]
    public int roundCount_B = 1;

    // 점령 거점 차지하는 비율
    [HideInInspector]
    public int teamAOccupying = 0;
    [HideInInspector]
    public int teamBOccupying = 0;

    // 전체 타이머
    [HideInInspector]
    public float globalTimer = 0.0f;
    // 목표 전체 타이머 값
    [HideInInspector]
    public float globalDesiredTimer;
    // 거점 전환 타이머
    [HideInInspector]
    public float occupyingReturnTimer;
    // 추가시간 타이머
    [HideInInspector]
    public float roundEndTimer;
    // 현재 점령중인 팀
    [HideInInspector]
    public string currentOccupationTeam = "";

    // A팀의 점령도
    [HideInInspector]
    public Occupation occupyingA;
    // B팀의 점령도
    [HideInInspector]
    public Occupation occupyingB;
    // 점령 게이지 바
    [HideInInspector]
    public OccupyingTeam occupyingTeam;

    [HideInInspector]
    public List<PlayerData> playersA = new List<PlayerData>(); // Red
    [HideInInspector]
    public List<PlayerData> playersB = new List<PlayerData>(); // Blue 

    // 플레이어 소환 좌표
    [HideInInspector]
    public Transform[] playerSpawnA;
    [HideInInspector]
    public Transform[] playerSpawnB;

    // 이 이하는 테스트용 일시적 정보이다.
    // Scriptable Object로 데이터 전달

    [Tooltip("맵, 캐릭터 로딩 타임")]
    public float loadingTime = 1f;
    [Tooltip("캐릭터 선택 타임")]
    public float characterSelectTime = 1f;
    [Tooltip("대기실 준비 시간")]
    public float readyTime = 1f;
    [Tooltip("플레이어 리스폰 타임")]
    public float playerRespawnTime = 6;
    [Tooltip("어시스트 타임")]
    public float assistTime = 10;

    [Tooltip("민병대 쿨타임")]
    public float angelStatueCoolTime = 30.0f;
    [Tooltip("민병대 초당 체력 증가량")]
    public int angelStatueHpPerTime = 10;
    [Tooltip("민병대 효과 지속 시간")]
    public int angelStatueContinueTime = 10;

    [Tooltip("거점 전환 원 먹는 비율")]
    public float occupyingGaugeRate = 40f;
    [Tooltip("거점 전환하는 시간")]
    public float occupyingReturnTime = 3f;
    [Tooltip("거점 % 먹는 비율")]
    public float occupyingRate = 10f;
    [Tooltip("추가시간이 발생하는 기준 게이지")]
    public float occupyingComplete = 99f;
    [Tooltip("추가 시간")]
    public float roundEndTime = 5f;
    [Tooltip("라운드 결과 확인 시간")]
    public float roundEndResultTime = 6f;
    [Tooltip("게임 종료 라운드")]
    public int roundEndNumber = 2;

    // 팀 이름
    [HideInInspector]
    public string teamA = "A";
    [HideInInspector]
    public string teamB = "B";

    [HideInInspector]
    public PhotonView gameManagerPhotonView;
    [HideInInspector]
    public PhotonView characterSelectView;
    //[HideInInspector]
    //public SelectingCharacter characterSelect;
    [HideInInspector]
    public PhotonView inGameUIView;
    [HideInInspector]
    public InGameUI inGameUI;
    [HideInInspector]
    public BGMManager bgmManager;
    [HideInInspector]
    public PhotonView bgmManagerView;

    [HideInInspector]
    public GameObject betweenBGMObj;
    [HideInInspector]
    public AudioSource betweenBGMSource;

    [HideInInspector]
    public DeathCamUI deathUI;
    [HideInInspector]
    public PhotonView deathUIView;

    [HideInInspector]
    public PlayerStats playerStat;

}
