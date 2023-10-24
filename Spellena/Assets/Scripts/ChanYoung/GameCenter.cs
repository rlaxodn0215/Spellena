using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.Demo.PunBasics;
using System.Collections.Generic;
using Player;
using UnityEngine.UI;
using System;

public class GameCenter : MonoBehaviourPunCallbacks, IPunObservable
{
    GameObject gameStateText;
    Text gameStateTextUI;
    GameObject timerText;
    Text timerTextUI;
    GameObject teamAOccupyingText;
    GameObject teamBOccupyingText;
    Text teamAOccupyingTextUI;
    Text teamBOccupyingTextUI;
    GameObject occupyingGaugeText;
    Text occupyingGaugeTextUI;
    GameObject roundAText;
    GameObject roundBText;
    Text roundATextUI;
    Text roundBTextUI;

    enum GameState
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

    GameState gameState;


    public List<GameObject> playersA = new List<GameObject>();
    public List<GameObject> playersB = new List<GameObject>();

    struct OccupyingTeam
    {
        public string name;
        public float rate;
    }

    struct Occupation
    {
        public float rate;
    }


    int roundA = 0;
    int roundB = 0;

    int teamAOccupying = 0;
    int teamBOccupying = 0;

    float globalTimer;
    float occupyingReturnTimer;
    float roundEndTimer;

    string currentOccupationTeam = "";//현재 점령중인 팀

    Occupation occupyingA;//A팀의 점령도
    Occupation occupyingB;//B팀의 점령도
    OccupyingTeam occupyingTeam;//점령 게이지 바

    public int maxPlayers = 2;// 대 플레이어 수
    Character[] players;
    void Awake()
    {
        gameState = GameState.WaitingAllPlayer;
        gameStateText = GameObject.Find("GameState");
        gameStateTextUI = gameStateText.GetComponent<Text>();
        timerText = GameObject.Find("Timer");
        timerTextUI = timerText.GetComponent<Text>();
        teamAOccupyingText = GameObject.Find("TeamAOccupying");
        teamBOccupyingText = GameObject.Find("TeamBOccupying");
        teamAOccupyingTextUI = teamAOccupyingText.GetComponent<Text>();
        teamBOccupyingTextUI = teamBOccupyingText.GetComponent<Text>();
        occupyingGaugeText = GameObject.Find("OccupyingGauge");
        occupyingGaugeTextUI = occupyingGaugeText.GetComponent<Text>();
        roundAText = GameObject.Find("RoundA");
        roundBText = GameObject.Find("RoundB");
        roundATextUI = roundAText.GetComponent<Text>();
        roundBTextUI = roundBText.GetComponent<Text>();
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            timerTextUI.text = ((int)globalTimer).ToString();
            roundATextUI.text = roundA.ToString();
            roundBTextUI.text = roundB.ToString();


            if (gameState == GameState.WaitingAllPlayer)
            {
                gameStateTextUI.text = "Waiting Player";
                players = FindObjectsOfType<Character>();
                if(players.Length >= maxPlayers)
                    gameState = GameState.MatchStart;
            }
            else if (gameState == GameState.MatchStart)
            {
                gameStateTextUI.text = "Match Start";

                for(int i = 0; i < maxPlayers / 2; i++)
                {
                    playersA.Add(players[i].gameObject);
                    players[i].gameObject.GetComponent<Character>().SetTagServer("TeamA");
                }

                for(int i = maxPlayers / 2; i < maxPlayers; i++)
                {
                    playersB.Add(players[i].gameObject);
                    players[i].gameObject.GetComponent<Character>().SetTagServer("TeamB");
                }
                gameState = GameState.CharacterSelect;
            }
            else if (gameState == GameState.CharacterSelect)
            {
                gameStateTextUI.text = "Character Select";
                globalTimer += Time.deltaTime;
                if (globalTimer >= characterSelectTime)
                {
                    globalTimer = 0;
                    gameState = GameState.Ready;
                }
            }
            else if (gameState == GameState.Ready)
            {
                gameStateTextUI.text = "Ready";
                globalTimer += Time.deltaTime;
                if (globalTimer >= readyTime)
                {
                    gameState = GameState.Round;
                    ResetRound();
                }
            }
            else if (gameState == GameState.Round)
            {
                teamAOccupyingTextUI.text = ((int)occupyingA.rate).ToString();
                teamBOccupyingTextUI.text = ((int)occupyingB.rate).ToString();
                occupyingGaugeTextUI.text = occupyingTeam.name + " : " + ((int)occupyingTeam.rate).ToString();
                timerTextUI.text = ((int)roundEndTimer).ToString();

                gameStateTextUI.text = "Round";
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
                gameStateTextUI.text = "Round End";
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
                gameStateTextUI.text = "Match End";
                gameState = GameState.Result;
            }
            else if (gameState == GameState.Result)
            {
                gameStateTextUI.text = "Result";
                //종료
            }
        }

        for(int i = 0; i < playersA.Count; i++)
        {
            //Debug.Log(playersA[i].name);
        }

        for(int i = 0; i < playersB.Count; i++)
        {
            //Debug.Log(playersB[i].name);
        }
    }

    public void AddPlayer(GameObject player)
    {
        if (playersA.Count >= playersB.Count)
        {
            player.tag = "TeamB";
            playersB.Add(player);
        }
        else
        {
            player.tag = "TeamA";
            playersA.Add(player);
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
                roundA++;
            else if (currentOccupationTeam == teamB)
                roundB++;
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
        teamAOccupyingTextUI.text = "0";
        teamBOccupyingTextUI.text = "0";
    }
    //이 이하는 테스트용 일시적 정보이다.

    float characterSelectTime = 5f; //나중에 데이터 매니저에서 값을 가져오도록한다.
    float occupyingGaugeRate = 100f;
    float occupyingReturnTime = 3f;
    float occupyingRate = 5f;
    float occupyingComplete = 100f;

    float readyTime = 5f;
    float roundEndTime = 3f;
    string teamA = "A";
    string teamB = "B";


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(gameState);
            stream.SendNext(gameStateTextUI.text);
            stream.SendNext(timerTextUI.text);
            stream.SendNext(teamAOccupyingTextUI.text);
            stream.SendNext(teamBOccupyingTextUI.text);
            stream.SendNext(occupyingGaugeTextUI.text);
            stream.SendNext(roundATextUI.text);
            stream.SendNext(roundBTextUI.text);
        }
        else
        {
            gameState = (GameState)stream.ReceiveNext();
            gameStateTextUI.text = (string)stream.ReceiveNext();
            timerTextUI.text = (string)stream.ReceiveNext();
            teamAOccupyingTextUI.text = (string)stream.ReceiveNext();
            teamBOccupyingTextUI.text = (string)stream.ReceiveNext();
            occupyingGaugeTextUI.text =(string)stream.ReceiveNext();
            roundATextUI.text = (string)stream.ReceiveNext();
            roundBTextUI.text = (string)stream.ReceiveNext();
        }
    }
}
