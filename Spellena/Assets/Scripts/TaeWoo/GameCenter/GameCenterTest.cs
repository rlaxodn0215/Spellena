using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameCenterTest : MonoBehaviourPunCallbacks
{
    public GameObject angleStatue;
    public GameObject playerSpawnPoints;
    public GameObject bgmManagerObj;

    public GameObject characterSelectObj;
    public GameObject inGameUIObj;
    public GameObject deathUIObj;
    public GameObject gameResultObj;
    public GameObject playerStatObj;

    public GameCenterTestData gameCenterTestData;

    [HideInInspector]
    public PhotonView characterSelectView;
    [HideInInspector]
    public SelectingCharacter characterSelect;
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

    // 플레이어 소환 좌표
    [HideInInspector]
    public Transform[] playerSpawnA;
    [HideInInspector]
    public Transform[] playerSpawnB;

    public enum GameState
    {
        CharacterSelect,
        GameReady,
        DuringRound,
        RoundEnd,
        GameResult
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

    [HideInInspector]
    public Dictionary<GameState, CenterState> centerStates = new Dictionary<GameState, CenterState>();

    [HideInInspector]
    public GameState currentGameState;
    [HideInInspector]
    public CenterState currentCenterState;

    // 일시적인 죽은 플레이어
    [HideInInspector]
    public string tempVictim = "";

    // 라운드 점수
    [HideInInspector]
    public static int roundA = 0;
    [HideInInspector]
    public static int roundB = 0;

    // 점령 거점 차지하는 비율
    [HideInInspector]
    public int teamAOccupying = 0;
    [HideInInspector]
    public int teamBOccupying = 0;

    // 전체 타이머
    [HideInInspector]
    public static float globalTimer = 0.0f;
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

    //[HideInInspector]
    public List<Photon.Realtime.Player> playersA = new List<Photon.Realtime.Player>(); // Red
    //[HideInInspector]
    public List<Photon.Realtime.Player> playersB = new List<Photon.Realtime.Player>(); // Blue 

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

    // 팀 이름
    [HideInInspector]
    public string teamA = "A";
    [HideInInspector]
    public string teamB = "B";

    void Start()
    {
        InitManager();
    }

    void InitManager()
    {
        CharacterSelect temp2 = gameObject.AddComponent<CharacterSelect>();
        temp2.ConnectCenter(this);
        centerStates.Add(GameState.CharacterSelect, temp2);

        GameReady temp3 = gameObject.AddComponent<GameReady>();
        temp3.ConnectCenter(this);
        centerStates.Add(GameState.GameReady, temp3);

        DuringRound temp4 = gameObject.AddComponent<DuringRound>();
        temp4.ConnectCenter(this);
        centerStates.Add(GameState.DuringRound, temp4);

        RoundEnd temp5 = gameObject.AddComponent<RoundEnd>();
        temp5.ConnectCenter(this);
        centerStates.Add(GameState.RoundEnd, temp5);

        GameResult temp7 = gameObject.AddComponent<GameResult>();
        temp7.ConnectCenter(this);
        centerStates.Add(GameState.GameResult, temp7);

        bgmManager = bgmManagerObj.GetComponent<BGMManager>();
        bgmManagerView = bgmManagerObj.GetComponent<PhotonView>();
        deathUI = deathUIObj.GetComponent<DeathCamUI>();
        deathUIView = deathUIObj.GetComponent<PhotonView>();
        playerStat = playerStatObj.GetComponent<PlayerStats>();
        inGameUIView = inGameUIObj.GetComponent<PhotonView>();

        ConnectBetweenBGM("LoadingCharacterBGM");

        currentGameState = GameState.CharacterSelect;
        currentCenterState = centerStates[currentGameState];

        loadingTime = gameCenterTestData.loadingTime;
        characterSelectTime = gameCenterTestData.characterSelectTime;
        readyTime = gameCenterTestData.readyTime;
        playerRespawnTime = gameCenterTestData.playerRespawnTime;
        assistTime = gameCenterTestData.assistTime;

        angleStatue.GetComponent<AngelStatue>().angelStatueCoolTime = angelStatueCoolTime = gameCenterTestData.angelStatueCoolTime;
        angelStatueHpPerTime = gameCenterTestData.angelStatueHpPerTime;
        angelStatueContinueTime = gameCenterTestData.angelStatueContinueTime;

        occupyingGaugeRate = gameCenterTestData.occupyingGaugeRate;
        occupyingReturnTime = gameCenterTestData.occupyingReturnTime;
        occupyingRate = gameCenterTestData.occupyingRate;
        occupyingComplete = gameCenterTestData.occupyingComplete;
        roundEndTime = gameCenterTestData.roundEndTime;
        roundEndResultTime = gameCenterTestData.roundEndResultTime;

    }

    void ConnectBetweenBGM(string objName)
    {
        betweenBGMObj = GameObject.Find(objName);
        if(betweenBGMObj == null)
        {
            Debug.LogError("씬 연속 BGM 게임 오브젝트를 찾을 수 없음");
            return;
        }

        betweenBGMSource = betweenBGMObj.GetComponent<AudioSource>();
    }

    private void FixedUpdate()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            currentCenterState.StateExecution();
            currentCenterState = centerStates[currentGameState];

            photonView.RPC("SerializeGameCenterDatas", RpcTarget.AllBufferedViaServer, ToDoSerlize());

            if (inGameUI != null)
                GiveDataToUI();
        }

        if (currentGameState == GameState.CharacterSelect)
        {
            BGMVolControl();
        }
    }

    float soundDecreaseTime = 5;
    float soundDecreaseSpeed = 1.5f;

    void BGMVolControl()
    {
        // 일정 시간이 지나면 소리가 점차 감소됨
        if (globalDesiredTimer - globalTimer <= soundDecreaseTime)
        {
            if (betweenBGMSource != null)
            {
                BetweenBGMVolumControl(soundDecreaseSpeed * Time.deltaTime / 10, false);
            }
        }

        else
        {
            if (betweenBGMSource != null)
            {
                betweenBGMSource.volume = 1.0f * SettingManager.Instance.bgmVal * SettingManager.Instance.soundVal;
            }
        }
    }

    object[] ToDoSerlize()
    {
        object[] datas = new object[14];

        datas[0] = currentGameState;
        datas[2] = roundA;
        datas[3] = roundB;
        datas[4] = teamAOccupying;
        datas[5] = teamBOccupying;
        datas[6] = globalTimer;
        datas[7] = occupyingReturnTimer;
        datas[8] = roundEndTimer;
        datas[9] = currentOccupationTeam;
        datas[10] = occupyingA.rate;
        datas[11] = occupyingB.rate;
        datas[12] = occupyingTeam.name;
        datas[13] = occupyingTeam.rate;

        return datas;
    }

    [PunRPC]
    public void SerializeGameCenterDatas(object[] datas)
    {
        currentGameState = (GameState)datas[0];
        roundA = (int)datas[2];
        roundB = (int)datas[3];
        teamAOccupying = (int)datas[4];
        teamBOccupying = (int)datas[5];
        globalTimer = (float)datas[6];
        occupyingReturnTimer = (float)datas[7];
        roundEndTimer = (float)datas[8];
        currentOccupationTeam = (string)datas[9];
        occupyingA.rate = (float)datas[10];
        occupyingB.rate = (float)datas[11];
        occupyingTeam.name = (string)datas[12];
        occupyingTeam.rate = (float)datas[13];
    }

    [PunRPC]
    public void ActiveObject(string name, bool isActive)
    {
        switch(name)
        {
            case "characterSelectObj":
                characterSelectObj.SetActive(isActive);
                break;
            case "inGameUIObj":
                inGameUIObj.transform.GetChild(0).gameObject.SetActive(isActive);
                break;
            case "betweenBGMObj":
                betweenBGMObj.SetActive(isActive);
                break;
            case "gameResultObj":
                gameResultObj.SetActive(isActive);
                break;
            case "playerStatObj":
                playerStatObj.SetActive(isActive);
                break;
            case "deathUIObj":
                deathUIObj.SetActive(isActive);
                break;
            default:
                Debug.LogWarning("잘못된 게임 오브젝트 이름 사용");
                break;
        }
    }

    [PunRPC]
    public void ShowGameResult()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            PhotonView view = PhotonView.Find((int)player.CustomProperties["CharacterViewID"]);
            if (view == null) continue;

            view.RPC("DisActiveMe", RpcTarget.AllBuffered);
        }

        playerStatObj.SetActive(false);
        deathUIObj.SetActive(false);
        inGameUIObj.SetActive(false);
        betweenBGMObj.SetActive(true);
        betweenBGMSource.Stop();

        gameResultObj.SetActive(true);
    }

    [PunRPC]
    public void BetweenBGMPlay(bool isPlay)
    {
        if (betweenBGMSource.clip == null) return;

        if(isPlay)
        {
            betweenBGMSource.Play();
        }

        else
        {
            betweenBGMSource.Stop();
        }
    }

    [PunRPC]
    public void BetweenBGMVolumControl(float size, bool isIncrease)
    {
        if (betweenBGMSource.clip == null) return;

        if (isIncrease)
        {
            betweenBGMSource.volume += size * SettingManager.Instance.bgmVal * SettingManager.Instance.soundVal;
        }

        else
        {
            betweenBGMSource.volume -= size * SettingManager.Instance.bgmVal * SettingManager.Instance.soundVal;
        }
    }

    public static Photon.Realtime.Player FindPlayerWithCustomProperty(string key, string value)
    {
        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (player.CustomProperties.ContainsKey(key) && player.CustomProperties[key].ToString() == value)
            {
                return player;
            }
        }

        return null;
    }

    // 이 함수를 사용해야 클라이언트의 모든 변수가 동기화 된다. / 그냥 대입은 동기화 안됨
    public static void ChangePlayerCustomProperties(Photon.Realtime.Player player, string key, object value)
    {
        Hashtable temp = player.CustomProperties;

        if (temp[key] == null)
        {
            temp.Add(key, value);
        }

        else
        {
            temp[key] = value;
        }

        player.SetCustomProperties(temp);
    }

    public static GameObject FindObject(GameObject parrent ,string name)
    {
        GameObject foundObject = null;
        Transform[] array = parrent.GetComponentsInChildren<Transform>(true);

        foreach (Transform transform in array)
        {
            if (transform.name == name)
            {
                foundObject = transform.gameObject;
                break; // 찾았으면 루프를 종료.
            }
        }

        if(foundObject == null)
        {
            Debug.LogError("해당 이름의 게임 오브젝트를 찾지 못했습니다 : " + name);
        }

        return foundObject;
    }

    public static List<GameObject> FindObjects(GameObject parrent, string name)
    {
        List<GameObject> foundObject = new List<GameObject>();
        Transform[] array = parrent.GetComponentsInChildren<Transform>(true);

        foreach (Transform transform in array)
        {
            if (transform.name == name)
            {
                foundObject.Add(transform.gameObject);
            }
        }

        if (foundObject == null)
        {
            Debug.LogError("해당 이름의 게임 오브젝트를 찾지 못했습니다 : " + name);
        }

        return foundObject;
    }

    public static GameObject FindObjectWithViewID(int viewID)
    {
        PhotonView photonView = PhotonView.Find(viewID);

        if (photonView == null)
        {
            Debug.LogError("해당 " + viewID + "로 게임 오브젝트를 찾을 수 없습니다.");
            return null;
        }

        else
        {
            return photonView.gameObject;
        }
    }

    public void GiveDataToUI()
    {
        if (inGameUI == null) return;

        inGameUI.globalTimerUI = globalTimer;
        inGameUI.roundEndTimerUI = roundEndTimer;
        inGameUI.roundEndTimeUI = roundEndTime;
        inGameUI.occupyingAUI.rate = occupyingA.rate;
        inGameUI.occupyingBUI.rate = occupyingB.rate;
        inGameUI.occupyingTeamUI.name = occupyingTeam.name;
        inGameUI.occupyingTeamUI.rate = occupyingTeam.rate;
    }

    //private void OnApplicationFocus(bool focus)
    //{
    //    if (!focus)
    //    {
    //        if(FirebaseLoginManager.Instance == null)
    //        {
    //            FirebaseLoginManager temp = FirebaseLoginManager.Instance;
    //        }

    //        FirebaseLoginManager.Instance.SignOut();

    //    }

    //}

    //private void OnApplicationQuit()
    //{
    //    // 로그 아웃

    //    if (FirebaseLoginManager.Instance == null)
    //    {
    //        FirebaseLoginManager temp = FirebaseLoginManager.Instance;
    //    }

    //    FirebaseLoginManager.Instance.SignOut();
    //}

    ////인게임 프레임 확인
    //void OnGUI()
    //{
    //    GUI.Label(new Rect(10, 10, 100, 20), "FPS: " + (1.0f / Time.smoothDeltaTime));
    //}

    //인게임 프레임 확인

    //long temp = 0;
    //void OnGUI()
    //{
    //    GUI.Label(new Rect(10, 10, 100, 20), "Traffic : " + (PhotonNetwork.NetworkingClient.LoadBalancingPeer.BytesOut));
    //}
}
