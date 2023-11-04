using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using Player;

public class GameCenterTest : MonoBehaviourPunCallbacks, IPunObservable
{
    GameObject globalUIObj;
    PhotonView globalUIView;
    GlobalUI globalUI;

    GameObject playerSpawnPoints;

    // 플레이어 소환 좌표
    Transform playerSpawnA;
    Transform playerSpawnB;

    public enum GameState
    {
        WaitingAllPlayer,
        MatchStart,
        CharacterSelect,
        Ready,
        Round,
        RoundEnd,
        MatchEnd,
        Result
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

    public GameState gameState = GameState.WaitingAllPlayer;

    // 일시적인 게임 상태 string 테이터
    string gameStateString;

    // 라운드 점수
    int roundA = 0;
    int roundB = 0;

    // 점령 거점 차지하는 비율
    int teamAOccupying = 0;
    int teamBOccupying = 0;

    // 전체 타이머
    float globalTimer;
    // 거점 전환 타이머
    float occupyingReturnTimer;
    // 추가시간 타이머
    float roundEndTimer;
    // 현재 점령중인 팀
    string currentOccupationTeam = "";
    // A팀의 점령도
    Occupation occupyingA;
    // B팀의 점령도
    Occupation occupyingB;
    // 점령 게이지 바
    OccupyingTeam occupyingTeam;

    public Photon.Realtime.Player[] allPlayers;
    public List<GameObject> playersA = new List<GameObject>(); // Red
    public List<GameObject> playersB = new List<GameObject>(); // Blue

    // 이 이하는 테스트용 일시적 정보이다.
    // Scriptable Object로 데이터 전달

    // 맵, 캐릭터 로딩 타임
    float loadingTime = 3f;
    // 캐릭터 선택 타임
    float characterSelectTime = 5f;
    // 대기실 준비 시간
    float readyTime = 5f;
    // 거점 전환 원 먹는 비율
    float occupyingGaugeRate = 250f;
    // 거점 전환하는 시간
    float occupyingReturnTime = 3f;
    // 거점 % 먹는 비율
    float occupyingRate = 2f;
    // 추가시간이 발생하는 기준 게이지
    float occupyingComplete = 99f;
    //추가 시간
    float roundEndTime = 5f;

    string teamA = "A";
    string teamB = "B";

    void Start()
    {
        
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            TodoMaster();

            if (globalUI != null)
                GiveDataToUI();
        }

    }

    void TodoMaster()
    {
        switch (gameState)
        {
            case GameState.WaitingAllPlayer:
                WaitingPlayers();
                break;
            case GameState.MatchStart:
                DataLoading();
                break;
            case GameState.CharacterSelect:
                CharacterSelect();
                break;
            case GameState.Ready:
                Ready();
                break;
            case GameState.Round:
                Round();
                break;
            case GameState.RoundEnd:
                RoundEnd();
                break;
            case GameState.MatchEnd:
                MatchEnd();
                break;
            case GameState.Result:
                Result();
                break;
            default:
                Debug.LogError("GameState Error");
                break;
        }
    }

    void WaitingPlayers()
    {
        gameStateString = "다른 플레이어 기다리는 중...";

        // 방장이 시작 버튼 누르면 시작
        // 플레이어와 팀 정보 저장
        // ActorNumber로 플레이어 식별
        // allPlayers = PhotonNetwork.PlayerList;
        // custom property로 값 저장 (ActorNumber, name, team)

        int tempNum = 2;
        if (PhotonNetwork.CurrentRoom.PlayerCount >= tempNum)
        {
            globalTimer = loadingTime;
            allPlayers = PhotonNetwork.PlayerList;

            globalUIObj = PhotonNetwork.Instantiate("TaeWoo/Prefabs/UI/GlobalUI", Vector3.zero, Quaternion.identity);
            globalUIView = globalUIObj.GetComponent<PhotonView>();
            globalUI = globalUIObj.GetComponent<GlobalUI>();

            playerSpawnPoints = PhotonNetwork.Instantiate("TaeWoo/Prefabs/PlayerSpawnPoints", Vector3.zero, Quaternion.identity);
            MakeSpawnPoint();
            gameState = GameState.MatchStart;
        }

    }
    void MakeSpawnPoint()
    {
        playerSpawnA = FindObject(playerSpawnPoints, "TeamA").transform;
        playerSpawnB = FindObject(playerSpawnPoints, "TeamB").transform;
    }

