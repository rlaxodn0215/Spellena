using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.Demo.PunBasics;
using System.Collections.Generic;
using Player;
using UnityEngine.UI;
using System;

public class GameCenterTest : MonoBehaviourPunCallbacks, IPunObservable
{
    public GameObject playerSpawnPoints;
    public GameObject inGameUI;
    public GameObject etcUI;

    // 플레이어 소환 좌표
    Transform playerSpawnA;
    Transform playerSpawnB;

    // inGameUI 요소
    GameObject unContested;
    GameObject captured_Red;
    GameObject captured_Blue;
    Image redPayload;
    Image bluePayload;
    Text redPercentage;
    Text bluePercentage;
    GameObject extraTimeRed;
    GameObject extraTimeBlue;
    Text extraTimer;
    Image redCTF;
    Image blueCTF;
    GameObject redFirstPoint;
    GameObject redSecondPoint;
    GameObject blueFirstPoint;
    GameObject blueSecondPoint;

    //EtcUI
    Text gameStateUI;
    Text timer;

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

    public GameState gameState;

    struct OccupyingTeam
    {
        public string name;
        public float rate;
    }

    struct Occupation
    {
        public float rate;
    }

    // 라운드 점수
    int roundA = 0;
    int roundB = 0;

    //
    int teamAOccupying = 0;
    int teamBOccupying = 0;

    float globalTimer;
    float occupyingReturnTimer;
    float roundEndTimer;

    string currentOccupationTeam = "";  //현재 점령중인 팀

    Occupation occupyingA;              //A팀의 점령도
    Occupation occupyingB;              //B팀의 점령도
    OccupyingTeam occupyingTeam;        //점령 게이지 바

    Character[] players;

    // 이 이하는 테스트용 일시적 정보이다.

    float loadingTime = 3f;
    float characterSelectTime = 5f; //나중에 데이터 매니저에서 값을 가져오도록한다.
    float readyTime = 5f;

    //추가 시간
    float roundEndTime = 5f;

    float occupyingGaugeRate = 100f;
    float occupyingReturnTime = 3f;
    float occupyingRate = 5f;
    float occupyingComplete = 100f;

    string teamA = "A";
    string teamB = "B";

    public Photon.Realtime.Player[] allPlayers;
    public List<GameObject> playersA = new List<GameObject>(); // Red
    public List<GameObject> playersB = new List<GameObject>(); // Blue

    void Start()
    {
        gameState = GameState.WaitingAllPlayer;
        Init();
        etcUI.SetActive(true);
    }

    void Init()
    {
        playerSpawnA = FindObject(playerSpawnPoints, "TeamA").transform;
        playerSpawnB = FindObject(playerSpawnPoints, "TeamB").transform;

        unContested = FindObject(inGameUI, "UnContested");
        captured_Red = FindObject(inGameUI, "Captured_Red");
        captured_Blue = FindObject(inGameUI, "Captured_Blue");
        redPayload = FindObject(inGameUI, "RedPayload_Filled").GetComponent<Image>();
        bluePayload = FindObject(inGameUI, "BluePayload_Filled").GetComponent<Image>();
        redPercentage = FindObject(inGameUI, "RedOccupyingPercent").GetComponent<Text>();
        bluePercentage = FindObject(inGameUI, "BlueOccupyingPercent").GetComponent<Text>();
        extraTimeRed = FindObject(inGameUI, "RedPoint");
        extraTimeBlue = FindObject(inGameUI, "BluePoint");
        extraTimer = FindObject(inGameUI, "ExtaTimer").GetComponent<Text>();
        redCTF = FindObject(inGameUI, "RedCTF_Filled").GetComponent<Image>();
        blueCTF = FindObject(inGameUI, "BlueCTF_Filled").GetComponent<Image>();
        redFirstPoint = FindObject(inGameUI, "RedFirstPoint");
        redSecondPoint = FindObject(inGameUI, "RedSecondPoint");
        blueFirstPoint = FindObject(inGameUI, "BlueFirstPoint");
        blueSecondPoint = FindObject(inGameUI, "BlueSecondPoint");

        gameStateUI = FindObject(etcUI, "GameState").GetComponent<Text>();
        timer = FindObject(etcUI, "Timer").GetComponent<Text>();
    }

    int tempNum = 1;

    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            timer.text = ((int)globalTimer + 1).ToString();

