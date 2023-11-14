using Player;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameCenterTest : MonoBehaviourPunCallbacks, IPunObservable
{
    [HideInInspector]
    public GameObject globalUIObj;
    [HideInInspector]
    public PhotonView globalUIView;
    [HideInInspector]
    public GlobalUI globalUI;

    [HideInInspector]
    public GameObject playerSpawnPoints;

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

    // 일시적인 게임 상태 string 테이터
    [HideInInspector]
    public string gameStateString;

    // 일시적인 죽은 플레이어
    [HideInInspector]
    public string tempVictim = "";

    // 라운드 점수
    [HideInInspector]
    public int roundA = 0;
    [HideInInspector]
    public int roundB = 0;

    // 점령 거점 차지하는 비율
    public int teamAOccupying = 0;
    public int teamBOccupying = 0;

    // 전체 타이머
    [HideInInspector]
    public float globalTimer;
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

    // 맵, 캐릭터 로딩 타임
    [HideInInspector]
    public float loadingTime = 1f;
    // 캐릭터 선택 타임
    [HideInInspector]
    public float characterSelectTime = 1f;
    // 대기실 준비 시간
    [HideInInspector]
    public float readyTime = 1f;
    // 플레이어 리스폰 타임
    [HideInInspector]
    public float playerRespawnTime = 6;
    // 거점 전환 원 먹는 비율
    [HideInInspector]
    public float occupyingGaugeRate = 300f;
    // 거점 전환하는 시간
    [HideInInspector]
    public float occupyingReturnTime = 3f;
    // 거점 % 먹는 비율
    [HideInInspector]
    public float occupyingRate = 2f;
    // 추가시간이 발생하는 기준 게이지
    [HideInInspector]
    public float occupyingComplete = 99f;
    //추가 시간
    [HideInInspector]
    public float roundEndTime = 5f;
    // 라운드 결과 확인 시간
    [HideInInspector]
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
            currentCenterState.StateExecution();
            currentCenterState = centerStates[currentGameState];

            if (globalUI != null)
                GiveDataToUI();
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

        if (temp[key] != null)
        {
            temp[key] = value;
            player.SetCustomProperties(temp);
        }

        else
        {
            Debug.LogError("해당 플레이어의 키 값을 찾을 수 없습니다.");
            return;
        }

    }

    public void ShowTeamMateDead(string team, string deadName)
    {
        if(team=="A")
        {
            foreach(var player in playersA)
            {
                globalUIView.RPC("ShowTeamLifeDead", player, deadName,true);
            }
        }

        else if(team=="B")
        {
            foreach (var player in playersB)
            {
                globalUIView.RPC("ShowTeamLifeDead", player, deadName,true);
            }
        }
    }

    public GameObject FindObject(GameObject parrent ,string name)
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
                    globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "captured_Red", true);
                    globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "captured_Blue", false);
                    globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "extraObj", false);
                    globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "blueExtraObj", false);
                    globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "blueExtraUI", true);
                }

                else if (currentOccupationTeam == "B")
                {
                    globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "captured_Red", false);
                    globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "captured_Blue", true);
                    globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "extraObj", false);
                    globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "redExtraObj", false);
                    globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "redExtraUI", true);
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

    [PunRPC]
    public void TimeScaling(float ratio)
    {
        Time.timeScale = ratio;
    }

    public void GiveDataToUI()
    {
        globalUI.gameStateString = gameStateString;
        globalUI.globalTimerUI = globalTimer;
        globalUI.roundEndTimerUI = roundEndTimer;
        globalUI.roundEndTimeUI = roundEndTime;
        globalUI.occupyingAUI.rate = occupyingA.rate;
        globalUI.occupyingBUI.rate = occupyingB.rate;
        globalUI.occupyingTeamUI.name = occupyingTeam.name;
        globalUI.occupyingTeamUI.rate = occupyingTeam.rate;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(currentGameState);
            stream.SendNext(gameStateString);
            stream.SendNext(roundA);
            stream.SendNext(roundB);
            stream.SendNext(teamAOccupying);
            stream.SendNext(teamBOccupying);
            stream.SendNext(globalTimer);
            stream.SendNext(occupyingReturnTimer);
            stream.SendNext(roundEndTimer);
            stream.SendNext(currentOccupationTeam);
            stream.SendNext(occupyingA.rate);
            stream.SendNext(occupyingB.rate);
            stream.SendNext(occupyingTeam.name);
            stream.SendNext(occupyingTeam.rate);
        }
        else
        {
            currentGameState = (GameState)stream.ReceiveNext();
            gameStateString = (string)stream.ReceiveNext();
            roundA = (int)stream.ReceiveNext();
            roundB = (int)stream.ReceiveNext();
            teamAOccupying = (int)stream.ReceiveNext();
            teamBOccupying = (int)stream.ReceiveNext();
            globalTimer = (float)stream.ReceiveNext();
            occupyingReturnTimer = (float)stream.ReceiveNext();
            roundEndTimer = (float)stream.ReceiveNext();
            currentOccupationTeam = (string)stream.ReceiveNext();
            occupyingA.rate = (float)stream.ReceiveNext();
            occupyingB.rate = (float)stream.ReceiveNext();
            occupyingTeam.name = (string)stream.ReceiveNext();
            occupyingTeam.rate = (float)stream.ReceiveNext();
        }
    }

    //인게임 프레임 확인
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 100, 20), "FPS: " + (1.0f / Time.smoothDeltaTime));
    }
}
