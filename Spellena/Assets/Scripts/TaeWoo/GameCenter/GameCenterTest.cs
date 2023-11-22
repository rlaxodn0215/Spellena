using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameCenterTest : MonoBehaviourPunCallbacks
{
    public GameObject angleStatue;
    public GameObject playerSpawnPoints;
    public GameObject characterSelect;
    public GameObject inGameUIObj;

    [HideInInspector]
    public PhotonView inGameUIView;
    [HideInInspector]
    public InGameUI inGameUI;

    // 플레이어 소환 좌표
    [HideInInspector]
    public Transform[] playerSpawnA;
    [HideInInspector]
    public Transform[] playerSpawnB;

    public enum GameState
    {
        WaitingAllPlayer,
        DataLoading,
        CharacterSelect,
        GameReady,
        DuringRound,
        RoundEnd,
        MatchEnd,
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

    //// 일시적인 게임 상태 string 테이터
    //[HideInInspector]
    //public string gameStateString;

    // 일시적인 죽은 플레이어
    [HideInInspector]
    public string tempVictim = "";

    // 라운드 점수
    [HideInInspector]
    public int roundA = 0;
    [HideInInspector]
    public int roundB = 0;

    // 점령 거점 차지하는 비율
    [HideInInspector]
    public int teamAOccupying = 0;
    [HideInInspector]
    public int teamBOccupying = 0;

    // 전체 타이머
    [HideInInspector]
    public float globalTimer;
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
    public List<Photon.Realtime.Player> playersA = new List<Photon.Realtime.Player>(); // Red
    [HideInInspector]
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

    private void Awake()
    {
        WaitingPlayers temp = gameObject.AddComponent<WaitingPlayers>();
        temp.ConnectCenter(this);
        centerStates.Add(GameState.WaitingAllPlayer, temp);

        DataLoading temp1 = gameObject.AddComponent<DataLoading>();
        temp1.ConnectCenter(this);
        centerStates.Add(GameState.DataLoading, temp1);

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

        MatchEnd temp6 = gameObject.AddComponent<MatchEnd>();
        temp6.ConnectCenter(this);
        centerStates.Add(GameState.MatchEnd, temp6);

        GameResult temp7 = gameObject.AddComponent<GameResult>();
        temp7.ConnectCenter(this);
        centerStates.Add(GameState.GameResult, temp7);

        currentGameState = GameState.WaitingAllPlayer;
        currentCenterState = centerStates[currentGameState];
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log(currentGameState);
            currentCenterState.StateExecution();
            currentCenterState = centerStates[currentGameState];

            photonView.RPC("SerializeGameCenterDatas", RpcTarget.AllBufferedViaServer, ToDoSerlize());

            if (inGameUI != null)
            {
                GiveDataToUI();
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

    public void ShowTeamMateDead(string team, string deadName)
    {
        if(team=="A")
        {
            foreach(var player in playersA)
            {
                inGameUIView.RPC("ShowTeamLifeDead", player, deadName,true);
            }
        }

        else if(team=="B")
        {
            foreach (var player in playersB)
            {
                inGameUIView.RPC("ShowTeamLifeDead", player, deadName,true);
            }
        }
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

    public GameObject FindObjectWithViewID(int viewID)
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

    
    public void ChangeOccupyingRate(int num, string name) //점령 게이지 변화
    {
        if (occupyingTeam.name == name)
        {
            if (currentOccupationTeam == name)
                return;
            occupyingTeam.rate += occupyingGaugeRate * Time.deltaTime;
            if (occupyingTeam.rate >= 100)
            {
                currentOccupationTeam = name;
                occupyingTeam.name = "";
                occupyingTeam.rate = 0f;

                if (currentOccupationTeam == "A")
                {
                    inGameUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "captured_Red", true);
                    inGameUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "captured_Blue", false);
                    inGameUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "extraObj", false);
                    inGameUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "blueExtraObj", false);
                    inGameUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "blueExtraUI", true);
                    angleStatue.GetComponent<PhotonView>().RPC("ChangeTeam", RpcTarget.AllBufferedViaServer, "A");
                }

                else if (currentOccupationTeam == "B")
                {
                    inGameUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "captured_Red", false);
                    inGameUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "captured_Blue", true);
                    inGameUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "extraObj", false);
                    inGameUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "redExtraObj", false);
                    inGameUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "redExtraUI", true);
                    angleStatue.GetComponent<PhotonView>().RPC("ChangeTeam", RpcTarget.AllBufferedViaServer, "B");
                }
            }
        }
        else if (occupyingTeam.name == "")
        {
            if (currentOccupationTeam == name)
                return;
            occupyingTeam.name = name;
            occupyingTeam.rate += occupyingGaugeRate * Time.deltaTime;
        }
        else
        {
            occupyingTeam.rate -= occupyingGaugeRate * Time.deltaTime;
            if (occupyingTeam.rate < 0)
            {
                occupyingTeam.name = "";
                occupyingTeam.rate = 0;
            }
        }
    }

    public void GiveDataToUI()
    {
        if (inGameUI == null) return;
        inGameUI.globalTimerUI = globalDesiredTimer - globalTimer;
        inGameUI.roundEndTimerUI = roundEndTimer;
        inGameUI.roundEndTimeUI = roundEndTime;
        inGameUI.occupyingAUI.rate = occupyingA.rate;
        inGameUI.occupyingBUI.rate = occupyingB.rate;
        inGameUI.occupyingTeamUI.name = occupyingTeam.name;
        inGameUI.occupyingTeamUI.rate = occupyingTeam.rate;
    }

    //인게임 프레임 확인
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 100, 20), "FPS: " + (1.0f / Time.smoothDeltaTime));
    }
}
