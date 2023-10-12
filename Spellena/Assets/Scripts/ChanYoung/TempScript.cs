using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TempScript : MonoBehaviour
{
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

    List<Player> playersA = new List<Player>();
    List<Player> playersB = new List<Player>();

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

    private void Awake()
    {
        gameState = GameState.MatchStart;
    }

    private void Start()
    {
        //테스트용 리스트
        for (int i = 0; i < maxPlayers / 2; i++)
        {
            playersA.Add(new Player());
        }

        for (int i = 0; i < maxPlayers / 2; i++)
        {
            playersA.Add(new Player());
        }
    }

    private void Update()
    {
        if (gameState == GameState.MatchStart)
        {
            gameState = GameState.CharacterSelect;
        }
        else if (gameState == GameState.CharacterSelect)
        {
            globalTimer += Time.deltaTime;
            if (globalTimer >= characterSelectTime)
            {
                globalTimer = 0;
                gameState = GameState.Ready;
            }
        }
        else if (gameState == GameState.Ready)
        {
            globalTimer += Time.deltaTime;
            if (globalTimer >= readyTime)
            {
                gameState = GameState.Round;
                ResetRound();
            }
        }
        else if (gameState == GameState.Round)
        {
            //지역이 점령되어있으면 점령한 팀의 점령비율이 높아진다.
            if (currentOccupationTeam == teamA)
                occupyingA.rate += Time.deltaTime * occupyingRate;//약 1.8초당 1씩 오름
            else if (currentOccupationTeam == teamB)
                occupyingB.rate += Time.deltaTime * occupyingRate;

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
        for (int i = 0; i < playersA.Count; i++)
        {
            if (playersA[i].IsOccupying == true)
            {
                teamAOccupying++;
            }

        }

        for (int i = 0; i < playersB.Count; i++)
        {
            if (playersB[i].IsOccupying == true)
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
                    occupyingTeam.rate = 0f;
            }
        }

    }
    void ChangeOccupyingRate(int num, string name) //점령 게이지 변화
    {
        if (occupyingTeam.name == name)
        {
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
            occupyingTeam.name = name;
            occupyingTeam.rate += occupyingGaugeRate * Time.deltaTime * num;
        }
        else
        {
            occupyingTeam.rate -= occupyingGaugeRate * Time.deltaTime * num;
            if (occupyingTeam.rate < 0)
            {
                occupyingTeam.name = name;
                occupyingTeam.rate = -occupyingTeam.rate;
            }
        }
    }



    //이 이하는 테스트용 일시적 정보이다.

    float characterSelectTime = 5f; //나중에 데이터 매니저에서 값을 가져오도록한다.
    float occupyingGaugeRate = 14.3f;
    float occupyingReturnTime = 3f;
    float occupyingRate = 0.556f;
    float occupyingComplete = 100f;

    float readyTime = 5f;
    float roundEndTime = 3f;
    string teamA = "A";
    string teamB = "B";

    int maxPlayers = 10;

    struct OccupyingTeam
    {
        public string name;
        public float rate;
    }

    struct Occupation
    {
        public float rate;
    }

    struct Player
    {
        public string name;
        public bool IsOccupying;
    }

}