            if (gameState == GameState.WaitingAllPlayer)
            {
                gameStateUI.text = "다른 플레이어 기다리는 중...";

                // 방장이 시작 버튼 누르면 시작
                // 플레이어와 팀 정보 저장
                // ActorNumber로 플레이어 식별
                // allPlayers = PhotonNetwork.PlayerList;
                // custom property로 값 저장 (ActorNumber, name, team)



                if(PhotonNetwork.CurrentRoom.PlayerCount >= tempNum)
                {
                    globalTimer = loadingTime;
                    allPlayers = PhotonNetwork.PlayerList;
                    gameState = GameState.MatchStart;
                    Debug.Log("한 번만 실행...");
                }

            }

            else if (gameState == GameState.MatchStart)
            {
                gameStateUI.text = "데이터 불러오는 중...";

                // 맵 및 캐릭터 데이터 로딩
               
                globalTimer -= Time.deltaTime;
                if (globalTimer <= 0.0f)
                {
                    gameState = GameState.CharacterSelect;
                    globalTimer = characterSelectTime;
                    Debug.Log("Change CharacterSelect");
                }

            }
            else if (gameState == GameState.CharacterSelect)
            {
                gameStateUI.text = "Character Select";

                // 캐릭터 선택

                globalTimer -= Time.deltaTime;
                if (globalTimer <= 0.0f)
                {
                    // 선택한 캐릭터로 소환 및 태그 설정
                    // 적 플레이어는 빨강 쉐이더 적용
                    MakeCharacter();
                    globalTimer = readyTime;
                    gameState = GameState.Ready;
                    inGameUI.SetActive(true);
                }

            }
            else if (gameState == GameState.Ready)
            {
                gameStateUI.text = "Ready";

                globalTimer -= Time.deltaTime;
                if (globalTimer <= 0.0f)
                {
                    gameState = GameState.Round;
                    etcUI.SetActive(false);
                    ResetRound();
                }
            }
            else if (gameState == GameState.Round)
            {
                redPayload.fillAmount = occupyingA.rate;
                bluePayload.fillAmount = occupyingB.rate;
                extraTimer.text = string.Format("{0:F2}", roundEndTimer);

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
            else if (gameState == GameState.RoundEnd)
            {
                if (roundA >= 2 || roundB >= 2)
                {
                    gameState = GameState.MatchEnd;
                }
                else
                {
                    gameState = GameState.CharacterSelect;
                    ResetRound();
                }
            }
            else if (gameState == GameState.MatchEnd)
            {
                gameState = GameState.Result;
            }
            else if (gameState == GameState.Result)
            {
                //종료
            }
        }
    }

    void WaitingPlayers()
    {

    }

    void DataLoading()
    {

    }

    void CharacterSelect()
    {

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

            if (i % 2 == 1)     // A 팀 (Red)
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
            roundEndTimer += Time.deltaTime;
        }
        else if (occupyingB.rate >= occupyingComplete && currentOccupationTeam == teamB && teamAOccupying <= 0)
        {
            roundEndTimer += Time.deltaTime;
        }
        else
            roundEndTimer = 0f;

        if (roundEndTimer >= roundEndTime)
        {
            //라운드 종료
            if (currentOccupationTeam == teamA)
            {
                if (roundA == 0) redFirstPoint.SetActive(true);
                else if(roundA==1) redSecondPoint.SetActive(true);
                roundA++;
            }
            else if (currentOccupationTeam == teamB)
            {
                if (roundB == 0) blueFirstPoint.SetActive(true);
                else if (roundB == 1) blueSecondPoint.SetActive(true);
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
        }
        else if (teamAOccupying > 0)//A팀 점령
        {
            ChangeOccupyingRate(teamAOccupying, teamA);
            occupyingReturnTimer = 0f;
        }
        else if (teamBOccupying > 0)//B팀 점령
        {
            ChangeOccupyingRate(teamBOccupying, teamB);
            occupyingReturnTimer = 0f;
        }
        else
        {
            occupyingReturnTimer += Time.deltaTime;
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
            occupyingTeam.rate += occupyingGaugeRate * Time.deltaTime * num;
            if (occupyingTeam.rate >= 100)
            {
                currentOccupationTeam = name;
                occupyingTeam.name = "";
                occupyingTeam.rate = 0f;
            }
        }
        else if (occupyingTeam.name == "")
        {
            if (currentOccupationTeam == name)
                return;
            occupyingTeam.name = name;
            occupyingTeam.rate += occupyingGaugeRate * Time.deltaTime * num;
        }
        else
        {
            occupyingTeam.rate -= occupyingGaugeRate * Time.deltaTime * num;
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
        roundEndTimer = 0f;
        globalTimer = 0f;
        teamAOccupying = 0;
        teamBOccupying = 0;
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(gameState);

        }
        else
        {
            gameState = (GameState)stream.ReceiveNext();

        }
    }
}
