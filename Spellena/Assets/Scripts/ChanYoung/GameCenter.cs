using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.Demo.PunBasics;
using System.Collections.Generic;
using Player;
using static Photon.Pun.UtilityScripts.PunTeams;

public class GameCenter : MonoBehaviourPunCallbacks, IPunObservable
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

    List<GameObject> playersA = new List<GameObject>();
    List<GameObject> playersB = new List<GameObject>();

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

    string currentOccupationTeam = "";//���� �������� ��

    Occupation occupyingA;//A���� ���ɵ�
    Occupation occupyingB;//B���� ���ɵ�
    OccupyingTeam occupyingTeam;//���� ������ ��

    int maxPlayers = 10;//�ִ� �÷��̾� ��

    void Awake()
    {
        gameState = GameState.WaitingAllPlayer;
    }

    void Update()
    {
        if(gameState == GameState.WaitingAllPlayer)
        {
            if(PhotonNetwork.PlayerList.Length >= maxPlayers)
            {
                //¦���� A�� Ȧ���� B������ �����ȴ�.
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    GameObject _temp = PhotonNetwork.PlayerList[i].TagObject as GameObject;
                    if(i % 2 == 0)
                    {
                        playersA.Add(_temp);
                    }
                    else
                    {
                        playersB.Add(_temp);
                    }
                }
                gameState = GameState.MatchStart;
            }
        }
        else if (gameState == GameState.MatchStart)
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
            //������ ���ɵǾ������� ������ ���� ���ɺ����� ��������.
            if (currentOccupationTeam == teamA)
                occupyingA.rate += Time.deltaTime * occupyingRate;//�� 1.8�ʴ� 1�� ����
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
            //����
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
            //���� ����
            if (currentOccupationTeam == teamA)
                roundA++;
            else if (currentOccupationTeam == teamB)
                roundB++;
            gameState = GameState.RoundEnd;//���� ����
        }
    }
    void OccupyAreaCounts()//���� ������ �÷��̾ �� �� �����ϰ� �ִ��� Ȯ��
    {
        for (int i = 0; i < playersA.Count; i++)
        {
            if (playersA[i].GetComponent<Charactor>().isOccupying == true)
            {
                teamAOccupying++;
            }
        }

        for (int i = 0; i < playersB.Count; i++)
        {
            if (playersB[i].GetComponent<Charactor>().isOccupying == true)
            {
                teamBOccupying++;
            }
        }

        if (teamAOccupying > 0 && teamBOccupying > 0)
        {
            //���� ���� ���̶�� ���� �˸�
            occupyingReturnTimer = 0f;
        }
        else if (teamAOccupying > 0)//A�� ����
        {
            ChangeOccupyingRate(teamAOccupying, teamA);
            occupyingReturnTimer = 0f;
        }
        else if (teamBOccupying > 0)//B�� ����
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
    void ChangeOccupyingRate(int num, string name) //���� ������ ��ȭ
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
    //�� ���ϴ� �׽�Ʈ�� �Ͻ��� �����̴�.

    float characterSelectTime = 5f; //���߿� ������ �Ŵ������� ���� �����������Ѵ�.
    float occupyingGaugeRate = 14.3f;
    float occupyingReturnTime = 3f;
    float occupyingRate = 0.556f;
    float occupyingComplete = 100f;

    float readyTime = 5f;
    float roundEndTime = 3f;
    string teamA = "A";
    string teamB = "B";



    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
}