    void DataLoading()
    {
        gameStateString = "데이터 불러오는 중...";

        // 맵 및 캐릭터 데이터 로딩

        globalTimer -= Time.deltaTime;
        if (globalTimer <= 0.0f)
        {
            gameState = GameState.CharacterSelect;
            globalTimer = characterSelectTime;
        }
    }

    void CharacterSelect()
    {
        gameStateString = "캐릭터 선택";

        // 캐릭터 선택
        globalTimer -= Time.deltaTime;

        if (globalTimer <= 0.0f)
        {
            // 선택한 캐릭터로 소환 및 태그 설정
            // 적 플레이어는 빨강 쉐이더 적용
            MakeCharacter();
            globalTimer = readyTime;
            gameState = GameState.Ready;
            globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "inGameUI", true);
        }
    }

    void Ready()
    {
        gameStateString = "Ready";

        globalTimer -= Time.deltaTime;
        if (globalTimer <= 0.0f)
        {
            gameState = GameState.Round;
            ResetRound();
            globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "etcUI", false);
        }
    }

    void Round()
    {
        //지역이 점령되어있으면 점령한 팀의 점령비율이 높아진다.
        if (currentOccupationTeam == teamA)
        {
            occupyingA.rate += Time.deltaTime * occupyingRate;//약 1.8초당 1씩 오름
            if (occupyingA.rate >= occupyingComplete)
                occupyingA.rate = occupyingComplete;
        }
        else if (currentOccupationTeam == teamB)
        {
            occupyingB.rate += Time.deltaTime * occupyingRate;
            if (occupyingB.rate >= occupyingComplete)
                occupyingB.rate = occupyingComplete;
        }

        OccupyAreaCounts();
        CheckRoundEnd();
    }

    void RoundEnd()
    {
        if (roundA >= 2 || roundB >= 2)
        {
            gameState = GameState.MatchEnd;
        }
        else
        {
            globalTimer = readyTime;
            gameState = GameState.Ready;
            ResetRound();
        }
    }

    void MatchEnd()
    {
        gameState = GameState.Result;
    }

    void Result()
    {
        // 종료
    }

    void GiveDataToUI()
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

    GameObject FindObject(GameObject parrent ,string name)
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

    void MakeCharacter()
    {
        for(int i = 0; i < allPlayers.Length; i++)
        {
            //ActorNumber로 custom property 접근, 캐릭터 viewID 데이터 추가
            //현재 캐릭터는 에테르나 고정, 접속 순서에 따라 A,B 팀 나뉨

            if (i % 2 == 0)     // A 팀 (Red)
            {
                GameObject playerCharacter = PhotonNetwork.Instantiate("TaeWoo/Prefabs/Aeterna", playerSpawnA.position, Quaternion.identity);
                playerCharacter.GetComponent<PhotonView>().TransferOwnership(allPlayers[i].ActorNumber);
                playerCharacter.GetComponent<PhotonView>().RPC("IsLocalPlayer", allPlayers[i]);
                playerCharacter.GetComponent<Character>().SetTagServer("TeamA");
                playersA.Add(playerCharacter);
            }

            else                // B 팀 (Blue)
            {
                GameObject playerCharacter = PhotonNetwork.Instantiate("TaeWoo/Prefabs/Aeterna", playerSpawnB.position, Quaternion.identity);
                playerCharacter.GetComponent<PhotonView>().TransferOwnership(allPlayers[i].ActorNumber);
                playerCharacter.GetComponent<PhotonView>().RPC("IsLocalPlayer", allPlayers[i]);
                playerCharacter.GetComponent<Character>().SetTagServer("TeamB");
                playersB.Add(playerCharacter);
            }
        }

        for(int i = 0; i < playersA.Count;i++)
        {
            PhotonView photonView = PhotonView.Get(playersA[i]);
            photonView.gameObject.GetComponent<Character>().SetEnemyLayer();
        }

        for (int i = 0; i < playersB.Count; i++)
        {
            PhotonView photonView = PhotonView.Get(playersB[i]);
            photonView.gameObject.GetComponent<Character>().SetEnemyLayer();
        }


    }

    void CheckRoundEnd()
    {
        if (occupyingA.rate >= occupyingComplete && currentOccupationTeam == teamA && teamBOccupying <= 0)
        {
            globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "extraObj", true);
            globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "redExtraUI", false);
            globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "redExtraObj", true);
            roundEndTimer -= Time.deltaTime;

        }
        else if (occupyingB.rate >= occupyingComplete && currentOccupationTeam == teamB && teamAOccupying <= 0)
        {
            globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "extraObj", true);
            globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "blueExtraUI", false);
            globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "blueExtraObj", true);
            roundEndTimer -= Time.deltaTime;
        }
        else
        {
            roundEndTimer = roundEndTime;
        }

        if (roundEndTimer <= 0.0f)
        {
            //라운드 종료
            if (currentOccupationTeam == teamA)
            {
                if (roundA == 0)
                {
                    globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "redFirstPoint", true);
                }

                else if (roundA == 1)
                {
                    globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "redSecondPoint", true);
                }

                occupyingA.rate = 100;
                roundA++;
            }
            else if (currentOccupationTeam == teamB)
            {
                if (roundB == 0)
                {
                    globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "blueFirstPoint", true);
                }

                else if (roundB == 1)
                {
                    globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "blueSecondPoint", true);
                }

                occupyingB.rate = 100;
                roundB++;
            }

            gameState = GameState.RoundEnd;//라운드 종료
        }
    }
    void OccupyAreaCounts()//점령 지역에 플레이어가 몇 명 점령하고 있는지 확인
    {
        teamAOccupying = 0;
        teamBOccupying = 0;

        for (int i = 0; i < playersA.Count; i++)
        {
            if (playersA[i].GetComponent<Character>().isOccupying == true)
            {
                teamAOccupying++;
            }
        }

        for (int i = 0; i < playersB.Count; i++)
        {
            if (playersB[i].GetComponent<Character>().isOccupying == true)
            {
                teamBOccupying++;
            }
        }

        if (teamAOccupying > 0 && teamBOccupying > 0)
        {
            //서로 교전 중이라는 것을 알림
            occupyingReturnTimer = 0f;
            //globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "fighting", true);
        }
        else if (teamAOccupying > 0)//A팀 점령
        {
            ChangeOccupyingRate(teamAOccupying, teamA);
            occupyingReturnTimer = 0f;
            //globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "fighting", false);
        }
        else if (teamBOccupying > 0)//B팀 점령
        {
            ChangeOccupyingRate(teamBOccupying, teamB);
            occupyingReturnTimer = 0f;
            //globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "fighting", false);
        }
        else
        {
            occupyingReturnTimer += Time.deltaTime;
            //globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "fighting", false);
        }

        if (occupyingReturnTimer >= occupyingReturnTime)
        {
            if (occupyingTeam.rate > 0f)
            {
                occupyingTeam.rate -= Time.deltaTime;
                if (occupyingTeam.rate < 0f)
                {
                    occupyingTeam.rate = 0f;
                    occupyingTeam.name = "";
                }
            }
        }

    }
    void ChangeOccupyingRate(int num, string name) //점령 게이지 변화
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

    void ResetRound()
    {
        currentOccupationTeam = "";
        occupyingA = new Occupation();
        occupyingB = new Occupation();
        occupyingTeam = new OccupyingTeam();
        occupyingReturnTimer = 0f;
        roundEndTimer = 0;
        globalTimer = 0f;
        teamAOccupying = 0;
        teamBOccupying = 0;

        globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "captured_Red", false);
        globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "captured_Blue", false);
        globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "extraObj", false);
        globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "redExtraUI", true);
        globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "redExtraObj", false);
        globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "blueExtraUI", true);
        globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "blueExtraObj", false);
        globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "etcUI", true);

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(gameState);
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
            gameState = (GameState)stream.ReceiveNext();
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
}
