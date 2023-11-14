using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using Player;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameCenterTest : MonoBehaviourPunCallbacks, IPunObservable
{
    GameObject globalUIObj;
    PhotonView globalUIView;
    GlobalUI globalUI;

    GameObject playerSpawnPoints;

    // 플레이어 소환 좌표
    Transform[] playerSpawnA;
    Transform[] playerSpawnB;

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

    // 일시적인 죽은 플레이어
    string tempVictim = "";

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

    int masterActorNum;
    public List<Photon.Realtime.Player> playersA = new List<Photon.Realtime.Player>(); // Red
    public List<Photon.Realtime.Player> playersB = new List<Photon.Realtime.Player>(); // Blue 

    // 이 이하는 테스트용 일시적 정보이다.
    // Scriptable Object로 데이터 전달

    // 맵, 캐릭터 로딩 타임
    float loadingTime = 1f;
    // 캐릭터 선택 타임
    float characterSelectTime = 1f;
    // 대기실 준비 시간
    float readyTime = 1f;
    // 플레이어 리스폰 타임
    float playerRespawnTime = 6;
    // 거점 전환 원 먹는 비율
    float occupyingGaugeRate = 300f;
    // 거점 전환하는 시간
    float occupyingReturnTime = 3f;
    // 거점 % 먹는 비율
    float occupyingRate = 2f;
    // 추가시간이 발생하는 기준 게이지
    float occupyingComplete = 99f;
    //추가 시간
    float roundEndTime = 5f;
    // 라운드 결과 확인 시간
    float roundEndResultTime = 6f;

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

        int tempNum = 3;
        if (PhotonNetwork.CurrentRoom.PlayerCount >= tempNum)
        {
            globalTimer = loadingTime;

            SetPlayerDatas();

            globalUIObj = PhotonNetwork.Instantiate("TaeWoo/Prefabs/UI/GlobalUI", Vector3.zero, Quaternion.identity);
            globalUIView = globalUIObj.GetComponent<PhotonView>();
            globalUI = globalUIObj.GetComponent<GlobalUI>();

            playerSpawnPoints = PhotonNetwork.Instantiate("TaeWoo/Prefabs/PlayerSpawnPoints", Vector3.zero, Quaternion.identity);
            MakeSpawnPoint();

            gameState = GameState.MatchStart;
        }

    }

    void SetPlayerDatas()
    {
        masterActorNum = PhotonNetwork.CurrentRoom.MasterClientId;

        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            // 플레이어 이름, 캐릭터의 게임 오브젝트, 팀, 총 데미지 수, 킬 수, 죽은 수
            Hashtable playerData = new Hashtable();

            // 보여주는 데이터
            playerData.Add("Name", player.ActorNumber.ToString());
            playerData.Add("Team", "none");
            playerData.Add("TotalDamage", 0);
            playerData.Add("KillCount", 0);
            playerData.Add("DeadCount", 0);
            playerData.Add("IsAlive", true);

            // 보여주지 않는 데이터
            playerData.Add("CharacterViewID", 0);
            playerData.Add("ReSpawnTime", -1.0f);
            playerData.Add("SpawnPoint", new Vector3(0, 0, 0));
            playerData.Add("Parameter", null);

            player.SetCustomProperties(playerData);
        }

    }

    void MakeSpawnPoint()
    {
        playerSpawnA = FindObject(playerSpawnPoints, "TeamA").GetComponentsInChildren<Transform>(true);
        playerSpawnB = FindObject(playerSpawnPoints, "TeamB").GetComponentsInChildren<Transform>(true);
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

            globalTimer = readyTime;
            gameState = GameState.Ready;
            globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "inGameUI", true);

            MakeCharacter();
            MakeTeamStateUI();
        }
    }

    void Ready()
    {
        gameStateString = "Ready";

        globalTimer -= Time.deltaTime;
        if (globalTimer <= 0.0f)
        {
            //적 쉐이더 적용
            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                PhotonView view = PhotonView.Find((int)player.CustomProperties["CharacterViewID"]);
                view.RPC("SetEnemyLayer", player);
            }

            gameState = GameState.Round;
            ResetRound();
            globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "etcUI", false);
        }

    }

    void Round()
    {
        globalTimer += Time.deltaTime;

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
        CheckPlayerReSpawn();
        CheckRoundEnd();
    }

    void CheckPlayerReSpawn()
    {
        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if ((bool)player.CustomProperties["IsAlive"] && (bool)player.CustomProperties["IsAlive"] == true) continue;
            if ((float)player.CustomProperties["ReSpawnTime"] <= globalTimer)
            {
                PhotonView view = PhotonView.Find((int)player.CustomProperties["CharacterViewID"]);
                //player.CustomProperties["IsAlive"] = true;
                ChangePlayerCustomProperties(player, "IsAlive", true);

                if ((string)player.CustomProperties["Team"] == "A")
                {
                    view.RPC("PlayerReBornForAll", RpcTarget.AllBufferedViaServer, (Vector3)player.CustomProperties["SpawnPoint"]);                   
                }

                else if ((string)player.CustomProperties["Team"] == "B")
                {
                    view.RPC("PlayerReBornForAll", RpcTarget.AllBufferedViaServer, (Vector3)player.CustomProperties["SpawnPoint"]);
                }

                view.RPC("PlayerReBornPersonal", player);

                // 팀원 부활 알리기

                if((string)player.CustomProperties["Team"]=="A")
                {
                    foreach(var playerA in playersA)
                    {
                        globalUIView.RPC("ShowTeamLifeDead", playerA, (string)player.CustomProperties["Name"], false);
                    }
                }

                else if ((string)player.CustomProperties["Team"] == "B")
                {
                    foreach (var playerB in playersB)
                    {
                        globalUIView.RPC("ShowTeamLifeDead", playerB, (string)player.CustomProperties["Name"], false);
                    }
                }
            }
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

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
    {
        if (gameState == GameState.Round)
        {
            if (targetPlayer != null && changedProps != null)
            {
                //pararmeter로 변경된 key값을 찾는다.

                string param = (string)targetPlayer.CustomProperties["Parameter"];

                switch (param)
                {
                    case "TotalDamage":
                        if (globalUIView == null) break;
                        globalUIView.RPC("ShowDamageUI", targetPlayer);
                        break;
                    case "KillCount":
                        if (globalUIView == null) break;
                        globalUIView.RPC("ShowKillUI", targetPlayer, tempVictim);
                        globalUIView.RPC("ShowKillLog", RpcTarget.AllBufferedViaServer, targetPlayer.CustomProperties["Name"],
                            tempVictim, ((string)targetPlayer.CustomProperties["Team"] == "A"), targetPlayer.ActorNumber);
                        break;
                    case "DeadCount":
                        targetPlayer.CustomProperties["IsAlive"] = false;
                        targetPlayer.CustomProperties["DeadTime"] = globalTimer;
                        targetPlayer.CustomProperties["ReSpawnTime"] = globalTimer + playerRespawnTime;

                        tempVictim = (string)targetPlayer.CustomProperties["Name"];
                        ShowTeamMateDead((string)targetPlayer.CustomProperties["Team"],(string)targetPlayer.CustomProperties["Name"]);

                        PhotonView view = PhotonView.Find((int)targetPlayer.CustomProperties["CharacterViewID"]);
                        if (view == null) break;
                        view.RPC("PlayerDeadForAll", RpcTarget.AllBufferedViaServer);
                        view.RPC("PlayerDeadPersonal", targetPlayer);
                        break;
                    default:
                        break;
                }

                targetPlayer.CustomProperties["Parameter"] = "none";

            }
        }
    }

    void ShowTeamMateDead(string team, string deadName)
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

    void RoundEnd()
    {
        globalTimer -= Time.deltaTime;


        photonView.RPC("TimeScaling", RpcTarget.AllBuffered, 0.3f);

        if (globalTimer <= 0.0f)
        {
            globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "roundWin", false);
            globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "roundLoose", false);

            photonView.RPC("TimeScaling", RpcTarget.AllBuffered, 1.0f);

            if (roundA >= 2 || roundB >= 2)
            {
                gameState = GameState.MatchEnd;
                Debug.Log("Game End");
            }

            else
            {
                gameState = GameState.Ready;
                ResetRound();
            }
        }
    }

    [PunRPC]
    public void TimeScaling(float ratio)
    {
        Time.timeScale = ratio;
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
        int aTeamIndex = 1;
        int bTeamIndex = 1;

        foreach(var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            //현재 캐릭터는 에테르나 고정, 접속 순서에 따라 A,B 팀 나뉨
            //if((string)player.CustomProperties["Team"]=="A")
            string choseCharacter = "Aeterna";

            if (player.ActorNumber % 2 == 0)     // A 팀 (Red)
            {
                GameObject playerCharacter 
                    = PhotonNetwork.Instantiate("TaeWoo/Prefabs/" + choseCharacter, playerSpawnA[aTeamIndex].position, Quaternion.identity);

                playerCharacter.GetComponent<PhotonView>().TransferOwnership(player.ActorNumber);
                playerCharacter.GetComponent<PhotonView>().RPC("IsLocalPlayer", player);
                playerCharacter.GetComponent<Character>().SetTagServer("TeamA");

                ChangePlayerCustomProperties(player, "CharacterViewID", playerCharacter.GetComponent<PhotonView>().ViewID);
                ChangePlayerCustomProperties(player, "Team", "A");
                ChangePlayerCustomProperties(player, "SpawnPoint", playerSpawnA[aTeamIndex].position);
                aTeamIndex++;
                playersA.Add(player);
            }

            else                // B 팀 (Blue)
            {
                GameObject playerCharacter 
                    = PhotonNetwork.Instantiate("TaeWoo/Prefabs/" + choseCharacter, playerSpawnB[bTeamIndex].position, Quaternion.identity);

                playerCharacter.GetComponent<PhotonView>().TransferOwnership(player.ActorNumber);
                playerCharacter.GetComponent<PhotonView>().RPC("IsLocalPlayer", player);
                playerCharacter.GetComponent<Character>().SetTagServer("TeamB");

                ChangePlayerCustomProperties(player, "CharacterViewID", playerCharacter.GetComponent<PhotonView>().ViewID);
                ChangePlayerCustomProperties(player, "Team", "B");
                ChangePlayerCustomProperties(player, "SpawnPoint", playerSpawnB[bTeamIndex].position);
                bTeamIndex++;
                playersB.Add(player);
            }
        } 
    }

    void MakeTeamStateUI()
    {
        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if((string)player.CustomProperties["Team"] == "A")
            {
                foreach(var playerA in playersA)
                {
                    globalUIView.RPC("ShowTeamState", player, playerA.CustomProperties["Name"], "Aeterna");
                }
            }

            else if((string)player.CustomProperties["Team"] == "B")
            {
                foreach (var playerB in playersB)
                {
                    globalUIView.RPC("ShowTeamState", player, playerB.CustomProperties["Name"], "Aeterna");
                }
            }
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
                occupyingA.rate = 100;
                roundA++;

                if (roundA == 1)
                {
                    globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "redFirstPoint", true);

                    foreach (var player in playersA)
                    {
                        globalUIView.RPC("ShowRoundWin", player, roundA + roundB);
                    }

                    foreach (var player in playersB)
                    {
                        globalUIView.RPC("ShowRoundLoose", player, roundA + roundB);
                    }
                }

                else if (roundA == 2)
                {
                    globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "redSecondPoint", true);

                    foreach (var player in playersA)
                    {
                        globalUIView.RPC("ActiveUI", player, "victory", true);
                    }

                    foreach (var player in playersB)
                    {
                        globalUIView.RPC("ActiveUI", player, "defeat",true);
                    }
                }

            }

            else if (currentOccupationTeam == teamB)
            {
                occupyingB.rate = 100;
                roundB++;

                if (roundB == 1)
                {
                    globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "blueFirstPoint", true);

                    foreach (var player in playersB)
                    {
                        globalUIView.RPC("ShowRoundWin", player, roundA + roundB);
                    }

                    foreach (var player in playersA)
                    {
                        globalUIView.RPC("ShowRoundLoose", player, roundA + roundB);
                    }
                }

                else if (roundB == 2)
                {
                    globalUIView.RPC("ActiveUI", RpcTarget.AllBufferedViaServer, "blueSecondPoint", true);

                    foreach (var player in playersB)
                    {
                        globalUIView.RPC("ActiveUI", player, "victory", true);
                    }

                    foreach (var player in playersA)
                    {
                        globalUIView.RPC("ActiveUI", player, "defeat", true);
                    }
                }    
            }

            //라운드 종료
            gameState = GameState.RoundEnd;
            globalTimer = roundEndResultTime;

        }
    }

    GameObject FindObjectWithViewID(int viewID)
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

    void OccupyAreaCounts()//점령 지역에 플레이어가 몇 명 점령하고 있는지 확인
    {
        teamAOccupying = 0;
        teamBOccupying = 0;

        GameObject temp;

        for (int i = 0; i < playersA.Count; i++)
        {
            temp = FindObjectWithViewID((int)playersA[i].CustomProperties["CharacterViewID"]);

            if (temp.GetComponent<Character>().isOccupying == true)
            {
                teamAOccupying++;
            }
        }

        for (int i = 0; i < playersB.Count; i++)
        {
            temp = FindObjectWithViewID((int)playersB[i].CustomProperties["CharacterViewID"]);

            if (temp.GetComponent<Character>().isOccupying == true)
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
        globalTimer = readyTime;
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

        foreach(Photon.Realtime.Player player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if(player.CustomProperties["SpawnPoint"] !=null)
            {
                PhotonView view = PhotonView.Find((int)player.CustomProperties["CharacterViewID"]);
                if (view == null) continue;
                view.RPC("PlayerTeleport", RpcTarget.AllBuffered, (Vector3)player.CustomProperties["SpawnPoint"]);
            }
        }

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

    //인게임 프레임 확인
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 100, 20), "FPS: " + (1.0f / Time.smoothDeltaTime));
    }
}
